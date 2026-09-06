# The `.editorconfig` guide

`dotnet-fast` reads `.editorconfig`/`.globalconfig` the way `dotnet format` does — same chain, same
precedence, same parsing quirks — and recognises **180 keys** across core formatting, C# style, the
ported analyzer catalog and its own guardrails. Almost none of that surface is written down anywhere
else. This page is the whole picture: which files apply and in what order, how a line resolves to a
value, the vocabulary the severity keys accept, and the two places a "line budget" means something
different depending which command you ran.

The exhaustive, generated list of every key — value shape, default, one-line meaning — is
[editorconfig-keys.md](editorconfig-keys.md). This page is how to reason about the system; that one is
what to look up.

## The chain: which files apply, and in what order

Two file kinds, applied in a fixed order, nearest always winning:

1. **Every ancestor `.globalconfig`**, from the outermost directory down to the nearest. A
   `.globalconfig` is **section-less** — parsing stops at the first `[section]` header, so only the
   keys above it are read. Anything after the first section header is ignored, not misapplied.
2. **Every ancestor `.editorconfig`**, from the outermost down to the nearest, **truncated at the
   first one that sets `root = true`** — that file is included, nothing above it is read.

Within that combined chain, a **nearer file always overrides a farther one**, key by key — not
file by file. A `dotnet_diagnostic.CA1806.severity` set in a parent `.editorconfig` and left unset in
a child still applies to the child; only the keys the child actually mentions are overridden. And
within a single file, when more than one `[section]` matches the file, **the later matching section
wins**, top to bottom.

`.globalconfig` keys apply everywhere (no sections), and are layered in *before* any `.editorconfig`,
so a project-wide `.editorconfig` always has the last word over a `.globalconfig` default.

## Reading one line: keys, values, comments

- **Keys match exactly and case-sensitively.** `end_of_line` is not `End_Of_Line`. Every key this tool
  does not recognise — a typo, a key from a newer SDK, `dotnet_naming_rule.foo.severity` — is a
  **silent no-op**. Nothing warns, nothing errors; the line is read and discarded.
- **`#` or `;` opens a comment at the first occurrence in the value**, with no preceding whitespace
  required, and no key is exempt — including `file_header_template`, the one free-text value this
  tool honours. `dotnet_diagnostic.CA2200.severity = warning#glued` reads as `warning`;
  `indent_size = 2  # legacy` reads as `2`. A section header follows the same rule:
  `[*.md]  # docs only` scopes to `*.md`, it does not fail to parse.
- **The key itself is never comment-stripped.** A `#`/`;` cannot appear inside the key characters this
  tool matches, so `# key = value` keeps the marker glued to the key — which is what keeps a
  commented-out line commented out, rather than being read as a key literally named `# key`.

## Severity: the words `.editorconfig` accepts

Every `…severity = …` key — per-rule, per-category, bulk, or a code-style option's own severity
suffix — is parsed with the same vocabulary, case-insensitively:

| You write | Resolves to |
|---|---|
| `silent`, `hidden` | Hidden |
| `suggestion`, `info` | Info |
| `warning`, `warn` | Warning |
| `error` | Error |
| `none` | Suppressed — the rule is switched off |
| anything else | **The line is silently ignored** — no error, no fallback, the previous value stands |

That last row is the one worth remembering: `dotnet_diagnostic.CA1806.severity = warn ing` (a stray
space, a typo, a value from a tool that accepts more spellings than this one) does not error and does
not disable the rule — it does nothing, and whatever severity was already resolved for that rule
continues to apply.

## `unset`: what it actually resets

`key = unset` is the editorconfig convention for undefining a key, including a value inherited from a
higher `.editorconfig` in the chain. Four keys implement it fully — they revert all the way to the
tool's built-in default, as if the key had never appeared anywhere in the chain:

- `end_of_line`
- `insert_final_newline`
- `charset`
- `dotnet_sort_system_directives_first`

**Every other key's `unset` is a no-op that keeps whatever value the chain had already resolved.** The
one guarantee that holds for all 180 keys is that `unset` never garbage-parses into a real value —
early on, `end_of_line = unset` parsed to the LF default *with the "explicitly set" flag still true*,
which made the formatter strip every CR in a CRLF repository. That specific failure mode cannot
recur; a key without full `unset` support simply does not change.

## Precedence: which severity wins

For the ported analyzer catalog (the `IDE*`/`CA*`/`S*`/… rules that run without `--deep`) and for the
`DF900x` guardrails, up to four things can set a rule's severity. In order, nearer/more specific wins:

1. **Per-rule** — `dotnet_diagnostic.<ID>.severity`
2. **Per-category** — `dotnet_analyzer_diagnostic.category-<name>.severity`
3. **Bulk** — `dotnet_analyzer_diagnostic.severity`
4. **The rule's own default**

That order mirrors Roslyn's, with one precedent worth stating loudly: **a bulk or per-category key can
never *enable* two specific classes of rule** — only a per-rule `dotnet_diagnostic.<ID>.severity` can.

- **The six `DF900x` guardrails.** They are the catalog's only default-**off** family. A repository
  that already sets `dotnet_analyzer_diagnostic.severity = warning` does not silently acquire all six
  guardrails on upgrade — each has to be switched on by name. See [guardrails.md](guardrails.md).
- **Ported analyzers whose real, upstream analyzer is disabled by default** — its own
  `DiagnosticDescriptor.IsEnabledByDefault` is `false` under plain `dotnet build`. `CA1819`, `CA1002`
  and `CA2007` are three of roughly three dozen; a bulk/category key can *tune* the severity of one of
  these once it is on, but cannot turn it on in the first place — exactly as Roslyn treats a
  disabled-by-default analyzer. [ported-analyzers.md](ported-analyzers.md) has the full verified list
  and how each was sourced.

## `dotnet-fast`'s own keys: the six `dotnet_fast_*` thresholds

The [guardrails](guardrails.md) read six thresholds, namespaced `dotnet_fast_*` because they configure
this tool's own rules, not any Roslyn analyzer:

```ini
dotnet_fast_max_lines_per_function = 50
dotnet_fast_max_lines_per_file = 250
dotnet_fast_magic_number_allowed = 0,1,-1,2
dotnet_fast_max_cyclomatic_complexity = 22
dotnet_fast_max_cognitive_complexity = 22
dotnet_fast_max_halstead_difficulty = 80
```

**An unparseable threshold keeps the previous (or default) limit — it never disables the rule.**
`dotnet_fast_max_lines_per_file = fifty` leaves the limit at 250, not "no limit". The same holds for
`0`, a negative number and a non-finite float (`nan`, `inf`): all three are rejected and the prior
threshold stands. A typo that silently removed a limit would turn a green build into a false green,
which is the one failure mode a guardrail must never have.

## Two budget systems, two different numbers

`dotnet-fast` has **two independent places that call something a "line budget"**, and they do not
agree, do not read each other, and are not going to be unified:

| Knob | Lives in | Drives | Default |
|---|---|---|---|
| `dotnet_fast_max_lines_per_file` | `.editorconfig` | lint guardrail `DF9003` | **250** |
| `metrics.maxLinesPerFile` | `dotnet-fast.json` | the [`metrics`](metrics.md) command | **500** |
| `dotnet_fast_max_cyclomatic_complexity` | `.editorconfig` | `DF9005` | 22 |
| `metrics.maxCyclomatic` | `dotnet-fast.json` | `metrics` | 22 |
| `dotnet_fast_max_halstead_difficulty` | `.editorconfig` | `DF9006` | 80 |
| `metrics.maxHalsteadDifficulty` | `dotnet-fast.json` | `metrics` | 80 |

The cyclomatic and Halstead rows agree today — that is a coincidence of both having been picked from
the same "AI slop" writing, not a guarantee, and they may diverge in a future release. The lines-per-file
row already disagrees, by exactly 2×.

**Neither file format is read by the other side.** `lint` never opens the `metrics` section of
`dotnet-fast.json`; `metrics` never opens `.editorconfig`'s `dotnet_fast_*` keys. Setting one does
nothing to the other. This is why `dotnet-fast metrics` can call a 480-line file fine while
`dotnet-fast lint` fails it at 260 — the file is genuinely being measured against two different
numbers, not disagreeing about the same one.

**What *is* shared is the scorers.** The cyclomatic-complexity and Halstead-difficulty code behind
`DF9005`/`DF9006` is the exact code behind the matching `metrics` rows — not a second implementation
that could drift. A member's raw complexity number is identical wherever you read it; only the budget
each surface checks it against can differ.

## Recipes

### Scope a rule to part of the tree

Sections apply in file order, later-matching-wins, so a looser rule for tests reads naturally after
the main one:

```ini
[*.cs]
dotnet_diagnostic.DF9002.severity = warning
dotnet_fast_max_lines_per_function = 50

[**/*Tests/**.cs]
dotnet_fast_max_lines_per_function = 200
```

### A monorepo with `root = true`

Each package gets its own `.editorconfig` chain, truncated at its own `root = true` — a shared parent
`.editorconfig` above it never applies:

```ini
# packages/api/.editorconfig
root = true

[*.cs]
dotnet_diagnostic.CA1806.severity = warning
```

Anything set in a `.editorconfig` above `packages/api/` — even `indent_style` — stops applying the
moment `packages/api/.editorconfig` is read, because `root = true` ends the chain there.

### Blanket off, then curated on

Turn the whole opt-out ported catalog off, then re-enable specific rules by id — precedence (per-rule
beats bulk) does the rest:

```ini
[*.cs]
dotnet_analyzer_diagnostic.severity = none

dotnet_diagnostic.CA1806.severity = warning
dotnet_diagnostic.DF0016.severity = warning
```

### The `lint --baseline` on-ramp

Turning on a whole family of rules across an existing codebase produces a wall of findings. Recording
today's state and failing only on what's added afterward is usually the faster path to green:

```bash
dotnet-fast lint --baseline findings.json .
```

The same on-ramp works for the guardrails specifically — see
[guardrails.md](guardrails.md#adopting-them-without-a-wall-of-findings) for the worked example.

### Coming from `dotnet format`

An existing `.editorconfig` written for `dotnet format` carries over almost unchanged: the core keys,
the `csharp_*` layout and spacing keys, and the `dotnet_diagnostic.<ID>.severity`/
`dotnet_analyzer_diagnostic[.category-<c>].severity` precedence all parse and resolve the same way,
including the inline-comment handling (see the `## editorconfig` section of
[commands.md](commands.md#editorconfig) for exactly what changed there in v0.306.0). What is new: the
`dotnet_fast_*` guardrail keys are this tool's own and have no Roslyn equivalent, and — see below —
`dotnet_naming_*` has no effect here even though a real Roslyn-based pipeline honours it.

## Debugging what applied

Three commands answer "why is this setting doing what it's doing":

```bash
dotnet-fast editorconfig explain src/Program.cs       # the resolved chain + core settings, with provenance
dotnet-fast editorconfig explain src/Program.cs --json # same, machine-readable
dotnet-fast lint --explain DF9002                      # full description of one rule, incl. its threshold key
dotnet-fast editorconfig recommend                     # a curated profile for the ported analyzers
```

`explain` prints the exact `.editorconfig`/`.globalconfig` files it read for that file, in the
precedence order above, and for each core setting either `explicit` with the `file:line` that won, or
`default`. See [commands.md#editorconfig](commands.md#editorconfig) for the full reference.

## Not read

Four keys are recognised by name elsewhere in the .NET ecosystem but have **no handler at all** here —
setting them is a complete no-op, silently, like any other unrecognised key:

| Key | Why |
|---|---|
| `tab_width` | Deliberate divergence. With `indent_size = tab`, `dotnet format` widens tabs to an internal size; this tool emits one tab per level and honours the author's tab intent instead. |
| `max_line_length` | Not implemented. |
| `dotnet_naming_rule.*` / `dotnet_naming_symbols.*` / `dotnet_naming_style.*` | **Naming-convention rules are silently ignored.** If your `.editorconfig` configures naming conventions expecting a Roslyn-based tool to enforce them, nothing here does — no error, no partial support. |
| `dotnet_separated_import_directive_groups` | No handler — and `dotnet format` itself treats this as a no-op (probed against the SDK: `true`/`false` produce identical output), so the absence matches on purpose. |

The full key-by-key reference — including these four, and every one of the 180 that *is* read — is
[editorconfig-keys.md](editorconfig-keys.md).

## Related

- [editorconfig-keys.md](editorconfig-keys.md) — every recognised key: value shape, default, one line
  of meaning.
- [guardrails.md](guardrails.md) — the six `DF900x` rules the five `dotnet_fast_*` keys configure.
- [ported-analyzers.md](ported-analyzers.md) — the analyzers the severity precedence rules above apply
  to, and which are disabled by default upstream.
- [rules.md](rules.md) — the native `DFxxxx` catalog, and how to suppress a finding in source instead
  of `.editorconfig`.
- [commands.md#editorconfig](commands.md#editorconfig) — the full `editorconfig` subcommand reference.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

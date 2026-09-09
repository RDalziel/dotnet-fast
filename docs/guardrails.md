# Guardrails — lint rules for agent-written code

Eight optional rules, `DF9001`–`DF9008`, for repositories where an agent writes some of the code and a
human reviews it.

They exist because of one asymmetry: **an agent will happily ignore a style guide, but it cannot
ignore a lint error in CI.** Documentation is advice; a failing check is a constraint. So the rules
that actually shape what gets generated are the ones a linter enforces.

These are not ports of any Roslyn analyzer. They are opinions — deliberately so — and they are **off
by default**. Nothing changes in your repository until you ask for it.

## The rules

| Rule | Reports | Threshold |
|---|---|---|
| `DF9001` | A `//` or `/* … */` comment. XML documentation (`///`, `/** … */`) is exempt. | — |
| `DF9002` | A method, constructor, finalizer, operator, local function or accessor over the line budget. | `dotnet_fast_max_lines_per_function` (50) |
| `DF9003` | A file over the line budget, blank lines excluded. | `dotnet_fast_max_lines_per_file` (250) |
| `DF9004` | A numeric literal with no name. | `dotnet_fast_magic_number_allowed` (`0,1,-1,2`) |
| `DF9005` | A member with too many independent paths through it. | `dotnet_fast_max_cyclomatic_complexity` (22) |
| `DF9006` | A member doing too many different things with too few values. | `dotnet_fast_max_halstead_difficulty` (80) |
| `DF9007` | A member that is too hard to read, weighted by nesting. | `dotnet_fast_max_cognitive_complexity` (22) |
| `DF9008` | A `///` documentation comment on anything but a class or record. | — |

Two more of the ten published code-health budgets — redundant code and dynamic typing — are
deliberately **not** guardrails here; see
[Redundant code and dynamic typing](#redundant-code-and-dynamic-typing-are-covered-elsewhere) below.

## Enabling them

Each rule is switched on individually, in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.DF9001.severity = warning
dotnet_diagnostic.DF9002.severity = warning
dotnet_diagnostic.DF9003.severity = warning
dotnet_diagnostic.DF9004.severity = warning
dotnet_diagnostic.DF9005.severity = warning
dotnet_diagnostic.DF9006.severity = warning
dotnet_diagnostic.DF9007.severity = warning
dotnet_diagnostic.DF9008.severity = warning

# Thresholds, shown with their defaults — omit any you are happy with.
dotnet_fast_max_lines_per_function = 50
dotnet_fast_max_lines_per_file = 250
dotnet_fast_magic_number_allowed = 0,1,-1,2
dotnet_fast_max_cyclomatic_complexity = 22
dotnet_fast_max_halstead_difficulty = 80
dotnet_fast_max_cognitive_complexity = 22
```

Or run `dotnet-fast editorconfig recommend --guardrails --write` to append all eight at once, with
every threshold stated at the published code-health budget number rather than left to the rule's own
default — see [Adopting them without a wall of findings](#adopting-them-without-a-wall-of-findings).

Only a per-rule `dotnet_diagnostic.DF900x.severity` enables one. A bulk
`dotnet_analyzer_diagnostic.severity` key does **not**, mirroring how Roslyn treats an analyzer that
is disabled by default — so a repository that already sets a bulk severity does not silently acquire
eight new rule families on upgrade.

All eight are **report-only**. `lint --fix` never rewrites code on their account: "extract this into a
method" has no mechanical fix, and a tool that guessed at one would do more harm than good.

## The messages are the feature

Each message is written for the agent that has to act on it. It names **the design principle at
stake and the concrete move**, not just the violation — because "file too long" invites deleting
blank lines, while naming the single-responsibility principle invites separating concerns.

`DF9003` does not say *file too long*. It says:

> `File is 412 lines (limit 250). This is the SINGLE RESPONSIBILITY PRINCIPLE (the S in SOLID)
> stated as a measurement: a file this long almost always has more than one reason to change. Before
> editing further, identify the distinct responsibilities here and separate them — give each type its
> own file, and extract each collaborator behind an interface so callers depend on the abstraction
> rather than this file's internals (the D in SOLID). Adding to a file already over the limit
> compounds the problem; prefer a new, well-named type over another member here.`

`DF9002` does not say *method too long*. It says:

> `Method 'BuildReport' is 84 lines (limit 50). SINGLE RESPONSIBILITY applies to a member as much as
> to a class: this one is doing several jobs at once. Apply EXTRACT METHOD — find each cohesive step
> (a block you could name, often one already marked by a blank line or a comment), move it to its own
> private method named for WHAT it achieves rather than how, and repeat until the body reads as a
> list of intentions. Each extracted step also becomes separately testable, which is usually the
> larger win. If the steps do not share the member's state, that is a sign they belong on a different
> type entirely.`

`DF9004` does not say *magic number*. It names the value's meaning as the thing that is missing, and
points past the minimum fix:

> ``Magic number `3600` has no name, so its meaning lives only in the head of whoever wrote it.
> Extract it to a named constant (`private const … = 3600;`) named for what the value MEANS, not what
> it is — `RetryLimit`, not `Three`. This is DRY applied to knowledge rather than to code… If the
> value is a policy someone might tune (a timeout, a threshold, a page size), a constant is the
> minimum — consider whether it belongs in configuration or as a constructor parameter.``

`DF9001` is the one that most often gets the wrong edit — an agent told "no comments" may simply
delete the comment and lose what it knew. So the message distinguishes the two kinds:

> `…rename to intent-revealing names, extract the described block into a well-named method, or
> replace a literal with a named constant — then delete the comment… If the comment records WHY
> rather than WHAT (a rejected alternative, a workaround for an external bug, a non-obvious
> constraint), that knowledge is worth keeping — move it to `///` XML documentation, which is exempt.`

Run `dotnet-fast lint --explain DF9001` for the full description of any rule.

## What each rule is actually for

### `DF9001` — no explanatory comments

A comment usually marks the place where the code failed to say what it does. The durable fix is an
intent-revealing name, or extracting the described block into a well-named method. Comments rot
silently; names are checked by every reader.

**`///` XML documentation is exempt.** A public API contract is exactly where prose is the right
medium — the rule pushes explanation out of the body and into names, not out of the codebase.

Block comments (`/* … */`) are reported alongside `//`. That is deliberate: exempting them would
leave a one-keystroke way around a rule whose entire value is that it cannot be worked around.

Also exempt: this tool's own suppression directives (otherwise the rule could not be suppressed) and
`<auto-generated>` markers.

### `DF9002` — a line budget per member

A member past the limit is nearly always several jobs in a trench coat. The budget is what triggers
the extraction.

Measured from the first non-attribute token to the closing brace, blank lines excluded. Attribute
lines do not count — an eight-line `[Theory]`/`[InlineData]` stack should not consume a sixth of a
test's budget.

Lambdas and anonymous methods are not measured separately: their lines already count toward the
enclosing member, so reporting both would be two findings for one overlong body.

### `DF9003` — a line budget per file

The single-responsibility principle, stated as a measurement. A file this long has more than one
reason to change.

Reported once, at line 1, because the fix is structural rather than local. Generated files are
already outside the lint walk unless you pass `--include-generated`.

### `DF9004` — no unnamed numbers

Values are matched by value, not spelling: `0x02`, `2L` and `2.0` all satisfy a configured `2`.

Exempt are the positions where the surrounding syntax **already names the value**: a `const` or
`readonly` field initializer, a `const` local, an `enum` member, a parameter default, and an array
index.

Attribute arguments and `case` labels are **not** exempt. Both require a compile-time constant, but a
named `const` satisfies that requirement — so `[MaxLength(255)]` and `case 9:` are precisely the
sites worth naming.

### `DF9005` — a budget on branching

Cyclomatic complexity counts the independent paths through a member: one for the entry, plus one for
each `if`, loop, `catch`, ternary, short-circuiting `&&`/`||`/`??`/`?.`, `case` label, switch arm,
`when` guard and pattern `and`/`or`. A bare `else`, a `default:` label and a `_` arm add nothing —
they are the path you already counted.

That number is also, exactly, the number of cases a test suite has to cover to exercise the member.
A member at 40 needs 40 tests. That is usually the argument that lands.

**It is a different measurement from `S3776`**, the cognitive-complexity rule shipped among the
ported analyzers, and the two disagree on purpose. Six `if`s nested six deep score 21 for cognitive
complexity and 7 here; the same six written flat score 6 and 7. Cognitive complexity asks *how hard
is this to read*, cyclomatic asks *how many cases are there*. A long flat `switch` is easy to read
and still needs a test per arm.

The fix is to reduce branching, not line count: guard clauses that return early, a lookup table or
polymorphic type in place of a long `switch`, and each self-contained decision extracted into a named
predicate so the reader sees *why* a branch exists.

### `DF9006` — a budget on density

Halstead difficulty is `(distinct operators / 2) × (total operands / distinct operands)`, computed
over the member's tokens. It rises with the **variety** of operations in one member, and with how
often the same few values are re-read — together, the signature of a member juggling several levels
of abstraction at once.

It is the one limit here that does **not** fall by splitting a member in half: both halves keep the
same density. It falls when a step is given a name — a well-named helper in place of a chain of
primitive operations, or a type that owns the values currently being passed around together.

Because the partition of tokens into operators and operands is a choice every implementation makes
differently, ours is stated rather than left implicit: unnamed leaves (keywords, punctuation,
operators) are operators; named leaves (identifiers, literals, predefined type names) are operands;
comments are ignored entirely.

### `DF9007` — a budget on nesting

Cognitive complexity is `S3776`'s measure, scored by the same code: unlike `DF9005`, it **weights
nesting**. `if`/`for`/`foreach`/`while`/`do`/`switch`/`catch` each cost `1 + current nesting depth`;
`else`/`else if` cost a flat `+1` with no nesting term; entering any qualifying body raises nesting for
what is inside it. Six `if`s nested six deep score 21; the same six written flat score 6.

That is the point: it asks *how hard is this to read*, where `DF9005` asks *how many cases are there*.
A long flat `switch` can score low here and still need a test per arm — the two rules disagree on
purpose, and both are worth having.

The fix is to reduce **nesting**, not branch count: guard clauses that return early instead of wrapping
the rest of a body in an `if`, inverting a condition to flatten an `else`, and extracting a deeply
nested block into its own well-named method so the reader is never more than a level or two deep at
once.

Measured over the member's `body`; an expression-bodied member (`=>`) has none and is not measured,
matching `S3776`. Shares its scorer with `S3776` and `dotnet-fast metrics`, so no two of the three can
disagree about a member.

### `DF9008` — documentation lives on the type

`DF9001` above exempts `///` documentation because a public API contract is exactly where prose is the
right medium. This rule says **where** that contract belongs: on the `class` or `record`, and nowhere
else. Documentation on a property, method, field, event, constructor or accessor is reported.

The premise is that a type states its contract **once**, in one place a reader can find, and every
member is expected to say what it does through its **name and signature** instead. Member-level prose
is a second copy of that contract which nothing checks — it rots the first time the code moves on
without it, and a wrong comment costs more than no comment.

So the fix is to delete it. If it carried something the signature genuinely does not say — a unit, an
accepted range, a thrown exception, a threading rule — that is a gap in the *design*, not in the
prose, and the durable move is to close it:

| The comment says | The design move |
|---|---|
| `/// <summary>Timeout in seconds.</summary>` on `int Timeout` | Rename to `TimeoutSeconds`, or make the type `TimeSpan`. |
| `/// <summary>Must be between 1 and 100.</summary>` | A named type that cannot hold an invalid value, or a guard clause that says so in code. |
| `/// <summary>Not thread-safe.</summary>` | Fold it into the **type's** `<summary>` — it is a property of the whole contract. |

**This is narrower than "a type" on purpose.** Only `class` and `record` (including `record class` and
`record struct`) may carry documentation. An `interface`, `enum`, `struct` or `delegate` is reported
like any member — the rule enforces one documented shape, not a taxonomy of exceptions. If that is not
the convention you want, this is the guardrail to leave off; it is the most opinionated of the eight.

Mechanics worth knowing:

- A multi-line block reports **once**, at its first line. The grammar makes every `///` line its own
  comment, and five findings for one edit would be noise. A blank line starts a new block.
- `/** … */` is reported alongside `///` — it is documentation to the C# compiler too, so exempting it
  would leave a way to document a member that the rule never sees.
- Documentation attached to **no** declaration at all — the last thing in a block or a file — is
  reported, because it will never appear next to the thing it describes.
- Exempt, as in `DF9001`: dotnet-fast suppression directives and `<auto-generated>` markers, which are
  commonly written as `///` at the head of a generated file.

**`DF9008` contradicts `SA1600`, and you must pick one.** The ported StyleCop analyzer
[`SA1600`](ported-analyzers.md) ("Elements should be documented") requires a `///` comment on every
externally visible type **and member** — the exact opposite of this rule. `SA1601` (partial elements)
and `SA1602` (enumeration items) say the same thing for their own shapes. All three are ports, so
unlike this family they are **on by default**. Enable `DF9008` without turning them off and every
public member draws two findings that cannot both be satisfied: one telling you to add documentation,
one telling you to remove it.

There is no clever reconciliation here — they encode genuinely opposing conventions, and only you know
which one your codebase wants. If you want `DF9008`, switch the other three off explicitly:

```ini
[*.cs]
dotnet_diagnostic.DF9008.severity = warning

# SA1600/SA1601/SA1602 require documentation on exactly what DF9008 forbids it on.
dotnet_diagnostic.SA1600.severity = none
dotnet_diagnostic.SA1601.severity = none
dotnet_diagnostic.SA1602.severity = none
```

The rest of the `SA16xx` documentation family (`SA1604` onwards) does **not** conflict: those rules
validate documentation that already exists — a missing `<param>`, a `<returns>` on a `void` method —
so they simply never fire on a member that has none. You can leave them on.

`dotnet-fast editorconfig recommend --guardrails` writes those three lines **commented out**, with the
conflict spelled out above them — uncomment them if `DF9008` is the convention you want, or drop the
`DF9008` line and keep the analyzers'. They are not written active because turning a default-on
analyzer off is a bigger decision than switching a guardrail on, and the profile is append-only and
additive by design.

**One interaction to know about.** `DF9001`'s message tells you that a comment recording *why* is worth
keeping, and to move it to `///` documentation. With `DF9008` also enabled, the destination for that
move is the **type's** `<summary>` — not a `///` on the member the comment was sitting in, which trips
this rule instead. The two rules are complementary halves of one axis rather than a contradiction:
`DF9001` reports every comment that is not documentation, `DF9008` only ones that are, so no comment
ever draws both findings.

## Redundant code and dynamic typing are covered elsewhere

Two more of the ten published code-health budgets — redundant code and dynamic typing — could in
principle have become guardrails of their own. They do not, on purpose: **[`S4144`](ported-analyzers.md)** already
reports identical member bodies and **[`PH2044`](ported-analyzers.md)** already reports the `dynamic`
keyword, and both are ported analyzers that are on **by default** (opt-out, unlike this opt-in family).
A guardrail duplicating either detector would fire twice on the same line for anyone who has not
disabled the port — two findings for one defect is a bug, not a second opinion — and there is no
narrower or differently-scoped version of either check worth building: the ports already cover the
budget's full scope, so a second implementation would only be a second place for the same logic to
drift from the first. So: enable `S4144` and `PH2044` (on by default; nothing to switch on) to cover
those two budgets, and use `DF9001`–`DF9007` above for the other five. (`DF9008` is not a
code-health budget at all — it is a documentation convention, added because the same "an agent cannot
ignore a lint error" logic applies to where documentation lands.)

## Seeing where you stand

The guardrails are a ratchet: they stop new violations landing. To see the whole picture — including
coverage, CRAP and surviving mutants — run [`dotnet-fast metrics`](commands.md#metrics), which scores
a codebase against ten budgets in one pass and shares its complexity scorers with `DF9005`, `DF9006`
and `DF9007`, so the CI gate and the scoreboard can never disagree about a member.

**One thing they do not share: the budgets themselves.** `dotnet_fast_max_lines_per_file` here
(`DF9003`, default 250) and `metrics.maxLinesPerFile` (default 500) are two independent numbers that
never read each other, even though both are called a "line budget". See
[editorconfig.md](editorconfig.md#two-budget-systems-two-different-numbers) for the full picture,
including why cyclomatic and Halstead currently agree and lines-per-file does not.

## Adopting them without a wall of findings

Turning all seven on across an existing codebase will produce a lot of output. Three approaches that
work, from least to most effort:

**Let the tool write the whole block.** `dotnet-fast editorconfig recommend --guardrails --write`
appends every `DF900x.severity = warning` line plus every threshold — each stated explicitly at the
published code-health budget number, including `dotnet_fast_max_lines_per_file = 500` (the published
SLOC budget; this rule's own bare default, kept for backward compatibility, is 250). Idempotent:
running it again when the profile is already present is a no-op, and it only ever appends, never
overwrites, an existing `.editorconfig`.

**Scope them to where they earn their keep.** Test projects legitimately carry magic numbers and long
fixtures:

```ini
[*.cs]
dotnet_diagnostic.DF9004.severity = warning

[**/*Tests/**.cs]
dotnet_diagnostic.DF9004.severity = none
```

**Fail only on new findings.** `lint --baseline` records what exists today and fails only on what is
added after:

```bash
dotnet-fast lint --baseline guardrails.json .
```

That is usually the right on-ramp: the constraint applies to new code — which is where the agent is
working — without demanding a cleanup of everything already written.

## Suppressing individually

Same mechanisms as any other rule:

```csharp
Wait(3600); // dotnet-fast-disable-line DF9004

#pragma warning disable DF9004
```

## A note on thresholds

An unparseable threshold keeps the **default** rather than removing the limit:
`dotnet_fast_max_lines_per_file = fifty` leaves the limit at 250. A typo that silently switched a
guardrail off would turn a green build into a false green, which is the one failure mode a guardrail
must never have.

## Related

- [commands.md](commands.md#lint) — the full `lint` reference
- [commands.md](commands.md#editorconfig) — `editorconfig recommend --guardrails`, the one-command
  adoption profile
- [ported-analyzers.md](ported-analyzers.md) — the Roslyn analyzers re-implemented natively, which
  are on by default and are a different thing entirely; includes `S4144` and `PH2044`, which cover
  the redundant-code and dynamic-typing budgets instead of a `DF900x` guardrail

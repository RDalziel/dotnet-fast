# Release notes

What changed in recent releases, in plain English. Newest first.

The current **stable** line is `1.5.0`. Pre-1.0 history — predating the compatibility promise and the
NuGet package — is a git-history pointer, not full notes, in
[RELEASES-0.x.md](RELEASES-0.x.md).

## 1.5.0 — 2026-09-09

### New guardrail: `DF9008` — documentation lives on the type

`DF9001` reports comments that explain code, and exempts `///` XML documentation, because a public
API contract is where prose belongs. Nothing bounded that exemption: a `///` block on every property
and method satisfied `DF9001` while putting a second, unchecked copy of the type's contract on each
member — a copy that rots the first time the code moves on without it.

`DF9008` says **where** documentation belongs: on the `class` or `record`, and nowhere else.
Documentation on a property, method, field, event, constructor or accessor is reported, as is a
`///` block attached to no declaration at all. `/** … */` counts as documentation too. A multi-line
block reports once, at its first line.

The premise is that a type states its contract once, in one place a reader can find, and every member
says what it does through its name and signature. When a member comment carried something the
signature genuinely does not say — a unit, an accepted range, a threading rule — the rule's message
points at the design move that closes the gap for good (`int Timeout` → `TimeoutSeconds`, or a
`TimeSpan`) rather than at a better sentence.

It is the most opinionated of the family, and deliberately narrow: only `class` and `record`
(including `record class` and `record struct`) may carry documentation — an `interface`, `enum`,
`struct` or `delegate` is reported like any member.

**`DF9008` contradicts `SA1600`/`SA1601`/`SA1602`, and you must pick one.** Those ported StyleCop
analyzers require documentation on exactly what `DF9008` forbids it on, and being ports they are on
by default. Enable `DF9008` without switching them off and every public member draws two findings
that cannot both be satisfied. They encode opposing conventions, so there is nothing to reconcile in
code — [guardrails.md](docs/guardrails.md) spells out the conflict and gives you the three
`severity = none` lines, and `editorconfig recommend --guardrails` now writes them **commented out**
with the conflict stated above them, so adopting the profile is still one command and turning a
default-on analyzer off stays your decision. The rest of the `SA16xx` family does not conflict.

Like the rest of `DF900x` it is **off by default** and enabled only by its own
`dotnet_diagnostic.DF9008.severity` line — a bulk `dotnet_analyzer_diagnostic.severity` key does not
switch it on, so no repository acquires it on upgrade. It is report-only; `lint --fix` never deletes
a comment on its account.

## 1.4.1 — 2026-09-07

### Fix: a changed F# file no longer slips through a scoped `lint`

`lint --staged`, `--affected`, `--ci`, `--pr-base`, `--base` and `--from`/`--to` asked Git only for
the changed **`.cs`** files. A changed `.fs`, `.fsi` or `.fsx` file was dropped from the scope
before anything looked at it, so the F# hygiene rules (`FSH0001`–`FSH0004`, new in 1.4.0) and the
`--fantomas` lane never saw it and the run reported clean — while a plain `lint .` over the same
tree reported the findings. That is precisely how a pre-commit hook and a PR gate invoke this tool,
so staging a problem F# file passed the gate.

Changed-file discovery now covers every language the tool reads — `.cs` plus `.fs`/`.fsi`/`.fsx` —
and the extension list is derived from one place, so a language added later cannot drift out of the
Git queries again. Changed-line (hunk) scoping applies to F# exactly as it does to C#: an F# file
touched on one line reports only that line's findings, and `lint --fix` on a scoped run fixes the
in-scope F# files without touching F# files outside the scope.

**C# behaviour is unchanged** — the scope was widened, never narrowed, so a C#-only repository
produces byte-identical output on every scoped command.

This completes the F# work that began with [issue #1](https://github.com/RDalziel/dotnet-fast/issues/1).
With 1.4.0 and this fix together, one invocation covers both languages and only what changed:

```bash
dotnet-fast lint --fix --fantomas --staged .
```

C# formatted and linted, F# hygiene fixed, F# structure formatted by your pinned Fantomas. What F#
does and does not support, with the reason for each limit, is in
[support-matrix.md](docs/support-matrix.md).

## 1.4.0 — 2026-09-07

### F# formatting: `--fantomas` drives Fantomas, so `--verify-no-changes` can finally tell the truth about F#

`dotnet-fast lint --fantomas` and `dotnet-fast format --fantomas` format F# by running
[Fantomas](https://fsprojects.github.io/fantomas/), the community-standard F# formatter — we do not
reimplement it, and we do not intend to. F# is offside-rule: indentation is *syntax*, so a formatter
that guesses wrong emits code that will not compile, and there is no oracle to catch that, because
`dotnet format` has no F# support at all. Point real `dotnet format` at an `.fsproj` and it prints
one line and **exits 0** — its `--verify-no-changes` gives a green build on completely unformatted
F#. That is the gap this closes.

**Fantomas decides how F# looks; we decide what gets formatted, when, and how it is reported.**
Fantomas owns every style decision and is configured entirely by its own `.editorconfig` keys — the
`fsharp_*` namespace plus `max_line_length`, `indent_size`, `end_of_line` and `insert_final_newline`.
No `dotnet-fast` setting changes F# formatting.

What `dotnet-fast` adds is the workflow:

- **Project-aware, affected-scoped discovery.** `--project`, `--include`/`--exclude`, `--affected`,
  `--staged`, `--from`/`--to` and `--ci` narrow the F# set exactly as they narrow the C# one.
- **Unchanged files are skipped.** Fantomas has no notion of what changed and re-parses everything
  every time. A content hash — keyed together with the resolved Fantomas version and the
  `.editorconfig` chain, so an upgrade or a config edit correctly invalidates — means a second run
  over an untouched tree starts Fantomas **zero** times.
- **Batched.** Every file in scope goes out in as few invocations as a command line holds. On a
  48-file fixture that is one process instead of 48 (~38× on the measured fixture), and the skip
  cache then removes even that one on an unchanged re-run (~10× again).
- **`.fantomasignore`, with Fantomas' own semantics** — the nearest one wins and they are *not*
  merged the way `.gitignore` files are, so our file set and Fantomas' cannot drift apart.
- **One report.** F# results land in the same summary, `--report` JSON and `--sarif` output as the C#
  ones, tagged `engine: "fantomas"` / `language: "fsharp"`.

Two findings, and they are deliberately different answers: `FSFMT001` "Fantomas would reformat this"
(fixable) and `FSFMT002` "Fantomas could not process this" (not fixable — usually a file whose `#if`
branches are not each valid F# on their own). A file that did not parse has not been checked, and is
never reported as formatted.

**Strictly opt-in and strictly additive.** Without the flag, nothing changes: `format` still never
rewrites an F# file, and a C#-only run is byte-for-byte identical with and without `--fantomas`.
Nothing is ever downloaded or installed — if Fantomas is missing, the run **fails with exit `168`**
and prints the install command, rather than silently checking no F# and reporting success. The
version pinned in the repository's `.config/dotnet-tools.json` is preferred over a global install, so
everyone formats with the version the team chose.

### F# formatting hygiene: four line-level rules `lint` now reports on `.fs`, `.fsi` and `.fsx`

`dotnet-fast lint` previously had nothing to say about F# source. It now reports four things, and
only four:

| ID | Reports | `--fix` |
|---|---|---|
| `FSH0001` | Trailing whitespace at end of line | yes |
| `FSH0002` | File does not end with a newline | yes |
| `FSH0003` | Line ending does not match the resolved `end_of_line` | yes |
| `FSH0004` | Tab character in leading whitespace | **no — reported only** |

**The narrowness is the design, not a first cut.** F# is an offside-rule language: leading whitespace
is syntax, so changing indentation changes what the program means. Reprinting F# properly needs a
real parse, and [Fantomas](https://fsprojects.github.io/fantomas/) — an AST-based reprinter built on
a fork of the F# compiler — is the community standard for that. We are not competing with it: the
`--fantomas` lane above orchestrates it. These four are the changes that **cannot alter a parse at
all**. Indentation, line wrapping, spacing and anything structural are explicitly out of
scope.

**`FSH0004` is a correctness check, not a style opinion.** The F# compiler rejects a tab outside a
string literal or comment — error FS1161, "TABs are not allowed in F# code". It is reported and
never fixed, because converting a tab to spaces needs the indent width the author meant, and under
the offside rule a wrong width silently moves the line into a different block while still compiling.
That asymmetry with the other three rules is deliberate.

Details worth knowing:

- **No parser.** These are checks on bytes, so they keep working on the F# files a grammar cannot
  parse — which is exactly where you most need a tool to still say something.
- **`.fsx` scripts are covered.** A script is not a `<Compile>` item of any project, so it is found
  by walking each `.fsproj`'s directory. A script outside every `.fsproj` directory is not reached.
- **`.editorconfig` as usual**, with no new keys: `end_of_line`, `insert_final_newline`,
  `trim_trailing_whitespace`, and `dotnet_diagnostic.FSH000x.severity` (where `none` removes the
  finding *and* withholds the fix).
- **Nothing else moved.** The rules run under `lint` / bare `dotnet-fast` / `lint --fix` only. The
  `dotnet format`-compatible verbs (`format`, `whitespace`, `style`, `analyzers`, and the
  `dotnet-format-fast` binary) never touch F#, because real `dotnet format` never did. A C#-only
  repository sees byte-identical output in every mode, and the `DFxxxx` rule counts printed by
  `lint --list-rules` are unchanged — `lint --explain FSH0004` is how you read these four.

See [rules.md](docs/rules.md#f-formatting-hygiene-fsh0001fsh0004).

### `test-plan` no longer drops a test project in silence

Reported as [issue #1](https://github.com/RDalziel/dotnet-fast/issues/1): a project that
`test-plan` classified as a test project but discovered no fixtures in was removed from the plan
with **no diagnostic and exit `0`**. The reporter hit it after adding an F# NUnit project to a
sharded workspace — the project built, `dotnet test` ran its three tests locally, and CI stayed
green while never executing them. A false green is the worst thing this tool can produce, so both
halves are fixed.

- **Every such project is now reported.** It is named on stderr with the reason and the consequence
  ("none of its tests will run in any shard"), and appended to `--format json` and the
  `--report <DIR>` artifact as a new `skippedProjects` array. `reason` is one of
  `language-not-supported`, `source-discovery-failed`, `no-source-files`, `no-parsable-sources`,
  `some-sources-unparsed` or `no-fixtures`. Existing JSON fields are unchanged; the array is always
  present, usually empty.
- **New `--fail-on-skipped-projects`** exits `167` instead of warning. Off by default: a scaffolded
  but still-empty test project is legitimate, and a new non-zero default would break those repos.
  Turn it on once yours has none, and a test project that quietly stops contributing tests can never
  report green again.
- **F# NUnit fixtures are discovered.** `[<TestFixture>]` types, types with `[<Test>]` members and
  no attribute, module-level `[<Test>] let` bindings (the module is the fixture), nested modules,
  `[<AbstractClass>]` bases, `[<SetUpFixture>]` exclusion, and ``double-backtick`` names. Fixture
  names use the same runtime form as C# — a module compiles to a class, so a type in
  `module CalculatorTests` is `CalculatorTests+CalculatorTests`. Every name form was checked against
  a real `dotnet test` run. See [test-sharding.md](docs/test-sharding.md#f-test-projects).
- **`.fsproj` compile items are evaluated properly** rather than falling back to a `*.cs` directory
  walk: no implicit compile glob (the F# SDK disables it), declared order preserved, `.fs` and
  `.fsi` included, `.fsx` scripts excluded.
- **A solution resolves to the same project set everywhere.** `affected` returned an `.fsproj`
  listed in a `.sln`/`.slnx` while `format`/`lint` skipped it. Both now read one predicate, so an
  F# project in a solution is visible to every command. An `.fsproj` is also accepted as a target
  where it used to be rejected outright.
- `docs/support-matrix.md` said flatly that "VB and F# projects are not analyzed", which
  contradicted `affected`'s documented behaviour and is now wrong for `test-plan` too. It states
  what each command actually reads.

No existing flag, default, output field or exit code changed.

### `--help` doc pointers fixed

Three `--help` descriptions (`dead-code`, `dead-dependencies`, and `lint`'s
`--only-active-analyzers`) linked to a doc path that does not exist in the public repo — the pointer
went nowhere. `dead-code --help` now points at `docs/dead-code.md`, `dead-dependencies --help` at
`docs/dead-dependencies.md`, and `--only-active-analyzers` at
`docs/ported-analyzers.md#enabling-and-disabling-ports`. Text-only fix — no flag, default, or output
shape changed.

## 1.3.0 — 2026-09-03

**Action required if `metrics` gates your build: re-run `dotnet-fast metrics --write-baseline` before
you upgrade a baseline you rely on.** The scoreboard grows from ten budgets to fourteen — a baseline
recorded before this release never mentions the four new ids, so they are judged against their plain
budget the first time this build measures them, exactly like any other newly-measured row. That is
usually the right behaviour, but it means a repository that was previously green under `--baseline`
can now fail on debt the ratchet never saw. Re-running `--write-baseline` records where the four new
rows stand today and keeps the ratchet meaning what it always has.

### `metrics` — four new rows, `--top all`, `--by-project`

- **Four rows appended** (never renumbered): lines per member (≤ 50), nesting depth (≤ 3), parameter
  count (≤ 7), and maintainability index (≥ 20 — a floor, like coverage). Every default is lifted from
  a threshold this tool already ships elsewhere (the `DF9002` guardrail, the native `S134`/`S107` lint
  ports), not invented. New `--max-lines-per-member` / `--max-nesting-depth` / `--max-parameters` /
  `--min-maintainability-index` flags and `dotnet-fast.json` keys override them; the text board's
  column widths are unchanged and every one of the original ten lines still prints byte-for-byte what
  it always has.
- **`--top all`** (or `--top unlimited`) lists every offender behind an over-budget row instead of the
  default 5 — the only expensive flag here, by orders of magnitude on a large solution.
  `worstMembers` stays capped at 5 regardless, so it cannot grow into a multi-megabyte JSON array.
- **`--by-project`** (opt-in) breaks the board down one line per project — text gets an appended
  block, JSON gets an appended top-level `projects` array. Whole-run figures (coverage, surviving
  mutants, dead code) are not repeated per project; they read as not measured there. Never affects the
  exit code.
- Agent-mode output now opens with a one-line summary and names what was never measured, and caps
  offenders at 3 per row regardless of `--top`.
### The seventh guardrail, and one-command adoption

**New guardrail: `DF9007`, cognitive complexity.** Of the ten published code-health budgets, the
lint gate previously covered three: cyclomatic complexity (`DF9005`), Halstead difficulty (`DF9006`)
and lines-per-file (`DF9003`). Cognitive complexity had no configurable guardrail at all — the closest
thing was the `S3776` port, pinned at a fixed threshold of 15 on a different config axis. `DF9007`
closes that gap: opt-in like the rest of the family, default threshold 22, configured with
`dotnet_fast_max_cognitive_complexity`, scored by the same code `S3776` and `dotnet-fast metrics` use
so all three can never disagree about a member. See [guardrails.md](docs/guardrails.md).

Two more budgets — redundant code and dynamic typing — were assessed for guardrails of their own
(`DF9008`/`DF9009`) and deliberately **not built**: `S4144` (identical member bodies) and `PH2044`
(the `dynamic` keyword) already cover them, and both ports are on by default, so a guardrail twin
would double-report the same defect for anyone who has not disabled the port. Use `S4144`/`PH2044`
for those two.

**One-command adoption: `editorconfig recommend --guardrails`.** Adopting the guardrail family used to
mean switching on six rules (now seven) and setting five thresholds by hand, one of which (lines per
file) has always meant something different from the published SLOC budget — the rule's own default
is 250, the budget is 500. `dotnet-fast editorconfig recommend --guardrails --write` now appends the
whole block in one command: every `DF900x.severity = warning` line, and every threshold stated
explicitly at the published budget number (`dotnet_fast_max_lines_per_file = 500` included). Same
`--write` contract as the existing `recommend` command — appends, never overwrites, idempotent on a
second run.
### `metrics`: estate rollup across repositories (`metrics --merge`)

`metrics --format json` now appends two fields to its `summary`: `schemaVersion` and `repository`
(inferred from the git `origin` remote, overridable with `--repository-name`). New
`metrics --merge <glob>` reads many repositories' own `--format json` reports — offline, no server —
and renders one combined view (`text`, `json`, or a self-contained `html` page) sorted worst-first,
with a per-budget breach summary across the estate. Only budgets a source-only pass can see (source
statics and the opt-in dead-code pass) appear for every repository that ran them; the three
budgets that need a test or mutation run (coverage, CRAP, surviving mutants) appear as a column only
where at least one repository actually measured them, and never as a placeholder for the rest — a
blank column reads as "not measured", never as a pass. A file `--merge` cannot understand (missing or
unrecognised `schemaVersion`) is skipped with a clear reason, never silently merged. Everything here
is additive: a plain `metrics` run with no `--merge` is unchanged. See [metrics.md](docs/metrics.md)
and [ci.md](docs/ci.md).

## 1.2.0 — 2026-09-01

**Action required if `metrics` gates your build.** Five fixes below change numbers `metrics` reports —
mostly upward, because each one was under-reporting. Re-run `dotnet-fast metrics --fail-on-budget`
on a branch and look at the new figures before you let the fixed version gate a merge.

### `metrics` — five fixes

- **CRAP was too low for any member containing a blank line.** The coverage lookup for a member used
  its *non-blank* line count as the span, so it stopped short of the member's real last line and
  scored the missing lines as if they did not exist. Members with blank lines between their steps —
  the readable ones — came out looking better covered than they are. CRAP for those members rises.
- **A coverage report could attach to the wrong file.** File matching compared path suffixes without
  requiring a directory boundary, so `covered.cs` matched `uncovered.cs`, and when more than one
  candidate matched, which one won was not even stable between runs. A match now has to start at a
  path separator, and an ambiguous one is treated as no match — which shows up as an unmeasured file
  rather than a wrong number.
- **An empty mutation report read as a pass.** A Stryker run that scored nothing — a crash, a filter
  that matched no code, a report written before the run finished — produced `Surviving mutants 0`,
  green. `metrics` now fails with a clear message instead, the same way an empty coverage report
  already did. A board that is green because nothing was checked is worse than no board.
- **Fractional budgets printed rounded.** `--min-coverage 99.5` rendered as `100` in the text table
  and in the JSON `budget` field, so the report disagreed with the flag you passed. Budgets now print
  as given (`99.5` stays `99.5`, `100` stays `100`).
- **Files it could not read, or could not parse, vanished silently.** A file that is not valid UTF-8,
  or that a permission error hides, was skipped without a word, so the scoreboard quietly described
  less code than you thought. Both are now counted and reported: a new footer line, plus
  `summary.readFailures` and `summary.filesWithSyntaxErrors` appended to the JSON. Files with syntax
  errors are still measured — they are counted, not excluded.

### `metrics` — new options

- **Budgets for the four count metrics**: `--max-surviving-mutants`, `--max-dead-code`,
  `--max-duplicate-bodies` and `--max-dynamic-uses`, each also settable once in `dotnet-fast.json`
  as `maxSurvivingMutants`, `maxDeadCode`, `maxDuplicateBodies` and `maxDynamicUses`. Previously
  those four were fixed at zero.
- **More coverage formats.** `--coverage` now reads **OpenCover**, **LCOV** and **coverlet JSON** as
  well as Cobertura, and works out which it is from the file itself. `--coverage-format
  auto|cobertura|opencover|lcov|coverlet-json` pins the choice when you would rather be explicit.
  Directory search picks up `*.opencover.xml`, `lcov.info`/`*.lcov` and `coverage.json` too.
- **`--coverage` and `--mutation` are repeatable.** Pass one per test project and the reports are
  merged. A single use behaves exactly as before.
- **A ratchet, so you can adopt budgets on a codebase that is already over them.**
  `--write-baseline <file>` records where you stand today; `--baseline <file>` with
  `--fail-on-budget` then fails only on a row that got *worse* than the baseline. Rows that are over
  budget but no worse say so and do not fail the build. Without `--baseline`, nothing changes.
- **`--format sarif`**, so over-budget members and files land in GitHub code scanning next to your
  lint findings.
- **Worst offenders in the JSON.** Every metric row now carries an `offenders` array (name or file,
  line, value), sized by `--top`, and the text report lists offenders for surviving mutants,
  redundant code, `dynamic` usage and dead code as well.

### Everywhere else

- **The NuGet package page now shows the same README as the docs site.** The package had been
  shipping an internal document by mistake, with links that went nowhere for anyone but us.
- **Every release now has a downloadable binary.** `dotnet-fast-win-x64.exe` and its `.sha256` are
  attached to each GitHub release, closing the gap [security.md](docs/security.md) previously
  described as a 1.x item. NuGet remains the recommended install and the one with a signature —
  a checksum next to a file only catches a corrupted download.
- **These docs now have history.** The public repository keeps a commit per version with a `vX.Y.Z`
  tag, so you can read what a page said at the version you are running instead of only its current
  state.

Nothing existing changed: no command, flag, default, output shape or exit code moved.

## 1.1.0 — 2026-09-01 — `metrics`: one scoreboard for the numbers that catch AI slop

A new command, **`dotnet-fast metrics`**, scores a codebase against ten code-health budgets in one
build-free pass:

```
dotnet-fast metrics App.sln
```

Seven of them come straight from your source, measured in the same walk: cyclomatic complexity,
cognitive complexity, Halstead difficulty, lines per file, dead code, redundant code, and `dynamic`
usage (C#'s nearest equivalent to TypeScript's `any`). The other three cannot be derived from source
because they require running your tests — so `metrics` **reads the reports your pipeline already
produces** and never runs anything itself:

- `--coverage <path>` — a Cobertura report (`coverage.cobertura.xml`), or a directory to search. This
  also fills in **CRAP**, which multiplies complexity by lack of coverage so that a member which is
  both complex and untested rises to the top of the list.
- `--mutation <path>` — a Stryker.NET `mutation-report.json`, or a directory to search.

`--fail-on-budget` turns it into a CI gate; without it the command is report-only and exits 0.
`--format json` gives the machine-readable form, including the full Halstead figures per member.

**A metric you did not measure is never shown as passing.** Without a coverage report, the coverage
and CRAP rows read *not measured* and name the flag that would fill them, and `--fail-on-budget`
ignores them entirely. A board that is green because nothing was checked would be worse than no board.

Budgets default to widely-published values (cyclomatic and cognitive 22, Halstead difficulty 80, 500
lines per file, 100% coverage, CRAP 25, and zero for the rest) and can be overridden per run on the
command line or once in `dotnet-fast.json`.

### Two new guardrails: `DF9005` and `DF9006`

The opt-in guardrail family — the rules for repositories where an agent writes the code and a human
reviews it — gains the two budgets that belong in a per-file CI gate:

| Rule | Reports | Threshold |
|---|---|---|
| `DF9005` | A member over 22 independent paths — also the number of cases its tests must cover. | `dotnet_fast_max_cyclomatic_complexity` |
| `DF9006` | A member over 80 Halstead difficulty — too many different operations over too few values. | `dotnet_fast_max_halstead_difficulty` |

Both are **off by default** and report-only, like the other four, and both share their scorers with
`metrics`, so the gate and the scoreboard can never disagree about a member. See
[guardrails.md](docs/guardrails.md).

Note that `DF9005` measures something genuinely different from the `S3776` cognitive-complexity rule
already shipped among the ported analyzers: a wide flat `switch` scores 25 for cyclomatic complexity
and 1 for cognitive. One asks how many cases there are, the other how hard it is to read. Both are
worth knowing.

Nothing existing changed: no command, flag, default, output shape or exit code moved.

## 1.0.1 — tagged, never published to NuGet, superseded by 1.1.0

`1.0.1` exists as a tag only. It was never pushed to nuget.org, so `dotnet tool install` and
`dotnet-fast update` never saw it and there is nothing to upgrade from. Everything in it shipped in
`1.1.0`. If you pinned `1.0.1` in `.config/dotnet-tools.json`, the restore will fail — use `1.2.0`.

## 1.0.0 — 2026-08-08 — the compatibility promise starts here

**`1.0.0` is `1.0.0-rc.1` plus one fix**, found by the pre-release install verification and described
below. Nothing else in the tool differs between the two. What else changes is what you are entitled
to rely on.

```bash
# new install
dotnet tool install --global RDLL.dotnet-fast

# already have it (including from the candidate)
dotnet-fast update
```

A plain install or update now resolves `1.0.0` — no version flag needed, and the GitHub "latest
release" link points here.

### Fixed

**A `--tool-path` install into a deep directory could crash on its first run.** The failure was an
unhandled .NET exception ending in "The system cannot find the file specified" — about a file that
was sitting right there on disk.

The cause is a Windows limit rather than a missing file: a `--tool-path` install nests about 130
characters of `.store` layout under whatever directory you choose, and once the native binary's full
path reaches 260 characters `CreateProcess` refuses to start it. That limit is not lifted by the
`LongPathsEnabled` setting, so a machine configured for long paths hit it anyway. It affected the
install shape our own CI guidance recommends (`--tool-path <dir>`, then cache `<dir>`); the default
global install is nowhere near the limit.

The launcher now starts such a binary correctly via its short path, and if that is impossible — 8.3
short names can be disabled per volume — it exits with a message naming the path length, the limit,
and the fix, instead of a stack trace. A package smoke test now installs to a deliberately
over-length path on every run, and fails loudly if the path it produces is *not* over the limit, so
the guard cannot quietly stop testing anything.

### What 1.0 actually promises

Everything in **[versioning.md](docs/versioning.md)**, which was already how this tool was
maintained — the difference is that breaking it now costs a major version instead of being a judgement
call:

- **A pipeline pinned to `1.x` keeps working.** An existing command or flag's name, behavior,
  defaults, output shape (text, `--json` fields, SARIF fields, report file names) and exit codes do
  not change. New capability arrives as new commands and new opt-in flags.
- **Nothing is removed outright.** A retired flag keeps working while printing a deprecation notice
  naming its replacement, for multiple releases, before removal in a major — and then fails with a
  message telling you what to use, not `unknown command`.
- **The parity floors are part of the promise.** A release does not ship below the published
  per-repository floors in [support-matrix.md](docs/support-matrix.md).

### What 1.0 does not mean

It is a compatibility milestone, not a claim of completeness. Stated plainly so you can plan around
it rather than discover it:

- **Windows x64 only.** The package carries a `win-x64` native binary and nothing else, so a Linux or
  macOS install *succeeds* and then fails on first run. linux-x64 is a 1.x item and starts outside the
  promise.
- **Formatter parity is 99%+, not 100%.** The current per-repository figures — measured over files
  *both* tools actually opened, a check the harness now fails on rather than papering over — are in
  the [support matrix](docs/support-matrix.md). Three known divergences remain, each with an open
  issue rather than a silent expectation change.
- **`--deep` findings still come from your analyzers and your SDK**, not from us, so they move when
  those move.

### Upgrading from the candidate

Nothing to do beyond `dotnet-fast update`. If you pinned `1.0.0-rc.1` explicitly in
`.config/dotnet-tools.json` or a CI step, change it to `1.0.0`; the pre-release stays published and
installable, but it will not receive fixes.

## 1.0.0-rc.1 — 2026-08-05 — release candidate for 1.0

The first release candidate for `1.0.0`. Everything below also ships in it.

**It will not reach you by accident.** A plain `dotnet tool install` or `dotnet-fast update` still
resolves the stable line (`0.307.0`), and the GitHub "latest release" link still points at stable.
You get the candidate only by asking:

```bash
# new install
dotnet tool install --global RDLL.dotnet-fast --version 1.0.0-rc.1

# already have it
dotnet-fast update --to 1.0.0-rc.1

# back to stable at any time
dotnet-fast update --to 0.307.0
```

### What 1.0 means

Two documents define it, and both are worth reading before you pin to it:

- **[Support matrix](docs/support-matrix.md)** — what is in scope and what is deliberately not.
  Headline: **Windows x64 is the only supported platform**, and the only one a binary ships for.
- **[Versioning promise](docs/versioning.md)** — what a major, minor and patch will mean after 1.0,
  and the compatibility rules the command surface is held to.

### Fixed since 0.307.0

**We were formatting code `dotnet format` never opens — three separate causes.**

1. `<File>` entries under a `.slnx` solution's Solution Items were treated as **projects**. Where
   that file was an MSBuild traversal project at the repository root, it had no target framework and
   no real compile list, so its default file glob matched *the entire repository* — every `.cs` file
   was then processed a second time under the wrong set of preprocessor symbols.
2. A `#if` line with a trailing comment (`#if !NETFRAMEWORK // not supported here`) failed to parse,
   and an unparseable condition was assumed **active**. Code inside a branch your build excludes was
   being formatted.
3. A UTF-8 byte-order mark at the start of a file hid the `#` of a first-line directive, with the
   same result.

**`affected` could report a project nobody changed.** The same `.slnx` defect promoted that root
traversal file to a project, so `affected` named it on *any* source change. If your CI feeds that
list to `dotnet build`/`dotnet test`, it rebuilt everything — the exact opposite of the point.

**`DF0010` no longer rewrites code that then fails to compile.** Converting `$"{value.ToString()}"`
to `$"{value}"` is only valid on target frameworks with the modern interpolated-string handler. The
fix is now withheld unless *every* target framework of the owning project supports it, and an
unknown framework withholds rather than guesses.

**Files that are entirely inside a disabled `#if` now match `dotnet format` byte for byte.**

### Changed

The four opt-in [guardrail rules](docs/guardrails.md) (`DF9001`–`DF9004`) report the same code as
before, but their messages were rewritten to name the design principle behind the rule and the
concrete refactoring move — single responsibility and extract-method rather than "file too long".
They are aimed at an AI coding agent reading the error and deciding what to do next, where "too
long" invites deleting blank lines and naming the principle invites separating concerns.

### Trying it

If you run it, please report anything that breaks — that is what a candidate is for. Formatter
output differing from `dotnet format`, a fix that does not compile, or a command behaving
differently from its documentation are all worth an issue.

---

## Older releases

The `0.x` series predates the `1.0.0` compatibility promise and the NuGet package; its notes were
retired to git history — **[RELEASES-0.x.md](RELEASES-0.x.md)** says where to find them.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

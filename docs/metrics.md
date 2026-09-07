# Code-health metrics

`dotnet-fast metrics` scores a codebase against fourteen budgets in one build-free pass and prints a
single scoreboard. Eleven of the fourteen come straight from your source; the other three need your
tests to have run, so `metrics` **reads the reports your pipeline already produces** — it never runs
`dotnet test`, Stryker, or anything else itself.

These are the `metrics` command's own budgets, set in `dotnet-fast.json`'s `metrics` section — they
are **not** the `.editorconfig` guardrail thresholds `lint`'s `DF9002`/`DF9003`/`DF9005`/`DF9006` use,
even though several share a default and a scorer.

The pair most likely to catch you out is the file-length budget, because the two knobs are spelled
almost the same and their defaults differ: `metrics.maxLinesPerFile` here defaults to **500**, while
`dotnet_fast_max_lines_per_file` in `.editorconfig` — the one `DF9003` reads — defaults to **250**.
Setting one never changes the other. See [editorconfig.md](editorconfig.md#two-budget-systems-two-different-numbers)
for the two systems side by side.

The option table is in [commands.md](commands.md#metrics). This page is how to use it.

## Quick start

```bash
dotnet-fast metrics App.sln                              # the scoreboard (report-only, exits 0)
dotnet-fast metrics App.sln --coverage TestResults       # add coverage and CRAP
dotnet-fast metrics App.sln --mutation StrykerOutput     # add surviving mutants
dotnet-fast metrics App.sln --include-dead-code          # add the dead-code count (slower pass)
dotnet-fast metrics App.sln --fail-on-budget             # exit 1 when a measured budget is exceeded
```

```
Metric                 Worst  Budget  Status
Cyclomatic complexity     41   <= 22  7 members over budget
Cognitive complexity      38   <= 22  5 members over budget
Halstead difficulty      112   <= 80  3 members over budget
Lines per file           913  <= 500  11 files over budget
Test coverage          71.4%    100%  over budget
CRAP                    96.0   <= 25  9 members over budget
Surviving mutants          -       0  not measured — pass --mutation <mutation-report.json>
Dead code                  6       0  6 symbols over budget
Redundant code             4       0  4 duplicates over budget
Dynamic typing             2       0  2 uses over budget
Lines per member          74   <= 50  6 members over budget
Nesting depth              4    <= 3  2 members over budget
Parameter count             9    <= 7  1 member over budget
Maintainability index   31.2   >= 20  ok
```

The last four rows were appended after the original ten (ids `linesPerMember`/`nestingDepth`/
`parameterCount`/`maintainabilityIndex`) — the column widths above are unchanged, and every existing
row still prints byte-for-byte the same line it always has.

**A metric you did not measure never reads as passing.** An unmeasured row says *not measured*, names
the flag that would fill it, and is ignored by `--fail-on-budget`. A board that is green because
nothing was checked would be worse than no board.

## The fourteen metrics

| Metric | Default budget | `dotnet-fast.json` key | What it means |
|---|---|---|---|
| Cyclomatic complexity | ≤ 22 | `maxCyclomatic` | Independent paths through a member — also the number of cases its tests have to cover. |
| Cognitive complexity | ≤ 22 | `maxCognitive` | How hard the member is to *follow*. Nesting is weighted, so a deep `if` nest scores far above the same branches written flat. |
| Halstead difficulty | ≤ 80 | `maxHalsteadDifficulty` | The variety of distinct operations in a member, and how often the same values are re-read. |
| Lines per file | ≤ 500 | `maxLinesPerFile` | Non-blank lines. The single-responsibility principle stated as a measurement. |
| Test coverage | 100% | `minCoveragePercent` | Line coverage, from `--coverage`. |
| CRAP | ≤ 25 | `maxCrap` | `complexity² × (1 − coverage)³ + complexity`. Falls out of `--coverage`: complex *and* untested dominates the list. |
| Surviving mutants | 0 | `maxSurvivingMutants` | From `--mutation`. Counts both `Survived` and `NoCoverage`. |
| Dead code | 0 | `maxDeadCode` | Reuses the [`dead-code`](dead-code.md) analysis; opt in with `--include-dead-code`. |
| Redundant code | 0 | `maxDuplicateBodies` | Members with byte-identical bodies inside one file. |
| Dynamic typing | 0 | `maxDynamicUses` | The `any` analogue: `dynamic` in a type position (C#); `obj` in a type position, `box`/`unbox`, and the `?` dynamic-lookup operator (F#). See [Languages](#languages-c-and-f). |
| Lines per member | ≤ 50 | `maxLinesPerMember` | Non-blank lines in one member. |
| Nesting depth | ≤ 3 | `maxNestingDepth` | Deepest `if`/`for`/`foreach`/`while`/`do`/`switch` nesting in one member; a flat `else if` chain counts as one level. |
| Parameter count | ≤ 7 | `maxParameters` | Declared parameters on one member. |
| Maintainability index | ≥ 20 | `minMaintainabilityIndex` | The Microsoft-normalised `0`–`100` figure (`171 - 5.2·ln(volume) - 0.23·cyclomatic - 16.2·ln(lines)`, rescaled), higher is better. The one other floor besides coverage. |

Every budget is a **maximum, exceeded strictly**: a member at exactly 22 is inside a budget of 22.
Test coverage and maintainability index are the two minimums — a member (or the whole run, for
coverage) *below* the floor is over budget. Budgets resolve **command line > `dotnet-fast.json` >
default**, and each has a `--max-…` / `--min-…` flag of the same name for a one-off override.

The last four rows were appended after the original ten (never renumbered) once this scoreboard's
early adopters asked for the figures the CST pass already computed but never surfaced. Every default
is lifted from a threshold this repo already ships elsewhere — `linesPerMember` from the `DF9002`
guardrail, `nestingDepth`/`parameterCount` from the native `S134`/`S107` lint ports, and
`maintainabilityIndex`'s 20 from the Visual Studio "green" floor — not invented for this table.

## Where the reports come from

`metrics` reads files, so point it at the ones your test step already wrote. Either a file or a
directory to search recursively; `--coverage` and `--mutation` are both repeatable, so a solution with
several test projects can pass one of each per project and have them merged.

| Producer | File it writes | Pass it with |
|---|---|---|
| `dotnet test --collect:"XPlat Code Coverage"` (coverlet) | `TestResults/<guid>/coverage.cobertura.xml` | `--coverage TestResults` |
| coverlet with `-p:CoverletOutputFormat=opencover` | `coverage.opencover.xml` | `--coverage coverage.opencover.xml` |
| coverlet with `-p:CoverletOutputFormat=lcov` | `lcov.info` | `--coverage lcov.info` |
| coverlet with `-p:CoverletOutputFormat=json` | `coverage.json` | `--coverage coverage.json` |
| `dotnet stryker` | `StrykerOutput/<run>/reports/mutation-report.json` | `--mutation StrykerOutput` |

The coverage format is worked out from the file itself. `--coverage-format
auto|cobertura|opencover|lcov|coverlet-json` pins it when you would rather be explicit than let the
sniffer decide — useful if a generator writes an unusual extension.

A report that scores nothing — an empty coverage document, a mutation run that produced no results —
is a **hard error**, not a `0` that reads as a pass. Files `metrics` could not read, and files whose
source does not parse, are counted and reported rather than skipped silently
(`summary.readFailures`, `summary.parseFailures` and `summary.filesWithSyntaxErrors` in the JSON).

## Languages: C# and F#

`metrics` scores **C# and F#**, each project in its own language. A mixed solution gets one
scoreboard covering all of it — the same fourteen rows, the same budgets, one exit code — rather than
a board that quietly describes only its C# half.

```
Metric                 Worst  Budget  Status
…
15 files, 62 members, 4 projects in 0.2s
Languages: C# 11 files, 48 members; F# 4 files, 14 members
```

The `Languages:` line appears only when more than one language contributed. `--format json` always
carries the breakdown as `summary.languages`, one entry per language walked:

```json
"languages": [
  { "language": "csharp", "projects": 3, "files": 11, "members": 48,
    "parseFailures": 0, "readFailures": 0, "filesWithSyntaxErrors": 0 },
  { "language": "fsharp", "projects": 1, "files": 4, "members": 14,
    "parseFailures": 1, "readFailures": 0, "filesWithSyntaxErrors": 0 }
]
```

**What counts as a "member" in F#.** The same set of things C# measures, spelled F#'s way: a `let`
that declares parameters (module-level, inside a type, or local), a `let` bound to a `fun`/`function`
lambda, a `member` with an argument list, each `get`/`set` accessor, and an additional `new (…)`
constructor. A `let` bound to a **value** (`let maxRetries = 3`) is not a member — it would otherwise
add a stream of complexity-1, nesting-0 "members" that make a module of lookup tables read as a
codebase full of trivially healthy code. A property with no argument list, a `member val`
auto-property and an `abstract member` signature are likewise not measured, matching what the C# pass
does with their C# equivalents.

**Dynamic typing in F#.** F# has no `dynamic` keyword, so this row counts F#'s own ways out of the
type system: `obj` (or `System.Object`) in a **type** position, `box`/`unbox`, and the `?` /
`?<-` dynamic-lookup operators. A `0` therefore means what it means for C# — we looked, and found
none — rather than "this language was not checked". A *value* named `obj` is not counted, and neither
is `:?>`/`downcast` or a `:?` type-test pattern: those name a concrete target type, so the check moves
to runtime rather than being erased, which makes them casts rather than `dynamic`. If your codebase
overrides `Equals(obj)`, raise `metrics.maxDynamicUses` — the row deliberately has no built-in
exemptions.

**F# files the parser cannot read are reported, not dropped.** F#'s offside rule is not context-free
and the grammar approximates it, so a small share of real files cannot be parsed. Those land in
`summary.parseFailures` (and in that language's entry above), and the text board says
`N file(s) could not be parsed and were excluded from every figure.` They are never silently measured
as empty.

**VB.NET is not scored.** Nothing here parses `.vb`. A `.vbproj` in the solution is reported rather
than quietly counted as clean: a warning on stderr and an entry in `summary.skippedProjects`
(`{ "project", "reason": "language-not-supported", "message" }`). It does not fail the command.

## Gating CI

```bash
dotnet-fast metrics App.sln --coverage TestResults --mutation StrykerOutput --fail-on-budget
```

Exit `1` means a **measured** metric is over budget. Unmeasured rows never fail the build, so adding
the gate before you have a mutation report is safe.

### Adopting budgets on a codebase that is already over them

Turning `--fail-on-budget` on across an existing repository fails on day one, which usually means it
gets turned off again. The ratchet exists for that: record where you stand, then fail only on things
that get *worse*.

```bash
# Once, on your main branch — commit the file.
dotnet-fast metrics App.sln --coverage TestResults --write-baseline metrics-baseline.json

# In CI, from then on.
dotnet-fast metrics App.sln --coverage TestResults \
  --baseline metrics-baseline.json --fail-on-budget
```

A row that is over budget but no worse than the baseline reports `over budget (no worse than
baseline)` and does **not** fail the build. A row that regressed reports `regressed (+N)` and does.
Refresh the baseline deliberately — as a reviewed commit — whenever you want to lock in an
improvement. Without `--baseline`, behaviour is exactly as before.

Two things the ratchet will not do. A metric the baseline never measured — coverage, if the recording
run had no `--coverage` — is **not** exempt the first time you do measure it: it is judged against its
budget like any other new row. And because the recorded counts are counts of items over *the budgets
of that run*, changing a budget afterwards prints `baseline was written with different budgets;
comparisons are budget-relative` so the comparison is not read as more than it is.

### GitHub code scanning

```bash
dotnet-fast metrics App.sln --format sarif > metrics.sarif
```

Over-budget members and files become SARIF results with a file and line, so they land in the Security
tab alongside your lint findings. See [code-scanning.md](code-scanning.md) for the upload step.

### Machine-readable output

`--format json` gives `{ metrics: [...], worstMembers: [...], summary: {...} }`, with the full
Halstead figures per member and an `offenders` array on each metric row (sized by `--top`) naming the
worst members or files by name, line and value.

`--top all` (or `--top unlimited`) lists **every** offender behind an over-budget row instead of the
default 5 — `offenders.length` then equals the row's `overCount` exactly. This is the one expensive
flag here, by orders of magnitude on a large solution: expect 40k–150k tokens rather than the usual
~1.3k. `worstMembers` does not follow it past its own fixed cap of 5, so it stays a small diagnostic
sample regardless of `--top`.

Approximate costs, so you can budget for them:

| Output | Size |
|---|---|
| text (default) | ~1.3k tokens |
| agent text (`--agent`, or auto-detected in an AI shell) | ~0.9k tokens |
| `--format json --top 0` (the agent recipe — rows and summary, no offender lists) | ~1.2k tokens |
| `--by-project` block | ~1 line per project |
| `--top all` | 40k–150k tokens |

### `--by-project`: attribute the board to a solution's projects

Opt-in — nothing an existing agent or CI job reads today moves until you pass it:

```bash
dotnet-fast metrics App.sln --by-project
```

Text gets one appended line per project, worst rows first, `— ok` when a project is clean. JSON gets
a top-level `projects` array, present only under the flag: `[{ "name": "App.Core", "metrics": [{ "id",
"value", "overBudget", "overCount" }] }]` — no offenders, since an offender already names its file.
Every per-project figure comes from the *same* row builders the whole board uses, so it can never
disagree with `--project App.Core` run alone. Coverage, surviving mutants and dead code are whole-run
figures and are **not** repeated on every project line — they read as not measured there, the same way
an absent report reads on the whole board.

### Agent-mode output

In an AI shell (or with `--agent`), the text report opens with one line — `metrics: 5 of 14 over
budget, 3 not measured` — lists only the failing rows, then one line naming everything that was never
measured. Offenders are capped at 3 per row regardless of `--top`. `--human` forces the full board
even in an AI shell; `--format json` bypasses agent mode entirely either way.

## Budgets in `dotnet-fast.json`

Set them once at the repository root instead of repeating flags in every pipeline:

```json
{
  "metrics": {
    "maxCyclomatic": 15,
    "maxCognitive": 15,
    "maxHalsteadDifficulty": 60,
    "maxLinesPerFile": 400,
    "minCoveragePercent": 80,
    "maxCrap": 30,
    "maxSurvivingMutants": 0,
    "maxDeadCode": 0,
    "maxDuplicateBodies": 0,
    "maxDynamicUses": 0,
    "maxLinesPerMember": 40,
    "maxNestingDepth": 3,
    "maxParameters": 6,
    "minMaintainabilityIndex": 30
  }
}
```

A command-line flag still wins for a single run.

## Relationship to the `DF9005` / `DF9006` guardrails

`metrics` is a **scoreboard**: where the whole codebase stands today, including the numbers that need
your test reports. The [guardrails](guardrails.md) are a **gate**: `DF9005` (cyclomatic complexity)
and `DF9006` (Halstead difficulty) fire on the individual member as it is written, in the same `lint`
run everything else goes through.

They share their scorers — the complexity code behind `DF9005`/`DF9006` *is* the code behind the
`metrics` rows — so the gate and the scoreboard can never disagree about a member. The usual pairing
is `metrics` for the direction of travel and the guardrails to stop new violations landing.

## Repository identity

`--format json` always carries two fields in `summary`: `schemaVersion` (the JSON shape's own
version) and `repository` (this run's name). `repository` is inferred — the git `origin` remote's
repo name, then the directory name — or set explicitly:

```bash
dotnet-fast metrics App.sln --format json --repository-name my-service
```

Set `--repository-name` in CI so a later estate rollup (below) can tell repositories apart.

## Estate rollup: `metrics --merge`

If you run `metrics` across many repositories, `--merge` combines their `--format json` reports into
one view instead of measuring a codebase itself. It runs offline: no server, nothing to stand up —
every repository's own CI already produces the report `--merge` reads.

```bash
# each repository's CI, unchanged except naming itself:
dotnet-fast metrics . --format json --repository-name my-service > metrics.json
# … CI publishes metrics.json as a build artifact …

# a central job, after downloading every repository's artifact into artifacts/<repo>/metrics.json:
dotnet-fast metrics --merge "artifacts/*/metrics.json" --format text
dotnet-fast metrics --merge "artifacts/*/metrics.json" --format json
dotnet-fast metrics --merge "artifacts/*/metrics.json" --format html > estate.html
```

```
Estate: 3 repositories, 0 skipped

payments-api  4/10 over budget
  cyclomaticComplexity=41! cognitiveComplexity=38! halsteadDifficulty=92.1! linesPerFile=913!
  redundantCode=4! dynamicTyping=0 testCoverage=71.4! crap=96.0! survivingMutants=2!
checkout-web  1/9 over budget
  cyclomaticComplexity=18 cognitiveComplexity=15 halsteadDifficulty=54.0 linesPerFile=612!
  redundantCode=0 dynamicTyping=0
inventory-svc  0/7 over budget
  cyclomaticComplexity=12 cognitiveComplexity=9 halsteadDifficulty=41.2 linesPerFile=280
  redundantCode=0 dynamicTyping=0

Budgets breached across the estate:
  Cyclomatic complexity: 1/3 repositories over budget
  Cognitive complexity: 1/3 repositories over budget
  Halstead difficulty: 1/3 repositories over budget
  Lines per file: 2/3 repositories over budget
  Redundant code: 1/3 repositories over budget
  Dynamic typing: 0/3 repositories over budget
  Test coverage: 1/1 repositories over budget
  CRAP: 1/1 repositories over budget
  Surviving mutants: 1/1 repositories over budget
```

`!` marks a repository over its own budget for that column. `--merge` accepts a glob (`*`, `**`,
`?`), repeatable, and takes priority over every measurement flag — nothing else on the command line
matters once `--merge` is given. `--fail-on-budget` still exits `1` when any repository is over any
budget. `--format sarif` is not available with `--merge` (an estate row has nowhere to point a
physical location); use `text`, `json`, or `html`.

**Which columns appear is decided by the data, never hardcoded.** Cyclomatic, cognitive, Halstead,
lines per file, redundant code and dynamic typing come from source alone, so almost every repository
carries them. Test coverage, CRAP and surviving mutants need a test or mutation run behind
`--coverage`/`--mutation`, so **a column for one of those three appears only when at least one
repository in the merge actually measured it** — and inside that column, a repository that did not
measure it simply has no cell there, not a "-" one. `inventory-svc` above never ran `--mutation`, so
it has no `survivingMutants` value; that is not the same as passing, and the estate view is built
specifically not to let it read that way. A repository that later wires up coverage starts showing
that column automatically — nothing to configure. Dead code stays behind `--include-dead-code` in
every underlying run: it is the one expensive pass, so an estate reflects only the repositories that
chose to pay for it.

A file `--merge` cannot use — one predating this feature (no `schemaVersion`), one from a newer
`dotnet-fast` this build does not understand, or anything else unreadable — is **never silently
merged**. It is named with its reason, on stderr and in the output's `skipped` list, and the rest of
the estate still renders.

## Machine-readable estate output

`--merge --format json` gives:

```json
{
  "schemaVersion": 1,
  "repositories": [
    {
      "repository": "payments-api",
      "overCount": 4,
      "measuredCount": 10,
      "budgets": {
        "cyclomaticComplexity": { "value": 41.0, "overBudget": true }
      }
    }
  ],
  "budgets": {
    "cyclomaticComplexity": { "label": "Cyclomatic complexity", "repositoriesMeasured": 3, "repositoriesOverBudget": 1 }
  },
  "skipped": [{ "file": "artifacts/old-service/metrics.json", "reason": "missing summary.schemaVersion — this report predates the estate-merge feature; re-run `metrics --format json` to regenerate it" }],
  "summary": { "repositories": 3, "skipped": 1, "overBudget": true }
}
```

A repository's `budgets` object carries only the ids it measured — same no-placeholder rule as the
text view.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

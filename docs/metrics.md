# Code-health metrics

`dotnet-fast metrics` scores a codebase against ten budgets in one build-free pass and prints a single
scoreboard. Seven of the ten come straight from your source; the other three need your tests to have
run, so `metrics` **reads the reports your pipeline already produces** — it never runs `dotnet test`,
Stryker, or anything else itself.

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
```

**A metric you did not measure never reads as passing.** An unmeasured row says *not measured*, names
the flag that would fill it, and is ignored by `--fail-on-budget`. A board that is green because
nothing was checked would be worse than no board.

## The ten metrics

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
| Dynamic typing | 0 | `maxDynamicUses` | `dynamic` in a type position — C#'s nearest equivalent to TypeScript's `any`. |

Every budget is a **maximum, exceeded strictly**: a member at exactly 22 is inside a budget of 22.
Coverage is the one minimum. Budgets resolve **command line > `dotnet-fast.json` > default**, and each
has a `--max-…` / `--min-…` flag of the same name for a one-off override.

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
C# does not parse, are counted and reported rather than skipped silently (`summary.readFailures` and
`summary.filesWithSyntaxErrors` in the JSON).

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
    "maxDynamicUses": 0
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

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

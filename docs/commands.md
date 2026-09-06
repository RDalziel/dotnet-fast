# Command reference

`dotnet-fast` is one command with a handful of subcommands. Each finds your projects from a solution, a
project file, or a directory, and starts instantly. Add `--json` to any command for machine-readable
output (it includes the run's timing).

```
dotnet-fast <command> [target] [options]
```

`target` is optional — with nothing given, the tool finds the nearest `.sln`/`.csproj` or scans the
current directory.

## Contents

**Lint & format** — [`lint`](#lint) · [`format`](#format) · [`editorconfig`](#editorconfig)

**Code health** — [`metrics`](#metrics) · [`dead-code`](#dead-code) ·
[`dead-dependencies`](#dead-dependencies) · [`bom`](#bom) · [`doctor`](#doctor)

**CI** — [`affected`](#affected) · [`build`](#build) · [`test-plan`](#test-plan) ·
[`cache`](#cache) · [`insights`](#insights)

**Everything else** — [`hooks`](#hooks) · [`update`](#update) ·
[Version banner](#version-banner) · [Global options](#global-options) · [Exit codes](#exit-codes)

## Version banner

Every command writes one identity line to **stderr** before it does anything else, so a CI log always
records which build produced its output:

```
dotnet-fast 0.304.0
dotnet-fast 0.304.0 (matches pin in /src/repo/.config/dotnet-tools.json)
dotnet-fast 0.297.3 (manifest pins 0.304.0 at /src/repo/.config/dotnet-tools.json)
```

The last form means the running version differs from the one your `.config/dotnet-tools.json` pins —
usually a feed policy or version cascade substituting something else at install time. The running
version always leads the line, so `head -1` of stderr answers "what actually ran?".

It goes to stderr, never stdout, so `--json`, `--format count|json|matrix|traversal`, and SARIF output
are unaffected. Turn it off with `DOTNET_FAST_NO_BANNER=1` (also off under `-v quiet`, in agent mode,
and in the git hooks this tool installs).

---

## `lint`

Report formatting and lint findings. This is the fast CI gate.

```bash
dotnet-fast lint App.sln              # report everything, non-zero exit if any finding
dotnet-fast lint --fix App.sln        # apply every safe fix in one pass, then report what's left
dotnet-fast lint --deep App.sln       # also run the project's real Roslyn analyzers (see deep-linting.md)
```

The default path is **syntactic-only** — it reads your source as a syntax tree with no type
information or dataflow, which is what makes it start in milliseconds with no SDK or restore.
Rules that need to know what a symbol *is* (for example unused-`using` removal, IDE0005) are out
of its reach by design; `--deep` opts into the project's real Roslyn analyzers for those.

Useful options:

| Option | Effect |
|---|---|
| `--fix` | Apply the safe fixes (whitespace, style, simple lint rules) — changes only what it reports. |
| `--deep` | Opt in to the project's real Roslyn analyzers — the semantic rules the syntactic default can't run. Needs the .NET SDK + a restored project; degrades to the fast path when unavailable. |
| `--deep-cache` | Cache `--deep` diagnostics per project in the build cache so unchanged projects skip the re-bind (scales with change). Needs a configured build cache; opt-in. |
| `--staged` / `--affected` / `--ci` / `--pr-base <branch>` | Scope to changed files — the index, the branch, or the CI range. |
| `--all` (alias `--force-all`) | Accepted as a **no-op** (also on `build`/`test-plan`): these commands already process the whole target, so `--all` just means "no range scoping" and is ignored. Lets a pipeline share one range string with `affected --all` without stripping the flag. |
| `--whole-file` / `--changed-lines` | Opt out of / into changed-line scoping (see below). |
| `--severity <level>` | Only report at/above a level (`info`/`warning`/`error`). |
| `--exclude-diagnostics <ID...>` | Suppress specific rule IDs (e.g. `DF0001`). |
| `--baseline <file>` | Compare against a saved baseline and fail only on *new* findings. |
| `--json` / `--report <file>` | Emit results as JSON / write a JSON report. |
| `--sarif <file>` | Also write SARIF 2.1.0 for GitHub code scanning — see [code-scanning.md](code-scanning.md). |
| `--list-rules` | List the rule catalog and exit — grouped by category, with each rule's id, description and whether `--fix` can fix it. Narrow it with `--category <NAME>` or `--fixable`; add `--json` for tooling. |
| `--explain <ID>` | Print the full description of one rule (e.g. `--explain DF9002`) and exit. |
| `--only-active-analyzers` | Report only the analyzer diagnostics your `.editorconfig` has actually enabled, instead of the full bundled superset. |
| `--record-findings` | Append this run's finding counts to the configured timings store, so [`insights`](#insights) can show the trend, the top rules and the fix-adoption rate. Off by default; records counts only, never your code. |

The complete native rule catalog, with each rule's `--fix` status and the in-source suppression comments, is on
[rules.md](rules.md).

Findings honor your `.editorconfig` severity settings, just like a real build — see
[editorconfig.md](editorconfig.md) for the full severity vocabulary and precedence rules.

**Changed-line scoping.** On a pull request (`--ci` PR, `--pr-base`, `--base`) and for `--staged`, the
report covers only the lines you actually changed — pre-existing findings on untouched lines of a file
you happen to touch are suppressed, so a branch that lags `main` is not failed by old debt. Pass
`--whole-file` to report a touched file's full backlog, or `--changed-lines` to force the same scoping
on an explicit `--from`/`--to` or `--ci` push build. Scoping affects reported findings only; `--fix`
still rewrites whole files.

### Guardrails (opt-in): keeping AI-written code reviewable

Seven optional rules for repos where an agent writes the code and a human reviews it: no explanatory
comments, a line budget per method and per file, no magic numbers, and budgets on cyclomatic
complexity, Halstead difficulty and cognitive complexity. An agent will happily ignore a style guide,
but it cannot ignore a lint error in CI. All seven are **off by default** and report-only — nothing
changes for your repo until you switch one on:

```ini
# .editorconfig
[*.cs]
dotnet_diagnostic.DF9001.severity = warning   # no explanatory comments
dotnet_diagnostic.DF9002.severity = warning   # method length
dotnet_diagnostic.DF9003.severity = warning   # file length
dotnet_diagnostic.DF9004.severity = warning   # magic numbers
dotnet_diagnostic.DF9005.severity = warning   # cyclomatic complexity
dotnet_diagnostic.DF9006.severity = warning   # Halstead difficulty
dotnet_diagnostic.DF9007.severity = warning   # cognitive complexity

# Thresholds, shown with their defaults — omit to keep them.
dotnet_fast_max_lines_per_function = 50
dotnet_fast_max_lines_per_file = 250
dotnet_fast_magic_number_allowed = 0,1,-1,2
dotnet_fast_max_cyclomatic_complexity = 22
dotnet_fast_max_halstead_difficulty = 80
dotnet_fast_max_cognitive_complexity = 22
```

Or skip the hand-editing: `dotnet-fast editorconfig recommend --guardrails --write` appends all seven
at once, with every threshold stated explicitly at the published code-health budget number (including
`dotnet_fast_max_lines_per_file = 500`, the published SLOC budget — this rule's own bare default,
kept for backward compatibility, is 250).

| Rule | Reports | Threshold key |
|---|---|---|
| `DF9001` | a `//` or `/* … */` comment (`///` XML docs are exempt) | — |
| `DF9002` | a member over 50 lines, blank lines excluded | `dotnet_fast_max_lines_per_function` |
| `DF9003` | a file over 250 non-blank lines | `dotnet_fast_max_lines_per_file` |
| `DF9004` | a numeric literal with no name | `dotnet_fast_magic_number_allowed` |
| `DF9005` | a member over 22 independent paths | `dotnet_fast_max_cyclomatic_complexity` |
| `DF9006` | a member over 80 Halstead difficulty | `dotnet_fast_max_halstead_difficulty` |
| `DF9007` | a member over 22 cognitive complexity (nesting-weighted) | `dotnet_fast_max_cognitive_complexity` |

Only a per-rule `dotnet_diagnostic.DF900x.severity` enables one — see
[editorconfig.md](editorconfig.md#precedence-which-severity-wins) for why a bulk key can't. Run
`dotnet-fast lint --explain DF9002` for any rule's full description.

Two more of the ten published code-health budgets — redundant code and dynamic typing — are covered
by the always-on ported analyzers `S4144` and `PH2044` instead of a dedicated guardrail; see
[guardrails.md](guardrails.md#redundant-code-and-dynamic-typing-are-covered-elsewhere) for why.

**[guardrails.md](guardrails.md)** is the full page: what each rule reports and why, how to scope them
to the projects where they earn their keep (test projects legitimately carry magic numbers and long
fixtures), how to adopt them on an existing codebase without a wall of findings, and why the messages
are written for the agent that has to act on them.

The guardrails stop new violations landing. To see where a codebase stands overall — including
coverage, CRAP and surviving mutants — use **[`metrics`](#metrics)**, which shares its complexity
scorers with `DF9005`/`DF9006`/`DF9007` so the two can never disagree.

---

## `format`

Apply whitespace and style formatting. This is the `dotnet format`-compatible path — it edits files in
place and preserves everything it isn't explicitly changing.

```bash
dotnet-fast format App.sln            # format in place
dotnet-fast format --check App.sln    # report files that would change, change nothing (CI check)
```

---

## `affected`

List the projects a Git change set touches — plus every project that depends on them. Point your CI at
this to build and test only what a change can actually break.

```bash
dotnet-fast affected --ci             # compare against the CI base branch
dotnet-fast affected --base main      # compare against an explicit ref
dotnet-fast affected --pr-base        # compare against the pull-request base
```

The output is the list of affected project paths; `--json` gives you the structured set for a build
matrix. On shallow CI checkouts, explicit `--from <REV>` / `--to <REV>` ranges deepen and retry when a
named commit is missing, while keeping direct `from..to` comparison semantics.

For GitHub Actions push builds, `--ci` uses the event `before` SHA when it is available, so
multi-commit pushes are included. Providers that do not expose that SHA still compare push builds to
the parent commit. On Azure Pipelines batched/coalesced branch builds, use
`--ci-base last-successful-build` to query the previous completed+succeeded build for this
definition+branch and use its `triggerInfo['ci.sourceSha']` as the base — this needs
`SYSTEM_ACCESSTOKEN` in the step environment
(see [Azure DevOps](azure-devops.md#batched-trigger-builds---ci-base-last-successful-build)). When that
lookup cannot complete (missing/invalid token, no prior successful build), `--ci-base-fallback` decides
the behavior: `previous-commit` (default) narrows to the parent commit — which **silently under-builds a
batched trigger** — while `--ci-base-fallback all` conservatively builds every project and
`--ci-base-fallback error` fails loudly. This is separate from `--on-missing-base`, which governs an
unresolvable explicit `--from`/`--to`/`--base`.

`--ci-base last-completed-build` is the same lookup but also accepts a `partiallySucceeded` build.
Use it when the pipeline has a `continueOnError: true` step: such a step failing prevents the build
from ever reaching `succeeded`, so the baseline never advances and the comparison window grows until
it covers the whole solution
(see [when every project comes back affected](azure-devops.md#if-every-project-comes-back-affected)).

**When a pipeline step dirties a tracked file.** Some pipelines rewrite a checked-in file before the
build — swapping in a `NuGet.Config` or `.npmrc` that points at an internal feed is the common one.
That is a real change to a real shared config, so `affected` correctly fans out to every project.
`--exclude` does not help: it filters *projects* out of the result, never files out of the diff.

```bash
dotnet-fast affected --ci --ignore-changed NuGet.Config --ignore-changed .npmrc
```

`--ignore-changed <PATH>` drops matching paths from the changed set before the graph walk. Every
ignored path is printed, and a pattern that matches nothing warns — dropping a change is the one
direction that can **under-build**, so it never happens quietly.

Prefer fixing the pipeline where you can: write the file outside the repository, or
`git update-index --skip-worktree` it, so the tree stays clean and nothing needs ignoring.

**Azure DevOps output variables.** Instead of parsing `--format count` and hand-echoing a logging
command, emit the pipeline variable natively:

| Option | Effect |
|---|---|
| `--set-variable <NAME>` | Emit `##vso[task.setvariable variable=<NAME>]true\|false` — `true` when any project is affected. Use it in a later `condition: eq(variables.<NAME>, 'true')`. |
| `--set-count-variable <NAME>` | Emit `##vso[task.setvariable variable=<NAME>;isOutput=true]<count>` — the affected-project count as a cross-stage output variable. |
| `--exit-zero-on-empty` | Exit `0` instead of `166` when nothing is affected (outputs and variables are still emitted). |

Both variable flags compose with any `--format` and never change the exit code — pair with
`--exit-zero-on-empty` so the step also succeeds on an empty set. See
[Azure DevOps](azure-devops.md) for a full pipeline.

---

## `build`

Preview remote build cache for CI. Cached projects are restored from storage; misses are built normally
and can be uploaded from trusted builds.

```bash
dotnet-fast build --plan .             # inspect cache keys and cacheability
dotnet-fast build --plan --check .     # 0 all cached, 3 incomplete, 4 unavailable
dotnet-fast build --projects-file affected.proj .
dotnet-fast build .                    # restore hits, build/upload misses
dotnet-fast build --read-only .        # PR-safe: never upload
dotnet-fast build --report artifacts . # write artifacts/build-report.json
```

Useful options:

| Option | Effect |
|---|---|
| `--plan` | Compute keys and probe cache availability without building. |
| `--check` | With `--plan`, return a script-friendly availability exit code. |
| `--read-only` | Restore hits and build misses, but never upload. |
| `--no-cache` | Bypass the cache and build normally. |
| `--assume-restored` | Assert the tree is already restored (the pipeline ran `dotnet restore` first): passes `--no-restore` and compiles each topological layer's projects concurrently instead of one at a time. |
| `--build-jobs <N>` | With `--assume-restored`, cap concurrent project compiles (default: one per logical core). |
| `--configuration <name>` | Select `Debug`, `Release`, or another configuration. |
| `--json` | Emit a machine-readable build/cache report, including actions, paths, bytes, and timings. |
| `--report <DIR>` | Write the build/cache JSON report to `<DIR>/build-report.json`. |
| `--project <PROJECT>` / `--projects-file <FILE>` | Build or plan only the selected project set. |
| `--ci` / `--from <REV>` / `--to <REV>` / `--merge-base` | Scope to affected projects; explicit selectors are intersected. |
| `--format matrix\|ado-matrix` (+ `--auto-shards`/`--shards N`) | Emit a layered build job-matrix to shard the build across agents (topological waves). |
| `--layer L --shard I --of N` | Build one wave's shard; its dependencies (earlier waves) restore from cache. |
| `--timings <FILE>` / `--use-cached-timings` | Balance shard assignment by prior build duration instead of raw project count (a local timings file, or the merged history stored in the build cache). |
| `--record-timings` | After a real build, save this run's per-project durations so future runs can `--use-cached-timings`. |
| `--timings-scope <KEY>` | Scope key for the cached timings blob (defaults to the current Git branch). |

With `buildCache.timingsTable` / `DOTNET_FAST_TIMINGS_TABLE` set, the cached timing history lives in
repository-partitioned Azure Table entities on the cache account instead of the per-scope blob —
same flags, safe for concurrent shard recording; see
[test-sharding.md](test-sharding.md#azure-table-storage-timings-multi-repo-race-free).

With no timing data at all, shards fall back to today's count-balanced round-robin — these flags are
purely additive.

See [build-cache.md](build-cache.md) for setup, CI examples, and the build-sharding contract.

---

## `test-plan`

NUnit test sharding for CI agents. Tests are discovered from source and partitioned before test
assemblies are built.

```bash
dotnet-fast test-plan --shards 8 --format matrix .
dotnet-fast test-plan --shards 8 --format ado-matrix .
dotnet-fast test-plan --ci --auto-shards --min-per-shard 50 --max-shards 8 --format matrix .
dotnet-fast test-plan --shards 8 --format json .
dotnet-fast test-plan --shard 3 --of 8 .
dotnet-fast test-plan --shard 3 --of 8 --exec --test-args "--no-build --no-restore -c Release" --results-dir ./trx
dotnet-fast test-plan --ci --shards 8 --format json .
dotnet-fast test-plan --projects-file affected-tests.dotnet-test.txt --auto-shards --format matrix .
dotnet-fast test-plan --cache-misses-file build-cache-plan.json --auto-shards --format matrix .
dotnet-fast test-plan --shards 8 --verify .
```

Useful options:

| Option | Effect |
|---|---|
| `--shards <N>` | Total shard count for matrix/json output. |
| `--auto-shards --min-per-shard <W> --max-shards <N>` | Pick matrix size from discovered fixture weight. |
| `--shard <I> --of <N>` | Print only one agent's commands. |
| `--format commands|matrix|ado-matrix|json` | Select command lines, GitHub matrix, Azure matrix, or full structured plan. |
| `--project <PROJECT>` / `--projects-file <FILE>` | Shard only the listed test projects. |
| `--cache-misses-file <FILE>` | Read build-cache JSON and shard only tests impacted by cache misses/non-restored projects. |
| `--settings-dir <DIR>` | Write long filters to `.runsettings` files instead of inline command arguments. |
| `--exec` | Run the selected `dotnet test` commands directly. |
| `--restore-from-cache` (`--exec`) | Restore only this shard's project closure from the build cache before running (scopes the per-agent download); `--fallback` builds the closure if the cache is down. Keep `NUGET_PACKAGES` aligned (or unset) between the agent that populates the cache and the agents that consume it — see the warning below. |
| `--cache-wait <SECONDS>` (`--restore-from-cache`) | Wait up to this long for an in-progress cache upload before treating a project as a miss — for shards running in parallel with the build that populates the cache. Default `0` (no wait). |
| `--cache-wait-share-misses` (`--cache-wait`) | Record a give-up so other shards can skip the same wait. Needs a cache credential with write access; off by default. |
| `--assume-restored` / `--build-jobs <N>` (`--restore-from-cache`) | Assert the tree is already restored, so the shard's cache-miss closure compiles with `--no-restore` and one topological layer at a time in parallel. Same semantics as on `build`; off by default. |
| `--test-jobs <N>` (`--exec`) | Run a shard's `dotnet test` invocations concurrently (default `1`). Overlaps one container's startup with another's execution. With `--record-timings`, per-project startup estimates are skipped for that run. |
| `--min-per-shard-ms <MS>` (`--auto-shards`) | Size shards by time when cached timings are in play, instead of `--min-per-shard`'s fixture-count units. |
| `--test-args <ARGS>` | Append pass-through arguments to each `dotnet test`. |
| `--filter-and <FILTER>` | AND an extra VSTest filter with the generated fixture filter. |
| `--results-dir <DIR>` / `--collect <COLLECTOR>` | Emit TRX results and optional coverage collection. |
| `--allow-empty-test-results` (`--exec` + `--results-dir`) | Do not fail when a `dotnet test` exits `0` without writing its `.trx`. Since v0.306.0 that is an error, because it can only mean no test host started; a `.trx` present with **zero** tests is the legitimate empty-filter case and still passes. Off by default. |
| `--verify` | Run `dotnet test` and confirm the shard union matches the baseline list. |
| `--ci` / range flags | Shard only affected test projects. |
| `--exit-zero-on-empty` | Exit `0` instead of `166` when the affected range leaves no test project to shard (the empty plan is still emitted). Mirrors `affected --exit-zero-on-empty`. |
| `--timings <FILE>` / `--use-cached-timings` | Balance shards by prior-run fixture duration instead of static test count (a local `{ "Ns.Fixture": ms }` file, or the merged history stored in the build cache). |
| `--record-timings` (`--exec` + `--results-dir`) | After running a shard, roll its TRX durations into the cached history so future plans can `--use-cached-timings`. |
| `--timings-scope <KEY>` | Scope key for the cached timing history (defaults to the current Git branch). |

With `buildCache.timingsTable` / `DOTNET_FAST_TIMINGS_TABLE` set, the cached timing history lives in
repository-partitioned Azure Table entities on the cache account instead of the per-scope blob —
same flags, race-free when parallel shards record, with default-branch fallback for cold PR
branches; see
[test-sharding.md](test-sharding.md#azure-table-storage-timings-multi-repo-race-free).

> **A cache-restored shard can only test what its packages folder points at.** The build cache
> restores `obj/` — including the generated `*.nuget.g.props` — but that file only holds a *pointer*
> to the global NuGet packages folder, which is agent state the cache never ships. If `NUGET_PACKAGES`
> was set when the cache entry was produced, its literal absolute path is baked in; on an agent
> without that directory MSBuild silently skips the `Microsoft.NET.Test.Sdk` import, `IsTestProject`
> is never set, and `dotnet test --no-build --no-restore` exits `0` having run nothing.
>
> Since **v0.306.0** `test-plan --exec --restore-from-cache` pre-flights the restored closure
> whenever `--test-args` suppresses restore, and **refuses to run** (exit `1`) rather than report a
> green shard that tested nothing. Unsetting `NUGET_PACKAGES` everywhere is the durable fix: NuGet
> then bakes the portable `$(UserProfile)\.nuget\packages\` form, which resolves on any agent.
> Details in [test-sharding.md](test-sharding.md).

See [test-sharding.md](test-sharding.md) for CI examples and NUnit details.

---

## `dead-code`

Find unused code across a whole solution — types and members nothing in production reaches, plus a
distinct **test-only** category for code kept alive only by tests. Build-free and conservative by
contract: everything reported is safe to delete; any ambiguity (unresolvable name, reflection, parse
failure) keeps the symbol alive instead of risking a false positive.

```bash
dotnet-fast dead-code .                       # human report (report-only, exits 0)
dotnet-fast dead-code . --format json         # { findings: [...], summary: {...} }
dotnet-fast dead-code . --fail-on-dead        # exit 1 when dead code is found (CI gate)
dotnet-fast dead-code . --include-public      # closed-world: analyse public API too
dotnet-fast dead-code . --fix                 # DRY-RUN: preview the removal diff, write nothing
dotnet-fast dead-code . --fix --write         # apply the removals
```

| Option | Effect |
|---|---|
| `--fail-on-dead` | Exit `1` on findings — without it the command is report-only and exits `0`. |
| `--include-public` | Treat public API as analyzable too (only correct when nothing outside the solution consumes it). |
| `--no-test-only` | Report only fully-dead code, hiding the test-only category. |
| `--handler-pattern` | Teach it an in-house dispatch pattern (e.g. `"ICommandHandler<1,0>"`) so indirectly-dispatched handlers count as reachable. MediatR/MassTransit-style handlers are understood out of the box. |
| `--fix` / `--fix --write` | Remove auto-removable findings — dry-run diff by default, `--write` applies. Test-only code, partial types, whole dead projects, multi-name fields, and a dead type still referenced by a surviving partial part are never auto-removed (JSON findings carry a `"removable"` flag). One `--fix --write` pass always reaches the fixed point. |

By default the analysis is internal-only (public/protected types are live API roots), so a fresh run
on any repo reports internal/private dead code with near-zero false positives and no configuration.

See [dead-code.md](dead-code.md) for the full DC-id catalog, the conservative-marking rules,
framework indirection (`--handler-pattern`), and CI patterns.

---

## `metrics`

Score a codebase against fourteen code-health budgets in one build-free pass. Eleven come straight
from your source; the other three are read from the coverage and mutation reports your pipeline
already produces. **[metrics.md](metrics.md)** is the full guide — all fourteen metrics, where each
report file lands, `--by-project`, the CI gate and the baseline ratchet. This is the option reference.

```bash
dotnet-fast metrics App.sln                          # the scoreboard (report-only, exits 0)
dotnet-fast metrics App.sln --fail-on-budget         # exit 1 when a measured budget is exceeded
dotnet-fast metrics App.sln --coverage TestResults   # adds coverage and CRAP
dotnet-fast metrics App.sln --mutation StrykerOutput # adds surviving mutants
dotnet-fast metrics App.sln --include-dead-code      # adds the dead-code count
dotnet-fast metrics App.sln --by-project             # one line per project, appended
dotnet-fast metrics App.sln --format json            # machine-readable
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

**A metric you did not measure never reads as passing.** An unmeasured row says *not measured*, names
the flag that would fill it, and is ignored by `--fail-on-budget`. A green board that is green because
nothing was checked would be worse than no board at all.

| Option | Effect |
|---|---|
| `--fail-on-budget` | Exit `1` when any **measured** metric is over budget. Report-only without it. |
| `--coverage <path>` | Ingest a coverage report — a file, or a directory to search recursively. Repeatable: pass one per test project and the reports merge. Fills in coverage *and* CRAP. |
| `--coverage-format <FORMAT>` | Which coverage format `--coverage` holds: `auto` (default), `cobertura`, `opencover`, `lcov`, `coverlet-json`. `auto` sniffs the report's own content rather than trusting its filename. |
| `--mutation <path>` | Ingest a Stryker.NET `mutation-report.json`, or a directory to search. Repeatable. Counts `Survived` and `NoCoverage` as surviving. |
| `--include-dead-code` | Also run the solution-wide dead-code pass. Off by default because it is markedly slower. |
| `--by-project` | Also break the board down one line per project (JSON: an appended `projects` array). Off by default; never affects the exit code. |
| `--top <N>` | How many worst offenders to list under each failing metric, and how many go in the JSON `offenders` array (default 5). `all` (or `unlimited`) lists every one — the only expensive option here, by orders of magnitude on a large solution. `worstMembers` stays capped at 5 regardless. |
| `--format <FORMAT>` | `text` (default), `json`, or `sarif`. `json` is `{ metrics, worstMembers, summary }` with the full Halstead set per member; `sarif` is SARIF 2.1.0 on stdout for [code scanning](code-scanning.md). `html` is valid only with `--merge`. |
| `--merge <glob>` | Estate mode: read many repositories' own `--format json` reports and render one combined view, sorted worst-first, with a per-budget breach summary. Repeatable. Offline — no server, no storage. A report whose `schemaVersion` this build does not recognise is skipped with a reason rather than silently merged. Renders as `text`, `json` or a self-contained `html` page. |
| `--repository-name <name>` | The name this run reports itself as, for `--merge`. Inferred from the git `origin` remote, falling back to the directory name, so it is usually unnecessary. |
| `--max-cyclomatic`, `--max-cognitive`, `--max-halstead-difficulty`, `--max-lines-per-file`, `--min-coverage`, `--max-crap` | Override a scored budget for this run. |
| `--max-surviving-mutants`, `--max-dead-code`, `--max-duplicate-bodies`, `--max-dynamic-uses` | Override a *count* budget for this run — surviving mutants, dead symbols, duplicate bodies, `dynamic` uses. Each defaults to `0`. |
| `--max-lines-per-member`, `--max-nesting-depth`, `--max-parameters`, `--min-maintainability-index` | Override one of the four appended budgets — defaults 50, 3, 7, and 20 (a floor). |
| `--write-baseline <file>` | Record where the codebase stands today, for the ratchet below. |
| `--baseline <file>` | With `--fail-on-budget`, fail only on a row that got **worse** than the baseline. A row over budget but no worse reports `over budget (no worse than baseline)` and does not fail the build. |

Every budget is a **maximum, exceeded strictly** — a member at exactly 22 is inside a budget of 22 —
and resolves command line > `dotnet-fast.json` > default. The complexity scorers are shared with the
`DF9005`/`DF9006`/`DF9007` guardrails, so the CI gate and this scoreboard can never disagree about a
member. These are `metrics`' own budgets (`dotnet-fast.json`'s `metrics` section) — not the
`.editorconfig` guardrail thresholds `lint` uses; see [editorconfig.md](editorconfig.md).

Files `metrics` could not read (a non-UTF-8 file, a permission error) and files whose C# does not
parse are counted and reported rather than skipped silently — as a footer line, and as
`summary.readFailures` / `summary.filesWithSyntaxErrors` in the JSON.

---

## `dead-dependencies`

Find dependencies a solution declares but does not use — direct `PackageReference`s and
`ProjectReference`s nothing in the referencing project touches, plus MSBuild-side smells (orphaned
central `PackageVersion`, duplicate references, redundant `VersionOverride`). Alias: `dead-deps`.
Build-free and conservative by contract: everything reported is safe to remove; any ambiguity (an
incomplete package inventory, a compile-invisible reference shape, reflection, a parse failure) keeps
the reference instead of risking a false positive. Separate `DD####` id space.

```bash
dotnet-fast dead-dependencies .                    # human report (report-only, exits 0)
dotnet-fast dead-dependencies . --format json      # { findings: [...], summary: {...} }
dotnet-fast dead-dependencies . --no-info          # only the unused tier (DD0001/DD0002/DD0007/DD0009)
dotnet-fast dead-dependencies . --fail-on-unused   # exit 1 when an unused dep is found (CI gate)
dotnet-fast dead-dependencies . --fix              # DRY-RUN: preview the removal diff, write nothing
dotnet-fast dead-dependencies . --fix --write      # apply the surgical csproj/props removals
dotnet-fast dead-dependencies . --verify           # opt-in: build each candidate removal first
```

| Option | Effect |
|---|---|
| `--fail-on-unused` | Exit `1` on an unused finding — without it the command is report-only and exits `0` (info findings never flip the exit). |
| `--no-info` | Report only the unused tier (`DD0001`/`DD0002`/`DD0007`/`DD0009`), hiding the info smells. |
| `--keep <ID>` | Ad-hoc known-keep for a package id or trailing-`*` glob (repeatable) — never report it as unused. |
| `--fix` / `--fix --write` | Remove the removable findings — dry-run diff by default, `--write` applies. `DD0005`/`DD0008` are report-only. |
| `--verify` | Opt-in SDK lane: copy the workspace, apply the removals, `dotnet build` the affected projects, and mark each finding verified iff the build stayed green. With `--fix --write --verify`, only verified removals are written. |

See [dead-dependencies.md](dead-dependencies.md) for the full DD-id catalog, the tri-state detection
gates, the fact tables, and the build-verify lane.

---

## `bom`

Generate a Software Bill of Materials for a project or a solution — every direct and transitive
`PackageReference` plus every `ProjectReference`, with purls, content hashes (when the source tier
carries them), and a full dependency graph, as **CycloneDX** or **SPDX**. Build-free by default: pure
text parsing of `packages.lock.json` / `obj/project.assets.json`, no MSBuild and no `dotnet restore`
shelled out unless you pass `--restore`. A single `.csproj` reports that project alone; a directory,
`.sln`, `.slnx`, or `.slnf` merges every discovered project's graph into one document, scoped by
`.slnf` or `--project`.

```bash
dotnet-fast bom App/App.csproj                     # writes App/bom.json, prints a text summary
dotnet-fast bom App/App.csproj --output sbom.json   # write to an explicit path
dotnet-fast bom App/App.csproj --json               # machine summary (counts + output path) on stdout
dotnet-fast bom .                                   # solution-level: merges every discovered project
dotnet-fast bom . --format spdx                     # SPDX 2.3 JSON instead of CycloneDX 1.6
dotnet-fast bom . --output-format xml               # CycloneDX XML
dotnet-fast bom . --restore                         # restore first if a project has neither tier
```

| Option | Effect |
|---|---|
| `--output <FILE>` | Where to write the BOM document. Defaults to `bom.json` next to the project/solution (`bom.xml` with `--output-format xml`). |
| `--json` | Print a machine-readable summary to stdout instead of the human one. The document itself always goes to `--output`. |
| `--project <NAME>` | (Solution runs) limit to specific projects — the same global flag `affected`/`dead-dependencies` use. |
| `--format <FORMAT>` | Document format: `cyclonedx` (default) or `spdx`. |
| `--spec-version <VERSION>` | CycloneDX accepts `1.4`, `1.5`, `1.6` (default `1.6`); SPDX accepts `2.2`, `2.3` (default `2.3`). |
| `--output-format <ENCODING>` | Encoding: `json` (default) or `xml`. XML is CycloneDX-only. |
| `--restore` | Shell `dotnet restore` for any project with neither a lock file nor restored assets, then retry. Off by default — this is the one flag that gives up "build-free". A hung restore is killed after a bounded timeout. |
| `--exclude-dev` | Omit dev-only components (`PrivateAssets="all"` — analyzers, source generators, build-time tooling) entirely. By default they are included and scoped as CycloneDX `scope: "excluded"` / SPDX `DEV_DEPENDENCY_OF`. |

An unsupported combination — `spdx` with `--output-format xml`, or `cyclonedx` with
`--spec-version 2.2` — fails with an error listing exactly what *is* supported, never a silent
fallback to whichever serializer happens to be newest.

A `.csproj` target requires a `packages.lock.json` next to it (`dotnet restore` with
`<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>`); without one, `bom` fails clearly
rather than guessing the transitive graph. A solution run is more forgiving per project: lock file
preferred, falls back to a restored `obj/project.assets.json`, else that project is skipped with a
stated reason (never a silent partial document). `serialNumber` is derived from the document's own
content, not the current time — re-running against an unchanged project or solution reproduces the same
`serialNumber`. See [bom.md](bom.md) for the full component model, tier table, and document shapes.

---

## `doctor`

A fast, build-free scan for common workspace problems — duplicate package references, conflicting target
frameworks, stale lockfiles, central-package-management mistakes, and more. No restore, no build.

```bash
dotnet-fast doctor App.sln
```

Each finding has a short code (e.g. `DUP-PKG`, `TFM-CONFLICT`) and a one-line explanation of the fix.

---

## `hooks`

Install Git hooks — no husky/lint-staged or JS toolchain needed.

```bash
dotnet-fast hooks install               # write .git/hooks/pre-commit
dotnet-fast hooks install --pre-push    # ...and a pre-push hook
dotnet-fast hooks install --force       # overwrite a foreign existing hook
```

The pre-commit hook runs `lint --fix --staged` (auto-fix + re-stage what you're committing), then
`lint --staged` to block the commit on any leftover finding; the pre-push hook runs `lint` over the
whole tree. Hooks carry a `# managed by dotnet-fast hooks` marker, so re-installing is idempotent
and a hand-written hook is never clobbered without `--force`. Honors `core.hooksPath`.

---

## `editorconfig`

Inspect, infer, or seed your `.editorconfig`. For the full `.editorconfig`/`.globalconfig` precedence
chain, severity vocabulary, and this tool's own threshold keys, see
**[editorconfig.md](editorconfig.md)** — this is the option reference for the subcommands below.

```bash
dotnet-fast editorconfig explain src/Program.cs     # why is (or isn't) an option applying?
dotnet-fast editorconfig init                       # infer one from the existing code, print it
dotnet-fast editorconfig init src --write            # ...or write src/.editorconfig
dotnet-fast editorconfig recommend                  # print the curated ported-analyzer profile
dotnet-fast editorconfig recommend --write           # ...or append it to ./.editorconfig
dotnet-fast editorconfig recommend --guardrails --write  # ...or the guardrail-adoption profile instead
```

**`explain <file>`** prints the `.editorconfig`/`.globalconfig` chain that resolves for a file (in
precedence order) plus the effective core settings (`indent_style`, `indent_size`, `end_of_line`,
`insert_final_newline`), each marked `explicit` or `default` — and each explicit one carries the
exact `.editorconfig:line` that won. `--json` emits the same information for tooling.

**`init [target]`** samples the C# sources (skipping `bin`/`obj`) and infers the dominant style —
tabs vs spaces, indent width, line endings, trailing-newline habit — emitting the four core options.
`--write` writes `<target>/.editorconfig` (refuses to overwrite an existing file).

**`recommend [target]`** prints a curated `.editorconfig` profile for the ported analyzers — the
high-signal bug/dead-code/redundancy rules on at `warning`, subjective style and documentation
rules off — with reasoning and doc links. `--write` appends it to `<target>/.editorconfig`. See
[ported-analyzers.md](ported-analyzers.md).

**`recommend --guardrails [target]`** prints a different profile instead: every AI-guardrail rule
(`DF9001`-`DF9007`) enabled at the published code-health budget numbers, each stated explicitly
rather than left to the rule's own default — including `dotnet_fast_max_lines_per_file = 500`, the
published SLOC budget, not the rule's own bare default of 250. `--write` appends it the same way,
idempotently (a second run is a no-op) and without ever overwriting an existing file. See
[guardrails.md](guardrails.md#adopting-them-without-a-wall-of-findings).

> **Changed in v0.306.0 — inline comments.** `.editorconfig` and `.globalconfig` are now parsed the
> way Roslyn parses them, so a trailing comment no longer voids the line it is on:
> `dotnet_diagnostic.CA2200.severity = none  # noisy` now takes effect, `indent_size = 2  # legacy`
> now applies, and keys under a header written `[*.md]  # docs only` are now scoped to `*.md`
> instead of leaking to every file. Whole-line `#`/`;` comments are ignored exactly as before. This
> makes `format`/`lint` agree with `dotnet format` on configs that already carried inline comments,
> and can therefore change output on a repository you did not touch.

---

## `cache`

Manage the remote build cache's access control.

```bash
dotnet-fast cache ensure-access
```

Grants the current credential's identity the **Storage Blob Data Contributor** role on the cache's
storage account (resolved from `buildCache.url` / `DOTNET_FAST_CACHE_URL`), so CI steps can read
and write the cache without a separate `az role assignment` step. Idempotent. Requires a
control-plane role that can create role assignments (e.g. Owner). See
[build-cache.md](build-cache.md).

---

## `insights`

Read-only analytics over the timing history `dotnet-fast` already records — including a
self-contained `--html` report you can open from a CI artifact. It reads the same timings table test
sharding and the build cache use; the *recording* half runs on the command that produces the numbers.

```bash
dotnet-fast build --record-timings ; dotnet-fast insights build              # slowest projects
dotnet-fast test-plan --shard 1 --of 1 --exec --results-dir trx --record-timings
dotnet-fast insights test --html insights.html                               # slowest fixtures -> HTML
dotnet-fast lint --record-findings ; dotnet-fast insights lint               # finding-count trend
```

| Option | Effect |
|---|---|
| `--html <FILE>` | Write a self-contained HTML report (inline CSS, no external requests). |
| `--json` | Machine-readable document instead of the text table. |
| `--top <N>` | Keep only the N slowest entries (`0` = all); totals still reflect the whole set. |
| `--scope <BRANCH>` | Read a specific branch's history (defaults to the current Git branch). |
| `--baseline-scope <BRANCH>` (`build`/`test`) | Regression view: diff `--scope` against a baseline and report slower/faster/new/missing beyond `--threshold` (default 10%). |

With no timings table configured it prints one stderr note and exits `0` — never fails a pipeline.
See [insights.md](insights.md) for the full guide and
[test-sharding.md](test-sharding.md#azure-table-storage-timings-multi-repo-race-free) for the table
setup.

---

## `update`

Update the tool to the latest published version, using the SDK's own
`dotnet tool update` underneath.

```bash
dotnet-fast update              # update to the latest
dotnet-fast update --check      # report only; change nothing
dotnet-fast update --dry-run    # print the exact command that would run
dotnet-fast update --to 0.305.0 # pin to a specific version
```

This doesn't replace `dotnet tool update` — it *runs* it, with the arguments that match how your copy
was installed. A manifest-pinned install and a global install need different arguments, and using the
wrong form gives you a confusing error instead of an update. The tool already resolves your
`.config/dotnet-tools.json` for the version banner, so it knows which case you are in.

| Option | Effect |
|---|---|
| `--check` | Report the installed and latest versions, then exit. Changes nothing. |
| `--dry-run` | Print the `dotnet tool update` command that would run. Needs no network. |
| `--to <VERSION>` | Update to an exact version instead of the latest. |
| `--exit-code-on-outdated` | Exit `1` when a newer version exists — for a pipeline that wants to fail (or warn) when its tooling has fallen behind. Implies `--check`. |

**A manifest update rewrites `.config/dotnet-tools.json`**, which is tracked in most repositories —
so it is a source-control change, not just a machine change. The command says so before it acts, and
`--dry-run` shows you the command without running it.

**Nothing checks for updates on any other code path.** `lint`, `format`, `affected` and `build` never
reach the network for this, never cache a "last checked" timestamp, and never nag. This command
touches nuget.org because you invoked it.

If you are already ahead of the feed — a local build, a pre-release, or a release still indexing — it
says so and does nothing, rather than offering to move you backwards.

**If you are running a pre-release, mind what `--dry-run` prints.** `--dry-run` needs no network, so
it cannot know what the feed holds — it prints the plain `dotnet tool update` command for your
install shape. That command resolves to the latest **stable** version, which from a pre-release is a
*downgrade*. Running `update` itself is safe (it checks the feed first and declines to move you
backwards); it is only the printed command, run by hand, that would take you back to stable. To move
deliberately in either direction, name the version:

```bash
dotnet-fast update --to 1.0.0-rc.1   # onto a pre-release
dotnet-fast update --to 1.0.0        # back to stable
```

### Removed commands

`restore` and `sbom` were removed or renamed in v0.154.0. Invoking one used to produce
`error: formatting failed: target does not exist: restore`, because the name was parsed as a path to
format — indistinguishable from a typo. They now say what happened and what to use instead, and exit
`2`. A directory genuinely named `restore` is still treated as a target, as before.

---

## Global options

| Option | Effect |
|---|---|
| `--json` | Machine-readable output, including the run's own timing. |
| `--no-deep` | Force the fast native path for one run (overrides any deep-by-default setting). |
| `--help` | Per-command help. |

## Exit codes

- **0** — clean / nothing to do.
- **non-zero** — findings were reported (for `lint`/`format --check`/`doctor`), so CI fails the step.
- **`dead-code` is report-only by default**: it exits `0` even when it finds dead code. Pass
  `--fail-on-dead` to make a `dead`-category finding exit `1` — see [dead-code.md](dead-code.md).
- **`dead-dependencies` is report-only by default**: it exits `0` even when it finds unused
  dependencies. Pass `--fail-on-unused` to make an unused finding exit `1` (info findings never flip
  the exit) — see [dead-dependencies.md](dead-dependencies.md).

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

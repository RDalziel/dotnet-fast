# Command reference

`dotnet-fast` is one command with a handful of subcommands. Each finds your projects from a solution, a
project file, or a directory, and starts instantly. Add `--json` to any command for machine-readable
output (it includes the run's timing).

```
dotnet-fast <command> [target] [options]
```

`target` is optional — with nothing given, the tool finds the nearest `.sln`/`.csproj` or scans the
current directory.

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

Findings honor your `.editorconfig` — a rule set to `none` is suppressed, and `warning`/`error`
promotions are reflected, just like a real build.

**Changed-line scoping.** On a pull request (`--ci` PR, `--pr-base`, `--base`) and for `--staged`, the
report covers only the lines you actually changed — pre-existing findings on untouched lines of a file
you happen to touch are suppressed, so a branch that lags `main` is not failed by old debt. Pass
`--whole-file` to report a touched file's full backlog, or `--changed-lines` to force the same scoping
on an explicit `--from`/`--to` or `--ci` push build. Scoping affects reported findings only; `--fix`
still rewrites whole files.

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
| `--restore-from-cache` (`--exec`) | Restore only this shard's project closure from the build cache before running (scopes the per-agent download); `--fallback` builds the closure if the cache is down. |
| `--test-args <ARGS>` | Append pass-through arguments to each `dotnet test`. |
| `--filter-and <FILTER>` | AND an extra VSTest filter with the generated fixture filter. |
| `--results-dir <DIR>` / `--collect <COLLECTOR>` | Emit TRX results and optional coverage collection. |
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
dotnet-fast dead-dependencies . --no-info          # only the unused tier (DD0001/DD0002/DD0007)
dotnet-fast dead-dependencies . --fail-on-unused   # exit 1 when an unused dep is found (CI gate)
dotnet-fast dead-dependencies . --fix              # DRY-RUN: preview the removal diff, write nothing
dotnet-fast dead-dependencies . --fix --write      # apply the surgical csproj/props removals
dotnet-fast dead-dependencies . --verify           # opt-in: build each candidate removal first
```

| Option | Effect |
|---|---|
| `--fail-on-unused` | Exit `1` on an unused finding — without it the command is report-only and exits `0` (info findings never flip the exit). |
| `--no-info` | Report only the unused tier (`DD0001`/`DD0002`/`DD0007`), hiding the info smells. |
| `--keep <ID>` | Ad-hoc known-keep for a package id or trailing-`*` glob (repeatable) — never report it as unused. |
| `--fix` / `--fix --write` | Remove the removable findings — dry-run diff by default, `--write` applies. `DD0005`/`DD0008` are report-only. |
| `--verify` | Opt-in SDK lane: copy the workspace, apply the removals, `dotnet build` the affected projects, and mark each finding verified iff the build stayed green. With `--fix --write --verify`, only verified removals are written. |

See [dead-dependencies.md](dead-dependencies.md) for the full DD-id catalog, the tri-state detection
gates, the fact tables, and the build-verify lane.

---

## `bom`

Generate a Software Bill of Materials for a project or a solution as CycloneDX 1.6 JSON — every direct
and transitive `PackageReference` plus every `ProjectReference`, with purls, content hashes (when the
source tier carries them), and a full dependency graph. Build-free: pure text parsing of
`packages.lock.json` / `obj/project.assets.json`, no MSBuild, no `dotnet restore` shelled out. A single
`.csproj` reports that project alone; a directory, `.sln`, `.slnx`, or `.slnf` merges every discovered
project's graph into one document, scoped by `.slnf` or `--project`.

```bash
dotnet-fast bom App/App.csproj                     # writes App/bom.json, prints a text summary
dotnet-fast bom App/App.csproj --output sbom.json   # write to an explicit path
dotnet-fast bom App/App.csproj --json               # machine summary (counts + output path) on stdout
dotnet-fast bom .                                   # solution-level: merges every discovered project
```

| Option | Effect |
|---|---|
| `--output <FILE>` | Where to write the BOM document. Defaults to `bom.json` next to the project/solution. |
| `--json` | Print a machine-readable summary to stdout instead of the human one. The document itself always goes to `--output`. |
| `--project <NAME>` | (Solution runs) limit to specific projects — same global flag `affected`/`dead-dependencies` use. |
| `--format` / `--spec-version` | Reserved flags — only `cyclonedx`/`1.6` are accepted today, so a future format/version is purely additive. |

A `.csproj` target requires a `packages.lock.json` next to it (`dotnet restore` with
`<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>`); without one, `bom` fails clearly
rather than guessing the transitive graph. A solution run is more forgiving per project: lock file
preferred, falls back to a restored `obj/project.assets.json`, else that project is skipped with a
stated reason (never a silent partial document). `serialNumber` is derived from the document's own
content, not the current time — re-running against an unchanged project or solution reproduces the same
`serialNumber`. See [bom.md](bom.md) for the full component model, tier table, and CycloneDX shape.

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

Inspect, infer, or seed your `.editorconfig`.

```bash
dotnet-fast editorconfig explain src/Program.cs     # why is (or isn't) an option applying?
dotnet-fast editorconfig init                       # infer one from the existing code, print it
dotnet-fast editorconfig init src --write            # ...or write src/.editorconfig
dotnet-fast editorconfig recommend                  # print the curated ported-analyzer profile
dotnet-fast editorconfig recommend --write           # ...or append it to ./.editorconfig
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

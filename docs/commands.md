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

Useful options:

| Option | Effect |
|---|---|
| `--fix` | Apply the safe fixes (whitespace, style, simple lint rules) — changes only what it reports. |
| `--deep` | Add the project's real Roslyn analyzers. Needs the .NET SDK + a restored project. |
| `--severity <level>` | Only report at/above a level (`info`/`warning`/`error`). |
| `--exclude-diagnostics <ID...>` | Suppress specific rule IDs (e.g. `DF0001`). |
| `--baseline <file>` | Compare against a saved baseline and fail only on *new* findings. |
| `--json` / `--report <file>` | Emit results as JSON / write a JSON report. |
| `--sarif <file>` | Write SARIF (for code-scanning dashboards). |

Findings honor your `.editorconfig` — a rule set to `none` is suppressed, and `warning`/`error`
promotions are reflected, just like a real build.

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
matrix.

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

See [build-cache.md](build-cache.md) for setup and CI examples.

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
| `--test-args <ARGS>` | Append pass-through arguments to each `dotnet test`. |
| `--filter-and <FILTER>` | AND an extra VSTest filter with the generated fixture filter. |
| `--results-dir <DIR>` / `--collect <COLLECTOR>` | Emit TRX results and optional coverage collection. |
| `--verify` | Run `dotnet test` and confirm the shard union matches the baseline list. |
| `--ci` / range flags | Shard only affected test projects. |

See [test-sharding.md](test-sharding.md) for CI examples and NUnit details.

---

## `doctor`

A fast, build-free scan for common workspace problems — duplicate package references, conflicting target
frameworks, stale lockfiles, central-package-management mistakes, and more. No restore, no build.

```bash
dotnet-fast doctor App.sln
```

Each finding has a short code (e.g. `DUP-PKG`, `TFM-CONFLICT`) and a one-line explanation of the fix.

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

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

# Insights

`dotnet-fast insights` turns the timing history `dotnet-fast` already records into **read-only**
reports — including a self-contained HTML page you can open straight from a CI artifact. No extra
services and no separate database: it reads the same timings table that test sharding and the build
cache use (`buildCache.timingsTable` in `dotnet-fast.json`, or `DOTNET_FAST_TIMINGS_TABLE`; see
[test-sharding.md](test-sharding.md#azure-table-storage-timings-multi-repo-race-free)).

Each subcommand reports one recorded kind. The only trick is that the *recording* half runs on the
command that produces the numbers, and `insights` reads them back:

| Report | Recorded by | Shows |
|---|---|---|
| `insights build` | `build --record-timings` | per-project build time, slowest first |
| `insights test` | `test-plan --exec --results-dir <dir> --record-timings` | per-fixture run time, slowest first |
| `insights lint` | `lint --record-findings` | lint finding counts, top rules, severity split, fix-adoption |

## Generate an HTML report of your slowest tests

Three lines — and the first is one-time setup:

```powershell
# 1. Once: point dotnet-fast at a timings table (the same one test sharding / build cache use).
#    In dotnet-fast.json:  { "buildCache": { "timingsTable": "dotnetfasttimings" } }
#    (or set DOTNET_FAST_TIMINGS_TABLE=dotnetfasttimings). See test-sharding.md.

# 2. Record a run's fixture durations into that table.
dotnet-fast test-plan --shard 1 --of 1 --exec --results-dir trx --record-timings

# 3. Render a self-contained HTML report of the slowest fixtures.
dotnet-fast insights test --html insights.html
```

`insights.html` is a single self-contained file — inline CSS, zero external requests, theme-aware —
so it opens straight from a CI artifact with nothing else to serve. Swap `test` for `build` (record
with `build --record-timings`) or `lint` (record with `lint --record-findings`) for the other two
reports.

## The three reports

```bash
dotnet-fast insights build              # rank projects by recorded build time
dotnet-fast insights test               # rank fixtures by recorded run time
dotnet-fast insights lint               # lint finding-count trend + top rules
```

- **`build`** ranks projects by recorded build time, each with its share of total build time.
- **`test`** ranks test fixtures by recorded run time — find the slow fixture that sets your CI
  wall-clock before adding more shards.
- **`lint`** shows the lint finding-count backlog over time, the rules that fire most, a by-severity
  split, and the fix-adoption rate (how much of what's auto-fixable is actually getting fixed).

## Options

Common to all three subcommands:

| Option | Effect |
|---|---|
| `--json` | Emit a machine-readable JSON document instead of the text table. |
| `--html <FILE>` | Write a self-contained HTML report to `<FILE>` (text still prints unless `--json`). |
| `--top <N>` | Keep only the N slowest entries (`0` = all); totals still reflect the whole set. |
| `--scope <BRANCH>` | Read a specific branch's history (defaults to the current Git branch). |
| `--env <ci\|local>` | Read the `ci` or `local` partition explicitly, instead of auto-detecting from the invoking shell — e.g. read what a CI run recorded from your own machine. |

### Regression view (`build` / `test`)

Pass `--baseline-scope <BRANCH>` to diff `--scope` against a baseline (typically your default
branch) instead of the plain hotspots list — it reports what got **slower**, **faster**, **new**, or
**missing** beyond a threshold:

```bash
dotnet-fast insights test --scope my-feature --baseline-scope main --threshold 10
```

`--threshold PCT` (default `10`) filters out moves smaller than `PCT` percent as noise; each row
shows both durations, the signed delta, and both sample counts, so a one-sample "regression" is not
presented like a fifty-sample one.

## Degrades quietly

With no timings table configured, every `insights` command prints one note to stderr and **exits 0**
— it never fails a pipeline. The same holds if the store is briefly unreachable. Configure the table
once (see [test-sharding.md](test-sharding.md#azure-table-storage-timings-multi-repo-race-free)) and
the reports fill in as your runs record history.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

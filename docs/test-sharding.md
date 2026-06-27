# Test sharding

`dotnet-fast test-plan` splits NUnit test projects across CI agents without building the assemblies
first. It discovers fixtures from source, balances them across shards, and emits a GitHub Actions
matrix, an Azure DevOps matrix, JSON, or runnable `dotnet test` commands.

v1 is NUnit-only. xUnit and MSTest support are planned follow-ups.

## Quick start

```bash
dotnet-fast test-plan --shards 8 --format matrix .
dotnet-fast test-plan --shards 8 --format ado-matrix .
dotnet-fast test-plan --ci --auto-shards --min-per-shard 50 --max-shards 8 --format matrix .
dotnet-fast test-plan --shards 8 --format json .
dotnet-fast test-plan --shard 3 --of 8 .
dotnet-fast test-plan --shard 3 --of 8 --exec --test-args "--no-build --no-restore -c Release" --results-dir ./trx
dotnet-fast test-plan --ci --shards 8 --format json .
dotnet-fast affected --ci --tests-only --format dotnet-test --output-name affected-tests .
dotnet-fast test-plan --projects-file affected-tests.dotnet-test.txt --auto-shards --format matrix .
dotnet-fast build --plan --json . > build-cache-plan.json
dotnet-fast test-plan --cache-misses-file build-cache-plan.json --auto-shards --format matrix .
dotnet-fast test-plan --shards 8 --verify .
```

## Azure Pipelines parallel jobs

Each agent computes the same plan, then runs its own shard.

```yaml
jobs:
  - job: tests
    strategy:
      parallel: 8
    steps:
      - checkout: self
        fetchDepth: 0
      - pwsh: dotnet-fast test-plan --shard $(System.JobPositionInPhase) --of $(System.TotalJobsInPhase) --settings-dir $(Agent.TempDirectory) | pwsh -
```

`--settings-dir` writes long filters to `.runsettings` files, avoiding command-line length limits on
large suites.

Use `--exec` to run the selected shard directly. `--test-args` appends real `dotnet test` arguments,
`--filter-and` ANDs a category/trait filter with the generated fixture filter, `--results-dir` adds one
TRX logger per shard project, and `--collect cobertura` expands to `Code Coverage;Format=cobertura`.

## GitHub Actions matrix

```yaml
jobs:
  plan:
    runs-on: ubuntu-latest
    outputs:
      matrix: ${{ steps.plan.outputs.matrix }}
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      - id: plan
        shell: pwsh
        run: |
          $json = dotnet-fast test-plan --ci --auto-shards --min-per-shard 50 --max-shards 8 --format matrix .
          "matrix=$json" >> $env:GITHUB_OUTPUT

  tests:
    needs: plan
    runs-on: ubuntu-latest
    strategy:
      matrix: ${{ fromJson(needs.plan.outputs.matrix) }}
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      - shell: pwsh
        run: dotnet-fast test-plan --shard ${{ matrix.shard }} --of ${{ matrix.totalShards }} --settings-dir $env:RUNNER_TEMP | pwsh -
```

## End-to-end optimized pipeline

The biggest wins combine three levers, in order: **scope to affected** projects,
**shard** the rest across agents, and **balance shards by time** so the slowest
one (which sets the wall-clock) is minimized. A leaf-file change then runs only
the impacted project; a full run spreads its slow fixtures evenly.

```yaml
name: ci
on: [pull_request, push]
concurrency: { group: ci-${{ github.ref }}, cancel-in-progress: true }
permissions: { contents: read, id-token: write }   # id-token: OIDC -> Azure blob cache
env:
  DOTNET_FAST_CACHE_URL: "https://<account>.blob.core.windows.net/dotnet-buildcache"

jobs:
  plan:                                  # scope to affected, emit a shard matrix
    runs-on: ubuntu-latest
    outputs: { matrix: ${{ steps.p.outputs.matrix }} }
    steps:
      - uses: actions/checkout@v4
        with: { fetch-depth: 0 }         # affected needs history (merge-base)
      - uses: ./.github/actions/dotnet-fast
      - id: p
        run: |
          dotnet-fast affected --ci --tests-only --format traversal --output-name affected
          echo "matrix=$(dotnet-fast test-plan --projects-file affected.proj \
            --auto-shards --max-shards 12 --format matrix)" >> "$GITHUB_OUTPUT"
      - uses: actions/upload-artifact@v4
        with: { name: affected, path: affected.proj }

  tests:
    needs: plan
    runs-on: ubuntu-latest
    strategy:
      fail-fast: false
      matrix: ${{ fromJSON(needs.plan.outputs.matrix) }}   # [{shard, totalShards}]
    steps:
      - uses: actions/checkout@v4
        with: { fetch-depth: 0 }
      - uses: ./.github/actions/dotnet-fast
      - uses: azure/login@v2                     # OIDC; no stored secret
        with:
          client-id: ${{ vars.AZURE_CLIENT_ID }}
          tenant-id: ${{ vars.AZURE_TENANT_ID }}
          subscription-id: ${{ vars.AZURE_SUBSCRIPTION_ID }}
      - uses: actions/download-artifact@v4
        with: { name: affected }
      - run: dotnet-fast build --read-only --fallback --projects-file affected.proj .  # restore from cache
      - run: |
          dotnet-fast test-plan --projects-file affected.proj \
            --shard ${{ matrix.shard }} --of ${{ matrix.totalShards }} \
            --exec --results-dir trx --use-cached-timings \
            ${{ github.ref == 'refs/heads/main' && '--record-timings' || '' }} \
            --test-args "--no-build -c Release"
```

Install `dotnet-fast` once and cache it (no `dotnet tool restore` per agent) with a
composite action that downloads the [standalone binary](build-cache.md):

```yaml
# .github/actions/dotnet-fast/action.yml
runs:
  using: composite
  steps:
    - uses: actions/setup-dotnet@v4
      with: { dotnet-version: '9.0.x' }
    - uses: actions/cache@v4
      id: c
      with: { path: ${{ runner.tool_cache }}/dotnet-fast, key: dotnet-fast-${{ runner.os }} }
    - if: steps.c.outputs.cache-hit != 'true'
      shell: bash
      run: |
        mkdir -p "${{ runner.tool_cache }}/dotnet-fast"
        curl -sSL -o "${{ runner.tool_cache }}/dotnet-fast/dotnet-fast" \
          https://github.com/RDalziel/dotnet-fast/releases/latest/download/dotnet-fast-linux-x64
        chmod +x "${{ runner.tool_cache }}/dotnet-fast/dotnet-fast"
    - shell: bash
      run: echo "${{ runner.tool_cache }}/dotnet-fast" >> "$GITHUB_PATH"
```

### Tuning

- **`affected` first.** Most PRs touch a few files — scoping the build+test to the
  impacted projects is the single biggest lever. A change to a foundational library
  legitimately fans out to most of the suite; a leaf change runs one project.
- **Shard count knee.** Adding agents helps only until the **heaviest single
  fixture** dominates — a fixture is atomic and cannot be split, so its duration is
  a hard floor. Find your slowest fixture (`--timings`/TRX) before raising
  `--max-shards`; past the knee, extra agents sit idle. `--auto-shards` keys the
  count off the affected size.
- **Record clean timings.** `--record-timings` should run on independent agents
  (a real matrix), not several shards crammed onto one machine — durations measured
  under local CPU contention are noisy and can *worsen* balancing.
- **The build cache pays off on CI**, where the agent↔storage link is fast: a warm
  cache skips the rebuild. It needs committed `packages.lock.json` (see
  [Build cache](build-cache.md)); without lock files every project is uncacheable.
  On a developer box, a plain incremental `dotnet build` is usually faster than the
  cache round-trip.

## Command output

Default command output is PowerShell-safe and can be piped to `pwsh -`. JSON output includes `argv`
arrays for CI systems that execute processes directly.

```json
{
  "commands": ["dotnet test 'Tests.csproj' --filter 'FullyQualifiedName~N.Fixture.'"],
  "argv": [["dotnet", "test", "Tests.csproj", "--filter", "FullyQualifiedName~N.Fixture."]]
}
```

The same command modifiers used by `--exec` are reflected in `commands` and JSON `argv`, so callers
that execute the plan themselves do not need to splice arguments into the emitted commands.

`--auto-shards` chooses `ceil(totalWeight / --min-per-shard)`, clamped to `1..--max-shards`. With an
affected range, no affected test projects emits an empty matrix and exits `166`.

Use `--format ado-matrix` for Azure Pipelines `strategy.matrix`; `--format matrix` stays the GitHub
Actions `{"include":[...]}` shape.

## Balance by prior-run time (`--timings`)

By default shards are balanced by **fixture count**, which can leave one shard holding the slow fixtures
while the others idle. Pass `--timings <file>` to balance by **prior-run execution time** instead:

```bash
dotnet-fast test-plan --ci --auto-shards --max-shards 8 --timings prior-durations.json --format ado-matrix .
```

The file is a JSON object mapping each fixture's VSTest fully-qualified name (nested classes use the
runtime `Ns.Outer+Inner` form) to its duration in **milliseconds**:

```json
{ "MyApp.Tests.OrderServiceTests": 4200, "MyApp.Tests.PricingTests": 1800 }
```

The slowest fixtures are spread across agents (longest-processing-time bin-packing), shortening the
parallel stage's tail. A fixture absent from the file (new or renamed) is estimated from its static
weight scaled to the average time-per-weight of the timed fixtures, so a cold or partial file still
balances sensibly — and an empty/unmatched file falls back to plain count balancing. Produce the file
from a previous run's TRX results (sum each fixture's test durations).

## Cached timings — zero YAML (`--use-cached-timings` / `--record-timings`)

Managing the timings file yourself (produce it from TRX, publish it as an artifact, download it next
run) is plumbing. If you already use the [build cache](build-cache.md), let `dotnet-fast` own the
round-trip through the cache blob instead — timing-balanced sharding then becomes two flags on calls
the pipeline already makes:

```bash
# Planner: balance by durations stored in the cache (falls back to weight when none recorded).
dotnet-fast test-plan --ci --auto-shards --use-cached-timings --format ado-matrix .

# Each agent: after running its shard, record its TRX durations back to the cache.
dotnet-fast test-plan --shard $(System.JobPositionInPhase) --of $(System.TotalJobsInPhase) \
  --exec --results-dir "$(Agent.TempDirectory)/trx" --record-timings .
```

`--record-timings` parses the TRX the shard already writes (`--results-dir`), aggregates per-fixture
durations, and merges them into the cached blob with a moving average (so a one-off slow run doesn't
permanently skew planning). `--use-cached-timings` downloads that blob and balances by time. The blob
is keyed per **scope** — `--timings-scope`, else `DOTNET_FAST_TIMINGS_SCOPE`, else the current Git
branch — so a PR balances against trunk's history. A missing blob or unconfigured cache degrades
gracefully to weight-based planning. No artifact publish/download, no timings file in YAML.

## How fixtures are split

- a fixture stays on one shard
- small projects run whole when they fit a shard
- only oversized projects are split by fixture
- nested fixtures use runtime `+` names
- generic fixture filters are kept collision-safe
- affected range flags narrow the plan to affected test projects
- repeated `--project` and `--projects-file` can pass an exact project set from an affected step
- `--cache-misses-file` consumes build-cache JSON and keeps only tests impacted by non-hit projects

Use `--verify` when rolling the plan out to a new repository. It runs the baseline tests and shard
filters, then checks that the shard union matches the baseline list.

## Build-cache test impact

`test-plan --cache-misses-file <FILE>` accepts `dotnet-fast build --plan --json`, real
`dotnet-fast build --json`, or the `build-report.json` written by `dotnet-fast build --report <DIR>`.
It selects cache misses/non-restored projects plus dependent test projects, so cache-hit-only test
projects can be skipped or backed by previous results.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

# Build cache

`dotnet-fast build` is a preview remote build cache for .NET CI. On a clean agent, cached projects are
restored from storage instead of rebuilt. Cache misses are built normally, and trusted builds can upload
the resulting artifacts for the next run.

The cache is conservative: when a project cannot be fingerprinted safely, `dotnet-fast` builds it
normally and does not cache it.

## Quick start

```bash
dotnet-fast build --plan .           # inspect keys and cacheability
dotnet-fast build --plan --json .    # machine-readable plan
dotnet-fast build --plan --check .   # 0 all cached, 3 incomplete, 4 unavailable
dotnet-fast build --projects-file affected.proj .
dotnet-fast build .                  # restore hits, build/upload misses
dotnet-fast build --json .           # real-run cache report
dotnet-fast build --report artifacts .
dotnet-fast build --read-only .      # PR-safe: never upload
```

`build` can be scoped with repeated `--project <name-or-path>`, `--projects-file <file>`, or affected
range flags (`--ci`, `--from`, `--to`, `--merge-base`). `--projects-file` accepts newline
names/paths, `dotnet test <project>` lines, affected matrix JSON, or a traversal project. Explicit
scope and affected scope are intersected. `build --plan` stays on the selected projects; a real `build`
also processes the selected set's transitive dependency closure and lists those dependency projects
separately in the report (a `+ N dependency project(s) (closure)` line in text, `dependencyProjects` in
`--json`), so a scoped run surfaces the dependency work instead of hiding it.

## Compiling misses concurrently

Cache hits restore in parallel and artifacts upload in parallel, but the projects that actually have
to be **compiled** — cache misses and un-cacheable projects — build one at a time by default. On a
cold or heavily-invalidated build that single loop is most of the wall clock.

It is sequential for a reason. Those projects were not restored from the cache, so each `dotnet
build` runs its own implicit NuGet restore, and several implicit restores at once race on the shared
global packages folder — producing a spurious `CS0246` for a package that is correctly locked.

If your pipeline already restores the whole tree before building, tell the tool:

```bash
dotnet restore --locked-mode MySolution.sln
dotnet-fast build --assume-restored .
dotnet-fast build --assume-restored --build-jobs 4 .   # cap concurrency
```

`--assume-restored` passes `--no-restore` to every compile — which is now correct — and, because
nothing is restoring any more, compiles the projects in each topological layer concurrently. Layers
still run in order, so dependencies are always on disk before their dependents build, and a failed
project still skips its dependents rather than letting them fail confusingly.

`--build-jobs N` caps how many compile at once (default: one per logical core). Each `dotnet build`
is itself multi-threaded, so a lower number is sometimes faster on a small agent.

Do not pass `--assume-restored` without that upfront restore: a project that was not a cache hit
would have no restore state, and `--no-restore` fails it outright.

## When the cache can't help

Two configurations used to look exactly like a working cache. Both now warn.

**Nothing in the repo is cacheable.** A project without a committed `packages.lock.json` can't be
cached — the package graph isn't pinned reproducibly, so a key can't be computed. If that's true of
*every* project, the cache can never produce a hit, and the run pays full build cost plus cache
overhead forever:

```
build: warning: 0 of 12 projects are cacheable — the cache cannot restore or upload anything for
this run. Dominant reason: no packages.lock.json (package versions not pinned reproducibly)
(12/12). Run `dotnet-fast build --plan` for the per-project breakdown.
```

Fix it by committing lock files (`dotnet restore --use-lock-file`, or set
`<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>`). The warning fires only when every
project in the run is un-cacheable — a repo with a few un-cacheable projects still gets real value
and stays quiet.

**The credential isn't usable.** A run authenticating with Azure AD now reports which credential it
resolved, and a bad configuration says so instead of failing later:

```
build: cache credential resolved: Azure AD bearer (az CLI).
build: warning: cache container https://acct.blob.core.windows.net/cache not found — nothing can be
restored, and uploads will fail (a read/write run will end with failed projects).
```

Neither warning changes exit codes; they are diagnostics. `build --json` and `--report` also carry
`cacheable`, `uncacheable`, and `uncacheableReasons` in the summary object if you'd rather gate in CI.

## Progress output

A cache-backed build streams its work to stderr as it happens — one line per phase and one per
project — so a long cold run is visibly working rather than looking hung:

```
build: restoring from cache 147 project(s)…
build: [1/147] restored Contracts (312 file(s), 4.1 MB)
build: [2/147] miss App.Api — will build
build: building 32 project(s)…
build: [1/32] built Contracts (11.4s)
build: archiving + uploading 32 project(s)…
build: [1/32] uploaded Contracts (4.2 MB)
```

The report on stdout (and `--json`) is unchanged, so anything parsing it is unaffected. `test-plan
--restore-from-cache` streams the same lines under its own prefix.

## Diagnosing where the time and bytes go

Two environment variables, both off by default and safe to leave on in CI.

`DOTNET_FAST_TIMING=1` breaks a cache-backed build into sub-phases, so "the cache step is slow" turns
into an answerable question — network, CPU, or disk:

```
[timing] build-cache detail: restore = download 2435ms + verify 0ms + extract 31ms;
         upload = archive 30468ms + hash 538ms + put 132194ms (323.4 MB raw -> 91.2 MB compressed)
```

Those are worker-summed totals — compare them against each other, not against the wall clock.

`DOTNET_FAST_CACHE_STATS=1` reports how much of what you upload is **duplicate content**, and names
the files responsible:

```
[cache-stats] 5 artifact(s), 1551 file(s), 323.4 MB raw; unique content 123.6 MB,
              duplicated 199.8 MB (61.8%)
[cache-stats]   13.9 MB wasted by bin/Release/net10.0/runtimes/linux-arm64/native/libcapstone.so
                (3 copies across 1 project(s))
```

Read the `copies across N project(s)` part carefully — it tells you *which kind* of duplication you
have, and they have different fixes:

- **`N copies across 1 project`** — the same bytes repeated inside one project's own output,
  typically native assets shipped for several runtime identifiers. Trimming the RIDs you actually
  deploy (`<RuntimeIdentifiers>`) is usually the fastest win.
- **`N copies across N projects`** — a dependency's output copied into each dependent's `bin/`. This
  is inherent to how MSBuild lays out build output.

The probe only sees what a run *uploads*, so run it against a cold cache (or a branch that
invalidates keys) for a full picture; a warm run uploads little and will under-report.

## Configure the cache

Set the cache URL with `DOTNET_FAST_CACHE_URL`, or commit a `dotnet-fast.json`:

```json
{
  "buildCache": {
    "url": "https://account.blob.core.windows.net/cache",
    "sasToken": "",
    "connectionString": "",
    "configuration": "Release",
    "timingsTable": ""
  }
}
```

Setting `timingsTable` (env `DOTNET_FAST_TIMINGS_TABLE`) opts the cached test/build timings into
Azure Table Storage on the same account — repository-partitioned and race-free for wide shard
fan-outs; see
[test-sharding.md](test-sharding.md#azure-table-storage-timings-multi-repo-race-free) for the full
behavior and the related `timingsTableSas` / `repositoryId` / `timingsDefaultScope` /
`timingsMaxAgeDays` keys.

Azure Blob auth can use short-lived SAS input:

- `DOTNET_FAST_CACHE_SAS` or `buildCache.sasToken`
- `DOTNET_FAST_CACHE_CONNECTION_STRING` or `buildCache.connectionString` with `BlobEndpoint` and
  `SharedAccessSignature`
- a SAS query appended to the cache URL

Without SAS input, auth uses Entra credentials:

- environment client credentials (`AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`)
- managed identity on hosted agents
- `az login` for local development

Use read/list access for PR builds and create/write access for trusted builds that upload. Account-key
connection strings are not used directly; generate an account-key SAS or user-delegation SAS instead.

## Grant cache access (one step, no AzureCLI wrapper)

When the storage account has shared-key auth disabled, a CI principal needs the
**Storage Blob Data Contributor** data-plane role before it can read/write the
cache. `cache ensure-access` grants it using the same credential the build steps
already use — so you don't wrap `dotnet-fast` in an `AzureCLI@2`/`az role
assignment` task just to set up RBAC:

```bash
dotnet-fast cache ensure-access .
```

It reads the account from `buildCache.url`, finds the principal from its own
token, and ensures the role assignment (idempotent — an existing grant is a
read-only no-op). The principal must hold a control-plane role that can create
role assignments (e.g. Owner). Run it once in a setup step. With a timings
table configured it also grants **Storage Table Data Contributor** in the same
run.

## CI pattern

One invocation covers cache-hit, cache-miss, **and** cache-down — `--fallback`
runs a plain `dotnet build` if the cache is unreachable or unconfigured, and
`--report <dir>` captures the hit/miss report, so there's no hand-written probe /
fallback / report scaffolding:

```bash
# Trusted branch (writes the cache); PRs add --read-only.
dotnet-fast build --fallback --report artifacts --projects-file affected.proj .
```

Or spell out the branches explicitly. Trusted branch:

```bash
if dotnet-fast build --plan --check .; then
  dotnet-fast build .
else
  dotnet restore
  dotnet build --no-restore
fi
```

Pull request:

```bash
dotnet-fast build --read-only --fallback .
```

Affected-only:

```bash
dotnet-fast affected --ci --format traversal --output-name affected .
dotnet-fast build --projects-file affected.proj .
```

Cache-driven test impact:

```bash
dotnet-fast build --plan --json . > build-cache-plan.json
dotnet-fast build --plan --report artifacts .
dotnet-fast build .
dotnet-fast test-plan --cache-misses-file build-cache-plan.json --auto-shards --format matrix .
```

## Cacheability limits

Projects are built normally, not cached, when the tool sees:

- package references without a committed `packages.lock.json`
- a custom **build-phase** target (a publish-phase target such as `AfterTargets="Publish"` does **not**
  disqualify caching — it runs after the build), or an `Exec`/`UsingTask` hosted by a build-phase target
- T4 templates
- project/import parse failures
- a build-output redirection property (`PackageOutputPath`, `BaseOutputPath`/`OutputPath`) that can't be
  statically resolved to a directory inside the project — when it *can* be resolved in-repo (e.g.
  `GeneratePackageOnBuild` with a custom `PackageOutputPath`), the project stays cacheable and that
  directory is captured into the artifact instead

Cached artifacts include `bin/<configuration>` and `obj/<configuration>`, **plus the NuGet restore
state** at the `obj/` root (`project.assets.json` and the generated NuGet props/targets) and any
in-project build-output redirection directory (e.g. a custom `PackageOutputPath`), with SHA-256
integrity verification before extraction. Including the restore state means a cache-restored tree is
valid for `--no-restore` consumers — `lint --deep` and `dotnet publish --no-build` — without a separate
`dotnet restore`. (`project.assets.json` holds machine-absolute paths, so this assumes producer and
consumer share an agent image and checkout path; if not, the step simply asks for a restore as before.)

**Cache key & versioning.** A project's key is a Merkle hash over a stable key-format version, the .NET
SDK version, the configuration, the project's input fingerprint, and its dependencies' keys. The
key-format version (`keyformat=` in the text header, `keyFormatVersion` in `--json`) is **not** the
`dotnet-fast` tool version — a routine tool upgrade keeps the cache warm. Only an incompatible
key-format change invalidates prior entries (a one-time cold rebuild), and it is visible in the report
before the run.

**Sharding the build across agents.** For a cold cache or a foundation change, `build --format
matrix`/`--auto-shards` partitions the affected build's closure into topological **layers** (waves) and
shards each layer; `build --layer L --shard I --of N` builds one wave's slice while its dependencies
(earlier waves) restore from the cache. The pipeline runs the layers as sequential waves. Mainly helps
cold/foundation builds (warm PR builds are already restore-dominated); the exact multi-wave YAML is
still being validated against real Azure DevOps pipelines.

By default each shard is balanced by raw project *count* — fine when projects are similarly sized,
but a lopsided shard (one huge project alongside many small ones) can bottleneck the whole wave on
its slowest shard. Add `--record-timings` to a shard job so it saves its own per-project build
durations to the cache, and `--use-cached-timings` on later runs so shard assignment is balanced by
actual build time instead of count:

```bash
dotnet-fast build --layer 0 --shard $SHARD --of $TOTAL --use-cached-timings --record-timings .
```

Every agent building a shard of the *same* run should pass the same timing flags, so they all slice
consistently. With no timing data yet (first run, or a cold cache), this degrades to the same
count-balanced behavior as today — the flags are purely additive.

After a successful `dotnet-fast build`, every processed project has `bin/<configuration>` and
`obj/<configuration>` materialized either from the verified cache artifact or from a real `dotnet build
-c <configuration> --no-dependencies`. Downstream CI can run `dotnet test --no-build --no-restore -c
<configuration>` against that tree without a separate warm restore step. `dotnet-fast build --json`
reports each project's action (`restored`, `built-and-uploaded`, `built`, or `failed`), path, duration,
downloaded/uploaded bytes, and summary hit-rate totals. `--report <DIR>` writes the same JSON to
`<DIR>/build-report.json` for plan and real build runs.

## Retention and concurrent writes

The CLI does not prune cache blobs. Configure Azure Blob lifecycle management for the cache container,
for example deleting blobs older than 30-90 days.

Concurrent writes to the same key are safe: small uploads use atomic `Put Blob`; large uploads commit
only after all blocks are present; readers verify SHA-256 metadata before extraction. An interrupted
chunked upload leaves uncommitted blocks, not a partially-readable hit.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

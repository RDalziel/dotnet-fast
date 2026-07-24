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

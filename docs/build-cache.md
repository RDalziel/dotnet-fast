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
scope and affected scope are intersected. Plans and reports stay on the selected projects; real builds
may process referenced projects internally when a selected cache miss needs them.

## Configure the cache

Set the cache URL with `DOTNET_FAST_CACHE_URL`, or commit a `dotnet-fast.json`:

```json
{
  "buildCache": {
    "url": "https://account.blob.core.windows.net/cache",
    "sasToken": "",
    "connectionString": "",
    "configuration": "Release"
  }
}
```

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
role assignments (e.g. Owner). Run it once in a setup step.

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
- custom targets, `Exec`, or `UsingTask`
- T4 templates
- project/import parse failures

Cached artifacts include `bin/<configuration>` and `obj/<configuration>`, with SHA-256 integrity
verification before extraction.

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

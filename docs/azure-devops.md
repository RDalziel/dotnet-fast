# Azure DevOps

`dotnet-fast` has several Azure Pipelines-specific capabilities that don't show up if you only read
the generic command docs: a batched-trigger-aware `affected` baseline, one-command build-cache RBAC
setup, and `ado-matrix` output for both build and test sharding. This page ties them together into
one real pipeline. See [`affected`](commands.md#affected) / [build cache](build-cache.md) /
[test sharding](test-sharding.md) for the full per-command reference.

## Batched trigger builds (`--ci-base last-successful-build`)

Azure Pipelines can coalesce several pushes into one build with `trigger: batch: true`, so a build's
immediate parent commit isn't necessarily the right diff baseline — it might span several pushes'
worth of unrelated changes. `--ci-base last-successful-build` fixes this: it queries the Build API
for the **previous completed+succeeded build** of the same pipeline definition and branch, and uses
that build's own source SHA as the baseline instead of the immediate parent commit.

The source SHA is read from the previous build's `triggerInfo['ci.sourceSha']` when present, otherwise
from its top-level `sourceVersion`. On a standard batched branch trigger Azure leaves `triggerInfo`
empty (`{}`) and only populates `sourceVersion`, so the `sourceVersion` path is what actually resolves
the baseline in the common case. The resolver prints its Build API query, the resolved build's
id/number, both raw fields it found, and how it chose the base to stderr on every run, so a surprising
baseline needs no manual Build REST API spelunking to diagnose.

```yaml
trigger:
  batch: true
  branches:
    include: [main]

pool:
  vmImage: ubuntu-latest

steps:
  - checkout: self
    fetchDepth: 0
  - pwsh: |
      dotnet-fast affected --ci --ci-base last-successful-build --ci-base-fallback all --format traversal --output-name affected .
    env:
      SYSTEM_ACCESSTOKEN: $(System.AccessToken)   # required — see below
```

**This needs `SYSTEM_ACCESSTOKEN` in the step environment.** Azure doesn't expose the pipeline's
OAuth token to scripts by default — you must pass `$(System.AccessToken)` explicitly (as shown
above), or enable "Allow scripts to access the OAuth token" on the pipeline/job.

**Guard the token failure with `--ci-base-fallback all`.** When the last-successful-build lookup
can't complete — a missing/invalid token, the first build on a new branch, or no prior successful
build — `dotnet-fast` has to choose a fallback baseline. The default (`--ci-base-fallback
previous-commit`) narrows to the parent commit, which on a batched trigger **silently under-builds**:
projects changed in earlier commits of the batch are never processed, and the build stays green while
shipping unverified changes. Pass **`--ci-base-fallback all`** (shown above) so that exact scenario
instead falls back to a conservative *full* build — every project is treated as affected — rather than
a too-narrow one. Use `--ci-base-fallback error` if you would rather fail the build loudly than run
either fallback. (This is separate from `--on-missing-base`, which only governs an unresolvable
explicit `--from`/`--to`/`--base`; it does **not** cover the ci-base lookup, because that path resolves
"successfully" to the narrow parent baseline.) If your affected sets look wrong on a batched pipeline,
check the step's stderr for the ci-base warning first.

`PullRequest`-triggered builds don't need any of this — Azure Pipelines already exposes
`SYSTEM_PULLREQUEST_TARGETBRANCH`, and `--ci` alone compares the merge base of that branch against
`HEAD`, batching or not.

## Build cache RBAC (`cache ensure-access`)

When the cache storage account has shared-key auth disabled, the pipeline's identity needs the
**Storage Blob Data Contributor** data-plane role before `build` can read/write it. Normally that
means a separate `AzureCLI@2` step running `az role assignment create`. `cache ensure-access` does
it with the same credential the build step already has:

```yaml
steps:
  - pwsh: dotnet-fast cache ensure-access .
```

Idempotent (an existing grant is a no-op) and only needs a control-plane role that can create role
assignments (e.g. Owner). Prefer running it **once in a setup job** that everything else depends on
— it's cheapest and keeps the grant off the hot path. Full auth/config detail in
[build cache](build-cache.md#configure-the-cache).

**Calling it from every shard of a `strategy: parallel` matrix is also safe**, and is the right
choice when the job that needs cache access can't depend on a shared setup job (e.g. a test-shard
job that only depends on an earlier affected/plan job, not on whatever job first granted access).
The role-assignment name `ensure-access` requests is a deterministic GUID derived from
account+role+principal, so every shard racing to create it targets the *same* assignment: exactly
one create succeeds, and Azure resolves every other concurrent attempt to `409 RoleAssignmentExists`
— which `dotnet-fast` treats as `already_granted = true`, a success, not an error. This holds even
on the very first cold run against a freshly provisioned cache account, not just steady state.

## Test sharding (`--format ado-matrix`)

```yaml
jobs:
  - job: tests
    strategy:
      parallel: 8
    steps:
      - checkout: self
        fetchDepth: 0
      - pwsh: dotnet-fast cache ensure-access .   # safe to call from every shard — see above
      - pwsh: dotnet-fast test-plan --shard $(System.JobPositionInPhase) --of $(System.TotalJobsInPhase) --settings-dir $(Agent.TempDirectory) | pwsh -
```

Each parallel job computes the same plan independently and runs its own shard — no plan-then-fan-out
job dependency needed. Include `cache ensure-access` in the shard itself (as above) whenever the
shard's cache-restoring steps — `test-plan --restore-from-cache`, or a `build --projects-file` step
in the same job — can't rely on a separate setup job having granted access first. See
[test sharding](test-sharding.md) for `--auto-shards`, cache-miss-driven sharding, and coverage
collection.

## Build sharding (`--format ado-matrix`)

`build --format ado-matrix --auto-shards` partitions the affected build's dependency closure into
topological **layers** (waves) and shards each layer, so `build --layer L --shard I --of N` builds
one wave's slice while its dependencies (earlier waves) restore from the cache. Mainly pays off on a
cold cache or a foundation-project change — a warm PR build is already restore-dominated, so
sharding the build itself buys little there. The exact multi-wave YAML shape is still being
validated against real Azure DevOps pipelines, so it's not reproduced here yet — see
[build cache § Sharding the build across agents](build-cache.md#configure-the-cache) for the current
state.

## Gate stages on "did anything change?" (`--set-variable`)

To skip a whole stage when a change touches nothing, you'd otherwise parse `affected --format count`
and hand-echo a `##vso[task.setvariable ...]` logging command in PowerShell. `affected` emits it for
you: `--set-variable <name>` sets a `true`/`false` job variable from whether any project is affected,
and `--set-count-variable <name>` sets the count as a **cross-stage** output variable (`isOutput=true`).
Add `--exit-zero-on-empty` so an empty set doesn't fail the step.

```yaml
stages:
  - stage: plan
    jobs:
      - job: affected
        steps:
          - checkout: self
            fetchDepth: 0
          - pwsh: |
              dotnet-fast affected --ci --ci-base last-successful-build --ci-base-fallback all `
                --set-variable anyAffected --set-count-variable affectedCount `
                --exit-zero-on-empty --format traversal --output-name affected .
            name: affected            # step name → variable prefix for cross-stage refs
            env:
              SYSTEM_ACCESSTOKEN: $(System.AccessToken)

  - stage: build_and_test
    dependsOn: plan
    # Only run this stage when the plan stage found something affected.
    condition: eq(dependencies.plan.outputs['affected.affected.anyAffected'], 'true')
    jobs: [ ... ]
```

`--set-variable` covers same-job/step gating (`condition: eq(variables.anyAffected, 'true')` in a later
step of the same job); `--set-count-variable` (an output variable) is what a downstream **stage/job**
reads via `dependencies.<stage>.outputs['<job>.<step>.affectedCount']`.

## Putting it together

A batched-trigger main pipeline: scope to what actually changed since the last successful build,
gate the build/test stage on whether anything is affected, restore what's cached, shard the tests, and
grant cache access once in setup.

```yaml
trigger:
  batch: true
  branches:
    include: [main]

pool:
  vmImage: ubuntu-latest

stages:
  - stage: setup
    jobs:
      - job: grant_cache_access
        steps:
          - pwsh: dotnet-fast cache ensure-access .

  - stage: build_and_test
    dependsOn: setup
    jobs:
      - job: affected_build
        steps:
          - checkout: self
            fetchDepth: 0
          - pwsh: |
              dotnet-fast affected --ci --ci-base last-successful-build --ci-base-fallback all `
                --set-variable anyAffected --exit-zero-on-empty `
                --format traversal --output-name affected .
              dotnet-fast build --projects-file affected.proj --fallback --report artifacts .
            env:
              SYSTEM_ACCESSTOKEN: $(System.AccessToken)
      - job: tests
        dependsOn: affected_build
        strategy:
          parallel: 8
        steps:
          - checkout: self
            fetchDepth: 0
          # --all is accepted (and ignored) here, so a shared range string works unchanged.
          - pwsh: dotnet-fast test-plan --shard $(System.JobPositionInPhase) --of $(System.TotalJobsInPhase) --exit-zero-on-empty --settings-dir $(Agent.TempDirectory) | pwsh -
```

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

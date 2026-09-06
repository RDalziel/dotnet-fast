# dotnet-fast in CI

Copy-paste wiring for GitHub Actions, Azure Pipelines, GitLab CI and Jenkins: install the tool, run the lint gate, scope
the build to what changed, and read the exit codes correctly. Flag-by-flag detail is in
[commands.md](commands.md); this page is the plumbing.

## Exit-code contract (read this first)

CI logic hinges on the exit codes, and there are two families. The lint family signals "findings
present" with `1`; the `dotnet format`-compatible verbs signal a verify/apply failure with `2`, the
value real `dotnet format` uses.

| Code | Returned by | Meaning | What CI should do |
|---:|:---|:---|:---|
| `0` | all | clean, or the affected list was written | pass |
| `1` | `dotnet-fast lint` (and bare `dotnet-fast`), `metrics --fail-on-budget`, `dead-dependencies --fail-on-unused` | findings present | **fail the build** |
| `2` | `dotnet-fast format` / `style` / `whitespace` / `analyzers`, and `--verify-no-changes` | verify failed, or a fixer failed | **fail the build** |
| `166` | `affected` | ran fine, **no projects affected** | **skip** the downstream jobs — this is not a failure |

Both `1` and `2` are gate failures, so **test `!= 0`, not a specific value.** The one code that needs
handling on its own is `166`: treat it as "nothing to do", or a no-op change turns the build red.

## Installing the tool in CI

```bash
dotnet tool install -g RDLL.dotnet-fast
```

**Windows x64 only** — see [install.md](install.md). To keep a parallel test matrix from paying a
tool restore on every agent, install once into a directory and cache it:

```bash
dotnet tool install RDLL.dotnet-fast --tool-path ./.dotnet-fast
./.dotnet-fast/dotnet-fast lint path/to/App.sln
```

Key the cache on the pinned version and later jobs skip the install entirely. Each release also
attaches `dotnet-fast-win-x64.exe` and a matching `.sha256` if you would rather fetch the binary
directly — though the NuGet path is the one with a signature ([security.md](security.md)).

> `lint --deep` (real Roslyn analyzers) needs the .NET SDK on the agent. The native `lint`,
> `lint --fix`, `affected`, `metrics` and `dead-code` paths do not.

## The lint gate

```bash
dotnet-fast lint --ci .                   # exit 1 on any finding — this is the gate
dotnet-fast lint --ci . --sarif results.sarif  # also emit SARIF for code scanning
```

With `--ci` on a pull request the report is **scoped to the lines you changed** by default, so a branch that lags
`main` is not failed by pre-existing findings in a file it happens to touch. `--whole-file` reports a
touched file's full backlog; `--changed-lines` forces the same scoping on a push build.

## GitHub Actions

`--ci` detects GitHub Actions on its own: a `pull_request` event diffs the merge base of the base ref
against `HEAD`; a `push` event diffs the event's `before` SHA against `HEAD`. **Use
`fetch-depth: 0`** so that base history exists.

```yaml
name: gate
on:
  push:
    branches: [main]
  pull_request:

jobs:
  gate:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0                 # required: --ci needs base history
      - run: dotnet tool install -g RDLL.dotnet-fast

      # Hard gate: exit 1 fails the job if anything is unformatted or a rule fires.
      - name: Lint
        run: dotnet-fast lint .

      # Lint report to the Security tab. Don't fail the build twice on the same findings.
      - name: Lint (SARIF)
        run: dotnet-fast lint . --sarif results.sarif
        continue-on-error: true
      - uses: github/codeql-action/upload-sarif@v3
        if: always()
        with:
          sarif_file: results.sarif

  affected:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      - run: dotnet tool install -g RDLL.dotnet-fast
      - name: Affected projects
        shell: bash
        run: |
          set +e
          dotnet-fast affected --ci --format json
          code=$?
          if [ "$code" -eq 166 ]; then echo "No projects affected — skipping."; exit 0; fi
          exit $code
```

For a gitflow PR against a non-default base, drop `--ci` in favour of `--pr-base develop`.

## Azure Pipelines

`--ci` detects Azure Pipelines too: a `PullRequest` build diffs the merge base of the target branch;
any other build diffs the parent commit.

```yaml
trigger:
  branches: { include: [main] }
pr:
  branches: { include: ['*'] }

pool:
  vmImage: windows-latest

steps:
  - checkout: self
    fetchDepth: 0                        # required: --ci needs base history

  - script: dotnet tool install -g RDLL.dotnet-fast
    displayName: Install dotnet-fast

  - script: dotnet-fast lint .
    displayName: Lint gate

  # Affected projects — treat skip-code 166 as success.
  - script: |
      dotnet-fast affected --ci --format json
      code=$?
      if [ "$code" -eq 166 ]; then echo "No projects affected — skipping."; exit 0; fi
      exit $code
    displayName: Affected projects
```

For a **batched** trigger (`trigger: batch: true`), one build covers several commits and the
previous-commit baseline is wrong. Add `--ci-base last-successful-build` to compare against the last
green build of the same definition and branch, plus `--on-missing-base all` so an unavailable
baseline conservatively builds everything rather than failing:

```yaml
- pwsh: dotnet-fast affected --ci --ci-base last-successful-build --on-missing-base all --format json
  displayName: Affected projects since the last successful build
  env:
    SYSTEM_ACCESSTOKEN: $(System.AccessToken)
```

That query needs `System.AccessToken` exposed to the step. There are two ways a Build-API baseline
can silently widen to "everything" — a baseline that never advances, and a PR merge SHA that no
longer exists — both described with their fixes in [azure-devops.md](azure-devops.md).

## GitLab CI

`--ci` auto-detects GitLab CI (`GITLAB_CI=true`): a merge-request pipeline
(`CI_PIPELINE_SOURCE=merge_request_event`, which includes merged-results pipelines) diffs the merge
base of `CI_MERGE_REQUEST_TARGET_BRANCH_NAME`..`HEAD`; every other pipeline source (branch push,
schedule, web) diffs against the parent commit. GitLab's default clone is shallow (`GIT_DEPTH: 20`) —
the automatic deepening usually recovers the base, but **set `GIT_DEPTH: 0`** to skip it entirely.

```yaml
format-check:
  image: mcr.microsoft.com/dotnet/sdk:10.0
  variables:
    GIT_DEPTH: 0                         # full history: --ci needs the MR base
  rules:
    - if: $CI_PIPELINE_SOURCE == "merge_request_event"
    - if: $CI_COMMIT_BRANCH == $CI_DEFAULT_BRANCH
  script:
    - dotnet tool install -g RDLL.dotnet-fast
    # Hard gate: non-zero (exit 1) fails the job.
    - dotnet-fast lint .
    # Affected projects — treat skip-code 166 as success.
    - |
      dotnet-fast affected --ci --format json || code=$?
      if [ "${code:-0}" -eq 166 ]; then echo "No projects affected — skipping."; exit 0; fi
      exit "${code:-0}"
```

SARIF on GitLab: write `--sarif results.sarif` and declare it as a job artifact; GitLab's native
code-quality widget expects its own JSON format, so SARIF is for downstream tooling.

## Jenkins

`--ci` auto-detects Jenkins (`JENKINS_URL` set): a **multibranch pipeline** PR build (the Branch
Source plugin's `CHANGE_*` contract) diffs the merge base of `CHANGE_TARGET`..`HEAD`; a plain
branch build (`BRANCH_NAME` only) diffs against the parent commit. Multibranch PR checkouts
typically fetch only the PR head — the target branch ref is recovered by the automatic shallow-clone
deepening, but fetching it up front is simpler and avoids the retry:

```groovy
pipeline {
  agent any
  stages {
    stage('Gate') {
      steps {
        sh 'dotnet tool install -g RDLL.dotnet-fast'
        // PR builds: make sure the target branch ref exists locally for the merge base.
        sh '''
          if [ -n "$CHANGE_TARGET" ]; then
            git fetch origin "+refs/heads/$CHANGE_TARGET:refs/remotes/origin/$CHANGE_TARGET"
          fi
        '''
        // Hard gate: non-zero (exit 1) fails the build.
        sh 'dotnet-fast lint .'
        // Affected projects — treat skip-code 166 as success.
        sh '''
          set +e
          dotnet-fast affected --ci --format json
          code=$?
          if [ "$code" -eq 166 ]; then echo "No projects affected — skipping."; exit 0; fi
          exit $code
        '''
      }
    }
  }
}
```

**Freestyle jobs** (no multibranch plugin) export `JENKINS_URL` but no `CHANGE_*` variables, so
`--ci` treats them as branch builds (parent-commit diff). For a freestyle job that gates PRs, pass
the target explicitly: `dotnet-fast affected --pr-base main --format json`.

SARIF on Jenkins: write `--sarif results.sarif` and feed it to the Warnings NG plugin
(`recordIssues tool: sarif(pattern: 'results.sarif')`), which renders the findings in the build UI.

## AI agents

If the "CI" is an AI coding agent rather than a pipeline, set `DOTNET_FAST_AGENT=1` in its
environment (or pass `--agent`). Output becomes terse and low-token and ends with the exact
`dotnet-fast lint --fix <target>` command, so the agent runs the CLI fixer instead of burning tokens
hand-editing. Exit codes are unchanged, so the same `!= 0` gating applies. See the `--agent` flag
under [global options](commands.md#global-options).

## Estate rollup: one view across every repository

Each repository's own CI already produces a `metrics --format json` report; a **separate, central**
job (a scheduled pipeline, a nightly workflow — not part of any one repository's gate) can combine
them into one estate view with `metrics --merge`. No storage service to run: the reports are files,
`--merge` reads a glob of them.

```yaml
# each repository's existing CI step — name it so the estate can tell repositories apart:
- run: dotnet-fast metrics . --format json --repository-name my-service > metrics.json
- uses: actions/upload-artifact@v4
  with: { name: metrics-my-service, path: metrics.json }

# a separate, central job, after downloading every repository's artifact into artifacts/<repo>/:
- run: dotnet-fast metrics --merge "artifacts/*/metrics.json" --format html > estate.html
```

**Nine (soon eleven) of the ten (soon fourteen) budgets are free from that static pass alone** —
cyclomatic, cognitive, Halstead, lines per file, redundant code, dynamic typing, and (once the
scoreboard grows) lines per member, nesting depth, parameter count, maintainability index; dead code
needs only `--include-dead-code`, a solution-wide static pass. **Three need a test or mutation run
behind `--coverage`/`--mutation` in the per-repository step** — test coverage, CRAP, surviving
mutants — and the estate view shows a column for one of those three only where at least one
repository actually measured it, with no `"-"` filler cell standing in for repositories that did not.
Do not read a blank cell as a pass. See [metrics.md](metrics.md) for the full output shapes and the
schema-compatibility rule `--merge` applies to each file it reads.

## Scoping the rest of the build

```bash
dotnet-fast affected --ci --format traversal --output-name affected .
dotnet-fast build --projects-file affected.proj .
```

- **Skip the rebuild** — [build-cache.md](build-cache.md) covers the Azure Blob remote build cache
  (`build --plan --check` is a scriptable "is it fully cached?" probe).
- **Parallelize the tests** — [test-sharding.md](test-sharding.md) covers NUnit sharding, planned
  from source before any test assembly is built.
- **Gate code health** — [metrics.md](metrics.md) covers `metrics --fail-on-budget` and the baseline
  ratchet for a codebase that starts out over budget.

`--all` accepts an optional boolean (`--all=$(forceFullBuild)`), so a pipeline parameter becomes an
argument rather than an `if`/`else` around the step.

## Affected manifest (compute once, reuse)

In a multi-stage pipeline, each of `lint`, `build`, and `test-plan` will otherwise recompute the
affected set. `affected --emit-manifest <file>` writes the affected analysis once — change range,
affected projects, the test subset, and the changed files — as a single JSON artifact that the other
commands consume via `--projects-file`:

```yaml
# Stage 1 — compute affected once and publish the manifest.
- run: dotnet-fast affected --ci --emit-manifest affected.manifest.json .

# Later stages — reuse it; no second diff/discovery pass.
- run: dotnet-fast build --projects-file affected.manifest.json .
- run: dotnet-fast test-plan --projects-file affected.manifest.json --auto-shards --format ado-matrix .
```

The manifest is `{ schemaVersion, repositoryPath, range:{from,to,mergeBase}, changedFiles,
affectedProjects:[{name,path,isTestProject}], testProjects:[...] }`. Consumers read the `path` keys, so
`test-plan` keeps only the test subset on its own; the explicit `testProjects` list makes the artifact
self-describing for other tooling.

## Minimal-fetch mode (drop `fetch-depth: 0`)

`fetch-depth: 0` is the simplest way to give `--ci` the base history, but it fully unshallows the repo.
On a large repo, pass `--fetch-base` (alias `--minimal-fetch`) instead: on a shallow clone the tool
fetches only the base ref and deepens it incrementally until the merge base is reachable, rather than
unshallowing everything. The CI checkout can then keep its default shallow depth.

```yaml
- uses: actions/checkout@v4   # no fetch-depth: 0
- run: dotnet-fast lint --ci --fetch-base .
```

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

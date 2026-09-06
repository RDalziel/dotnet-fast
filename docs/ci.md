# dotnet-fast in CI

Copy-paste wiring for GitHub Actions and Azure Pipelines: install the tool, run the lint gate, scope
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

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

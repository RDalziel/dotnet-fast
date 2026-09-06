# dotnet-fast documentation

Every page, grouped by what you are trying to do. If you only read one, read
[commands.md](commands.md) — it is the full reference for every command and flag.

## Start here

| Page | What it covers |
|---|---|
| [install.md](install.md) | Install, first run, verify the download, update, uninstall — transcribed from a real pass. |
| [commands.md](commands.md) | The full command reference: every command, every option, exit codes. |
| [ci.md](ci.md) | Wiring the lint gate, affected scoping and the exit-code contract into GitHub Actions, Azure Pipelines, GitLab CI or Jenkins; the affected manifest and minimal-fetch mode. |
| [../examples/](../examples/) | Small runnable projects to point the tool at. |

## Lint & format

| Page | What it covers |
|---|---|
| [rules.md](rules.md) | The native `DFxxxx` rule catalog: every rule `lint` reports by default, with its `--fix` status and how to suppress it. |
| [deep-linting.md](deep-linting.md) | `lint --deep` — running your project's real Roslyn analyzers, and when it is fast enough to leave on. |
| [ported-analyzers.md](ported-analyzers.md) | Popular Roslyn analyzers re-implemented natively, so they run on the fast path without `--deep`. |
| [guardrails.md](guardrails.md) | The seven opt-in rules for repositories where an agent writes the code and a human reviews it. |
| [code-scanning.md](code-scanning.md) | SARIF output and a working GitHub code-scanning upload workflow. |
| [editorconfig.md](editorconfig.md) | How `.editorconfig`/`.globalconfig` resolves: the chain, severity precedence, and the two line-budget systems that don't agree. |
| [editorconfig-keys.md](editorconfig-keys.md) | Every recognised `.editorconfig` key — value shape, default, one line of meaning. |

## Code health

| Page | What it covers |
|---|---|
| [metrics.md](metrics.md) | The ten-budget scoreboard: complexity, coverage, CRAP, surviving mutants, dead code — and how to gate CI on it. |
| [dead-code.md](dead-code.md) | Solution-wide unused types and members, with a separate test-only category. |
| [dead-dependencies.md](dead-dependencies.md) | Unused `PackageReference`s and `ProjectReference`s, and the build-verified fix. |
| [bom.md](bom.md) | Software Bill of Materials — CycloneDX or SPDX, without a build. |
| [insights.md](insights.md) | Analytics over recorded build, test and lint history: hotspots, trends, regressions. |

## CI accelerators

| Page | What it covers |
|---|---|
| [build-cache.md](build-cache.md) | The Azure Blob remote build cache: restore prior outputs instead of rebuilding. |
| [test-sharding.md](test-sharding.md) | Splitting an NUnit suite across parallel agents, planned from source before anything is built. |
| [azure-devops.md](azure-devops.md) | The Azure Pipelines specifics: batched-trigger baselines, cache RBAC, `ado-matrix` sharding. |

## What you can rely on

| Page | What it covers |
|---|---|
| [support-matrix.md](support-matrix.md) | The frozen supported/unsupported matrix: input shapes, platforms, parity floors, rule coverage. |
| [versioning.md](versioning.md) | What SemVer means here, and the deprecation policy for CLI changes. |
| [benchmarks.md](benchmarks.md) | How every speed claim is measured, on what hardware — and where the margin is smallest. |
| [releasing.md](releasing.md) | How a release is produced and which gates it clears before it is tagged. |
| [security.md](security.md) | What is signed and checksummed, and what is deliberately *not* claimed. |
| [../RELEASES.md](../RELEASES.md) | Plain-English release notes, newest first. |

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

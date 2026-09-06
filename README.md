# dotnet-fast — fast .NET formatter, linter, build cache & test sharding

[![NuGet](https://img.shields.io/nuget/v/RDLL.dotnet-fast.svg?logo=nuget)](https://www.nuget.org/packages/RDLL.dotnet-fast)
[![NuGet downloads](https://img.shields.io/nuget/dt/RDLL.dotnet-fast.svg)](https://www.nuget.org/packages/RDLL.dotnet-fast)
[![License: Freeware](https://img.shields.io/badge/License-Freeware-blue.svg)](https://github.com/RDalziel/dotnet-fast/blob/main/LICENSE.txt)
[![Buy Me A Coffee](https://img.shields.io/badge/Buy%20Me%20A%20Coffee-support-FFDD00?logo=buymeacoffee&logoColor=black)](https://buymeacoffee.com/rdll)

**Rust-powered .NET workspace tooling: a `dotnet format` alternative, a C# linter, a code-health
scoreboard, and CI accelerators** — Git affected-project detection, an Azure Blob remote build cache,
and NUnit test sharding for Azure DevOps and GitHub Actions. The native paths load no MSBuild and no
Roslyn, so they start in milliseconds and run common jobs 10–100× faster than the official tools
([how that is measured](https://github.com/RDalziel/dotnet-fast/blob/main/docs/benchmarks.md)); the
documented exception is `lint --deep`, which runs your project's *real* Roslyn analyzers.

## Install

```bash
dotnet tool install -g RDLL.dotnet-fast
dotnet tool update  -g RDLL.dotnet-fast   # already installed? update to the latest
```

**Windows x64 only, and the .NET 10 SDK is a hard floor.** The install *succeeds* on Linux and macOS —
it is a portable .NET tool package — but the first run fails, because only a `win-x64` native binary
ships. Prerequisites, the repo-pinned manifest form, verifying the download, updating and uninstalling:
[install.md](https://github.com/RDalziel/dotnet-fast/blob/main/docs/install.md).

## First run

```bash
dotnet-fast lint path/to/App.sln          # report findings, non-zero exit on any (the CI gate)
dotnet-fast lint --fix path/to/App.sln    # apply every safe fix in one pass
dotnet-fast metrics path/to/App.sln       # score the codebase against ten code-health budgets
```

## In CI

```yaml
- run: dotnet tool install -g RDLL.dotnet-fast
- run: dotnet-fast lint --ci .                             # exit 1 on any finding — the gate
- run: dotnet-fast affected --ci --format traversal --output-name affected .
```

Exit codes: `0` clean, `1` findings, `2` a `dotnet format`-compat verify/apply failure, `166` no
projects affected (skip the downstream jobs — not a failure). Test `!= 0`, and handle `166`. On a pull
request `--ci` scopes the lint report to the lines you changed, so old debt in a touched file does not fail
the build. Copy-paste Actions and Azure Pipelines wiring:
[ci.md](https://github.com/RDalziel/dotnet-fast/blob/main/docs/ci.md).

## Commands

| Command | What it's for |
|---|---|
| [`lint`](https://github.com/RDalziel/dotnet-fast/blob/main/docs/commands.md#lint) | Report formatting + lint findings (the fast CI gate). `--fix` applies the safe fixes; `--deep` adds your real Roslyn analyzers. |
| [`metrics`](https://github.com/RDalziel/dotnet-fast/blob/main/docs/commands.md#metrics) | Score a codebase against ten budgets: complexity, coverage, CRAP, surviving mutants, dead code. |
| [`affected`](https://github.com/RDalziel/dotnet-fast/blob/main/docs/commands.md#affected) | List the projects a Git change set affects, plus their reverse-dependents, for scoped CI. |
| [`build`](https://github.com/RDalziel/dotnet-fast/blob/main/docs/commands.md#build) | Remote build cache (preview): restore prior build outputs instead of rebuilding on a clean checkout. |
| [`test-plan`](https://github.com/RDalziel/dotnet-fast/blob/main/docs/commands.md#test-plan) | NUnit test sharding for CI agents, planned from source before any test assembly is built. |
| [`dead-code`](https://github.com/RDalziel/dotnet-fast/blob/main/docs/commands.md#dead-code) | Types and members nothing in production reaches, plus a distinct test-only category. |
| [`dead-dependencies`](https://github.com/RDalziel/dotnet-fast/blob/main/docs/commands.md#dead-dependencies) | Unused `PackageReference`s and `ProjectReference`s, with an optional build-verified fix. |
| [`bom`](https://github.com/RDalziel/dotnet-fast/blob/main/docs/commands.md#bom) | Software Bill of Materials — CycloneDX or SPDX — without a build. |
| [`doctor`](https://github.com/RDalziel/dotnet-fast/blob/main/docs/commands.md#doctor) | Build-free scan for common workspace problems: duplicate references, CPM conflicts, lock-file drift. |
| [`insights`](https://github.com/RDalziel/dotnet-fast/blob/main/docs/commands.md#insights) | Analytics over recorded build/test/lint history: hotspots, trends, regressions, HTML report. |
| [`hooks`](https://github.com/RDalziel/dotnet-fast/blob/main/docs/commands.md#hooks) | Install Git hooks that run `lint --fix --staged` on commit — no husky or lint-staged needed. |
| [`editorconfig`](https://github.com/RDalziel/dotnet-fast/blob/main/docs/commands.md#editorconfig) | Explain which `.editorconfig` settings apply to a file, infer one from your code, or seed a profile. |
| [`cache`](https://github.com/RDalziel/dotnet-fast/blob/main/docs/commands.md#cache) | Grant the build cache's storage account the data-plane role CI needs, without a separate az step. |
| [`update`](https://github.com/RDalziel/dotnet-fast/blob/main/docs/commands.md#update) | Update the tool using the `dotnet tool update` form that matches how your copy was installed. |

`format`, `style`, `whitespace` and `analyzers` are accepted as `dotnet format`-compatible aliases.
Add `--json` to any command for machine-readable results and timing; `lint`, `doctor`, `metrics` and
`dead-dependencies` also emit SARIF for GitHub code scanning. `lint` ships 153 native CST rules (`DF0001`–`DF0153`), each listed with its `--fix` status on the
[rules page](https://github.com/RDalziel/dotnet-fast/blob/main/docs/rules.md), and a large set of popular Roslyn analyzers
is re-implemented natively and runs on the fast path **by default** — no `--deep` needed.

## Documentation

**[Every page, grouped by task →](https://github.com/RDalziel/dotnet-fast/blob/main/docs/README.md)**
— install, the command reference, CI wiring, code health, the build cache, test sharding, the support
matrix, benchmarks, the security position. Plus runnable
[`examples/`](https://github.com/RDalziel/dotnet-fast/tree/main/examples).

## Issues & feedback

Found a bug, a formatting mismatch vs `dotnet format`, or want a feature?
**[Open an issue](https://github.com/RDalziel/dotnet-fast/issues/new/choose)** — this repo is the
user-facing home for `dotnet-fast` and where reports are triaged. The implementation lives in a
separate repository; you don't need it to use the tool.

## Release notes

Plain-English notes for each version:
**[RELEASES.md](https://github.com/RDalziel/dotnet-fast/blob/main/RELEASES.md)**. Each release attaches
`dotnet-fast-win-x64.exe` and a matching `.sha256`, and each version of these docs is tagged `vX.Y.Z`.

## License

Free to use under a freeware license — see
[LICENSE](https://github.com/RDalziel/dotnet-fast/blob/main/LICENSE.txt). The Software is distributed
as a binary; its source is proprietary and not published. Use is permitted (including commercially
within your own organization); redistribution, resale, modification and reverse engineering are not.
Not affiliated with Microsoft or the .NET Foundation.
[**Buy me a coffee** ☕](https://buymeacoffee.com/rdll) if it speeds up your builds — it genuinely helps.

# dotnet-fast — fast .NET formatter, linter, build cache & test sharding

[![NuGet](https://img.shields.io/nuget/v/RDLL.dotnet-fast.svg?logo=nuget)](https://www.nuget.org/packages/RDLL.dotnet-fast)
[![NuGet downloads](https://img.shields.io/nuget/dt/RDLL.dotnet-fast.svg)](https://www.nuget.org/packages/RDLL.dotnet-fast)
[![License: Freeware](https://img.shields.io/badge/License-Freeware-blue.svg)](LICENSE.txt)
[![Buy Me A Coffee](https://img.shields.io/badge/Buy%20Me%20A%20Coffee-support-FFDD00?logo=buymeacoffee&logoColor=black)](https://buymeacoffee.com/rdll)

**Fast .NET workspace tooling — format, lint, affected-project detection, remote build caching, and
NUnit test sharding.** A Rust-powered, instant-start alternative to `dotnet format` and a C# linter,
plus CI accelerators (Git affected-project detection, an Azure Blob remote build cache, and NUnit test
sharding for Azure DevOps and GitHub Actions). It starts instantly for the native paths and runs common
jobs 10–100× faster than the official tools. Add `--json` to any command for machine-readable results
and timing.

> **This is the user-facing home for `dotnet-fast`** — docs, samples, release notes, and the place to
> [open an issue](https://github.com/RDalziel/dotnet-fast/issues). The implementation lives in a separate
> repository; you don't need it to use the tool.

## What it does

| Command | What it's for |
|---|---|
| `dotnet-fast lint` | Report formatting + lint findings (the fast CI gate). `--fix` applies the safe fixes. |
| `dotnet-fast format` | Apply whitespace/style formatting (a `dotnet format`-compatible path). |
| `dotnet-fast affected` | List the projects a Git change set affects (+ their reverse-dependents) — for scoped CI. |
| `dotnet-fast build` | Preview remote build cache for CI: restore cached build outputs, build misses, upload trusted artifacts. |
| `dotnet-fast test-plan` | NUnit test sharding for CI agents, discovered from source before test assemblies are built. |
| `dotnet-fast doctor` | Fast, build-free scan for common workspace problems. |
| `dotnet-fast lint --deep` | Opt in to running the project's *real* Roslyn analyzers (needs the .NET SDK). |

## Install

Install the global .NET tool from NuGet.org:

```bash
dotnet tool install -g RDLL.dotnet-fast
dotnet tool update  -g RDLL.dotnet-fast   # already installed? update to the latest
```

### Supported platforms

**Windows x64 is the validated platform**: every release runs the full parity, corpus, and
whole-repository verification suites on Windows against the real `dotnet format` before shipping.
Other platforms (linux-x64, macOS) are **experimental and unvalidated** — the tool is native Rust
and generally builds there, but no release gate exercises them yet; treat results accordingly.
Validated Linux support is planned for the 1.x line.

To verify a downloaded release binary, compare its SHA-256 against the published `.sha256` asset:
`Get-FileHash dotnet-fast-win-x64.exe` (PowerShell) or `sha256sum` (Linux/macOS).

### Standalone binary (CI matrices)

`dotnet-fast` is a native binary, so each tagged release also ships a
self-contained executable as a GitHub release asset (`dotnet-fast-<rid>.exe` plus
a `.sha256`). Download/cache it once and invoke `./dotnet-fast` directly — handy
on a parallel test matrix where every agent would otherwise pay `dotnet tool
restore`:

```bash
curl -sSL -o dotnet-fast.exe \
  https://github.com/RDalziel/dotnet-fast/releases/latest/download/dotnet-fast-win-x64.exe
```

## Quick start

```bash
dotnet-fast lint path/to/App.sln          # report findings, non-zero exit on any (the CI gate)
dotnet-fast lint --fix path/to/App.sln    # apply every safe fix in one pass
dotnet-fast affected --ci                 # which projects to build/test for this change
dotnet-fast build --plan --check .        # can the build be restored from cache?
dotnet-fast build --projects-file affected.proj .
dotnet-fast build --json .                # restored/built/bytes report
dotnet-fast build --report artifacts .    # writes artifacts/build-report.json
dotnet-fast test-plan --shards 8 --format matrix .
dotnet-fast test-plan --shards 8 --format ado-matrix .
dotnet-fast test-plan --cache-misses-file build-cache-plan.json --auto-shards --format matrix .
dotnet-fast doctor                        # what's wrong with this workspace?
```

`affected` and every command that consumes affected ranges (`lint`, `build`, `test-plan`) can use
`--from`/`--to`; shallow checkouts deepen and retry when an explicit revision is missing. Azure
Pipelines batched branch builds can use
`dotnet-fast affected --ci --ci-base last-successful-build --on-missing-base all`.

See **[docs/commands.md](docs/commands.md)** for the full command reference and **[docs/deep-linting.md](docs/deep-linting.md)**
for the deep (real-Roslyn) analyzer mode and when it's fast enough to turn on by default.
**[docs/ported-analyzers.md](docs/ported-analyzers.md)** lists the popular Roslyn analyzers re-implemented
as native, opt-in rules (no `--deep` needed). Build/test
CI guides are in **[docs/build-cache.md](docs/build-cache.md)** and
**[docs/test-sharding.md](docs/test-sharding.md)**; Azure Pipelines-specific capabilities (batched-trigger
baselines, cache RBAC, `ado-matrix` sharding) are tied together in **[docs/azure-devops.md](docs/azure-devops.md)**.
`lint`/`format`/`affected`/`doctor` all write SARIF for GitHub code scanning — see
**[docs/code-scanning.md](docs/code-scanning.md)** for a working upload workflow. Solution-wide unused-code
detection is covered in **[docs/dead-code.md](docs/dead-code.md)**, and unused-dependency detection
(unused packages/project references) in **[docs/dead-dependencies.md](docs/dead-dependencies.md)**.
CycloneDX Software Bill of Materials generation for a project or a whole solution is in
**[docs/bom.md](docs/bom.md)**. The frozen supported/unsupported matrix — input shapes, platforms,
formatter parity floors, and native-lint coverage — is in **[docs/support-matrix.md](docs/support-matrix.md)**,
and the versioning/compatibility promise (what SemVer means here, and the deprecation policy for CLI
changes) is in **[docs/versioning.md](docs/versioning.md)**.

## Samples

[`examples/`](examples/) has small, runnable projects you can point the tool at.

## Issues & feedback

Found a bug, a formatting mismatch vs `dotnet format`, or want a feature?
**[Open an issue](https://github.com/RDalziel/dotnet-fast/issues/new/choose)** — bug reports and feature
requests are both welcome. This repo is where we collect and triage them.

## Release notes

Plain-English notes for each version are in **[RELEASES.md](RELEASES.md)**.

## FAQ

**Is this a `dotnet format` replacement?** `dotnet-fast format` / `lint --fix` is a fast,
`dotnet format`-compatible path for whitespace and style fixes, plus a native C# linter. The native
rules are **syntactic** (no type information) — that's why it starts in milliseconds with no
MSBuild/Roslyn load. Rules that need semantics (e.g. unused-`using` removal) come from `lint --deep`,
which runs your project's real Roslyn analyzers.

**Can it speed up `dotnet test` on Azure DevOps?** Yes — `dotnet-fast test-plan` discovers NUnit tests
from source (no build) and emits a balanced shard matrix so you can run tests in parallel across CI
agents, including the Azure DevOps `parallel`/`matrix` strategies and GitHub Actions.

**Does the build cache work with GitHub Actions / Azure DevOps?** `dotnet-fast build` is a remote build
cache backed by Azure Blob Storage (managed identity / Entra) — restore cached build outputs on a clean
checkout instead of rebuilding. See [docs/build-cache.md](docs/build-cache.md).

**Which test frameworks are supported for sharding?** NUnit today (xUnit/MSTest planned). See
[docs/test-sharding.md](docs/test-sharding.md).

## Support

`dotnet-fast` is free to use (see [LICENSE](LICENSE.txt)). If it speeds up your builds and CI, you can
[**buy me a coffee** ☕](https://buymeacoffee.com/rdll) — it genuinely helps and is much appreciated.

## License

Free to use under a freeware license — see [LICENSE](LICENSE.txt). The Software is distributed as a binary;
its source code is proprietary and not published. Use is permitted (including commercially within your
own organization); redistribution, resale, modification, and reverse engineering are not. `dotnet-fast`
is not affiliated with Microsoft or the .NET Foundation.

# dotnet-fast — fast .NET formatter, linter, build cache & test sharding

[![NuGet](https://img.shields.io/nuget/v/RDLL.dotnet-fast.svg?logo=nuget)](https://www.nuget.org/packages/RDLL.dotnet-fast)
[![NuGet downloads](https://img.shields.io/nuget/dt/RDLL.dotnet-fast.svg)](https://www.nuget.org/packages/RDLL.dotnet-fast)
[![License: Freeware](https://img.shields.io/badge/License-Freeware-blue.svg)](LICENSE.txt)
[![Buy Me A Coffee](https://img.shields.io/badge/Buy%20Me%20A%20Coffee-support-FFDD00?logo=buymeacoffee&logoColor=black)](https://buymeacoffee.com/rdll)

**Fast .NET workspace tooling — format, lint, affected-project detection, remote build caching, and
NUnit test sharding.** A Rust-powered, instant-start alternative to `dotnet format` and a C# linter,
plus CI accelerators (Git affected-project detection, an Azure Blob remote build cache, and NUnit test
sharding for Azure DevOps and GitHub Actions). It starts instantly for the native paths and runs common
jobs 10–100× faster than the official tools ([how that is measured, and where the margin is
smallest](docs/benchmarks.md)). Add `--json` to any command for machine-readable results
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

Needs the **.NET 10 SDK** — on .NET 9 the install fails with a message that doesn't say why.
**[docs/install.md](docs/install.md)** is the full walkthrough: prerequisites, the repo-pinned local
manifest form, a first run with real output, verifying the download, updating, and uninstalling.

### Supported platforms — Windows x64

**Windows x64 is the only supported platform, and the only one a binary ships for.** Every release is
built and verified there — the test suite, the CLI-contract goldens and the differential parity
fixtures against the real `dotnet format` all run on a Windows x64 machine before a release is tagged.
The heavier whole-repository sweeps are run for formatter changes; which gate runs when is set out in
[docs/releasing.md](docs/releasing.md).

**Linux and macOS do not work today** — worth saying plainly, because the install step won't warn you.
`dotnet tool install -g` *succeeds* on both, since the package is a portable .NET tool; the first run
then fails with `dotnet-fast native binary was not found` and exit code 1, because the package carries
a `win-x64` native binary only. No published artifact runs on those platforms. linux-x64 is the first
target for the 1.x line, macOS after it —
a platform counts as supported when a release note says so. Detail:
[docs/support-matrix.md](docs/support-matrix.md#platform).

### Verifying what you installed

The NuGet package is repository-signed by nuget.org — check it with
`dotnet nuget verify <package>.nupkg --all` (exit code `0` is the result; there is no success line at
the default verbosity). That signature is the only integrity artifact on a release, and it comes from
nuget.org rather than from us. **[docs/security.md](docs/security.md)** has the exact commands, what
each check does and doesn't prove, and the 1.0 position on provenance and attestation.

### Avoiding a tool restore per CI agent

There is **no standalone binary download today** — NuGet is the only channel a release is published to.
A downloadable release asset is a 1.x item.

To keep a parallel Windows test matrix from paying `dotnet tool restore` on every agent, install once
into a directory and cache that directory:

```bash
dotnet tool install RDLL.dotnet-fast --tool-path ./.dotnet-fast
./.dotnet-fast/dotnet-fast lint path/to/App.sln
```

Cache `./.dotnet-fast` with your CI's cache action keyed on the pinned version, and later jobs skip the
install entirely.

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
as native, opt-in rules (no `--deep` needed). If an agent writes some of your code, the
**[docs/guardrails.md](docs/guardrails.md)** covers the four opt-in rules for that case — no explanatory
comments, a line budget per method and per file, no magic numbers. They exist because an agent will
ignore a style guide but cannot ignore a lint error in CI, and their messages are written to tell the
agent which design move to make. Build/test
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
changes) is in **[docs/versioning.md](docs/versioning.md)**. How a release is actually produced — what
each one ships, and the parity and contract gates it clears before it is tagged — is in
**[docs/releasing.md](docs/releasing.md)**, and what is signed, checksummed, and *not* claimed about
build provenance is in **[docs/security.md](docs/security.md)**.

Every speed claim on this page is backed by **[docs/benchmarks.md](docs/benchmarks.md)** — what is
measured, against which tool and version, on what hardware, how the comparison is kept fair, the
commands to reproduce it yourself, and the scenarios where the margin is smallest.

## Samples

[`examples/`](examples/) has small, runnable projects you can point the tool at.

## Issues & feedback

Found a bug, a formatting mismatch vs `dotnet format`, or want a feature?
**[Open an issue](https://github.com/RDalziel/dotnet-fast/issues/new/choose)** — bug reports and feature
requests are both welcome. This repo is where we collect and triage them.

## Release notes

Plain-English notes for each version are in **[RELEASES.md](RELEASES.md)**. How releases are made — what
ships, the gates every version clears before it is tagged, and where to find the notes for a specific
version — is in **[docs/releasing.md](docs/releasing.md)**.

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

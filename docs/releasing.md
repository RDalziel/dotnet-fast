# How releases are made

This is the published account of how a `dotnet-fast` release is produced and what comes with it. It is
not a build-from-source guide — the tool ships as a binary and its source is proprietary
([LICENSE](../LICENSE.txt)) — it is here so you can judge what "a new version shipped" actually means
before you pin one in CI.

## Version numbers

`dotnet-fast` follows [Semantic Versioning](https://semver.org/). What each segment means here — and the
CLI-contract and deprecation rules that decide which segment moves — is in
**[versioning.md](versioning.md)**; that page is the compatibility promise, this one is the process
behind it.

One version identifies everything in a release: the NuGet package version, the release tag (`vX.Y.Z` —
the same number with a `v` prefix), and what `dotnet-fast --version` prints (`1.0.0`, bare) all refer
to the same build. There is no separate build number, and a published version is never re-cut with
different content.

The stable line is **`1.0.0`**. The compatibility rules in [versioning.md](versioning.md) were already
how we worked before it; from `1.0` they are a formal promise rather than a working practice, and a
break in them costs a major version.

A pre-release version carries a suffix (`1.0.0-rc.1`) and is published as a **pre-release** on both
NuGet and GitHub, so `dotnet tool install` and the GitHub "latest release" link keep resolving the
stable line until the final version ships. Installing one is always explicit — see
[RELEASES.md](../RELEASES.md) for the commands.

## What a release ships

- **The NuGet package `RDLL.dotnet-fast`** — the supported way to install and update, and the only
  channel a release is published to
  (`dotnet tool install -g RDLL.dotnet-fast` / `dotnet tool update -g RDLL.dotnet-fast`).
- **A plain-English entry in [RELEASES.md](../RELEASES.md)** for anything user-visible, with anything
  action-required stated first.
- **Updated documentation on this site** when the command surface or the lint rule catalog changed.

A self-contained `dotnet-fast-win-x64.exe` is built from the same commit and checksummed, but **it is
not currently published anywhere you can download it** — NuGet is the only distribution channel today.
Publishing it as a downloadable release asset is a 1.x item. Until then, if you want a tool-restore-free
binary on a CI agent, install once with `dotnet tool install RDLL.dotnet-fast --tool-path <dir>` and
cache `<dir>`; see the [README](../README.md).

### Integrity

**The NuGet package is the artifact with an integrity story, and it is not one we produce ourselves** —
nuget.org repository-signs the package on ingestion, which anyone can check with `dotnet nuget verify`.
We publish no author signature, no code signature, and no build-provenance attestation. The full
position for `1.0`, including why build attestation and a SLSA level are *not* claimed, is in
**[security.md](security.md)**.

## The gates a release passes before it is tagged

These run *before* the version is tagged and published, so a failure means no release — not a release
followed by a hotfix.

Not every gate is the same kind of thing, and it would be misleading to present them as one list. Some
are automated and run on every single release; others are run deliberately, when the change is the kind
that can break what they check. Both are stated below, labelled.

### Automated, every release

The release command runs these itself and stops on the first failure:

- **Build hygiene and the full automated test suite** — a warning-free build and lint of the tool's own
  source, then the complete unit and CLI integration suite.
- **CLI-contract golden tests** — every command's help text, output shape, and exit codes are pinned by
  golden files. Changing any of them fails the suite rather than reaching your pipeline. This is the
  mechanical half of the compatibility promise in [versioning.md](versioning.md). Part of the test
  suite, so it runs on every test run too.
- **The parity-regression ledger** — every formatting divergence anyone reports becomes a permanent test
  case: the reported input, plus the real `dotnet format` output as the expectation, generated from a
  compilable probe. The fix always goes into the formatter, never into the expectation. This suite runs
  on *every* test run, not only on formatter changes, so a divergence that has been reported once cannot
  come back quietly.
- **Differential formatter parity against the real `dotnet format`** — 146 fixtures are formatted by
  `dotnet-fast` and by the real `dotnet format` from the installed .NET SDK (10.x), then byte-compared.
  The expectation is the oracle's actual output, not a hand-written file.
- **Documentation drift** — a test asserts that every native rule that ships is documented, that the
  published `fix` / `report-only` marker for each one matches what the code actually does, and that the
  rule counts quoted in the docs — including the verbatim `lint --list-rules` header on
  [install.md](install.md) — match the catalog. A rule added, removed, or made unfixable without the
  documentation following fails the build rather than shipping a page that lies. (The `install.md`
  transcript was NOT covered until 2026-08-03: withdrawing RCS0062's autofix dropped the fixable total
  from 145 to 144 and the page shipped stale with a green suite. That gap is what the check now closes.)

There is an escape hatch on the parity gate for releases that cannot touch the formatter (a docs-only
or packaging-only cut). Using it prints a loud warning naming the version that shipped without parity
verification. It is not used for a change that touches formatting.

### Run deliberately, when the change warrants it

These are not run by the release command. They are heavier, and each one is run when a change is of the
kind it can catch:

- **A whole-repository parity sweep, for formatter changes** — five real open-source repositories
  (Newtonsoft.Json, Polly, Dapper, AutoMapper, Serilog) are formatted end to end by both tools and
  compared file by file. Each repository has a published floor in
  **[support-matrix.md](support-matrix.md)**; a change that would drop any of them below its floor does
  not ship as-is. The floors on that page carry the date they were last measured. The sweep also
  verifies that both tools processed the *same* files and fails when they did not — a percentage
  computed over files only one tool opened is not evidence of parity. As of 2026-08-03 four of the five
  clear that check: Dapper started clearing it once the `#if`-region parsing defects behind its one-sided
  files were fixed, and Newtonsoft.Json now fails it on one file for a documented, pre-existing
  final-line edge case that does not affect its parity figure. Both are explained in
  [support-matrix.md](support-matrix.md).
- **Real-world fix safety, for changes to any automatic fix** — a set of open-source repositories is
  fixed and then *compiled*. Fixed code that no longer builds is the failure mode that fixture corpora
  cannot see (preprocessor directives, `#pragma` regions, and parse edge cases are the recurring
  culprits), so the compile step is the gate that catches it.
- **The benchmark floor, for performance-affecting changes** — the medium and large tiers are re-run
  with output verification and fail if any scenario drops below 2× versus `dotnet format`. The
  methodology, and the date of the last recorded run, are in **[benchmarks.md](benchmarks.md)**. A
  published speed number is verified as of its stated date; it is not re-verified on every release.

Two things worth stating plainly:

- **The parity gates need a real .NET SDK, and the oracle is not perfectly deterministic.** On at least
  one known input shape, `dotnet format` itself settles on two different results across runs of
  identical code. Where that happens the affected fixture is re-run in isolation and the release
  proceeds only if it then passes cleanly; the expectation is never relaxed to make a failure go away.
- **Every gate, and the packaging, runs on Windows x64 — there is no non-Windows step anywhere.** That
  is why Windows x64 is the only supported platform: the package carries a `win-x64` native binary only,
  so a Linux or macOS install succeeds and then fails on first run. Read
  [support-matrix.md](support-matrix.md#platform) before planning around either.

## Publishing

- **NuGet publication uses NuGet's Trusted Publishing (OIDC).** The publish job exchanges a short-lived
  identity token for a single-use API key valid for one hour. There is no long-lived NuGet API key for
  this package — nothing stored to leak, nothing to rotate.
- **Nothing publishes itself.** No commit, merge, or schedule triggers a release; every publish is
  started deliberately by the maintainer.
- **Dependency advisories are scanned daily**, independently of release cadence (RustSec advisory
  database, plus license and source policy). A vulnerability disclosed between releases surfaces within
  a day rather than at the next release.

## Cadence

There is no fixed release train. Releases go out when a feature wave or a fix is ready, which in
practice means patch and minor releases land often. A reproducible user-reported bug is normally fixed
in the next release rather than held for a milestone — [0.306.1](../RELEASES.md) is a worked example of
that path, including what it changed and why.

Ahead of a significant release, a pre-release may be published under an `-rc` suffix (for example
`1.0.0-rc.1`) so it can be validated in real pipelines before the final tag. Pre-releases are outside
the compatibility promise — see [versioning.md](versioning.md#pre-release-channel).

## Finding the notes for a version

- **[RELEASES.md](../RELEASES.md)** — plain-English notes, newest first, with anything action-required
  called out at the top of the entry.
- **[nuget.org/packages/RDLL.dotnet-fast](https://www.nuget.org/packages/RDLL.dotnet-fast)** — the
  version history of what is actually live, including any pre-releases.
- **`dotnet-fast --version`** — the bare version you are running right now, matching a `RELEASES.md`
  heading.

If a release note doesn't explain a change you can see, that's a bug in the notes:
**[open an issue](https://github.com/RDalziel/dotnet-fast/issues/new/choose)** and it'll be corrected.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

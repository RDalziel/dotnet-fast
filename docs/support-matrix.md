# Support matrix

What `dotnet-fast` supports today, what it explicitly doesn't, and where the boundaries are. This is
the frozen reference for the 1.0 release — if a capability isn't listed here as supported, treat it as
unsupported until a release note says otherwise.

## Supported input

- **SDK-style C# projects and solutions** — a `.csproj`/`.sln`/`.slnx`/`.slnf` rooted in a
  `<Project Sdk="...">` (or an `<Import Sdk="...">`). This is what every command is built and verified
  against.
- **Non-SDK-style (legacy) `.csproj`** is detected and handled narrowly rather than crashing: a project
  with no `Sdk` attribute doesn't get the SDK's implicit default-item globbing (only files it lists
  explicitly are seen), and commands that need a reliable dependency graph (`dead-dependencies`) skip
  such a project outright with a stated `"non-sdk-style"` reason rather than guessing. It is not a
  validated, supported input shape — see **Unsupported** below.

## Platform

**Windows x64 is the only supported platform, and the only one anything ships for.**

- **Tested — Windows x64.** Every release is built, packaged, and verified there: the test suite, the
  CLI-contract goldens and the differential parity fixtures against the real `dotnet format` run before
  shipping, the heavier whole-repository sweeps run for formatter changes
  ([which gate runs when](releasing.md#the-gates-a-release-passes-before-it-is-tagged)), and the
  release automation (build hygiene, test gate, pack, install smoke test, publish) runs on a Windows
  x64 machine. There is no non-Windows job anywhere in that pipeline.
- **What actually ships.** The `RDLL.dotnet-fast` NuGet package is a *portable* .NET tool
  (`tools/net10.0/any/`), but the tool itself is a native binary that the managed entry point launches,
  and the package carries that binary for **`win-x64` only** (`tools/net10.0/any/runtimes/win-x64/native/`)
  — the `--deep` Roslyn sidecar included. No other runtime identifier is published, and the NuGet
  package is the only artifact you can download: there is no standalone binary release asset today
  (see [security.md](security.md#the-standalone-binary--not-published-today)).
- **Linux and macOS do not work today** — a stronger statement than "untested", and worth stating
  plainly because the install step gives no warning. `dotnet tool install -g RDLL.dotnet-fast`
  **succeeds** on Linux or macOS: NuGet sees a portable .NET tool and installs it. The first run then
  fails with `dotnet-fast native binary was not found under '<tool directory>'` and exit code 1,
  because there is no Linux or macOS binary inside the package. Since no published artifact runs on
  those platforms, there is no "unsupported but working anyway" path to fall back on.
- **Never exercised.** No CI job, release step, or verification suite has ever built or run the tool
  for a non-Windows target. So beyond "the shipped package cannot run there", no claim is made about
  how it would behave if it were built for one: that is unvalidated — not known-good, and not
  known-bad either.
- **1.x plan.** linux-x64 first — produce and publish a per-RID binary, then put the same parity and
  corpus verification on a Linux runner — and macOS after it. A platform is supported when a release
  note says so, not before.

## Formatter scope

`dotnet-fast format` / `lint --fix` cover whitespace and style formatting — a `dotnet format`-compatible
path, tracking the .NET SDK 10.x `dotnet format`. Parity is measured continuously against real
open-source repositories; the floors a release must clear before shipping are:

| Repository | Files | Parity floor |
|---|---|---|
| Newtonsoft.Json | 945 | 944/945 — 99.89% |
| Polly | 797 | 796/797 — 99.87% |
| Dapper | 157 | 156/157 — 99.36% |
| AutoMapper | 512 | 508/512 — 99.22% |
| Serilog | 216 | 214/216 — 99.07% |

**Correction, 2026-08-03 — Polly's floor was `797/797 — 100%` and that figure is withdrawn.** It was not
a measurement of Polly: the parity harness selected its solution target with a directory walk that
returned nested solutions first, so from Polly's move to `.slnx` (May 2025) onward every Polly run
measured the 20-file `samples\Samples.slnx` instead of the 764-file `Polly.slnx`, and the 777 files
neither tool opened scored as free matches. The harness now picks the solution reaching the most source
files, prints and records which one it picked, and runs a coverage probe that fails the run if the two
tools did not process the same files. Re-measured under that harness at Polly `101d6af`: **796/797 =
99.87%** whole-tree, **775/776 = 99.87%** counting only files both tools actually formatted. The single
divergence is a multi-space argument run in `src/Snippets/Docs/ResiliencePipelineRegistry.cs`. No other
repository's floor was affected — their targets were already correct.

The other four floors were re-measured on 2026-07-17 with the byte-exact, BOM-aware parity comparer (binary
`e9c1117a`). Newtonsoft.Json's floor is one file below a round 100% because that comparer now compares
raw bytes rather than culture-decoded text: it surfaces a single pre-existing divergence in
`JContainer.cs` where the source indents a doc-comment continuation with non-breaking spaces (U+00A0)
that `dotnet format` preserves and `dotnet-fast` normalizes to ASCII spaces — a content difference the
old text comparer silently equated. It is unrelated to the BOM/`charset` policy.

### Latest coverage-checked sweep (2026-08-03)

The floors above are the minimum a release must clear. This is what the current build actually
measures, under the harness that verifies both tools processed the same files:

| Repository | Files both tools formatted | Parity | Run |
|---|---|---|---|
| Serilog | 216 / 216 | 100% | pass |
| AutoMapper | 512 / 512 | 100% | pass |
| Newtonsoft.Json | 940 / 941 | 99.89% | **fails the coverage check** (1 one-sided file — see below) |
| Polly | 775 / 776 | 99.87% | pass |
| Dapper | 153 / 154 | 99.35% | pass |

**Dapper's figure is new: the previous sweep recorded "not measurable — fails the coverage check", and
that entry is superseded.** Three defects, all in `dotnet-fast`, were making it process files the oracle
never opened, which showed up as a one-sided comparison the harness (correctly) refused to score:

- The `.slnx` reader treated a `<File Path="…">` **solution item** as a project. Dapper's solution pins
  `Build.csproj` — a repository-root `Microsoft.Build.Traversal` project — into a "Solution Items"
  folder, so the tool acquired a phantom project whose default compile glob is the entire repository and
  whose preprocessor symbols fall back to `DEBUG;TRACE`. Every `.cs` in the tree was then formatted a
  second time under the wrong symbol set.
- A UTF-8 BOM immediately before `#if` stopped the line reading as a directive (U+FEFF is not Unicode
  whitespace), so `benchmarks\Dapper.Tests.Performance\Benchmarks.Norm.cs` — whose whole body sits in
  `#if !NET4X` — was formatted as ordinary code.
- A comment inside an `#if` condition (`#if !NETFRAMEWORK // platform not supported exception`, the shape
  in `tests\Dapper.Tests\Providers\SnowflakeTests.cs`) aborted condition parsing, and an unparsed
  condition defaulted to "branch is active".

The earlier explanation for those two files — that `dotnet-fast` is "syntax-only across the union of
target frameworks" — was wrong: the tool does bind the project's real preprocessor symbols, and these
were three parsing bugs rather than a design limitation. Re-measured at Dapper `72a54c4` with the
coverage probe: **153/154 = 99.35%** over files both tools formatted, zero one-sided files, whole-tree
156/157 = 99.36%. The single remaining divergence is the indentation of a `= new(…)` continuation line
in `Dapper\CompiledRegex.cs`.

**Newtonsoft.Json now fails the coverage check, and its parity figure is unchanged at 99.89%.** The
one-sided file is `Src\Newtonsoft.Json.Tests\Issues\Issue3080.cs`. This is the *already-documented*
final-line edge case, not a new divergence: that file's last line is an unterminated `#endif` (no
trailing newline), and `dotnet format` responds to an unterminated final directive line by trimming the
trailing whitespace on **every** directive line in the document, while leaving the disabled body alone.
Probed directly on SDK 10.0.302: with a terminated final `#endif` the oracle preserves every directive
line's trailing run (which is what `dotnet-fast` does); change only that last line to unterminated and
the oracle trims all of them. The file surfaces in the coverage probe now because the BOM fix above
stopped `dotnet-fast` rewriting its first line. **Pristine parity is unaffected** — whole-tree
944/945 = 99.89%, identical to the published floor — because none of these lines carries trailing
whitespace in the real source; only the probe's synthetic perturbation reaches it. All five
repositories clear the 99% floor; four of five clear the coverage check.

Corpus pins: Polly `101d6af`, Dapper `72a54c4`, Serilog `49b5339`, AutoMapper `dfa6dd5`,
Newtonsoft.Json `4f73e74`; oracle `dotnet format whitespace --no-restore` on SDK 10.0.302. Full
methodology is in [benchmarks.md](benchmarks.md#formatter-parity-floors).

One documented, intentional divergence: with `indent_size = tab`, `dotnet format` emits multiple tabs
sized by an internal width for `tab_width`, while `dotnet-fast` emits one tab per indent level, honoring
the author's tab intent rather than reformatting a tabs-only repository against it.

**The `.editorconfig` `charset` key: the UTF-8 BOM policy is supported.** `charset = utf-8` strips a
leading UTF-8 BOM and `charset = utf-8-bom` adds one (oracle-verified against `dotnet format`); an
unset `charset` preserves the file's existing BOM. `latin1`/`utf-16be`/`utf-16le` remain out of scope
— those imply a full content transcode, which is never performed (content is read and written as
UTF-8 bytes and never re-encoded).

## Native lint (default path)

`dotnet-fast lint`'s default path is **syntactic-only**: it reads source as a syntax tree with no type
information or dataflow, which is what lets it start in milliseconds with no .NET SDK, no restore, and
no MSBuild/Roslyn load.

- **91 of the .NET SDK's 121 `IDExxxx` style rules are covered** natively (tracked against SDK
  10.0.109). **25 are out of scope** for the syntactic path because they need the semantic model
  (for example `IDE0001`/`IDE0002` "simplify name/member access", `IDE0060` unused parameter,
  `IDE0079` unnecessary suppression, `IDE1006` naming rules) — those need `--deep`. A further 5 are
  deferred (not yet triaged). The current coverage is generated and kept in sync with each SDK bump;
  the always-current list is in [ported-analyzers.md](ported-analyzers.md) and
  [deep-linting.md](deep-linting.md).
- Beyond the SDK's own `IDExxxx` set, `dotnet-fast` also **natively re-implements popular third-party
  Roslyn analyzers** (SonarAnalyzer, Microsoft.CodeAnalysis.NetAnalyzers, StyleCop, Roslynator,
  Meziantou, and others) as syntactic rules verified at exact parity against the real analyzer. See
  [ported-analyzers.md](ported-analyzers.md) for the full catalog.

### Superset semantics — native rules may report more than the real analyzer

The native ports are a **bundled superset, active by default**: they run regardless of which analyzer
packages your project actually references, so `lint` can report rule IDs a plain `dotnet build` or
`dotnet format` would never have surfaced for that project (because you don't reference the package the
rule came from). Pass `--only-active-analyzers` to restrict findings to the analyzers your project
actually references, matching what a real build would show.

## `--deep` (real Roslyn analyzers)

`--deep` is the opt-in depth pass: it runs **your project's real Roslyn analyzers** — every analyzer
package you reference (StyleCop, Roslynator, Meziantou, SonarAnalyzer, etc.) plus the .NET SDK's
built-in `CA*` analyzers — and merges their findings into the same report. It needs the .NET SDK
installed and a restored project (`dotnet restore` or a prior build).

`--deep` runs analyzers at their **default analysis level** — the rules and severities a plain
`dotnet build` enables. It does not replicate stricter opt-in modes (`AnalysisLevel`/`AnalysisMode`
escalations such as `All`/`AllEnabledByDefault`); findings that only appear under a strict mode will not
appear here.

## CI accelerators

- **Test sharding (`test-plan`)** — **NUnit only**. xUnit and MSTest support are planned follow-ups.
- **Bill of materials (`bom`)** — **CycloneDX** is the default format (1.4/1.5/1.6, JSON or XML); SPDX
  2.2/2.3 JSON is also supported.
- **Build cache (`build`)** — backed by **Azure Blob Storage** (SAS or Entra/managed-identity auth). No
  other storage backend is supported.

## Explicitly unsupported

- **Fixes that need semantic analysis outside `--deep`** — the native default path is syntactic-only by
  design; anything requiring type/symbol/dataflow information (unused-`using` removal, "simplify name",
  unused-parameter checks, and the rest of the 25 out-of-scope `IDExxxx` rules above) is not available
  without `--deep`.
- **Non-SDK-style / legacy `.csproj` projects** — detected and handled without crashing (see
  **Supported input** above), but not a validated, supported project shape. Commands that depend on a
  reliable project graph skip them with a stated reason rather than reporting partial or guessed
  results.
- **VB.NET and F#** — `dotnet-fast` is a C# tool; VB and F# projects are not analyzed.
- **`restore` and SBOM-as-of-v0.154** — a legacy `restore`/`update`/SBOM feature set was removed in
  v0.154.0 as part of a scope narrowing to format/lint/affected/CI-acceleration. `restore` has not been
  reintroduced and there is no plan to. (The current `bom` command described above is a distinct,
  current feature, unrelated to the removed SBOM support — as is the current `update` command, which is
  a tool self-updater added in 0.306.0 and shares nothing with the removed package-`update` verb beyond
  the name; see [install.md](install.md#update).)
- **Linux and macOS** — no binary is published for either, so the installed tool cannot run there at
  all (it installs, then fails at first run; see **Platform** above). linux-x64 is the first target for
  the 1.x line.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

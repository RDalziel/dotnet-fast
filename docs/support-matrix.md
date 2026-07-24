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

**Windows x64 is the validated platform**: every release runs the full parity, corpus, and
whole-repository verification suites on Windows against the real `dotnet format` before shipping.
Other platforms (linux-x64, macOS) are **experimental and unvalidated** — the tool is native Rust and
generally builds there, but no release gate exercises them yet; treat results accordingly. Validated
Linux support is planned for the 1.x line.

## Formatter scope

`dotnet-fast format` / `lint --fix` cover whitespace and style formatting — a `dotnet format`-compatible
path, tracking the .NET SDK 10.x `dotnet format`. Parity is measured continuously against real
open-source repositories; the floors a release must clear before shipping are:

| Repository | Files | Parity floor |
|---|---|---|
| Newtonsoft.Json | 945 | 944/945 — 99.89% |
| Polly | 797 | 797/797 — 100% |
| Dapper | 157 | 156/157 — 99.36% |
| AutoMapper | 512 | 508/512 — 99.22% |
| Serilog | 216 | 214/216 — 99.07% |

These floors were re-measured on 2026-07-17 with the byte-exact, BOM-aware parity comparer (binary
`e9c1117a`). Newtonsoft.Json's floor is one file below a round 100% because that comparer now compares
raw bytes rather than culture-decoded text: it surfaces a single pre-existing divergence in
`JContainer.cs` where the source indents a doc-comment continuation with non-breaking spaces (U+00A0)
that `dotnet format` preserves and `dotnet-fast` normalizes to ASCII spaces — a content difference the
old text comparer silently equated. It is unrelated to the BOM/`charset` policy.

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
- **`restore`, `update`, and SBOM-as-of-v0.154** — a legacy `restore`/`update`/SBOM feature set was
  removed in v0.154.0 as part of a scope narrowing to format/lint/affected/CI-acceleration. It has not
  been reintroduced and there is no plan to. (This is unrelated to the current, supported `bom` command
  described above, which is a distinct, current feature.)
- **Validated Linux/macOS behavior pre-1.x** — see **Platform** above; these platforms are experimental
  until the 1.x line ships validated support.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

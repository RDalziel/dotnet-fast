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

**Windows x64 is the verified platform. Linux x64 ships a binary but is not yet verified. macOS has
nothing.**

- **Tested — Windows x64.** Every release is built, packaged, and verified there: the test suite, the
  CLI-contract goldens and the differential parity fixtures against the real `dotnet format` run before
  shipping, the heavier whole-repository sweeps run for formatter changes
  ([which gate runs when](releasing.md#the-gates-a-release-passes-before-it-is-tagged)), and the
  release automation (build hygiene, test gate, pack, install smoke test, publish) runs on a Windows
  x64 machine.
- **What actually ships.** The `RDLL.dotnet-fast` NuGet package is a *portable* .NET tool
  (`tools/net10.0/any/`), but the tool itself is a native binary that the managed entry point launches.
  The package carries that binary for **`win-x64`** and **`linux-x64`**
  (`tools/net10.0/any/runtimes/<rid>/native/`), each with the `--deep` Roslyn sidecar. Each release
  also attaches standalone `dotnet-fast-win-x64.exe` and `dotnet-fast-linux-x64` binaries (with their
  `.sha256`s), checksummed but unsigned — NuGet is still the recommended channel (see
  [security.md](security.md#the-standalone-binary)).
- **Linux x64 — ships, not yet verified.** The binary is a static `x86_64-unknown-linux-musl` build,
  cross-compiled on the Windows release machine, so it needs no particular glibc and runs in
  runtime-only container images. The test suite, the parity fixtures and the corpus sweeps have not
  yet run on Linux, so no claim is made that its output matches Windows byte for byte.
- **macOS does not work today.** `dotnet tool install -g RDLL.dotnet-fast` **succeeds** there, because
  NuGet sees a portable .NET tool, but the first run fails with
  `dotnet-fast native binary was not found under '<tool directory>'` and exit code 1.
- **1.x plan.** Put the same parity and corpus verification on a Linux runner, then macOS. A platform
  is supported when a release note says so, not before.

## Formatter scope

`dotnet-fast format` / `lint --fix` cover whitespace and style formatting — a `dotnet format`-compatible
path, tracking the .NET SDK 10.x `dotnet format`. Parity is measured continuously against real
open-source repositories; the floors a release must clear before shipping are:

| Repository | Files | Parity floor |
|---|---|---|
| Newtonsoft.Json | 945 | 945/945 — 100% |
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
`e9c1117a`). Newtonsoft.Json's floor sat one file below a round 100% from then until 2026-09-16 because
that comparer compares raw bytes rather than culture-decoded text: it surfaced a single pre-existing
divergence in `JContainer.cs` where the source indents a declaration with non-breaking spaces (U+00A0)
that `dotnet format` preserves and `dotnet-fast` normalized to ASCII spaces — a content difference the
old text comparer silently equated. It was unrelated to the BOM/`charset` policy, and it is **fixed**:
see the 2026-09-16 sweep below.

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

**On this sweep Newtonsoft.Json failed the coverage check, with its parity figure unchanged at 99.89%
(superseded by the 2026-09-16 sweep below, where it passes at 100%).** The
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

### Latest coverage-checked sweep (2026-09-16)

Re-measured after the two whitespace fixes described below, same harness, same corpus pins, oracle
`dotnet format whitespace --no-restore` on SDK 10.0.303:

| Repository | Files both tools formatted | Parity | Run |
|---|---|---|---|
| Newtonsoft.Json | 942 / 942 | **100%** (was 99.89%) | **passes the coverage check** (0 one-sided; was 1) |
| Serilog | 216 / 216 | 100% | pass |
| AutoMapper | 512 / 512 | 100% | pass |
| Polly | 775 / 776 | 99.87% | pass |
| Dapper | 153 / 154 | 99.35% | pass |

**Newtonsoft.Json is at 100% and clears the coverage check for the first time.** Its only divergence —
the `JContainer.cs` non-breaking-space indent — is closed by the second fix below, and the one-sided
`Issue3080.cs` finding from the 2026-08-03 sweep no longer reproduces. Whole-tree arithmetic agrees at
945/945. The four other repositories are byte-for-byte unchanged from their published figures, which is
the no-regression check for the `case (` fix: it moves output in real repositories, and it moved none of
theirs.

Polly's and Dapper's single divergences are the same two already documented above (a multi-space
argument run in `ResiliencePipelineRegistry.cs`; a `= new(…)` continuation indent in
`CompiledRegex.cs`). Neither is affected by this work.

### Two whitespace rules fixed against the oracle (1.6.x)

Both are byte-level formatter behaviour — no flag, default, output field or exit code changed.

**A `case` label keeps its space before `(`.** `case ("XS"):`, `case (int, string):`,
`case (int)Kind.A:`, `case (1):` and `goto case (1);` all keep exactly one space after the keyword; a
tight `case("XS"):` is normalized to the spaced form and a multi-space run collapses to one, matching
`dotnet format whitespace` in all three directions. Earlier builds classified that parenthesis as a
method call and tightened it, so `lint --fix` rewrote every parenthesized switch label in a
repository. The space is **unconditional**: `csharp_space_after_keywords_in_control_flow_statements =
false` tightens `if(`/`while(`/`switch(`/`foreach(` and leaves `case (` alone, which is what the SDK
does and is now pinned by a regression case carrying that key.

If you run `lint` as a CI gate, expect the finding set to move: no more whitespace finding on
`case (`, and a new one on `case(`. Output shape and exit codes are unchanged.

**An already-correct indent run under a comment is left byte-for-byte alone.** When a line's leading
whitespace contains a character that is neither a space nor a tab — in practice a non-breaking space
(U+00A0) inherited from an old encoding — it is now preserved verbatim, provided a comment sits in the
trivia above the line (an XML doc block, a `//` line, a `/* … */` line, or a trailing `// …` on the
preceding code line, with any number of blank lines between) **and** the run is already exactly as wide
as the indent that would be computed. That is what `dotnet format` does: a comment between two tokens
routes Roslyn through its complex-trivia path, which compares columns and emits no edit when they
already match. Move the line off its correct column, or take the comment away, and it is re-indented
normally; for an all-ASCII indent the rule is invisible, since rebuilding a space run at the same width
reproduces the same bytes. This closes the `JContainer.cs` divergence described above. A run that mixes
such a character with more than one ASCII space is still re-indented — a documented remaining gap.

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

- **Test sharding (`test-plan`)** — **NUnit, xUnit and MSTest**, in C# and F#, with no flag to set. The
  vocabulary is read by attribute NAME, so a derived attribute type (`[SkippableFact]`, your own
  `FactAttribute`/`TheoryAttribute`/`TestMethodAttribute` subclass) is not recognised: a project where
  EVERY test class uses one is reported with `no-fixtures`, while a project that MIXES them is planned
  from its recognised classes and the rest are absent from the plan without a warning
  ([detail](test-sharding.md#xunit-and-mstest-test-projects)). The zero-match-filter behaviour the shard
  audit relies on was measured on the **VSTest** adapters; the Microsoft.Testing.Platform runners
  (`EnableMSTestRunner`, xunit.v3) are not covered.
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
- **F#** — supported across the board, so a mixed C#/F# solution is handled rather than half-skipped.
  `affected`, `build`, `doctor` and `bom` work from the project graph and treat `.fsproj` like any
  other project. `test-plan` discovers NUnit, xUnit and MSTest fixtures from F# sources, `metrics` scores F# against the
  same budgets, and `dead-dependencies` reads `open` declarations the way it reads `using` directives.

  **F# formatting is done by driving [Fantomas](https://fsprojects.github.io/fantomas/), not by a
  formatter of our own.** F# is offside-rule — indentation is syntax, not style, so a formatter that
  guesses wrong produces code that does not compile or quietly means something else — and Fantomas is
  the community standard, is AST-based on a fork of the F# compiler, and is what Microsoft's F# style
  guide defers to. Pass `--fantomas` to `lint` or `format` and it runs, scoped by the same
  `--project`/`--include`/affected flags as the C# lane, skipping files whose content it has already
  formatted, honouring `.fantomasignore`, and reporting into the same summary/JSON/SARIF output
  (`FSFMT001` would reformat, `FSFMT002` could not parse). **Fantomas owns every style decision** and
  is configured by its own `.editorconfig` keys (`fsharp_*`, plus `max_line_length`, `indent_size`,
  `end_of_line`, `insert_final_newline`) — no `dotnet-fast` setting changes how F# looks. It is
  strictly opt-in: without `--fantomas`, `format` never rewrites an F# file, and if Fantomas is not
  installed the run fails with exit `168` and the install command rather than reporting a repository
  of unformatted F# as clean. Nothing is ever fetched or installed. See
  [commands](commands.md#f-formatting-with-fantomas).

  Two further limits are deliberate, and each exists because the alternative would be a confident
  wrong answer. **`dead-code` reports unused members only** — not unused types, test-only symbols or
  dead projects, and never with `--fix` — because F# type inference lets a record literal or a union
  case use a type without ever naming it, so absence of a name is not absence of use. **`lint`'s
  native rules cover F# hygiene only** (`FSH0001`–`FSH0004`: trailing whitespace, final newline, line
  endings, and tab characters, which the F# compiler rejects outright as FS1161); the C# rule catalog
  does not apply to F#, and everything structural belongs to Fantomas.

  F# source is parsed with a grammar that cannot parse every file. A file it rejects, or a project
  whose language nothing here reads, is **reported with a stated reason** — in the plan, the
  scoreboard or the findings — never silently skipped.
- **VB.NET** — `.vbproj` project graphs resolve, so `affected` and the build cache treat them like
  any other project, but no command reads VB source. A VB project reaching a command that needs
  source is reported with a stated reason rather than dropped.
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

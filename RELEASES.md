# Release notes

What changed in recent releases, in plain English. Newest first. The tool is `0.x` — usable and tested,
but commands may still change before `1.0`.

## 0.296.1

Completes the previous release's build-failure reporting. v0.296.0 surfaced a failed project's compiler
errors to the Azure DevOps Errors tab, but only those tied to a specific source line (like
`Foo.cs(20,20): error CS0029`). Errors with no source location — a "program does not contain a static
'Main' method", a "metadata file could not be found", linker errors, and similar (very common when a
referenced project fails to build) — were still being dropped. Those now show up too, both in the
"Failures" section and as Azure DevOps error annotations.

## 0.296.0

`dotnet-fast build` now makes a failed project much easier to find and act on. Two changes:

1. When several projects build in one run, results print in completion order — a failure could land
   anywhere among dozens of success lines, with only a count at the end (`1 failed`) and no name. Now
   every failure is also collected into a **"Failures" section** printed once, last, and the summary
   line names the failed project(s) directly (`1 failed: MyApp.Tests`) instead of just counting them.
2. On **Azure DevOps** specifically, a failed project's real compiler error (e.g. `error CS0029: ...`)
   used to be invisible to the pipeline's Errors tab and REST API — only a generic "build failed with
   exit code 1" wrapper reached there. `dotnet-fast build` now re-emits each failure's actual compiler
   diagnostic as an ADO `##vso[task.logissue]` command, so the real error shows up where you'd look
   for it. This is Azure Pipelines-specific for now; it's a no-op everywhere else (including running
   locally).

No existing output changed shape — this only adds new lines to the failure path.

## 0.295.1

Fixes a real regression from the previous release: `dotnet-fast build` could fail with a NuGet
"assets file not found" error for any project that wasn't served by a remote build-cache hit — for
example a project with no committed `packages.lock.json`, or any run with the cache turned off
(`--no-cache`, or no cache configured at all). A recent fix (correctly) stopped re-restoring
packages for projects whose restore state was already pulled from the cache, but that stricter
behavior was accidentally applied everywhere, including the many cases that had no restore step to
piggyback on. Restore now only gets skipped for the one case that was actually broken, and every
other project builds exactly as before.

## 0.295.0

Two fixes for `insights` in CI. First, a bug: on Azure Pipelines (and every other supported CI
provider), the checkout is normally in a "detached HEAD" state, which broke the fallback logic that
figures out which branch a build belongs to — every CI run was silently recording its build/test/lint
timings under the same generic `default` bucket instead of its real branch name, so per-branch history
(and the `--baseline-scope` regression comparison) wasn't actually separating branches from each other.
Fixed: the real branch name is now read directly from the CI provider's own environment variables
first. Second, a new `--env ci` / `--env local` flag on `insights build`/`insights test`/`insights lint`
lets you read what a CI run recorded from your own machine, without the undocumented workaround of
faking a CI environment variable. Purely additive — no existing command, flag, or output shape changed.

## 0.294.4

`insights test --help` (and `insights build`/`insights lint`) no longer bury the handful of flags
that actually apply under a wall of unrelated formatting flags — the relevant options now come
first, with a quickstart example printed right in the `--help` output showing exactly how to
record a test run and turn it into an HTML report. The documentation for the insights commands
has also been filled in properly. No command, flag, or output-shape changes — this is a clarity
fix only.

## 0.294.3

Four more real-world formatting shapes now match the reference formatter exactly: an opening
brace followed immediately by code on the same line is split onto its own line the same way the
reference formatter does; a closing brace that follows other code on the same line (for example,
ending a multi-line lambda passed as an argument) is likewise split out; the closing line of a
collection initializer now gets its usual spacing cleanup (so `} .Method()` becomes `}.Method()`);
and a lambda parameter list preceded by an attribute now lines up at the same depth as the
surrounding statement. Together with recent releases, this brings several previously-diverging
real-world files into full agreement with the reference formatter. No command, flag, or
output-shape changes.

## 0.294.2

A collection initializer nested inside another initializer (for example, a `Dictionary` assigned
to a property inside a larger object initializer, or a jammed-together nested object as a list
element) is now preserved exactly as written, matching the reference formatter, instead of being
re-split or re-indented. This closes out several remaining real-world formatting differences found
by sweeping open-source repositories. No command, flag, or output-shape changes.

## 0.294.1

Two more formatting shapes brought into byte-exact agreement with the reference formatter: a
comment in the middle of an expression (`a +   /* note */ b`) no longer confuses the spacing
around the operator, and whitespace right after a closing `*/` is now preserved the same way
whitespace right before an opening `/*` already was. Both fixes only apply near block comments —
everyday code is unaffected. No command, flag, or output-shape changes.

## 0.294.0

The formatter now honors the `.editorconfig` `charset` key for byte-order marks, matching the
reference formatter: `charset = utf-8` strips a leading BOM, `charset = utf-8-bom` adds a missing
one, and a mismatch is reported (and fails `--verify-no-changes`) like any other formatting
difference. Content is still never transcoded — `latin1` and `utf-16` values remain out of scope
and are left untouched. Files with no `charset` configured keep their existing BOM exactly as
before.

New analytics: `insights lint` joins `insights build` and `insights test`. Opt in by adding
`--record-findings` to your `lint` runs (it needs the same timings store as test sharding, and
changes nothing else about the run); the tool then tracks finding counts over time, and
`insights lint` shows the trend, the rules that fire most, a severity breakdown, and how much of
what's auto-fixable is actually getting fixed — as text, `--json`, or a self-contained `--html`
report.

One new lint rule ported from the analyzer ecosystem (MA0211, single-line XML `<summary>`
comments that should be multi-line), bringing the ported set to 679. Analyzer-parity floors were
re-measured with a stricter byte-exact comparison across the five reference repositories.

## 0.293.2

Four formatting false positives found by sweeping real open-source repositories against the
reference formatter, all fixed: a pattern like `result is -1` no longer loses the space before the
negative number; a trailing `// comment` on an object-initializer member is no longer split onto
its own line; generic types such as `Span<object?>` inside conditional expressions (and generics
followed by a trailing comment) are no longer torn apart with operator spacing; and a `using (...)`
statement whose header mentions `new` is no longer mistaken for an object initializer (which could
tear an unrelated line apart at a comma). Each fixed shape is pinned by a regression test verified
against the reference formatter's own output. No command, flag, or output-shape changes.

## 0.293.1

Closes out the long-running formatting false positive on deeply-aligned object initializers inside
fluent chains: the trigger was a multi-line collection expression (`[` ... `]`) enclosing the shape.
Real `dotnet format` preserves everything inside a multi-line collection expression exactly as the
author aligned it — the same way it already treats collection initializers — and dotnet-fast now
does too, instead of re-indenting those lines to computed depth. Verified byte-for-byte against the
reporting solution's file (24 spurious findings, now zero, with real `dotnet format` certifying the
file clean) and guarded so the genuine re-indent outside collection contexts is unchanged. Verified
against six OSS repositories with zero finding drift. No command, flag, or output-shape changes.

## 0.293.0

Fixes CycloneDX BOM output so documents containing package hashes pass strict CycloneDX 1.6 schema
validation (previously rejected by strict validators such as Dependency-Track). Content hashes from
the NuGet lock file are now emitted as the hex the `hashValue` type requires — not their source
base64 — and the `<hashes>` element is written in its schema-defined position within the `component`
sequence (before `purl`). This applies to both the XML and JSON serializers; a hash that cannot be
represented at a valid digest length is omitted rather than emitted invalid.

Test-timings history gains an environment dimension: CI and local runs record into separate
partitions (`ci`/`local`), so the shard planner's per-fixture averages are no longer skewed by
mixing environments that run at different speeds. Reads prefer the current environment and fall back
to prior history during the migration. No command, flag, or output-shape changes.

## 0.292.9

Reverts 0.292.7's fluent-call tightening after a collection-initializer close: field verification
on the reporting solution showed the transform firing on real-world geometry that `dotnet format`
itself leaves untouched, making that file's finding count worse rather than better. The 0.292.6
fixes (tuple-array `new` spacing, switch-pattern brace tightening) are field-confirmed byte-exact
and unaffected. The reverted shape's investigation continues with its regression-ledger case
parked until an exact reproduction (file + `.editorconfig` chain) is available. All five
repository parity floors re-verified byte-exact.

## 0.292.8

Internal hardening — no behavior changes. The dead-code analyzer is now fully mutation-tested end
to end (collection, symbol resolution, mark-and-sweep, reporting, and handler-pattern tables all
proven covered by failing tests when mutated), and the formatter gained an always-on regression
suite of real-`dotnet format`-verified golden files covering every formatting divergence reported
from the field — enforced before any release or NuGet publish. Output, flags, and exit codes are
unchanged.

## 0.292.7

One more formatting-parity fix from the same real-solution comparison as 0.292.6: a fluent call
continuing on a collection initializer's closing-brace line (`} .Concat(...)`) now tightens its
member-access dot (`}.Concat(...)`) exactly as `dotnet format` does — the initializer's interior
and trivia-only closing lines (`});`, `},`) remain untouched. Byte-verified against the real
formatter across five structural contexts and the full five-repository parity floors.

## 0.292.6

Two formatting-parity fixes from a real ~800-project solution comparison against `dotnet format`:
a tuple-array creation now gets its space after `new` (`new (string name, decimal cost)[]` — the
parenthesized run is a tuple type, not an argument list; target-typed `new(...)` stays tight), and
a switch-expression pattern's `{` directly after `(` is now tightened (`({ } min, { } max)`),
matching `dotnet format` exactly. Both verified byte-for-byte against the real formatter and the
full five-repository parity floors. Also: the dead-code analyzer's mark-and-sweep pass is now fully
mutation-tested.

## 0.292.5

Two fixes from real CI usage. **`test-plan --report <dir>` now works**: it writes the computed
plan as `test-plan-report.json` (shard membership, per-project weights, filters, argv) in every
mode — plan, `--format json`/`matrix`, single-shard, and `--exec` — written before tests run so
the artifact survives a failing run; previously the flag was silently ignored. **An unreachable or
unauthorized timings store no longer fails the run**: `--use-cached-timings` (on both `test-plan`
and `build`) now logs a `skipped` warning and falls back to fixture-weight/project-count balancing,
matching `--record-timings`' existing graceful behavior — a misconfigured credential can't turn an
optional optimization into a hard CI failure. Genuine configuration mistakes (an invalid table
name, a cache URL a table endpoint can't be derived from) still fail loudly.

Also: internal hardening with no behavior changes — Extended the mutation-testing campaign into the dead-code
analyzer: its per-file collection layer and symbol-merge/name-resolution logic are now pinned by
direct tests (every field value, accessibility rule, and entry-point gate proven covered by a
failing test when mutated). Output, flags, and exit codes are unchanged.

## 0.292.4

Internal hardening — no behavior changes. Continued the mutation-testing campaign across the
dead-dependency analyzer: its dependency-graph construction, source-usage scanner, and package
knowledge tables are now fully covered by direct tests (previously exercised only indirectly),
including on-disk project fixtures for the layers that read real files. Output, flags, and exit
codes are unchanged.

## 0.292.3

Internal hardening — no behavior changes. The dead-dependency classifier and the Azure table
timings client are now fully mutation-tested (every code path proven covered by a failing test when
mutated), adding several hundred targeted tests across paging, date handling, and each detection
rule's edge cases. Output, flags, and exit codes are unchanged.

## 0.292.2

Internal hardening — no behavior changes. The Azure blob cache client's full HTTP surface is now
exercised against a local fake server in the test suite (status handling, chunked uploads at the
8 MB threshold, retry backoff, token handling), closing a coverage gap where a regression in cache
probing could previously have gone unnoticed. Duplicate authentication plumbing between the blob
and table clients was consolidated. Output, flags, and exit codes are unchanged.

## 0.292.1

Internal maintenance — no behavior changes. The build cache's core planning module is now fully
mutation-tested (every code path proven covered by a failing test when mutated, with the survivors
audited and documented), and the CLI's command orchestration was tidied internally for
maintainability. Output, flags, and exit codes are byte-for-byte unchanged.

## 0.292.0

**Test sharding now prices test-host startup time.** Per-test durations can't see the fixed cost
every `dotnet test` invocation pays before the first test runs (host boot, assembly load), so a
shard holding several small projects could be modeled as equal to a shard holding one big project.
`test-plan --exec --record-timings` now also records each project's startup residual (invocation
wall time minus its tests' recorded time) into the same timings history, and duration-balanced
planning charges it to a shard once per project it runs — so fragmenting small projects across
agents is priced at its real cost. Nothing to configure: the data appears after the first recorded
run, existing timings keep working, and plans without recorded startup data are unchanged. See the
[test sharding guide](docs/test-sharding.md). Also: substantially deeper test coverage for the
build cache's planning and key-derivation internals.

## 0.291.4

Internal hardening on the road to 1.0 — no behavior changes. Deeper test coverage for the build
cache's artifact hashing (mutation-tested: content hashing is now pinned against known-answer
SHA-256 vectors), and a redundancy pass over the build-cache, dead-code, and dead-dependencies
modules (duplicated retry/path-handling helpers consolidated into single shared implementations).
Also published since 0.291.3: the frozen [support matrix](docs/support-matrix.md), the
[versioning promise](docs/versioning.md), and the dated
[1.0 gate verification run](docs/benchmarks.md).

## 0.291.3

**The v1 parity gate is met**: whole-repository formatting now matches `dotnet format`
byte-for-byte on ≥99% of files across all five verification repositories — Newtonsoft.Json
**100%**, Polly **100%**, Dapper 99.4%, AutoMapper 99.2%, Serilog 99.1%. This release lands the
final seven fixes (block-comment interior handling, `switch` expressions after `=>`, `++`/`--`
prefix spacing, multi-declarator field headers, standalone comment alignment, and method chains
inside lambda arguments). The README now also states the supported-platform position explicitly:
Windows x64 is the validated platform; other platforms are experimental until the 1.x line.

## 0.291.2

A major parity release on the road to v1. Formatting now matches `dotnet format` byte-for-byte on
**100% of Newtonsoft.Json (945 files) and Polly (797 files)**, with Serilog at 99.1% and AutoMapper
at 99.0%. The biggest change: `#if`/`#else` regions are now classified using your project's real
preprocessor symbols (`DefineConstants`, target-framework implications, OS-conditioned target
groups) exactly as `dotnet format` does — previously a built-in guess could leave whole regions
formatted differently. Two dozen further fixes ride along, from LINQ query alignment to `catch ...
when` filters, multi-declarator fields, and comment handling inside conditional regions.

## 0.291.1

Four more formatting fixes verified against `dotnet format`, continuing the march to v1 parity. A
conditional expression whose branch carries an inline object initializer no longer loses the
indentation of its following `: ...` line; wrapped conditional operands inside `switch` cases keep
their indentation; initializer members starting with a minus keep their brace padding (`{ -1, 10 }`);
casts before a negative value stay tight (`(ulong)-value`); and anonymous `delegate (` methods keep
their space. Whole-repository parity: Newtonsoft.Json 95.7% → 96.6%, AutoMapper 98.6%, Polly 100%.

## 0.291.0

**Important correctness fix — update recommended.** Verbatim strings (`@"..."`) containing
backslash-and-quote sequences (common in JSON-in-string test data) could have their *contents*
altered by formatting — spaces inserted or removed inside the string value itself. The formatter
now treats backslashes in verbatim strings as the literal characters they are, and a new permanent
check guarantees no string or character literal's bytes are ever changed by formatting again.

Also in this release: `test-plan` gains an opt-in Azure Table Storage store for cached test/build
timings (share timing data across agents without a blob container), and a family of deep-nesting
indentation fixes verified against `dotnet format` — object initializers passed as method arguments
now snap to the correct depth (whole-repository parity: AutoMapper 98.4%, Newtonsoft.Json 95.7%,
Polly 100%).

## 0.290.17

Internal reliability release — no behavior changes. The Windows path handling that caused
0.290.16's build-cache fix is now consolidated in one place (the same mistake can't be
reintroduced at another call site), and SBOM generation gained a performance benchmark to catch
regressions early.

## 0.290.16

Two fixes from user bug reports. Formatting: conditional expressions whose branches are object or
anonymous-type initializers aligned to the `?` column are now left alone, matching `dotnet format`
(previously flagged with spurious dedent demands), and a deeply-indented lambda body following a
fluent chain is now re-indented relative to its own opening brace exactly as `dotnet format` does
(previously missed entirely — the gate let real drift through). Verified against the official
formatter on replicas of every reported construct, plus whole-repository parity improvements on all
four measured repos. Build cache: a project packing a NuGet package to a custom folder
(`PackageOutputPath`) now genuinely round-trips that package through a cache hit — the folder was
being archived under an unusable path on Windows and silently skipped on restore.

## 0.290.15

Four formatting fixes, each verified against `dotnet format`. A nullable object creation inside a
conditional expression (`flag ? new decimal?() : source`) is no longer torn apart into
`new decimal ? ()`. A multi-line array initializer keeps its original brace spacing (`new[]{` stays
tight, `new[] {` stays spaced) instead of always gaining a space. A comma directly before a
trailing `//` comment keeps its spacing rather than being pushed apart. And generic casts now
tighten to their operand like simple ones (`(IMyInterface<T>) Map(...)` becomes
`(IMyInterface<T>)Map(...)`). Whole-repository parity: AutoMapper 96.5% → 97.7%.

## 0.290.14

Internal robustness release — no behavior changes. Every remaining spot in production code where an
unexpected internal state could abort the tool mid-run was rewritten to handle the state gracefully
or document why it cannot occur, and a new automated guard keeps it that way (any future change that
could crash instead of reporting a clean error now fails the build). Verified crash-free against
large open-source repositories before shipping.

## 0.290.13

Indentation fix inside `switch` statements, verified against `dotnet format`: a comment written
just above the next `case`/`default` label (a common way to annotate fall-through) now lines up
with that label instead of staying at the case-body depth.

## 0.290.12

Build-cache fix: projects that pack a NuGet package on build into a custom folder
(`GeneratePackageOnBuild` with `PackageOutputPath`) now have that folder captured in the cache
artifact, so a cache hit restores the `.nupkg` too instead of silently omitting it. Output paths
the cache can't safely capture (outside the project, or computed dynamically) now mark the project
uncacheable with an explanatory reason rather than caching incompletely.

## 0.290.11

Two more formatting fixes verified against `dotnet format`. Array and collection initializers
written with the brace on the same line (`new[] {`, `new List<int>() {`) are no longer forced onto
a new line — only object initializers, anonymous types, and lambda bodies split, matching the real
formatter. And a nasty pair of comment-parsing bugs is fixed: an apostrophe inside a same-line
block comment (`catch { /* don't care */ }`) could make the formatter mis-count braces and indent
everything after it one level too deep, compounding through the file. Whole-repository parity:
Dapper jumped to 94.3% (+7.7 points), Serilog 97.2%, AutoMapper 96.5%.

## 0.290.10

Two formatting fixes found by re-surveying five large open-source repositories. Files starting
with a UTF-8 byte-order mark now get the blank line after a file-scoped `namespace ...;` like any
other file (the invisible BOM character was hiding the namespace from that rule). And stacked
`using (...)` statements — the common flat chain style — are no longer stair-step indented; the
chain stays level, with only the final body indented, matching `dotnet format`. Whole-repository
parity: AutoMapper jumped to 95.7% (+9 points), Newtonsoft.Json 94.5%, Serilog 95.4%, Polly holds
100%.

## 0.290.9

Two `dead-code` false positives fixed, both reported from real codebases. LightBDD scenario
classes (`FeatureFixture` subclasses with `[Scenario]` methods) are now recognized as executing
tests, like classes with plain `[Test]`/`[Fact]` methods. And EF Core `Migration` classes are now
always kept alive: their identifying `[DbContext]`/`[Migration]` attributes live on the generated
`.Designer.cs` half, which the scanner deliberately skips as generated code, so on whole-solution
scans real applied migrations could be reported dead — and offered for deletion by `--fix`.

## 0.290.8

Two more indentation fixes verified against `dotnet format`: an object initializer's braces (both
the opening and closing `{ }`) now always line up with the computed indentation like their
members do, and a collection initializer's closing brace now stays exactly where you wrote it
even when the surrounding statement is being re-indented. Whole-repository parity: AutoMapper
86.5%.

## 0.290.7

`lint`, `format`, and `doctor` now accept a bare folder whose projects live in subdirectories with
no solution file — the same auto-discovery `dead-code` and `dead-dependencies` already did — so
all five commands honor the same "project, solution, or folder" target the help text promises.
Folders with a solution, single-project folders, and the ambiguity/empty-folder errors behave
exactly as before.

## 0.290.6

Two reported bugs fixed. `dotnet-fast lint` now follows the same rule-enablement precedence as
`dotnet format`: the bulk and per-category severity settings in `.editorconfig` only tune rules
that are already enabled — they no longer switch on analyzers that are off by default (like
CA1819 or CA2007), which only an explicit `dotnet_diagnostic.<ID>.severity` line can do. This
makes a curated `.editorconfig` behave identically under both tools. And `dead-code` now applies
its partial-scope protection to folder targets: scanning a subfolder whose projects are consumed
from elsewhere in the solution treats public API as in-use (with the existing warning) instead of
marking externally-used types as safely removable.

## 0.290.5

Two fixes for code inside `switch` statements, verified against `dotnet format`: `case` labels of
a switch nested inside another switch now indent correctly when the surrounding code is
reformatted, and collection/object initializers inside a `case` block are now left alone (or moved
with their statement) the same way they are everywhere else, instead of being force-reindented.
Whole-repository parity: Newtonsoft.Json 93.3%, Dapper 86.6%.

## 0.290.4

Six small formatting quirks fixed, each verified against `dotnet format`: the space before a
trailing empty statement in `for` loops, spacing of `..` spreads in multi-line collections, the
space after LINQ's `select` keyword before a tuple, deliberately-deep initializer braces (now left
alone like other wrapped code), a stray space appearing between `}` and a following `.Member`, and
trailing whitespace on `#if`/`#endif` lines (now preserved exactly). Whole-repository parity:
AutoMapper jumped to 85.4% (+4.5 points), Newtonsoft.Json 93.1%.

## 0.290.3

Wrapped code that you deliberately indented deeper than usual is now left alone in four more
places, matching `dotnet format` exactly: interface/class base lists (including ones whose colon
sits mid-line), `where` generic-constraint clauses, ternary operands written with trailing `?`/`:`,
and pattern-matching lines with balanced inline braces. Whole-repository parity: Serilog 93.5%,
Newtonsoft.Json 92.8%, Dapper 85.4%, AutoMapper 80.9% — with the remainder now dominated by a
handful of known one-line quirks.

## 0.290.2

A closing brace of an `if`/`for`/`try` block nested inside a `switch` case now indents to the same
depth as its own body, exactly matching `dotnet format` — including under
`csharp_indent_switch_labels = false` configurations. This was the largest remaining formatting
divergence on real-world code; Newtonsoft.Json's whole-repository parity rose to 92.5%.

## 0.290.1

Two formatting fixes found by measuring against large open-source repositories: text on directive
lines (`#region my-section-name`, `#pragma warning disable` lists) is now preserved verbatim
instead of being spaced like code, and open generic types with omitted type arguments
(`typeof(IDictionary<,>)`) no longer gain a stray space. Whole-repository byte-identical parity
with `dotnet format` now measures 100% on Polly, ~91% on Serilog and Newtonsoft.Json, 83% on
Dapper, and 79% on AutoMapper — up 7–12 points since the continuation-indent work.

## 0.290.0

Formatting now tracks .NET SDK 10.0.301's `dotnet format` exactly: tuple commas honor your
`.editorconfig` comma-spacing settings, the method-group simplifier skips explicitly-typed lambdas
and explicit generic arguments (as the SDK now does), `??`-conversion keeps the blank line between
two consecutively converted statements, and the `analyzers` command now applies `IDE0051` (remove
unused private members) — which the SDK's own formatter recently started doing. Also fixed a
long-standing native quirk where converting to `System.Threading.Lock` could insert a stray blank
line at the top of the file. The full live-oracle fixture suite is back to 144/144.

## 0.289.1

`SA1000` now handles C# 9 target-typed `new` the way modern StyleCop does: `new(@"...")` is
correct as written (no more "should be followed by a space" on it), and it's the *spaced* form
`new (...)` that gets flagged — consistent with `IDE0090`, which actively recommends `new()`.

## 0.289.0

`bom` is feature-complete: a new `--restore` flag runs `dotnet restore` for projects without a
lock file or restore output (per-project, so one failure never spoils the whole document), and
development-only dependencies (`PrivateAssets="all"` — analyzers, source generators) are now
marked with each format's proper notation (CycloneDX `excluded` scope, SPDX `DEV_DEPENDENCY_OF`)
or omitted entirely with `--exclude-dev`. In merged solutions a package counts as dev-only just
when *every* project marks it so — runtime use anywhere wins.

## 0.288.1

`SYSLIB1045` now also catches the target-typed form — `Regex TargetTyped = new(@"...")` is flagged
just like `new Regex(@"...")`, matching `dotnet format` on fields, properties, and locals.

## 0.288.0

`bom` now speaks multiple formats and versions: CycloneDX **1.4, 1.5, and 1.6** in both **JSON and
XML**, plus **SPDX 2.2 and 2.3** (JSON) — selected with `--format`, `--spec-version`, and
`--output-format`, each version emitting exactly the fields its specification defines (no
newer-spec fields smuggled into older documents). An unsupported combination fails with a message
listing what is supported. Defaults are unchanged, and every format keeps the same reproducibility
guarantee — identical inputs produce byte-identical documents.

## 0.287.0

`bom` now works at the solution level: point it at a directory, `.sln`, `.slnx`, or `.slnf` and it
produces one merged CycloneDX document — the solution as the subject, every project nested inside
it, shared packages deduplicated (with each contributing project recorded on the component), and
differing versions across projects kept as distinct entries. Projects without a lock file fall
back to `project.assets.json` (restored projects), and anything that can't be analyzed is listed
in the document with the reason — the BOM always states its own coverage.

## 0.286.0

New command: `dotnet-fast bom` generates a software bill of materials for a project. Point it at a
`.csproj` with a `packages.lock.json` and it emits a CycloneDX 1.6 JSON document — direct and
transitive packages with exact resolved versions, package URLs, SHA-512 content hashes, project
references, and the full dependency graph. Output is reproducible (the BOM serial number derives
from content, not randomness), so it diffs cleanly in CI. Solution-level generation, more formats
(SPDX, CycloneDX XML/older versions), and a restore-based fallback are on the roadmap.

## 0.285.1

`SA1206` (modifier order) now places C# 11's `required` correctly — after access modifiers,
between `unsafe` and `volatile` — so `public required string Name` is accepted as-is and
`required public` is fixed to the canonical order, agreeing with `dotnet format`, `IDE0036`,
and `RCS1019`.

## 0.285.0

New `lint --only-active-analyzers` flag: restricts findings to rules whose analyzer package your
project actually references — StyleCop rules only if `StyleCop.Analyzers` is installed, Sonar
rules only with `SonarAnalyzer.CSharp`, and so on, while SDK built-ins (`CA*`, `IDE*`, `SYSLIB*`)
stay active. This makes `lint` directly comparable to what `dotnet format` and `dotnet build`
would report. Without the flag, `lint` remains the full bundled superset — now stated plainly in
`lint --help` and the docs.

## 0.284.0

Six built-in .NET diagnostics that `dotnet format` reports are now covered by `lint` (opt-in via
the usual `.editorconfig` severity entries): `IDE0090` (use `new()` when the type is apparent),
`IDE0028` (use collection initializers), `IDE0290` (use primary constructors), `SYSLIB1045` (use
`GeneratedRegexAttribute`), `CA1873` (avoid unguarded expensive logging arguments), and `CA1822`
(member can be static — reported only when provably safe). Teams elevating these rules in
`.editorconfig` now see them in `lint` output exactly where `dotnet format` finds them.

## 0.283.2

`dead-dependencies` no longer skips an entire project just because it contains a custom MSBuild
`<Target>` — an extremely common pattern (build hooks, codegen, Docker steps). The project is now
analyzed normally, with one targeted precaution: any package actually mentioned inside a custom
target is kept, while genuinely unused packages elsewhere in the project are reported as before.

## 0.283.1

`dead-code` no longer reports a project as dead when it can't see the whole picture: scanning a
single `.csproj` (rather than its solution or repo directory) now treats the project's public API
as in-use — consumers outside the scan can't be ruled out — and says so in the report. Projects
exposing internals via `InternalsVisibleTo` get the same caution. Solution and directory scans are
unchanged; a genuinely dead internal type in a lone project is still reported.

## 0.283.0

A long-standing formatting divergence is closed: when reformatting moves the first line of a
wrapped statement, its continuation lines now move with it by exactly the same amount — matching
how `dotnet format` re-indents wrapped concatenations, call arguments, and fluent chains inside
code it touches, while still preserving your intentional alignment in untouched code.
Object-initializer members now also snap to the computed indent depth exactly as the real
formatter does. This was the dominant source of formatting differences on real-world repositories.

## 0.282.1

Four small accuracy fixes: `SA1000` now flags a `var(a, b)` deconstruction missing its space,
`SA1110` catches a method call whose opening parenthesis wrapped onto the next line, `SA1119`
flags redundant parentheses around a lambda's body (`() => (GetValue())`), and `S2486` no longer
flags an empty `catch` that has a `when` filter — the filter is the handling decision.

## 0.282.0

`SA1008` (opening-parenthesis spacing) now implements StyleCop's complete context table: it knows
when a `(` should be preceded by a space based on what the parenthesis *is* — an `if`/`while`
condition, a cast, a tuple, a lambda parameter list, a deconstruction — so `x =(1)`, `a >(b)` and
`1 +(2)` are flagged while generic calls like `Foo<int>(x)` are correctly left alone. Every case
comes with an automatic fix. This also fixed missed warnings on `var (a, b)` deconstructions and
`catch ... when (...)` filters.

## 0.281.0

Thirteen more StyleCop spacing rules now fix themselves with `lint --fix` — parentheses
(`SA1008`/`SA1009`), opening/closing braces (`SA1012`/`SA1013`), unary and dereference symbols
(`SA1014`, `SA1018`-`SA1022`), member-access dots (`SA1019`/`SA1020`), attribute brackets
(`SA1016`/`SA1017`), and the no-space-after-keyword rule (`SA1026`) — 91 rules with automatic
fixes in total. `SA1008` also learned a missing case: a ternary's `?` or `:` directly touching a
parenthesized operand (`x ?(a) : b`) is now flagged and fixed like the real analyzer.

## 0.280.0

Six token-spacing rules now fix themselves with `lint --fix`: `SA1001` (commas), `SA1002`
(semicolons), `SA1010`/`SA1011` (square brackets), `SA1015` (generic brackets), and `SA1024`
(colons) insert the missing space or remove the extra one, exactly as StyleCop's own code fix
does. `SA1015` and `SA1024` were also rebuilt from narrow checks to the full StyleCop tables —
every colon context (base lists, ternaries, labels, named arguments, interpolation format
clauses) and every generic-bracket adjacency now judged like the real analyzer.

## 0.279.0

The whitespace-spacing rules learned their other half: `SA1001` and `SA1002` now also flag a comma
or semicolon *missing* its trailing space (with StyleCop's exact exemptions — `Func<,>`,
`for (;;)`, end-of-line), `SA1010` flags a space after an opening bracket, and `SA1011` knows the
full set of tokens a closing bracket may legally touch (`)`, `,`, `;`, `.`, member access, type
arguments, interpolation braces and more). String-interpolation alignment clauses get the reversed
rule StyleCop applies (`$"{x, 5}"` is flagged, `$"{x,5}"` is fine). Also fixed: `S3400` no longer
flags methods returning `null`, and a `for` loop's empty middle clause no longer trips `SA1002`.

## 0.278.6

The whitespace-spacing rules (`SA1001` commas, `SA1002` semicolons, `SA1010`/`SA1011` square
brackets) now check code inside string-interpolation holes — `$"{Foo(a , b)}"` is flagged like any
other call, while format specifiers such as `$"{x,5:D}"` are correctly left alone. A closing
bracket at the start of a line is no longer flagged (matching StyleCop), `RCS1118` no longer fires
on `int[]`/`object` variables initialized to `null`, and `RCS0033` now catches a statement sharing
the opening brace's line.

## 0.278.5

The blank-line-between-members rules (`RCS0009`, `RCS0010`, `RCS0012`, `RCS0013`, `RCS0036`) now
handle comments and preprocessor directives between members exactly like Roslynator: a `/* */`
block comment or `#region`/`#pragma` between members suspends the whole family for that member
list (matching the real analyzer's deliberate bail-out), a comment-occupied line is no longer
mistaken for a blank line, and a `//` note above a `///` doc comment no longer confuses the
doc-adjacency rule. Fixes both missed warnings and spurious ones around commented code.

## 0.278.4

`CA1710` now picks the right suffix for types implementing several collection interfaces across
`partial` parts — the first interface in declaration order decides, exactly as the real analyzer
resolves it. `CA1708` now also catches `internal` members whose names differ only by case (its
default visibility check includes internal, not just public).

## 0.278.3

The remaining equality rules are now exact on `partial` types and beyond: `S3897`/`MA0077` no
longer flag types whose type-specific `Equals` lives in another part — and no longer misfire on
unrelated `Equals(OtherType)` overloads at all. `CA1815` now recognizes that *any* member (not just
fields and properties) makes a struct worth equality checks, and `S1206` covers structs as well as
classes — each matching its real analyzer's exact scope and reporting position.

## 0.278.2

The equality-rule family (`CA1066`, `CA1067`, `CA2231`, `MA0095`, `S4035`) no longer misjudges
`partial` types whose `IEquatable<T>` interface, `Equals` override, or equality operator lives in a
different part or file — the rules now see the whole type before deciding, like the real analyzers.
`CA1708` now catches identifiers differing only by case even when the two members are declared in
different files of a partial type.

## 0.278.1

An audit of every type-level rule against `partial` types fixed duplicate reports: naming rules
(`CA1704`, `CA1707`, `CA1711`, `AV1706`) fired once per part instead of once per type, and
`CA1704` also repeated its namespace warning in every file sharing that namespace — all now report
exactly once, matching the real analyzers.

## 0.278.0

`partial` types split across *multiple files* are now analyzed as the single type they really are:
`MA0036` and `CA1052` consider every part's members together and report once, `S1118` no longer
repeats its diagnostic on each part, and `AV1704` deduplicates across files — all matching the
compiler's one-symbol view. The interface-implementation exemption used by `MA0046`/`CA1003` now
follows interface inheritance chains (`IExtended : IBase`), exactly as far as the real analyzers
do and no further.

## 0.277.0

Generic delegates are now understood by the event-naming rules: `CA1710`, `MA0046`, and `CA1003`
substitute a generic delegate's type arguments before judging its shape, so an event declared as
`GenericDel<MyEventArgs>` is analyzed against the real resulting signature. This also fixed a false
positive where `MA0046` could flag events using perfectly compliant generic delegates. Sonar's
empty-class rule (`S2094`) gained the last piece of its exemption logic: an empty class whose base
hides its parameterless constructor (e.g. `protected`) is recognized as the deliberate pattern it
is and left alone.

## 0.276.0

`MA0046` ("use `EventHandler<T>` to declare events") and `CA1003` (generic event handler
instances) now judge events by the delegate's *actual signature* instead of its name — a delegate
correctly shaped as `void (object, EventArgs)` no longer gets flagged just for not being called
`EventHandler`, and each rule applies the real analyzer's exact exemptions (interface
implementations, overrides, COM-visible types). `CA1003` also gained its second half: delegate
*declarations* with the classic handler shape are flagged for replacement with `EventHandler<T>`,
matching the shipped analyzer.

## 0.275.0

`CA1710` now checks events, not just type names: an event whose delegate type is declared in your
own code, has the classic event-handler shape (`void (object sender, SomeEventArgs e)`), but isn't
named `…EventHandler` gets flagged at the event — exactly matching the real analyzer, including
field-like events, event properties, and interface events. Events using the BCL's `EventHandler`,
`Action`, `Func`, or delegates that don't have the handler shape are left alone.

## 0.274.0

`partial` types are now analyzed the way the compiler sees them: `MA0036` ("make class static")
and `CA1052` (static holder types) judge all of a type's same-file parts together and report once,
instead of misjudging each part in isolation. `RCS1074` no longer suggests removing a redundant
constructor that carries documentation. Primary constructors are handled precisely: a zero-argument
`class C()` / `record R()` is flagged as redundant by `S3253` (unless a base call or another
constructor needs it), and Sonar's empty-class rule (`S2094`) now exempts types with a
parameterized primary constructor — matching the real analyzers exactly in all cases.

## 0.273.0

Sonar's `S2094` ("classes should not be empty") now matches the real analyzer's exemption rules
exactly — derived from Sonar's own source at the exact analyzer version: marker names
(`…Command`/`…Event`/`…Message`/`…Query`, `AssemblyDoc`, `NamespaceDoc`), conditional-compilation
bodies, and a precise base-type policy (including a subtlety where an unqualified generic base
exempts but a fully-qualified one doesn't). `CA1710`'s full suffix table is ported (DataSet,
DataTable, sets, dictionaries, and the non-generic `IEnumerable` case), with four never-shipped
guesses removed. Records are now covered correctly by `RCS1251` (no more false flags on
parameterless records) and `CA1010`.

## 0.272.0

`CA1711` now checks the full rule: beyond the always-wrong suffixes, it flags types whose name
*claims* a kind they aren't — a class named `FooException` that doesn't derive `Exception`, a
`FooCollection` that isn't enumerable, and likewise for `Attribute`/`Dictionary`/`EventArgs`/
`Stream`/`Queue`/`Stack`/`Permission` — verified against the shipped analyzer's own source and
behavior. When the type *does* derive the right base (including through your own base classes),
it stays silent; anything the analysis can't resolve is silently kept, never guessed. Also fixed:
`CA1710` no longer flags correctly-named `…Queue`/`…Stack` collection types.

## 0.271.5

`SA1629` ("documentation text should end in a period") was rebuilt to match StyleCop exactly on
multi-line documentation: it now checks each element's full reconstructed text, anchors precisely
after the last real character, understands nested `<list>`/`<para>`/`<code>` blocks the way the
real analyzer does, and no longer mis-anchors on trailing whitespace. Empty documentation elements
split across `///` lines are now detected by five rules (`RCS1138`, `SA1610`, `SA1614`, `SA1616`,
`SA1622`).

## 0.271.4

Documentation-comment rules now handle summaries that span multiple `///` lines: empty multi-line
summaries are detected (`SA1600`/`SA1601`/`SA1606`), constructor standard-text checks work when
the phrase wraps across lines (`SA1642`/`SA1643`), and property summary prefixes were already
correct. Also fixed: `SA1606` now correctly skips `partial` types, matching StyleCop.

## 0.271.3

Twenty-one documentation-comment rules across StyleCop, Roslynator, and Meziantou now correctly
recognize a `///` doc block even when ordinary `//` comment lines sit between it and its
declaration — previously such members read as undocumented (or, for one rule, wrongly flagged as
missing a blank line). All three analyzer vendors agree on this behavior, verified against each
one's real analyzer.

## 0.271.2

Nine lint rules got accuracy fixes, all verified against the real analyzers. Five spacing rules
(`SA1001`/`SA1002`/`SA1008`/`SA1010`/`SA1011`) no longer flag punctuation inside single-line string
literals. Documentation-comment handling improved across four rules: `SA1600` recognizes a doc
comment separated from its declaration by ordinary `//` lines, `SA1514` correctly requires a blank
line when a doc header directly follows a comment, `SA1629` now matches the real rule exactly (only
a period ends a summary — `?`/`!` are flagged too), and `RCS1181` no longer suggests converting
comments that sit above an already-documented declaration.

## 0.271.1

Three lint accuracy fixes around comments, each verified against the real analyzers: `SA1120` now
understands comment blocks (a bare `//` separator between two chunks of comment text is fine —
matching StyleCop exactly, including the subtle edge cases); `SA1009` no longer flags punctuation
that appears inside comment text or string literals; and Sonar's `S4663` empty-comment rule now
follows Sonar's actual block behavior (one finding per fully-empty comment block, at its first
line) — which turns out to differ from StyleCop's rule on the very same code shapes.

## 0.271.0

Four new native analyzers from vs-threading (`VSTHRD004`, `VSTHRD112`, `VSTHRD113`, `VSTHRD115`) —
async/threading correctness rules for `JoinableTaskFactory` and `IAsyncDisposable` patterns, each
verified to match the real Roslyn analyzer exactly on position and firing behavior. Nine existing
rules also got accuracy fixes surfaced by the new test fixtures (ConfigureAwait exemptions,
namespace-less top-level classes, comment-paragraph blank lines, explicit interface
implementations). The full ported-analyzer catalog page reflects the new count.

## 0.270.0

New `dead-dependencies` finding: **DD0009 — dead framework condition**. A reference conditioned on
a target framework the project no longer declares (the classic leftover after dropping `net472`)
can never take part in any build, so it's reported and removable — even when old `#if` code still
mentions the package, which is exactly the case usage-based analysis can't see through. Indirect
or inherited framework declarations, `!=` conditions, and anything ambiguous stay silently kept.
When the same package is declared once live and once behind a dead condition, only the dead line
is touched.

## 0.269.0

`dead-dependencies` finds more unused `ProjectReference`s: an edge declared `PrivateAssets="all"`
never flows to the declaring project's own consumers, so it's now analyzed like a leaf project's
reference instead of being kept just because consumers exist. Two accuracy fixes alongside: a
source generator in a project now also protects that project's `ProjectReference`s (not just its
packages), and package "supply" is no longer counted through `PrivateAssets="all"` paths that
don't actually flow in MSBuild.

## 0.268.1

`dead-dependencies` accuracy fix for WinForms projects: a package referenced only from a `.resx`
resource file (a serialized resource typed by a package-provided class) is no longer reported
unused. WPF XAML-only usage, WinForms designer files, MAUI XAML, and Blazor `_Imports.razor`
patterns were all verified correct and are now pinned by tests.

## 0.268.0

`dead-dependencies --verify-tests` (implies `--verify`): after proving a removal still builds, the
affected test projects are run too — a package that compiles away cleanly but breaks a test at
runtime (reflection-loaded types, for example) is caught and never auto-removed. Only test projects
whose dependency chain touches a change are run. Also: air-gapped or source-less environments now
get one clear "restore unavailable" message instead of a wall of build errors — findings still
report, just unverified. Central-package-management edge cases (nested `Directory.Packages.props`,
opt-outs, explicit imports) verified against MSBuild semantics and pinned by tests.

## 0.267.1

`dead-dependencies` robustness: the restore-output reader now handles older and future SDK output
schemas, RID-specific targets, and multiple package folders — anything it doesn't recognize safely
falls back to the conservative analysis path instead of erroring or guessing. Fixed a subtle bug
where multiple NuGet package folders could be probed in the wrong order. Mixed-case package ids,
version ranges, and unusual cache paths (trailing separators, Windows long-path prefixes) are all
pinned by tests.

## 0.267.0

`dead-dependencies` improvements. **DD0006 (redundant `VersionOverride`) is now auto-fixable** —
fix mode removes just the attribute, preserving the rest of the line byte-for-byte. Projects the
command can't analyze (legacy non-SDK style, `packages.config`) are no longer silently ignored:
the JSON report lists them with reasons in a new `skippedProjects` array, and text output notes
them. Under the hood, the fixer is now covered by property-based tests over thousands of generated
project-file shapes (attribute orders, line endings, BOMs, formatting styles) proving edits never
touch anything but the intended reference.

## 0.266.0

Three `dead-dependencies` improvements. New finding **DD0008 — orphaned `GlobalPackageReference`**:
a real library pinned globally in `Directory.Packages.props` that no project uses (build-tooling
packages like SourceLink are recognized and never reported). Safer around source generators: a
package whose namespaces appear only in generator-emitted code can no longer be reported unused —
generator presence now suppresses unused-package findings it can't see past. And the command now
supports the terse agent output mode (auto-enabled in AI shells) with a one-line `fix:` command,
matching `format` and `lint`.

## 0.265.0

New `dead-dependencies` finding: **DD0007 — unused shared-import reference**. A package declared
in `Directory.Build.props` (or an imported `.targets`/`.props` file) is reported when *every*
project importing that file provably doesn't use it — previously these were always silently kept.
The rule is deliberately strict: if any importer can't be fully analyzed, if the shared file sits
above the scanned root (where invisible importers could exist), or if the reference is
conditional, nothing is reported. Fix mode removes the line from the shared file once, and the
dry-run diff names every affected project.

## 0.264.0

`dead-dependencies` gains two CI-integration features: `--format sarif` emits standard SARIF 2.1.0
(upload to GitHub code scanning or any SARIF viewer — findings appear alongside lint results as one
tool), and `--baseline <file>` accepts a previous JSON report so only *new* findings fail
`--fail-on-unused` — adopt the command on an existing codebase without fixing historic findings
first. Baseline matching ignores line numbers, so file edits don't resurrect old findings.

## 0.263.3

`dead-dependencies` knows 24 more popular packages out of the box (NLog, FluentValidation, Npgsql,
AWS/Azure SDKs, EPPlus, RabbitMQ, MessagePack, Serilog sinks, and more) — every namespace mapping
verified by reflecting over the real assemblies, including traps like `AWSSDK.Core` →
`Amazon` and `EPPlus` → `OfficeOpenXml`. Docs now show how to teach it your in-house packages via
`packageNamespaces` config.

## 0.263.2

`dead-dependencies` accuracy, validated by hand against six real open-source codebases: central
package versions referenced from shared MSBuild files (`Directory.Build.props`, custom `.targets`)
or from `#:package` directives in file-based apps are no longer reported as orphaned — this
removed 25 of 31 findings on Dapper and Polly, and every remaining finding was verified genuinely
removable (reference deleted, build still succeeds).

## 0.263.1

`dead-code` accuracy: private members called only from nested classes inside the same type are no
longer reported dead, and any spec-valid `Main` entry point (including `private static async Task
Main` in Azure Functions isolated-worker apps) is now recognized — previously only `public` `Main`
methods were safe.

## 0.263.0

New command: `dotnet-fast dead-dependencies` (alias `dead-deps`) — find dependencies your solution
declares but doesn't use: unused direct `PackageReference`s and `ProjectReference`s, plus MSBuild-side
smells (orphaned central `PackageVersion`s, duplicate references, redundant `VersionOverride`s).
Build-free and conservative: everything reported is safe to remove; anything it can't prove unused (an
analyzer/generator package, a reference shape invisible to a source scan, reflection, a package whose
public surface it can't fully see) is kept. `--fix` previews the exact csproj/props removals as a diff
(dry-run by default, `--write` applies). A new opt-in `--verify` lane copies the workspace, applies the
removals, and runs `dotnet build` to prove each removal keeps the build green — with `--fix --write
--verify`, only verified removals are written. Report-only by default; `--fail-on-unused` makes it a CI
gate. See [dead-dependencies](docs/dead-dependencies.md).
## 0.262.5

`dead-code` now understands more framework conventions out of the box: ASP.NET Core controllers
and Razor Pages models, Blazor components, AutoMapper profiles, and FluentValidation validators
are recognized as reachable even though those frameworks discover them by scanning rather than
explicit references — including through intermediate base classes (`UsersController :
ApiControllerBase : ControllerBase`). Each is gated on the framework actually being referenced, so
a look-alike class in a non-web project is still reported.

## 0.262.4

`dead-code` accuracy: types that frameworks reach only through reflection are no longer reported
dead — attribute classes applied at assembly/module level (`[assembly: MyScope]`, including the C#
`...Attribute` naming convention), EF Core `IEntityTypeConfiguration<T>` classes swept in by
`ApplyConfigurationsFromAssembly`, and `IDesignTimeDbContextFactory<T>` classes used by
`dotnet ef` design-time tooling. Genuinely-unused attribute types are still reported.

## 0.262.3

Fixed `lint --deep` with `--base`/`--ci` on branches that change only project files (`.csproj` —
e.g. automated dependency-bump PRs): these previously resolved to no project at all and produced a
confusing "no obj/project.assets.json found under ." error, silently skipping deep analysis on
exactly the PRs where new package versions can surface new analyzer findings. A changed project
file now scopes deep analysis to its own project; a shared `Directory.Build.props`/
`Directory.Packages.props` change conservatively covers every project it reaches; and a range with
no changed projects now says so clearly and skips instead of erroring.

## 0.262.2

Formatting fix: a comma inside a generic type (`Dictionary<string, SourceValue>`) could be
mistaken for a member separator and wrongly wrapped across lines, which also made a second
`format` pass report new work. Fixed — `format` output now matches the real `dotnet format` on
this shape and always converges in one pass. The full validation matrix (six real open-source
codebases, every write path) is now completely green.

## 0.262.1

Reliability release, validated end-to-end against six real open-source codebases (Newtonsoft.Json,
Dapper, FluentValidation, AutoMapper, Polly, Serilog): `dead-code --fix` now correctly keeps
extension-method containers, xUnit `[CollectionDefinition]` classes, and nested types still
referenced by surviving partial-class parts — and seven `lint --fix` edge cases that could produce
non-compiling code (around `??` with `?.`, tuple generics, empty statements as loop bodies,
`delegate {}`, `Array.Empty` without `using System`, `?.` on nullable value types, and removing
`= null` from never-assigned fields) are fixed. Every write path — `format`, `lint --fix`, and
`dead-code --fix --write` — now builds cleanly on all six repos.

## 0.262.0

`lint --fix` now auto-expands StyleCop's brace-placement family: single-line statements and
single-line method bodies reflow to properly-indented multi-line form, and an opening brace
sharing its header's line moves to its own line — 72 of 668 rules now have autofixes. Where
multiple rules flag the same construct, they produce one identical edit, never conflicting
rewrites.

## 0.261.0

Four new built-in rules that understand your type hierarchy: parameter names that differ from the
base/interface member they implement, default parameter values that don't match the base
declaration, and NUnit `TestCaseSource`/`ValueSource` strings that should be `nameof(...)` (only
suggested when the member actually exists). Also fixed an accuracy gap: the optional-parameters
rule no longer flags overrides and interface implementations, matching its upstream analyzer.

## 0.260.0

New command: `dotnet-fast dead-code` — find unused code across a whole solution (types and members
nothing in production reaches, plus a separate "test-only" category), build-free and conservative:
everything reported is safe to delete. `--fix` previews a removal diff (dry-run by default,
`--write` applies); framework-dispatched handlers (MediatR/MassTransit-style) are understood so
they don't get flagged. See [commands](docs/commands.md#dead-code).

Also: `lint --fix` auto-resolves four more rules — expression bodies moved to their own line,
single-line blocks expanded to multiple lines, embedded statements and multiple-statements-per-line
split — with correct indentation synthesized from your `.editorconfig` (or inferred from the file).
69 of 664 rules now have autofixes.

## 0.259.1

Final fix-safety hardening: on the rare constructs the tool's parser can't fully understand
(certain `#if`/`#endif` arrangements), `lint --fix` now leaves that specific spot untouched instead
of risking a bad edit — fixes elsewhere in the file still apply. With this, `lint --fix` on the
entire Newtonsoft.Json codebase produces a build that compiles cleanly, verified end-to-end.

## 0.259.0

`#pragma warning disable` is now honored: code you've explicitly suppressed with C# pragmas is no
longer reported or rewritten (previously only `dotnet-fast-disable` comments were recognized). Two
more fix-safety improvements: the `Array.Empty<T>()` rewrite is withheld on projects targeting
frameworks that don't have it (pre-.NET 4.6), and the trailing-comma fix no longer fires inside
dictionary-initializer `{ key, value }` pairs where a trailing comma isn't legal C#.

## 0.258.1

Fix-safety release: two autofixes could produce code that no longer compiles when the fixed code
sat inside `#if`/`#endif` preprocessor blocks — the trailing-comma fix could place a comma after
`#endif`, and the redundant-parentheses fix could drop directive lines. Both fixed (the first with
correct placement, the second by holding back the fix in that situation), found by validating
`lint --fix` against real open-source codebases and verifying the result still builds.

## 0.258.0

`lint --fix` now auto-resolves 18 more rules (65 of 664 total): blank-line spacing rules (extra
blank lines removed, missing ones inserted — preserving your file's CRLF/LF style and never
detaching comments), redundant `.ToString()` on strings, empty record bodies (`{ }` → `;`), and
redundant parentheses. Every fix is verified to resolve its own finding in one pass and never
change what the rules detect.

## 0.257.0

Accuracy release: three existing rules (empty methods, constant-returning methods, unused
parameters) no longer flag legitimate no-op interface implementations — matching their upstream
analyzers exactly. Two new rules from the dispose-pattern family: call `GC.SuppressFinalize` from
`Dispose` when your class has a finalizer, and make `Dispose(bool)` virtual in that situation.

## 0.256.0

Four new built-in rules that were previously only available in `--deep`, unlocked by a new
built-in knowledge base of common .NET types (verified mechanically against the real BCL): avoid
`bool` parameters in public signatures (with correct exemptions for interface implementations like
`IComparer<bool>`), seal classes with a default `Dispose`/`DisposeAsync` (two rules), and don't
declare `public` members inside non-public types. When the tool can't be certain about an external
type it stays silent rather than guessing — accuracy over volume.

## 0.255.0

Six new built-in rules from a 17th analyzer library (Gu.Analyzers): self-assignment (`x = x`),
`throw new NotImplementedException()`, enum flag values that overflow `int` (`1 << 31`), reading a
variable named `_` as a value, unnamed positional boolean arguments, and swapped
`ArgumentException` message/parameter-name arguments. Also fixed ten subtle accuracy bugs in
existing rules that the new test corpus uncovered — several rules now match their upstream
analyzers more precisely around argument-exception and `_`-variable edge cases.

## 0.254.1

Performance fix: a disabled lint rule's setup cost is now skipped entirely, not just its output.
Previously, disabling every built-in rule still paid CA1704's one-time dictionary-loading cost on
every run — measured about 2x faster (~60ms → ~30ms) on a config that disables everything.

## 0.254.0

One more built-in rule, the biggest yet: misspelled identifiers. Checks every name your code
declares (types, members, parameters, locals, and more) against a full English dictionary, splitting
`camelCase`/`PascalCase`/acronym-style names into words first, so `GetKustomerName` gets flagged on
`Kustomer` specifically.

## 0.253.0

Two new built-in rules: a `[Flags]` enum whose name isn't plural (e.g. `Permission` instead of
`Permissions`), and a non-`[Flags]` enum whose name IS plural when it shouldn't be.

## 0.252.2

Fixed another `--deep` blind spot: a file relying on an implicit `using` (bare `Console`, `Task`,
LINQ methods — the .NET SDK's default project style) could silently suppress real analyzer
findings in that file. `--deep` now sees the same implicit usings a real `dotnet build` would.

## 0.252.1

Fixed `lint --deep`/`fix --deep` silently ignoring a project's `AdditionalFiles` (e.g. a real
analyzer package's `PublicAPI.txt` or `BannedSymbols.txt`) — those analyzers now see the same files
`dotnet build` would give them, instead of reporting nothing.

## 0.252.0

One more security rule: using the Digital Signature Algorithm (DSA) — it's considered weak; the
message points you at RSA (2048-bit+), ECDH, or ECDSA instead.

## 0.251.0

Three more built-in security rules: using a weak hashing algorithm (SHA1/TripleDES/RIPEMD160),
using a broken one (MD5/DES/RC2/DSA), and a certificate-validation callback that always returns
`true` (silently disabling certificate checks entirely).

## 0.250.0

Two new built-in rules from a brand new category — security: a weak `System.Security.Cryptography`
`CipherMode` (`ECB`, `CBC`, `OFB`, `CFB`, `CTS`), and using `System.Random` for anything
security-sensitive (it isn't cryptographically random).

## 0.249.0

`build`'s CI shard matrix can now balance by actual build time instead of just project count.
New flags: `--timings <file>` (balance from a local timings file), `--use-cached-timings` (balance
from durations recorded on a prior run), `--record-timings` (save this run's per-project durations
for future runs to use), and `--timings-scope <key>`. A project with no recorded time still gets a
sensible estimate, and everything falls back to today's count-based balancing when no timing data
exists yet.

## 0.248.1

Fixed a confusing error message: an unreadable `.editorconfig`/`.globalconfig` (bad permissions,
non-UTF-8 content) used to fail with no filename at all — it now names the file.

## 0.248.0

Twelve more built-in maintainability rules: inline `new Foo().Bar()` construction, hardcoded
Windows-style absolute paths, overly complex `if`/ternary conditions, LINQ query clauses that
aren't on their own line, high method cognitive load, multiple lambdas on one line, `throw` from an
unexpected place (`Equals`, `GetHashCode`, `Dispose`, `ToString`, an exception's own constructor, a
static constructor, a finalizer, or `==`/`!=`), not passing the real sender to an event handler,
multi-line `&&`/`||` conditions with an operator at the start of a line, commented-out code, a
conversion operator to `string`, and unnecessary empty attribute parentheses.

## 0.247.0

Fifteen more built-in maintainability rules: `[ExcludeFromCodeCoverage]` usage, negative-sounding
identifier names, `Assembly.GetEntryAssembly()`, `[SuppressMessage]` usage, seven rules that flag a
type declaring an unbalanced pair of overloaded operators (e.g. `+` without `-`), merging two nested
`if` statements with no `else`, implementing a finalizer, using `ArrayList`, and reassigning a `for`
loop's variable inside the loop body.

## 0.246.0

Ten more built-in maintainability rules: discarding the result of `.ToString()`, a backwards `for`
loop, a variable literally named `_`, an empty `#region`, a `#region` inside a method body, a
property's `set`/`init` accessor listed before `get`, an unnamed tuple element, an empty `static`
constructor, a `protected` field, and a `#pragma warning` directive.

## 0.245.0

A new analyzer source: **Philips.CodeAnalysis.MaintainabilityAnalyzers**, adding 10 built-in
maintainability rules — avoid `goto` (and its target label), avoid an empty statement, avoid an
empty `catch` block, prefer `async Task` over `async void`, avoid an empty statement block, avoid a
`switch` whose only real section is `default`, avoid locking on a newly-created object, avoid
assignments inside an `if`/`while` condition, avoid duplicate `#region` names within a type, and
avoid `dynamic`.

## 0.244.0

Eleven more built-in lint rules from Roslynator.Formatting.Analyzers, rounding out that library's
blank-line/brace/attribute family: **RCS0001/0002/0005/0006/0007/0010** (blank-line placement around
embedded statements, `#region`/`#endregion`, `using` lists, accessors, and declarations),
**RCS0016** (attribute list on its own line), **RCS0025** (full accessor on its own line),
**RCS0029** (constructor initializer on its own line), **RCS0048** (collapse a single-element
initializer onto one line), and **RCS0057** (normalize leading file whitespace). Also fixes two
existing rules that were missing real-world cases: **CA1041** (Obsolete needs a message) now
correctly reads an unmodified accessor's/enum member's visibility instead of assuming private; **SA1134**
(attributes shouldn't share a line) now also catches an attribute sharing a line with the member it
decorates, not just with another attribute.

## 0.243.0

Seven new built-in lint rules from Roslynator.Formatting.Analyzers: **RCS0033** (put a statement on
its own line), **RCS0023** (reflow a type declaration's single-line braces), **RCS0013** (blank
line between adjacent single-line declarations of a different kind), **RCS0020** (reflow an
accessor's single-line braces), **RCS0049** (blank line after a file's leading comment),
**RCS0036** (remove a redundant blank line between same-kind single-line declarations), and
**RCS0009** (blank line before a documentation comment).

## 0.242.33

One new built-in lint rule: **NUnit2009** flags an NUnit assertion that compares a value against
itself (`Assert.That(x, Is.EqualTo(x))` — always passes, tests nothing), covering the whole
constraint surface (`Is.Not.EqualTo`, `Is.GreaterThan`, `Is.SameAs`, chained `.And` parts).

## 0.242.32

Eight new built-in lint rules. Six more from Roslynator.Formatting.Analyzers — **RCS0062**
(expression body on its own line), **RCS0021** (single-line block braces), **RCS0012** (blank line
between single-line declarations), **RCS0030** (embedded statement on its own line), **RCS0008**
(blank line after a closing brace), and **RCS0063** (remove unnecessary blank line). Plus **AV1010**
(member hides an inherited member with `new`) and **MA0182** (internal type that is never used
anywhere in the project — the first rule powered by a new project-wide declaration scan, which
looks across all your files, not just one at a time, while staying instant and dependency-free).

## 0.242.31

Two new built-in lint rules from Roslynator.Formatting.Analyzers: **RCS0024** flags a `switch`
`case`/`default` label whose statement starts on the same line, and **RCS0031** flags an `enum`
member that isn't on its own line.

## 0.242.30

Fixed another real formatting bug, found the same way as the last one: a user-defined `checked`/
`unchecked` operator declaration (`operator checked -(...)`, C# 11+ generic-math operators) was
getting a stray space before its parameter list (`operator checked - (...)`) instead of staying
tight like every other operator declaration.

## 0.242.29

Fixed a real formatting bug: a generic type with a parameterless-constructor constraint (`class
Cache<TKey, TValue> where TValue : new()`) was silently confusing the formatter into skipping all
spacing normalization for the rest of that class or method body — every line inside kept whatever
spacing it started with, no matter how messy. `whitespace`/`format` are back to exact parity with
`dotnet format` on the full test corpus.

## 0.242.28

Internal performance improvement: the spacing/trivia lint rules (the ones that check things like
"no space before this comma" or "no trailing whitespace") used to each independently re-scan the
whole file for strings and comments. They now share that work, computing it once per file instead
of once per rule.

## 0.242.27

24 more built-in lint rules from NUnit.Analyzers, completing the `ClassicAssert.*` migration
family: every legacy `ClassicAssert.AreEqual`/`AreSame`/`Null`/`Greater`/`Zero`/`Positive`/
`IsInstanceOf`-and-friends call now has a matching rule pointing at the modern
`Assert.That(...)` constraint model.

## 0.242.26

Six new built-in lint rules from a new source, **NUnit.Analyzers**: flags for the legacy
`ClassicAssert.False/IsFalse/IsTrue/True`, `StringAssert`, and `CollectionAssert` calls — all
superseded by the modern `Assert.That(...)` constraint model.

## 0.242.25

Fixed a real bug: every SARIF report written by `lint`, `format`, `affected`, and `doctor`
(`--sarif <file>`) linked back to the private source repository — anyone clicking through from
their GitHub code-scanning results would hit an inaccessible link. Now points at the public repo.
Also added a guide showing how to wire any of those four commands' SARIF output into GitHub code
scanning — see [code-scanning.md](https://github.com/RDalziel/dotnet-fast/blob/main/docs/code-scanning.md).

## 0.242.24

Four new built-in lint rules from a new source, **IDisposableAnalyzers**: flags for the standard
`IDisposable` dispose-pattern shapes — `GC.SuppressFinalize` called with the wrong argument, a
`Dispose(bool)` call passing the wrong value, and a pointless `GC.SuppressFinalize(this)` in a
sealed type with no finalizer.

## 0.242.23

Fixed a real bug: `--ci-base last-successful-build` only worked for uncommon Azure Pipelines trigger
types — on standard batched branch-trigger CI (the common case) it always fell through to its
fallback, silently, because the field it read was never populated there. It now also reads the
build's `sourceVersion`, which Azure always populates, so the optimization actually engages on a
normal batched pipeline. It also now logs exactly which build it resolved and how it chose the
baseline, so a wrong result is diagnosable from the pipeline log instead of the Azure REST API. One
more built-in lint rule too: **AV1706** flags a single-letter identifier name or a common
abbreviation (`Btn`, `Ctrl`, `Len`, `Idx`, and 30+ more) anywhere in a name.

## 0.242.22

One more built-in lint rule: **AV1225** flags an event raised outside a dedicated `protected
virtual OnEventName` method — including from an accessor, constructor, or local function, or from
a differently-named or under-modified method.

## 0.242.21

Three small additions that cut hand-rolled PowerShell out of Azure Pipelines: `affected
--set-variable <name>` / `--set-count-variable <name>` emit native Azure DevOps `##vso` variable
commands directly, instead of a pipeline parsing `--format count`'s stdout and echoing the command
by hand; `lint`/`build`/`test-plan` now accept `--all` as a no-op so one resolved range string can
feed both `affected` and these commands without special-casing; and `test-plan
--exit-zero-on-empty` mirrors `affected`'s existing flag, turning the "nothing to shard" exit code
into a clean success.

## 0.242.20

Fixed a real bug reported from production use: `affected --ci --ci-base last-successful-build` (the
Azure Pipelines batched-trigger baseline) used to silently narrow to the parent commit when the
Build API lookup couldn't complete — a missing token, or no prior successful build — which under-builds
a batched trigger without any warning that matters. New `--ci-base-fallback all|error|previous-commit`
(default `previous-commit`, unchanged) lets you opt into a conservative full build or a loud failure
instead. Also confirmed and documented that `cache ensure-access` is safe to call from every shard of
a parallel test matrix, including the very first cold run.

## 0.242.19

One more built-in lint rule: **AV1704** flags a digit in a type, method, property, field, event,
local function, or local variable name (like `field1` or `Scout56`) — common names like `Int32`,
`Utf8`, or `Point3D` are allowed.

## 0.242.18

One more built-in lint rule: **AV1708** flags a type name containing a vague term like `Helper`,
`Utility`, `Common`, or `Shared` — a hint the type is a grab-bag rather than a focused noun.

## 0.242.17

Four more built-in lint rules from CSharpGuidelinesAnalyzer: **AV1561** flags a method with more
than 3 parameters (or any tuple-typed parameter), **AV1739** flags an unused lambda parameter that
should be renamed to `_`, **AV1115** flags a method name with "and" in the middle (like
`SaveAndClose`, suggesting it does more than one thing), and **AV1008** flags an unnecessary static
class.

## 0.242.16

Six new built-in lint rules from a new source, **CSharpGuidelinesAnalyzer**: **AV1500** flags a
method with more than 7 statements, **AV1535** flags a `switch` `case`/`default` clause missing its
`{ }` block, **AV1537** flags an if/else-if chain missing a final unconditional `else`, **AV1562**
flags `ref`/`out` parameters, **AV1710** flags a member whose name repeats its containing type's
name, and **AV2407** flags any use of `#region`.

## 0.242.15

New built-in lint rule: **MA0204** flags an unnecessary `partial` modifier on a type declared in
just one place.

## 0.242.14

Two more built-in lint rules: **MA0192** flags the `(x & Flag) == Flag` enum bitwise idiom, suggesting
`x.HasFlag(Flag)` instead. **MA0196** flags a bare `<inheritdoc/>` on a member that has nothing to
inherit documentation from.

## 0.242.13

Two more built-in lint rules: **MA0184** flags an interpolated string with no `{...}` placeholders
(it's just a plain string with a stray `$`), and **S3267** flags a `foreach` loop whose body is
nothing but a single conditional add/action — usually clearer as a LINQ `.Where(...)` filter.

Also fixed ten existing lint rules that were silently never recognizing `foreach` loops due to an
internal naming mismatch — they now correctly apply to `foreach`, not just `for`/`while`/`do`.

## 0.242.12

Two more built-in lint rules: **MA0203** flags a `<returns>` doc tag on a `void` method (meaningless
since there's nothing to return), and **MA0206** flags an empty-body `class`/`struct`/`record`
declaration whose braces add nothing.

## 0.242.11

Refreshed the underlying SonarAnalyzer/Roslynator/Meziantou rule packs behind three existing lint
rules, matching their latest published behavior:

- **S1481** (unused local variable) now also flags an unused `out var x` declaration.
- **S1871** (duplicate implementation) now also catches two branches of an `if`/`else if` chain
  with identical bodies, not just duplicate `switch` sections.
- **RCS1077** (optimize LINQ call) now also flags an identity-key `OrderBy(x => x)`, suggesting
  `Order()`.

No new commands or flags — this release is entirely about the built-in lint rule catalog staying
current with its upstream sources.

## 0.242.9

Two more built-in lint rules: **EF1002** flags an interpolated SQL string passed to a raw-SQL
Entity Framework Core method (losing auto-parameterization), and **EF1003** flags the same risk
for a concatenated SQL string — both a SQL-injection hazard.

## 0.242.8

New built-in lint rule: **AsyncFixer02** flags a blocking `Thread.Sleep(...)` call inside an `async`
method (a narrower sibling of the existing MA0040-family CancellationToken/async rules — use
`await Task.Delay(...)` instead).

## 0.242.7

New built-in lint rule: **CA2016** flags a `CancellationToken` that's available in scope but not
forwarded to a callee's matching overload — Microsoft's equivalent of the existing MA0040 rule.

## 0.242.6

New built-in lint rule: **MA0003** flags a bare `null`/boolean argument passed positionally instead
of by name (e.g. `Foo(true)` instead of `Foo(enabled: true)`), which hurts call-site readability.

## 0.242.5

New built-in lint rule (first milestone): **S3776** flags a method whose Cognitive Complexity —
nested and branching control flow — is too high, so it's harder to follow than it needs to be.

## 0.242.4

New built-in lint rule: **MA0143** flags a primary-constructor parameter that gets reassigned
somewhere in the type body, which is surprising for what reads like a constructor argument.

## 0.242.3

New built-in lint rule: **MA0040** flags a `CancellationToken` that's available in scope but not
forwarded to a callee's matching overload, silently breaking cancellation propagation.

## 0.242.2

Two more built-in lint rules: **S125** flags commented-out code, and **MA0099** flags a bare `0`
used where an explicit enum value would be clearer. The existing **CA1707** rule (no underscores in
identifiers) was also broadened to cover delegate names, enum members, and method parameters.

## 0.242.1

Fix: `affected --ci-base last-successful-build` no longer hard-errors when the Azure DevOps access
token is present but invalid or expired — it now falls back gracefully to the previous-commit
baseline, matching the existing behavior for a missing token.

## 0.242.0

Azure Pipelines batched branch builds can now resolve their own affected base:

- **`affected --ci --ci-base last-successful-build`** queries the Azure DevOps Build API for the most
  recent completed+succeeded build of the same definition and branch, then uses
  `triggerInfo['ci.sourceSha']` as the comparison base.
- **`--on-missing-base error|all|empty`** controls unresolvable explicit bases after shallow-clone
  recovery. Use `all` for the conservative full-build fallback when a supplied `--from`, `--to`, or
  `--base` is pruned, missing, or otherwise unreachable.
- The Azure CI docs now use the built-in flag instead of a hand-written PowerShell Build API query.

## 0.240.3

Fix for explicit-range linting on shallow CI checkouts:

- **`lint --from <rev> --to <rev>` now deepens before diffing** when the named revision is missing from
  a shallow clone, matching the documented `affected` / `build` / `test-plan` behavior from 0.240.0.
- The direct `from..to` comparison is preserved; this does not switch explicit ranges to merge-base
  semantics.

## 0.240.2

Documentation-only release for Azure Pipelines batched builds:

- The CI recipe now resolves the comparison base from the previous successful build's
  `triggerInfo['ci.sourceSha']`, falling back to `sourceVersion` when older build records do not carry
  trigger metadata.
- The recipe now lists the required Azure variables and clarifies that completed-build queries already
  exclude the currently running build.

## 0.240.1

`affected --ci` is safer on GitHub Actions push builds:

- **GitHub push events now use the event `before` SHA** when it is available, so batched or
  multi-commit pushes compare the whole pushed range instead of only `HEAD~1`.
- **Branch-creation pushes still fall back safely**: GitHub's all-zero `before` sentinel is ignored,
  preserving the existing parent/root behavior.
- **CI docs now call out batched-build limits** for providers that do not expose a pre-push SHA, with
  an Azure Pipelines recipe for passing `--from <last-successful-sha>`.

## 0.240.0

`--from` / `--to` ranges now behave better on shallow CI checkouts:

- **Explicit revisions auto-deepen** — if a named `--from` or `--to` commit is missing from a shallow
  clone, `affected`, `lint`, `build`, and `test-plan` deepen the checkout and retry before giving up.
  The comparison remains the direct `from..to` range; it does not switch to merge-base semantics.

## 0.239.0

Four more analyzers ported to the native path (**502 total** — past the 500 mark), each at exact parity
with the real Roslyn analyzer:

- **MA0004** — await a task with `.ConfigureAwait(false)`; a bare `await` captures the synchronization
  context and can deadlock library callers (the Meziantou companion to CA2007, also covering
  `await using`).
- **RCS1208** — a trailing `if (c) { … }` in a `void` method should be inverted into a guard
  (`if (!c) return; …`) to reduce nesting.
- **RCS1056** — avoid `using X = …;` alias directives; they hide a type's real name from readers.
- **S6354** — `DateTime.Now`/`UtcNow`/`Today` reads the ambient clock; inject a testable time provider
  so code stays unit-testable.

## 0.238.0

Three more analyzers ported to the native path (**498 total**), each at exact parity with the real
Roslyn analyzer:

- **S2342** — enum names should be PascalCase, and a `[Flags]` enum should be plural (end in `s`)
  since it names a set of values.
- **S4022** — an enum with a narrower-than-`Int32` storage type (`byte`/`short`…) should just use
  `Int32`; the smaller type saves nothing and invites interop surprises.
- **S1858** — calling `.ToString()` on a value that is already a string is redundant.

## 0.237.0

Four more analyzers ported to the native path (**495 total**), each at exact parity with the real
Roslyn analyzer:

- **CA2007** — await a task with `.ConfigureAwait(false)`: a bare `await` captures the synchronization
  context and can deadlock callers of library code.
- **RCS1080** / **CA1860** — an array's `.Any()` should use the `Length` property (compare to 0):
  O(1) instead of materialising an enumerator.
- **VSTHRD200** — follow the `Async` naming convention: methods that return an awaitable end in
  `Async`, and methods that don't, don't.

## 0.236.0

Three more analyzers ported to the native path (**491 total**), each at exact parity with the real
Roslyn analyzer:

- **RCS1130** — a bitwise `&`/`|`/`^` on an enum that is not marked `[Flags]` is almost always a
  mistake (the enum was never designed to be combined).
- **RCS1242** — do not pass a non-`readonly` struct by `in`: the compiler makes a defensive copy on
  every member access, defeating the point of `in`.
- **MA0089** — `string.Join(",", …)` with a single-character string separator should use the `char`
  overload (`string.Join(',', …)`) to skip an allocation.

## 0.235.0

Four more analyzers ported to the native path (**488 total**), each at exact parity with the real
Roslyn analyzer:

- **S1696** — `NullReferenceException` should not be caught (catching it hides the real bug — a missing
  null check).
- **S2123** — values should not be uselessly incremented: `i = i++;` assigns the *old* value back, so
  it does nothing.
- **S2386** — mutable fields should not be `public static` (an array or mutable-collection field is
  global shared state any caller can corrupt).
- **S3962** — a non-visible `static readonly` field with a constant initializer should be `const`.

All four are report-only. As always, ported analyzers run in the default Roslyn-free `lint` path and are
configurable per category via `.editorconfig`.

## 0.234.0

Three more analyzers ported to the native path (**484 total**), each at exact parity with the real
Roslyn analyzer:

- **CA1716** (identifiers should not match keywords) — flags a namespace or an externally-visible type
  whose name matches a reserved keyword in C#, Visual Basic, or C++/CLI (case-insensitive), so it's
  usable from other languages.
- **RCS1097** / **MA0044** (remove redundant/useless `ToString` call) — a no-argument `.ToString()` on a
  value that is already a `string` does nothing; flagged when the receiver is a `string`-typed parameter
  or local.

All three are report-only. As always, ported analyzers run in the default Roslyn-free `lint` path and can
be enabled/disabled per category via `.editorconfig`.

## 0.233.0

`--deep` that scales with change, not project count.

- **`lint --deep --deep-cache`** caches each project's Roslyn analyzer diagnostics in the build cache, so
  a project whose inputs and rules are unchanged restores its diagnostics as a blob lookup instead of
  re-binding the semantic model from source. On a cache-backed pipeline `--deep` previously re-analysed
  every project even when the build was a 100% cache hit; now it scales with change — a warm PR touching a
  few projects drops from minutes to seconds.
- The cache key is **strictly larger** than the binary cache: the project's build-cache key (source,
  references, SDK, and analyzer versions via `packages.lock.json`) **plus** its `.editorconfig` chain
  (rule severities) **plus** the analyzer-host identity. A severity edit or an analyzer-package bump moves
  the key, so stale diagnostics are never served. Diagnostic locations are repo-relative, so the cache is
  portable across agents.
- Requires a configured build cache; it respects `buildCache.readOnly` (PR runs consume but don't upload —
  the trusted/main build is the writer). **Opt-in**, since a few analyzers aren't deterministic.

## 0.232.1

Fixes for `lint --ci` on shallow / no-base CI runs, and the build cache now restores NuGet state.

- **`lint --ci` no longer silently analyses the whole repo when no base resolves.** On a manual or
  force-full run with a shallow checkout (no PR base), it used to fall back to git's empty tree —
  "everything changed" — and could then crash diffing a committed binary file. It now **fails fast** with
  a clear message (deepen the checkout, pass `--from`/`--base`, or drop `--ci` to lint the whole tree). A
  genuine first commit still compares against the empty tree as before.
- **Binary files in the changed set no longer break changed-line scoping.** The diff that scopes findings
  to changed lines is restricted to `.cs` files, so a committed `.docx`/image is never diffed.
- **The build cache now restores NuGet state** (`project.assets.json` + the generated NuGet
  props/targets), so a cache-restored tree is valid for `--no-restore` consumers — `lint --deep` and
  `dotnet publish --no-build` — without a separate `dotnet restore`. (Assumes cache producer/consumer
  share an agent image and checkout path; otherwise the step just asks for a restore, as before.)

## 0.232.0

Build sharding across agents, and cache-key transparency.

- **Shard the build across agents** — `dotnet-fast build --format matrix` / `--auto-shards` partitions
  the affected build's transitive closure into **topological layers** (waves) and shards each layer, so
  a cold or foundation build no longer compiles the whole miss set serially on one agent. Each agent
  runs `build --layer L --shard I --of N` to build its slice; its dependencies (built by earlier waves)
  restore from the cache. The pipeline runs the layers as sequential waves. Mainly helps cold caches and
  foundation changes — warm PR builds are already restore-dominated. (The exact multi-wave CI YAML is
  still being validated against real Azure DevOps pipelines.)
- **Cache key-format version is surfaced** — the build cache key folds in a *stable* key-format version,
  **not** the `dotnet-fast` tool version, so a routine upgrade keeps every cache entry warm. It now shows
  as `keyformat=…` in the `build`/`build --plan` header and `keyFormatVersion` in `--json`, so an
  (uncommon) incompatible format change is visible before the cold run instead of being discovered on a
  PR. Docs clarify that enabling caching for *more* projects (e.g. v0.230's publish-target fix) causes a
  one-time build+upload of the newly-cacheable projects, then they restore — not a recurring invalidation.

## 0.231.0

Per-agent test restore, scoped to the shard.

- **`test-plan --exec --restore-from-cache`** — a sharded test stage no longer restores the *whole*
  affected build tree on every agent. This restores only **the shard's project closure** (its test
  projects plus their transitive dependencies) from the build cache before running, so each agent pulls
  just its slice instead of the full tree — a large download cut on wide fan-outs — and the separate
  `build --read-only` restore step disappears. `--fallback` builds the shard's closure if the cache is
  unconfigured or unreachable. Match the cache's configuration in `--test-args` (e.g. `-c Release`).
- **Build report surfaces the dependency closure** — a scoped `build --projects-file …` builds the
  selected set *and* its transitive dependencies; the report now lists those dependency projects
  separately (a `+ N dependency project(s) (closure): X restored, Y built` line, and `dependencyProjects`
  in `--json`) instead of hiding them behind the selected set. The selected list and cache keys are
  unchanged.
- Docs: an Azure DevOps Pipelines wall-clock baseline alongside the GitHub Actions one, both using the
  new shard-scoped restore.

## 0.230.0

PR lint, scoped to the lines you actually changed.

- **Changed-line (hunk) lint scoping** — on a pull request (`--ci` PR, `--pr-base`, `--base`) and for
  `--staged`, `dotnet-fast lint` now reports only findings on the lines your branch changed. Touching a
  file no longer surfaces its whole backlog of pre-existing findings on lines you never edited — the
  noise that grows the longer a branch lags `main`, and reads as "errors that aren't from my change".
  Newly-added files are wholly in scope; a pure rename falls back to file scope.
- **`--whole-file`** opts back out (report a touched file's full backlog), and **`--changed-lines`**
  forces the same scoping on an explicit `--from`/`--to` or `--ci` push build. Scoping affects reported
  findings only (native and `--deep`); `--fix` still rewrites whole files.
- Build-cache fix: a publish-phase custom `<Target>` (e.g. `AfterTargets="Publish"`) no longer disables
  build caching for the whole service layer — only true build-phase targets do (#90).

## 0.229.0

CI-ergonomics release — four build-cache / pipeline features that cut hand-written CI scaffolding:

- **One `build` invocation for every cache state** — `dotnet-fast build --fallback` runs a plain
  `dotnet build` when the cache is unreachable or unconfigured (instead of failing), and `--report
  <dir>` captures the hit/miss report. So one call covers cache-hit, cache-miss, and cache-down — no
  hand-written probe / fallback / report branch.
- **`dotnet-fast cache ensure-access`** — grants the current identity the **Storage Blob Data
  Contributor** role on the cache's storage account via ARM, using the same credential the build
  already uses. Removes the `AzureCLI@2` wrapper teams add purely to set up RBAC. Idempotent.
- **Cached test timings** — `test-plan --record-timings` uploads a shard's per-fixture durations to the
  build-cache blob, and `--use-cached-timings` balances future shards by that time. Timing-balanced
  sharding becomes two flags on calls the pipeline already makes — no timings file in YAML, no artifact
  publish/download. Keyed per branch so a PR balances against trunk's history.
- **Standalone binary release asset** — every tagged release now ships a self-contained
  `dotnet-fast-<rid>` executable (plus `.sha256`). CI matrices can download/cache it once and skip
  `dotnet tool restore` on every agent.

## 0.228.0

- **9 more analyzers ported — 481 total**, each verified at exact parity vs the real Roslyn analyzers:
  - **`SA1506` / `SA1509` / `SA1511`** — a blank line should not sit after a `///` doc header, before an
    opening `{`, or before a `do`/`while` footer.
  - **`SA1604` / `RCS1139`** — an element's documentation should have a `<summary>` (StyleCop and Roslynator
    both flag a `///` comment that has none and is not an `<inheritdoc>`).
  - **`SA1626`** — single-line comments should not use documentation style slashes (a `///` embedded in a
    method body).
  - **`SA1608`** — documentation should not keep the IDE's default `Summary description for …` text.
  - **`SA1613`** — a `<param>` element should declare a parameter name.
  - **`SA1651`** — do not use `<placeholder>` elements in documentation.
- **Parity corrections to neighbouring rules**: `RCS1036` now also flags a blank line directly after a `///`
  doc header, and `SA1629` no longer mis-flags documentation whose content is a nested element.

## 0.227.0

- **9 more analyzers ported — 472 total**, each verified at exact parity vs the real Roslyn analyzers:
  - **`SA1519` / `RCS1001`** — braces should not be omitted from a multi-line child statement (StyleCop and
    Roslynator both flag it).
  - **`SA1108`** — block statements should not contain embedded comments (a comment in a control-statement
    header, like `while (b) // note`).
  - **`SA1134`** — attributes should not share a line (`[A][B]` → one per line).
  - **`SA1113`** — a comma should be on the same line as the previous parameter, not leading the next line.
  - **`RCS1226`** — add a `<para>` to a documentation comment whose `<summary>` has two blank-line-separated
    paragraphs.
  - **`RCS1069`** — remove an unnecessary `case` label that shares its section with `default`.
  - **`SA1114`** — a parameter list should follow the declaration, not begin after a blank line.
  - **`SA1510`** — chained statement blocks (`else`/`catch`/`finally`) should not be preceded by a blank line.
- **Parity corrections to neighbouring rules**: `SA1503` no longer double-flags a multi-line un-braced body
  (that is `SA1519`'s), and `RCS1036` now also flags a blank line before an `else`/`catch`/`finally`.

## 0.226.0

- **9 more analyzers ported — 463 total**, each verified at exact parity vs the real Roslyn analyzers:
  - **`SA1004` / `SA1014` / `SA1026`** — documentation-line, generic-bracket, and `new[]` spacing should be
    written without stray spaces.
  - **`S2971` / `MA0029` / `RCS1077`** — a `Where(predicate).Count()` chain (or `.Any()`, `.First()`, …) folds
    the predicate into the terminal call. Sonar, Meziantou, and Roslynator each flag this; MA0029 also flags a
    terminal that already carries its own argument, and anchors at the start of the chain.
  - **`MA0025`** — implement the functionality instead of leaving a `throw new NotImplementedException()`
    placeholder.
  - **`SA1120` / `S4663`** — comments should contain text (an empty `//` line comment is just noise). StyleCop
    and Sonar both flag it.
- **Parity correction**: `SA1010` no longer fires on the `[` of an implicit `new[]` array (that spacing is
  `SA1026`'s rule).

## 0.225.0

- **8 more analyzers ported — 454 total**, each verified at exact parity vs the real Roslyn analyzers:
  - **`RCS1136`** — merge adjacent switch sections with equivalent (small) content.
  - **`SA1209` / `SA1216`** — using alias / `using static` directives placed in the right group (the order is
    namespace, then static, then alias).
  - **`SA1208`** broadened — a `System` namespace using must precede *every* other using, including aliases and
    `using static` (not just other namespace usings).
  - **`S1172`** — unused parameters of effectively-private methods and local functions (the Sonar counterpart
    of RCS1163).
  - **`MA0136`** — a multi-line raw string literal (`"""…"""`) embeds the source file's line endings.
  - **`SA1016` / `SA1017` / `SA1018`** — attribute brackets and the nullable `?` symbol should not carry stray
    spaces (`[ Obsolete]`, `[Obsolete ]`, `int ?`).
- **Parity corrections to neighbouring rules**: `S1871` no longer flags a one- or two-statement duplicate
  switch section (that is `RCS1136`'s merge), and `SA1011` no longer fires on an attribute's closing bracket
  (that is `SA1017`'s spacing rule).
- **Broader coverage for existing rules** (since 0.224.0): `S1481` now flags unused declaration-pattern and
  tuple-deconstruction variables; `RCS1212` flags a literal-initialised dead store; the using-ordering rules
  `SA1210`/`SA1211`/`SA1217` landed; and `S1144`/`RCS1163`/`RCS1141` gained local-function and
  primary-constructor coverage.

## 0.224.0

- **3 more analyzers ported — 446 total** (the StyleCop using-ordering family), each verified at exact parity
  vs the real Roslyn analyzers:
  - **`SA1210`** — "Using directives should be ordered alphabetically": plain namespace usings should be
    alphabetical within the System-first grouping (`System.*` first, then the rest).
  - **`SA1211`** — "Using alias directives should be ordered alphabetically by alias name."
  - **`SA1217`** — "Using static directives should be ordered alphabetically by type name."
- **Broader coverage for existing rules** (since 0.223.0), each still at exact parity:
  - **`S1481`** (unused local) now also flags an unused declaration-pattern variable (`o is int n`,
    `case int n:`, switch-expression arms) and an unused tuple-deconstruction element (`var (a, b) = …`).
  - **`RCS1212`** (redundant assignment) now also flags a literal-initialised dead store
    (`int v = 0; v = 1;`).
  - **`S1144`** / **`RCS1163`** now cover unused **local functions** and their parameters; **`RCS1141`** now
    covers class/struct **primary-constructor** parameter docs.

## 0.223.0

- **4 more analyzers ported — 443 total**, each verified at exact parity vs the real Roslyn analyzers:
  - **`RCS1170`** — "Use read-only auto-implemented property": an auto-property `{ get; set; }` whose setter
    is effectively private and is only ever assigned in a constructor can drop the setter (`{ get; }`).
  - **`S3264`** — "Events should be invoked": a field-like event that is declared but never raised (only
    subscribed to with `+=`/`-=`) is dead — either invoke it or remove it.
  - **`MA0159`** — "Use `Order` instead of `OrderBy`": sorting by the element itself (`OrderBy(x => x)`,
    `OrderByDescending(x => x)`, or an identity `orderby x`) is the identity ordering, better written with
    the `Order()` / `OrderDescending()` operators.
  - **`RCS1181`** — "Convert comment to documentation comment": a `//` comment that documents a type or
    member (leading it, or trailing it on the same line) should be an XML `///` doc comment. `// TODO`-style
    task comments and comments inside method bodies are left alone.
- **Broader coverage for existing rules**, each still at exact parity:
  - The unused-member rules **`S1144`** and **`RCS1213`** and the combine-fields rule **`SA1132`** now also
    flag field-like **events** (an unused private event, or a combined `event T A, B;`).
  - The empty-documentation-element rules **`RCS1228`**, **`SA1614`**, **`SA1616`**, and **`SA1622`** now fire
    on **every kind of declaration** (types, records, constructors, indexers, delegates, operators, …), not
    just methods and properties.
  - **`S3257`** also flags an unused parenthesized-lambda parameter (`(a, b) => …`) that should be a discard
    (`_`); **`RCS1163`** (unused parameter) now covers unused lambda parameters too.

## 0.222.0

- **2 more analyzers ported — 439 total**, each verified at exact parity vs the real Roslyn analyzers:
  - **`RCS1005`** — "Simplify nested using statement": a `using` whose block holds only another `using` can be
    stacked as `using (a) using (b) { … }`.
  - **`RCS1118`** — "Mark local variable as const": a local whose initializer is a compile-time constant (a
    literal, `nameof(…)`, a `const` in scope, or arithmetic over those) and that is never reassigned can be
    declared `const`.
- **`RCS1179` parity fix** — no longer flags a bare `if` with no `else`; the "assign in every branch" pattern
  requires the `if` to have an `else`/`else if`.

## 0.221.0

- **5 analyzer parity corrections** (no new ports — still **437 total**), each verified at exact parity vs
  the real Roslyn analyzers and found by stress-testing modern C# constructs:
  - **`S3400`** no longer flags `T M<T>() => default` — `default(T)` of a type parameter is not a constant.
  - **`SA1011`** now flags a nullable array type `string[]?` (the `]` should be separated from the `?`).
  - **`CA1003`** / **`MA0046`** now check custom events too, and treat `EventHandler<int>` as non-conventional
    (the type argument must be an `EventArgs` type) — only `EventHandler<TEventArgs>` is exempt.
  - **`RCS1047`** no longer flags a non-`async` method whose name ends in `Async` when it returns an awaitable
    type (`Task`/`ValueTask`/`IAsyncEnumerable`) — such a method is asynchronous by signature.

## 0.220.0

- **9 analyzer parity corrections** (no new ports — still **437 total**), each verified at exact parity vs
  the real Roslyn analyzers and found by stress-testing modern C# constructs:
  - **`S4487` / `S3459`** (field usage): `!field` / `-field` are reads, not writes; and a field initialized
    in a constructor or finalizer is no longer reported as write-only.
  - **`RCS1085` / `S2292`** (auto-property): expression-bodied accessors (`get => f; set => f = value;`) are
    now recognised; `S2292` leaves a property with a `private set` alone.
  - **`S3257`** no longer flags a jagged array's outer type (`new int[][] { … }`) or a multidimensional array.
  - **`S818`** flags only a standalone lower-case `l` suffix (`1l`); `u`/`U` and combined `1ul`/`1lu` are fine.
  - **`S3240`** / **`RCS1211`** no longer fire on `else if` chains; **`S1751`** no longer flags a loop that can
    `continue` and iterate.

## 0.219.0

- **9 analyzer parity corrections** (no new ports — still **437 total**), each verified at exact parity vs
  the real Roslyn analyzers and found by stress-testing modern C# constructs:
  - **`MA0008`** now counts auto-properties (`int A { get; }`) as instance fields, so a `struct` with two
    auto-properties and no `[StructLayout]` is flagged.
  - **`CA1034`** no longer flags nested `enum`/`delegate` types (a nested enum/delegate is conventional API).
  - **`CA1046`** / **`S3875`** (operator `==` on a reference type) now exempt a class that opts into value
    equality — CA1046 on an `Object.Equals` override *or* `IEquatable<T>`, S3875 on `IEquatable<T>`.
  - **`SA1201`** orders conversion operators before regular operators (StyleCop's element order).
  - **`CA1041`** (Obsolete message) fires only on externally-visible API — private members and local
    functions are exempt; **`MA0070`** exempts only local functions.
  - **`CA1710`** requires the `Collection` suffix only for a directly-named `ICollection` (a bare
    `IEnumerable`/`IList` implementer is left alone).
  - **`SA1011`** flags a `]` immediately followed by the null-forgiving `!` (`map[k]!`).
  - **`RCS1244`** (simplify `default(T)`) no longer flags it as a method argument, cast operand,
    collection element, or variable initializer (where the type is needed).

## 0.218.0

- **4 more analyzers ported — 437 total** (across three batches), each verified at exact parity vs the real
  Roslyn analyzers:
  - **`RCS1048`** (fixable) — "Use lambda expression instead of anonymous method": `delegate (int x) { … }`
    becomes `(int x) => { … }`. The bare `delegate { … }` form (which ignores the delegate's parameters) is
    left alone.
  - **`S3981`** + **`RCS1215`** — a collection's `.Count` or an array/string's `.Length` compared to `0`
    with `>=` / `<` is constant (`list.Count >= 0` is always `true`, `array.Length < 0` is always `false`);
    meaningful comparisons (`> 0`, `== 0`) are left alone.
  - **`SA1410`** (fixable) — "Remove delegate parenthesis when possible": an empty `delegate () { … }` drops
    the `()` to become `delegate { … }`.
- **`S3257` extended** — now also flags a redundant empty `delegate ()` parameter list (in addition to the
  redundant array type it already covered).

## 0.217.0

- **CRITICAL `lint --fix` correctness fix (issue #82).** Two auto-fixes produced broken code on a real
  codebase; both are fixed, plus a new safety net:
  - `CA1805` / `RCS1188` no longer leave a dangling `;` when removing a redundant default initializer from
    an **auto-property** — `bool X { get; set; } = false;` now becomes `bool X { get; set; }` (valid)
    instead of `bool X { get; set; };` (CS1597). The field case was already correct.
  - `RCS1214` no longer strips the `$` from an interpolated string that **has** a hole wrapped in
    doubled-brace escapes (`$"{{{{{key}}}}}"`) — it now does its own `{{`/`}}` accounting, so the hole is
    preserved.
  - **Post-fix verification:** the fix engine re-parses each rewritten file and, if a change introduces a
    syntax error the source did not have, falls back to only the individually-safe fixes — a broken rewrite
    can no longer reach disk.
- **2 more analyzers ported — 433 total** (`S2743` static fields in generic types, `S3010` static fields
  updated in constructors).

## 0.216.0

- **7 more analyzer parity fixes** for naming conventions, member ordering, and array parameters (no new
  ports — still **431 total**), all verified at exact parity vs the real Roslyn analyzers:
  - **Naming:** `S100`/`S101` now disagree on underscores correctly (a method `do_work` is left alone, but a
    type `Scout_Helper` is flagged); `SA1300` covers field-like events and enum members; `SA1312`/`SA1307`
    leave `const` locals and fields alone (those follow the constant-naming convention); `S2368` flags
    **jagged** array parameters (`int[][]`), not just multidimensional.
  - **Ordering:** `SA1204` (static before instance) now groups by access level — a `private static` member
    after a `public` instance member is fine — while a static constructor stays access-insensitive.
  - **Assignment:** `RCS1212` only folds `x = expr; return x;` when `x` is a local/parameter, not a field.

## 0.215.0

- **6 more analyzer parity fixes** for documentation, preprocessor directives, and assignment analysis (no
  new ports — still **431 total**), all verified at exact parity vs the real Roslyn analyzers:
  - **`<inheritdoc>`:** `SA1611` (parameters), `SA1615` (return value), and `SA1618` (type parameters) no
    longer demand explicit `<param>`/`<returns>`/`<typeparam>` tags on a member documented with
    `<inheritdoc>` — it inherits them by reference.
  - **Preprocessor:** `SA1515` no longer requires a blank line before a `//` comment that follows a
    directive (e.g. inside an `#else` branch); `SA1137` no longer treats an `#if`/`#pragma` directive (which
    sits at column 0) as an indentation peer of the surrounding statements.
  - **Assignment:** `RCS1212` (redundant assignment) only folds `x = expr; return x;` when `x` is a local
    or parameter — assigning a *field* is a real state change and is left alone.

## 0.214.0

- **Cross-file partial-class awareness — far fewer false positives on real solutions (issue #80).** A
  workspace `lint` now scans for `partial` type declarations across the whole solution before it reports,
  so a `partial` type whose parts are split over multiple files is understood as one type:
  - `RCS1043` / `S2333` (gratuitous `partial`) no longer fire on EF Core migrations, BDD
    `Foo.cs` + `Foo.Steps.cs` fixtures, or source-generated halves. A `partial` confined to one file is
    still flagged.
  - `S1144` / `RCS1213` (unused private member) no longer flag a member of a cross-file partial type — a
    use in another part (e.g. a step called from the fixture) is now accounted for.
  - Together with the **reflection-used-member** exemption in 0.213.0 and the (already-working) per-glob
    category severities, this resolves the ~1,500-finding false-positive cluster reported in #80.

## 0.213.0

- **Fewer false positives on reflection-used members (issue #80).** `S1144` and `RCS1213` (unused private
  member) now treat an **attributed** member as potentially used through a channel the syntax cannot see —
  serialization, dependency injection, an ORM hydrating a `private set;` (EF Core), a saga-state writer
  (MassTransit), or an explicit `[UsedImplicitly]` marker. So those members are no longer flagged, removing
  the bulk of that noise **without** a blanket `.editorconfig` suppression. Verified at exact parity vs the
  Roslyn host (S1144 exempts any attributed field/method/property; RCS1213 exempts an attributed method).
- **5 more analyzer parity fixes** (no new ports — still **431 total**), all verified at exact parity:
  - `SA1009` now also flags a `)` *followed* by a space before a hugging token (`;` `,` `.` `)` `]` `++`
    `--`) and before a primary constructor's base `:`.
  - `SA1019` no longer flags the wrapped `.` of a multi-line fluent chain (`x\n    .Foo()\n    .Bar()`).
  - `RCS1164` stops flagging a method type parameter that carries a constraint (`where T : struct`).

## 0.212.0

- **Agent-mode `lint` output is now compact on large solutions (#81).** Agent mode auto-enables in AI
  shells, but it used to enumerate every manual finding — ~1,600 lines on a 175-project solution,
  regardless of context. Beyond 40 manual findings the report now prints a `top rules:` histogram (the
  dominant rules with counts, e.g. `DF0080×167 CA1819×62 …`), the first 40 findings, then a `… +N more`
  tail; small results are still listed in full. Re-run scoped to a path, or with `--human`, for everything.
- **10 more analyzer parity fixes** for delegates, events, records, generics, and modifier ordering (no
  new ports — still **431 total**), all verified at exact parity vs the real Roslyn analyzers:
  - `SA1615` now checks delegate return-value docs; `SA1625` scans indexer and delegate doc comments.
  - `CA1003` / `MA0046` accept a nullable `EventHandler?`, and split on visibility (MA0046 fires on every
    event, CA1003 only externally-visible ones).
  - `RCS1141` flags a positional record whose primary-constructor parameters lack `<param>` docs.
  - `SA1201` no longer orders members inside a `record`; `MA0018` / `CA1000` flag only `public` static
    members of generic types.
  - `RCS1169` no longer suggests `readonly` for a static field assigned in an instance constructor;
    `SA1307` fires on public/internal fields only; `RCS1019` orders `new` before the accessibility keywords.

## 0.211.0

- **Parity-correctness release — 14 more analyzer fixes for modern C# syntax.** No new ports (still
  **431 total**); this round tightens existing ports against C# 11/12 shapes and other constructs that
  were slipping through, all verified at exact parity vs the real Roslyn analyzers:
  - **Generic math / checked operators:** `S4050` fires on classes only (structs have built-in value
    equality); `SA1000` flags a space after `checked`/`unchecked` in `operator checked +`; `MA0018` /
    `CA1000` exempt `static abstract` interface members; `CA2225` exempts the checked operator variant;
    `SA1625` scans operator doc comments.
  - **Modern expressions:** `S4487` / `S3459` treat `x ??= v` and `ref` aliasing as both a read and a
    write; `CA1814` flags multidimensional arrays in every position (fields, properties, return types,
    indexers, `new T[r,c]`); `SA1010` exempts index initializers (`new() { ["a"] = 1 }`).
  - **Other:** `CA1031` / `S2221` exempt a `catch … when (…)` filter; `S1144` / `RCS1213` handle
    `partial` methods correctly; and `S3400` exempts interface methods.

## 0.210.0

- **Parity-correctness release — 11 analyzer fixes for records, statics, and modern C# syntax.** No new
  ports (still **431 total**); this round tightens existing ports so they match the real Roslyn analyzers
  exactly on shapes that were slipping through:
  - **Records:** `RCS1169` and `RCS1213` no longer fire on fields/members of a record *class* (the
    synthesized copy/equality members make those suggestions unsafe) — record *structs* still fire.
  - **Static members:** `SA1642` now checks static-constructor summary text, and `S2933` correctly leaves
    static fields alone (only instance fields are flagged).
  - **C# 11/12 syntax:** `SA1206` / `RCS1019` / `CA1045` stop mis-flagging `ref readonly` parameters;
    `S2360` ignores lambda default parameters; `S3400` no longer treats a `"…"u8` UTF-8 literal as a
    constant; `SA1008` now reports a space *after* an opening parenthesis; and `S3260` treats a
    `file`-scoped class like a private one.

## 0.209.0

- **New: `dotnet-fast editorconfig recommend` — a curated analyzer profile.** On-by-default (0.208) can
  surface tens of thousands of subjective style/documentation warnings on a large repo, drowning the
  high-signal rules. The new command prints (or, with `--write`, appends to `.editorconfig`) a curated
  profile: every ported analyzer off, then the **Correctness / Concurrency / Performance / Redundancy**
  categories re-enabled at `warning` — with reasoning comments and a doc link. Style and Maintainability
  (layout, spacing, ordering, XML docs, naming) stay off as a deliberate per-category opt-in. So a fresh
  `lint` reports real defects rather than house-style noise, and you tighten the bar when you choose to.
- **10 more analyzers ported since 0.208.0 — 431 total** (44 with an autofix), including the dead-code /
  redundancy rules `RCS1124` (inline a single-use local), `RCS1179` (unnecessary branch assignment),
  `RCS1169` / `S2933` (make field read-only), `RCS1262` (unnecessary raw string), and `CA1033` / `S4039`
  (explicit interface members hidden from subclasses) — plus many parity refinements.

## 0.208.0

- **Ported analyzers are now ON by default (opt-out).** The native, Roslyn-free ports that re-implement
  popular Roslyn analyzers used to be opt-in per `.editorconfig`; they now report (and `--fix`-rewrite,
  where they carry a fix) in the default `lint` path at **`warning`** severity — no `--deep`, no config
  required. This is a **breaking change**: a `lint` over an existing repo will surface more findings than
  before. Tune them with the standard analyzer-config keys, following the usual Roslyn precedence:
  - per-rule — `dotnet_diagnostic.CA1820.severity = none`
  - per-category — `dotnet_analyzer_diagnostic.category-Correctness.severity = error`
  - bulk (turn them all off, re-enable what you want) — `dotnet_analyzer_diagnostic.severity = none`
- **44 more analyzers ported since 0.207.0 — 421 total** (44 with an autofix), across SonarAnalyzer,
  Microsoft.CodeAnalysis.NetAnalyzers, StyleCop, Roslynator, and Meziantou. See
  [ported analyzers](docs/ported-analyzers.md) for the full, generated list.

## 0.207.0

- **7 more analyzers ported to the native, Roslyn-free `lint` path — 377 total** (42 with an autofix).
  A correctness / maintainability wave across Microsoft, Sonar, Roslynator, and Meziantou — each opt-in
  per repo via `.editorconfig` and verified at exact parity against the real analyzer:
  - **`CA2241`** / **`S2275`** — `string.Format` whose `{N}` index has no matching argument.
  - **`S3878`** — arrays should not be created for `params` parameters (pass the elements directly).
  - **`CA1806`** / **`S2201`** — do not ignore a method result (a discarded object creation or pure
    string-method call).
  - **`S818`** — literal suffixes should be upper case (`1l` → `1L`).
  - **`MA0090`** — remove an empty `finally`/`else` block.
- **`RCS1259`** (remove empty syntax) now also flags empty `finally` and `else` clauses.

## 0.206.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 370 total** (42 with an autofix).
  A correctness / performance wave across Microsoft, Sonar, and Meziantou — each opt-in per repo via
  `.editorconfig` and verified at exact parity against the real analyzer:
  - **`CA2011`** / **`S2190`** — a property setter that assigns its own property (infinite recursion).
  - **`S3237`** — a `set`/`init` accessor should use the `value` parameter.
  - **`CA2245`** — do not assign a property to itself.
  - **`CA1866`** — use the `IndexOf(char)` overload for a single character.
  - **`CA2249`** — use `string.Contains` instead of `string.IndexOf(...) >= 0`.
  - **`CA1847`** — use a `char` literal with `Contains` for a single character.
  - **`CA1834`** / **`MA0028`** — use `StringBuilder.Append(char)` for a single character.

## 0.205.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 361 total** (42 with an autofix).
  A structural / maintainability wave across StyleCop, Microsoft, Roslynator, and Meziantou — each
  opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer:
  - **`SA1100`** — do not prefix a call with `base` unless the member is overridden locally.
  - **`SA1127`** — generic type constraints should be on their own line.
  - **`SA1128`** — constructor initializers should be on their own line.
  - **`SA1404`** — a `[SuppressMessage]` should carry a `Justification`.
  - **`SA1405`** / **`SA1406`** — `Debug.Assert` / `Debug.Fail` should provide a message.
  - **`CA1507`** / **`RCS1015`** / **`MA0043`** — use `nameof` in place of a string literal that
    matches a parameter name (the same rule from three analyzer packages).

## 0.204.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 352 total** (42 with an autofix).
  A StyleCop token-spacing wave — each opt-in per repo via `.editorconfig` and verified at exact
  parity against the real analyzer:
  - **`SA1002`** — a semicolon should not be preceded by a space.
  - **`SA1012`** / **`SA1013`** — an initializer's `{` / `}` should be spaced correctly.
  - **`SA1015`** — a closing generic bracket should not be preceded by a space.
  - **`SA1019`** — a member access `.` should not be preceded by a space.
  - **`SA1020`** — a postfix `++`/`--` should not be preceded by a space.
  - **`SA1021`** / **`SA1022`** — a unary `-` / `+` sign should not be followed by a space.
  - **`SA1024`** — a switch-label colon should not be preceded by a space.

## 0.203.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 343 total** (42 with an autofix).
  A layout-and-spacing wave from StyleCop and Roslynator — each opt-in per repo via `.editorconfig`
  and verified at exact parity against the real analyzer:
  - **`RCS1049`** (Roslynator) — simplify a negating boolean comparison (`x == false` → `!x`).
  - **`SA1001`** (StyleCop) — a comma should not be preceded by whitespace.
  - **`SA1008`** (StyleCop) — an opening parenthesis should not be preceded by a space.
  - **`SA1010`** (StyleCop) — an opening square bracket should not be preceded by a space.
  - **`SA1011`** (StyleCop) — a closing square bracket should not be preceded by a space.
  - **`SA1110`** (StyleCop) — an opening parenthesis should be on the declaration line.
  - **`SA1111`** (StyleCop) — a closing parenthesis should be on the line of the last parameter.
  - **`SA1112`** (StyleCop) — a closing parenthesis should be on the line of the opening parenthesis.
  - **`SA1137`** (StyleCop) — sibling elements should have the same indentation.

## 0.202.0

- **8 more analyzers ported to the native, Roslyn-free `lint` path — 334 total** (42 with an autofix).
  A spread across SonarAnalyzer, Roslynator, StyleCop, and xunit.analyzers — each opt-in per repo via
  `.editorconfig` and verified at exact parity against the real analyzer:
  - **`S3257`** (Sonar) — redundant array type when an initializer is present (`new int[] { 1, 2 }`).
  - **`S3459`** (Sonar) — a `private` field that is read but never assigned.
  - **`S4487`** (Sonar) — a `private` field that is written but never read.
  - **`S2156`** (Sonar) — a `sealed` class should not declare `protected` members.
  - **`RCS1212`** (Roslynator) — redundant assignment (`x = expr; return x;` → `return expr;`).
  - **`RCS1232`** (Roslynator) — order documentation comment elements to match the parameters.
  - **`SA1009`** (StyleCop) — a closing parenthesis should not be preceded by a space.
  - **`xUnit2006`** — do not use a generic `Assert.Equal`/`StrictEqual` for string equality.

## 0.201.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 326 total** (42 with an autofix).
  More **xunit.analyzers** test-authoring rules — each opt-in per repo via `.editorconfig` and
  verified at exact parity against the real analyzer:
  - **`xUnit1000`** — test classes must be public.
  - **`xUnit1002`** — a test method cannot carry multiple `[Fact]`/`[Theory]` attributes.
  - **`xUnit1008`** — test-data attributes should only be used on a `[Theory]`.
  - **`xUnit1009`** — an `[InlineData]` supplies fewer values than the method's parameters.
  - **`xUnit1010`** — an `[InlineData]` value is not convertible to the parameter type.
  - **`xUnit1014`** — `[MemberData]` should reference the member with `nameof` (**autofix**).
  - **`xUnit1030`** — do not call `ConfigureAwait(false)` in a test method.
  - **`xUnit2004`** — do not use `Assert.Equal`/`NotEqual` to check a boolean condition.
  - **`xUnit2021`** — async assertions (`Assert.ThrowsAsync`) should be awaited or stored.

## 0.200.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 317 total** (41 with an autofix).
  This wave deepens the **xunit.analyzers** test-authoring coverage — each opt-in per repo via
  `.editorconfig` and verified at exact parity against the real analyzer:
  - **`xUnit1006`** — theory methods should have parameters.
  - **`xUnit1011`** — an `[InlineData]` value has no matching method parameter.
  - **`xUnit1012`** — do not pass `null` for a non-nullable value-type parameter.
  - **`xUnit1024`** — test methods should not be overloaded.
  - **`xUnit1025`** — `[InlineData]` should be unique within its theory.
  - **`xUnit1028`** — a test method must return `void`, `Task`, or `ValueTask`.
  - **`xUnit1048`** — avoid `async void` unit tests.
  - **`xUnit2002`** — do not use a null check on a value type.
  - **`xUnit2014`** — do not use a synchronous throws check for an async delegate.

## 0.199.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 308 total** (41 with an autofix).
  This wave brings the **xunit.analyzers** assert/test rules to the native path — each opt-in per repo
  via `.editorconfig` and verified at exact parity against the real analyzer:
  - **`xUnit2024`** — do not use a boolean assert to test equality against a literal.
  - **`xUnit2010`** — …against a string `.Equals`.
  - **`xUnit2009`** — …to check for a substring.
  - **`xUnit2008`** — …to match a regular expression.
  - **`xUnit2007`** / **`xUnit2015`** — do not pass `typeof` to `Assert.IsType` / `Assert.Throws`.
  - **`xUnit2005`** — do not use `Assert.Same` on value types.
  - **`xUnit2022`** — boolean assertions should not be negated.
  - **`xUnit1031`** — do not use blocking task operations in a test method.

## 0.198.0

- **7 more analyzers ported to the native, Roslyn-free `lint` path — 299 total** (41 with an autofix).
  Documentation, partial-type, and ordering rules — each opt-in per repo via `.editorconfig` and
  verified at exact parity against the real analyzer:
  - **`SA1606`** (StyleCop) / **`RCS1138`** (Roslynator) — flag an empty `<summary>`.
  - **`SA1205`** (StyleCop) — partial elements should declare an access modifier.
  - **`SA1601`** (StyleCop) — partial elements should be documented.
  - **`SA1207`** (StyleCop) — `protected` should come before `internal`.
  - **`SA1212`** (StyleCop) — a property's `get` accessor should appear before its `set`.
  - **`SA1213`** (StyleCop) — an event's `add` accessor should appear before its `remove`.
- **`SA1600`** (elements should be documented) now treats a member with an empty `<summary>` as
  undocumented, matching the analyzer.

## 0.197.0

- **8 more analyzers ported to the native, Roslyn-free `lint` path — 292 total** (41 with an autofix).
  This wave fills in StyleCop's XML-documentation rules — each opt-in per repo via `.editorconfig` and
  verified at exact parity against the real analyzer:
  - **`SA1614`** — `<param>` documentation should have text.
  - **`SA1617`** — a `void` method should not document a `<returns>` value.
  - **`SA1622`** — `<typeparam>` documentation should have text.
  - **`SA1610`** — `<value>` documentation should have text.
  - **`SA1625`** — documentation should not be copied and pasted between elements.
  - **`SA1609`** — a documented property should have a `<value>` element.
  - **`SA1612`** — `<param>` elements should be in the same order as the parameters.
  - **`SA1624`** — the summary should begin "Gets" when the setter is less visible than the property.
- **`RCS1228`** (empty documentation element) now also flags empty `<param>`, `<typeparam>`, and
  `<value>`, not just `<returns>`.

## 0.196.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 284 total** (41 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer —
  type-alias, enum, documentation, control-flow, and exception rules:
  - **`SA1121`** (StyleCop) — use a built-in type alias (`System.Int32` → `int`, autofix).
  - **`RCS1234`** (Roslynator) / **`CA1069`** (Microsoft) — flag a duplicated explicit enum value.
  - **`SA1643`** (StyleCop) — destructor summary should begin with the standard text.
  - **`RCS1006`** (Roslynator) — merge `else { if … }` into `else if` (autofix).
  - **`S2372`** (Sonar) / **`CA1065`** (Microsoft) — do not throw from a property getter.
  - **`RCS1228`** (Roslynator) / **`SA1616`** (StyleCop) — flag an empty `<returns></returns>`.

## 0.195.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 275 total** (39 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer —
  nullable, coalesce, exception, cast, and lambda rules:
  - **`RCS1020`** (Roslynator) / **`SA1125`** (StyleCop) — simplify `Nullable<T>` to `T?` (autofix).
  - **`RCS1084`** (Roslynator) — use `x ?? y` instead of `x != null ? x : y` (autofix).
  - **`RCS1073`** (Roslynator) — collapse `if (c) return <bool>; [else] return <bool>;` to one return.
  - **`SA1139`** (StyleCop) — use a literal suffix instead of casting (`(long)1` → `1L`, autofix).
  - **`MA0012`** (Meziantou) — do not raise runtime-reserved exception types.
  - **`RCS1244`** (Roslynator) — simplify `default(T)` to `default`.
  - **`CA2201`** (Microsoft) — do not raise reserved exception types (general bases + runtime-reserved).
  - **`SA1130`** (StyleCop) — use lambda syntax instead of an anonymous `delegate { }` (autofix).
- **`S112`** (throw of a general exception) now also flags the runtime-reserved types
  (`NullReferenceException`, `IndexOutOfRangeException`, …), and **`RCS1123`** now parenthesizes mixed
  `&&` / `||` as well as arithmetic.

## 0.194.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 266 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer —
  equality, documentation, whitespace, control-flow, and exception rules:
  - **`S1206`** (SonarAnalyzer) — `Equals(object)` and `GetHashCode()` should be overridden in pairs.
  - **`SA1025`** (StyleCop) — code should not contain multiple whitespace characters in a row.
  - **`SA1642`** (StyleCop) — constructor summary documentation should begin with standard text.
  - **`RCS1036`** (Roslynator) — remove redundant empty lines (file edges, after `{` / before `}`).
  - **`xUnit1026`** (xunit.analyzers) — Theory methods should use all of their parameters.
  - **`AsyncFixer03`** (AsyncFixer) — avoid fire-and-forget `async void` methods.
  - **`SA1408`** (StyleCop) — conditional expressions should declare precedence (mixed `&&` / `||`).
  - **`RCS1190`** (Roslynator) — join concatenated string literals.
  - **`S112`** (SonarAnalyzer) — general exception types should never be thrown.
- **`RCS1123`** (parenthesize for precedence) now also covers `&&` mixed with `||`, not just arithmetic.

## 0.193.0

- **6 more analyzers ported to the native, Roslyn-free `lint` path — 257 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer —
  boolean, lambda, and member-access rules:
  - **`MA0073`** (Meziantou) / **`RCS1033`** (Roslynator) — remove a redundant comparison with a
    boolean constant (`x == true`).
  - **`RCS1021`** (Roslynator) — use an expression-bodied lambda.
  - **`RCS1058`** (Roslynator) — use a compound assignment (`x = x + 1` → `x += 1`).
  - **`RCS1089`** (Roslynator) — use `++`/`--` instead of `x = x + 1`.
  - **`RCS1146`** (Roslynator) — use conditional access (`?.`).
- **CI: minimal-fetch mode.** `affected`/`lint`/`build` accept `--fetch-base` (alias `--minimal-fetch`):
  on a shallow CI clone it fetches only the base ref and deepens it incrementally until the merge base
  is reachable, instead of unshallowing the whole repo — so jobs can drop `fetch-depth: 0`.
- **CI: shared affected manifest.** `affected --emit-manifest <file>` writes one JSON artifact (change
  range + affected projects + test subset + changed files). Feed it to `lint`/`build`/`test-plan`
  with `--projects-file` so the affected set is computed once per run and reused.
- **Docs:** documented duration-weighted test sharding's Azure DevOps timing-source recipe.

## 0.192.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 251 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer —
  control-flow, switch, and property rules:
  - **`S907`** (SonarAnalyzer) — `goto` should not be used.
  - **`S4524`** (SonarAnalyzer) — `default` clauses should be first or last in a `switch`.
  - **`S1199`** (SonarAnalyzer) — nested code blocks should not be used.
  - **`S131`** (SonarAnalyzer) — `switch` statements should have a `default` clause.
  - **`RCS1070`** (Roslynator) — remove a redundant `default` switch section.
  - **`MA0016`** (Meziantou) — prefer a collection abstraction over a concrete type.
  - **`RCS1085`** (Roslynator) / **`S2292`** (SonarAnalyzer) — use auto-implemented properties.
  - **`MA0102`** (Meziantou) — make a non-mutating `struct` method `readonly`.

## 0.191.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 242 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer —
  API-design, parameter, and control-flow rules:
  - **`CA1034`** (Microsoft) — nested types should not be visible.
  - **`CA1002`** (Microsoft) — do not expose `List<T>`.
  - **`CA2227`** (Microsoft) — collection properties should be read-only.
  - **`RCS1019`** (Roslynator) — order modifiers consistently.
  - **`S2360`** (SonarAnalyzer) — optional parameters should not be used.
  - **`S1226`** (SonarAnalyzer) — method parameters should not be reassigned.
  - **`S121`** (SonarAnalyzer) / **`RCS1007`** (Roslynator) — control structures should use curly braces.
  - **`RCS1158`** (Roslynator) — static members in generic types should use a type parameter.

## 0.190.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 233 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer —
  a wave of API-design rules:
  - **`CA1045`** (Microsoft) — do not pass types by reference (`ref` parameters).
  - **`CA1024`** (Microsoft) — use properties instead of parameterless `GetX` methods.
  - **`CA1707`** (Microsoft) — identifiers should not contain underscores.
  - **`CA1054`** / **`CA1055`** / **`CA1056`** (Microsoft) — URI parameters, return values, and
    properties should be `System.Uri`, not `string`.
  - **`CA1813`** (Microsoft) — avoid unsealed attributes.
  - **`CA1019`** (Microsoft) — define accessors for attribute arguments.
  - **`CA1721`** (Microsoft) — property names should not match `Get` methods.

## 0.189.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 224 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer —
  enum, property, and exception-handling rules:
  - **`CA1044`** (Microsoft) — properties should not be write-only.
  - **`S3052`** (SonarAnalyzer) — do not initialize members to their default value.
  - **`RCS1135`** (Roslynator) — a `[Flags]` enum should declare a zero-value member.
  - **`CA1008`** (Microsoft) — enums should have a zero-value member.
  - **`CA1700`** (Microsoft) — do not name enum values `Reserved`.
  - **`CA1027`** (Microsoft) — mark power-of-two enums with `[Flags]`.
  - **`CA1031`** (Microsoft) / **`S2221`** (SonarAnalyzer) — do not catch general exception types.
  - **`CA1021`** (Microsoft) — avoid `out` parameters.

## 0.188.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 215 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer —
  a wave of type-design rules:
  - **`CA1005`** (Microsoft) — avoid more than two type parameters on a generic type.
  - **`CA1003`** (Microsoft) — use a generic `EventHandler` for events.
  - **`CA2225`** (Microsoft) — operator overloads should have a named alternate (`+` ↔ `Add`).
  - **`CA1068`** (Microsoft) — `CancellationToken` parameters must come last.
  - **`CA1010`** (Microsoft) — collections should also implement `IEnumerable<T>`.
  - **`S3875`** (SonarAnalyzer) — do not overload `operator ==` on a class (reference type).
  - **`CA1047`** (Microsoft) — do not declare `protected` members in `sealed` types.
  - **`CA1819`** (Microsoft) — properties should not return arrays.
  - **`S3253`** (SonarAnalyzer) — remove redundant constructor/destructor declarations.
- **Lint fix:** the native **`S1118`** check (utility classes should not have public constructors) no
  longer flags a class that declares operators — such a type is value-like, not a static holder.

## 0.187.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 206 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer.
  This wave completes the value-type / equality family:
  - **`CA1815`** (Microsoft) — a value type with state should override `Equals` and `operator ==`.
  - **`CA1067`** (Microsoft) / **`MA0095`** (Meziantou) — override `Equals` when implementing
    `IEquatable<T>`.
  - **`S3897`** (SonarAnalyzer) / **`MA0077`** (Meziantou) — a class that provides `Equals(T)` should
    implement `IEquatable<T>`.
  - **`S4035`** (SonarAnalyzer) — classes implementing `IEquatable<T>` should be `sealed`.
  - **`MA0018`** (Meziantou) / **`CA1000`** (Microsoft) — do not declare static members on generic types.
  - **`S2376`** (SonarAnalyzer) — write-only properties (a `set` with no `get`) should not be used.

## 0.186.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 197 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer:
  - **`CA1064`** (Microsoft) / **`S3871`** (SonarAnalyzer) — exception types should be `public`, so callers
    can catch them by name across assemblies.
  - **`CA2231`** (Microsoft) — a value type that overrides `Equals` should also overload `operator ==`.
  - **`CA1018`** family grows: **`S3993`** (SonarAnalyzer) and **`MA0010`** (Meziantou) join CA1018/RCS1203
    — mark a custom attribute with `[AttributeUsage]`.
  - **`S3260`** (SonarAnalyzer) — a non-derived `private` class should be `sealed`.
  - **`CA1040`** (Microsoft) / **`S4023`** (SonarAnalyzer) — avoid empty interfaces (a marker with no
    members).
  - **`CA1066`** (Microsoft) — a value type that overrides `Equals` should implement `IEquatable<T>`.
- **Lint fix:** the native **`S3400`** check (methods should not return constants) no longer flags
  `override`/`virtual` methods, which cannot collapse to a constant — matching the analyzer.

## 0.185.0

- **8 more analyzers ported to the native, Roslyn-free `lint` path — 188 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer:
  - **`RCS1043`** (Roslynator) / **`S2333`** (SonarAnalyzer) — remove the `partial` modifier from a type
    that has only one part (no other part to merge with).
  - **`S3261`** (SonarAnalyzer) — namespaces should not be empty (a `namespace N { }` with no members).
  - **`RCS1251`** (Roslynator) — remove the unnecessary braces of an empty-body type (`class C { }`).
  - **`CA1018`** (Microsoft) / **`RCS1203`** (Roslynator) — mark a custom attribute type with
    `[AttributeUsage]` so its valid targets are explicit.
  - **`RCS1031`** (Roslynator) — remove the unnecessary braces wrapping a whole `switch` section.
  - **`SA1629`** (StyleCop) — documentation text (`<summary>`/`<param>`/`<returns>`/…) should end with a
    period.
- **Lint:** the existing **`RCS1259`** (remove empty syntax) port now also flags an empty `namespace N { }`
  block, matching the analyzer.

## 0.184.0

- **6 more analyzers ported to the native, Roslyn-free `lint` path — 180 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer:
  - **`S134`** (SonarAnalyzer) — control flow statements should not be nested more than three deep.
  - **`CA1050`** / **`MA0047`** / **`S3903`** / **`RCS1110`** — declare types in a namespace, not the
    global scope (the Microsoft, Meziantou, Sonar, and Roslynator forms of the same rule).
  - **`CA1810`** (Microsoft) — initialize static fields inline instead of in an assignment-only static
    constructor.

## 0.183.0

- **6 more analyzers ported to the native, Roslyn-free `lint` path — 174 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer:
  - **`S1862`** (SonarAnalyzer) — related `if`/`else if` conditions should not be the same.
  - **`S1871`** (SonarAnalyzer) — two `switch` sections should not have the same implementation.
  - **`SA1027`** (StyleCop) — tabs and spaces should be used correctly (no tabs by default).
  - **`S2436`** (SonarAnalyzer) — types and methods should not have more than two generic parameters.
  - **`SA1133`** (StyleCop) — each attribute should be in its own `[…]` brackets.
  - **`SA1129`** (StyleCop) — use `default` instead of a value type's parameterless constructor.

## 0.182.0

- **6 more analyzers ported to the native, Roslyn-free `lint` path — 168 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer:
  - **`S1067`** (SonarAnalyzer) — expressions should not be too complex (more than 3 `&&`/`||`).
  - **`S3963`** (SonarAnalyzer) — static fields should be initialized inline (an assignment-only static
    constructor).
  - **`MA0051`** (Meziantou) — method is too long (body over 60 lines).
  - **`S138`** (SonarAnalyzer) — functions should not have too many lines (over 80).
  - **`SA1116`** (StyleCop) — split parameters should begin on the line after the declaration.
  - **`SA1117`** (StyleCop) — parameters should all be on the same line or each on its own line.
- **`test-plan` can balance shards by prior-run time.** `test-plan --timings <file>` reads a JSON map of
  fixture → milliseconds and balances shards by duration instead of fixture count, so the slowest
  fixtures spread across agents and the parallel stage's tail shrinks. New or unseen fixtures are
  estimated from their static weight; an empty file falls back to count balancing. See
  [docs/test-sharding.md](docs/test-sharding.md).
- **Lint fix:** the native `SA1306` check (private field names lower-case) no longer flags
  `static readonly` fields, which are PascalCase by convention (SA1311's rule).

## 0.181.0

- **6 more analyzers ported to the native, Roslyn-free `lint` path — 162 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer:
  - **`SA1307`** (StyleCop) — accessible fields should begin with an upper-case letter.
  - **`SA1310`** (StyleCop) — field names should not contain an underscore.
  - **`CA1708`** (Microsoft) — identifiers should differ by more than case.
  - **`S107`** (SonarAnalyzer) — methods should not have more than seven parameters.
  - **`CA1052`** (Microsoft) — a class holding only static members should be `static` or `sealed`.
  - **`SA1500`** (StyleCop) — the opening brace of a multi-line block should be on its own line.
- **`lint` is clearer about a quiet gate.** When `--severity` filters out findings, the run now prints a
  one-line hint that lower-severity findings were suppressed, so a "clean" result is never misleading.
- **`build` tolerates a project whose declared input is missing**, instead of failing the run.

## 0.180.0

- **6 more analyzers ported to the native, Roslyn-free `lint` path — 156 total** (34 with an autofix).
  This wave fills in the **naming-convention** rules. Each is opt-in per repo via `.editorconfig` and
  verified at exact parity against the real analyzer:
  - **`S100`** (SonarAnalyzer) — methods and properties should be PascalCase (`doThing` → `DoThing`).
  - **`SA1300`** (StyleCop) — types and members should begin with an upper-case letter.
  - **`SA1303`** (StyleCop) — `const` field names should begin with an upper-case letter.
  - **`SA1308`** (StyleCop) — field names should not carry an `m_` / `s_` prefix.
  - **`SA1312`** (StyleCop) — local variable names should begin with a lower-case letter.
  - **`SA1313`** (StyleCop) — parameter names should begin with a lower-case letter.

## 0.179.0

- **4 more analyzers ported to the native, Roslyn-free `lint` path — 150 total** (34 with an autofix).
  This wave adds the first **scope-aware** ports (they reason about how a name is used within a method or
  type, not just its shape). Each is opt-in per repo via `.editorconfig` and verified at exact parity
  against the real analyzer:
  - **`S1481`** (SonarAnalyzer) — unused local variables (a local declared but never read).
  - **`RCS1163`** (Roslynator) — unused parameters (a method/operator parameter never used; methods whose
    signature is fixed by a contract — `abstract`/`virtual`/`override` — are left alone).
  - **`RCS1213`** (Roslynator) — unused private members (a private field/constant/method the type never
    references).
  - **`S1144`** (SonarAnalyzer) — the broader form: also flags unused private nested types and enums.

## 0.178.0

- **6 more analyzers ported to the native, Roslyn-free `lint` path — 146 total** (34 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer.
  New in this release:
  - **`CA1805`** (Microsoft) — do not initialize a field to its type's default (`= 0` / `= false` /
    `= null`); the **fix** drops the redundant initializer.
  - **`RCS1129`** (Roslynator) — the same, under Roslynator's id (**autofix**).
  - **`S1264`** (SonarAnalyzer) — a `for` loop with only a condition should be a `while`; the **fix**
    rewrites the header to `while (…)`.
  - **`RCS1206`** (Roslynator) — a null-check ternary (`x != null ? x.M() : null`) should use the
    null-conditional operator; the **fix** rewrites it to `x?.M()`.
  - **`S2761`** (SonarAnalyzer) — doubled `!`/`~` operators (`!!x`, `~~y`) cancel out and should be
    removed.
  - **`RCS1187`** (Roslynator) — a non-public `static readonly` field with a constant value could be a
    `const` (public fields are left alone — that would be a binary-compatibility change).

## 0.177.0

- **9 more analyzers ported to the native, Roslyn-free `lint` path — 140 total** (30 with an autofix).
  Each is opt-in per repo via `.editorconfig` and verified at exact parity against the real analyzer.
  New in this release:
  - **`S2326`** (SonarAnalyzer) — unused type parameters should be removed (a generic parameter never
    referenced in its declaration).
  - **`RCS1164`** (Roslynator) — the same, scoped to methods and local functions.
  - **`S4144`** (SonarAnalyzer) — methods should not have identical implementations (two methods of a
    type sharing the same body).
  - **`S1125`** (SonarAnalyzer) — redundant boolean literals (`!true` / `!false`, `cond ? true : false`).
  - **`MA0005`** (Meziantou) — use `Array.Empty<T>()` instead of allocating `new T[0]` / `new T[] { }`
    (**autofix**).
  - **`SA1124`** (StyleCop) — do not use regions (a `#region` outside a code-element body).
  - **`SA1123`** (StyleCop) — do not place regions within a method/accessor body.
  - **`RCS1189`** (Roslynator) — add the region name to a bare `#endregion`.
  - **`RCS1140`** (Roslynator) — add an `<exception>` element to a documented member that throws a new
    exception.
- **Formatting/lint fix:** corrected a case where the native `SA1516` check ("elements should be
  separated by a blank line") wrongly flagged the first type member after a `#region`/`#if` directive —
  preprocessor directives are now treated as trivia, not members.

## 0.176.2

- **License changed from MIT to a freeware license.** `dotnet-fast` remains free to use, including
  commercially within your own organization. The binary is now distributed under a proprietary freeware
  EULA (see `LICENSE`): redistribution, resale, modification, and reverse engineering are not permitted,
  and the source code stays private. No change to how you install or run the tool.

## 0.176.0

- **Build cache auth now supports short-lived SAS.** `dotnet-fast build` can use
  `DOTNET_FAST_CACHE_SAS`, `buildCache.sasToken`, a SAS URL, or a connection string carrying
  `SharedAccessSignature`, avoiding standing blob-data RBAC for CI jobs.
- **Real build runs now produce an observability report.** `dotnet-fast build --json` includes each
  project's path, action, files, duration, downloaded/uploaded bytes, plus summary hit-rate and
  transfer totals.
- **Test sharding can use the build-cache hit/miss set.** `test-plan --cache-misses-file
  build-cache-plan.json` consumes `build --plan --json` or real `build --json`, then shards only tests
  impacted by cache misses/non-restored projects and their dependents.
- **Build cache operations are documented.** The guide now covers Azure Blob lifecycle retention rules
  and the concurrent-write safety contract for atomic uploads and SHA-verified restores.
- **Concurrent fixer runs are safer.** Simultaneous `lint --fix` processes now serialize source-file
  writes per file so deterministic rewrites do not interleave on disk.

## 0.175.0

- **Build cache can now stay affected-only.** `dotnet-fast build` and `build --plan` honor repeated
  `--project`, accept `--projects-file` traversal/list handoffs, and intersect explicit scope with
  `--ci`/`--from` affected ranges. The plan/report stays on the selected projects while real builds
  still materialize referenced projects internally when a selected miss needs them.
- **Auto-sized test matrices now size after file handoff scope.** `test-plan --projects-file
  affected.proj --auto-shards` chooses the shard count from the scoped test projects instead of the
  wider workspace.

## 0.174.0

- **Test sharding can consume an exact affected test-project handoff.** Repeated `--project` now scopes
  bare `test-plan`, and `--projects-file` accepts project paths/names, `affected --format dotnet-test`
  run lists, affected matrix JSON, or traversal projects. When combined with `--ci`/`--from`, the
  explicit list is intersected with the affected range so shards do not run unbuilt projects.
- **Azure matrix output is explicit.** `test-plan --format ado-matrix` emits the Azure Pipelines
  `strategy.matrix` object shape, while the existing `--format matrix` remains the GitHub Actions
  `include` matrix.

## 0.173.0

- **Test sharding can now pick CI parallelism itself.** `dotnet-fast test-plan --auto-shards
  --min-per-shard <weight> --max-shards <N> --format matrix` chooses the shard count from discovered
  fixture weight and emits a ready matrix. With affected scoping, an empty affected test set emits an
  empty matrix and exits `166`.
- **Test sharding can run a shard directly.** `test-plan --exec` runs the generated `dotnet test`
  commands, while `--test-args`, `--filter-and`, `--results-dir`, and `--collect cobertura` handle the
  CI flags, category gates, TRX output, and coverage collection that used to require wrapper scripts.
- **Build cache contract clarified.** `dotnet-fast build` now documents the restored-tree guarantee:
  successful runs materialize `bin/<configuration>` and `obj/<configuration>` from cache hits or real
  SDK builds, so downstream `dotnet test --no-build --no-restore` steps can rely on the tree.

## 0.171.0

- **Build support:** `dotnet-fast build` adds a preview remote build cache for CI. It can restore
  cached project outputs on clean agents, build misses normally, and upload trusted-branch artifacts
  for future runs. See [docs/build-cache.md](docs/build-cache.md).
- **Test support:** `dotnet-fast test-plan` adds NUnit test sharding for parallel CI agents. It
  discovers fixtures from source, emits matrix/JSON plans or runnable `dotnet test` commands, and can
  verify shard coverage against the runtime test list. See [docs/test-sharding.md](docs/test-sharding.md).

## 0.166.0

- **Across-the-board performance pass.** A batch of speedups to the hottest paths, all behaviour-preserving:
  - **`affected`** indexes the project-graph lookups, so resolving reverse-dependencies on a large solution
    no longer does repeated linear scans.
  - **`lint`** skips the suppression-comment scan entirely on files that have no suppressions (the common
    case), and reuses cached results for files whose report came back clean.
  - **`format`** avoids copying lines that the whitespace fast-path leaves untouched.
  - Shipped binaries now abort on panic instead of unwinding — slightly faster and smaller.
- **Formatting parity fix:** corrected formatter boundary handling under the .NET 10 SDK so output stays
  in step with `dotnet format`.

## 0.165.0

- **`lint --deep` is much faster on CI / affected runs.** When you point it at a Git change set
  (`--ci`, `--affected`, `--staged`, or `--from <ref>`), it now analyzes **only the changed files** —
  it still builds the full project so types resolve, but skips re-running analyzers over the thousands
  of files you didn't touch. On a large, analyzer-heavy solution a small PR no longer pays to analyze
  the whole project. Bare `lint --deep` (no Git range) stays exhaustive; set `DOTNET_FAST_DEEP_FULL=1`
  to force a complete whole-project run.
- Trade-off: the scoped fast path runs each changed file's syntax/semantic/symbol rules but not the rare
  whole-program (compilation-end) analyzer rules — the exhaustive path still covers those. See
  [docs/deep-linting.md](docs/deep-linting.md).

## 0.164.0

- **Deep linting reports its memory + timings.** Each `lint --deep` run now prints
  per-project analyzer count, findings, time, and memory (peak working set / heap) — useful for sizing
  CI agents on large, analyzer-heavy solutions.
- The deep analyzer host caps how many projects it keeps cached, so a long-running run across a big
  solution stays within a bounded memory footprint.

## 0.163.0

- **`lint --deep` is more expressive.** It logs each project as it's analyzed — `[1/3] MyProject — 274
  analyzers, 12 finding(s) in 728 ms` — so a CI log shows exactly what deep linting is doing and where the
  time goes. (Stderr, so `--format` output on stdout stays clean.)

## 0.162.0

- **`affected` is clearer in CI.** It now logs which branches it's comparing
  (`comparing <head> against <base>`) and lists the affected projects in the build log — so a CI run is
  self-documenting without opening the output file. (Both on stderr, so `--format count`/`matrix` output
  on stdout stays clean.)
- **Faster deep linting on large projects** — source files are now parsed in parallel (~40% faster parse
  on a 400-file project); analyzers still report exactly the same diagnostics.

## 0.161.0

- **Formatting fix:** `lint --fix` no longer misaligns lambda braces in fluent method chains (e.g.
  `.ConfigureAppConfiguration((c, b) => { … })`) — the braces stay aligned with the body, matching
  `dotnet format`.

## 0.160.0

- **`affected --format count`** — emit a bare affected-project count to stdout (no file to parse), handy
  for CI parallelism math: `dotnet-fast affected --ci --tests-only --format count`.
- **Formatting fix:** a collection initializer nested inside an object initializer (e.g.
  `new Foo { Items = new[] { 1, 2 } }` laid out over multiple lines) now keeps its element spacing
  exactly as `dotnet format` leaves it.

## 0.159.0

- **Critical fix:** `lint --fix` could delete `return this;` (and other keyword returns like
  `return default;`) from fluent-builder methods, breaking the build (CS0161). Fixed — `--fix` never
  removes a value-returning `return`. **Upgrade if you use `lint --fix`.**

## 0.158.0

- **`affected --tests-only`** correctly detects test projects whose `IsTestProject` is set by an
  MSBuild condition (e.g. in a shared `Directory.Build.props`).

## 0.155.0 – 0.157.0

- **Deep linting reliability:** `lint --deep` now runs analyzers against the installed SDK's Roslyn, so
  analyzers built for newer Roslyn (including bundled/meta-package analyzers) load and run instead of being
  silently skipped. It also warns instead of silently reporting nothing when no analyzers load.
- **Formatting parity:** delimiter-aligned and multi-line collection initializers are left untouched to
  match `dotnet format`.

## 0.154.0

- **Focused on format, lint, and affected.** The `restore`, `update`, and `sbom` commands have been
  removed. They overlapped with `dotnet restore` / `dotnet list package` without a clear speed win on the
  paths most people hit, so the tool now does fewer things well. Use the official `dotnet` commands for
  package restore, updates, and SBOMs.

## 0.153.0

- `doctor` now flags a project that lists the same target framework twice.

## 0.149.0

- **Deep linting can be turned on by default.** Set `DOTNET_FAST_DEEP=1` and a plain `lint` uses your real
  Roslyn analyzers when the SDK and a restored project are available, and falls back to the fast path when
  they aren't. See [docs/deep-linting.md](docs/deep-linting.md).

## 0.148.0

- Deep linting now also reports the .NET SDK's built-in `CA*` analyzers — matching `dotnet build`, not
  just the analyzer packages you reference.

## 0.146.0 – 0.145.0

- Formatting fixes for closer agreement with `dotnet format`: correct spacing around comparison operators
  before a number, and around `?`/`:` at the start of a wrapped line.

---

Older releases predate these notes.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

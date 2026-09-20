# Release notes

What changed in recent releases, in plain English. Newest first.

The current **stable** line is `1.9.0`. Pre-1.0 history — predating the compatibility promise and the
NuGet package — is a git-history pointer, not full notes, in
[RELEASES-0.x.md](RELEASES-0.x.md).

## 1.9.0 — 2026-09-20

`--deep` now sees two kinds of code it was previously blind to, and `update` stops giving the wrong
answer to anyone who installed the tool outside the .NET tool system.

**If you gate CI on `--deep`, read the first two sections before upgrading** — both make analyzers see
more of your code, so finding counts can rise. That is the fix working, not a regression, but it is a
change you should meet deliberately rather than in a failing build.

### `--deep` now defines your preprocessor symbols

Analyzers under `--deep` were handed every source file with **no preprocessor symbols defined at all**.
So every `#if DEBUG` region was invisible, and — the direction that catches people out — every `#else`
branch that a real build *excludes* was the branch being analysed. Two halves of this tool disagreed
about which code existed: the formatter and native lint have always evaluated per-project symbols,
`--deep` never did.

Symbols are now derived per project, the way the rest of the tool derives them: `DefineConstants`
through `Directory.Build.props`/`.targets` and any imports, plus `DEBUG`/`TRACE`, plus the SDK's
implicit target-framework chain (`NET8_0`, `NET8_0_OR_GREATER`, `NETSTANDARD2_0`, and so on).

Verified against a real build rather than against ourselves: on a fixture where `dotnet build -c Debug`
reports a diagnostic at lines 24, 33 and 40, `--deep` now reports exactly 24, 33 and 40. Previously it
reported line 26 — the `#else` branch the compiler never sees.

- **Expect movement on any project with `#if` regions.** Findings inside a live `#if` appear for the
  first time; findings inside a dead `#else` correctly disappear.
- **Multi-target projects analyse the first target framework only.** Other legs' `#if` branches remain
  invisible. That is a stated limit, not an implied one.
- `DOTNET_FAST_DEEP_NO_SYMBOLS=1` restores the old behaviour if you need to stage the change.

### `--deep` now binds types from your project references

Types coming from a `ProjectReference` were unresolved, so any analyzer that needed them quietly
skipped the code using them. A type inheriting from a base class in a sibling project simply did not
look like what it was.

Verified the same way: on a fixture where a real build reports two `CA1032` diagnostics, `--deep`
now reports the same two. Remove the sibling's build output and it reports none; restore it and the
two come back.

Two things worth knowing, both measured rather than predicted:

- **The binding needs the sibling project to have been built**, and it is skipped when the sibling's
  sources are newer than its output. A fresh `git clone` can leave source timestamps newer than
  committed build output, so on such a checkout this stays inactive until you build. The direction is
  safe — it withholds rather than binding against a stale assembly — but it means the improvement can
  be silently absent.
- **The real-world effect may be nothing at all.** On a large multi-project solution the finding set
  was byte-identical before and after (33 findings either way). The fixture proves the mechanism; your
  repository may see no change.

### `update` no longer tells a winget user to run `dotnet tool update`

`update` recognised two kinds of install: manifest-pinned, and global .NET tool. Anything else fell
through to "global", so a copy installed outside the .NET tool system was told to run
`dotnet tool update --global`. That either fails, or succeeds and installs a **second** copy alongside
the first, after which two package managers each maintain a different binary on your `PATH` and the
tool reports success.

A copy running from a winget install directory is now recognised as such and told `winget upgrade`.
Detection is anchored on the directories winget actually installs into, so a repository that merely
happens to live at a path like `C:\src\winget\packages` is not mistaken for one.

New flag: **`update --explain-install`** prints how the running copy was installed and what evidence
led to that conclusion — useful when a CI log needs to say which install path it is on.

Manifest-pinned and global installs are unchanged, byte for byte.

### Also in this release

- The deep-parity comparison follows Roslynator 5.0's renamed rule ids, so an empty `else` is compared
  against the successor id upstream now reports rather than the one it used to.
- Test-harness correctness: suites that claimed to be hermetic were, on Windows, reaching the real
  .NET SDK instead of the stub they thought they were using. They now genuinely intercept, which means
  the build-cache and test-plan suites test what they claim to.

## 1.8.1 — 2026-09-18

Fix-safety fixes. Every one of them stops the tool from writing a change it could not prove was
correct. If you run `--fix` or `dead-code --write` in CI, take this release.

The pattern across all of them: the rewrite was right for the *common* case and wrong for a case the
rule could not see. Three of these compile cleanly and silently change what your program does, which
is worse than a build break — nothing tells you.

### `dead-code --fix --write` no longer deletes compiler-referenced polyfills

Reported against Polly, where it deleted `internal static class IsExternalInit` and broke the build
with `CS0518` on every `init` accessor and every record, across three target frameworks.

Nothing in the source ever names that type. The compiler references
`System.Runtime.CompilerServices.IsExternalInit` by well-known fully-qualified name at code generation
time, so a reachability graph built from source references correctly concludes nothing reaches it —
and deletes a type the compiler requires. Any project targeting netstandard2.0 or older frameworks
that hand-rolls these shims (or uses a polyfill package that emits them into your source) was exposed.

The same mechanism covers `System.Index` and `System.Range` — `xs[^1]` and `s[1..]` lower to
constructors resolved the same invisible way. Those are now protected too.

Matching is on the full name, so your own unrelated type that happens to share a simple name is still
reported normally.

### `lint --fix` now reaches a fixed point on nullable array types

`string[]?` sent `lint --fix` into a loop: `SA1011` inserted a space before the `?`, `SA1018` removed
it, and each run undid the last. A CI job running `--fix` to convergence would never terminate.

Both rules are faithful ports — upstream StyleCop has the identical contradiction, and an IDE never
exposes it because it offers one fix at a time. `SA1011` still **reports** this shape; it no longer
offers the insert fix for it, which leaves the file exactly where `dotnet format` puts it.

### Four rewrites that could silently change your program's behaviour

Each of these is now withheld when the tool can see the hazard, and still reported:

- **`x == null` → `x is null`** when the operand's type declares its own `operator ==`. A user-defined
  `==` can report a live reference as equal to null — Unity's fake-null pattern, or any hand-rolled
  null-object type — while `is null` always does the real check. The rewrite flips the result with no
  compile error.
- **`!(a == b)` → `a != b`.** C# requires a user-defined `==`/`!=` pair to be *declared* together; it
  does not require them to *agree*. Only the predefined operators are guaranteed opposites.
- **`!(a < b)` → `a >= b`** on floating-point operands. With `NaN` on either side, every ordering
  comparison is false, so the negation and the flipped operator disagree.
- **`0 < s` → `s > 0`.** Swapping operands re-binds overload resolution: `operator <(int, T)` and
  `operator >(T, int)` are different methods and need not be consistent.

These guards read the operand's written declaration, so they fire when the type is declared in the
same file. A type from another file, a partial whose operator lives elsewhere, or an external package
type is still invisible to them and keeps the fix — closing that needs a project-wide symbol index,
which is tracked separately.

### What moves

`lint --fix`, `lint --fix-safe-only` and `dead-code --fix --write` apply **fewer** rewrites than in
1.8.0 — the ones they withhold are the ones that could not be proven safe. Nothing stopped being
reported: every withheld rewrite still appears as a finding, marked manual. The published fixable rule
count is unchanged at 142, because no rule became report-only; each one narrowed the shapes it will
rewrite.

Real-world check: `format`, `lint --fix` and `dead-code --fix --write` now all build clean on every
repository in the validation corpus — Serilog, Dapper, AutoMapper, FluentValidation, MassTransit,
Polly and Newtonsoft.Json — for the first time.

## 1.8.0 — 2026-09-18

Three additions. Nothing existing changes behaviour: every command you already run produces the same
bytes and the same exit codes as 1.7.0.

### New: `rewrite` — structural search across your code

`dotnet-fast rewrite` finds code by **shape** rather than by text. A pattern is C# with
metavariables, so `Foo($A, $B)` matches any two-argument call to `Foo` regardless of what the
arguments look like or how the call is wrapped across lines — no regex, no false hits inside strings
or comments.

```
dotnet-fast rewrite --pattern 'Assert.AreEqual($A, $B)'
dotnet-fast rewrite --pattern 'Foo($A, $B)' --rewrite 'Bar($B, $A)'     # preview a transformation
dotnet-fast rewrite --pattern 'ObsoleteHelper($A)' --check              # CI gate: exit 1 on any hit
```

With `--rewrite` you get a unified diff of what the transformation *would* do. `--check` exits 1 on
any match, which makes it a gate for "this pattern must not reappear". `--json` gives the machine-
readable form. `--context` and `--selector` narrow what counts as a match.

**There is deliberately no `--write`.** The command previews and reports; it never edits your files.
That is worth explaining rather than leaving you to discover it.

Applying a structural rewrite safely needs a guarantee that the result still compiles, and the only
check available here is a re-parse of the output. That check is permissive on a file that already
fails to parse — and the C# grammar in use does not parse every modern construct, so a single C# 11
list pattern in a file switches the check off for that whole file. A malformed template could then
write code that does not compile while reporting success. Rather than ship a writer whose only
safety net has that hole, the command ships read-only. Apply the previewed diff yourself, with your
own review, and you keep the part that was never in doubt.

Two properties of the preview to know before you apply one by hand:

- **Precedence is not adjusted.** `--pattern 'Wrap($X)' --rewrite '$X'` on `Wrap(1 + 2) * 10`
  previews `1 + 2 * 10`. That compiles and changes the value from 30 to 21. A capture spliced into a
  different precedence context is never parenthesised.
- **Comments inside a match are dropped.** `Foo(1 /* keep me */, 2)` loses the comment.

Both are printed as a caution on every non-empty preview, not just documented here.

`--check` tells you when it could not read something rather than calling it clean: a file that is
not valid UTF-8, one containing the reserved marker, or one the grammar cannot parse is counted,
named with its reason, and makes the run exit 1. A gate that could not look at everything should not
report success.

### Every machine-readable document now says which build produced it

`--format json`, SARIF output and the report files now carry the producing version. An artifact you
find in a CI run is self-describing, so a result can be attributed to a specific build without
reconstructing it from install logs — which, when a feed policy silently serves an older version
than your manifest pins, is otherwise guesswork.

New fields are appended; nothing existing was renamed or moved.

### `doctor --include-dependency-smells`

Opt-in flag folding the dead-dependency MSBuild checks into `doctor`: orphaned central package
versions, duplicate references, and the rest of the CPM rules, reported alongside everything else
`doctor` already looks at. Off unless you ask for it, and `doctor`'s existing output is unchanged.

## 1.7.0 — 2026-09-17

A fix-safety release. Everything here exists because `--fix` could delete code that was used, or write
code that did not compile. If you run `lint --fix` or `format` in CI, take this one.

Two behaviour changes are called out at the end. Read those before upgrading.

### `--fix` no longer deletes private members that are used (#277)

Reported from a real repository where a run removed whole method bodies and broke the build. The
style-tier `IDE0051` pass answered "is this private member used?" from one file's text, and that text
had been stripped in ways that hid real references. Five shapes could each lose a member something
still called:

- **A `partial` type** — a member used from another part in another file looked unreferenced. This
  produced the mass deletions in generated and step files.
- **A reference inside an interpolated string** — `$"...{EscapeOData(value)}..."` was blanked before the
  scan, so a member called only from inside a hole looked unused.
- **An attributed member** — `[DataMember] private int _x;` was deleted and its attribute left behind to
  land on the next member (`CS0592`). Multi-line attributes, stacked attributes, and attributes whose
  arguments contain a bracket inside a string each slipped a different check.
- **A reference only in a doc comment** — `/// <see cref="Helper"/>` was stripped as a comment
  (`CS1574`, an error under `GenerateDocumentationFile` with `TreatWarningsAsErrors`). Both `///` and
  `/** */` are now read.

The pass withholds the deletion whenever one file's text cannot prove the member unused. Expect
`IDE0051` to delete less and report more as manual — a withheld fix costs a look, a deleted method
costs a build. It reports exactly as before, and only runs at all if your `.editorconfig` sets
`dotnet_diagnostic.IDE0051.severity`.

**Known limitation:** a member referenced only from a *disabled* `#if` branch is still removed. That
matches `dotnet format`, which also treats disabled branches as trivia.

### `--fix` no longer writes `??` between incompatible types (#271)

`x != null ? x : y` becomes `x ?? y` only if the operands share a common type. A conditional's natural
type is looser, so the ternary compiles where the `??` does not — `CS0019`. This shipped from **five**
independent places, all now closed:

- The style-tier `IDE0029`/`IDE0030` ternary rewrite and the ported **`RCS1084`** are now **report-only**.
  They still report; they no longer rewrite. Nothing on the line names a target type, so no safe
  rewrite is derivable — the same call already made for `DF0093` and `S3240`.
- **`IDE0270`** (folding `if (x == null) { x = y; }`) now synthesises the widening cast, matching
  `dotnet format` byte for byte: `object value = (object?)text ?? DBNull.Value;`. The cast copies the
  declaration's own nullable spelling, so it does not introduce `CS8632` in a nullable-disabled project.
- `IDE0270` no longer folds a **value type** — `int number = 3; if (number == null)` is legal C# but
  `3 ?? 4` is not.
- **`DF0004`**'s `== null` → `is null` rewrite is withheld on non-nullable value types (`CS0037`).
  Reference types and `int?` keep their fix.

Real-world proof: `lint --fix` now builds clean on all seven repositories in the validation corpus.
MassTransit, which had been failing this exact way, passes for the first time.

`IDE0031`'s `?.` rewrite is also withheld inside a lambda or LINQ query on the same line, where an
expression tree cannot contain `?.` (`CS8072`). Expression-bodied members and switch-expression arms
still get fixed. **Known limitation:** when the lambda's `=>` is on an earlier line than the ternary,
the guard cannot see it.

### `--fix-safe-only` is now actually safe

Several of the rewrites above were classed as safe-tier, so the flag whose purpose is withholding risky
rewrites was applying them and breaking builds. With those rules withheld or corrected, it holds.

### Fixed: a cached "clean" could hide real findings

The cache fingerprint joined selected diagnostic ids with a comma, so `--diagnostics RCS1084,IDE0029`
(one unknown id, selecting nothing) and `--diagnostics RCS1084 IDE0029` (two real ids) produced the
same key. The first cached a "clean" result, and every later verify run of the real selection reused
it — **exit 0 on a file with findings**. Ids are now length-prefixed. No cache version bump is needed
and existing entries stay valid.

### `--fix-changed-lines` bounds `--fix` to what was reported

New opt-in flag on `lint`. With a changed-line scope (`--pr-base`, `--staged`, `--ci`, `--affected`,
`--from`), `lint --fix` reported findings only on the lines your branch touched but rewrote the whole
file. Adding `--fix-changed-lines` keeps the write inside the same scope the report used; a hunk
touching even one out-of-scope line is withheld whole rather than split.

`--fix` on its own is **unchanged, byte for byte**, including with every range flag. Given without a fix
pass or without a resolvable range, the new flag errors rather than quietly doing nothing.
`lint --diff --fix-changed-lines` previews the bounded patch and writes nothing.

### Formatter parity: two shapes the oracle preserves and we did not

- A space after a `case` label before `(` — `case ("XS"):` was tightened to `case("XS"):`. `dotnet
  format` keeps the space, so a repo gating on both tools ping-ponged forever (#254).
- An exotic indent run following an XML doc comment was normalised where the oracle leaves it (#249).

### Behaviour change: `--severity` now applies to `--fix`, not just the report (#251)

**Read this if you run `lint --fix` with `--severity`, `--diagnostics`, `--exclude-diagnostics`, or a
`dotnet_diagnostic.<ID>.severity = none` in `.editorconfig`.**

The native `DFxxxx` rules skipped that gate on the fix path: a finding your options excluded from the
report still had its rewrite applied. The report and the write disagreed, and the write was the one
touching your files. They now agree — `lint --fix` applies a `DFxxxx` fix only when the same invocation
would have reported it.

Practical effect: **`lint --fix --severity error` applies fewer fixes than it did in 1.6.1**, and the
ones it applies are the ones it told you about. Plain `lint --fix` with no selection options is
unaffected and byte-identical. Ported analyzer rules were always gated correctly.

### Behaviour change: fewer fixable findings overall

Between the `IDE0051` withholds, the coalesce family going report-only, and `DF0004`'s value-type
guard, a repository that ran `lint --fix` in 1.6.1 will see some findings move from **fixable** to
**manual**. The fixable rule count moves from 144 to 143. Nothing stopped being *reported*; the tool
stopped writing rewrites it could not prove safe from the information available to it.

## 1.6.1 — 2026-09-17

One security fix. No behaviour changes, no command surface changes: every command produces
byte-identical output to 1.6.0.

### Fix: patched TLS library (RUSTSEC-2026-0285)

The TLS stack bundled in 1.6.0 and earlier (`rustls` 0.23.40) accepted TLS 1.3 handshake messages
across encryption level boundaries. It is updated to 0.23.45, which fixes it.

This affects the two places the tool opens an HTTPS connection of its own — `dotnet-fast update`
(the version check and download) and the remote build cache client (`build --plan` against Azure
Blob/Table storage). Formatting, linting, `affected`, `test-plan`, `dead-code` and `dead-deps` do no
networking at all and were never exposed.

Nothing about the fix changes what the tool does: no flag, default, exit code, report field or
output byte moves. If you do not use `update` or the remote build cache, upgrading is optional.

**NuGet users go from 1.5.1 straight to 1.6.1.** The 1.6.0 package was never published — its
publish run failed on infrastructure, and rather than ship a package with a known-vulnerable TLS
library we folded it into this release. Everything in the 1.6.0 notes below is in 1.6.1, and the
1.6.0 GitHub Release binary stays available for anyone who already has it.

## 1.6.0 — 2026-09-16

Four changes. Three of them make the tool see more of your repository — more test frameworks, more of
MSBuild, more of what your analyzers were configured to read — so a few counts move, always in the
direction of doing more work rather than less. Each is named below so none reads as a regression.

### `test-plan` now shards xUnit and MSTest, not only NUnit

`test-plan` discovers **xUnit** (`[Fact]`, `[Theory]` with `[InlineData]`/`[MemberData]`/`[ClassData]`)
and **MSTest** (`[TestMethod]`, `[DataTestMethod]` with `[DataRow]`/`[DynamicData]`) fixtures alongside
NUnit, in C# and F# alike. Nothing to configure and no new flag: those projects previously contributed
zero fixtures and were reported as `no-fixtures`, so none of their tests ran in any shard.

- **A pure-NUnit repository gets the same plan it always did** — byte-identical.
- A repository that mixes frameworks now sees its xUnit/MSTest tests enter the partition, so shard
  membership and a project's `--filter` can change shape. Strictly more tests run, never fewer.
- `--fail-on-skipped-projects` stops exiting `167` for an xUnit/MSTest repository that used to trip
  it, because those projects are now planned. A project with one unparsable source now reports
  `some-sources-unparsed` where it used to report `no-fixtures` — no code was renamed, but the code a
  given project reports can change.
- The property the shard audit relies on — a `--filter` that matches nothing exits 0 and writes a
  zero-count `.trx` — is now **measured per adapter** (NUnit3TestAdapter 5.2.0, xunit.runner.visualstudio
  2.8.2, MSTest.TestAdapter 3.6.4) and pinned by a live test, as is the filter-escaping grammar. The
  VSTest lane is what is covered; Microsoft.Testing.Platform runners (`EnableMSTestRunner`, xunit.v3)
  define their own "nothing ran" exit code and are not.

Two limits, stated precisely. Attributes are matched by **name**, so a derived attribute
(`[SkippableFact]`, your own `FactAttribute` subclass) is not recognised: a project where *every* test
class uses one is reported `no-fixtures` with a warning; a project that *mixes* recognised and derived
attributes is planned from the recognised classes and the others are absent **without a warning** —
give such a project its own `dotnet test` job. Dynamic data sources score a flat weight, which costs
balance but never coverage, and `--use-cached-timings` replaces the estimate on the second run. One
oddity worth knowing because it looks like a sharding bug and is not: `MSTest.TestAdapter` does not run
an F# test class whose name is double-backtick-quoted, under a plain `dotnet test` exactly as under a
sharded one.

### `lint --deep` reads the analyzer inputs your build reads (#138, #139)

`lint --deep` now evaluates a project's `AdditionalFiles` and its implicit and explicit global usings
through the full MSBuild import chain — `Directory.Build.props`, `Directory.Build.targets`, any
`<Import>` — with conditions, `$()` expansion, globs and multi-target-framework union. Before, only
items written literally in the project's own `.csproj` were seen, and a globbed spec such as
`PublicAPI.*.txt` was skipped outright. A repository keeping `BannedSymbols.txt`, `PublicAPI.*.txt`,
`stylecop.json` or `<ImplicitUsings>` in a shared props file was getting **zero** findings from the
analyzers that depend on them, and unbound symbols were suppressing semantic findings.

**A `--deep` gate on such a repository can go green → red on its first run after upgrading.** Those
findings are what `dotnet build` already reports. The resolution is strictly additive — it is unioned
with what the Roslyn sidecar already worked out on its own, so no repository loses a finding it had,
including where a condition is too exotic for the evaluator and it falls back to the old read.

**One-time `--deep-cache` invalidation.** The cache namespace moves so every existing blob is retired
and the first run after upgrade re-analyzes each project. The key now folds in the analyzer input files'
*content*: an edit to `BannedSymbols.txt` changed what the analyzers reported while leaving the old key
unmoved, and the warm server could serve a stale result after such an edit. Both are closed.

Not claimed, and stated in the docs: `#if` regions (no preprocessor symbols are defined), types from
`ProjectReference`s, the extra Web/Worker SDK implicit usings, and globbed `AdditionalFiles` inside
`bin`/`obj`. Those are #272, #273 and #274.

### `affected` evaluates conditioned references the way MSBuild does (#262)

Conditioned project and package references in shared props are now evaluated against the **final**
property values each project ends up with — MSBuild's pass order — so an opt-in shared-source or
test-utility reference gated on a property the `.csproj` sets later is no longer missed. The reported
set is the union of the previous document-order pass and the new item pass, keeping the old pass's
`Remove`/`Exclude` filters, so an ambiguous evaluation can only widen the set, never narrow it.

- Some repositories will see a slightly larger affected set, and slightly more CI work. That is the
  safe direction: a project previously skipped despite a change reaching it is now built.
- `affected --tests-only` can now return a matrix where it previously exited `166`, because a gated
  test-framework `PackageReference` in shared props now classifies the project. The exit code's meaning
  is unchanged; a pipeline that branches on `166` will behave differently on such a repository.
- Build-cache keys change once for exactly the projects whose evaluation changed — a one-time cold
  miss, never a stale hit. No cache-version bump, nothing to do.
- Cost: on a repository where nearly every project trips the second evaluation, 213 ms → 263 ms
  (MassTransit, ~130 projects).

`Choose`/`When` was measured against the pinned SDK and matches MSBuild — branch selection happens in
the property pass — which corrects an older line in the docs that called it a limitation.

### `dead-code` and `dead-dependencies` show progress on long runs (#215)

Both commands now stream phase-by-phase progress to **stderr**: discovery, scanning, and for
`dead-code` the symbol table and the mark pass. The scanning phase names its denominator and gives each
scanned project a `[k/N]` line with its file or reference count and timing; the other phases report
their start and finish only, and each phase closes with its elapsed time. Under
`--verify`/`--verify-tests` there is one line per project built and one per bisect candidate.

Per-item lines are per **project**, never per source file. At or below 50 projects every project gets a
line; above that they collapse to periodic ticks — every 25 projects and at least ~2 s apart — with a
~10 s backstop so silence is bounded. The lines are plain and uncolored; no spinner, no carriage-return
redraw.

**stdout is unchanged.** The report, `--format json` and `--format sarif` are byte-identical, so
pipelines are unaffected. Progress can never change an exit code: the lines are written best-effort, so
a reader that closes stderr early (`| head`, `| grep -m1`, a truncated CI log) loses the lines and
nothing else. Progress is silent in agent mode, under `-v quiet`, and with the new
`DOTNET_FAST_NO_PROGRESS=1` opt-out — the same three switches as the version banner.

## 1.5.1 — 2026-09-16

Three fixes. Two of them are the kind that matter most: cases where the tool told you it was safe and
was not.

### Fix: `lint --fix` no longer writes C# that does not compile (#252, #253)

Two shapes produced a rewrite that the compiler then rejected:

- **A line-wrap inside a string literal.** When the whitespace pass wrapped a long line containing an
  interpolated string whose *hole* itself contained a literal (`$"…{string.Join(", ", x)}…"`), the
  split point could land inside the nested literal's text, not just between arguments. The scanner is
  now hole-aware: a nested literal ends where it really ends. (#252)
- **A null check rewritten inside an expression tree.** `client != null ? client.Name : null` inside a
  LINQ-to-Entities query was rewritten to `client?.Name` — which an expression tree cannot contain
  (`CS8072`). It took **two** rules to close this: `DF0004` was guarded first, and the same shape then
  fell through to the ported `RCS1206`, so the reporter's build stayed broken with a different error.
  Both now withhold their fix when the expression could be an expression tree — query syntax or a
  lambda — and keep reporting the finding as manual. (#253)

Counts move in three places as a result, none of them a regression: whitespace findings drop on repos
with nested interpolations (the tool was mis-reading them), and `DF0004` and `RCS1206` each shift a
few findings from *fixable* to *manual* (measured: three on FluentValidation, none elsewhere in the
validation corpus). Everything else in the fix path is byte-identical to 1.5.0 — the parity ledger
gained two oracle-generated cases and the existing ones are untouched.

A related problem the same investigation found in the *style* tier — `IDE0031`/`IDE0029` rewrites that
can also produce non-compiling code — is a separate issue, filed as #271, and not in this release.

### Fix: build cache no longer returns a stale assembly when only generated sources changed (#268)

The cache key's input fingerprint skipped files the tool classifies as *generated* — an EF Core
migration's `*.Designer.cs`, a `*ModelSnapshot.cs`, a committed `.g.cs`. Those are ordinary compiled
sources, so a commit that touched only a model snapshot kept the old key and CI restored an assembly
built without the change. The build cache now fingerprints every source the project actually compiles,
excluding only `obj/` intermediates.

**No cache-version bump.** The key format is unchanged; only the set of inputs feeding it widened. Keys
move for exactly the projects that own such a file (and their dependents) and stay byte-identical
everywhere else — no fleet-wide cold rebuild. A related but separate problem, the cached restore
props baking in the producing agent's package root (#241), is not in this release; its fix needs a
cache-version bump and is being paired with other artifact-format work so that bump is spent once.

### `bom` says *why* a project was skipped

On a solution run, a project with neither `packages.lock.json` nor a restored `project.assets.json` was
listed as skipped with no reason unless you asked for `--json`. The default summary now prints the
reason under each entry — including how to get a lock file (`RestorePackagesWithLockFile`, `dotnet
restore`, or `--restore`) and, when `--restore` itself failed, the tail of MSBuild's output. The
existing `  - {name}: {path}` line is unchanged; the reason is on new indented lines below it.

## 1.5.0 — 2026-09-09

### New guardrail: `DF9008` — documentation lives on the type

`DF9001` reports comments that explain code, and exempts `///` XML documentation, because a public
API contract is where prose belongs. Nothing bounded that exemption: a `///` block on every property
and method satisfied `DF9001` while putting a second, unchecked copy of the type's contract on each
member — a copy that rots the first time the code moves on without it.

`DF9008` says **where** documentation belongs: on the `class` or `record`, and nowhere else.
Documentation on a property, method, field, event, constructor or accessor is reported, as is a
`///` block attached to no declaration at all. `/** … */` counts as documentation too. A multi-line
block reports once, at its first line.

The premise is that a type states its contract once, in one place a reader can find, and every member
says what it does through its name and signature. When a member comment carried something the
signature genuinely does not say — a unit, an accepted range, a threading rule — the rule's message
points at the design move that closes the gap for good (`int Timeout` → `TimeoutSeconds`, or a
`TimeSpan`) rather than at a better sentence.

It is the most opinionated of the family, and deliberately narrow: only `class` and `record`
(including `record class` and `record struct`) may carry documentation — an `interface`, `enum`,
`struct` or `delegate` is reported like any member.

**`DF9008` contradicts `SA1600`/`SA1601`/`SA1602`, and you must pick one.** Those ported StyleCop
analyzers require documentation on exactly what `DF9008` forbids it on, and being ports they are on
by default. Enable `DF9008` without switching them off and every public member draws two findings
that cannot both be satisfied. They encode opposing conventions, so there is nothing to reconcile in
code — [guardrails.md](docs/guardrails.md) spells out the conflict and gives you the three
`severity = none` lines, and `editorconfig recommend --guardrails` now writes them **commented out**
with the conflict stated above them, so adopting the profile is still one command and turning a
default-on analyzer off stays your decision. The rest of the `SA16xx` family does not conflict.

Like the rest of `DF900x` it is **off by default** and enabled only by its own
`dotnet_diagnostic.DF9008.severity` line — a bulk `dotnet_analyzer_diagnostic.severity` key does not
switch it on, so no repository acquires it on upgrade. It is report-only; `lint --fix` never deletes
a comment on its account.

## 1.4.1 — 2026-09-07

### Fix: a changed F# file no longer slips through a scoped `lint`

`lint --staged`, `--affected`, `--ci`, `--pr-base`, `--base` and `--from`/`--to` asked Git only for
the changed **`.cs`** files. A changed `.fs`, `.fsi` or `.fsx` file was dropped from the scope
before anything looked at it, so the F# hygiene rules (`FSH0001`–`FSH0004`, new in 1.4.0) and the
`--fantomas` lane never saw it and the run reported clean — while a plain `lint .` over the same
tree reported the findings. That is precisely how a pre-commit hook and a PR gate invoke this tool,
so staging a problem F# file passed the gate.

Changed-file discovery now covers every language the tool reads — `.cs` plus `.fs`/`.fsi`/`.fsx` —
and the extension list is derived from one place, so a language added later cannot drift out of the
Git queries again. Changed-line (hunk) scoping applies to F# exactly as it does to C#: an F# file
touched on one line reports only that line's findings, and `lint --fix` on a scoped run fixes the
in-scope F# files without touching F# files outside the scope.

**C# behaviour is unchanged** — the scope was widened, never narrowed, so a C#-only repository
produces byte-identical output on every scoped command.

This completes the F# work that began with [issue #1](https://github.com/RDalziel/dotnet-fast/issues/1).
With 1.4.0 and this fix together, one invocation covers both languages and only what changed:

```bash
dotnet-fast lint --fix --fantomas --staged .
```

C# formatted and linted, F# hygiene fixed, F# structure formatted by your pinned Fantomas. What F#
does and does not support, with the reason for each limit, is in
[support-matrix.md](docs/support-matrix.md).

## 1.4.0 — 2026-09-07

### F# formatting: `--fantomas` drives Fantomas, so `--verify-no-changes` can finally tell the truth about F#

`dotnet-fast lint --fantomas` and `dotnet-fast format --fantomas` format F# by running
[Fantomas](https://fsprojects.github.io/fantomas/), the community-standard F# formatter — we do not
reimplement it, and we do not intend to. F# is offside-rule: indentation is *syntax*, so a formatter
that guesses wrong emits code that will not compile, and there is no oracle to catch that, because
`dotnet format` has no F# support at all. Point real `dotnet format` at an `.fsproj` and it prints
one line and **exits 0** — its `--verify-no-changes` gives a green build on completely unformatted
F#. That is the gap this closes.

**Fantomas decides how F# looks; we decide what gets formatted, when, and how it is reported.**
Fantomas owns every style decision and is configured entirely by its own `.editorconfig` keys — the
`fsharp_*` namespace plus `max_line_length`, `indent_size`, `end_of_line` and `insert_final_newline`.
No `dotnet-fast` setting changes F# formatting.

What `dotnet-fast` adds is the workflow:

- **Project-aware, affected-scoped discovery.** `--project`, `--include`/`--exclude`, `--affected`,
  `--staged`, `--from`/`--to` and `--ci` narrow the F# set exactly as they narrow the C# one.
- **Unchanged files are skipped.** Fantomas has no notion of what changed and re-parses everything
  every time. A content hash — keyed together with the resolved Fantomas version and the
  `.editorconfig` chain, so an upgrade or a config edit correctly invalidates — means a second run
  over an untouched tree starts Fantomas **zero** times.
- **Batched.** Every file in scope goes out in as few invocations as a command line holds. On a
  48-file fixture that is one process instead of 48 (~38× on the measured fixture), and the skip
  cache then removes even that one on an unchanged re-run (~10× again).
- **`.fantomasignore`, with Fantomas' own semantics** — the nearest one wins and they are *not*
  merged the way `.gitignore` files are, so our file set and Fantomas' cannot drift apart.
- **One report.** F# results land in the same summary, `--report` JSON and `--sarif` output as the C#
  ones, tagged `engine: "fantomas"` / `language: "fsharp"`.

Two findings, and they are deliberately different answers: `FSFMT001` "Fantomas would reformat this"
(fixable) and `FSFMT002` "Fantomas could not process this" (not fixable — usually a file whose `#if`
branches are not each valid F# on their own). A file that did not parse has not been checked, and is
never reported as formatted.

**Strictly opt-in and strictly additive.** Without the flag, nothing changes: `format` still never
rewrites an F# file, and a C#-only run is byte-for-byte identical with and without `--fantomas`.
Nothing is ever downloaded or installed — if Fantomas is missing, the run **fails with exit `168`**
and prints the install command, rather than silently checking no F# and reporting success. The
version pinned in the repository's `.config/dotnet-tools.json` is preferred over a global install, so
everyone formats with the version the team chose.

### F# formatting hygiene: four line-level rules `lint` now reports on `.fs`, `.fsi` and `.fsx`

`dotnet-fast lint` previously had nothing to say about F# source. It now reports four things, and
only four:

| ID | Reports | `--fix` |
|---|---|---|
| `FSH0001` | Trailing whitespace at end of line | yes |
| `FSH0002` | File does not end with a newline | yes |
| `FSH0003` | Line ending does not match the resolved `end_of_line` | yes |
| `FSH0004` | Tab character in leading whitespace | **no — reported only** |

**The narrowness is the design, not a first cut.** F# is an offside-rule language: leading whitespace
is syntax, so changing indentation changes what the program means. Reprinting F# properly needs a
real parse, and [Fantomas](https://fsprojects.github.io/fantomas/) — an AST-based reprinter built on
a fork of the F# compiler — is the community standard for that. We are not competing with it: the
`--fantomas` lane above orchestrates it. These four are the changes that **cannot alter a parse at
all**. Indentation, line wrapping, spacing and anything structural are explicitly out of
scope.

**`FSH0004` is a correctness check, not a style opinion.** The F# compiler rejects a tab outside a
string literal or comment — error FS1161, "TABs are not allowed in F# code". It is reported and
never fixed, because converting a tab to spaces needs the indent width the author meant, and under
the offside rule a wrong width silently moves the line into a different block while still compiling.
That asymmetry with the other three rules is deliberate.

Details worth knowing:

- **No parser.** These are checks on bytes, so they keep working on the F# files a grammar cannot
  parse — which is exactly where you most need a tool to still say something.
- **`.fsx` scripts are covered.** A script is not a `<Compile>` item of any project, so it is found
  by walking each `.fsproj`'s directory. A script outside every `.fsproj` directory is not reached.
- **`.editorconfig` as usual**, with no new keys: `end_of_line`, `insert_final_newline`,
  `trim_trailing_whitespace`, and `dotnet_diagnostic.FSH000x.severity` (where `none` removes the
  finding *and* withholds the fix).
- **Nothing else moved.** The rules run under `lint` / bare `dotnet-fast` / `lint --fix` only. The
  `dotnet format`-compatible verbs (`format`, `whitespace`, `style`, `analyzers`, and the
  `dotnet-format-fast` binary) never touch F#, because real `dotnet format` never did. A C#-only
  repository sees byte-identical output in every mode, and the `DFxxxx` rule counts printed by
  `lint --list-rules` are unchanged — `lint --explain FSH0004` is how you read these four.

See [rules.md](docs/rules.md#f-formatting-hygiene-fsh0001fsh0004).

### `test-plan` no longer drops a test project in silence

Reported as [issue #1](https://github.com/RDalziel/dotnet-fast/issues/1): a project that
`test-plan` classified as a test project but discovered no fixtures in was removed from the plan
with **no diagnostic and exit `0`**. The reporter hit it after adding an F# NUnit project to a
sharded workspace — the project built, `dotnet test` ran its three tests locally, and CI stayed
green while never executing them. A false green is the worst thing this tool can produce, so both
halves are fixed.

- **Every such project is now reported.** It is named on stderr with the reason and the consequence
  ("none of its tests will run in any shard"), and appended to `--format json` and the
  `--report <DIR>` artifact as a new `skippedProjects` array. `reason` is one of
  `language-not-supported`, `source-discovery-failed`, `no-source-files`, `no-parsable-sources`,
  `some-sources-unparsed` or `no-fixtures`. Existing JSON fields are unchanged; the array is always
  present, usually empty.
- **New `--fail-on-skipped-projects`** exits `167` instead of warning. Off by default: a scaffolded
  but still-empty test project is legitimate, and a new non-zero default would break those repos.
  Turn it on once yours has none, and a test project that quietly stops contributing tests can never
  report green again.
- **F# NUnit fixtures are discovered.** `[<TestFixture>]` types, types with `[<Test>]` members and
  no attribute, module-level `[<Test>] let` bindings (the module is the fixture), nested modules,
  `[<AbstractClass>]` bases, `[<SetUpFixture>]` exclusion, and ``double-backtick`` names. Fixture
  names use the same runtime form as C# — a module compiles to a class, so a type in
  `module CalculatorTests` is `CalculatorTests+CalculatorTests`. Every name form was checked against
  a real `dotnet test` run. See [test-sharding.md](docs/test-sharding.md#f-test-projects).
- **`.fsproj` compile items are evaluated properly** rather than falling back to a `*.cs` directory
  walk: no implicit compile glob (the F# SDK disables it), declared order preserved, `.fs` and
  `.fsi` included, `.fsx` scripts excluded.
- **A solution resolves to the same project set everywhere.** `affected` returned an `.fsproj`
  listed in a `.sln`/`.slnx` while `format`/`lint` skipped it. Both now read one predicate, so an
  F# project in a solution is visible to every command. An `.fsproj` is also accepted as a target
  where it used to be rejected outright.
- `docs/support-matrix.md` said flatly that "VB and F# projects are not analyzed", which
  contradicted `affected`'s documented behaviour and is now wrong for `test-plan` too. It states
  what each command actually reads.

No existing flag, default, output field or exit code changed.

### `--help` doc pointers fixed

Three `--help` descriptions (`dead-code`, `dead-dependencies`, and `lint`'s
`--only-active-analyzers`) linked to a doc path that does not exist in the public repo — the pointer
went nowhere. `dead-code --help` now points at `docs/dead-code.md`, `dead-dependencies --help` at
`docs/dead-dependencies.md`, and `--only-active-analyzers` at
`docs/ported-analyzers.md#enabling-and-disabling-ports`. Text-only fix — no flag, default, or output
shape changed.

## 1.3.0 — 2026-09-03

**Action required if `metrics` gates your build: re-run `dotnet-fast metrics --write-baseline` before
you upgrade a baseline you rely on.** The scoreboard grows from ten budgets to fourteen — a baseline
recorded before this release never mentions the four new ids, so they are judged against their plain
budget the first time this build measures them, exactly like any other newly-measured row. That is
usually the right behaviour, but it means a repository that was previously green under `--baseline`
can now fail on debt the ratchet never saw. Re-running `--write-baseline` records where the four new
rows stand today and keeps the ratchet meaning what it always has.

### `metrics` — four new rows, `--top all`, `--by-project`

- **Four rows appended** (never renumbered): lines per member (≤ 50), nesting depth (≤ 3), parameter
  count (≤ 7), and maintainability index (≥ 20 — a floor, like coverage). Every default is lifted from
  a threshold this tool already ships elsewhere (the `DF9002` guardrail, the native `S134`/`S107` lint
  ports), not invented. New `--max-lines-per-member` / `--max-nesting-depth` / `--max-parameters` /
  `--min-maintainability-index` flags and `dotnet-fast.json` keys override them; the text board's
  column widths are unchanged and every one of the original ten lines still prints byte-for-byte what
  it always has.
- **`--top all`** (or `--top unlimited`) lists every offender behind an over-budget row instead of the
  default 5 — the only expensive flag here, by orders of magnitude on a large solution.
  `worstMembers` stays capped at 5 regardless, so it cannot grow into a multi-megabyte JSON array.
- **`--by-project`** (opt-in) breaks the board down one line per project — text gets an appended
  block, JSON gets an appended top-level `projects` array. Whole-run figures (coverage, surviving
  mutants, dead code) are not repeated per project; they read as not measured there. Never affects the
  exit code.
- Agent-mode output now opens with a one-line summary and names what was never measured, and caps
  offenders at 3 per row regardless of `--top`.
### The seventh guardrail, and one-command adoption

**New guardrail: `DF9007`, cognitive complexity.** Of the ten published code-health budgets, the
lint gate previously covered three: cyclomatic complexity (`DF9005`), Halstead difficulty (`DF9006`)
and lines-per-file (`DF9003`). Cognitive complexity had no configurable guardrail at all — the closest
thing was the `S3776` port, pinned at a fixed threshold of 15 on a different config axis. `DF9007`
closes that gap: opt-in like the rest of the family, default threshold 22, configured with
`dotnet_fast_max_cognitive_complexity`, scored by the same code `S3776` and `dotnet-fast metrics` use
so all three can never disagree about a member. See [guardrails.md](docs/guardrails.md).

Two more budgets — redundant code and dynamic typing — were assessed for guardrails of their own
(`DF9008`/`DF9009`) and deliberately **not built**: `S4144` (identical member bodies) and `PH2044`
(the `dynamic` keyword) already cover them, and both ports are on by default, so a guardrail twin
would double-report the same defect for anyone who has not disabled the port. Use `S4144`/`PH2044`
for those two.

**One-command adoption: `editorconfig recommend --guardrails`.** Adopting the guardrail family used to
mean switching on six rules (now seven) and setting five thresholds by hand, one of which (lines per
file) has always meant something different from the published SLOC budget — the rule's own default
is 250, the budget is 500. `dotnet-fast editorconfig recommend --guardrails --write` now appends the
whole block in one command: every `DF900x.severity = warning` line, and every threshold stated
explicitly at the published budget number (`dotnet_fast_max_lines_per_file = 500` included). Same
`--write` contract as the existing `recommend` command — appends, never overwrites, idempotent on a
second run.
### `metrics`: estate rollup across repositories (`metrics --merge`)

`metrics --format json` now appends two fields to its `summary`: `schemaVersion` and `repository`
(inferred from the git `origin` remote, overridable with `--repository-name`). New
`metrics --merge <glob>` reads many repositories' own `--format json` reports — offline, no server —
and renders one combined view (`text`, `json`, or a self-contained `html` page) sorted worst-first,
with a per-budget breach summary across the estate. Only budgets a source-only pass can see (source
statics and the opt-in dead-code pass) appear for every repository that ran them; the three
budgets that need a test or mutation run (coverage, CRAP, surviving mutants) appear as a column only
where at least one repository actually measured them, and never as a placeholder for the rest — a
blank column reads as "not measured", never as a pass. A file `--merge` cannot understand (missing or
unrecognised `schemaVersion`) is skipped with a clear reason, never silently merged. Everything here
is additive: a plain `metrics` run with no `--merge` is unchanged. See [metrics.md](docs/metrics.md)
and [ci.md](docs/ci.md).

## 1.2.0 — 2026-09-01

**Action required if `metrics` gates your build.** Five fixes below change numbers `metrics` reports —
mostly upward, because each one was under-reporting. Re-run `dotnet-fast metrics --fail-on-budget`
on a branch and look at the new figures before you let the fixed version gate a merge.

### `metrics` — five fixes

- **CRAP was too low for any member containing a blank line.** The coverage lookup for a member used
  its *non-blank* line count as the span, so it stopped short of the member's real last line and
  scored the missing lines as if they did not exist. Members with blank lines between their steps —
  the readable ones — came out looking better covered than they are. CRAP for those members rises.
- **A coverage report could attach to the wrong file.** File matching compared path suffixes without
  requiring a directory boundary, so `covered.cs` matched `uncovered.cs`, and when more than one
  candidate matched, which one won was not even stable between runs. A match now has to start at a
  path separator, and an ambiguous one is treated as no match — which shows up as an unmeasured file
  rather than a wrong number.
- **An empty mutation report read as a pass.** A Stryker run that scored nothing — a crash, a filter
  that matched no code, a report written before the run finished — produced `Surviving mutants 0`,
  green. `metrics` now fails with a clear message instead, the same way an empty coverage report
  already did. A board that is green because nothing was checked is worse than no board.
- **Fractional budgets printed rounded.** `--min-coverage 99.5` rendered as `100` in the text table
  and in the JSON `budget` field, so the report disagreed with the flag you passed. Budgets now print
  as given (`99.5` stays `99.5`, `100` stays `100`).
- **Files it could not read, or could not parse, vanished silently.** A file that is not valid UTF-8,
  or that a permission error hides, was skipped without a word, so the scoreboard quietly described
  less code than you thought. Both are now counted and reported: a new footer line, plus
  `summary.readFailures` and `summary.filesWithSyntaxErrors` appended to the JSON. Files with syntax
  errors are still measured — they are counted, not excluded.

### `metrics` — new options

- **Budgets for the four count metrics**: `--max-surviving-mutants`, `--max-dead-code`,
  `--max-duplicate-bodies` and `--max-dynamic-uses`, each also settable once in `dotnet-fast.json`
  as `maxSurvivingMutants`, `maxDeadCode`, `maxDuplicateBodies` and `maxDynamicUses`. Previously
  those four were fixed at zero.
- **More coverage formats.** `--coverage` now reads **OpenCover**, **LCOV** and **coverlet JSON** as
  well as Cobertura, and works out which it is from the file itself. `--coverage-format
  auto|cobertura|opencover|lcov|coverlet-json` pins the choice when you would rather be explicit.
  Directory search picks up `*.opencover.xml`, `lcov.info`/`*.lcov` and `coverage.json` too.
- **`--coverage` and `--mutation` are repeatable.** Pass one per test project and the reports are
  merged. A single use behaves exactly as before.
- **A ratchet, so you can adopt budgets on a codebase that is already over them.**
  `--write-baseline <file>` records where you stand today; `--baseline <file>` with
  `--fail-on-budget` then fails only on a row that got *worse* than the baseline. Rows that are over
  budget but no worse say so and do not fail the build. Without `--baseline`, nothing changes.
- **`--format sarif`**, so over-budget members and files land in GitHub code scanning next to your
  lint findings.
- **Worst offenders in the JSON.** Every metric row now carries an `offenders` array (name or file,
  line, value), sized by `--top`, and the text report lists offenders for surviving mutants,
  redundant code, `dynamic` usage and dead code as well.

### Everywhere else

- **The NuGet package page now shows the same README as the docs site.** The package had been
  shipping an internal document by mistake, with links that went nowhere for anyone but us.
- **Every release now has a downloadable binary.** `dotnet-fast-win-x64.exe` and its `.sha256` are
  attached to each GitHub release, closing the gap [security.md](docs/security.md) previously
  described as a 1.x item. NuGet remains the recommended install and the one with a signature —
  a checksum next to a file only catches a corrupted download.
- **These docs now have history.** The public repository keeps a commit per version with a `vX.Y.Z`
  tag, so you can read what a page said at the version you are running instead of only its current
  state.

Nothing existing changed: no command, flag, default, output shape or exit code moved.

## 1.1.0 — 2026-09-01 — `metrics`: one scoreboard for the numbers that catch AI slop

A new command, **`dotnet-fast metrics`**, scores a codebase against ten code-health budgets in one
build-free pass:

```
dotnet-fast metrics App.sln
```

Seven of them come straight from your source, measured in the same walk: cyclomatic complexity,
cognitive complexity, Halstead difficulty, lines per file, dead code, redundant code, and `dynamic`
usage (C#'s nearest equivalent to TypeScript's `any`). The other three cannot be derived from source
because they require running your tests — so `metrics` **reads the reports your pipeline already
produces** and never runs anything itself:

- `--coverage <path>` — a Cobertura report (`coverage.cobertura.xml`), or a directory to search. This
  also fills in **CRAP**, which multiplies complexity by lack of coverage so that a member which is
  both complex and untested rises to the top of the list.
- `--mutation <path>` — a Stryker.NET `mutation-report.json`, or a directory to search.

`--fail-on-budget` turns it into a CI gate; without it the command is report-only and exits 0.
`--format json` gives the machine-readable form, including the full Halstead figures per member.

**A metric you did not measure is never shown as passing.** Without a coverage report, the coverage
and CRAP rows read *not measured* and name the flag that would fill them, and `--fail-on-budget`
ignores them entirely. A board that is green because nothing was checked would be worse than no board.

Budgets default to widely-published values (cyclomatic and cognitive 22, Halstead difficulty 80, 500
lines per file, 100% coverage, CRAP 25, and zero for the rest) and can be overridden per run on the
command line or once in `dotnet-fast.json`.

### Two new guardrails: `DF9005` and `DF9006`

The opt-in guardrail family — the rules for repositories where an agent writes the code and a human
reviews it — gains the two budgets that belong in a per-file CI gate:

| Rule | Reports | Threshold |
|---|---|---|
| `DF9005` | A member over 22 independent paths — also the number of cases its tests must cover. | `dotnet_fast_max_cyclomatic_complexity` |
| `DF9006` | A member over 80 Halstead difficulty — too many different operations over too few values. | `dotnet_fast_max_halstead_difficulty` |

Both are **off by default** and report-only, like the other four, and both share their scorers with
`metrics`, so the gate and the scoreboard can never disagree about a member. See
[guardrails.md](docs/guardrails.md).

Note that `DF9005` measures something genuinely different from the `S3776` cognitive-complexity rule
already shipped among the ported analyzers: a wide flat `switch` scores 25 for cyclomatic complexity
and 1 for cognitive. One asks how many cases there are, the other how hard it is to read. Both are
worth knowing.

Nothing existing changed: no command, flag, default, output shape or exit code moved.

## 1.0.1 — tagged, never published to NuGet, superseded by 1.1.0

`1.0.1` exists as a tag only. It was never pushed to nuget.org, so `dotnet tool install` and
`dotnet-fast update` never saw it and there is nothing to upgrade from. Everything in it shipped in
`1.1.0`. If you pinned `1.0.1` in `.config/dotnet-tools.json`, the restore will fail — use `1.2.0`.

## 1.0.0 — 2026-08-08 — the compatibility promise starts here

**`1.0.0` is `1.0.0-rc.1` plus one fix**, found by the pre-release install verification and described
below. Nothing else in the tool differs between the two. What else changes is what you are entitled
to rely on.

```bash
# new install
dotnet tool install --global RDLL.dotnet-fast

# already have it (including from the candidate)
dotnet-fast update
```

A plain install or update now resolves `1.0.0` — no version flag needed, and the GitHub "latest
release" link points here.

### Fixed

**A `--tool-path` install into a deep directory could crash on its first run.** The failure was an
unhandled .NET exception ending in "The system cannot find the file specified" — about a file that
was sitting right there on disk.

The cause is a Windows limit rather than a missing file: a `--tool-path` install nests about 130
characters of `.store` layout under whatever directory you choose, and once the native binary's full
path reaches 260 characters `CreateProcess` refuses to start it. That limit is not lifted by the
`LongPathsEnabled` setting, so a machine configured for long paths hit it anyway. It affected the
install shape our own CI guidance recommends (`--tool-path <dir>`, then cache `<dir>`); the default
global install is nowhere near the limit.

The launcher now starts such a binary correctly via its short path, and if that is impossible — 8.3
short names can be disabled per volume — it exits with a message naming the path length, the limit,
and the fix, instead of a stack trace. A package smoke test now installs to a deliberately
over-length path on every run, and fails loudly if the path it produces is *not* over the limit, so
the guard cannot quietly stop testing anything.

### What 1.0 actually promises

Everything in **[versioning.md](docs/versioning.md)**, which was already how this tool was
maintained — the difference is that breaking it now costs a major version instead of being a judgement
call:

- **A pipeline pinned to `1.x` keeps working.** An existing command or flag's name, behavior,
  defaults, output shape (text, `--json` fields, SARIF fields, report file names) and exit codes do
  not change. New capability arrives as new commands and new opt-in flags.
- **Nothing is removed outright.** A retired flag keeps working while printing a deprecation notice
  naming its replacement, for multiple releases, before removal in a major — and then fails with a
  message telling you what to use, not `unknown command`.
- **The parity floors are part of the promise.** A release does not ship below the published
  per-repository floors in [support-matrix.md](docs/support-matrix.md).

### What 1.0 does not mean

It is a compatibility milestone, not a claim of completeness. Stated plainly so you can plan around
it rather than discover it:

- **Windows x64 only.** The package carries a `win-x64` native binary and nothing else, so a Linux or
  macOS install *succeeds* and then fails on first run. linux-x64 is a 1.x item and starts outside the
  promise.
- **Formatter parity is 99%+, not 100%.** The current per-repository figures — measured over files
  *both* tools actually opened, a check the harness now fails on rather than papering over — are in
  the [support matrix](docs/support-matrix.md). Three known divergences remain, each with an open
  issue rather than a silent expectation change.
- **`--deep` findings still come from your analyzers and your SDK**, not from us, so they move when
  those move.

### Upgrading from the candidate

Nothing to do beyond `dotnet-fast update`. If you pinned `1.0.0-rc.1` explicitly in
`.config/dotnet-tools.json` or a CI step, change it to `1.0.0`; the pre-release stays published and
installable, but it will not receive fixes.

## 1.0.0-rc.1 — 2026-08-05 — release candidate for 1.0

The first release candidate for `1.0.0`. Everything below also ships in it.

**It will not reach you by accident.** A plain `dotnet tool install` or `dotnet-fast update` still
resolves the stable line (`0.307.0`), and the GitHub "latest release" link still points at stable.
You get the candidate only by asking:

```bash
# new install
dotnet tool install --global RDLL.dotnet-fast --version 1.0.0-rc.1

# already have it
dotnet-fast update --to 1.0.0-rc.1

# back to stable at any time
dotnet-fast update --to 0.307.0
```

### What 1.0 means

Two documents define it, and both are worth reading before you pin to it:

- **[Support matrix](docs/support-matrix.md)** — what is in scope and what is deliberately not.
  Headline: **Windows x64 is the only supported platform**, and the only one a binary ships for.
- **[Versioning promise](docs/versioning.md)** — what a major, minor and patch will mean after 1.0,
  and the compatibility rules the command surface is held to.

### Fixed since 0.307.0

**We were formatting code `dotnet format` never opens — three separate causes.**

1. `<File>` entries under a `.slnx` solution's Solution Items were treated as **projects**. Where
   that file was an MSBuild traversal project at the repository root, it had no target framework and
   no real compile list, so its default file glob matched *the entire repository* — every `.cs` file
   was then processed a second time under the wrong set of preprocessor symbols.
2. A `#if` line with a trailing comment (`#if !NETFRAMEWORK // not supported here`) failed to parse,
   and an unparseable condition was assumed **active**. Code inside a branch your build excludes was
   being formatted.
3. A UTF-8 byte-order mark at the start of a file hid the `#` of a first-line directive, with the
   same result.

**`affected` could report a project nobody changed.** The same `.slnx` defect promoted that root
traversal file to a project, so `affected` named it on *any* source change. If your CI feeds that
list to `dotnet build`/`dotnet test`, it rebuilt everything — the exact opposite of the point.

**`DF0010` no longer rewrites code that then fails to compile.** Converting `$"{value.ToString()}"`
to `$"{value}"` is only valid on target frameworks with the modern interpolated-string handler. The
fix is now withheld unless *every* target framework of the owning project supports it, and an
unknown framework withholds rather than guesses.

**Files that are entirely inside a disabled `#if` now match `dotnet format` byte for byte.**

### Changed

The four opt-in [guardrail rules](docs/guardrails.md) (`DF9001`–`DF9004`) report the same code as
before, but their messages were rewritten to name the design principle behind the rule and the
concrete refactoring move — single responsibility and extract-method rather than "file too long".
They are aimed at an AI coding agent reading the error and deciding what to do next, where "too
long" invites deleting blank lines and naming the principle invites separating concerns.

### Trying it

If you run it, please report anything that breaks — that is what a candidate is for. Formatter
output differing from `dotnet format`, a fix that does not compile, or a command behaving
differently from its documentation are all worth an issue.

---

## Older releases

The `0.x` series predates the `1.0.0` compatibility promise and the NuGet package; its notes were
retired to git history — **[RELEASES-0.x.md](RELEASES-0.x.md)** says where to find them.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

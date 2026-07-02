# Release notes

What changed in recent releases, in plain English. Newest first. The tool is `0.x` — usable and tested,
but commands may still change before `1.0`.

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

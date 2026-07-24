# Dead code

`dotnet-fast dead-code` finds unused code across a whole solution — types and members that nothing
in production reaches — plus a distinct **test-only** category for code that's alive only because
tests exercise it. It's build-free (no MSBuild, no restore, no Roslyn) and conservative by
contract: **everything it reports is safe to delete.** Any ambiguity it can't resolve — a name it
can't pin down, reflection it can't follow, a file it can't parse — keeps the symbol alive instead
of risking a false positive. It understands xUnit collection fixtures, extension methods,
assembly/module-level attribute applications, and the assembly-scanning discovery patterns used by
EF Core, ASP.NET Core (controllers, Razor Pages), Blazor, AutoMapper, and FluentValidation, so it
won't ask you to delete any of them.

Validated end-to-end against six real open-source codebases (Newtonsoft.Json, Dapper,
FluentValidation, AutoMapper, Polly, Serilog): every write path — `format`, `lint --fix`, and
`dead-code --fix --write` — builds cleanly on all six, including a fresh copy with `--fix --write`
applied and then run a second time to confirm there's nothing left to remove.

## Quick start

```bash
dotnet-fast dead-code .                       # human report (report-only, exits 0)
dotnet-fast dead-code . --format json         # { findings: [...], summary: {...} }
dotnet-fast dead-code . --fail-on-dead        # exit 1 when dead code is found (CI gate)
dotnet-fast dead-code . --include-public      # closed-world: analyse public API too
dotnet-fast dead-code . --fix                 # DRY-RUN: preview the removal diff, write nothing
dotnet-fast dead-code . --fix --write         # apply the removals
```

A clean run against a small solution looks like this:

```
src/Billing/OrphanCommand.cs:16:16: DC0001 dead type Billing.OrphanCommand
src/Billing/OrphanHandler.cs:20:16: DC0001 dead type Billing.OrphanHandler
dead-code: 2 finding(s) — 2 dead type(s), 0 test-only type(s), 0 dead member(s), 0 dead project(s).
```

## What it finds

| ID | Category | Meaning |
|---|---|---|
| `DC0001` | dead | a type is unreachable from anything in production |
| `DC0002` | dead | a member (method, property, field, …) is unreachable from anything in production |
| `DC0003` | test-only | a type is reachable only from test code, not from production |
| `DC0005` | dead | a whole project is unreferenced by anything else in the solution |
| `DC0006` | dead | dead code *inside a test project itself* — e.g. a test helper no test actually calls |

There's no "maybe dead" bucket. A symbol is either proven unreachable (reported) or it stays
silent — ambiguity always resolves to "alive," never to a finding.

## The conservative-by-contract guarantee

Marking is name-based, not type-checked: a use of a name marks *every* candidate symbol with that
name that could possibly be visible from the calling code. If more than one candidate could match
— an overload, a same-named type in another namespace, a reflection call whose target string isn't
a literal — **all of them** are marked alive, not just the "likely" one. A few consequences that
follow from that rule:

- **Public and protected members are roots by default.** Anything externally visible is assumed
  consumed by something outside the solution the tool can see, so it's never reported unless you
  opt into closed-world analysis (`--include-public`, below).
- **Any attributed symbol is a root** (serialization attributes, DI markers, test attributes, a
  custom framework attribute you've never heard of) — except a short, curated list of purely
  cosmetic attributes (`[Obsolete]`, `[DebuggerDisplay]`, `[ExcludeFromCodeCoverage]`) that never
  represent a real call site.
- **A string literal that looks like an identifier** (e.g. passed to `GetMethod("Cancel")`) counts
  as a use of a symbol named `Cancel`. A *non-literal* reflection target (a computed string) can't
  be read at all, so the containing project falls back to type-level findings only — no
  member-level reporting there.
- **A file the parser can't read taints its whole project** — nothing is reported for that project,
  because one unreadable file could hide any use edge.
- **Generated code** (`*.g.cs`, `*.Designer.cs`, `obj/**`) is always a source of uses, never a
  target you'll see flagged.

The asymmetry is deliberate: a false positive here means someone deletes code that was actually
used — a real correctness bug. A false negative just means a missed cleanup opportunity. Every rule
above leans toward the second, cheaper mistake.

## Test-only code

Code that exists solely to support tests (helpers, fixtures, fakes) is real and intentional — but
"you can delete this from production" and "you can delete this at all" are different facts. To tell
them apart, `dead-code` marks reachability twice: once from production roots only, and once adding
test roots (`[Fact]`, `[Theory]`, `[Test]`, `[TestCase]`, `[TestMethod]`) and test-originated use
edges. A symbol reachable only in the second pass is `DC0003` — safe to remove from production, but
check the test that references it first. A symbol unreachable in *both* passes and living inside a
test project itself is `DC0006` — a dead test helper. Pass `--no-test-only` to hide the `DC0003`
category and see only fully-dead code.

Test frameworks also wire some fixtures up purely by reflection, with no code ever naming them —
xUnit's `[CollectionDefinition]` classes are the standard example. `dead-code` treats any attributed
type inside a test project as reachable for exactly this reason, the same way it already treats an
attributed production type as reachable — so those fixtures are never reported, and never deleted.

## Framework indirection (`--handler-pattern`)

Message-driven frameworks (MediatR, MassTransit, and similar) dispatch handlers through DI/runtime
plumbing, so a plain reachability pass never sees a direct call from "publish this message" to "the
handler that handles it." `dead-code` understands this shape generically: a handler type that
implements a generic handler interface is bound to its message-type argument, so if the message is
constructed anywhere in production, the handler counts as reachable — and if *both* the message and
its handler are unreachable, that's reported as the useful "orphaned handler" case.

Built-in, package-gated patterns (only activate when the matching NuGet package is actually
referenced) cover MediatR, MassTransit, Rebus, NServiceBus, Wolverine, and Brighter — nothing to
configure for those.

For an in-house command/handler framework, teach the tool the shape with `--handler-pattern`:

```bash
dotnet-fast dead-code . --handler-pattern "ICommandHandler<1,0>"
```

The form is `IName`, `IName<arity>`, or `IName<arity,messageArgIndex>` — the interface's simple
name, how many generic arguments it takes, and which argument position is the message type
(defaults: arity `1`, message argument `0`). It's repeatable for multiple in-house interfaces.
The same patterns can live in `dotnet-fast.json` instead of being repeated on every invocation:

```json
{
  "deadCode": {
    "handlerPatterns": [
      { "interface": "ICommandHandler", "arity": 1, "messageArg": 0 }
    ]
  }
}
```

If a pattern's interface name happens to collide with an unrelated local `interface` declaration,
that's just another ambiguity — the type in question stays alive, never gets reported. You lose a
little recall on that one collision, never correctness.

## Framework awareness built in

Some patterns need no configuration at all — `dead-code` recognizes them out of the box:

- **Assembly/module attribute applications.** A type applied as `[assembly: Foo]` or `[module: Foo]`
  is reachable via that application, and the C# short-form attribute convention is honored too — a
  bare name like `[assembly: Foo]` may bind to either `Foo` or `FooAttribute`, so a custom attribute
  class wired up purely this way (e.g. a test-runner scope attribute instantiated by the framework's
  own reflection scan at assembly load) is never reported dead just because nothing calls `new` on it.
- **EF Core reflection discovery.** `IEntityTypeConfiguration<TEntity>` implementers (swept in by
  `modelBuilder.ApplyConfigurationsFromAssembly(...)`, which reflects over the assembly and
  instantiates every implementer), `IDesignTimeDbContextFactory<TContext>` implementers (discovered by
  the external `dotnet ef` design-time tooling reflecting over the built assembly), and `Migration`
  subclasses (your applied EF Core migrations, discovered by EF's own migrations-assembly scan) are
  always treated as alive once the solution references an EF Core package — none of these patterns has
  an in-repo call site by design, so there's nothing to configure here either.
- **LightBDD scenarios.** A method carrying `[Scenario]` is recognized the same way `[Fact]`/`[Test]`
  already are — executed by LightBDD's own runner via the underlying NUnit/xUnit/MSTest adapter, never
  called directly from your code. A `FeatureFixture`-derived class with a `[Scenario]` method (whether
  declared in one file or split across a `Scenario.Steps.cs`/`Scenarios.cs` partial pair, a common
  LightBDD convention) is never reported dead.
- **ASP.NET Core controllers and Razor Pages.** `ControllerBase`/`Controller` subclasses (discovered
  by `AddControllers()`'s assembly scan) and `PageModel` subclasses (discovered the same way by Razor
  Pages) are always alive in a web project — recognized whether the project uses the
  `Microsoft.NET.Sdk.Web` SDK or an explicit ASP.NET Core framework reference. An in-house base class
  interposed between the framework type and your own controllers (`ApiControllerBase :
  ControllerBase`, then every controller derives from that) is covered too, not just a direct
  subclass.
- **Blazor components.** `ComponentBase`, `LayoutComponentBase`, and `OwningComponentBase` subclasses
  are always alive once the solution references the Blazor components package or a web project SDK —
  components are typically referenced only from `.razor` markup, which sits outside anything a
  C#-only analysis can see, so without this they'd be permanent false positives.
- **AutoMapper `Profile` and FluentValidation validators.** `Profile` subclasses (swept in by
  `AddAutoMapper(assembly)`) and `AbstractValidator<T>`/`IValidator<T>` implementers (swept in by
  `AddValidatorsFromAssembly(assembly)`) are always alive once the matching package is referenced —
  same reflection-sweep shape as the EF Core and ASP.NET Core patterns above.
- **xUnit `[CollectionDefinition]` fixtures** (above) and **extension methods** (below) are the same
  kind of zero-configuration awareness.

A known limitation worth stating plainly: `.razor`/`.cshtml` markup files are outside this analysis
entirely (it parses C#, not Razor markup). That cuts both ways. In the direction that matters most —
false positives — the keep-alive entries above cover it: a component or page discovered purely by
markup routing is never wrongly reported dead. But the tool has no way to see the other direction
either: it can't tell you whether a component is genuinely still referenced somewhere in your
`.razor` files, because it never reads them. It simply treats every framework-discovered
component/page as alive rather than guessing.

## Extension methods

`widget.Describe()` never spells out the static class that declares `Describe` — that's the whole
point of extension-method syntax. `dead-code` resolves the called name against the declaring
extension class too, not just against ordinary type names, so a class full of extension methods
someone actually calls is never reported (or deleted) as dead. A class of extension methods nobody
calls is still reported, same as any other dead type.

## Removing dead code (`--fix`)

`--fix` is **dry-run by default** — it prints a unified diff of what it would remove and changes
nothing on disk:

```
--- a/src/Billing/Unused.cs
+++ b/src/Billing/Unused.cs
@@ -1,8 +1,2 @@
 namespace Billing;

-internal class Unused
-{
-    public void Never()
-    {
-    }
-}
dead-code --fix (dry-run): 1 symbol(s) across 1 file(s) would be removed. Re-run with --write to apply.
```

Add `--write` to actually apply it. `--fix` only ever removes *auto-removable* findings — a
single-part dead type (`DC0001`) or a single-name dead member (`DC0002`/`DC0006`), each together
with its own leading comment lines. It **never** touches:

- test-only code (`DC0003`) — it's still live under tests
- partial types — they can span files a generated part may own
- a whole dead project (`DC0005`)
- a multi-name field declaration (`private int a, b;`)
- a dead type still referenced by a surviving partial part's members — if the rest of a dead but
  unremovable `partial` type still names one of its own dead nested types, that nested type is
  reported but not removed either, since the surviving code has to keep compiling

Every finding's JSON also carries `"removable": true`/`false` so tooling can tell which ones `--fix`
would actually touch without guessing from the id alone. And you never need to run `--fix --write`
more than once to clean up everything it's going to clean up — the removability of every finding,
including the partial-type case above, is fully resolved before anything is written to disk, so a
second run is always a no-op.

Those are left for a human to decide. Every removal is a pure text cut over the declaration's exact
span, never a reformat, so the diff you see is exactly what gets applied.

## Closed-world analysis (`--include-public`)

By default, `dead-code` is **internal-only**: any `public`/`protected` member of a non-test project
is assumed to be a library's public surface, consumed by something the tool can't see (a NuGet
package, a downstream repo), so it's never a candidate. That's what makes a first run on any
solution safe with zero configuration.

`--include-public` switches to closed-world analysis — public and protected members must earn a
real use edge like anything else — which only makes sense when the scanned solution really is the
whole world with no external consumers. Even then, two exemptions stay in force because they *are*
deliberately-external surfaces even inside a closed repo: types in a packable project
(`IsPackable=true` or a `PackageId` set — i.e. it ships as a NuGet package), and symbols exposed to
another assembly via `InternalsVisibleTo`.

**Partial-scope runs ignore `--include-public` automatically.** If you point `dead-code` at a
single project file directly (e.g. `dotnet-fast dead-code Lib/Lib.csproj`) instead of a directory or
solution, the tool cannot see sibling projects outside that file's own folder — a real consumer
(`Consumer/Consumer.csproj` referencing `Lib/Lib.csproj`) could exist and simply be invisible to the
run. Rather than risk reporting a used type or project as dead, `--include-public` is treated as
absent for that run — public API stays rooted, and a project with `InternalsVisibleTo` is never
reported as a whole dead project (`DC0005`) either. The human report explains this with an extra
note, and `--format json` sets `summary.partialScopePublicRooted: true`. Point `dead-code` at the
containing directory or a solution file to get a scan that can see every consumer.

The same protection applies to a **folder** target that isn't the whole solution: if a governing
`.sln`/`.slnx`/`.slnf` found above the scanned folder lists a project the folder scan didn't
discover, the folder is narrower than that solution's own graph, so it gets the same
`--include-public` downgrade and warning. A folder that directly contains its own matching solution
file (or an ordinary directory scan with no solution file anywhere above it) is unaffected.

## CI patterns

`dead-code` is report-only by default — a clean report and a report full of findings both exit `0`,
because this is a discovery tool, not a gate, until a team opts in. Add `--fail-on-dead` to turn
production-dead findings into a CI failure:

```bash
dotnet-fast dead-code . --fail-on-dead
```

Test-only findings (`DC0003`) never flip the exit code on their own, even under `--fail-on-dead` —
they're informational, not something to fail a build over.

For automation, `--format json` gives you a structured report:

```json
{
  "findings": [
    {
      "id": "DC0002",
      "category": "dead",
      "kind": "member",
      "symbol": "MyApp.Services.OrderProcessor.Cancel",
      "file": "src/MyApp.Services/OrderProcessor.cs",
      "line": 42,
      "column": 17,
      "project": "MyApp.Services",
      "removable": true
    }
  ],
  "summary": {
    "deadTypes": 1,
    "deadMembers": 1,
    "testOnlyTypes": 0,
    "deadProjects": 0,
    "suppressedReflectionProjects": 0,
    "total": 2,
    "typesAnalyzed": 5
  }
}
```

Pair it with `--report <dir>` to also write the JSON to disk, or scope a run to a subset of
projects with `--project <name>` / `--exclude-project <name>` (same shape as `affected`'s
project-scoping flags).

## Exit codes

- **`0`** — clean report, or findings exist but `--fail-on-dead` wasn't passed (the default).
- **`1`** — `--fail-on-dead` was passed and at least one `dead`-category finding (`DC0001`,
  `DC0002`, `DC0005`, `DC0006`) exists. Test-only findings alone never trigger this.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

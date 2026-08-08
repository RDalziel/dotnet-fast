# Dead dependencies

`dotnet-fast dead-dependencies` (alias `dead-deps`) finds dependencies your solution declares but
doesn't use — direct `PackageReference`s and `ProjectReference`s that nothing in the referencing
project touches — plus a handful of MSBuild-side smells (orphaned central `PackageVersion`s, duplicate
references, redundant `VersionOverride`s). It's build-free by default (no MSBuild, no restore, no
Roslyn) and conservative by contract: **everything it reports is safe to remove.** Any ambiguity it
can't resolve — a package whose public surface it can't fully see, a reference shape that's invisible
to a source scan, reflection it can't follow, a file it can't parse — keeps the reference instead of
risking a false positive.

## Quick start

```bash
dotnet-fast dead-dependencies .                    # human report (report-only, exits 0)
dotnet-fast dead-dependencies . --format json      # { findings: [...], summary: {...} }
dotnet-fast dead-dependencies . --no-info          # only the unused tier (DD0001/DD0002/DD0007/DD0009)
dotnet-fast dead-dependencies . --fail-on-unused   # exit 1 when an unused dep is found (CI gate)
dotnet-fast dead-dependencies . --keep "Acme.*"    # ad-hoc known-keep (repeatable)
dotnet-fast dead-dependencies . --fix              # DRY-RUN: preview the removal diff, write nothing
dotnet-fast dead-dependencies . --fix --write      # apply the surgical csproj/props removals
dotnet-fast dead-dependencies . --verify           # opt-in: build each candidate removal first
dotnet-fast dead-dependencies . --format sarif      # SARIF 2.1.0 log for code scanning
dotnet-fast dead-dependencies . --baseline base.json --fail-on-unused  # gate only new findings
dotnet-fast dead-dependencies . --agent            # terse, low-token report for AI agents
```

A clean run against a small solution looks like this:

```
src/App/App.csproj:9: DD0001 unused package newtonsoft.json (project App)
dead-dependencies: 1 finding(s) — 1 unused package(s), 0 unused project ref(s), 0 orphaned central version(s), 0 duplicate(s), 0 redundant override(s).
```

## What it finds

| ID | Category | Meaning |
|---|---|---|
| `DD0001` | unused | a direct `PackageReference` that is provably unused — safe to remove |
| `DD0002` | unused | a `ProjectReference` whose transitive closure contributes nothing the project uses |
| `DD0003` | info | an orphaned central `PackageVersion` (in `Directory.Packages.props`, referenced by no project) |
| `DD0004` | info | a duplicate package/project reference in one project |
| `DD0005` | info | a used direct reference that's fully satisfied transitively (removing it un-pins the version — report-only) |
| `DD0006` | info | a `VersionOverride` identical to the central version (redundant) |
| `DD0007` | unused | a `PackageReference` declared in a shared import (`Directory.Build.props`/`.targets`, or an `<Import>`) that every importing project proves unused — safe to remove from the shared file |
| `DD0008` | info | an orphaned `GlobalPackageReference` — a package pinned solution-wide in `Directory.Packages.props` that behaves like a library (real compile assets, assembly-verified), yet no project anywhere uses it |
| `DD0009` | unused | a reference whose `'$(TargetFramework)'=='netX'` condition can never match the project's declared target frameworks — the classic leftover after a framework was dropped; it never takes part in any build, so it's safe to remove even if old `#if` code still mentions it |

There's no "maybe unused" bucket. A reference is either proven unused (reported) or it stays silent —
ambiguity always resolves to "keep," never to a finding.

### Orphaned `GlobalPackageReference` (`DD0008`)

A `GlobalPackageReference` is conventionally build tooling (SourceLink, analyzers, MinVer, …), added by
MSBuild to every project, so "no `PackageReference` names it" doesn't mean anything for it — that check
(`DD0003`) skips it entirely. `DD0008` checks something narrower and meaningful instead: is it a
*library* (assembly-verified real compile assets, not a curated/heuristic build-tooling id) that no
project anywhere actually references? Both conditions must hold, and the evidence bar is the highest in
the tool — only real assembly metadata counts, never the curated namespace table or the id-prefix
heuristic. It's report-only for now.

### Dead framework conditions (`DD0009`)

When a project drops a target framework (say `net472`) but a `Condition="'$(TargetFramework)'=='net472'"`
reference lingers, no build can ever activate that reference — it's removable by MSBuild semantics
alone, even though the leftover `#if NET472` code still *mentions* the package (which is exactly why
the usage-based checks can't catch this one). The rule only fires when the project declares its
target frameworks literally in its own file and the condition is an `==` that matches none of them;
anything indirect stays kept. If the same package is declared twice — once live, once behind a dead
condition — only the dead line is reported and removed.

### `ProjectReference` closure and `PrivateAssets="all"`

A project with its own consumers normally keeps every outgoing `ProjectReference`, since removing one
could strand whatever a downstream consumer reaches through it — except an edge declared
`PrivateAssets="all"`, which never flows to that project's own consumers in the first place, so it's
evaluated for `DD0002` like a leaf project's reference instead.

### Shared imports (`DD0007`)

A `PackageReference` declared in `Directory.Build.props`, `Directory.Build.targets`, or a file reached
via `<Import Project=…>` is reportable when **every** project that imports the file proves it unused —
the same evidence rules as a per-project reference, applied to each importer. It only fires when the
importer set is provably complete, which needs the shared file to live inside the directory tree you
scanned: MSBuild always resolves `Directory.Build.props`/`.targets` to the file *nearest* a project,
walking upward, so a file above the scanned root could also be picked up by a sibling project outside
what was scanned — invisibly. In that case (or under a `--project`/`--exclude-project` filter, which
has the same blind spot), the finding is withheld rather than guessed. `ProjectReference` declared
outside a project's own csproj is still never reasoned about — its relative path resolves ambiguously
across importers at different depths.

## The conservative-by-contract guarantee

A package is only ever called unused when the tool holds a **complete** inventory of its public
surface *and* finds zero evidence it's used. That evidence includes `using` directives (including
`global using`, `using static`, csproj `<Using>` items, and SDK implicit usings), fully-qualified
names, reflection-shaped string literals, tokens in non-C# content files (`.razor`/`.cshtml`/`.xaml`/
config), and the extension-method soundness rule (an extension method needs its namespace in scope, so
a package whose namespace is in scope anywhere is kept). A few consequences of the "prove it or keep
it" rule:

- **Analyzers, source generators, and build/test tooling are kept** — a curated fact table plus
  `.Analyzers`/`.SourceGenerator`/… suffix heuristics recognize packages whose whole job is invisible
  to a source scan. Heuristics may only *keep* a package, never condemn one.
- **A known source generator taints its own project.** A generator can emit code — into a
  compiler-generated file this tool never scans — that references a *different* declared package with
  zero trace in your own source. A project referencing a `.SourceGenerator`/`.SourceGenerators`-named
  package (a narrower signal than the general analyzer suffix, which also matches diagnostic-only
  analyzers like StyleCop) keeps every one of its other packages silently, the same way reflection
  taints a project.
- **Compile-invisible reference shapes are kept**: `IncludeAssets`/`ExcludeAssets` that strip
  `compile`, `GeneratePathProperty="true"`, `OutputItemType`, `ReferenceOutputAssembly="false"`,
  `Aliases=` (extern-alias routing), and any `Condition` that isn't a plain target-framework check.
- **Non-literal reflection taints a project** — a `Assembly.Load(name)` or `Type.GetType(expr)` with a
  computed argument could name any type, so every package verdict in that project is suppressed.
- **A non-SDK project, a legacy project using `packages.config`, an unparseable project, or one with
  custom MSBuild `<Target>`/`<UsingTask>` logic is skipped whole** and counted in `projectsSkipped`;
  `--format json` names each one's specific reason in an additive `skippedProjects` array.
- **A package with no complete inventory** (not in the curated table, no restore output to read, no
  user-supplied namespaces) is never reported — the id-as-namespace-prefix heuristic can prove a
  package *used*, but never *unused*.

The asymmetry is deliberate: a false positive here means someone deletes a dependency that was actually
load-bearing — a real build break. A false negative just means a missed cleanup. Every rule leans
toward the second, cheaper mistake. The summary makes the conservatism visible: `unknownKept` counts
references kept because the evidence was incomplete, and `projectsSkipped` counts ineligible projects.

## Removing dead dependencies (`--fix`)

`--fix` is **dry-run by default** — it prints a unified diff of the exact csproj/props removals and
changes nothing on disk:

```
--- a/src/App/App.csproj
+++ b/src/App/App.csproj
@@ -6,7 +6,4 @@
   </PropertyGroup>
-  <ItemGroup>
-    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
-  </ItemGroup>
 </Project>
dead-dependencies --fix (dry-run): 1 reference(s) across 1 file(s) would be removed. Re-run with --write to apply.
```

Add `--write` to apply it. The edit is byte-level — line endings, BOM, and comments on surviving
references are preserved exactly; a comment documenting a removed reference travels with it, and an
`<ItemGroup>` left empty is removed whole. Only removable findings are touched: `DD0001`/`DD0002` in
the project's own csproj, `DD0003` (the orphaned central version), `DD0004` (the duplicate), `DD0006`
(a redundant `VersionOverride` — an attribute-only splice that leaves the rest of the element
untouched), and `DD0007` (the shared-import reference — cut once from the shared file, with every
affected project listed in the diff/report). `DD0005` and `DD0008` are report-only. Under central
package management, removing the last use of a
centrally-versioned package cuts its now-orphaned `PackageVersion` in the same edit — but only when the
whole solution was scanned, so a package a filtered-out project still uses is never orphaned.

## Build-verify lane (`--verify`)

`dotnet-fast` is otherwise build-free, but `--verify` is an **opt-in** lane that actually compiles the
result to prove a removal is safe. It copies the workspace to a throwaway directory, applies the
candidate removals, and runs `dotnet build` on the affected projects (in dependency order). Each
finding then carries a `verified` verdict — `true` only if removing it kept every affected project
compiling. If a build breaks with all removals applied, the lane bisects the project's candidates to
isolate the safe subset. With `--fix --write --verify`, **only verified removals are written**, so the
change you apply is one the compiler already accepted.

```bash
dotnet-fast dead-dependencies . --fix --write --verify
```

Add `--verify-tests` (implies `--verify`) to also run the affected test projects after the build
phase: a removal that compiles but breaks a test — for example, a package a test reaches only
through reflection — is demoted to `verified: false` and never written. Only test projects whose
dependency chain touches a changed project are run.

If verification can't restore packages (air-gapped or source-less environments), the run reports a
single clear restore error and falls back to the plain unverified report — findings are still
listed, just without `verified` verdicts.

## CI patterns

`dead-dependencies` is report-only by default — a clean report and a report full of findings both exit
`0`, because this is a discovery tool, not a gate, until you opt in. Add `--fail-on-unused` to turn an
unused finding into a CI failure:

```bash
dotnet-fast dead-dependencies . --fail-on-unused
```

Info-tier findings (`DD0003`–`DD0006`, `DD0008`) never flip the exit code on their own, even under
`--fail-on-unused`. For automation, `--format json` gives a structured report:

```json
{
  "findings": [
    {
      "id": "DD0001",
      "category": "unused",
      "kind": "package",
      "subject": "newtonsoft.json",
      "project": "App",
      "file": "src/App/App.csproj",
      "line": 9,
      "removable": true,
      "evidence": "no namespace or type of Newtonsoft.Json (curated inventory) is referenced in this project"
    }
  ],
  "summary": {
    "total": 1,
    "unusedPackages": 1,
    "unusedProjectRefs": 0,
    "orphanedCentral": 0,
    "duplicates": 0,
    "transitiveInfo": 0,
    "redundantOverrides": 0,
    "unknownKept": 4,
    "projectsAnalyzed": 3,
    "projectsSkipped": 0,
    "skippedProjects": []
  }
}
```

Scope a run with `--exclude-project <name>` (same shape as `affected`'s project-scoping flags), or
extend the built-in fact tables in `dotnet-fast.json`:

```json
{
  "deadDependencies": {
    "knownKeep": ["MyCompany.BuildTools", "Acme.*"],
    "packageNamespaces": [
      { "package": "MyCompany.Core", "namespaces": ["MyCompany.Core"], "complete": true },
      {
        "package": "MyCompany.Extensions",
        "namespaces": ["MyCompany.Extensions"],
        "complete": true,
        "extensionHeavy": true
      }
    ]
  }
}
```

`complete: true` (the default) asserts you've listed *every* namespace the package's surface lives
under — the precondition for it ever being provable as unused. Set `extensionHeavy: true` for a
package whose surface is mostly extension methods (like `MyCompany.Extensions` above): the tool then
keeps it whenever one of its namespaces is merely in scope anywhere, since an extension method needs
its namespace visible to be callable at all.

### SARIF for code scanning

`--format sarif` emits a SARIF 2.1.0 log — one rule per `DD` id (with a description and help link) and
one result per finding, `unused`-tier findings at `warning`, `info`-tier at `note`. It shares the
`dotnet-fast` driver identity with the `lint`/`format`/`affected` SARIF output, so uploading it to
GitHub code scanning annotates PRs alongside the rest of the toolchain's findings:

```bash
dotnet-fast dead-dependencies . --format sarif > dead-deps.sarif
```

### `--baseline`: gate a repo that already has a backlog

`--baseline <file>` takes a previous `--format json` report and hides any finding that's already in it;
only genuinely *new* findings are reported, and only they count toward `--fail-on-unused`. That makes
turning the gate on non-disruptive for a repo with existing unused dependencies:

```bash
dotnet-fast dead-dependencies . --format json > baseline.json   # snapshot today
dotnet-fast dead-dependencies . --baseline baseline.json --fail-on-unused   # CI: only new findings fail
```

Matching is by `(id, file, package/reference id)` — not by line number, since a dependency
declaration's line shifts on any unrelated edit to the same file. `--baseline` can't be combined with
`--fix`.

## Exit codes

- **`0`** — clean report, or findings exist but `--fail-on-unused` wasn't passed (the default).
- **`1`** — `--fail-on-unused` was passed and at least one `unused`-category finding (`DD0001`,
  `DD0002`, `DD0007`) exists. Info-tier findings alone never trigger this.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

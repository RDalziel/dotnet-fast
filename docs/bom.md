# Bill of materials

`dotnet-fast bom` generates a Software Bill of Materials for a project or a solution: every direct and
transitive `PackageReference` plus every `ProjectReference`, with purls, content hashes (when the
source tier carries them), and a full dependency graph. It's **build-free by default** — pure text
parsing of `packages.lock.json` / `obj/project.assets.json`, no MSBuild, no Roslyn, and no `dotnet
restore` shelled out — unless you opt in with `--restore` (see below).

Build-free has a precise meaning: a project needs **either** a committed `packages.lock.json`
**or** a previously restored `obj/project.assets.json`. A project with neither is **skipped with a
stated reason** — never silently half-reported — and `--restore` is the one opt-in that runs
`dotnet restore` to fill the gap. The document describes components and their relationships
(purls, hashes where the source tier carries them, the dependency graph); it is **not a
vulnerability scan** — feed it to your SCA tooling for that.

One neutral component model feeds a pluggable set of serializers, so the same document can come out
in any of these (format, spec version, encoding) combinations:

| `--format` | `--spec-version` | `--output-format` |
|---|---|---|
| `cyclonedx` (default) | `1.4` · `1.5` · `1.6` (default) | `json` (default) · `xml` |
| `spdx` | `2.2` · `2.3` (default) | `json` |

An unsupported combination — e.g. `spdx` with `--output-format xml`, or `cyclonedx` with
`--spec-version 2.2` — fails with a clear error listing exactly what IS supported, never a silent
fallback to whichever serializer happens to be newest.

## Quick start

```bash
dotnet-fast bom App/App.csproj                         # single project, writes App/bom.json
dotnet-fast bom App/App.csproj --output sbom.json       # write to an explicit path
dotnet-fast bom App/App.csproj --json                   # machine summary (counts + output path) on stdout
dotnet-fast bom .                                       # solution-level: merges every discovered project
dotnet-fast bom App.sln --project Core --project Api    # scope a solution run to specific projects
dotnet-fast bom App.slnf                                # scoped to the filter's own project set
```

A clean single-project run looks like this:

```
bom: wrote 12 component(s) — 3 direct package(s), 9 transitive package(s), 1 project reference(s) — to App/bom.json (App)
```

A solution run reports the project count too, and calls out any project it had to skip:

```
bom: wrote 41 component(s) — 9 direct package(s), 30 transitive package(s), 2 project reference(s) — to bom.json (App, 3 project(s))
bom: 1 project(s) skipped (no lock file or restore output):
  - Tools: Tools/Tools.csproj
```

## Single project vs. solution

- **A `.csproj` target** reports that project alone, and requires a `packages.lock.json` next to it.
- **A directory, `.sln`, `.slnx`, or `.slnf` target** resolves the whole project set (`.slnf` scopes to
  its own filtered set; `--project` narrows further) and merges every project's graph into one document.

## Two source tiers, per project

| Tier | Source | Content hashes | Used when |
|---|---|---|---|
| `lock` | `packages.lock.json` | Yes (SHA-512) | Preferred whenever present |
| `assets` | `obj/project.assets.json` | **No** — left empty, a documented fidelity gap | No lock file, but the project has been restored |
| `skipped` | — | — | Neither file exists — the project still appears in the document with a stated skip reason, never a silent gap |

### Requires a lock file (single-project path) or a prior restore (solution path) — or pass `--restore`

Enable a lock file once with:

```xml
<PropertyGroup>
  <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
</PropertyGroup>
```

...then `dotnet restore`. Without a lock file and without `--restore`, the single-project path fails
with a clear message instead of either shelling out to restore itself or guessing the transitive graph
from the `.csproj` alone — a `.csproj` only ever lists *direct* references, so guessing would silently
omit every transitive dependency, which for a BOM is a correctness lie, not just an incomplete report.
The solution path is more forgiving: it falls back to a restored `obj/project.assets.json` per project,
and only skips (with a stated reason) a project with neither file — the document always says its own
coverage rather than reporting a silent partial graph.

### `--restore`: for an unrestored checkout

Pass `--restore` and `bom` will shell `dotnet restore` for any project that has neither a lock file nor
a restored `obj/project.assets.json`, then retry those tiers:

```bash
dotnet-fast bom App/App.csproj --restore   # single project: assets tier first, else `dotnet restore`
dotnet-fast bom . --restore                # solution: restore is invoked once per project that needs it
```

- **Per-project, not once at the solution root** — a solution-root `dotnet restore App.sln` is one
  MSBuild invocation covering every project, so one project's failure would fail the whole command with
  one combined log, impossible to attribute cleanly. Restoring per project means a failure is always
  traceable to exactly one project, at the cost of a little redundant MSBuild-evaluation overhead (NuGet's
  own global packages cache means only the first download in a run actually hits the network).
- **A hung restore is killed, not left to block forever** — an interactive credential prompt or an
  unreachable/misconfigured feed are the two ways `dotnet restore` is known to hang without ever
  producing output. `bom --restore` kills it after a bounded timeout and reports a clear error.
- **A restore failure never aborts the whole document** — on a solution run, a project whose restore
  fails is recorded as `skipped`, with the restore failure itself as the stated reason; the rest of the
  solution is still reported. On a single-project run there's nothing else to report, so a restore
  failure is a hard error, same as every other unresolvable-tier failure in that path.

## What's in the document

Every target-framework section a source tier exposes is read and unioned into one component graph:

- A package resolved to the **same version under every TFM** collapses to a single component, noting
  every TFM it participates in.
- A package that resolves to **different versions across TFMs** becomes distinct components instead of
  silently picking one and dropping the other — a real multi-targeting divergence is represented
  honestly, not averaged away.
- Direct vs. transitive comes from the lock file's own `"type"` field, or — on the assets tier — from
  the project's own declared `PackageReference`/`ProjectReference` items vs. the rest of the resolved
  graph.
- A `ProjectReference` becomes a component too — `type: "application"`, no `purl` (there's no canonical
  purl type for an in-repo project reference).
- The dependency graph comes from each entry's own dependency edges. A raw dependency name with no
  version attached that matches more than one resolved version (the multi-TFM-divergence case above, or
  — on a solution run — two projects resolving the same id differently) fans out to every matching
  component rather than guessing one.

**On a solution run**, the same rule extends across projects: the same (id, version) contributed by more
than one project dedups to a single top-level component — carrying one `dotnet-fast:project` property
per contributing project as provenance — while the same id at genuinely different versions across
projects stays distinct.

Every `PackageReference` component gets a `pkg:nuget/Name@Version` purl and `type: "library"`. A
`ProjectReference` component deliberately gets **no** purl — there's no canonical purl type for an
in-repo project, and inventing one (e.g. `pkg:generic`) would assert a resolvable identity the
component doesn't have; its `bom-ref` (`project:<name>`) is enough for the document's own dependency
graph. The NuGet-level direct/transitive distinction is preserved in each component's `properties`
(`dotnet-fast:scope`) regardless of CycloneDX `scope`.

### Dev-dependency scoping

A `PackageReference` with `PrivateAssets="all"` (NuGet's own umbrella marker for build-time-only
tooling — analyzers, source generators, `SourceLink`, and similar packages that never flow into the
built output) is scoped as a dev dependency instead of an ordinary runtime one:

| | Runtime dependency (default) | `PrivateAssets="all"` (dev) |
|---|---|---|
| CycloneDX `scope` | `required` | `excluded` — the spec's own wording: "components... not reachable within a call graph at runtime" |
| SPDX relationship | `DEPENDS_ON` | `DEV_DEPENDENCY_OF` (SPDX 2.2 Annex I Table 68 — supported unchanged on 2.3 too) |

Detection reads the project's own `.csproj` directly: neither `packages.lock.json` nor
`project.assets.json`'s resolved-graph section carries `PrivateAssets`, so the `.csproj`'s
`PackageReference` items are the one place this fact is always declared. On a solution run, a package
shared by more than one project is only scoped dev when **every** contributing project declares it
`PrivateAssets="all"` — a package that's a real runtime dependency in even one project is not honestly
"excluded" at solution scope.

Dev dependencies are **included** by default (scoped as above); pass `--exclude-dev` to omit them from
the document entirely:

```bash
dotnet-fast bom App/App.csproj --exclude-dev
```

## Solution document shape

`metadata.component` is the solution/directory itself, with every discovered project nested under its
own `components[]` — CycloneDX 1.5+'s recursive component composition, the spec-native way to express
"this is made of these sub-projects." Each project subcomponent carries a `dotnet-fast:tier` property
(`lock`/`assets`/`skipped`) and, when skipped, a `dotnet-fast:skip-reason`. The top-level `components[]`
stays flat and deduped — what `dependencies[]` edges point at — and the graph fans out from the solution
to one entry per non-skipped project, then from each project to its own direct dependencies, then the
usual per-component edges.

## Reproducible `serialNumber`

CycloneDX's `serialNumber` is normally a random UUID, which makes every run of a BOM generator look
like a brand-new, unrelated document to a diff tool — useless for tracking whether a dependency
actually changed. `dotnet-fast bom` instead derives `serialNumber` from the document's own content
(everything except the timestamp), using a UUID version 8 (RFC 9562 §5.8 — reserved for
implementation-specific, hash-based UUIDs, so it isn't falsely claiming v4 randomness or v5 provenance
it doesn't have). Re-running `bom` against an unchanged project or solution produces a byte-identical
`serialNumber`; a real dependency change produces a different one.

## Other formats, spec versions, and encodings

```bash
dotnet-fast bom App/App.csproj --format cyclonedx --spec-version 1.4 --output-format xml
dotnet-fast bom App/App.csproj --format spdx --spec-version 2.2
```

`--format`, `--spec-version`, and `--output-format` default to `cyclonedx`/`1.6`/`json` — an existing
zero-flag invocation's output is unchanged. `--spec-version`'s legal values (and default) depend on
`--format`: CycloneDX accepts `1.4`/`1.5`/`1.6`, SPDX accepts `2.2`/`2.3`. The two spec versions differ
in real, cited ways, not just the version string:

- **CycloneDX `1.4` vs. `1.5`/`1.6`**: `metadata.tools` is CycloneDX 1.4's legacy flat array-of-`tool`
  shape (the only shape its schema defines); 1.5 introduced (and 1.6 keeps) the modern
  `tools.components[]` shape. `dotnet-fast bom` emits whichever shape the requested version actually
  defines — never a field a version's own schema doesn't have.
- **SPDX `2.2` vs. `2.3`**: `primaryPackagePurpose` (`APPLICATION`/`LIBRARY`) is a field SPDX 2.3 added
  to its Package Information clause that 2.2 doesn't define; a `2.2` document never carries it.

Requesting a combination outside the matrix above — `spdx` with `--output-format xml`, or `cyclonedx`
with `--spec-version 2.2` — is a clear error listing exactly what IS supported for the part of the
request that didn't match.

## `--json`: machine-readable summary

The BOM document always goes to `--output` (default `bom.json`); `--json` only changes what's printed to
stdout. Single project:

```json
{
  "output": "App/bom.json",
  "format": "cyclonedx",
  "specVersion": "1.6",
  "outputFormat": "json",
  "project": "App",
  "summary": {
    "total": 12,
    "directPackages": 3,
    "transitivePackages": 9,
    "projectReferences": 1,
    "frameworks": ["net8.0"]
  }
}
```

Solution — adds a `solution` name and a `projects[]` array with each project's tier (and skip reason,
when skipped):

```json
{
  "output": "bom.json",
  "format": "cyclonedx",
  "specVersion": "1.6",
  "outputFormat": "json",
  "solution": "App",
  "summary": { "total": 41, "directPackages": 9, "transitivePackages": 30, "projectReferences": 2, "frameworks": ["net8.0"] },
  "projects": [
    { "name": "Core", "path": "Core/Core.csproj", "tier": "lock", "skipReason": null },
    { "name": "Tools", "path": "Tools/Tools.csproj", "tier": "skipped", "skipReason": "no packages.lock.json and no obj/project.assets.json found under Tools — enable RestorePackagesWithLockFile and run `dotnet restore`, or otherwise restore the project, to include it in this BOM" }
  ]
}
```

## Roadmap

Shipped so far: the lock-file and `project.assets.json` tiers, the neutral component model, solution-
level merging with per-project provenance and `.slnf`/`--project` scoping, pluggable serializers —
CycloneDX 1.4/1.5/1.6 in both JSON and XML, and SPDX 2.2/2.3 JSON — the `--restore` tier for an
unrestored checkout, and dev-dependency scoping (`--exclude-dev`). Planned next:

- **SPDX tag-value** — the JSON-only SPDX serializer's plain-text sibling format.
- **License data** — neither the lock nor assets tier carries license information, so `bom` doesn't
  invent any (`licenseConcluded`/`licenseDeclared` stay `NOASSERTION` on SPDX; CycloneDX omits
  `licenses` entirely). Real license data would mean reading each package's `.nuspec` out of the global
  packages folder — a real fourth data source this crate doesn't touch today, not a gap in what the
  lock/assets tiers themselves carry.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

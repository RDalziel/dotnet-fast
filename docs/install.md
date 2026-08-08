# Install & first run

This page was written from an actual install-and-run pass of `RDLL.dotnet-fast` 1.0.0 on Windows x64
— install, first run, verify, update, uninstall. Every output below is transcribed from that pass, with
the sample working directory renamed to `C:\src\hello`. Nothing here is illustrative.

## Prerequisites

| | Requirement |
|---|---|
| OS / architecture | **Windows x64.** Nothing else works — see [support-matrix.md](support-matrix.md#platform). |
| To install | **.NET 10 SDK.** Verified on `10.0.110` and `10.0.302`. |
| To run | The .NET 10 runtime that comes with that SDK. |
| For `lint --deep` only | A restored project (`dotnet restore` or a prior build). |

`dotnet-fast` itself is a native Rust binary with no managed dependencies; the NuGet package is a
portable .NET tool (`tools/net10.0/any/`) whose managed entry point launches that binary. That
`net10.0` folder is why the .NET 10 SDK is a hard floor, not a recommendation.

**On .NET 9 the install fails, and the error does not say why.** It also fails *differently* depending
on which install form you used. Under an SDK 9 pin (a `global.json` selecting `9.0.316`), a global or
`--tool-path` install gives:

```
Tool 'rdll.dotnet-fast' failed to update due to the following:
The settings file in the tool's NuGet package is invalid: Settings file 'DotnetToolSettings.xml' was not found in the package.
Tool 'rdll.dotnet-fast' failed to install. Contact the tool author for assistance.
```

while a local-manifest install (`dotnet tool install RDLL.dotnet-fast`) gives only:

```
Unhandled exception: Settings file 'DotnetToolSettings.xml' was not found in the package.
```

Both mean the same thing: "this SDK found no tools folder it understands", i.e. no .NET 10. The
`DotnetToolSettings.xml` phrase is the part common to both — that is the string to search for. Check
`dotnet --version` in the directory you are installing from — a repository `global.json` can pin you
to an older SDK without you noticing.

## Install into a repository (local tool manifest)

The recommended shape for a repository: the version is pinned in a file you commit, so every machine
and every CI agent gets the same one.

```bash
dotnet new tool-manifest          # only if the repo has no manifest yet
dotnet tool install RDLL.dotnet-fast
```

```
You can invoke the tool from this directory using the following commands: 'dotnet tool run dotnet-fast' or 'dotnet dotnet-fast'.
Tool 'rdll.dotnet-fast' (version '1.0.0') was successfully installed. Entry is added to the manifest file C:\src\hello\dotnet-tools.json.
```

Where the manifest lands is the SDK's choice, not ours: **.NET 10 writes `dotnet-tools.json` in the
current directory; .NET 9 wrote `.config/dotnet-tools.json`.** Both are found automatically, by the
SDK and by `dotnet-fast`'s own version banner, which probes `.config/dotnet-tools.json` first and then
the bare file at each level up the tree. Commit whichever one you get.

The manifest entry looks like this:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "rdll.dotnet-fast": {
      "version": "1.0.0",
      "commands": [
        "dotnet-fast"
      ],
      "rollForward": false
    }
  }
}
```

On a fresh clone or a CI agent, `dotnet tool restore` installs exactly that version. Invoke it as
`dotnet dotnet-fast …` (or `dotnet tool run dotnet-fast …`).

## Install for the whole machine (global tool)

```bash
dotnet tool install -g RDLL.dotnet-fast
```

This puts `dotnet-fast` on your `PATH` and it is then invoked as `dotnet-fast …`, with no `dotnet`
prefix. Use `--tool-path <dir>` instead of `-g` to install into a directory you choose — useful on a CI
agent you want to keep self-contained, or to try a version without touching your machine-wide tools:

```bash
dotnet tool install RDLL.dotnet-fast --tool-path C:\tools\dotnet-fast
```

```
You can invoke the tool using the following command: dotnet-fast
Tool 'rdll.dotnet-fast' (version '1.0.0') was successfully installed.
```

A `--tool-path` (or global) install is a real executable, so it is **not** affected by a repository
`global.json` SDK pin — running it from a directory pinned to SDK 9 still works, because it resolves
the .NET 10 runtime directly rather than going through the SDK's local-tool runner.

To list what you have:

```bash
dotnet tool list                                 # local manifest
dotnet tool list --tool-path C:\tools\dotnet-fast
```

```
Package Id            Version      Commands         Manifest
------------------------------------------------------------------------------
rdll.dotnet-fast      1.0.0        dotnet-fast      C:\src\hello\dotnet-tools.json
```

## First run

The commands below use the local-manifest form (`dotnet dotnet-fast`). For a global install, drop the
leading `dotnet`.

### 1. Confirm the version

```bash
dotnet dotnet-fast --version
```

```
1.0.0
```

Every other command prints a one-line banner to **stderr** before it does anything, naming the version
and — if a tool manifest is in scope — whether the running binary matches the version that manifest
pins:

```
dotnet-fast 1.0.0 (matches pin in C:\src\hello\dotnet-tools.json)
```

It goes to **stderr**, not stdout, so it never contaminates `--json` output or a piped report.

### 2. See what it can find

```bash
dotnet dotnet-fast lint --list-rules
```

```
836 native CST lint rules (DF0001-SYSLIB1045; 144 fixable):

Correctness (132):
  DF0001  [report-only]  Empty catch block swallows exceptions.
  DF0013  [fix:suggestion]  Rethrow resets the stack trace.
  DF0018  [report-only]  Self-assignment.
  DF0024  [report-only]  Assignment in an `if` condition.
  …
```

`--list-rules --json` gives the same list machine-readably. These are the **native**, syntax-only rules
— no project load, no restore, no MSBuild. `--deep` is a separate mode (step 5).

### 3. Lint a project

Given a minimal `HelloApp/HelloApp.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

and a deliberately messy `HelloApp/Program.cs`:

```csharp
namespace HelloApp;

public static class Program
{
    public static void Main()
    {
            var name  = "world";
        try
        {
            System.Console.WriteLine($"hello {name}");
        }
        catch (System.Exception)
        {
        }
    }
}
```

```bash
dotnet dotnet-fast lint HelloApp/HelloApp.csproj
```

```
HelloApp\Program.cs:3:21: SA1600 Elements should be documented
HelloApp\Program.cs:5:24: SA1600 Elements should be documented
HelloApp\Program.cs:7:9: WHITESPACE Fix whitespace formatting. Delete 4 characters.
HelloApp\Program.cs:7:13: RCS1118 Mark local variable as const
HelloApp\Program.cs:7:21: SA1025 Code should not contain multiple whitespace characters in a row.
HelloApp\Program.cs:7:22: WHITESPACE Fix whitespace formatting. Delete 1 characters.
HelloApp\Program.cs:8:1: SA1137 Elements should have the same indentation.
HelloApp\Program.cs:12:9: CA1031 Do not catch general exception types
HelloApp\Program.cs:12:9: DF0001 Empty catch block swallows exceptions.
HelloApp\Program.cs:12:9: RCS1075 Avoid empty catch clause that catches System.Exception
HelloApp\Program.cs:12:9: S2486 Handle the exception or explain in a comment why it can be ignored.
HelloApp\Program.cs:12:16: S2221 Catch a list of specific exception subtype or use exception filters instead.
HelloApp\Program.cs:13:9: PH2098 Avoid empty catch blocks
HelloApp\Program.cs:13:9: S108 Either remove or fill this block of code.

Found 14 diagnostics in 1 file (2 fixable):
     2  SA1600      2  WHITESPACE      1  CA1031      1  DF0001      1  PH2098      1  RCS1075      1  RCS1118      1  S108      1  S2221      1  S2486      1  SA1025      1  SA1137
  Correctness 6   Maintainability 3   Style 3
Run with --fix to apply fixable diagnostics.
```

**`lint` exits `1` when it finds anything** — that is what makes it usable as a CI gate with no extra
scripting. It changes no source file.

### 4. Apply the safe fixes

```bash
dotnet dotnet-fast lint --fix HelloApp/HelloApp.csproj
```

This is **silent on success and exits `0`** — there is no summary unless you ask for one. Add
`-v normal` if you want to see what it touched:

```
Formatted code file 'C:\src\hello\HelloApp\Program.cs'.
checked 1 file(s), cached 0, changed 1, would change 0
```

Re-running `lint` now reports 10 diagnostics instead of 14: the whitespace and indentation findings are
gone, and what remains is the set that needs a human decision (an empty `catch`, missing XML docs, a
local that could be `const`). `--fix` changes only what it reports, and only the findings marked
fixable — see [commands.md](commands.md) for the full `lint` surface and
[deep-linting.md](deep-linting.md) for how the rule set is composed.

### 5. Optional: run your project's real Roslyn analyzers

```bash
dotnet restore HelloApp/HelloApp.csproj
dotnet dotnet-fast lint --deep HelloApp/HelloApp.csproj
```

stderr shows the depth pass; findings merge into the same report:

```
deep: running Roslyn analyzers over 1 project(s)
deep: [1/1] HelloApp — 271 analyzers, 0 finding(s) in 828 ms
```

The Roslyn sidecar ships **inside** the NuGet package, so `--deep` needs no extra install — but it does
need the .NET SDK and a restored project, and it loads and runs real analyzers.

**Don't read that millisecond figure as a fixed cost.** 828 ms was one cold pass on this project;
repeat runs against a warm analyzer host on the same project measured **27–125 ms**, and an unchanged
project served from cache reports 0 ms. The number swings by an order of magnitude with host warmth and
cache state, so treat it as "`--deep` costs real time even on a trivial project", not as a benchmark.
The figures worth planning against are on a real repository, in
[benchmarks.md](benchmarks.md#where-we-are-not-faster). [deep-linting.md](deep-linting.md) covers when
it is worth turning on.

## Verify what you downloaded

Every package on nuget.org is repository-signed by NuGet.org, and the SDK can check that signature.
Fetch the package and verify it before installing:

```bash
curl -sSL -o RDLL.dotnet-fast.1.0.0.nupkg \
  https://api.nuget.org/v3-flatcontainer/rdll.dotnet-fast/1.0.0/rdll.dotnet-fast.1.0.0.nupkg
dotnet nuget verify RDLL.dotnet-fast.1.0.0.nupkg
```

```
Verifying RDLL.dotnet-fast.1.0.0
Content hash: WhLgD60/cnglAQfwaMXex+mCRO2cI4d1llIB3Ro8fGo6MbXtPomHwFUbjN5P8rDWgluA0DlYFdSA1fnvrWdkIg==

Signature type: Repository
  Subject Name: CN=NuGet.org Repository by Microsoft, O=NuGet.org Repository by Microsoft, …
  SHA256 hash: 1F4B311D9ACC115C8DC8018B5A49E00FCE6DA8E2855F9F014CA6F34570BC482D
  Valid from: 23/02/2024 00:00:00 to 19/05/2027 00:59:59
```

Exit code `0` means the signature checked out. **Be clear about what this proves**: the package is the
one nuget.org served, unmodified in transit or at rest. It is a *repository* signature — it attests to
nuget.org's copy, not to a separate publisher identity. [security.md](security.md) has the full
picture: what each check does and does not prove, how to make your build *require* it, and what is
deliberately not claimed.

## Update

`dotnet-fast update` (new in 0.306.0) works out how your copy was installed and runs the matching
`dotnet tool update` command, so you never have to pick between the manifest form and the global form:

```bash
dotnet dotnet-fast update --check       # report only; changes nothing
dotnet dotnet-fast update --dry-run     # print the exact command; no network needed
dotnet dotnet-fast update --to 0.306.2  # move to an exact version, not the latest
dotnet dotnet-fast update               # do it
```

```
dotnet-fast 1.0.0 is the latest published version.
```

`--dry-run` from a manifest install prints the command it would run, and says up front what it would
touch:

```
dotnet tool update RDLL.dotnet-fast --tool-manifest C:\src\hello\dotnet-tools.json
note: this rewrites C:\src\hello\dotnet-tools.json, which is tracked in most repositories.
      Commit the change deliberately — it pins the version your CI installs.
```

`--to <VERSION>` pins the move to an exact version instead of the latest — the flag is `--to` rather
than `--version` because the latter is the binary's own version flag.

`--exit-code-on-outdated` makes it a pipeline check: non-zero when a newer version exists, and nothing
is modified (it implies `--check`). **`update` is the only command that checks for a new version** —
nothing else phones home to see whether you are current, on any code path.

That is a narrower statement than "the tool makes no network calls", and the difference matters if you
are auditing CI egress: `build` and `cache` talk to the remote build cache (Azure Blob Storage) when
one is configured, which is their entire purpose. `lint`, `format` and `affected` make no network calls
of their own.

The plain SDK commands still work if you prefer them:

```bash
dotnet tool update RDLL.dotnet-fast        # local manifest
dotnet tool update -g RDLL.dotnet-fast     # global
```

See [commands.md](commands.md#update) for the full flag list.

## Uninstall

```bash
dotnet tool uninstall RDLL.dotnet-fast                          # local manifest
dotnet tool uninstall -g RDLL.dotnet-fast                       # global
dotnet tool uninstall RDLL.dotnet-fast --tool-path C:\tools\dotnet-fast
```

```
Tool 'rdll.dotnet-fast' was successfully uninstalled and removed from manifest file C:\src\hello\dotnet-tools.json.
```

A `--tool-path` or global uninstall reports the version instead:

```
Tool 'rdll.dotnet-fast' (version '1.0.0') was successfully uninstalled.
```

Uninstalling removes the tool but leaves the downloaded package in the NuGet cache
(`%USERPROFILE%\.nuget\packages\rdll.dotnet-fast\`). `dotnet nuget locals global-packages --clear`
clears that cache — it is shared with every other package on the machine, so only do it if that is what
you want.

`dotnet-fast` also writes an incremental formatting cache, `.dotnet-format-fast-cache.json`, next to
the project it formatted. Deleting it is safe (the next run just re-does the work), and `--no-cache`
stops it being used at all.

## Troubleshooting

**`Settings file 'DotnetToolSettings.xml' was not found in the package`** — you are on an SDK older
than .NET 10. See [Prerequisites](#prerequisites).

**`dotnet-fast native binary was not found under '<tool directory>'`, exit code 1** — you are on Linux
or macOS. The install succeeds there because NuGet sees a portable .NET tool, but the package carries
a `win-x64` binary only. This is a documented limitation, not a bug:
[support-matrix.md](support-matrix.md#platform).

**The banner reads `dotnet-fast 1.0.0 (manifest pins 1.0.0-rc.1 at …)`** — the binary that ran is not
the version the repository pins. Usually a global install is shadowing the manifest, or
`dotnet tool restore` has not run since the manifest changed. The banner names both versions and the
manifest path, so you can see exactly which two disagree.

**`Could not execute because the specified command or file was not found`** (from
`dotnet dotnet-fast …`) — the SDK found no manifest in scope that declares the `dotnet-fast` command.
Either you are outside the directory tree containing the manifest, or `dotnet tool restore` has not run
in this clone yet.

**`` `restore` was removed in v0.154.0 `` / `` `sbom` was renamed in v0.154.0 ``, exit code 2** — you
invoked a command that no longer exists. Both now name their replacement and exit `2` instead of being
parsed as a path to format:

```
`restore` was removed in v0.154.0. Use the SDK directly:
    dotnet restore
```

```
`sbom` was renamed in v0.154.0. Use:
    dotnet-fast bom
```

A directory genuinely named `restore` is still treated as a target, as before. See
[commands.md](commands.md#removed-commands).

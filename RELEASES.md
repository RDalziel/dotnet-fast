# Release notes

What changed in recent releases, in plain English. Newest first.

The current **stable** line is `1.2.0`. Notes for every `0.x` release are in
[RELEASES-0.x.md](RELEASES-0.x.md).

> **Standing correction (2026-08-03): every whole-repository parity percentage published in an entry
> dated before 2026-08-03 is withdrawn, and none of them should be quoted.** The harness that
> produced them counted files *neither* tool had opened as matches, and it selected Polly's 20-file
> `samples\Samples.slnx` rather than the 764-file `Polly.slnx` — so in particular **every "Polly 100%"
> in these notes is wrong**, at every version it appears. Newtonsoft.Json's 945/945 is separately withdrawn
> as a comparer artifact. The entries are left as they were written, with per-entry notes on the ones
> that carried the headline claims; the only current figures are in the
> [support matrix](docs/support-matrix.md), measured under a harness that verifies both tools processed
> the same files and fails the run when they did not.

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

Notes for every `0.x` release are in **[RELEASES-0.x.md](RELEASES-0.x.md)**.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

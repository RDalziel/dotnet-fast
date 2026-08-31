# Benchmark methodology

Every speed number this project publishes comes from a run described here. The point of this page is
that you can judge what a number means, see where it does *not* hold, and reproduce the shape of the
test on your own repository.

Nothing here is modelled, extrapolated, or rounded up from a different scenario. Where a figure is
old, the date says so. Where the comparison flatters us, this page says that too.

**Currency, stated up front:** the current release is **1.0.0**, and the most recent recorded
benchmark run on this page is **2026-07-17**. Releases since then have been correctness fixes, and
none of them was benchmarked — so every speed figure below is verified as of the date printed next to
it and has not been re-verified against the version you are installing. The parity table at the bottom
*is* current: it was re-measured on 2026-08-03.

## What is being compared

| We run | Against | Version measured |
|---|---|---|
| `dotnet-fast format` / `whitespace` | `dotnet format` (the .NET SDK's built-in formatter) | SDK 10.0.108 / 10.0.109 / 10.0.302 depending on the run — stated per table |
| `dotnet-fast affected` | `dotnet affected` ([`dotnet-affected`](https://github.com/leonardochaia/dotnet-affected)) | 6.2.0 |
| `dotnet-fast lint --deep` | Roslynator CLI `analyze` | 0.12.0 |
| `dotnet-fast lint` | `dotnet format analyzers` | SDK 10.0.108 |

`dotnet-fast` is always a **release** build. The official tools are the published releases named
above, run from the same shell, on the same machine, in the same session.

## The machine

All published numbers were produced on one box:

- **CPU** AMD Ryzen 7 5800X, 8 cores
- **OS** Windows 11 x64 (build 10.0.26200)
- **Timing** [hyperfine](https://github.com/sharkdp/hyperfine) 1.20.0

This is a developer workstation, not a CI agent. Absolute milliseconds will not transfer to your
hardware; the *ratios* are the transferable part, and even those move with the environment — see
[When the environment moves the number](#when-the-environment-moves-the-number).

## What makes a run publishable

A benchmark backs a published claim only when all of these hold:

- **Output is verified before time is measured.** For the corpus runs, both tools format a copy of
  the same tree and the resulting `.cs` files are compared before any timing happens. A tool that is
  fast because it does something different is not faster, it is wrong. Runs without verification are
  marked exploratory and never published.
- **Release build**, pinned and stated versions of the official tools and the .NET SDK.
- **Raw data is kept with the result** — hyperfine's JSON/CSV plus a metadata record (exact commands,
  tool versions, Git commit, OS, CPU, corpus, warm/cold state, verification state). A summary table
  without its metadata record is not a result.
- **A suite, not a favourite scenario.** Headline numbers come from a matrix (cold whitespace,
  default formatting, targeted project, warm/no-op, generated files) across corpus sizes. Tiny
  corpora are used for startup smoke checks only.
- **A regression floor, run deliberately.** A benchmark gate re-runs the medium and large tiers with
  output verification and fails if any scenario drops below 2× versus `dotnet format`. Be clear about
  what this is and isn't: it is a step run on purpose, **not** something every release passes
  automatically. The automated release path runs the correctness gates — build, test suite,
  CLI-contract goldens, formatter parity — and does not run the benchmark suite. The last recorded
  gate run is 2026-07-17 (below); every scenario passed. So a figure here is verified as of the date
  next to it, not re-verified on the day you read it. That is why every table on this page is dated.

## How the comparison is kept fair

**Restore is excluded from every timed command.** Both sides run against an already-restored tree,
and `dotnet format` is invoked with `--no-restore`. This is deliberately *unfavourable* to us:
`dotnet-fast` needs no restore at all for the native paths, so excluding restore removes a cost only
the official tool pays. On MassTransit, `dotnet restore MassTransit.sln` measured **105 s** — a
one-time cost per clean checkout that does not appear in any ratio on this page.

**Cold vs warm is stated, never blended.** Corpus runs are cold: the tree is reset to a pristine copy
before every timed iteration, so neither tool benefits from the previous run's work. Where a number
depends on a warm process — `lint --deep`'s analyzer host is the only one — the table says "warm" and
the cold figure is given alongside it.

**Convergence passes are counted honestly.** On the default-formatting (IDE0007) corpora,
`dotnet-fast` reaches the verified final tree in **one** invocation; the probed SDK behaviour needs
**three** `dotnet format` invocations to converge the same corpus, so the official side of those rows
is the cost of all three passes. Those rows are labelled below. Compare single invocations and the
ratio is roughly a third of the quoted figure. The whitespace and targeted-project rows are strictly
one invocation against one invocation.

**We are not doing the same work.** This is the honest core of every ratio on this page.
`dotnet format` evaluates the real MSBuild project, loads a Roslyn workspace, and builds a full
semantic model. `dotnet-fast`'s native paths do their own filesystem and XML project discovery and
apply text-preserving transforms with no MSBuild and no Roslyn. That is why the margin is largest on
small runs (where the official tool's fixed startup dominates) and smallest on large rewrites (where
both tools are doing real work per file). It also means `dotnet-fast format` implements a **verified
subset** of `dotnet format`'s fixers — see the [support matrix](support-matrix.md) — so on an
arbitrary repository the two do not produce byte-identical output. The corpus runs are constructed so
that they do; the real-repository numbers below are explicitly labelled where they do not.

### The hyperfine `-N` detail

The startup-bound and `affected` comparisons are run with hyperfine's `-N` (no intermediate shell).
On Windows, hyperfine's default is to launch each command through `cmd.exe`, which costs roughly
16 ms of shell startup *per run* — enough to swamp a command whose whole job takes under 20 ms, and
enough to make the ratio meaningless. `-N` makes hyperfine tokenize and launch the command itself.
Both sides are measured shell-free, so the comparison stays symmetric; without it, the sub-20 ms
side is measuring `cmd.exe`, not the tool. It also sidesteps a Windows default-shell quoting bug.

If you reproduce these numbers without `-N`, expect the startup row to collapse.

### When the environment moves the number

`affected` spends most of its wall time spawning `git` subprocesses; its own work (project discovery
plus the reverse-dependency graph walk) is around 10 ms. That makes the ratio sensitive to anything
that slows process creation on the host — on Windows, typically an anti-malware scanner inspecting
`git.exe`.

Every benchmark artifact therefore records a canary: the cost of a bare `git --version`, minimum of
five spawns. On this machine ~10 ms is healthy; above ~25 ms the git-bound ratios are
environment-limited rather than tool-limited. The 2026-07-17 re-baseline was taken with the canary at
**30.2 ms**, roughly 3× the healthy baseline, which is why the 60-project `affected` ratio reads
6.40× there against 15.3× three weeks earlier on the same hardware with the canary at ~10 ms. The
tool's own measured phase timings across those two runs are unchanged; the delta is the environment.
The format numbers (which spawn no `git`) are unaffected. This is recorded rather than smoothed over because it is
exactly the kind of drift that turns a benchmark into folklore.

## Results

### Startup and discovery bound — `format`

A 30-file project that is already formatted, so both tools do the full discovery work and then find
nothing to change. Both tools are run once before timing so that neither is measuring a first-write.
hyperfine `-N`, warmup 2, 12 runs.

| Run | `dotnet-fast format` | `dotnet format --no-restore` | Ratio |
|---|---:|---:|---:|
| 2026-07-17 re-baseline, v0.294.3 | 21.1 ms | 5.05 s | **~240×** |
| 2026-06-28 re-validation, v0.230.0, min of 20 runs | 14.8 ms floor / 17.6 ms mean | not re-timed | — |

The second row is a floor check on our own side only — it says the native binary's startup-bound
cost had not regressed, and carries no ratio of its own.

This is the most flattering row on the page and the noisiest, and it is worth understanding what it
actually says: it is almost entirely the official tool's process start and workspace load versus a
native binary's. It does not say we rewrite files 240× faster. It says that if your CI step formats a
small project, or verifies an already-clean one, you are paying five seconds for nothing.

### Whole-solution work — the suite

Synthetic corpora, output verified, cold tree, `hyperfine` 1.20.0, SDK 10.0.108, measured 2026-06-09.
Warmup 0 and a 2-run minimum — which is why the standard deviation on the short targeted rows is
large; those rows are directional.

| Corpus | Scenario | `dotnet-fast` | `dotnet format` | Ratio |
|---|---|---:|---:|---:|
| medium (8 projects / 1,200 files) | whitespace | 1,019.0 ms ± 197.3 | 9,502.1 ms ± 48.3 | 9.32× |
| medium | targeted project (whitespace) | 104.4 ms ± 43.7 | 3,079.6 ms ± 60.4 | 29.50× |
| medium | default formatting, IDE0007 † | 1,172.2 ms ± 232.3 | 44,443.0 ms ± 163.9 | 37.92× |
| large (8 projects / 2,000 files) | whitespace | 2,075.6 ms ± 250.2 | 15,250.0 ms ± 221.8 | 7.35× |
| large | targeted project (whitespace) | 203.0 ms ± 89.1 | 3,744.2 ms ± 30.2 | 18.44× |
| large | default formatting, IDE0007 † | 2,326.4 ms ± 329.8 | 69,496.0 ms ± 2,821.4 | 29.87× |
| massive (16 projects / 8,000 files) | whitespace | 10,370.2 ms ± 106.6 | 61,237.5 ms ± 410.8 | **5.91×** |
| massive | targeted project (whitespace) | 492.6 ms ± 309.8 | 5,619.8 ms ± 71.4 | 11.41× |
| massive | default formatting, IDE0007 † | 11,790.1 ms ± 171.4 | 182,580.9 ms ± 1,531.7 | 15.49× |

† one `dotnet-fast` invocation versus three `dotnet format` invocations — see
[convergence passes](#how-the-comparison-is-kept-fair).

The benchmark gate re-ran the medium and large tiers on 2026-07-17 under SDK 10.0.302 with output
verification on: whitespace 9.06× / 6.84×, targeted 18.10× / 13.81×, default formatting 36.73× /
30.31×, all above the 2× floor. Same shape, different SDK, five weeks later.

Read the table top to bottom and the trend is the story: **the bigger the rewrite, the smaller the
margin.** Whitespace on the massive corpus is 5.91×, and that is the number to plan a large
solution's CI around — not the 240× startup row.

### `affected`

A synthetic N-project reference chain with a leaf change (`--from HEAD~1 --to HEAD`), after checking
that both tools return the same project set. hyperfine `-N`, warmup 2, 12 runs.

| Run | Chain | `dotnet-fast affected` | `dotnet affected` 6.2.0 | Ratio |
|---|---|---:|---:|---:|
| 2026-07-17, v0.294.3 (git canary 30.2 ms) | 60 projects | 178.9 ms | 1.14 s | 6.40× |
| 2026-06-28, v0.230.0 (git canary ~10 ms) | 60 projects | — | — | 15.3× |
| 2026-06-28, v0.230.0 | 300 projects | 96.2 ms | 2.18 s | 22.7× |

The spread between the first two rows is the environment, not the tool. See
[When the environment moves the number](#when-the-environment-moves-the-number).

### A real repository — MassTransit

Synthetic corpora are repeatable; they are also built by us. The standing real-world comparison is
[MassTransit](https://github.com/MassTransit/MassTransit) — an unmodified third-party solution
(~5,856 C# files, ~70 projects, multi-targeted `net472`/`net8.0`/`net9.0`/`net10.0`, referencing seven
analyzer families) that neither tool was tuned for.

| Lane | `dotnet-fast` | Official | Ratio | Method |
|---|---:|---:|---:|---|
| `affected`, `HEAD~1..HEAD` | 94.3 ms ± 4.4 | `dotnet affected` 6.020 s ± 0.061 | **63.8×** | hyperfine, warmup 1, 5 runs, **identical affected set** |
| `format` whitespace, `--verify-no-changes` | 4.75 s | `dotnet format` 41 s | ~8.6× | single wall-clock run each |
| `format` full (whitespace + style) | 9.6 s | `dotnet format` 94 s | ~9.8× | single wall-clock run each |
| `lint --deep` warm, compact output | 28.96 s | Roslynator `analyze` 85.38 s | ~2.9× | single wall-clock run each |

Dates: the `affected` and `format` rows are 2026-06-06 on a 5,502-file snapshot; the `--deep` and
Roslynator rows are a 2026-06-16 driver pass on the 5,856-file snapshot, SDK 10.0.109.

Three caveats that matter more than the ratios:

- The `format` rows are **not** an output-equivalence comparison. `dotnet-fast format` implements a
  verified subset of `dotnet format`'s fixers, so on an untuned repository the two do not produce
  identical trees. What both tools agree on here is that the repository is *not* fully formatted
  (both exit 2). Treat these as order-of-magnitude evidence, not a parity result.
- The format and analyzer rows are single wall-clock runs of commands taking 40–110 s, not hyperfine
  distributions. At that scale the gap is an order-of-magnitude story; do not read the second digit.
- The Roslynator comparison is not rule-for-rule. `--deep` runs the project's *own* referenced
  analyzers; Roslynator reports its own mix through an MSBuild workspace. Roslynator 0.12 also could
  not load the solution under its default SDK-10 MSBuild path at all and had to be pointed at an
  SDK-9 MSBuild; its `fix` mode failed before completing the solution, so there is no completed
  full-solution Roslynator fix baseline for this repository.

## Where we are not faster

This section exists because a benchmark page that only lists wins is advertising.

- **Large whole-solution whitespace runs.** 5.91× on the massive corpus, 6.84–7.35× on large. Both
  tools are doing real per-file work; our advantage narrows to the constant factors. Anyone sizing a
  big solution's CI budget should use these rows, not the startup row.
- **`--deep` on a cold, unscoped run.** `lint --deep` shells out to the real Roslyn analyzers, so it
  is subject to the same costs everything else is. On MassTransit's `src/MassTransit` (1,998 `.cs`
  files, 1,341 analyzers) a cold exhaustive `--deep` measured **~86–95 s** (2026-07-02). Scoping it
  to a 4-file change set — which is what CI actually needs — brings it to **~6.0 s**, about 15×, and
  ~6 s is the floor because the full compilation still has to be built. A warm host returns cached
  results in tens of milliseconds, but that is not the honest number for a fresh CI job.
- **Against Roslynator, `--deep` is ~2.9×, not orders of magnitude.** Once we are running real Roslyn
  analyzers we are in the same cost class as every other Roslyn-based tool. The large multiples on
  this page all come from the paths that avoid Roslyn entirely.
- **Anything needing type information or dataflow.** The native rules are syntactic. Rules that need
  semantics (unused-`using` removal, for instance) only exist under `--deep`, at `--deep` speed.
- **The `affected` ratio on a host with slow process creation.** 15.3× became 6.40× on the same
  hardware purely because `git` spawns got 3× more expensive. Your number depends on your machine's
  process-spawn cost, not only on ours.
- **Where the official tool does more.** Every ratio above compares a tool that loads MSBuild and
  Roslyn against one that does not. That is the design, and it is why the tool is fast — but it means
  the correct reading is "this workload does not need a semantic model", not "we made Roslyn faster".

### Reading the headline honestly

The site's "10–100× faster on common jobs" is a summary of the verified suite. The full measured
spread on the corpora above is **5.9× to 389×** — 5.91× for whole-solution whitespace on the massive
corpus at one end, 388.77× for default formatting on a tiny corpus (2026-06-16 smoke gate) at the
other. Whole-solution whitespace runs on the large and massive corpora (5.91–7.35×) and the
real-repository `format` lanes (~8.6–9.8×) all sit at the **bottom** of that range — below the
"10–100×" summary, not inside it. Those are the rows to plan against.

Your repository, hardware and configuration will land somewhere on that curve, and which end depends
mostly on how much of the run is fixed startup versus per-file rewriting. What does not vary is the
output: the verification requirement means a faster run is the same run.

## Formatter parity floors

Speed only counts if the output is right. The byte-exact whole-repository parity measurements against
real `dotnet format whitespace` live in the [support matrix](support-matrix.md#formatter-scope). The
current, coverage-checked sweep (2026-08-03):

| Repository | Files both tools formatted | Parity |
|---|---|---|
| Serilog | 216 / 216 | 100% |
| AutoMapper | 512 / 512 | 100% |
| Newtonsoft.Json | 940 / 941 | 99.89% |
| Polly | 775 / 776 | 99.87% |
| Dapper | 153 / 154 | 99.35% |

Two things about this table are worth more than the numbers in it.

**A parity percentage on its own is not a result.** Until 2026-08-03 the harness compared every
source file in two formatted copies of a repository — including files *neither* tool had opened,
which are byte-identical and scored as matches. It also chose its solution target with a directory
walk that returned nested solutions first, so on Polly it measured the 20-file `samples\Samples.slnx`
rather than the 764-file `Polly.slnx`, and 777 of the 797 counted files were free matches. **Every
Polly figure that harness produced is withdrawn, not restated** — it measured a different target, so
no comparison against it is meaningful. The harness now picks the solution reaching the most source
files and prints it, and runs a coverage probe that reports four numbers (matched, diverged,
untouched by both, touched by exactly one) and **fails the run** when files were touched by only one
tool. The percentage above is defined over files both tools actually formatted.

**Dapper's run used to fail that check; it now passes, and the earlier "not measurable" entry is
superseded.** The one-sided files were `tests\Dapper.Tests\Providers\SnowflakeTests.cs` and
`benchmarks\Dapper.Tests.Performance\Benchmarks.Norm.cs`, each wrapped whole in an `#if !NETFRAMEWORK`
/ `#if !NET4X` region that `dotnet format` treats as disabled text and never rewrites. The cause was
not, as previously published, that `dotnet-fast` is "syntax-only" about target frameworks — it binds
the project's real preprocessor symbols. It was three parsing bugs that made it mis-classify those
regions as active: a `.slnx` `<File Path="Build.csproj">` **solution item** read as a project (giving a
phantom repository-wide project with `DEBUG;TRACE` symbols), a UTF-8 BOM before `#if` stopping the line
reading as a directive, and a comment inside an `#if` condition aborting condition parsing. All three
are fixed; the coverage probe now reports zero one-sided files on Dapper, and the figure above —
**153/154 = 99.35%** — is the result. Whole-tree arithmetic, for continuity with the older series:
156/157 = 99.36%. The single remaining divergence is the indentation of a `= new(…)` continuation line
in `Dapper\CompiledRegex.cs`.

**Newtonsoft.Json now fails that check instead, on one file, without its figure moving.** Details are in
the [support matrix](support-matrix.md#formatter-scope): `Issue3080.cs` ends with an unterminated
`#endif`, which makes `dotnet format` trim the trailing whitespace on every directive line in the
document — a known final-line edge case, reachable only by the probe's synthetic perturbation, not by
the real source. Whole-tree parity is 944/945 = 99.89%, exactly the published floor.

Newtonsoft.Json is one file short of 100% for a separate, also-deliberate reason: a byte-exact,
BOM-aware comparer replaced a text comparer that decoded both sides first and treated a non-breaking
space (U+00A0) as equal to an ordinary space. One file indents a doc-comment continuation with
non-breaking spaces; `dotnet format` preserves them, `dotnet-fast` normalizes them to ASCII spaces.
The previously published 945/945 was a measurement artifact, and correcting it downward is what the
measurement is for.

Polly's single remaining divergence is a multi-space argument run in
`src\Snippets\Docs\ResiliencePipelineRegistry.cs`.

Corpus pins for the run above: Polly `101d6af`, Dapper `72a54c4`, Serilog `49b5339`, AutoMapper
`dfa6dd5`, Newtonsoft.Json `4f73e74`; oracle `dotnet format whitespace --no-restore` on SDK 10.0.302.

## Reproducing this

You do not need our harness. Everything above is `hyperfine` around two public commands, and the
methodology is the part that matters: reset the tree between runs, exclude restore from both sides,
use `-N`, and check the output before you trust the time.

Prerequisites: [hyperfine](https://github.com/sharkdp/hyperfine), the .NET SDK, `dotnet-fast`, and —
for the `affected` comparison — `dotnet tool install --global dotnet-affected`.

**Startup / already-formatted project.** Format once with each tool first so both measure a no-op,
then time:

```powershell
dotnet restore .\src\YourProject\YourProject.csproj
dotnet-fast format .\src\YourProject\YourProject.csproj
dotnet format .\src\YourProject\YourProject.csproj --no-restore

hyperfine -N --warmup 2 --runs 12 `
  --command-name 'dotnet-fast' 'dotnet-fast format .\src\YourProject\YourProject.csproj' `
  --command-name 'dotnet format' 'dotnet format .\src\YourProject\YourProject.csproj --no-restore'
```

**Whole-solution whitespace, cold tree.** `--prepare` restores a pristine copy before every timed
iteration, so neither tool ever measures an already-formatted tree:

```powershell
# one-time: keep a pristine copy to reset from
Copy-Item -Recurse .\work\subject .\work\pristine

hyperfine --warmup 0 --runs 5 `
  --prepare 'Remove-Item -Recurse -Force .\work\subject; Copy-Item -Recurse .\work\pristine .\work\subject' `
  --command-name 'dotnet-fast' 'dotnet-fast whitespace .\work\subject\YourSolution.sln --no-restore' `
  --command-name 'dotnet format' 'dotnet format whitespace .\work\subject\YourSolution.sln --no-restore'
```

Then verify equivalence before believing the timing — format one copy with each tool and compare the
trees:

```powershell
Get-ChildItem -Recurse -Filter *.cs .\a | ForEach-Object {
  $b = $_.FullName -replace '\\a\\', '\b\'
  if ((Get-FileHash $_.FullName).Hash -ne (Get-FileHash $b).Hash) { "DIFF: $($_.FullName)" }
}
```

**`affected`.** Check the result sets match first — a faster answer to a different question is not a
result:

```powershell
dotnet-fast affected -p . --from HEAD~1 --to HEAD --dry-run -f text
dotnet affected      -p . --from HEAD~1 --to HEAD --dry-run -f text

hyperfine -N --warmup 2 --runs 12 `
  --command-name 'dotnet-fast' 'dotnet-fast affected -p . --from HEAD~1 --to HEAD --dry-run -f text' `
  --command-name 'dotnet affected' 'dotnet affected -p . --from HEAD~1 --to HEAD --dry-run -f text'
```

**Record the environment canary alongside it**, or the `affected` ratio is uninterpretable:

```powershell
(1..5 | ForEach-Object { (Measure-Command { git --version }).TotalMilliseconds } |
  Measure-Object -Minimum).Minimum
```

Under ~10 ms is healthy. Above ~25 ms, your `affected` ratio is limited by process-spawn cost on your
machine, and comparing it to the numbers on this page tells you about the two machines rather than
about the two tools.

## What is not measured here

- **Non-Windows platforms.** Windows x64 is the only supported and only validated platform; there are
  no Linux or macOS numbers because there is no shipped binary to measure. See the
  [support matrix](support-matrix.md#platform).
- **Hosted CI agents.** Everything on this page is one developer workstation. A 2-core hosted agent
  changes the balance for anything I/O- or transfer-bound (the remote build cache especially), and
  matrix shards are independent agents rather than contended processes on one box.
- **Microbenchmarks.** Criterion benchmarks track internal hot paths (parser, discovery, formatter
  stages) and are used to explain regressions between releases. They are never the public story on
  their own — only a whole-project run proves the tool is faster for a real repository.

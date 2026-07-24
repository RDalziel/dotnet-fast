# How the performance numbers are measured

Every speed claim this project publishes follows the same methodology, so you can judge what a
number means and reproduce the shape of the test yourself.

## Two layers

1. **End-to-end corpus benchmarks** — `dotnet-fast` vs the official `dotnet format` /
   `dotnet-affected`, timed with [hyperfine](https://github.com/sharkdp/hyperfine) over real and
   synthetic C# repositories. These decide every published claim.
2. **Criterion microbenchmarks** — internal hot-path tracking (parser, discovery, formatting
   stages, SBOM generation). These explain regressions and price changes; they are never the
   public story on their own.

## What makes a number publishable

A benchmark run backs a claim only when all of these hold:

- **Output verification**: the run first proves both tools produce equivalent output on the corpus
  (byte-compared). A fast tool that formats differently isn't faster — it's wrong. Runs without
  verification are marked exploratory and never published.
- **Release builds** of this tool; pinned, stated versions of the official tools and .NET SDK.
- **Raw data kept**: hyperfine's JSON/CSV plus a metadata record (commands, versions, git state,
  OS, CPU, corpus, warm/cold cache state) accompanies every published table.
- **Suite over cherry-pick**: headline numbers come from the medium/large/massive
  default-formatting corpora across a scenario matrix (cold whitespace, default formatting,
  targeted project, warm/no-op, generated files, each claimed diagnostic) — not from a single
  favorable scenario. Tiny corpora are used only for startup smoke checks.
- **A regression floor**: releases run a gate that fails if any measured scenario drops below a
  minimum speedup vs `dotnet format`, so a published claim can't silently rot.

## Cold vs warm, stated explicitly

First runs (CI, one-shot) are cold; editor/daemon loops are warm. Where a claim depends on a warm
process (for example `--deep`'s analyzer host), the number says so — cold `--deep` is seconds, and
the docs say that too.

## The real-world case study

Beyond synthetic corpora, the standing head-to-head is a large unmodified production repository
(MassTransit, ~5,800 C# files): `affected` 63.8× faster with an identical result set, `format`
~9–10× faster with verified-equivalent output, and `--deep` (real Roslyn analyzers through the warm
host) at 29 s vs Roslynator `analyze` at 85 s on the same machine. Hardware, versions, and full
tables ship with the release notes when these are regenerated.

## The 1.0 gate verification run (v0.291.3, 2026-07-12)

The final pre-1.0 run against the frozen [support matrix](support-matrix.md), on the standing gate
machine (Windows 11 x64, .NET SDK 10.x, release build, hyperfine, shell-free `-N` timing):

**Formatting parity** (whole-repository, byte-compared against real `dotnet format`):

| Repository | Byte-identical files | Parity |
|---|---|---|
| Newtonsoft.Json | 945 / 945 | 100% |
| Polly | 797 / 797 | 100% |
| Dapper | 156 / 157 | 99.36% |
| AutoMapper | 508 / 512 | 99.22% |
| Serilog | 214 / 216 | 99.07% |

**Speed** (output-verified first, per the rules above):

- `format` (startup-bound, already-formatted project): **19.8 ms ± 1.1 ms** vs `dotnet format` at
  **4.92 s ± 0.06 s** — **~248× faster** (12 runs, tight variance).
- `affected` (60-project reference chain, leaf change): **6.40× faster** end-to-end on the latest
  re-baseline (2026-07-17, v0.294.3). Honest caveat: the tool's own work here is ~10 ms (project
  discovery + graph); the rest of its wall time is spawning `git` subprocesses, and on this machine's
  current configuration a bare `git --version` alone costs ~30 ms (min of 5), which compresses the
  measured ratio. A prior run on the same hardware (2026-06-28, v0.230.0) measured **15.3×** (60
  projects) and **22.7×** (300 projects) when git spawned in ~10 ms; the 2026-07-12 gate run measured
  **4.0×** at the same degraded git cost. The delta is environmental, not a change in the tool — the
  format numbers above (which never spawn `git`) are unaffected. This git-spawn latency is now recorded
  automatically as a canary (bare `git --version`, min of 5) in every benchmark artifact the harness
  writes, so any future ratio compression is traceable to the environment. Root-cause remediation (a
  Windows Defender exclusion) requires elevated access and is tracked separately.

## Reading the headline honestly

"10–100× faster on common jobs" summarizes the verified suite results across corpora and scenarios
on the stated hardware. Your repository, hardware, and configuration will land somewhere in that
range, not at a fixed point — the verification requirement means the *output* is the same either
way.

# Deep linting (real Roslyn analyzers)

By default `dotnet-fast lint` is fast and self-contained: it reads your code, applies its own rule set, and
finishes in milliseconds with no setup — no .NET SDK required, no restore, runs on any checkout. That's the
right default for a quick CI gate.

`--deep` is the opt-in depth pass. It runs **your project's real Roslyn analyzers** and merges their
findings into the same report.

```bash
dotnet-fast lint --deep App.sln
```

## What `--deep` runs

Every analyzer your project actually uses:

- **Analyzer packages you reference** — StyleCop, Roslynator, Meziantou, SonarAnalyzer, and any other.
- **The .NET SDK's built-in analyzers (`CA*`)** — the same `CA*` findings `dotnet build` produces.

Findings honor your `.editorconfig` severities, exactly like a real build.

One boundary to know: `--deep` runs analyzers at their **default analysis level** — the rules and
severities a plain `dotnet build` enables. It does not replicate the stricter opt-in modes
(`AnalysisLevel`/`AnalysisMode` escalations such as `All`/`AllEnabledByDefault`); findings that
only appear under a strict mode will not appear here.

## Why it's opt-in (not the default)

`--deep` needs two things the default doesn't:

1. The .NET SDK installed.
2. A restored project (you've run `dotnet restore` or built once).

The default needs neither — that "runs anywhere, no setup" property is the whole point. Making deep the
default would silently do nothing on a fresh checkout and require the SDK everywhere.

## How fast is it?

- **Default lint:** milliseconds.
- **`--deep` in an editor or warm CI loop:** still fast — analyzers stay loaded and results are cached
  between runs.
- **`--deep` on a cold first run** (a fresh one-shot CI step): a few seconds, because it has to start up
  and load every analyzer once.

So deep stays snappy where you run it repeatedly, and costs a one-time few seconds on a cold start.

## Scoped analysis for changed files (CI / affected runs)

When you point `--deep` at a Git change set — `--ci`, `--affected`, `--staged`, or `--from <ref>` — it
only reports findings in the files you changed, so there's no reason to re-analyze the whole project.
In that case deep **scopes the analysis to the changed files**: it still builds the full compilation
(so a changed file's types resolve against the rest of the project), but runs the analyzers over only
the changed syntax trees. On a large, analyzer-heavy solution a PR touching a handful of files no longer
pays to re-analyze thousands of unchanged ones — the analyze phase drops toward the cost of those few
files instead of the whole project.

This is the default for change-set runs. The trade-off: scoped analysis runs each changed file's
syntax, semantic, and symbol rules, but **not whole-program (compilation-end) rules** — a small number of
analyzers (e.g. some cross-file Sonar/Roslynator/Meziantou checks) that only report after seeing the
entire compilation. These are rare, and the exhaustive path still exists for the authoritative run:

```bash
# Force a complete whole-compilation deep run (includes compilation-end rules),
# even when a changed-file scope is available:
DOTNET_FAST_DEEP_FULL=1 dotnet-fast lint --deep --ci
```

A bare `lint --deep` with no Git range is already exhaustive.

## Caching deep results across runs (`--deep-cache`)

Scoped analysis cuts the *analyze* phase, but `--deep` still binds every project's semantic model from
source — so on a cache-backed pipeline the compilation is effectively built twice (once by
`dotnet-fast build`, once by `--deep`), even for projects that were a 100% cache hit. `--deep-cache`
removes that for unchanged projects: it caches each project's analyzer diagnostics in the build cache,
keyed by the project's **build-cache key** + its **`.editorconfig` chain** + the **host identity**.

```bash
# CI: analyze changed projects, restore the rest from cache.
dotnet-fast lint --deep --deep-cache --ci App.sln
```

- A project whose source, references, analyzer versions (`packages.lock.json`), SDK, and rule config are
  all unchanged restores its diagnostics as a **blob lookup — no re-bind**. Only changed projects
  actually re-run analyzers, so `--deep` scales with change, not project count: a warm PR touching a few
  projects drops from minutes to seconds.
- The key is **strictly larger** than the binary cache — a rule-severity edit (`.editorconfig`) or an
  analyzer-package bump moves it, so stale diagnostics are never served. Diagnostic locations are
  repo-relative, so the cache is portable across agents (unlike `project.assets.json`).
- Requires a configured build cache; it respects `buildCache.readOnly` (PR runs consume but don't
  upload — the trusted/main build is the writer). **Opt-in**, because a few analyzers aren't
  deterministic; leave it off for those.

## Turning deep on by default

If your environment always has the SDK and a restored project (a warm CI agent, an editor session), opt
into deep-by-default:

```bash
export DOTNET_FAST_DEEP=1
```

A plain `lint` then uses deep **only when the SDK and a restored project are present**. When they aren't,
it quietly falls back to the fast default with a one-line note — never an error, never a silent skip. Use
`--no-deep` to force the fast path for a single run.

## Native ports — popular analyzers without `--deep`

A growing set of popular Roslyn analyzers is re-implemented **natively** (no Roslyn, no SDK), so you get
those findings in the fast default path. Each is opt-in per repo via `.editorconfig` and verified at exact
parity against the real analyzer. See **[ported-analyzers.md](ported-analyzers.md)** for the full list and
how to enable each one.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

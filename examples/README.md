# Examples

Small, runnable projects to try `dotnet-fast` against.

## HelloWorld

A tiny console app with deliberately messy spacing so you can see the tool work.

```bash
cd HelloWorld

dotnet-fast lint .          # report the formatting findings (non-zero exit)
dotnet-fast lint --fix .    # fix them in place
dotnet-fast lint .          # clean now (exit 0)

dotnet-fast doctor .        # build-free workspace scan
dotnet-fast metrics .       # score it against the ten code-health budgets
```

`metrics` prints a scoreboard rather than a list of findings — complexity, lines per file, redundant
code and `dynamic` usage straight from the source, with coverage, CRAP and surviving mutants filled in
from reports when you pass `--coverage` / `--mutation`. HelloWorld is small enough that every row is
comfortably inside budget, which is what a clean board looks like. See
[../docs/metrics.md](../docs/metrics.md).

Want to try deep linting? Add an analyzer package to `HelloWorld.csproj`, restore, then:

```bash
dotnet restore
dotnet-fast lint --deep .
```

See [../docs/deep-linting.md](../docs/deep-linting.md) for what deep mode runs and when it's fast.

## CI features

The tiny example is intentionally too small to benefit from build caching or test sharding, but the same
tool commands apply to larger workspaces:

- [CI wiring](../docs/ci.md) - the lint gate, affected scoping, and the exit-code contract.
- [Build cache](../docs/build-cache.md) - restore cached `bin/` and `obj/` outputs on CI agents.
- [Test sharding](../docs/test-sharding.md) - split NUnit test projects across parallel agents.
- [Dead code](../docs/dead-code.md) - find unused types and members across a whole solution.
- [All documentation](../docs/README.md) - every page, grouped by task.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

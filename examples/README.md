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
```

Want to try deep linting? Add an analyzer package to `HelloWorld.csproj`, restore, then:

```bash
dotnet restore
dotnet-fast lint --deep .
```

See [../docs/deep-linting.md](../docs/deep-linting.md) for what deep mode runs and when it's fast.

## CI features

The tiny example is intentionally too small to benefit from build caching or test sharding, but the same
tool commands apply to larger workspaces:

- [Build cache](../docs/build-cache.md) - restore cached `bin/` and `obj/` outputs on CI agents.
- [Test sharding](../docs/test-sharding.md) - split NUnit test projects across parallel agents.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

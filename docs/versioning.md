# Versioning policy

`dotnet-fast` follows [Semantic Versioning](https://semver.org/) (`MAJOR.MINOR.PATCH`). This page is the
concrete, `dotnet-fast`-specific meaning of each version segment, and what is (and isn't) covered by the
compatibility promise.

## What each segment means

- **MAJOR** — a breaking change to the CLI contract: an existing command or flag's name, behavior,
  default value, output shape (text, `--json` fields, SARIF fields, report file names), or exit code
  changes. Reserved for the rare case a breaking change is unavoidable, and always called out clearly in
  the release notes.
- **MINOR** — additive changes: a new subcommand, a new opt-in flag, a new lint rule turned on by
  default, a new `--json` field appended without renaming or removing an existing one. Existing
  commands/flags keep working exactly as before.
- **PATCH** — fixes with no CLI-contract change: bug fixes, formatting/lint parity improvements,
  performance work, and similar changes that don't alter any documented command, flag, default, output
  shape, or exit code.

## The CLI contract

Once a command or flag ships, its **name, behavior, arguments, defaults, output shape, and exit codes
are stable**. New ways of doing things are added as new, opt-in surface — a new subcommand or a new flag
with a default that preserves the old behavior — never by silently changing what an existing invocation
does. A pipeline that works today keeps working on every later release in the same major line.

### Removing something

Nothing is removed outright. A command or flag that's being retired goes through a deprecation cycle:

1. The old entry **keeps working**, unchanged, while a deprecation notice is printed to **stderr**,
   pointing at the replacement (and, where known, the version it will be removed in).
2. It stays available like that for **multiple releases** — long enough for pipelines to migrate — and
   the deprecation is documented in the release notes.
3. Only after that window does removal happen, and only as a **major** version bump. After removal, the
   old command **fails with a helpful "removed — use X instead" message**, never a generic
   unknown-command error.

## Formatting parity floors are part of the promise

`dotnet-fast format` / `lint --fix` track `dotnet format` (.NET SDK 10.x). A release never ships below
the parity floors published in [support-matrix.md](support-matrix.md) — currently 99%+ on all five
tracked repositories (Newtonsoft.Json, Polly, Dapper, AutoMapper, Serilog). If a change would drop any
of them below its floor, it doesn't ship as-is. The exact per-repository figures live on that page and
are the authority; this page does not restate them, because it is the page that got them wrong. It read
"100% on Newtonsoft.Json and Polly" after both figures had been withdrawn as measurement artifacts —
see the 2026-08-03 corrections in [support-matrix.md](support-matrix.md#formatter-scope).

## What this promise does **not** cover

Some things are deliberately outside the compatibility guarantee, because pinning them would block
routine improvement without giving you anything reliable to depend on:

- **Exact byte content of human-readable text** — unified diff bodies, log/progress text written to
  stderr, and similar free-text output can be reworded between releases. Machine-readable shapes
  (`--json` fields, SARIF fields, report file names, exit codes) are covered; prose isn't.
- **Timing field values** — `--json` output includes the run's own timing; the fields exist and are
  stable, but the numbers themselves obviously vary and are not part of any compatibility promise.
- **Anything outside Windows x64** — Windows x64 is the only supported platform and the only one a
  binary ships for; nothing published runs on Linux or macOS today (see
  [support-matrix.md](support-matrix.md#platform)). When linux-x64 is added in the 1.x line it starts
  outside this promise, and joins it only when a release note says it is validated.
- **`--deep` findings** — `--deep` runs your project's own real Roslyn analyzers (whatever packages you
  reference, plus the .NET SDK's built-in analyzers) at their default analysis level. Those findings
  come from your dependencies and your installed SDK, not from `dotnet-fast` itself, so they change as
  your analyzer packages and SDK version change — independent of the `dotnet-fast` version.

## Pre-release channel

Ahead of a significant release, a pre-release build may be published to NuGet under an `-rc` version
suffix (e.g. `1.0.0-rc.1`) so it can be validated in real pipelines before the final tag. Pre-release
versions are not covered by the compatibility promise above — install a stable (non-`-rc`) version for
anything that needs it.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

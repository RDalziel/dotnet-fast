# Security & supply chain

What `dotnet-fast` is signed with, what it isn't, and exactly how to check a copy you've downloaded.

`dotnet-fast` is a tool you run inside your build, so it's reasonable to want to know where the bits
came from. This page states the position plainly, including the parts we **don't** cover — an
overstated supply-chain claim is worse than an honest gap.

## Summary

| Artifact | Integrity | Signature | Build provenance |
|---|---|---|---|
| NuGet package `RDLL.dotnet-fast` | Package content hash, enforced by the NuGet client | **nuget.org repository signature** (Microsoft, DigiCert chain, RFC-3161 timestamped) | None published |

**The NuGet package is the only artifact published.** There is no downloadable standalone binary today
(see [below](#the-standalone-binary--not-published-today)). Everything in the "Signature" column comes
from nuget.org, not from us: there is **no author signature** and **no build attestation**. The rest of
this page is what that does and doesn't buy you.

## The NuGet package

This is the supported install path, and the channel with the strongest guarantees.

```bash
dotnet tool install -g RDLL.dotnet-fast
```

### What nuget.org does automatically

When nuget.org accepts a package it **repository-signs** it: a countersignature over the exact
package content, made with a Microsoft-held code-signing certificate chaining to DigiCert, and
RFC-3161 timestamped. The signature embeds the source service index and the package's owner list at
the time of submission.

This is applied by nuget.org during ingestion — the package we push is unsigned when it leaves the
build. Concretely, the repository signature proves:

- The package content is **byte-identical** to what nuget.org ingested. If a `.nupkg` is modified
  after publication, the NuGet client rejects it with `NU3008` rather than installing it.
- nuget.org accepted it under the **`RDLL`** owner account, i.e. it went through that account's
  publishing credentials, not an anonymous upload.
- **When** it was signed, via the trusted timestamp — so the signature remains checkable after the
  signing certificate expires.

It does **not** prove who built the package, from what source, or on what machine. It is a
distribution-integrity signature, not a provenance statement.

### Verify a package yourself

Download the `.nupkg` and verify every signature on it:

```bash
curl -sSLO https://api.nuget.org/v3-flatcontainer/rdll.dotnet-fast/1.0.0/rdll.dotnet-fast.1.0.0.nupkg
dotnet nuget verify rdll.dotnet-fast.1.0.0.nupkg --all
```

That prints the real output for `1.0.0` — at the default verbosity, exactly this and nothing else:

```
Verifying RDLL.dotnet-fast.1.0.0
Content hash: WhLgD60/cnglAQfwaMXex+mCRO2cI4d1llIB3Ro8fGo6MbXtPomHwFUbjN5P8rDWgluA0DlYFdSA1fnvrWdkIg==

Signature type: Repository
  Subject Name: CN=NuGet.org Repository by Microsoft, O=NuGet.org Repository by Microsoft, L=Redmond, S=Washington, C=US
  SHA256 hash: 1F4B311D9ACC115C8DC8018B5A49E00FCE6DA8E2855F9F014CA6F34570BC482D
  Valid from: 23/02/2024 00:00:00 to 19/05/2027 00:59:59
```

**There is no success line at the default verbosity — check the exit code, not the text.** `0` means
verified. Add `-v normal` to get an explicit
`Successfully verified package 'RDLL.dotnet-fast.1.0.0'.`, or `-v detailed` to additionally print
`Service index: https://api.nuget.org/v3/index.json` and `Owners: RDLL` plus the full certificate and
timestamp chains.

`Signature type: Repository` is the only signature you should expect. If you ever see an
**Author** signature on this package, treat it as suspicious and
[report it](https://github.com/RDalziel/dotnet-fast/issues) — we don't author-sign.

### Make your build *require* it

You don't need anything from us to harden this. Register nuget.org as a trusted repository pinned to
this package's owner, then require signature validation:

```bash
dotnet nuget trust source nuget.org --owners RDLL
dotnet nuget trust list
```

That writes a `trustedSigners` entry with nuget.org's repository certificate fingerprints and
`Trusted owners: RDLL`. Pair it with `signatureValidationMode` in your `nuget.config` to turn it into
a hard gate:

```xml
<configuration>
  <config>
    <add key="signatureValidationMode" value="require" />
  </config>
</configuration>
```

With `require` set, restore fails unless the package carries a signature from a signer you've
trusted. Note this applies to **every** package you restore, not just this one — roll it out
deliberately.

### How the package reaches nuget.org

Publication uses **NuGet Trusted Publishing**: the release job proves its identity to nuget.org with
a short-lived OIDC token and receives a **one-hour, single-use API key** in exchange. There is no
long-lived NuGet API key stored anywhere — nothing to leak, and nothing an attacker can steal and
reuse later. Publication is started deliberately by the maintainer; nothing publishes on a code push,
a merge, or a schedule.

That constrains *who* can publish. It does not, on its own, attest to what was built.

## The standalone binary — not published today

Each release builds a self-contained `dotnet-fast-win-x64.exe` and a matching SHA-256, but **neither is
currently available for you to download.** NuGet is the only distribution channel today. Publishing the
binary as a downloadable release asset is a 1.x item; when it happens, this section will describe how to
verify it and a release note will say so.

If what you actually want is a CI agent that doesn't pay a `dotnet tool restore` per job, install once
into a directory and cache that directory:

```bash
dotnet tool install RDLL.dotnet-fast --tool-path ./.dotnet-fast
```

That install path goes through NuGet, so the repository signature described above applies to it — which
is the stronger check anyway. `--tool-path` is documented in [install.md](install.md).

Worth stating even though it's moot today: a checksum published next to the file it describes only
detects accidental corruption — a truncated download, a bad proxy, a flaky mirror. Anyone able to
replace the binary could replace the checksum beside it. It is not protection against a compromised
release channel. The NuGet path is stronger precisely because the signature is issued by a third party.

Nothing we ship is Authenticode-signed, so Windows SmartScreen may warn on first run.

Windows x64 is the only validated platform — see
[support-matrix.md](support-matrix.md#platform) and [versioning.md](versioning.md).

## The v1.0 position

**For 1.0 the position is: nuget.org's repository signature on the package, and no provenance or
attestation claim beyond that.** We considered the alternatives and are not adopting them for 1.0. The
reasoning, so you can judge it rather than take it on trust:

**GitHub artifact attestations** (`actions/attest-build-provenance`) produce a Sigstore-signed
statement binding an artifact's digest to the workflow, repository, and commit that produced it —
genuinely useful, and the option we'd most want. Two things block it here. First, on GitHub Free,
Pro, and Team plans attestations are only available for **public** repositories; private repositories
require GitHub Enterprise Cloud. `dotnet-fast` is distributed as a binary under a
[freeware license](../LICENSE.txt) with its implementation kept closed, so the repository that builds
it is private and the feature is not available to it. Second, attestations for private repositories
are signed by GitHub's private Sigstore instance with **no public transparency log**, so an outside
consumer could not independently audit them even if we produced them. Publishing an attestation you
can't check isn't worth the badge.

**SLSA.** GitHub describes its attestations as corresponding to SLSA v1.0 **Build Level 2**, which
requires the build to run *"on dedicated infrastructure, not an individual's workstation"*, with
provenance tied to that infrastructure by a digital signature. `dotnet-fast` release builds run on
infrastructure the maintainer controls rather than on an isolated, ephemeral third-party builder, so
Build L2 is **not** honestly claimable today. Build L3 — which additionally requires runs to be
isolated from each other and signing material to be unreachable from build steps — is further out
still. Build L1 only requires that provenance *exist*; we could emit a provenance document tomorrow,
but self-asserted, unsigned provenance from a closed build adds a file, not a security property. So
we claim no SLSA level rather than claim one we can't support.

**The repository signature, documented as such** — what you're reading. It's the option that describes
reality, and auditing it for this page surfaced something we'd been undercounting: the nuget.org
repository signature is meaningfully stronger than a checksum, is already on every release, and most
consumers weren't being told it was there or how to enforce it. Documenting and enforcing what we
actually have beats advertising a property we don't.

If author signing becomes worthwhile after 1.0, **NuGet author signing** is the natural next step: it
would add the one property missing today — that the package was signed by a key the publisher
controls — and, unlike attestations, it needs neither a public repository nor a hosted build service.
It requires a CA-issued code-signing certificate and somewhere safe to keep the private key, so it's
a deliberate decision rather than a switch to flip. It is not part of 1.0.

## What we do not claim

- **No build provenance.** Nothing published today ties a released artifact back to a specific source
  commit or build. The `<repository>` metadata inside the `.nupkg` points at this documentation
  repository and is not a verifiable build record — don't treat it as one.
- **No SLSA level.** See above.
- **No reproducible builds.** Building the tool twice is not guaranteed to produce byte-identical
  output, and there is no independent rebuild to compare against.
- **No author signature or Authenticode signature** on the package or the binary.
- **No third-party audit** of the tool's source.

What you *can* rely on: a package on nuget.org whose content is signed and timestamped by Microsoft
under a named owner, published through a keyless flow with no standing credential.

## Reporting a security issue

If you find a security problem in `dotnet-fast` — a crash on hostile input, a path-traversal in file
writing, credential handling in the build cache — please
[open an issue](https://github.com/RDalziel/dotnet-fast/issues/new/choose) and say up front that it's
security-related, so it gets triaged first. Include the version (`dotnet-fast --version`) and a
minimal reproduction.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

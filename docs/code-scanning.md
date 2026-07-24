# GitHub code scanning (SARIF)

Four commands can write [SARIF 2.1.0](https://sarifweb.azurewebsites.net/) instead of — or
alongside — their normal output: `lint`, `format`, `affected`, and `doctor`. Upload the result with
`github/codeql-action/upload-sarif` and it shows up in your repository's **Security → Code
scanning** tab, same as any other scanner.

## Which commands, and what each SARIF means

```bash
dotnet-fast lint App.sln --sarif lint.sarif           # findings: whitespace/style/lint/deep
dotnet-fast format App.sln --sarif format.sarif        # findings: same set, from the format path
dotnet-fast doctor App.sln --sarif doctor.sarif        # findings: workspace-health smells
dotnet-fast affected --ci --format sarif --output-name affected .   # impact, not findings
```

**`lint`/`format`/`doctor`** emit finding-level SARIF — one result per diagnostic, with a location
(file/line/column), a rule id, and a severity. `lint --deep` findings merge into the same report:
every result carries `properties.engine` (`native-format`, `native-cst`, or `roslyn-deep`) so you
can tell which engine caught what, without losing the unified view.

**`affected --format sarif`** is different: it's an *impact* report, one result per affected
project under a single `AFFECTED` rule, not a set of code-quality findings. It answers "what did
this change touch" rather than "what's wrong with this code" — useful as a companion signal
alongside the finding-level reports, showing which projects a PR's findings actually matter for.

## They share one driver — one tool entry in the Security tab

All four reports use the same SARIF driver name (`dotnet-fast`), so uploading several from one job
doesn't create four separate tool entries in GitHub's UI — they merge into one. Native `DF*` rules
and every ported analyzer rule carry full driver metadata (`shortDescription`, `fullDescription`,
`helpUri` pointing at the rule's own docs page), so the alert detail in GitHub shows the same
explanation you'd get from `dotnet-fast lint --explain <id>`.

## A working GitHub Actions job

```yaml
name: code-scanning
on:
  push:
    branches: [main]
  pull_request:

permissions:
  contents: read
  security-events: write   # required to upload SARIF

jobs:
  scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      - name: Install dotnet-fast
        run: dotnet tool install -g dotnet-fast
      - name: Lint (report-only, don't fail the step — let code scanning surface it)
        run: dotnet-fast lint . --sarif lint.sarif
        continue-on-error: true
      - name: Doctor
        run: dotnet-fast doctor . --sarif doctor.sarif
        continue-on-error: true
      - uses: github/codeql-action/upload-sarif@v3
        with:
          sarif_file: .
          category: dotnet-fast
```

`upload-sarif` accepts either a single file or, as above, a directory — it uploads every `*.sarif`
file it finds, which is the simplest way to merge `lint.sarif` and `doctor.sarif` (and `affected`'s,
if you're also running that) into one upload step. `continue-on-error: true` on the scanning steps
matters: if the step itself fails the job on a non-zero exit code, the upload step never runs and
nothing reaches the Security tab — let code scanning be the gate, not the shell exit code, unless
you also want a hard CI failure.

## PR-scoped scanning

Add `--from`/`--to` (or `--ci`) to any of the finding-level commands to scope the SARIF to a
pull request's actual diff instead of the whole repository — the same scoping `lint`'s text report
uses. See [`lint`](commands.md#lint) for the full range-flag set.

```bash
dotnet-fast lint . --ci --sarif lint.sarif
```

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

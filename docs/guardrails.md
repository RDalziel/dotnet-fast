# Guardrails — lint rules for agent-written code

Four optional rules, `DF9001`–`DF9004`, for repositories where an agent writes some of the code and a
human reviews it.

They exist because of one asymmetry: **an agent will happily ignore a style guide, but it cannot
ignore a lint error in CI.** Documentation is advice; a failing check is a constraint. So the rules
that actually shape what gets generated are the ones a linter enforces.

These are not ports of any Roslyn analyzer. They are opinions — deliberately so — and they are **off
by default**. Nothing changes in your repository until you ask for it.

## The rules

| Rule | Reports | Threshold |
|---|---|---|
| `DF9001` | A `//` or `/* … */` comment. XML documentation (`///`, `/** … */`) is exempt. | — |
| `DF9002` | A method, constructor, finalizer, operator, local function or accessor over the line budget. | `dotnet_fast_max_lines_per_function` (50) |
| `DF9003` | A file over the line budget, blank lines excluded. | `dotnet_fast_max_lines_per_file` (250) |
| `DF9004` | A numeric literal with no name. | `dotnet_fast_magic_number_allowed` (`0,1,-1,2`) |

## Enabling them

Each rule is switched on individually, in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.DF9001.severity = warning
dotnet_diagnostic.DF9002.severity = warning
dotnet_diagnostic.DF9003.severity = warning
dotnet_diagnostic.DF9004.severity = warning

# Thresholds, shown with their defaults — omit any you are happy with.
dotnet_fast_max_lines_per_function = 50
dotnet_fast_max_lines_per_file = 250
dotnet_fast_magic_number_allowed = 0,1,-1,2
```

Only a per-rule `dotnet_diagnostic.DF900x.severity` enables one. A bulk
`dotnet_analyzer_diagnostic.severity` key does **not**, mirroring how Roslyn treats an analyzer that
is disabled by default — so a repository that already sets a bulk severity does not silently acquire
four new rule families on upgrade.

All four are **report-only**. `lint --fix` never rewrites code on their account: "extract this into a
method" has no mechanical fix, and a tool that guessed at one would do more harm than good.

## The messages are the feature

Each message is written for the agent that has to act on it. It names **the design principle at
stake and the concrete move**, not just the violation — because "file too long" invites deleting
blank lines, while naming the single-responsibility principle invites separating concerns.

`DF9003` does not say *file too long*. It says:

> `File is 412 lines (limit 250). This is the SINGLE RESPONSIBILITY PRINCIPLE (the S in SOLID)
> stated as a measurement: a file this long almost always has more than one reason to change. Before
> editing further, identify the distinct responsibilities here and separate them — give each type its
> own file, and extract each collaborator behind an interface so callers depend on the abstraction
> rather than this file's internals (the D in SOLID). Adding to a file already over the limit
> compounds the problem; prefer a new, well-named type over another member here.`

`DF9002` does not say *method too long*. It says:

> `Method 'BuildReport' is 84 lines (limit 50). SINGLE RESPONSIBILITY applies to a member as much as
> to a class: this one is doing several jobs at once. Apply EXTRACT METHOD — find each cohesive step
> (a block you could name, often one already marked by a blank line or a comment), move it to its own
> private method named for WHAT it achieves rather than how, and repeat until the body reads as a
> list of intentions. Each extracted step also becomes separately testable, which is usually the
> larger win. If the steps do not share the member's state, that is a sign they belong on a different
> type entirely.`

`DF9004` does not say *magic number*. It names the value's meaning as the thing that is missing, and
points past the minimum fix:

> ``Magic number `3600` has no name, so its meaning lives only in the head of whoever wrote it.
> Extract it to a named constant (`private const … = 3600;`) named for what the value MEANS, not what
> it is — `RetryLimit`, not `Three`. This is DRY applied to knowledge rather than to code… If the
> value is a policy someone might tune (a timeout, a threshold, a page size), a constant is the
> minimum — consider whether it belongs in configuration or as a constructor parameter.``

`DF9001` is the one that most often gets the wrong edit — an agent told "no comments" may simply
delete the comment and lose what it knew. So the message distinguishes the two kinds:

> `…rename to intent-revealing names, extract the described block into a well-named method, or
> replace a literal with a named constant — then delete the comment… If the comment records WHY
> rather than WHAT (a rejected alternative, a workaround for an external bug, a non-obvious
> constraint), that knowledge is worth keeping — move it to `///` XML documentation, which is exempt.`

Run `dotnet-fast lint --explain DF9001` for the full description of any rule.

## What each rule is actually for

### `DF9001` — no explanatory comments

A comment usually marks the place where the code failed to say what it does. The durable fix is an
intent-revealing name, or extracting the described block into a well-named method. Comments rot
silently; names are checked by every reader.

**`///` XML documentation is exempt.** A public API contract is exactly where prose is the right
medium — the rule pushes explanation out of the body and into names, not out of the codebase.

Block comments (`/* … */`) are reported alongside `//`. That is deliberate: exempting them would
leave a one-keystroke way around a rule whose entire value is that it cannot be worked around.

Also exempt: this tool's own suppression directives (otherwise the rule could not be suppressed) and
`<auto-generated>` markers.

### `DF9002` — a line budget per member

A member past the limit is nearly always several jobs in a trench coat. The budget is what triggers
the extraction.

Measured from the first non-attribute token to the closing brace, blank lines excluded. Attribute
lines do not count — an eight-line `[Theory]`/`[InlineData]` stack should not consume a sixth of a
test's budget.

Lambdas and anonymous methods are not measured separately: their lines already count toward the
enclosing member, so reporting both would be two findings for one overlong body.

### `DF9003` — a line budget per file

The single-responsibility principle, stated as a measurement. A file this long has more than one
reason to change.

Reported once, at line 1, because the fix is structural rather than local. Generated files are
already outside the lint walk unless you pass `--include-generated`.

### `DF9004` — no unnamed numbers

Values are matched by value, not spelling: `0x02`, `2L` and `2.0` all satisfy a configured `2`.

Exempt are the positions where the surrounding syntax **already names the value**: a `const` or
`readonly` field initializer, a `const` local, an `enum` member, a parameter default, and an array
index.

Attribute arguments and `case` labels are **not** exempt. Both require a compile-time constant, but a
named `const` satisfies that requirement — so `[MaxLength(255)]` and `case 9:` are precisely the
sites worth naming.

## Adopting them without a wall of findings

Turning all four on across an existing codebase will produce a lot of output. Two approaches that
work:

**Scope them to where they earn their keep.** Test projects legitimately carry magic numbers and long
fixtures:

```ini
[*.cs]
dotnet_diagnostic.DF9004.severity = warning

[**/*Tests/**.cs]
dotnet_diagnostic.DF9004.severity = none
```

**Fail only on new findings.** `lint --baseline` records what exists today and fails only on what is
added after:

```bash
dotnet-fast lint --baseline guardrails.json .
```

That is usually the right on-ramp: the constraint applies to new code — which is where the agent is
working — without demanding a cleanup of everything already written.

## Suppressing individually

Same mechanisms as any other rule:

```csharp
Wait(3600); // dotnet-fast-disable-line DF9004

#pragma warning disable DF9004
```

## A note on thresholds

An unparseable threshold keeps the **default** rather than removing the limit:
`dotnet_fast_max_lines_per_file = fifty` leaves the limit at 250. A typo that silently switched a
guardrail off would turn a green build into a false green, which is the one failure mode a guardrail
must never have.

## Related

- [commands.md](commands.md#lint) — the full `lint` reference
- [ported-analyzers.md](ported-analyzers.md) — the Roslyn analyzers re-implemented natively, which
  are on by default and are a different thing entirely

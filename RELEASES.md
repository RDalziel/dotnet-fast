# Release notes

What changed in recent releases, in plain English. Newest first.

The current **stable** line is `1.11.0`. Pre-1.0 history — predating the compatibility promise and the
NuGet package — is a git-history pointer, not full notes, in
[RELEASES-0.x.md](RELEASES-0.x.md).

## 1.11.0 — 2026-10-08

### Removed: the `rewrite` command

`dotnet-fast rewrite` (structural search and replace, added in 1.8.0) has been removed. It was
read-only (search, a diff preview, and a `--check` gate) and nothing else in the tool used it.

Running `rewrite` now, with or without its old flags, prints a notice and exits `2`, so a pipeline
still calling `rewrite --check` fails loudly instead of passing silently. dotnet-fast has no
replacement: to ban an API in CI, use the Roslyn analyzer `Microsoft.CodeAnalysis.BannedApiAnalyzers`;
for structural search over C#, use [ast-grep](https://ast-grep.github.io).

## 1.10.7 — 2026-10-06

### Fixed: a space forced after `(` in front of a relational pattern (#342)

`whitespace`, `format` and `lint` added a space between an opening parenthesis and a relational
pattern that starts inside it, and reported the code as needing a fix, so a CI gate failed on valid
code that `dotnet format` leaves alone. 1.10.5 and 1.10.6 both did this. For example:

- `x is > 1 and (< 5 or > 9)` became `x is > 1 and ( < 5 or > 9)`;
- `x is (< 5)`, `x is not (<= 5)`, `case (< 5):` and the switch arm `(>= 5 and < 9) =>` got the
  same space, as did `p is Pt(< 5, > 9)` and the nested `t is ((< 5), _)`.

These now stay as written, and an existing space there (`x is ( < 5)`, `a is [ < 5, _]`) is removed,
as `dotnet format` does. The one place `dotnet format` keeps a space is a positional pattern with no
type name in front, such as `t is (< 5, > 9)`, which it writes as `t is ( < 5, > 9)`; `dotnet-fast`
does the same.

The two shapes in the original report, `foreach (var v in (values ?? []).Where(...))` and
`day is not (DayOfWeek.Saturday or DayOfWeek.Sunday)`, were already left alone since 1.10.5.

## 1.10.6 — 2026-10-03

### Fixed: tight `>=` and `<=` being split into `> =` and breaking the build (#326)

`whitespace`, `format` and `lint --fix` split a tight `>=` or `<=` into `> =` when the character
before it was not a plain letter, digit or closing bracket, or the operand after it started with
`.5`. The result did not compile (`CS1525`), and `--verify-no-changes` reported clean code as dirty.
1.10.4 and 1.10.5 both did this. For example:

- `o as Gen<int> >= 1` became `o as Gen<int> > = 1`;
- the list pattern `arr is [>= 1, <= 2]` became `arr is [> = 1, <= 2]`;
- `o as int?>=1` became `o as int?> = 1`, and `x>=.5` became `x> = .5`.

A `>=` or `<=` is now always kept as one operator. Where `dotnet format` spaces it, so does
`dotnet-fast` (`o as int? >= 1`, `x >= .5`), and the same tight forms are still reported by
`whitespace --verify-no-changes` and `lint`. A non-ASCII name before the operator (`café >= 1`) no
longer stops the run in the `none` and `ignore` spacing modes.

One shape is unchanged from 1.10.x and still differs from `dotnet format`: the tight
`o as Gen<int>>= 1` becomes `o as Gen<int >>= 1`, which builds, where `dotnet format` writes
`o as Gen<int> >= 1`. Changing it would bring back the #315 build break on `a < b ? x >>>= 1 : x`.

### Fixed: spacing next to a non-ASCII name

A `+` or `-` with a non-ASCII name on either side (`ρ + x`, `x - café`, `(double)ρ + 1.5`) was read
as a sign, not an operator, and lost its spaces: `ρ + x` became `ρ+x`, and a tight `ρ+x` was not
reported, where `dotnet format` leaves the first alone and spaces the second. 1.10.4 and 1.10.5 did
this under the default spacing. Under `none` and `ignore` they stopped the run on these lines instead
(see #326 above), so with that fixed the same rewrite reached those modes too, and under `none`
`static double ρ = 1;` became `static double ρ= 1;`. All of these now match `dotnet format`, and a
wrapped line that starts with `+` or `-` after a line ending in a non-ASCII name keeps its space.
Other operators next to a non-ASCII name are still not always spaced the way `dotnet format` spaces
them (a tight `ρ*x` is left as written, for example); the code builds and means the same either way.

### Fixed: `lint --fix` and IDE0047 removing a tuple's parentheses (#317)

These shapes are valid tuples, but the parser reads the `<` ... `>` in them as a generic. Removing the
parentheses breaks the build (`CS1002`/`CS1513`), and `dotnet format`, StyleCop and Roslynator leave
them alone:

- IDE0047 (`format`, `style`) removed the parentheses of `(a < b + 1, c > (d))`,
  `(a < this.c, d > (b))`, `(a < b, (c) > (d))`, `(a < base.GetHashCode(), d > (b))`,
  `(a < int.MaxValue, b > (a))`, `(o is T0<xq, c >>> d)` and `(o is T0 < xq, c > .5)`. 1.10.4 and
  1.10.5 both did this.
- SA1119 and RCS1032 (`lint --fix`) removed them from `return (await t < b, b >> a);` and from most
  other `await` forms of it, and from `(a < this.c, d > (b))`. In `Use((await t < b, b >> a))` the
  fix would have turned one tuple argument into two arguments. The `await` shape was listed as an
  open issue in the 1.10.5 notes.

These parentheses are now left in place. IDE0047 still removes them when the text between `<` and `>`
can only be a type-argument list, and that now includes lists with comments in them
(`(Make2<int, string /* a > b */>())`), as 1.10.4 and `dotnet format` do.

### Fixed: SA1014/SA1015, SA1119 and IDE0047 findings that 1.10.5 lost (#316, #317)

The 1.10.5 guard against that misreading also hid findings that 1.10.4 reported and that StyleCop or
`dotnet format` agree with. These are reported (and fixed) again:

- SA1015 on an `is` pattern generic followed by a `>` operator, `o is Gen<int > > xb`, including a
  `global::`-qualified one, `o is global::N.Gen<int > > 1`. Both were listed as open in the 1.10.5
  notes.
- SA1015 on the inner brackets of a nested generic in a `foreach` header,
  `foreach (IF<Ctx<int >> f in xs)`. The outer brackets are still not checked, as in every release
  (see [Known limit: generic or comparison?](docs/support-matrix.md#known-limit-generic-or-comparison)).
- SA1119 on `(o is Gen<int> > xb)`.
- IDE0047 removing the parentheses around a type test written with a space before `<`, when the
  generic ends the parenthesized value: `(x is Dictionary <int, string>)`.

Some shapes 1.10.5 lost are still not reported. They are listed under Known limits below.

### Fixed: SA1014/SA1015 reporting comparisons as generics (#331)

In an argument list, `G2(a < b, c > d)`, `G3(a < b, c < d, e >> g)` and
`G4(a < b, c < d, e < f, g >>> g)` are comparisons, but 1.10.4 and 1.10.5 reported SA1014/SA1015 on
them and `lint --fix` rewrote them, where StyleCop reports nothing. The same applies to
`G2(a < b, c > -d)`, `G2(a < b, c > +d)` and the collection expression `[a < b, c > d]`. These lines
now get no SA1014/SA1015 finding and no fix. Real declarations next to them (`out List<int > v`,
deconstruction) are still reported, and so is a generic nested in such a chain whose `>` is followed
by `.` or `(` (`G2(a < b, Gen<int >.V > d)`). See
[Known limit: generic or comparison?](docs/support-matrix.md#known-limit-generic-or-comparison).

### Fixed: comparison chains that end in a shift by a literal (#323)

`a<b>>1`, `G2i(a<b, b>>>1)` and `G3(a<b, c<d, e>>>1)` were read as generics and left unspaced.
`dotnet format` spaces them as comparisons and shifts (`a < b >> 1`), and so does `dotnet-fast` now,
including tightening the spaced form under `csharp_space_around_binary_operators = none`. A chain
whose last operand is a name, a parenthesized expression or an element access (`a<b>>c`) is still
left as written; see the Known limit linked above.

### Fixed: function-pointer types made dirty by the formatter (#330)

`delegate*<int, void> f` became `delegate*<int, void > f`, and `delegate* managed<int, void>` became
`delegate * managed<int, void>` (the same for `unmanaged` and `unmanaged[Cdecl]`). Both are clean code
that `dotnet format` leaves alone; 1.10.4 and 1.10.5 changed them. They are now left alone, and
`delegate *<`, `delegate* <` and `delegate*managed` are spaced the way `dotnet format` spaces them.
Spacing inside the brackets (`delegate*< int, void >`) is still not normalized.

### Fixed: `lint --staged` checked the working tree, not what the commit records (#327)

`--staged` takes its changed lines from the staged copy of each file, but the check itself read the
file on disk. For a file that was only partly staged (some edits staged, others not) the two
disagreed, and the check could pass what it never looked at:

- an unstaged line inserted above the staged change moved the report off the staged line, so the
  staged finding was dropped and another one was pinned to the wrong line;
- a staged violation that was fixed again in the working tree (but not re-staged) passed the hook and
  was committed;
- a clean staged line with an unstaged violation on top blocked the commit;
- the F# hygiene rules exited 0 on a staged `FSH0001` once an unstaged line sat above it.

A partly staged file is now checked on its staged content, read through git's own checkout filters
(so a `core.autocrlf` checkout is not flagged for line endings it does not have). Its line numbers in
the report now refer to the staged content: what the commit records, which can differ from the line
numbers your editor shows until the unstaged edits are staged or stashed. Fully staged files are
checked exactly as before. In the partly staged cases we checked (unstaged edits above, below and on
top of the staged change), the report and exit code are now exactly what 1.10.5 gives for the same
repository once the working tree is reset to the index.

### Changed: `--staged --fix-changed-lines` skips the fix for a partly staged file and says so (#310)

Bounding a fix to the staged lines cannot be done safely while the file on disk has other, unstaged
edits: the staged line numbers do not line up with the working-tree file, and three attempts to map
them were backed out. So for a partly staged file, `lint --staged --fix --fix-changed-lines` (and its
`--diff` preview) now leaves the file and the index untouched and prints, on stderr, in every output
mode:

```
skipped fix for Target.cs: file has unstaged changes; stage or stash them and re-run
```

The exit code still reflects the staged content: `1` when what will be committed still has a finding in
the staged lines, `0` when it does not. `--sarif` still records those findings. Before, such a file
could get a fix on the wrong line, or none at all with exit 0, and a violation that existed only in the
working tree could be fixed and re-staged into a commit that never contained it. Fully staged files,
plain `--staged`, and `--staged --fix` without `--fix-changed-lines` are unchanged.

### Fixed: commit hooks checked the wrong index for `git commit -a`, `-i` and `--only` (#329)

For `git commit -a`, `git commit -i <path>` and `git commit <path>` / `--only`, git hands the
pre-commit hook a temporary index (`.git/index.lock` or `.git/next-index-*.lock`) holding what the
commit will record. `lint --staged` ignored it and read `.git/index`, so a violation the commit was
about to record could pass the hook, and `lint --staged --fix` could not re-stage into the commit's
index. The hook's index is now used whenever it lives in the repository's own git directory (anything
else is still ignored, as before). Plain `git commit` and linked worktrees are unchanged.

One git behavior to know: after `git commit --only <path>` (or `git commit <path>`), git restores the
index to what it held before the commit, so a fix the hook re-staged is in the commit but the index
still shows the pre-fix line until you stage it again.

### Fixed: staged files with non-ASCII names were not checked (#335)

`lint --staged` silently skipped a staged file whose name contains a non-ASCII character (`Café.cs`),
because git printed the name escaped. Such files are now checked.

### Fixed: `--fix-changed-lines` dropped a staged fix next to committed debt (#328)

When the full fix also changed a line of older, unrelated code directly above or below the staged line
(for example a committed `int debt = 3;;` right above a staged `int fixme = 2;;`), the two edits were
treated as one, the older line was outside the staged lines, and the whole fix was withheld: the
`--diff` preview showed nothing and exited 0. The staged line is now fixed on its own and the older
line is left byte-for-byte as committed. This only applies when each changed line is plainly the same
statement before and after its fix; reordered or merged lines are still withheld whole.

### Known limits for partly staged files

`--deep` and `--fantomas` still read the working tree under `--staged`, so for a partly staged file
they can report on unstaged lines (#339). A file that is staged and then deleted from the working tree
is not checked (#337). Plain `lint --staged --fix` on a partly staged F# file can still fix the wrong
line (#336). In each case, staging or stashing the unstaged edits first gives the exact result.

### Fixed: IDE0044 adding `readonly` where it breaks the build or copies a struct (#311, #325)

IDE0044 decided each field from the one file it was fixing. These shapes were marked `readonly` by
1.10.4 and 1.10.5, and `dotnet format` leaves all of them alone:

- A field passed to a `ref this` extension method (or a member of a C# 14 `extension(ref …)` block)
  declared in another file of the project, in another part of a `partial` static class, or in a
  `ProjectReference`'d project. The build then failed (`CS0192`). A call to such a method inside an
  interpolation hole, `$"{_f.Bump()}"`, was missed too, even with the method in the same file.
- `_f!++`, `(_f!)++` and similar writes through the null-forgiving `!` (`CS0191`).
- A write in a raw interpolated string nested in another with the same number of quotes,
  `$"""{$"""{_d++}"""}"""` (`CS0191`).
- A field whose type is a mutable struct: one declared in another file, a positional
  `record struct` or a struct with a primary constructor, or a BCL struct with public mutable state
  (`Vector2`, `Point`, `DictionaryEntry`, `HashCode` and others). This builds, but every member call on
  a `readonly` struct field works on a copy, so behavior can change.

In a project, solution or folder run, IDE0044 now reads every file of the project (and of the
projects it references) for `ref this` methods and mutable structs, before `--include`/`--exclude`
are applied, and an edit to such a sibling file invalidates cached verdicts. Matching is by name, so
an unrelated type or method that shares a name can also hold a field back. That only loses a
finding; it never breaks a build. Every change here only holds fields back, so on these shapes
`dotnet-fast` now reports fewer IDE0044 findings than 1.10.5, matching `dotnet format`. One trade-off is listed under
Known limits below: the text of raw and verbatim interpolated strings.

Some ways of writing a mutable struct in another file are still not recognised, so a field of that type
can still be marked `readonly`, as in 1.10.4 and 1.10.5: accessors on their own lines, more than one
member on a line, a member on the struct's header or opening-brace line, an array, collection or lambda
initializer on a struct field, a `using` alias for the struct, a nested struct written as
`Holder.Inner`, and `Nullable<Mut>` (`Mut?` is recognised). The build still succeeds, but member calls
on the field then work on a copy (#325, still open).

### Fixed: IDE0044 on structs that overwrite themselves as a whole

A struct that overwrites itself as a whole anywhere in its body (`this = default;`,
`this = other;`, `ref this`, `out this`, `ref S r = ref this;`, or `(this, o) = (o, this);`) gets no
IDE0044 on any of its fields from `dotnet format`. `dotnet-fast` now does the same. Before, it marked
such a field when the constructor that wrote it ran over several lines, so `--fix` added a `readonly`
that `dotnet format` would not.

### Fixed: IDE0044 build breaks from things that look like constructors

These were already in 1.10.4 and 1.10.5, and were found while fixing #324. IDE0044 treated any
line whose last name before `(` matched the type as a constructor, and trusted writes it should not
have. Each of these made `--fix` add `readonly` and the build then failed (`CS0191`/`CS0198`), while
`dotnet format` leaves all of them alone:

- a finalizer, `~C() { _n = 1; }`;
- a conversion operator, `public static implicit operator C(int x) { s_n = x; … }` (read as a static
  constructor);
- a property or field initializer wrapped onto several lines, `public C Next { get; } = new C(1) {` …;
- a write inside a local function that returns a value, `int L() { _n = 1; return 0; }`;
- a nested type's static constructor writing the outer type's static field;
- an object initializer of the same type inside a constructor, `new C(1) { _n = 1 }` or
  `new(1) { _n = 1 }`.

None of these count as a constructor write any more.

A related break was found on a real repository (Polly's `NonSlidingTtl`). When an expression-bodied
member wraps onto a second line, `public C(int n) =>` then `this.n = n;`, that second line was read as
a field declaration. `readonly` was put in front of the statement (`CS1525`), or, when the member was
an ordinary method, the write was ignored and the field it assigns was marked (`CS0191`). That line is
now read as the assignment it is.

### By design: IDE0044 and `partial` types (#321)

No change in behavior: IDE0044 still leaves every field of a `partial` type alone, as it has since
1.10.4. On `partial` types `dotnet-fast` therefore reports fewer IDE0044 findings than `dotnet format`,
and `style --verify-no-changes` can exit 0 where `dotnet format style --verify-no-changes` exits 2.
This is now listed under Known limits in the docs.

### Known limits: findings left out on purpose, to avoid breaking builds

Each of these is a finding `dotnet format`, StyleCop or Roslynator reports and 1.10.4 reported, which
this release does not. In each case the code builds either way: `dotnet-fast` reports less, it does
not rewrite anything wrongly. Reporting them would need a compiler to tell the shape apart from one
where the same fix breaks the build, so they are documented rather than guessed at. All are in the
docs' Known limits sections.

- **IDE0044 and the text of raw or verbatim interpolated strings.** A field name followed by a write
  (`_e++`, `_e = 2`) anywhere in a raw (`$"""…"""`, `$$"""…"""`) or verbatim (`$@"…"`) interpolated
  string that has a hole stops IDE0044 from marking that field, even when the name is only literal
  text. Telling literal text from holes in these strings needs a full C# lexer, and when the write
  really is in a hole (`$"""{$"""{_d++}"""}"""`), adding `readonly` breaks the build (`CS0191`), as
  1.10.4 and 1.10.5 did. This now also covers nested strings, so these lose the finding compared
  with 1.10.5: `$"""{$""" _e++ {1}"""}"""`, `$"""{$"""{1} _e++"""}"""`,
  `$"""{$"""{1}"""} _e++"""`, `$"""{$""" _e = 2 {1}"""}"""` and
  `$$"""{{$$""" _e++ {{1}}"""}}"""`. Two more, `$""""{$""" _e++ {1}"""}""""` and
  `$@"{$@" _e++ {1}"}"`, were already left alone by 1.10.5. Plain `$"…"` strings are read exactly,
  so `$"{$" _e++ {1}"}"` is still marked.
- **IDE0044 and structs with a mutable field inside `#if`.** A field whose type is a struct that
  declares a mutable field in an `#if` branch is left un-`readonly`, even when that branch is not
  compiled in a Debug build: `#if RELEASE` / `public int Z;` / `#endif`, `#if !DEBUG` with a
  `readonly` field under `#else`, or a symbol you define elsewhere. `dotnet format` judges only the
  build it loads, and 1.10.4 and 1.10.5 marked these fields `readonly` too. `dotnet-fast` does not know
  which symbols your other builds define, and in a build where that field exists, `readonly` makes
  every member call on the field work on a copy, so behavior can change. When the mutable field is
  in an `#if DEBUG` branch, `dotnet format` and 1.10.x leave the field alone as well.
- **SA1119 and RCS1032 on a comparison with a shift on its right.** `f = (a < b >> 1);`,
  `f2 = (a < b >>> 1);`, `Use2((a < b >> 1), c > d);` and the inner pair of `((a < b >> 1))` get no
  finding (the outer pair of `((…))` still does). StyleCop and Roslynator report them, and so did
  1.10.4. 1.10.5 already did not. The parser reads `a < b >> 1` as a generic `a<b>` followed by
  `> 1`, the same shape it gives the tuple `(c < d < xb, d >>> 1)`, whose parentheses cannot be
  removed. A fix that told the two apart was tried for this release and backed out, because it
  still broke the build on tuples like that one.
- **IDE0047 on a type test with a space before `<` that is followed by more of the expression.**
  `(x is Dictionary <int, string> and not null)` and `(x as Dictionary <int, string> ?? null)` keep
  their parentheses, where 1.10.4 and `dotnet format` remove them. 1.10.5 already kept them. The
  same check that would allow these also removed the parentheses of valid tuples such as
  `(o is T0 < xq, string.Empty.Length > d)` and broke the build, so it was backed out. When the
  generic ends the parenthesized value, `(x is Dictionary <int, string>)`, the parentheses are
  removed again.

## 1.10.5 — 2026-10-01

### Fixed: `List<int>` getting spaced apart to `List < int>` before `&& || & | ^ == != }` (#318)

The generic-vs-less-than scanner only accepted a narrow set of bytes as proof that a `>` really
closes a generic type-argument list (letters, `( [ . : { , ; > ) ] ?` and `=>`). The C# spec's full
disambiguation follower set is wider — `( ) ] } : ; , . ? == != | ^ && || & [` — so a binary operator
or `}` directly after the close fell outside it, the scanner gave up on the pairing, and the opening
`<` fell through to the relational-operator path. `is X<Y<Z<int>>> &&` and plain `List<int>&&ok`
alike got `<`/`>` spaced apart as if they were comparisons. A follow-up repair in the same release
makes a tight `&`/`|`/`^` right after the now-correctly-paired close get spaced: `List<int>&ok` now
becomes `List<int> & ok`, matching `dotnet format`. The first fix had left that line unflagged
entirely. 1.10.4 did flag it, but rewrote it wrongly to `List < int>&ok`, so this is new behavior,
not a restored one. Both confirmed against real `dotnet format` and the released 1.10.4 binary.

### Fixed: a block comment losing its space before a `none`-mode operator (#319)

With `csharp_space_around_binary_operators = none`, the whitespace run between a block comment's
`*/` and a following binary or compound operator was trimmed unconditionally — `x /* c */   >>>=1`
became `x /* c */>>>=1`. The `none`-mode code path never got the same block-comment guard its
`BeforeAndAfter` sibling already had (#201); #315's new `>>>`/`>>>=` handling in 1.10.4 went through
the same unguarded path, which is why those two operators regressed in 1.10.4 specifically. The
matching unary-operator path (`++`/`--`/unary `-`/`+`) had the same bug in every spacing mode,
including the default one, where a multi-space or tab run before the operator was being collapsed
to a single space instead of preserved as `dotnet format` does; fixed alongside it.

### Fixed: a relational `<` reading through a `)` as a generic open (#322)

`if (a<b) a>>=2;` and the same shape with `>>>=`, `>>`, or `>>>` got half-formatted: the shift
operator was spaced correctly but the `<` guarding it was not, because the shared
generic-vs-comparison scanner read straight through the `if`'s closing `)` and treated the
following shift run as a nested generic close. `whitespace --verify-no-changes` reported the
half-formatted file as clean where real `dotnet format` exits 2. Fixed by tracking paren/bracket
balance from the candidate `<` — a type-argument list can never contain an unmatched `)`/`]`.
Two cosmetic gaps remain, both also present in 1.10.4 and non-breaking: a few constructs without an
enclosing `)` (`G2(a<b, b>>1)`, a `?:` operand, wrapped multi-line calls) still don't get the same
spacing `dotnet format` gives them, and a function-pointer type (`delegate*<List<int>, void>`,
`delegate* managed<`) gets mis-spaced in a way that makes oracle-clean code dirty — tracked
separately, not part of this fix's scope.

### Fixed: SA1014/SA1015 no longer rewrite a misparsed generic-vs-shift/comparison into a build break (#316)

`tree-sitter-c-sharp` always prefers a generic parse for `a < b`, so `a < b >>> 1` or `a < b >= 1`
comes out of the grammar as a closed generic followed by a shift or comparison — a shape Roslyn's
disambiguation rule never actually produces (a `>`-led token can never legally follow a real generic
close there). SA1014/SA1015 trusted that parse at face value and `lint --fix` split the trailing
`>>>`/`>>`/`>=` apart, a build break (`CS1525`) that real `dotnet format` and StyleCop.Analyzers
1.1.118 both leave alone. Fixed across three passes, the last of which stopped the exemption from
also swallowing a *real* generic that happens to sit in an `as`/`is` type position followed by its
own unrelated `>`-led operator (`o as Gen<int> > 1`), which had started silently dropping genuine
findings 1.10.4 still caught.

Known, not yet closed, and newly visible now that this guard ships: two narrower shapes still lose
real SA1014/SA1015 findings that 1.10.4, StyleCop, and a real build all confirm — an `is`-pattern
generic followed by a `>`-led operator where the user's own type overloads `operator >` (`o is
Gen<int> > xb`), and a `global::`-qualified generic after `as` (`o as global::GG<int> > 1`). Neither
one risks a build break (the parens/guard direction here is "don't rewrite," not "rewrite wrong"),
so withholding was the safe call; tracked in #316, which stays open. A long-standing, unrelated false
positive on nested shift comparisons without a preceding `<` (`G3(a < b, c < d, e >> g)`) is also
still open and was not made worse by this fix.

### Fixed: RCS1032/SA1119 no longer deletes a tuple's own comma on a generic-shaped misparse (#317)

The same grammar misparse above also hits a parenthesized tuple whose element starts with `>>`,
`>>>`, or `>=` (`(a < b, b >> a)`): `tree-sitter-c-sharp` reads it as a fake generic-fronted
binary/assignment expression inside otherwise-redundant parens. SA1119 (and the report-only RCS1032)
then "fixed" it by deleting the tuple's own comma — `CS1002`/`CS1513` — reproduced on both HEAD and
the released 1.10.4 binary. Fixed across three passes, the last of which replaced an ad hoc list of
exempted shapes with a structural, recursive check so any depth of wrapping (nested binary/assignment,
a cast, a unary over a deeper member access) is caught the same way; also widened IDE0047's own
unrelated generic-argument carve-out (same misparse family) to recognise a qualified type name,
`global::`, and `is not`, so it stopped misreading those as a tuple and losing true positives of its
own.

Known, not yet closed: an `await` operand feeding the misparse (`return (await t < b, b >> a);`)
still breaks the build the same way under `lint --fix`, and a lambda `=>` or a `> .5` literal sharing
the line can still mask the real tuple comma from IDE0047's formatter path (`return (a < b, () => {
});`) — both pre-existing in 1.10.4, not new here. IDE0047 also still leaves some redundant parens in
place that 1.10.4 and `dotnet format` strip — a pattern-combinator operand (`x is null or
Dictionary<int, string>`), a generic nested inside a qualified name, an `@`-prefixed identifier, or
whitespace before `<` all still fall outside its carve-out. None of these is a build break or dirties
otherwise-clean code; #317 stays open for them.

### Fixed: more IDE0044 write shapes that were still getting marked `readonly`, safely (#311)

Following on from #302 and the 1.10.3/1.10.4 passes: `IDE0044 --fix` no longer marks a field
`readonly` when it's written through a `this`-qualified receiver wrapped in parentheses
(`(this._n).Bump()`), through a `checked`/`unchecked` block or expression wrapping the write, or
through a same-file `ref this` method or C# 14 `extension(ref int v)` block whose receiver is
parenthesised or qualifier-wrapped (`(this._f).BX01()`, `(_f).Trim()`). These were real build breaks
(`CS0191`/`CS0192`) in 1.10.4.

Known, not yet fixed, already present in 1.10.4 (not a regression): a `ref`-receiver extension or
`extension(ref int v)` block declared in a *sibling* file, the raw-string form of the nested
interpolated-string-hole write shape where the inner and outer raw strings use the same number of
quotes, and `++`/`--` applied to a field through the null-forgiving `!` operator (`_f!++;`,
`(_f!)++;`). #311 stays open until these are closed.

### Investigated, not fixed: IDE0044 and multi-file `partial` declarations (#321)

1.10.4 stopped marking any field of a `partial` type `readonly` at all, to avoid missing a write in a
sibling file. A narrower fix — recognise a sibling reliably, so a plain partial class with no real
sibling write could go back to being flagged, matching `dotnet format` — was attempted and backed out
after verification found it still misses several real sibling-header shapes: the keyword and type
name split across two lines, two type headers on the same source line, a header preceded by a block
comment, and a header following a UTF-8 BOM (the Visual Studio default for new files). Each of those
still breaks the build (`CS0191`) exactly the way 1.10.4's blanket holdback exists to prevent. No
behavior changed in this release for #321; the holdback stays as broad as it was in 1.10.4.

### Investigated, not fixed: `--staged --fix-changed-lines` on a partially-staged file, continued (#310)

A further repair to the staged/working-tree line-mapping algorithm was attempted and backed out
after verification found a new way it can still misplace a fix: on a file with duplicate line text,
the mapper's tie-break can land a fix on a *committed* line that was never staged, while silently
letting the actual staged violation through uncorrected and re-staging it. One of the two repro
shapes is a confirmed regression against 1.10.4 (a real violation gets committed where 1.10.4 left
the file alone). No behavior changed in this release for `--staged --fix-changed-lines`; #310 stays
open.

## 1.10.4 — 2026-09-29

### Fixed: the C# 11 `>>>=`/`>>>` operators splitting apart and breaking the build (#315)

`whitespace` and `lint --fix` split the unsigned-right-shift-assignment operator `>>>=` into
`> >>=`, producing code that doesn't build (`CS1525`). 1.10.3 and 1.10.2 both did this; real
`dotnet format` leaves the operator alone. The operator tokenizer had spacing rules for `>>=`,
`>>`, and `>=`, but nothing for the newer 3- and 4-character tokens, so the first `>` was left
untouched and the second one matched the `>>=` rule and got a space forced in front of it. (Plain
`>>>` was never split, just never spaced: `a>>>3` stayed tight where `dotnet format` writes
`a >>> 3`.) Fixed: `>>>=` and `>>>` are now recognised ahead of the older shift/comparison rules,
checked against real `dotnet format` (SDK 10.0.303) and a real build across plain, chained,
parenthesized, and declaration/operator-body shapes, in default, `none`, and `ignore` spacing
modes — and against generic closers like `List<List<List<int>>>`, so a triple-nested generic close
is never misread as the new `>>>` operator.

Two known leftovers, both compile fine. With `csharp_space_around_binary_operators = none`, a
block comment right before `>>>`/`>>>=` now gets tightened against it (`x /* c */ >>>=1;` becomes
`x /* c */>>>=1;`). `dotnet format` and 1.10.3 leave that line alone, so for these two operators
it is new in 1.10.4, although it matches what 1.10.3 already did for every other binary/compound
operator (#319). And a `<` comparison earlier on the same line as `>>=`/`>>>=` keeps whatever
spacing it had (`if (a<b) a>>>=2;` becomes `if (a<b) a >>>= 2;`, where `dotnet format` also spaces
the `<`). That already happened with `>>=` in 1.10.3 (#322).

### Fixed: more IDE0044 write shapes that were still getting marked `readonly`, safely (#311)

Following on from #302 and the 1.10.3 fixes: `IDE0044 --fix` no longer marks a field `readonly`
when it's written through a write target wrapped in any number of redundant parentheses
(`(_n)++`, `((_n))++`, `(this._n)++`, `PutR(ref ((_n)))`), through `ref` with a `//` line
comment between it and the field, as a deconstruction target nested more than one level deep
(`((a, b), (c, _n)) = ((1, 2), (3, 4))`), or through a same-file `ref this` extension-method
call (`_n.Bump()`, `(_n).Bump()`), whether the extension is declared `ref this`, `this ref`,
`scoped ref this`, or generic. These were all real build breaks (`CS0191`/`CS0192`) in 1.10.3.
Two related changes. A field whose type is a mutable struct is no longer marked `readonly`, which
matches `dotnet format`: marking it builds, but it quietly forces a defensive copy on every access.
And every field of a `partial` type is now left alone, because a sibling part in another file or a
source generator can write it (1.10.3 broke the build in those cases). The cost is that a plain
single-file partial class is no longer flagged either, although `dotnet format` and 1.10.3 flag it
(#321).

Known, not yet fixed, and already present in 1.10.3, so not a regression: a `this`-qualified
receiver in parens (`(this._n).Bump()`, `((this._n)).Bump()`, `((this)._n).Bump()`), a comment or
line break inside the receiver's parens, the null-forgiving `_n!.Bump()` form, a `ref this`
declaration with an attribute on the receiver parameter or a comment between `ref` and `this`, a
`ref this` declaration in a sibling file, a write inside a nested interpolated-string hole
(`$"{$"{_n++}"}"` and its raw/verbatim variants), and the C# 14 `extension(ref int v)` block
(`dotnet format` breaks that one too). Tracked in #311, which stays open until these are closed.

### Investigated, not fixed: `--staged --fix-changed-lines` on a partially-staged file (#310)

When a file has both staged and unstaged changes, `--staged --fix-changed-lines` can still aim a
staged line's fix at the wrong working-tree line and silently skip it. This is unchanged from
1.10.3. A fix landed and was reverted after verification found it traded one bug for another: when the
same unstaged hunk both inserts a line above a staged line and edits that staged line, the line
remap can land on the newly inserted line instead of the staged one, silently dropping the fix and
turning a real `exit 1` into a false-clean `exit 0`. No behavior changed in this release for
`--staged --fix-changed-lines`; #310 stays open.

## 1.10.3 — 2026-09-28

### Fixed: keyword-operand parens/brackets losing their space, `case [1]:` included (#306, #307, #313)

`WHITESPACE`/SA1008/SA1010/SA1011 disagreed with `dotnet format` on a parenthesized or bracketed
operand right after a pattern/statement keyword — `params (T, T)[]`, `ref (x)`, `is (0, 0)`,
`as (int, int)?`, `case [1]:`, `o is (int) or (long)`, `ref (arr)[0]`, `foreach (var x in (xs)[0..1])`
and several nested-pattern-comma shapes all got mis-spaced, and `lint --fix` could rewrite
oracle-clean code into a shape `dotnet format whitespace` then rejected. The fix reads the syntax
tree instead of guessing from token text, so a method literally named `scoped` or `and` is never
mistaken for the keyword (a *variable* named `and`/`not` followed by an indexer is a separate,
pre-existing gap, listed below). A case label's own `[` / `]` — `case [.. var r]:`, a list
pattern — is included: `lint --fix` used to delete the space `case ` needs, and a case-label colon
right after a `]` (`case [1]:`) is now correctly left as a report-only finding instead of being
silently dropped by the older #202 grammar-gap filter, which was found to be dropping it (a
regression caught in the same pass, never shipped). A narrower nullable-array shape the #202 filter
correctly drops (`string[]?` inside a C# 14 `extension` block) was checked to make sure the fix for
the case-label gap didn't resurrect it. Out of scope, unrelated pre-existing gaps confirmed present
and unaffected on both the released 1.10.2 binary and this build: `async(1)`/`var(1)` calls and a
method named `when` getting spaced, `when (` under `csharp_space_after_keywords_in_control_flow_statements
= false`, `is (> 0` inner space, `*(p)` becoming `* (p)`, `and[0]`/`not[0]` indexers, SA1008 missing
`nameof (a)`, and SA1008's `in(int, int) t` false positive. Also confirmed still present, improved but
not eliminated: `and not`/`or not` before a parenthesized predefined type, when a parenthesized type
comes before it (`o is (int) and not (long)`), still loses its space — 1.10.2 mis-spaced both sides of
that shape (`is not(int) and not(long)`); this release fixes the first sign and leaves only the second.

### Fixed: a cast followed by a unary sign misread as binary arithmetic (#312)

`DF0139`/`SA1021`/`SA1022` trusted a `tree-sitter-c-sharp` misparse at face value: `(v) + 1` always
parses as a cast of `v` whose operand is a spaced unary `+1`, whatever `v` is — but real Roslyn only
agrees when `v` is a shape its parser can resolve as a type purely syntactically (a predefined type
keyword, `global::`-alias-qualified name, or a nullable/array/pointer suffix — never a bare
identifier, dotted name, generic, or a local named `nint`/`nuint`). `lint --fix` rewrote `(v) + 1` to
`(v) +1`, which `dotnet format whitespace` then rejected, and `(v) + +1` to `(v) ++1`, which doesn't
build (CS1002). Both are fixed: the misread is withheld everywhere real Roslyn would read it as
binary, and the fix additionally checks a real double-sign fusion hazard (`- -w` staying `- -w`, not
becoming `--w`) without over-applying that check to a *mixed* sign pair (`- +w`/`+ -w`, which can
never fuse and the oracle wants tight) — an over-broad first version of that guard was caught in the
same pass and never shipped. A cast's own unary-sign operand inside a string interpolation hole
(`$"{(int) -y}"`, `$"{(global::Foo) -y}"`) is also now spaced correctly under
`csharp_space_after_cast`; that spacing decision had nowhere else to be made inside a hole and was
silently wrong before.

### Fixed: IDE0044 marking more written fields `readonly` than it should, safely (#311)

Following on from #302: IDE0044 now correctly leaves several previously-mis-marked write shapes
alone — a `this.`-qualified assignment, a prefix `--`, a tuple-deconstruction target, a `ref`/`out`
argument, a `ref` local alias, and an assignment inside a verbatim or raw string interpolation hole
are all now recognised as real writes, so the field they touch no longer gets a `readonly` that
breaks the build (CS0191/CS0192). A regression introduced by this same work and caught before it
shipped: a generic, parameterless, expression-bodied method (`private static T Make<T>() => default;`)
could be misread as an unwritten field named after its own type parameter and get `readonly` inserted
before its return type (CS0106) — fixed alongside it. Known, still not fixed, all pre-existing and
present in 1.10.2 too (not caused by this release): a write through redundant parentheses around the
field (`(_n)++`, `((_n)) = 4`), a nested or nested-with-redundant-parens deconstruction target, a
`ref` argument split across a line by a `//` comment, and a write via a `ref this` extension method.
Also pre-existing: a field only ever written through a mutable struct method call
(`_p.Bump()` on a non-readonly struct field) still gets `readonly`, which builds but silently changes
behavior (the call now runs on a defensive copy) — `dotnet format` leaves it alone.

Known issue, not IDE0044 and not new (1.10.2 does the same): the whitespace pass splits the C# 11
`>>>=` operator into `> >>=`, which doesn't compile. `whitespace` and `lint --fix` both do it, whether
or not IDE0044 is enabled, and `lint` reports a `WHITESPACE` finding on correct `>>>=` code; under `lint --fix` the broken statement then also stops counting as a
write, so the field gets `readonly` too. IDE0044 on its own (`style --diagnostics IDE0044`) handles
`>>>=` correctly in this release. Until this is fixed, exclude files that use `>>>=` from `--fix`.

### Fixed: DF0044 could fold a constant ternary into a different type (#309)

`true ? a : b` / `false ? a : b` has a constant condition, so one branch never runs — but the dead
branch still takes part in deciding the whole expression's type: the best common type, nullable
lifting, the typing of `null`/`default`/`throw`, and overload resolution can all depend on both arms,
not just the taken one. `--fix` folded straight to the taken arm's raw text regardless, which could
silently change what the expression evaluates to, or its static type, while the file still built with
zero errors — for example `true ? 1 : 2.0` folded to `1`, turning a `double` result into an `int` one,
and `M(true ? 1 : 2.0)` changed which overload of `M` got called. Some shapes went further and broke
the build outright (`true ? null : "s"`, `true ? default : 5`, a ternary with a `throw` arm). The fold
now applies only when both arms are literals whose types can be proven identical from the syntax
alone (`1 : 2`, `1.5f : 2F`, `"a"u8 : "b"u8`, and so on); measuring the fix turned up two further traps
along the way that go beyond the fold's original design — the C# integer-literal size rules mean two
unsuffixed integer literals of the same *kind* can still be different types (`1 : 3000000000` is
`int`/`uint`), and a declared target type doesn't make a narrowing fold safe either (folding away a
`float` arm can lose precision a wider target type would have kept). Everything else — identifiers,
calls, `null`, `default`, `throw`, casts, a unary `-`, `ref` expressions (the pre-existing #308 case),
lambdas, interpolated strings — is now report-only: the finding still fires, but `--fix` leaves it
alone.

## 1.10.2 — 2026-09-27

### Fixed: a killed run no longer blocks the next runs for up to five minutes (#304)

The write lock that keeps two concurrent runs from formatting the same file at once was just a file,
removed when the run finished. A run that was killed, ran out of memory or crashed mid-fix never removed
it, and the next runs could not tell a dead owner from a live one. They only treated the lock as stale
once it was more than 300 seconds old. Until then, every run that needed that file waited out its full
30-second lock budget and then failed. The same age-only rule also meant a lock still held by a live run
could be taken over once it passed 300 seconds. The lock is now tied to a real OS-level file lock
(`flock`/`LockFileEx`). The OS releases it the moment the owning process dies, so ownership is checked
directly instead of estimated from a timestamp. A
follow-up in the same fix closed a narrower gap it left: two writers could still both take a *newly
created, not-yet-locked* lock file in the same instant. Known, deliberate residuals: don't run 1.10.1
and 1.10.2 against the same tree at the same time (1.10.1 never takes the OS lock, so a live 1.10.1
lock can be taken over once it looks older than 200ms to the new build); a killed run can still leave a
harmless stray lock or temp file next to an already-correctly-formatted file; and a filesystem or share
with a badly skewed clock can still cause extra lock retries (never a double write) under the new check.

### Fixed: very deep nesting could overflow the stack instead of reporting cleanly (#305)

`lint` and `metrics` both walk a file's syntax tree recursively. A file nested many thousands of levels
deep could overflow the stack and crash instead of failing cleanly. The issue's nested-parentheses
repro crashed a release build at around 10,000–20,000 levels. That's the kind of file a generator or a
pathological formatter loop can produce, not something anyone writes by hand. Both commands now stop
walking past 3000 levels and report a clean `TOODEEP` finding / `overBudget` result instead of
crashing — deep files that were fine before
this depth still measure exactly as before. A first pass at the cap fired too early and silently
dropped findings on ordinary, non-pathological files; that's fixed, so normal files are unaffected
either way. Contract note: a file over the depth cap that previously produced a plain (if slow) `metrics`
result now gets `overBudget: true` and `lint` exits 1 with `TOODEEP` for that file, which is the
intended clean failure, not silent success. A separate, pre-existing performance issue — `lint` time
grows very steeply with ordinary nested blocks (if/lock/try/local functions), independent of this cap,
and was already present in 1.10.1 — is a known, separate problem; it is not part of this fix.

### Fixed: `lint --fix` could still touch code inside a broken extension/union block (#202)

The bulk of #202 (extension-member false positives and modifier/body mistakes) shipped in 1.10.1. One
gap remained: when a parse error orphaned an extension or union block from its containing class, its
own withhold didn't apply and `lint --fix` could still rewrite code inside it. That gap is now closed.
Some report-only false positives on valid C# 14 extension-member code (a stray-`;`-looking analyzer
firing on an expression-bodied member, SA1008 on a generic extension header) are still reported, never
fixed, and are unchanged from 1.10.1 — see the issue for the list.

### Fixed: DF0044 could rewrite a `ref` conditional into code that doesn't build (#308)

`lint --fix`'s DF0044 (`?:` operand style) could fold a `ref` conditional such as
`ref int r = ref (true ? ref v : ref v);` into a form that fails to build (CS1525). The fix is now
withheld on every `ref` conditional; DF0044 still reports it as a finding, it just doesn't rewrite it.
Two separate, pre-existing problems found in the same pass are not part of this fix: DF0044 can fold a
ternary whose arms have different types (`true ? 1 : 2.0`) and change the result's type (#309), and
DF0139/SA1022 misread `(v) + 1` as a cast followed by unary plus and rewrite it to `(v) +1`. That
rewrite builds and behaves the same, but `dotnet format whitespace` then puts the space back (#312).

### Fixed: three more shapes where IDE0005/IDE0032/IDE0044 could break a build (#302)

Following on from the disabled-`#if`-branch work already in earlier releases: a field or `using` only
referenced from inside a disabled `#if` branch, or only inside a raw/verbatim string interpolation
hole, could still be wrongly removed by IDE0005/IDE0032, breaking the build. Both shapes are now
withheld. IDE0044 gets the disabled-branch withhold for a field assigned there with `=`, a compound
assignment or `++`/`--`, so it no longer marks that field `readonly`. A `//` comment before the
assignment could defeat that IDE0044 check outright; that's fixed too. Known, not yet fixed
(#311): IDE0044 still marks a field `readonly` when its only writes are `this._n = …`, prefix `--`, a
tuple-deconstruction target, a `ref`/`out` argument, a `ref` local, or an assignment inside a verbatim
or raw interpolation hole. This happens in ordinary active code, not only in disabled branches, and it
breaks the build (CS0191/CS0192). It predates this release and was present in 1.10.1 too.

### Fixed: `--fix-changed-lines` could duplicate a moved or re-indented line (#292)

A line that moved (or only changed indentation) inside a changed hunk could be written out twice —
once at its old position and once at its new one. Fixed. A pre-existing, separate nondeterminism in
plain `lint --fix` across large multi-file runs (also present in 1.10.1) was found while investigating
this. It isn't caused or changed by this fix. Also separate and not fixed here: `--staged
--fix-changed-lines` can compute its line scope from the wrong line numbers when a file is only
partially staged (#310).

### Fixed: a CST guard for collection expressions vs. patterns, without the regression 1.10.1 pulled back (#281)

1.10.1 shipped one part of #281 (initializer-continuation indentation) but withdrew a second part —
telling a multi-line collection expression `[ … ]` apart from a list/positional/property pattern —
after it turned out to misread real patterns. That guard is back, built on the actual syntax tree
instead of guessing from brackets, and a regression it introduced against 1.10.1's close-brace
splitting has been fixed alongside it. Known leftovers, byte-identical to 1.10.1 and not new in this
release: a few collection expressions next to a list pattern still have multi-space runs collapsed
where `dotnet format` keeps them. These are a list pattern followed by `&&`, one inside a ternary, and
one followed by a `when` clause with a collection argument. There are also some layout differences
from `dotnet format`: splitting `new X { Items = [` onto its own line, and `} else {` inside a lambda.
One line shape does change from 1.10.1. A literal tab inside a collection expression's interior is now
kept verbatim, where `dotnet format` turns it into spaces. 1.10.1 was also wrong here, in a different
way: it collapsed the run but kept a leading tab. See the issue for the full list.

## 1.10.1 — 2026-09-25

### Fixed: an initializer continuation that starts with `=` keeps its indentation (#281)

A property or field initializer whose continuation line begins with `=` (for example a `Regex` built
across several lines, as in Dapper's `CompiledRegex.cs`) was snapped back to member depth. `dotnet format`
keeps the author's indentation there, so a repository gating on both tools disagreed. It now matches.

Not in this release: a companion change for multi-line collection-expression `[ … ]` spacing (seen on
Polly's `ResiliencePipelineRegistry.cs`) was developed after `1.10.0` but withdrawn before release,
because it also misread list, positional and property patterns and left spacing that `dotnet format`
collapses. No released version ever carried it: collection-expression spacing behaves exactly as in
`1.10.0`. It remains open in #281.

### Fixed: the build cache's restored NuGet state was not actually portable across agents (#241)

The build cache archived `project.assets.json` and the generated `g.props` unchanged and called the
result portable across machines. It wasn't: `project.assets.json` bakes the producing agent's literal
absolute package-folder path into fields the SDK reads directly, so extracting a producer's `obj/`
onto a consumer with a different resolved package root made `dotnet build --no-restore` fail with
`NETSDK1064` — a false "already restored" that turned into a hard build failure instead of a cache
miss. The producer now records its package root as a small sidecar alongside the archive (the
archived files themselves stay byte-identical) and the consumer rewrites just that one root on
extraction when its own differs, or withholds the restore-state files entirely when it can't prove the
rewrite is safe — withholding just means a real `dotnet restore` runs, not a new silent no-op. A
follow-up fix closes a gap in the first one: a producer with a NuGet.config fallback packages folder
configured was treated as "unknown" and had its restore state withheld even on a matching agent;
only the SDK's own first (resolved) `packageFolders` entry is recorded and rewritten, any fallback
folder is left exactly as archived. **The build-cache format version moved (v2 → v3)**, so every
consumer does one cold rebuild on its next cache use — same as any prior cache-format change.

### GitHub releases now attach the linux-x64 binary too (#301)

Since `1.10.0`, the NuGet package carries a `linux-x64` native binary alongside `win-x64`. The GitHub
release (both here and on the public mirror) only ever attached the Windows binary, so anyone fetching
the standalone executable directly — rather than through the .NET tool — got Windows only, with no
way to get the Linux build outside of NuGet. Each release now attaches `dotnet-fast-linux-x64` and its
`.sha256` next to the existing `dotnet-fast-win-x64.exe` and `.sha256`; nothing about the Windows asset
changed. See [security.md](docs/security.md#the-standalone-binary) for how to check it, and
[support-matrix.md](docs/support-matrix.md#platform) for what "ships" vs. "verified" means for Linux.

### Fixed: deeply nested single-line interpolation could crash instead of erroring (#297)

A single-line interpolated string nested roughly 200+ levels deep (`$"{$"{$"{…}"}"}"`) could overflow
the stack and crash the process instead of producing a clean error. Three separate passes each followed
that nesting with no depth limit: the whitespace spacer, the explicit-tuple-name rewrite (IDE0033), and
the deconstructed-variable-declaration rewrite (IDE0042). All three now share the same nesting cap the
string-literal scanners already used (issue #294): past 16 levels deep, each pass does the conservative,
source-preserving thing instead of recursing again — the whitespace spacer leaves that portion of the
hole unformatted, the tuple-name rewrite leaves a `.ItemN` access unrenamed, and the deconstruction
rewrite withholds the whole declaration rather than risk leaving a renamed reference next to an
unrenamed one (which would not compile).

A crash never left a source file half-written — every write path goes through an atomic
write-then-rename — but it could leave stray lock/temp files behind and, on the very next run, that
stale lock could make the tool wait 30 seconds and then fail. That's a separate, already-tracked
follow-up, not something this fix changes.

**Known, deliberate gap:** real `dotnet format` keeps spacing operators inside interpolation holes
past 16 levels of nesting; this tool now stops at 16 and leaves anything deeper as written. That gap
only exists between roughly level 17 and where the tool used to crash — no real codebase nests
interpolation that deep, and leaving it unformatted is safer than the alternative.

### Fixed: the format/lint cache could mark unchecked content "clean" (#298)

Two distinct ways a cache entry could describe content nobody actually checked:

- The cache-freshness stat used to be taken fresh at the very end of a file's run, after its content
  was already judged (and, for a rewrite, written). An edit landing in that narrow window could have
  its size/timestamp recorded as "clean" without ever being read. The entry is now built from the same
  file handle the read itself already used — or, for a rewrite, from a reopen-and-read-back that proves
  the recorded bytes are the ones just written.
- `lint --staged`'s changed-line scoping (the pre-commit-hook shape) could have its narrower "clean
  within the staged lines" result recorded as an ordinary whole-file cache entry. A later, unscoped
  `lint` run then reused that entry and reported no findings on a file that still had one outside the
  staged lines — most likely to bite a pre-commit `lint --staged` followed by a plain local `lint`.
  Scoped runs no longer write a whole-file entry; they still correctly reuse one an earlier unscoped run
  wrote.

The on-disk cache format version moved so any entry a previous build already got wrong is discarded
automatically — the next run of any command just re-verifies each affected file once, with no change to
any command's flags, output shape or exit codes.

Fixes three ways formatting could change the **value** of a string literal. Each compiled cleanly and
passed a whitespace-insensitive diff, so nothing warned you. If your code keeps meaningful text in
multi-line strings — prompts, templates, SQL, JSON — take this release.

### Style and cleanup rules no longer rewrite the inside of multi-line strings

The style and analyzer passes treated the lines of a multi-line raw (`"""`) or verbatim (`@"…"`)
string as C# code:

- **Consecutive blank lines inside the string were collapsed** by the blank-line rules.
- Code-shaped content inside a string was rewritten — `string s = "x";` became `var s`, and a
  single-line `if` gained braces.
- Converting to file-scoped namespaces **de-indented `@"…"` content**, removing leading spaces that are
  part of the value.

These passes now leave literal interiors untouched. The whitespace pass already did; the others didn't.

### Two string-boundary bugs that treated string content as code

- **A raw string nested in an interpolation hole of another raw string** (`$$"""…{{ """…""" }}…"""`)
  could make the tool think the outer string ended early, and format the rest of it as code — usually
  a build break.
- **`$@"…"` strings with a multi-line interpolation hole** had the text after the hole re-indented,
  changing the value. Output now matches `dotnet format` exactly.

### `remove unused private members`/`remove unused usings` no longer delete something still referenced

The same literal-hiding fix above had a second-order effect this release introduced: once a multi-line
or raw interpolated string's interior was hidden from the style passes, a private member or `using`
referenced only from *inside* one of those hidden interpolation holes (`$"""…{Helper()}…"""`,
multi-line `$@"…{Helper()}…"`) looked unreferenced and was deleted — a build break, not a formatting
change. Fixed: both rules now see that hidden reference again.

Separately (issue #280): a private member referenced only inside a **disabled** `#if` branch (one whose
symbol your build doesn't currently define) is also no longer deleted. `dotnet format` genuinely does
delete it — Roslyn never parses a disabled branch at all — so this is a deliberate, narrow difference
from its behavior, taken because the alternative is a build break the moment someone defines that symbol
for another target framework or configuration. It only withholds the deletion when the member's own name
appears inside the disabled text; an `#if` elsewhere in the file that never mentions the member is
unaffected.

### Fixed: `prefer readonly field` no longer corrupts expression-bodied methods and properties

With `dotnet_style_readonly_field` on, an expression-bodied private method or property (`private static
int Helper() => 1;`, `private int X => _field;`) could be mistaken for a field declaration and get
`readonly` inserted into its signature — `private static readonly int Helper() => 1;`, which doesn't
compile (`CS0106`). `dotnet format` never does this. Fixed: the candidate scan now checks what follows
the name it found, and backs off when that's a parameter list or an arrow, so only real fields —
including ones with a lambda initializer — are still fixed.

### Unchanged, and why

A raw string's **opening** `"""` can still move when its member is re-indented. That is what
`dotnet format` does too, and it cannot change the value: C# strips content indentation relative to the
**closing** delimiter, and that is never moved.

### Fixed: directory discovery ignored `.fsproj`/`.vbproj` at a monorepo root

`lint`/`format`/`doctor`/`bom`'s directory scan only ever counted `.csproj` files when deciding
whether a directory held one project, several, or none. An `.fsproj` or `.vbproj` sitting there was
invisible to that check, so a root holding an F# or VB project behaved differently from the same
shape in C# — and in the worst case, a root `.csproj` next to a root `.fsproj` silently used the
`.csproj` and dropped the `.fsproj` instead of erroring the way `dotnet format` does on any ambiguous
project mix (issue #265). The scan now counts `.csproj`, `.fsproj`, and `.vbproj` alike, matching
`dotnet format`'s own behavior for every workspace-selection shape it was checked against. C#-only
directories are unaffected — this only changes shapes that previously behaved inconsistently or
silently dropped a project.

### How this was checked

81 string values across every literal form were compiled and read back at runtime before and after
`lint --fix`, `format`, `--fix-changed-lines` and repeated runs, on LF and CRLF files. All unchanged.

Three related problems found during that check are tracked separately and are **not** fixed here:
`--fix-changed-lines` duplicating a line in one specific shape, a bare LF inside a literal in a
mixed-line-ending file becoming CRLF, and single-line `$@"…"` strings with a string inside a hole.

## 1.10.0 — 2026-09-24

**Linux x64 support.** The NuGet package now runs on Linux.

Until this release the package installed on Linux but could not start there: it carried only the Windows
binary, so `dotnet-fast` exited with "native binary was not found". If you run the tool in a Linux CI agent
or container, this is the release that makes it work.

- The package now ships a **linux-x64** native binary alongside the Windows one, and the launcher picks the
  right one for the machine it runs on.
- The Linux binary is **statically linked** (musl), so it has no dependency on the system's C library
  version. It runs on any reasonably modern x64 Linux, including slim container images.
- NuGet drops Unix file permissions when it extracts a package, so the launcher restores the binary's
  execute permission before its first run. Nothing to configure.

Checked before release on the Ubuntu 24.04 (noble) .NET 10 SDK image, installed as a global tool and run
both as root and as an ordinary user: `format`, `format --verify-no-changes`, `lint`, `metrics` and
`affected` all behave as they do on Windows.

**What is not yet covered on Linux:** the parity and real-world validation suites that gate every release
still run on Windows only, so Linux behaviour is checked by the smoke tests above rather than by the full
corpus. `--deep` has not been validated on Linux. macOS and Arm64 Linux are not supported.

Nothing changes on Windows: the Windows binary, every command, flag, output and exit code are identical to
1.9.0.


## 1.9.0 — 2026-09-20

`--deep` now sees two kinds of code it was previously blind to, and `update` stops giving the wrong
answer to anyone who installed the tool outside the .NET tool system.

**If you gate CI on `--deep`, read the first two sections before upgrading** — both make analyzers see
more of your code, so finding counts can rise. That is the fix working, not a regression, but it is a
change you should meet deliberately rather than in a failing build.

### `--deep` now defines your preprocessor symbols

Analyzers under `--deep` were handed every source file with **no preprocessor symbols defined at all**.
So every `#if DEBUG` region was invisible, and — the direction that catches people out — every `#else`
branch that a real build *excludes* was the branch being analysed. Two halves of this tool disagreed
about which code existed: the formatter and native lint have always evaluated per-project symbols,
`--deep` never did.

Symbols are now derived per project, the way the rest of the tool derives them: `DefineConstants`
through `Directory.Build.props`/`.targets` and any imports, plus `DEBUG`/`TRACE`, plus the SDK's
implicit target-framework chain (`NET8_0`, `NET8_0_OR_GREATER`, `NETSTANDARD2_0`, and so on).

Verified against a real build rather than against ourselves: on a fixture where `dotnet build -c Debug`
reports a diagnostic at lines 24, 33 and 40, `--deep` now reports exactly 24, 33 and 40. Previously it
reported line 26 — the `#else` branch the compiler never sees.

- **Expect movement on any project with `#if` regions.** Findings inside a live `#if` appear for the
  first time; findings inside a dead `#else` correctly disappear.
- **Multi-target projects analyse the first target framework only.** Other legs' `#if` branches remain
  invisible. That is a stated limit, not an implied one.
- `DOTNET_FAST_DEEP_NO_SYMBOLS=1` restores the old behaviour if you need to stage the change.

### `--deep` now binds types from your project references

Types coming from a `ProjectReference` were unresolved, so any analyzer that needed them quietly
skipped the code using them. A type inheriting from a base class in a sibling project simply did not
look like what it was.

Verified the same way: on a fixture where a real build reports two `CA1032` diagnostics, `--deep`
now reports the same two. Remove the sibling's build output and it reports none; restore it and the
two come back.

Two things worth knowing, both measured rather than predicted:

- **The binding needs the sibling project to have been built**, and it is skipped when the sibling's
  sources are newer than its output. A fresh `git clone` can leave source timestamps newer than
  committed build output, so on such a checkout this stays inactive until you build. The direction is
  safe — it withholds rather than binding against a stale assembly — but it means the improvement can
  be silently absent.
- **The real-world effect may be nothing at all.** On a large multi-project solution the finding set
  was byte-identical before and after (33 findings either way). The fixture proves the mechanism; your
  repository may see no change.

### `update` no longer tells a winget user to run `dotnet tool update`

`update` recognised two kinds of install: manifest-pinned, and global .NET tool. Anything else fell
through to "global", so a copy installed outside the .NET tool system was told to run
`dotnet tool update --global`. That either fails, or succeeds and installs a **second** copy alongside
the first, after which two package managers each maintain a different binary on your `PATH` and the
tool reports success.

A copy running from a winget install directory is now recognised as such and told `winget upgrade`.
Detection is anchored on the directories winget actually installs into, so a repository that merely
happens to live at a path like `C:\src\winget\packages` is not mistaken for one.

New flag: **`update --explain-install`** prints how the running copy was installed and what evidence
led to that conclusion — useful when a CI log needs to say which install path it is on.

Manifest-pinned and global installs are unchanged, byte for byte.

### Also in this release

- The deep-parity comparison follows Roslynator 5.0's renamed rule ids, so an empty `else` is compared
  against the successor id upstream now reports rather than the one it used to.
- Test-harness correctness: suites that claimed to be hermetic were, on Windows, reaching the real
  .NET SDK instead of the stub they thought they were using. They now genuinely intercept, which means
  the build-cache and test-plan suites test what they claim to.

## 1.8.1 — 2026-09-18

Fix-safety fixes. Every one of them stops the tool from writing a change it could not prove was
correct. If you run `--fix` or `dead-code --write` in CI, take this release.

The pattern across all of them: the rewrite was right for the *common* case and wrong for a case the
rule could not see. Three of these compile cleanly and silently change what your program does, which
is worse than a build break — nothing tells you.

### `dead-code --fix --write` no longer deletes compiler-referenced polyfills

Reported against Polly, where it deleted `internal static class IsExternalInit` and broke the build
with `CS0518` on every `init` accessor and every record, across three target frameworks.

Nothing in the source ever names that type. The compiler references
`System.Runtime.CompilerServices.IsExternalInit` by well-known fully-qualified name at code generation
time, so a reachability graph built from source references correctly concludes nothing reaches it —
and deletes a type the compiler requires. Any project targeting netstandard2.0 or older frameworks
that hand-rolls these shims (or uses a polyfill package that emits them into your source) was exposed.

The same mechanism covers `System.Index` and `System.Range` — `xs[^1]` and `s[1..]` lower to
constructors resolved the same invisible way. Those are now protected too.

Matching is on the full name, so your own unrelated type that happens to share a simple name is still
reported normally.

### `lint --fix` now reaches a fixed point on nullable array types

`string[]?` sent `lint --fix` into a loop: `SA1011` inserted a space before the `?`, `SA1018` removed
it, and each run undid the last. A CI job running `--fix` to convergence would never terminate.

Both rules are faithful ports — upstream StyleCop has the identical contradiction, and an IDE never
exposes it because it offers one fix at a time. `SA1011` still **reports** this shape; it no longer
offers the insert fix for it, which leaves the file exactly where `dotnet format` puts it.

### Four rewrites that could silently change your program's behaviour

Each of these is now withheld when the tool can see the hazard, and still reported:

- **`x == null` → `x is null`** when the operand's type declares its own `operator ==`. A user-defined
  `==` can report a live reference as equal to null — Unity's fake-null pattern, or any hand-rolled
  null-object type — while `is null` always does the real check. The rewrite flips the result with no
  compile error.
- **`!(a == b)` → `a != b`.** C# requires a user-defined `==`/`!=` pair to be *declared* together; it
  does not require them to *agree*. Only the predefined operators are guaranteed opposites.
- **`!(a < b)` → `a >= b`** on floating-point operands. With `NaN` on either side, every ordering
  comparison is false, so the negation and the flipped operator disagree.
- **`0 < s` → `s > 0`.** Swapping operands re-binds overload resolution: `operator <(int, T)` and
  `operator >(T, int)` are different methods and need not be consistent.

These guards read the operand's written declaration, so they fire when the type is declared in the
same file. A type from another file, a partial whose operator lives elsewhere, or an external package
type is still invisible to them and keeps the fix — closing that needs a project-wide symbol index,
which is tracked separately.

### What moves

`lint --fix`, `lint --fix-safe-only` and `dead-code --fix --write` apply **fewer** rewrites than in
1.8.0 — the ones they withhold are the ones that could not be proven safe. Nothing stopped being
reported: every withheld rewrite still appears as a finding, marked manual. The published fixable rule
count is unchanged at 142, because no rule became report-only; each one narrowed the shapes it will
rewrite.

Real-world check: `format`, `lint --fix` and `dead-code --fix --write` now all build clean on every
repository in the validation corpus — Serilog, Dapper, AutoMapper, FluentValidation, MassTransit,
Polly and Newtonsoft.Json — for the first time.

## 1.8.0 — 2026-09-18

Three additions. Nothing existing changes behaviour: every command you already run produces the same
bytes and the same exit codes as 1.7.0.

### New: `rewrite` — structural search across your code

`dotnet-fast rewrite` finds code by **shape** rather than by text. A pattern is C# with
metavariables, so `Foo($A, $B)` matches any two-argument call to `Foo` regardless of what the
arguments look like or how the call is wrapped across lines — no regex, no false hits inside strings
or comments.

```
dotnet-fast rewrite --pattern 'Assert.AreEqual($A, $B)'
dotnet-fast rewrite --pattern 'Foo($A, $B)' --rewrite 'Bar($B, $A)'     # preview a transformation
dotnet-fast rewrite --pattern 'ObsoleteHelper($A)' --check              # CI gate: exit 1 on any hit
```

With `--rewrite` you get a unified diff of what the transformation *would* do. `--check` exits 1 on
any match, which makes it a gate for "this pattern must not reappear". `--json` gives the machine-
readable form. `--context` and `--selector` narrow what counts as a match.

**There is deliberately no `--write`.** The command previews and reports; it never edits your files.
That is worth explaining rather than leaving you to discover it.

Applying a structural rewrite safely needs a guarantee that the result still compiles, and the only
check available here is a re-parse of the output. That check is permissive on a file that already
fails to parse — and the C# grammar in use does not parse every modern construct, so a single C# 11
list pattern in a file switches the check off for that whole file. A malformed template could then
write code that does not compile while reporting success. Rather than ship a writer whose only
safety net has that hole, the command ships read-only. Apply the previewed diff yourself, with your
own review, and you keep the part that was never in doubt.

Two properties of the preview to know before you apply one by hand:

- **Precedence is not adjusted.** `--pattern 'Wrap($X)' --rewrite '$X'` on `Wrap(1 + 2) * 10`
  previews `1 + 2 * 10`. That compiles and changes the value from 30 to 21. A capture spliced into a
  different precedence context is never parenthesised.
- **Comments inside a match are dropped.** `Foo(1 /* keep me */, 2)` loses the comment.

Both are printed as a caution on every non-empty preview, not just documented here.

`--check` tells you when it could not read something rather than calling it clean: a file that is
not valid UTF-8, one containing the reserved marker, or one the grammar cannot parse is counted,
named with its reason, and makes the run exit 1. A gate that could not look at everything should not
report success.

### Every machine-readable document now says which build produced it

`--format json`, SARIF output and the report files now carry the producing version. An artifact you
find in a CI run is self-describing, so a result can be attributed to a specific build without
reconstructing it from install logs — which, when a feed policy silently serves an older version
than your manifest pins, is otherwise guesswork.

New fields are appended; nothing existing was renamed or moved.

### `doctor --include-dependency-smells`

Opt-in flag folding the dead-dependency MSBuild checks into `doctor`: orphaned central package
versions, duplicate references, and the rest of the CPM rules, reported alongside everything else
`doctor` already looks at. Off unless you ask for it, and `doctor`'s existing output is unchanged.

## 1.7.0 — 2026-09-17

A fix-safety release. Everything here exists because `--fix` could delete code that was used, or write
code that did not compile. If you run `lint --fix` or `format` in CI, take this one.

Two behaviour changes are called out at the end. Read those before upgrading.

### `--fix` no longer deletes private members that are used (#277)

Reported from a real repository where a run removed whole method bodies and broke the build. The
style-tier `IDE0051` pass answered "is this private member used?" from one file's text, and that text
had been stripped in ways that hid real references. Five shapes could each lose a member something
still called:

- **A `partial` type** — a member used from another part in another file looked unreferenced. This
  produced the mass deletions in generated and step files.
- **A reference inside an interpolated string** — `$"...{EscapeOData(value)}..."` was blanked before the
  scan, so a member called only from inside a hole looked unused.
- **An attributed member** — `[DataMember] private int _x;` was deleted and its attribute left behind to
  land on the next member (`CS0592`). Multi-line attributes, stacked attributes, and attributes whose
  arguments contain a bracket inside a string each slipped a different check.
- **A reference only in a doc comment** — `/// <see cref="Helper"/>` was stripped as a comment
  (`CS1574`, an error under `GenerateDocumentationFile` with `TreatWarningsAsErrors`). Both `///` and
  `/** */` are now read.

The pass withholds the deletion whenever one file's text cannot prove the member unused. Expect
`IDE0051` to delete less and report more as manual — a withheld fix costs a look, a deleted method
costs a build. It reports exactly as before, and only runs at all if your `.editorconfig` sets
`dotnet_diagnostic.IDE0051.severity`.

**Known limitation:** a member referenced only from a *disabled* `#if` branch is still removed. That
matches `dotnet format`, which also treats disabled branches as trivia.

### `--fix` no longer writes `??` between incompatible types (#271)

`x != null ? x : y` becomes `x ?? y` only if the operands share a common type. A conditional's natural
type is looser, so the ternary compiles where the `??` does not — `CS0019`. This shipped from **five**
independent places, all now closed:

- The style-tier `IDE0029`/`IDE0030` ternary rewrite and the ported **`RCS1084`** are now **report-only**.
  They still report; they no longer rewrite. Nothing on the line names a target type, so no safe
  rewrite is derivable — the same call already made for `DF0093` and `S3240`.
- **`IDE0270`** (folding `if (x == null) { x = y; }`) now synthesises the widening cast, matching
  `dotnet format` byte for byte: `object value = (object?)text ?? DBNull.Value;`. The cast copies the
  declaration's own nullable spelling, so it does not introduce `CS8632` in a nullable-disabled project.
- `IDE0270` no longer folds a **value type** — `int number = 3; if (number == null)` is legal C# but
  `3 ?? 4` is not.
- **`DF0004`**'s `== null` → `is null` rewrite is withheld on non-nullable value types (`CS0037`).
  Reference types and `int?` keep their fix.

Real-world proof: `lint --fix` now builds clean on all seven repositories in the validation corpus.
MassTransit, which had been failing this exact way, passes for the first time.

`IDE0031`'s `?.` rewrite is also withheld inside a lambda or LINQ query on the same line, where an
expression tree cannot contain `?.` (`CS8072`). Expression-bodied members and switch-expression arms
still get fixed. **Known limitation:** when the lambda's `=>` is on an earlier line than the ternary,
the guard cannot see it.

### `--fix-safe-only` is now actually safe

Several of the rewrites above were classed as safe-tier, so the flag whose purpose is withholding risky
rewrites was applying them and breaking builds. With those rules withheld or corrected, it holds.

### Fixed: a cached "clean" could hide real findings

The cache fingerprint joined selected diagnostic ids with a comma, so `--diagnostics RCS1084,IDE0029`
(one unknown id, selecting nothing) and `--diagnostics RCS1084 IDE0029` (two real ids) produced the
same key. The first cached a "clean" result, and every later verify run of the real selection reused
it — **exit 0 on a file with findings**. Ids are now length-prefixed. No cache version bump is needed
and existing entries stay valid.

### `--fix-changed-lines` bounds `--fix` to what was reported

New opt-in flag on `lint`. With a changed-line scope (`--pr-base`, `--staged`, `--ci`, `--affected`,
`--from`), `lint --fix` reported findings only on the lines your branch touched but rewrote the whole
file. Adding `--fix-changed-lines` keeps the write inside the same scope the report used; a hunk
touching even one out-of-scope line is withheld whole rather than split.

`--fix` on its own is **unchanged, byte for byte**, including with every range flag. Given without a fix
pass or without a resolvable range, the new flag errors rather than quietly doing nothing.
`lint --diff --fix-changed-lines` previews the bounded patch and writes nothing.

### Formatter parity: two shapes the oracle preserves and we did not

- A space after a `case` label before `(` — `case ("XS"):` was tightened to `case("XS"):`. `dotnet
  format` keeps the space, so a repo gating on both tools ping-ponged forever (#254).
- An exotic indent run following an XML doc comment was normalised where the oracle leaves it (#249).

### Behaviour change: `--severity` now applies to `--fix`, not just the report (#251)

**Read this if you run `lint --fix` with `--severity`, `--diagnostics`, `--exclude-diagnostics`, or a
`dotnet_diagnostic.<ID>.severity = none` in `.editorconfig`.**

The native `DFxxxx` rules skipped that gate on the fix path: a finding your options excluded from the
report still had its rewrite applied. The report and the write disagreed, and the write was the one
touching your files. They now agree — `lint --fix` applies a `DFxxxx` fix only when the same invocation
would have reported it.

Practical effect: **`lint --fix --severity error` applies fewer fixes than it did in 1.6.1**, and the
ones it applies are the ones it told you about. Plain `lint --fix` with no selection options is
unaffected and byte-identical. Ported analyzer rules were always gated correctly.

### Behaviour change: fewer fixable findings overall

Between the `IDE0051` withholds, the coalesce family going report-only, and `DF0004`'s value-type
guard, a repository that ran `lint --fix` in 1.6.1 will see some findings move from **fixable** to
**manual**. The fixable rule count moves from 144 to 143. Nothing stopped being *reported*; the tool
stopped writing rewrites it could not prove safe from the information available to it.

## 1.6.1 — 2026-09-17

One security fix. No behaviour changes, no command surface changes: every command produces
byte-identical output to 1.6.0.

### Fix: patched TLS library (RUSTSEC-2026-0285)

The TLS stack bundled in 1.6.0 and earlier (`rustls` 0.23.40) accepted TLS 1.3 handshake messages
across encryption level boundaries. It is updated to 0.23.45, which fixes it.

This affects the two places the tool opens an HTTPS connection of its own — `dotnet-fast update`
(the version check and download) and the remote build cache client (`build --plan` against Azure
Blob/Table storage). Formatting, linting, `affected`, `test-plan`, `dead-code` and `dead-deps` do no
networking at all and were never exposed.

Nothing about the fix changes what the tool does: no flag, default, exit code, report field or
output byte moves. If you do not use `update` or the remote build cache, upgrading is optional.

**NuGet users go from 1.5.1 straight to 1.6.1.** The 1.6.0 package was never published — its
publish run failed on infrastructure, and rather than ship a package with a known-vulnerable TLS
library we folded it into this release. Everything in the 1.6.0 notes below is in 1.6.1, and the
1.6.0 GitHub Release binary stays available for anyone who already has it.

## 1.6.0 — 2026-09-16

Four changes. Three of them make the tool see more of your repository — more test frameworks, more of
MSBuild, more of what your analyzers were configured to read — so a few counts move, always in the
direction of doing more work rather than less. Each is named below so none reads as a regression.

### `test-plan` now shards xUnit and MSTest, not only NUnit

`test-plan` discovers **xUnit** (`[Fact]`, `[Theory]` with `[InlineData]`/`[MemberData]`/`[ClassData]`)
and **MSTest** (`[TestMethod]`, `[DataTestMethod]` with `[DataRow]`/`[DynamicData]`) fixtures alongside
NUnit, in C# and F# alike. Nothing to configure and no new flag: those projects previously contributed
zero fixtures and were reported as `no-fixtures`, so none of their tests ran in any shard.

- **A pure-NUnit repository gets the same plan it always did** — byte-identical.
- A repository that mixes frameworks now sees its xUnit/MSTest tests enter the partition, so shard
  membership and a project's `--filter` can change shape. Strictly more tests run, never fewer.
- `--fail-on-skipped-projects` stops exiting `167` for an xUnit/MSTest repository that used to trip
  it, because those projects are now planned. A project with one unparsable source now reports
  `some-sources-unparsed` where it used to report `no-fixtures` — no code was renamed, but the code a
  given project reports can change.
- The property the shard audit relies on — a `--filter` that matches nothing exits 0 and writes a
  zero-count `.trx` — is now **measured per adapter** (NUnit3TestAdapter 5.2.0, xunit.runner.visualstudio
  2.8.2, MSTest.TestAdapter 3.6.4) and pinned by a live test, as is the filter-escaping grammar. The
  VSTest lane is what is covered; Microsoft.Testing.Platform runners (`EnableMSTestRunner`, xunit.v3)
  define their own "nothing ran" exit code and are not.

Two limits, stated precisely. Attributes are matched by **name**, so a derived attribute
(`[SkippableFact]`, your own `FactAttribute` subclass) is not recognised: a project where *every* test
class uses one is reported `no-fixtures` with a warning; a project that *mixes* recognised and derived
attributes is planned from the recognised classes and the others are absent **without a warning** —
give such a project its own `dotnet test` job. Dynamic data sources score a flat weight, which costs
balance but never coverage, and `--use-cached-timings` replaces the estimate on the second run. One
oddity worth knowing because it looks like a sharding bug and is not: `MSTest.TestAdapter` does not run
an F# test class whose name is double-backtick-quoted, under a plain `dotnet test` exactly as under a
sharded one.

### `lint --deep` reads the analyzer inputs your build reads (#138, #139)

`lint --deep` now evaluates a project's `AdditionalFiles` and its implicit and explicit global usings
through the full MSBuild import chain — `Directory.Build.props`, `Directory.Build.targets`, any
`<Import>` — with conditions, `$()` expansion, globs and multi-target-framework union. Before, only
items written literally in the project's own `.csproj` were seen, and a globbed spec such as
`PublicAPI.*.txt` was skipped outright. A repository keeping `BannedSymbols.txt`, `PublicAPI.*.txt`,
`stylecop.json` or `<ImplicitUsings>` in a shared props file was getting **zero** findings from the
analyzers that depend on them, and unbound symbols were suppressing semantic findings.

**A `--deep` gate on such a repository can go green → red on its first run after upgrading.** Those
findings are what `dotnet build` already reports. The resolution is strictly additive — it is unioned
with what the Roslyn sidecar already worked out on its own, so no repository loses a finding it had,
including where a condition is too exotic for the evaluator and it falls back to the old read.

**One-time `--deep-cache` invalidation.** The cache namespace moves so every existing blob is retired
and the first run after upgrade re-analyzes each project. The key now folds in the analyzer input files'
*content*: an edit to `BannedSymbols.txt` changed what the analyzers reported while leaving the old key
unmoved, and the warm server could serve a stale result after such an edit. Both are closed.

Not claimed, and stated in the docs: `#if` regions (no preprocessor symbols are defined), types from
`ProjectReference`s, the extra Web/Worker SDK implicit usings, and globbed `AdditionalFiles` inside
`bin`/`obj`. Those are #272, #273 and #274.

### `affected` evaluates conditioned references the way MSBuild does (#262)

Conditioned project and package references in shared props are now evaluated against the **final**
property values each project ends up with — MSBuild's pass order — so an opt-in shared-source or
test-utility reference gated on a property the `.csproj` sets later is no longer missed. The reported
set is the union of the previous document-order pass and the new item pass, keeping the old pass's
`Remove`/`Exclude` filters, so an ambiguous evaluation can only widen the set, never narrow it.

- Some repositories will see a slightly larger affected set, and slightly more CI work. That is the
  safe direction: a project previously skipped despite a change reaching it is now built.
- `affected --tests-only` can now return a matrix where it previously exited `166`, because a gated
  test-framework `PackageReference` in shared props now classifies the project. The exit code's meaning
  is unchanged; a pipeline that branches on `166` will behave differently on such a repository.
- Build-cache keys change once for exactly the projects whose evaluation changed — a one-time cold
  miss, never a stale hit. No cache-version bump, nothing to do.
- Cost: on a repository where nearly every project trips the second evaluation, 213 ms → 263 ms
  (MassTransit, ~130 projects).

`Choose`/`When` was measured against the pinned SDK and matches MSBuild — branch selection happens in
the property pass — which corrects an older line in the docs that called it a limitation.

### `dead-code` and `dead-dependencies` show progress on long runs (#215)

Both commands now stream phase-by-phase progress to **stderr**: discovery, scanning, and for
`dead-code` the symbol table and the mark pass. The scanning phase names its denominator and gives each
scanned project a `[k/N]` line with its file or reference count and timing; the other phases report
their start and finish only, and each phase closes with its elapsed time. Under
`--verify`/`--verify-tests` there is one line per project built and one per bisect candidate.

Per-item lines are per **project**, never per source file. At or below 50 projects every project gets a
line; above that they collapse to periodic ticks — every 25 projects and at least ~2 s apart — with a
~10 s backstop so silence is bounded. The lines are plain and uncolored; no spinner, no carriage-return
redraw.

**stdout is unchanged.** The report, `--format json` and `--format sarif` are byte-identical, so
pipelines are unaffected. Progress can never change an exit code: the lines are written best-effort, so
a reader that closes stderr early (`| head`, `| grep -m1`, a truncated CI log) loses the lines and
nothing else. Progress is silent in agent mode, under `-v quiet`, and with the new
`DOTNET_FAST_NO_PROGRESS=1` opt-out — the same three switches as the version banner.

## 1.5.1 — 2026-09-16

Three fixes. Two of them are the kind that matter most: cases where the tool told you it was safe and
was not.

### Fix: `lint --fix` no longer writes C# that does not compile (#252, #253)

Two shapes produced a rewrite that the compiler then rejected:

- **A line-wrap inside a string literal.** When the whitespace pass wrapped a long line containing an
  interpolated string whose *hole* itself contained a literal (`$"…{string.Join(", ", x)}…"`), the
  split point could land inside the nested literal's text, not just between arguments. The scanner is
  now hole-aware: a nested literal ends where it really ends. (#252)
- **A null check rewritten inside an expression tree.** `client != null ? client.Name : null` inside a
  LINQ-to-Entities query was rewritten to `client?.Name` — which an expression tree cannot contain
  (`CS8072`). It took **two** rules to close this: `DF0004` was guarded first, and the same shape then
  fell through to the ported `RCS1206`, so the reporter's build stayed broken with a different error.
  Both now withhold their fix when the expression could be an expression tree — query syntax or a
  lambda — and keep reporting the finding as manual. (#253)

Counts move in three places as a result, none of them a regression: whitespace findings drop on repos
with nested interpolations (the tool was mis-reading them), and `DF0004` and `RCS1206` each shift a
few findings from *fixable* to *manual* (measured: three on FluentValidation, none elsewhere in the
validation corpus). Everything else in the fix path is byte-identical to 1.5.0 — the parity ledger
gained two oracle-generated cases and the existing ones are untouched.

A related problem the same investigation found in the *style* tier — `IDE0031`/`IDE0029` rewrites that
can also produce non-compiling code — is a separate issue, filed as #271, and not in this release.

### Fix: build cache no longer returns a stale assembly when only generated sources changed (#268)

The cache key's input fingerprint skipped files the tool classifies as *generated* — an EF Core
migration's `*.Designer.cs`, a `*ModelSnapshot.cs`, a committed `.g.cs`. Those are ordinary compiled
sources, so a commit that touched only a model snapshot kept the old key and CI restored an assembly
built without the change. The build cache now fingerprints every source the project actually compiles,
excluding only `obj/` intermediates.

**No cache-version bump.** The key format is unchanged; only the set of inputs feeding it widened. Keys
move for exactly the projects that own such a file (and their dependents) and stay byte-identical
everywhere else — no fleet-wide cold rebuild. A related but separate problem, the cached restore
props baking in the producing agent's package root (#241), is not in this release; its fix needs a
cache-version bump and is being paired with other artifact-format work so that bump is spent once.

### `bom` says *why* a project was skipped

On a solution run, a project with neither `packages.lock.json` nor a restored `project.assets.json` was
listed as skipped with no reason unless you asked for `--json`. The default summary now prints the
reason under each entry — including how to get a lock file (`RestorePackagesWithLockFile`, `dotnet
restore`, or `--restore`) and, when `--restore` itself failed, the tail of MSBuild's output. The
existing `  - {name}: {path}` line is unchanged; the reason is on new indented lines below it.

## 1.5.0 — 2026-09-09

### New guardrail: `DF9008` — documentation lives on the type

`DF9001` reports comments that explain code, and exempts `///` XML documentation, because a public
API contract is where prose belongs. Nothing bounded that exemption: a `///` block on every property
and method satisfied `DF9001` while putting a second, unchecked copy of the type's contract on each
member — a copy that rots the first time the code moves on without it.

`DF9008` says **where** documentation belongs: on the `class` or `record`, and nowhere else.
Documentation on a property, method, field, event, constructor or accessor is reported, as is a
`///` block attached to no declaration at all. `/** … */` counts as documentation too. A multi-line
block reports once, at its first line.

The premise is that a type states its contract once, in one place a reader can find, and every member
says what it does through its name and signature. When a member comment carried something the
signature genuinely does not say — a unit, an accepted range, a threading rule — the rule's message
points at the design move that closes the gap for good (`int Timeout` → `TimeoutSeconds`, or a
`TimeSpan`) rather than at a better sentence.

It is the most opinionated of the family, and deliberately narrow: only `class` and `record`
(including `record class` and `record struct`) may carry documentation — an `interface`, `enum`,
`struct` or `delegate` is reported like any member.

**`DF9008` contradicts `SA1600`/`SA1601`/`SA1602`, and you must pick one.** Those ported StyleCop
analyzers require documentation on exactly what `DF9008` forbids it on, and being ports they are on
by default. Enable `DF9008` without switching them off and every public member draws two findings
that cannot both be satisfied. They encode opposing conventions, so there is nothing to reconcile in
code — [guardrails.md](docs/guardrails.md) spells out the conflict and gives you the three
`severity = none` lines, and `editorconfig recommend --guardrails` now writes them **commented out**
with the conflict stated above them, so adopting the profile is still one command and turning a
default-on analyzer off stays your decision. The rest of the `SA16xx` family does not conflict.

Like the rest of `DF900x` it is **off by default** and enabled only by its own
`dotnet_diagnostic.DF9008.severity` line — a bulk `dotnet_analyzer_diagnostic.severity` key does not
switch it on, so no repository acquires it on upgrade. It is report-only; `lint --fix` never deletes
a comment on its account.

## 1.4.1 — 2026-09-07

### Fix: a changed F# file no longer slips through a scoped `lint`

`lint --staged`, `--affected`, `--ci`, `--pr-base`, `--base` and `--from`/`--to` asked Git only for
the changed **`.cs`** files. A changed `.fs`, `.fsi` or `.fsx` file was dropped from the scope
before anything looked at it, so the F# hygiene rules (`FSH0001`–`FSH0004`, new in 1.4.0) and the
`--fantomas` lane never saw it and the run reported clean — while a plain `lint .` over the same
tree reported the findings. That is precisely how a pre-commit hook and a PR gate invoke this tool,
so staging a problem F# file passed the gate.

Changed-file discovery now covers every language the tool reads — `.cs` plus `.fs`/`.fsi`/`.fsx` —
and the extension list is derived from one place, so a language added later cannot drift out of the
Git queries again. Changed-line (hunk) scoping applies to F# exactly as it does to C#: an F# file
touched on one line reports only that line's findings, and `lint --fix` on a scoped run fixes the
in-scope F# files without touching F# files outside the scope.

**C# behaviour is unchanged** — the scope was widened, never narrowed, so a C#-only repository
produces byte-identical output on every scoped command.

This completes the F# work that began with [issue #1](https://github.com/RDalziel/dotnet-fast/issues/1).
With 1.4.0 and this fix together, one invocation covers both languages and only what changed:

```bash
dotnet-fast lint --fix --fantomas --staged .
```

C# formatted and linted, F# hygiene fixed, F# structure formatted by your pinned Fantomas. What F#
does and does not support, with the reason for each limit, is in
[support-matrix.md](docs/support-matrix.md).

## 1.4.0 — 2026-09-07

### F# formatting: `--fantomas` drives Fantomas, so `--verify-no-changes` can finally tell the truth about F#

`dotnet-fast lint --fantomas` and `dotnet-fast format --fantomas` format F# by running
[Fantomas](https://fsprojects.github.io/fantomas/), the community-standard F# formatter — we do not
reimplement it, and we do not intend to. F# is offside-rule: indentation is *syntax*, so a formatter
that guesses wrong emits code that will not compile, and there is no oracle to catch that, because
`dotnet format` has no F# support at all. Point real `dotnet format` at an `.fsproj` and it prints
one line and **exits 0** — its `--verify-no-changes` gives a green build on completely unformatted
F#. That is the gap this closes.

**Fantomas decides how F# looks; we decide what gets formatted, when, and how it is reported.**
Fantomas owns every style decision and is configured entirely by its own `.editorconfig` keys — the
`fsharp_*` namespace plus `max_line_length`, `indent_size`, `end_of_line` and `insert_final_newline`.
No `dotnet-fast` setting changes F# formatting.

What `dotnet-fast` adds is the workflow:

- **Project-aware, affected-scoped discovery.** `--project`, `--include`/`--exclude`, `--affected`,
  `--staged`, `--from`/`--to` and `--ci` narrow the F# set exactly as they narrow the C# one.
- **Unchanged files are skipped.** Fantomas has no notion of what changed and re-parses everything
  every time. A content hash — keyed together with the resolved Fantomas version and the
  `.editorconfig` chain, so an upgrade or a config edit correctly invalidates — means a second run
  over an untouched tree starts Fantomas **zero** times.
- **Batched.** Every file in scope goes out in as few invocations as a command line holds. On a
  48-file fixture that is one process instead of 48 (~38× on the measured fixture), and the skip
  cache then removes even that one on an unchanged re-run (~10× again).
- **`.fantomasignore`, with Fantomas' own semantics** — the nearest one wins and they are *not*
  merged the way `.gitignore` files are, so our file set and Fantomas' cannot drift apart.
- **One report.** F# results land in the same summary, `--report` JSON and `--sarif` output as the C#
  ones, tagged `engine: "fantomas"` / `language: "fsharp"`.

Two findings, and they are deliberately different answers: `FSFMT001` "Fantomas would reformat this"
(fixable) and `FSFMT002` "Fantomas could not process this" (not fixable — usually a file whose `#if`
branches are not each valid F# on their own). A file that did not parse has not been checked, and is
never reported as formatted.

**Strictly opt-in and strictly additive.** Without the flag, nothing changes: `format` still never
rewrites an F# file, and a C#-only run is byte-for-byte identical with and without `--fantomas`.
Nothing is ever downloaded or installed — if Fantomas is missing, the run **fails with exit `168`**
and prints the install command, rather than silently checking no F# and reporting success. The
version pinned in the repository's `.config/dotnet-tools.json` is preferred over a global install, so
everyone formats with the version the team chose.

### F# formatting hygiene: four line-level rules `lint` now reports on `.fs`, `.fsi` and `.fsx`

`dotnet-fast lint` previously had nothing to say about F# source. It now reports four things, and
only four:

| ID | Reports | `--fix` |
|---|---|---|
| `FSH0001` | Trailing whitespace at end of line | yes |
| `FSH0002` | File does not end with a newline | yes |
| `FSH0003` | Line ending does not match the resolved `end_of_line` | yes |
| `FSH0004` | Tab character in leading whitespace | **no — reported only** |

**The narrowness is the design, not a first cut.** F# is an offside-rule language: leading whitespace
is syntax, so changing indentation changes what the program means. Reprinting F# properly needs a
real parse, and [Fantomas](https://fsprojects.github.io/fantomas/) — an AST-based reprinter built on
a fork of the F# compiler — is the community standard for that. We are not competing with it: the
`--fantomas` lane above orchestrates it. These four are the changes that **cannot alter a parse at
all**. Indentation, line wrapping, spacing and anything structural are explicitly out of
scope.

**`FSH0004` is a correctness check, not a style opinion.** The F# compiler rejects a tab outside a
string literal or comment — error FS1161, "TABs are not allowed in F# code". It is reported and
never fixed, because converting a tab to spaces needs the indent width the author meant, and under
the offside rule a wrong width silently moves the line into a different block while still compiling.
That asymmetry with the other three rules is deliberate.

Details worth knowing:

- **No parser.** These are checks on bytes, so they keep working on the F# files a grammar cannot
  parse — which is exactly where you most need a tool to still say something.
- **`.fsx` scripts are covered.** A script is not a `<Compile>` item of any project, so it is found
  by walking each `.fsproj`'s directory. A script outside every `.fsproj` directory is not reached.
- **`.editorconfig` as usual**, with no new keys: `end_of_line`, `insert_final_newline`,
  `trim_trailing_whitespace`, and `dotnet_diagnostic.FSH000x.severity` (where `none` removes the
  finding *and* withholds the fix).
- **Nothing else moved.** The rules run under `lint` / bare `dotnet-fast` / `lint --fix` only. The
  `dotnet format`-compatible verbs (`format`, `whitespace`, `style`, `analyzers`, and the
  `dotnet-format-fast` binary) never touch F#, because real `dotnet format` never did. A C#-only
  repository sees byte-identical output in every mode, and the `DFxxxx` rule counts printed by
  `lint --list-rules` are unchanged — `lint --explain FSH0004` is how you read these four.

See [rules.md](docs/rules.md#f-formatting-hygiene-fsh0001fsh0004).

### `test-plan` no longer drops a test project in silence

Reported as [issue #1](https://github.com/RDalziel/dotnet-fast/issues/1): a project that
`test-plan` classified as a test project but discovered no fixtures in was removed from the plan
with **no diagnostic and exit `0`**. The reporter hit it after adding an F# NUnit project to a
sharded workspace — the project built, `dotnet test` ran its three tests locally, and CI stayed
green while never executing them. A false green is the worst thing this tool can produce, so both
halves are fixed.

- **Every such project is now reported.** It is named on stderr with the reason and the consequence
  ("none of its tests will run in any shard"), and appended to `--format json` and the
  `--report <DIR>` artifact as a new `skippedProjects` array. `reason` is one of
  `language-not-supported`, `source-discovery-failed`, `no-source-files`, `no-parsable-sources`,
  `some-sources-unparsed` or `no-fixtures`. Existing JSON fields are unchanged; the array is always
  present, usually empty.
- **New `--fail-on-skipped-projects`** exits `167` instead of warning. Off by default: a scaffolded
  but still-empty test project is legitimate, and a new non-zero default would break those repos.
  Turn it on once yours has none, and a test project that quietly stops contributing tests can never
  report green again.
- **F# NUnit fixtures are discovered.** `[<TestFixture>]` types, types with `[<Test>]` members and
  no attribute, module-level `[<Test>] let` bindings (the module is the fixture), nested modules,
  `[<AbstractClass>]` bases, `[<SetUpFixture>]` exclusion, and ``double-backtick`` names. Fixture
  names use the same runtime form as C# — a module compiles to a class, so a type in
  `module CalculatorTests` is `CalculatorTests+CalculatorTests`. Every name form was checked against
  a real `dotnet test` run. See [test-sharding.md](docs/test-sharding.md#f-test-projects).
- **`.fsproj` compile items are evaluated properly** rather than falling back to a `*.cs` directory
  walk: no implicit compile glob (the F# SDK disables it), declared order preserved, `.fs` and
  `.fsi` included, `.fsx` scripts excluded.
- **A solution resolves to the same project set everywhere.** `affected` returned an `.fsproj`
  listed in a `.sln`/`.slnx` while `format`/`lint` skipped it. Both now read one predicate, so an
  F# project in a solution is visible to every command. An `.fsproj` is also accepted as a target
  where it used to be rejected outright.
- `docs/support-matrix.md` said flatly that "VB and F# projects are not analyzed", which
  contradicted `affected`'s documented behaviour and is now wrong for `test-plan` too. It states
  what each command actually reads.

No existing flag, default, output field or exit code changed.

### `--help` doc pointers fixed

Three `--help` descriptions (`dead-code`, `dead-dependencies`, and `lint`'s
`--only-active-analyzers`) linked to a doc path that does not exist in the public repo — the pointer
went nowhere. `dead-code --help` now points at `docs/dead-code.md`, `dead-dependencies --help` at
`docs/dead-dependencies.md`, and `--only-active-analyzers` at
`docs/ported-analyzers.md#enabling-and-disabling-ports`. Text-only fix — no flag, default, or output
shape changed.

## 1.3.0 — 2026-09-03

**Action required if `metrics` gates your build: re-run `dotnet-fast metrics --write-baseline` before
you upgrade a baseline you rely on.** The scoreboard grows from ten budgets to fourteen — a baseline
recorded before this release never mentions the four new ids, so they are judged against their plain
budget the first time this build measures them, exactly like any other newly-measured row. That is
usually the right behaviour, but it means a repository that was previously green under `--baseline`
can now fail on debt the ratchet never saw. Re-running `--write-baseline` records where the four new
rows stand today and keeps the ratchet meaning what it always has.

### `metrics` — four new rows, `--top all`, `--by-project`

- **Four rows appended** (never renumbered): lines per member (≤ 50), nesting depth (≤ 3), parameter
  count (≤ 7), and maintainability index (≥ 20 — a floor, like coverage). Every default is lifted from
  a threshold this tool already ships elsewhere (the `DF9002` guardrail, the native `S134`/`S107` lint
  ports), not invented. New `--max-lines-per-member` / `--max-nesting-depth` / `--max-parameters` /
  `--min-maintainability-index` flags and `dotnet-fast.json` keys override them; the text board's
  column widths are unchanged and every one of the original ten lines still prints byte-for-byte what
  it always has.
- **`--top all`** (or `--top unlimited`) lists every offender behind an over-budget row instead of the
  default 5 — the only expensive flag here, by orders of magnitude on a large solution.
  `worstMembers` stays capped at 5 regardless, so it cannot grow into a multi-megabyte JSON array.
- **`--by-project`** (opt-in) breaks the board down one line per project — text gets an appended
  block, JSON gets an appended top-level `projects` array. Whole-run figures (coverage, surviving
  mutants, dead code) are not repeated per project; they read as not measured there. Never affects the
  exit code.
- Agent-mode output now opens with a one-line summary and names what was never measured, and caps
  offenders at 3 per row regardless of `--top`.
### The seventh guardrail, and one-command adoption

**New guardrail: `DF9007`, cognitive complexity.** Of the ten published code-health budgets, the
lint gate previously covered three: cyclomatic complexity (`DF9005`), Halstead difficulty (`DF9006`)
and lines-per-file (`DF9003`). Cognitive complexity had no configurable guardrail at all — the closest
thing was the `S3776` port, pinned at a fixed threshold of 15 on a different config axis. `DF9007`
closes that gap: opt-in like the rest of the family, default threshold 22, configured with
`dotnet_fast_max_cognitive_complexity`, scored by the same code `S3776` and `dotnet-fast metrics` use
so all three can never disagree about a member. See [guardrails.md](docs/guardrails.md).

Two more budgets — redundant code and dynamic typing — were assessed for guardrails of their own
(`DF9008`/`DF9009`) and deliberately **not built**: `S4144` (identical member bodies) and `PH2044`
(the `dynamic` keyword) already cover them, and both ports are on by default, so a guardrail twin
would double-report the same defect for anyone who has not disabled the port. Use `S4144`/`PH2044`
for those two.

**One-command adoption: `editorconfig recommend --guardrails`.** Adopting the guardrail family used to
mean switching on six rules (now seven) and setting five thresholds by hand, one of which (lines per
file) has always meant something different from the published SLOC budget — the rule's own default
is 250, the budget is 500. `dotnet-fast editorconfig recommend --guardrails --write` now appends the
whole block in one command: every `DF900x.severity = warning` line, and every threshold stated
explicitly at the published budget number (`dotnet_fast_max_lines_per_file = 500` included). Same
`--write` contract as the existing `recommend` command — appends, never overwrites, idempotent on a
second run.
### `metrics`: estate rollup across repositories (`metrics --merge`)

`metrics --format json` now appends two fields to its `summary`: `schemaVersion` and `repository`
(inferred from the git `origin` remote, overridable with `--repository-name`). New
`metrics --merge <glob>` reads many repositories' own `--format json` reports — offline, no server —
and renders one combined view (`text`, `json`, or a self-contained `html` page) sorted worst-first,
with a per-budget breach summary across the estate. Only budgets a source-only pass can see (source
statics and the opt-in dead-code pass) appear for every repository that ran them; the three
budgets that need a test or mutation run (coverage, CRAP, surviving mutants) appear as a column only
where at least one repository actually measured them, and never as a placeholder for the rest — a
blank column reads as "not measured", never as a pass. A file `--merge` cannot understand (missing or
unrecognised `schemaVersion`) is skipped with a clear reason, never silently merged. Everything here
is additive: a plain `metrics` run with no `--merge` is unchanged. See [metrics.md](docs/metrics.md)
and [ci.md](docs/ci.md).

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

The `0.x` series predates the `1.0.0` compatibility promise and the NuGet package; its notes were
retired to git history — **[RELEASES-0.x.md](RELEASES-0.x.md)** says where to find them.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

# Native lint rules (`DF0001`–`DF0153`)

The native `DFxxxx` catalog `dotnet-fast lint` reports by default. Every rule runs over a
`tree-sitter-c-sharp` syntax tree — no MSBuild, no Roslyn, no restore — which is what makes a bare
`lint` start in milliseconds. Rules that need type information are out of this catalog's reach by
design; `lint --deep` opts into your project's real Roslyn analyzers for those
([deep-linting.md](deep-linting.md)). The popular third-party analyzers re-implemented natively are
listed separately on [ported-analyzers.md](ported-analyzers.md), and the seven opt-in agent-code
guardrails (`DF9001`–`DF9007`) on [guardrails.md](guardrails.md).

The same catalog is available from the CLI without scanning any files:

```console
$ dotnet-fast lint --list-rules                          # every rule: id, [fix] marker, one-line summary
$ dotnet-fast lint --list-rules --category Performance   # one category
$ dotnet-fast lint --list-rules --fixable                # only the rules `--fix` rewrites
$ dotnet-fast lint --explain DF0042                      # full description, autofix status, docs link
```

`--explain` accepts `DF0042`, `df0042` or a bare `42`; add `--json` to either for machine-readable
output. Categories: **Correctness**, **Concurrency**, **Performance**, **Maintainability**,
**Redundancy**, **Style**.

## Suppressing a finding in place

These comments silence a rule locally and also stop `--fix` from rewriting the suppressed code:

```csharp
// dotnet-fast-disable-next-line DF0018
this.value = value;                        // silenced on the following line

x = x; // dotnet-fast-disable-line DF0018   // silenced on this line

// dotnet-fast-disable DF0018               // region: silenced from the next line…
legacy.Reassign();
// dotnet-fast-enable DF0018                // …until here

// dotnet-fast-disable-file DF0018          // whole file (put it near the top)
```

List several ids (space- or comma-separated) after any directive, or none to apply to every rule.
Region directives take effect on the line *after* the comment. To turn a rule off for a whole run
use `--exclude-diagnostics DF0018`, or `dotnet_diagnostic.DF0018.severity = none` in `.editorconfig` —
see [editorconfig.md](editorconfig.md) for the full severity vocabulary, the chain that resolves a
file's `.editorconfig`, and how per-rule, per-category and bulk severities are prioritized.

## The catalog

The `--fix` column says whether `lint --fix` rewrites the finding. `report-only` rules are the ones
whose rewrite cannot be proven sound from syntax alone (nullable-bool comparisons, expression-tree
lambdas, and the like) — they are reported and left for you. Every autofix is build-verified against
real open-source repositories before it ships.

| ID | Rule | `--fix` |
|---|---|---|
| `DF0001` | Empty `catch { }` block swallows exceptions. | report-only |
| `DF0002` | Empty `finally { }` block does nothing. | report-only |
| `DF0003` | Redundant empty statement (stray `;`). | **autofix** (removes it) |
| `DF0004` | `== null` / `!= null` — prefer `is null` / `is not null`. | **autofix** (rewrites; skipped inside lambdas — expression trees can't use `is`) |
| `DF0005` | Redundant comparison to a boolean literal (`x == true`). | report-only (unsound to fix: `bool? == true` ≠ `bool?`) |
| `DF0006` | Empty `if (...) { }` body. | report-only |
| `DF0007` | Redundant boolean literals in a conditional (`c ? true : false`). | **autofix** (`c` / `!c`) |
| `DF0008` | Empty initializer on `new T() { }`. | **autofix** (removes `{ }`) |
| `DF0009` | Duplicate `using` directive in the same scope (whitespace-insensitive; alias/static variants distinct; scope-local, so a namespace-level re-import is not flagged). | **autofix** (removes the duplicate line) |
| `DF0010` | Redundant `.ToString()` in an interpolation hole (`$"{x.ToString()}"`). Holes with a format clause, argumentful `ToString(...)`, `?.ToString()`, and `base.ToString()` are excluded. | **autofix** (removes the call) — **only when every target framework of *every project that compiles the file* is `net6.0`+ and its effective `<LangVersion>` is C# 10+**; otherwise (an older target, a language-version pin below 10 — including one written as a per-TFM `Condition` — or a target that cannot be resolved) the finding is reported with no fix attached. A file that is a compile item of several projects (shared-source / StrongName / link-include layouts) must satisfy all of them, since there is only one copy of the bytes. `$"{span}"` needs `DefaultInterpolatedStringHandler` (.NET 6) *and* the C# 10 handler lowering; without both the hole is converted to `object` and a `ref struct` receiver fails to compile (CS0029), and the receiver's type is not knowable syntactically. |
| `DF0011` | Empty `{ }` block standing alone as a statement. Method/accessor/loop/lambda bodies excluded (non-block parents). | report-only (usually a deleted-code stub worth a look) |
| `DF0012` | Comparison to `""` or `string.Empty` (`s == ""` / `s != string.Empty`) — prefer `string.IsNullOrEmpty` or `Length == 0`. | report-only (rewrites differ on a null receiver) |
| `DF0013` | `catch (E e) { throw e; }` resets the stack trace — use `throw;`. Skipped when the variable is reassigned/`ref`-passed or the throw sits in a nested lambda/local function (CS0156). | **autofix** (`throw e;` → `throw;`) |
| `DF0014` | Redundant `default: break;` switch section. Stacked-label fallthrough and `goto default` switches excluded. | report-only (explicit `default` is often a style marker) |
| `DF0015` | Double negation `!!x` (direct nesting only; `!(!x)` untouched). | **autofix** (removes both operators) |
| `DF0016` | `lock (this)` / `lock (typeof(T))` — the lock object is publicly reachable, so unrelated code can deadlock against it. Locks on fields/identifiers/member access are not judged (reachability is semantic). | report-only (fix is a private lock-object field — a design change) |
| `DF0017` | Empty `lock (...) { }` body — the lock is acquired and immediately released. Comment-only bodies (documented barriers) and braceless bodies excluded. | report-only (an empty lock is occasionally a deliberate wait-for-holder barrier) |
| `DF0018` | Self-assignment `x = x;` / `this.X = this.X;` — usually a constructor typo for `this.x = x;`. Statement-level plain `=` only; object/`with` initializers (`{ X = X }` reads outer scope), element access, and compound operators excluded. | report-only (property accessors can have side effects) |
| `DF0019` | Throwing the base `Exception` type (`throw new Exception(...)`) forces callers to `catch (Exception)` and swallow unrelated failures — throw a specific or custom type. Only the bare `Exception` / `System.Exception` is matched (statement and `throw`-expression position); subclasses like `ArgumentException` are fine. | report-only (the right type is a design decision) |
| `DF0020` | Empty `else { }` block — runs no alternative branch, usually leftover from deleted code. Only a truly empty `{ }` is matched; a comment-only `else { /* ... */ }` and `else if` chains are excluded. | report-only (a rare team keeps an empty `else` as a marker) |
| `DF0021` | `catch (E) { throw; }` only rethrows, so the `try`/`catch` has no effect — drop it or add real handling. Excluded: a `when (...)` filter, any other statement, a comment in the body, and `throw e;` (which resets the stack trace — that is DF0013). | report-only |
| `DF0022` | `new T[0]` / `new T[] { }` allocates a fresh empty array on every call; prefer `Array.Empty<T>()`, a cached singleton (CA1825). Only zero-length creations match — a populated initializer or a non-zero / variable size is fine. | report-only |
| `DF0023` | A bare `return;` as the last statement of a void method, constructor, accessor, or local function — control returns there anyway. Only the final statement of the body is matched; an early `return;` inside an `if`/loop/`switch` is left alone. | fix³ |
| `DF0024` | `if (x = y)` — a plain `=` assignment as the whole `if` condition is almost always a typo for `==`. A compound (`+=`, …) and the assign-and-test idiom `if ((x = f()) > 0)` (assignment nested in a larger expression) are excluded. | report-only |
| `DF0025` | `if (cond);` — a stray `;` is the body, so the conditional does nothing and the next statement runs unconditionally (accidental semicolon). Also matches `else;`. Loop bodies are not matched (an empty `for`/`while` body is sometimes deliberate); an empty `{ }` block is DF0006. | report-only |
| `DF0026` | `return` inside a `finally` swallows any exception propagating out of the `try`/`catch` and overrides a pending return (CA2219). A `return` inside a lambda or local function declared in the `finally` returns from *that* and is not matched. | report-only |
| `DF0027` | `lock (new object())` / `lock (new())` allocates a fresh monitor on every entry, so every caller takes a different lock and the `lock` provides no mutual exclusion. Lock a shared `private readonly object` field instead. | report-only |
| `DF0028` | `throw new NotImplementedException()` is unfinished scaffolding that should not reach production. Matches statement and `throw`-expression position; the bare `NotImplementedException` / `System.NotImplementedException` type. | report-only |
| `DF0029` | `x == x` / `x != x` compares a value to itself (constant result) — almost always a typo for a different operand. Only matches textually identical *simple* references (identifier or member-access chain); invocations and indexers are left alone. The deliberate float NaN idiom can't be excluded syntactically — prefer `double.IsNaN(x)`. | report-only |
| `DF0030` | `x == double.NaN` / `x != float.NaN` (either operand) — `NaN` is unequal to everything including itself, so `==` is always false and `!=` always true. Matches the `double`/`float`/`System.Double`/`System.Single` spellings. Use `double.IsNaN(x)`. | report-only |
| `DF0031` | `if (true)` / `if (false)` — a literal boolean condition makes the branch unconditional or dead. Loop conditions (`while (true)`) are a deliberate idiom and are not matched. | report-only |
| `DF0032` | `class C : object` names the implicit base explicitly; every class derives from `object`. Matches the `object` keyword and `System.Object` / `global::System.Object`, only in a class base list. | report-only |
| `DF0033` | `lock (this)` uses the publicly-reachable instance as its monitor — external code holding the reference can take the same lock and deadlock (CA2002). Lock a `private readonly object` field. Distinct from DF0016. | report-only |
| `DF0034` | `throw null;` / `throw null` throws a `NullReferenceException` instead of a meaningful exception — almost always a mistake. | report-only |
| `DF0035` | `switch (x) { }` with no sections does nothing — a forgotten body or dead scaffolding. | report-only |
| `DF0036` | `c ? x : x` yields the same value on both branches, so the condition is pointless. Only matches textually identical *simple* references; `c ? f() : f()` may differ and is left alone. | fix¹ |
| `DF0037` | A statement directly after `return` / `throw` / `break` / `continue` / `goto` in the same block is unreachable. A following local function (hoisted) or labelled statement (`goto` target) is skipped. | report-only |
| `DF0038` | A boolean literal in `&&` / `||` is redundant or constant: `x && true` is `x`, `x || false` is `x`, `x && false` is always `false`, `x || true` is always `true`. | fix² |
| `DF0039` | `lock ("text")` locks on a process-wide interned string, so unrelated components can take the same monitor and deadlock (CA2002). Lock a private object. | report-only |
| `DF0040` | `a && a` / `a || a` evaluates to just `a`. Only matches side-effect-free, textually identical *simple* references; `f() && f()` is left alone. | fix |
| `DF0041` | `a - a` (0), `a / a` (1), `a % a` (0), `a ^ a` (0), `a & a` (a), `a | a` (a) — a constant or redundant result, almost always a typo. `+`/`*` and non-simple operands are not matched. | report-only |
| `DF0042` | `if (a) … else if (a) …` repeats a condition in one if/else-if chain, so the later branch is unreachable. Conditions compared textually. | report-only |
| `DF0043` | `$"text"` with no `{…}` interpolations is an ordinary string — the `$` is redundant. Skipped when the content contains a brace (escaping would change). | fix¹ |
| `DF0044` | `true ? a : b` / `false ? a : b` has a constant condition, so one branch is always taken and the other dead. The ternary analogue of DF0031. | fix |
| `DF0045` | `new SomeException();` as a statement constructs an exception and discards it — almost always a missing `throw`. Matches a discarded creation of a type ending in `Exception`. | report-only |
| `DF0046` | `while (false) { … }` never executes its body. The `while (true)` idiom and `do … while (false)` are different constructs and not matched. | report-only |
| `DF0047` | `!(a == b)` / `!(a != b)` negates an equality test with a direct opposite (`a != b` / `a == b`). The `!!` case is DF0015. | fix |
| `DF0048` | `x.Equals(null)` is always false (or throws) — use `x is null`. Matches a one-argument `.Equals(null)` call. | report-only |
| `DF0049` | `x ?? null` yields `null` exactly when `x` is null, so it is just `x` — the `?? null` is redundant. | fix |
| `DF0050` | `namespace N { }` with an empty body declares a scope containing nothing — leftover scaffolding. Block-form namespaces only. | report-only |
| `DF0051` | `!true` / `!false` negates a constant — write the opposite literal directly. The `!!x` case is DF0015. | fix |
| `DF0052` | `x.Equals(x)` compares a value to itself (always true for a well-behaved `Equals`). Only matches textually identical *simple* references. | report-only |
| `DF0053` | `x ?? x` coalesces a value with itself, so it is just `x`. Only matches side-effect-free, textually identical operands. | fix |
| `DF0054` | `this?.Member` — `this` is never null inside an instance member, so the `?.` is dead and misleading. | report-only |
| `DF0055` | A `continue;` that is the last statement of a `while`/`for`/`foreach`/`do` body does nothing. The loop analogue of DF0023. | report-only |
| `DF0056` | `ReferenceEquals(x, x)` / `object.ReferenceEquals(x, x)` compares a reference to itself (always true). Two identical *simple* arguments. | report-only |
| `DF0057` | `"literal".ToString()` returns the same string — the call is redundant. | fix |
| `DF0058` | `x ??= null` assigns `null` only when `x` is already null — a no-op. | fix³ |
| `DF0059` | `x += 0` / `x -= 0` / `x *= 1` / `x /= 1` leave `x` unchanged — almost always a typo. Matches an integer-literal `0`/`1` right operand. | fix³ |
| `DF0060` | `await Task.FromResult(x)` allocates a completed task only to unwrap it synchronously — use the value directly. Matches a `Task`/qualified `...Task` receiver. | report-only |
| `DF0061` | `xs.Select(x => x)` is an identity projection that allocates an iterator for nothing. Matches a single identity lambda. Matched by method name. | report-only |
| `DF0062` | `xs.ToList().ToList()` / `xs.ToArray().ToArray()` re-materialises an already-materialised sequence. Matched by method name. | fix |
| `DF0063` | `xs.Count() == 0` / `> 0` / `!= 0` enumerates to test emptiness; `Any()` short-circuits (CA1827). Zero-arg `.Count()` vs literal `0`/`1`. Matched by method name. | report-only |
| `DF0064` | `x + ""` / `"" + x` concatenates an empty string — prefer `ToString()` or interpolation. Matches an empty `""`/`@""` operand of `+`. | report-only |
| `DF0065` | `!(x is T)` has the direct form `x is not T` (C# 9+). Only a bare type on the right (a captured pattern variable can't be rewritten). | fix |
| `DF0066` | `string.Format("text")` with a single literal argument returns it unchanged (or throws on a placeholder). | fix¹ |
| `DF0067` | `x < x` / `x > x` (always false) and `x <= x` / `x >= x` (always true). Identical *simple* references; the relational analogue of DF0029. | fix |
| `DF0068` | `x ^ true` is `!x`, `x ^ false` is `x` — XOR with a boolean literal is an obscure negation or no-op. | fix |
| `DF0069` | A boolean literal in non-short-circuit `&` / `|` is redundant or constant, and both sides still evaluate. The short-circuit form is DF0038. | fix² |
| `DF0070` | `if (c) { } else { }` with both branches empty does nothing regardless of the condition. | report-only |
| `DF0071` | `try { } catch …` with an empty `try` body cannot throw, so the `catch`/`finally` is dead. | report-only |
| `DF0072` | `((x))` wraps an already-parenthesised expression in a second, redundant pair. | fix |
| `DF0073` | `x * 1` / `1 * x` / `x / 1` / `x - 0` is an arithmetic identity (numeric no-op). `+ 0` is excluded (can concatenate). | fix |
| `DF0074` | `x | 0` / `x ^ 0` / `x << 0` / `x >> 0` leaves an integer unchanged — a redundant bitwise/shift identity. | fix |
| `DF0075` | `x & 0` (either operand) is the constant `0` — almost always a typo for a different mask. | report-only |
| `DF0076` | `x -= x` (0), `x /= x` (1), `x ^= x` (0), `x &= x` (x), `x \|= x` (x) — compound assignment of a value with itself. `+=`/`*=` excluded. | report-only |
| `DF0077` | `null is T` can never match — always false. Bare type on the right only (not a constant/`var` pattern). | report-only |
| `DF0078` | A `when (true)` / `when (false)` filter on a `catch` is redundant (`true`) or dead (`false`). | report-only |
| `DF0079` | `"literal" ?? y` / `5 ?? y` coalesces from a never-null constant, so the right operand is dead. | fix |
| `DF0080` | `xs.Length < 0` (always false) / `xs.Count >= 0` (always true) — `Length`/`Count` is never negative. `== 0` / `!= 0` not matched. | report-only |
| `DF0081` | `x.CompareTo(x)` always returns 0. Receiver and argument must be identical *simple* references. | report-only |
| `DF0082` | `Math.Max(x, x)` / `Math.Min(x, x)` just returns `x`. `Math`/`System.Math` receiver, two identical *simple* args. | report-only |
| `DF0083` | `x is var y` matches every value (including null), so the test is always true. | report-only |
| `DF0084` | `a || b || a` repeats an operand within one `&&`/`||` chain. Side-effect-free *simple* operands only. | report-only |
| `DF0085` | `this ?? x` — `this` is never null inside an instance member, so the right operand is dead. The literal form is DF0079. | fix |
| `DF0086` | `new T()?.Member` — a `new` expression is never null, so the `?.` is dead. The `this?.` form is DF0054. | fix |
| `DF0087` | `xs.Where(x => true)` is a no-op filter; `xs.Where(x => false)` empties the sequence. Matched by method name. | report-only |
| `DF0088` | `string.IsNullOrEmpty("literal")` / `IsNullOrWhiteSpace("literal")` has a compile-time-constant result. | fix¹ |
| `DF0089` | `xs.Distinct().Distinct()` re-distincts an already-distinct sequence; the second call is redundant. Matched by method name. | fix |
| `DF0090` | `1 == 2` / `"a" != "b"` compares two compile-time constants; the result is fixed. | report-only |
| `DF0091` | `new T() ?? y` — a `new` expression is never null, so the right operand is dead. (DF0085 = `this`, DF0079 = literal.) | fix |
| `DF0092` | `this is null` is always false, `this is not null` always true, inside an instance member. | fix |
| `DF0093` | `a == null ? b : a` / `a != null ? a : b` is `a ?? b` **when the operand types are compatible**; `a` must be a side-effect-free *simple* reference. Report-only since v0.306.2 (issue #219): the equivalence is a type fact a syntactic rule cannot establish — `x != null ? x : DBNull.Value` compiles because the conditional is target-typed to `object`, while `x ?? DBNull.Value` is `CS0019`. | report-only |
| `DF0094` | `!(a < b)` / `!(a >= b)` negates a relational test with a direct opposite. The `==`/`!=` form is DF0047. | fix |
| `DF0095` | `default(int)` / `default(bool)` — the literal (`0`/`false`) is clearer. Integer family and `bool` only. | fix |
| `DF0096` | `if (c) S; else S;` runs the same statement either way; the condition has no effect. The ternary form is DF0036. | report-only |
| `DF0097` | `xs.Skip(0)` skips nothing — a no-op. Matched by method name. | report-only |
| `DF0098` | `xs.Reverse().Reverse()` restores the original order — a no-op. Matched by method name. | fix |
| `DF0099` | `Math.Abs(Math.Abs(x))` is redundant — `Abs` is idempotent. Matched by method name. | fix |
| `DF0100` | `xs.Take(0)` always yields an empty sequence — almost always a mistake. Matched by method name. | report-only |
| `DF0101` | `c ? null : null` evaluates to `null` either way; the condition is pointless. The `c ? x : x` form is DF0036. | report-only |
| `DF0102` | `xs.Where(p).Any()` can fuse to `xs.Any(p)`; the filtered sequence is wasted. No-arg `Any()` over a single-param-lambda `Where` (indexed overload excluded). | fix |
| `DF0103` | `xs.Where(p).Count()` can fuse to `xs.Count(p)`; the filtered sequence is wasted. No-arg `Count()` over a single-param-lambda `Where`. | fix |
| `DF0104` | `xs.Where(p).First()` can fuse to `xs.First(p)`; the filtered sequence is wasted. No-arg `First()` over a single-param-lambda `Where`. | fix |
| `DF0105` | `xs.Where(p).FirstOrDefault()` can fuse to `xs.FirstOrDefault(p)`; the filtered sequence is wasted. No-arg overload over a single-param-lambda `Where`. | fix |
| `DF0106` | `xs.Where(p).Single()` can fuse to `xs.Single(p)`; the filtered sequence is wasted. No-arg `Single()` over a single-param-lambda `Where`. | fix |
| `DF0107` | `xs.Where(p).SingleOrDefault()` can fuse to `xs.SingleOrDefault(p)`; the filtered sequence is wasted. No-arg overload over a single-param-lambda `Where`. | fix |
| `DF0108` | `xs.Where(p).Last()` can fuse to `xs.Last(p)`; the filtered sequence is wasted. No-arg `Last()` over a single-param-lambda `Where`. | fix |
| `DF0109` | `xs.Where(p).LastOrDefault()` can fuse to `xs.LastOrDefault(p)`; the filtered sequence is wasted. No-arg overload over a single-param-lambda `Where`. | fix |
| `DF0110` | A no-arg idempotent string transform on its own result — `s.Trim().Trim()`, `ToLower`/`ToUpper`/`ToLowerInvariant`/`ToUpperInvariant`/`TrimStart`/`TrimEnd`. The outer call cannot change the result. Matched by method name. | fix |
| `DF0111` | `s.Substring(0)` returns the original string unchanged — a no-op slice. Matched by method name. | report-only |
| `DF0112` | `(T)(T)x` casts to the same type twice; the outer cast is a no-op. Both casts must name the same type (`(int)(long)x` is a real conversion, not matched). | fix |
| `DF0113` | `if (c) return true; else return false;` (or the inverse) collapses to `return c;` / `return !(c);`. The explicit-`else` form is fixable; the no-`else` sibling form is flagged without a fix. | fix |
| `DF0114` | `s.IndexOf(x) >= 0` / `== -1` (and `<`/`!=`/`>`/`<=` vs 0/-1) is a membership test better written as `Contains(x)`. `== 0` / `!= 0` (prefix tests) are excluded. | report-only |
| `DF0115` | A conditional with one boolean-literal branch simplifies to `&&`/`||`: `c ? true : x` → `c \|\| x`, `c ? x : false` → `c && x`, etc. (both-literal forms are DF0007/DF0101). | report-only |
| `DF0116` | `Nullable<T>` is the verbose form of `T?`. Only the unqualified form is matched (`System.Nullable<T>` / `Nullable<int>.Compare` skipped). | fix |
| `DF0117` | `ReferenceEquals(x, null)` (either order) is clearer as `x is null` and bypasses overloaded `==`. Matched when exactly one argument is `null`. | report-only |
| `DF0118` | An `async void` method can't be awaited and its exceptions escape (a crash hazard) — prefer `async Task` (VSTHRD100). Event-handler-shaped methods (`object, …EventArgs`) are exempt. | report-only |
| `DF0119` | `xs.ToList().Count` / `xs.ToArray().Length` / `xs.ToHashSet().Count` allocates a throwaway collection just to read its size; count the sequence directly (`xs.Count()`). | report-only |
| `DF0120` | `!x ? a : b` reads more directly as `x ? b : a` (branches swapped) — logically identical. `!!x` is left to DF0015. | fix |
| `DF0121` | `xs.Any(x => false)` is always `false`, `xs.All(x => true)` always `true` — a constant predicate makes the quantifier result independent of the sequence (likely a bug). `Where` is DF0087. | report-only |
| `DF0122` | `xs.OrderBy(a).OrderBy(b)` re-sorts by `b`, discarding the `a` ordering — almost always meant `OrderBy(a).ThenBy(b)`. | report-only |
| `DF0123` | `Task.Delay(0)` completes synchronously — a no-op delay (no wait, no yield). Use `Task.CompletedTask` or remove it. Matched on `Delay` with a literal `0`. | report-only |
| `DF0124` | `Math.Pow(x, 2)` squares via the general power routine; `x * x` is the direct, faster form. Matched on a two-arg `Pow` whose exponent is the literal `2`. | report-only |
| `DF0125` | `xs[xs.Length - 1]` / `xs[xs.Count - 1]` indexes the last element the long way; `xs[^1]` (C# 8+) indexes from the end directly. Matched on the same receiver. | report-only |
| `DF0126` | `s.ToCharArray()` fed to `.Length` or a LINQ operator allocates a throwaway `char[]`; a string is already an `IEnumerable<char>` with `Length`/an indexer. A real `char[]` consumer is not flagged. | report-only |
| `DF0127` | `xs.ElementAt(0)` / `xs.ElementAtOrDefault(0)` reads more directly as `xs.First()` / `xs.FirstOrDefault()`. Matched on a literal `0` index. | report-only |
| `DF0128` | `…GetAwaiter().GetResult()` blocks on async work (sync-over-async) — a deadlock/thread-pool-starvation risk (VSTHRD002); `await` it instead. | report-only |
| `DF0129` | `x && !x` is always false and `x \|\| !x` always true — an operand `&&`/`\|\|`'d with its own negation. Matches a simple reference only. | report-only |
| `DF0130` | `s.Replace(x, x)` replaces a value with itself — a no-op, almost always a typo. Matched on textually identical literal/simple-reference arguments. | report-only |
| `DF0131` | `s.PadLeft(0)` / `s.PadRight(0)` pads to width 0 — a no-op returning the original string. Matched on the single-arg width overload with a literal `0`. | report-only |
| `DF0132` | `s.Substring(0, s.Length)` copies the whole string — a no-op returning the original. Matched on the same receiver. (`Substring(0)` is DF0111.) | report-only |
| `DF0133` | `a.ToLower() == b.ToLower()` (or `ToUpper`/invariant) allocates throwaway strings to compare; use `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)` (CA1862). | report-only |
| `DF0134` | An `else` whose body is just `{ if (…) … }` can be written as `else if`, dropping a nesting level. Matched on a single braced nested `if`. | report-only |
| `DF0135` | `xs.Select(f).Count()` counts after projecting, but a projection never changes the count — `xs.Count()` is equivalent and skips the `Select`. | report-only |
| `DF0136` | `xs.Count(pred) <op> 0` counts all matches to test for any; `xs.Any(pred)` short-circuits at the first. The predicate-overload analogue of DF0063. | report-only |
| `DF0137` | Consecutive `.Where(a).Where(b)` compose into one predicate (`Where(x => a(x) && b(x))`), removing a second iterator layer. Merging the lambdas isn't a mechanical rewrite. | report-only |
| `DF0138` | Consecutive `.Select(a).Select(b)` compose into one projection (`Select(x => b(a(x)))`), saving an iterator layer. Same lambda-merging reason as DF0137. | report-only |
| `DF0139` | A unary plus (`+x`) is a no-op on every built-in numeric type — almost always a typo or leftover. `+` can be operator-overloaded, so dropping it isn't provably safe syntactically. | report-only |
| `DF0140` | `xs.ToList().ToArray()` (or `ToArray().ToList()`) materialises twice into different collection types; the inner allocation is discarded. The same-type case is DF0062. | report-only |
| `DF0141` | `xs.Concat(Enumerable.Empty<T>())` / `Array.Empty<T>()` concatenates a provably-empty sequence — a no-op; drop the `Concat`. | report-only |
| `DF0142` | `Math.Pow(x, 0)` is always `1` and `Math.Pow(x, 1)` is just `x`; a trivial exponent doesn't need the general power routine (exponent 2 is DF0124). | report-only |
| `DF0143` | `xs.Reverse().First()` is `xs.Last()` and `xs.Reverse().Last()` is `xs.First()` — reversing only to take an endpoint is wasteful. | report-only |
| `DF0144` | `xs.ToList().ForEach(...)` materialises a `List<T>` just to iterate it (losing laziness, allocating); a `foreach` loop is clearer and cheaper. | report-only |
| `DF0145` | `string.IsNullOrEmpty(s.Trim())` throws if `s` is null and ignores whitespace-only strings; `string.IsNullOrWhiteSpace(s)` is the safe, intended check. | report-only |
| `DF0146` | `new Regex(p).IsMatch(...)` (or `.Match`/`.Matches`/`.Replace`/`.Split`) compiles the pattern per call; use static `Regex.IsMatch`, a cached `static readonly Regex`, or `[GeneratedRegex]`. | report-only |
| `DF0147` | `xs.OrderBy(k).Any()` / `.Count()` / `.Contains(v)` / `.All(p)` sorts for nothing — the result is order-independent; drop the `OrderBy`/`ThenBy` (`First`/`Last`/`Min`/`Max` excluded). | report-only |
| `DF0148` | `xs.ToList().AsEnumerable()` / `xs.ToArray().AsEnumerable()` — the `AsEnumerable()` is redundant; the materialised collection already is an `IEnumerable<T>`. | report-only |
| `DF0149` | `this == null` / `this != null` is effectively constant — `this` is never null inside an instance member; use `this is null` / `is not null` if an identity check is intended (binary `==`/`!=` may bind an overloaded operator, so unlike DF0092 this is not folded). | report-only |
| `DF0150` | `new Guid()` constructs the all-zero GUID, usually an oversight; use `Guid.Empty` for the zero GUID or `Guid.NewGuid()` for a fresh one (only the zero-arg form is matched). | report-only |
| `DF0151` | `GC.Collect()` forces a full garbage collection and usually hurts throughput; let the runtime manage GC (the no-arg form; `GC.Collect(gen)` is left alone). | report-only |
| `DF0152` | `new ApplicationException(...)` — discouraged by Microsoft's guidance (adds nothing over `Exception`); throw a specific built-in or custom exception type instead. | report-only |
| `DF0153` | `Thread.Sleep(0)` is a no-op delay that signals a polling/timing hack; use `Thread.Yield()` (or `await Task.Yield()`), or remove it. | report-only |

¹ DF0036 is fixed only when the condition is itself side-effect-free (otherwise the rewrite would skip
its evaluation); DF0043 is fixed only for the plain `$"…"` form (the `$@"…"` / `@$"…"` variants stay
report-only); DF0088 is folded only for a plain, escape-free `"…"` literal (escaped/verbatim/raw forms
stay report-only); DF0066 is folded only when the literal has no `{`/`}` (a placeholder would throw,
a brace-escape would change). Each still reports in the cases it doesn't rewrite.

² DF0038 / DF0069: the identity folds (`x && true` → `x`, `x | false` → `x`) always apply; the
absorbing constants (`x && false` → `false`, `x | true` → `true`) apply only when the other operand is
side-effect-free, since they drop its evaluation — otherwise report-only.

³ DF0023 / DF0058 / DF0059 delete the whole line only when the dead statement is alone on it (just
indentation before, whitespace/newline after); if it shares its line with other code or a comment it
stays report-only, to avoid leaving a fragment.

## F# formatting hygiene (`FSH0001`–`FSH0004`)

A small, separate catalog for F# source. It is deliberately narrow, and the reason is worth stating
plainly: **F# is an offside-rule language — leading whitespace is syntax.** Change a line's
indentation and you change which block it belongs to, which changes what the program does. Reprinting
F# correctly therefore needs a real parse, and the community already has that in
[**Fantomas**](https://fsprojects.github.io/fantomas/), an AST-based reprinter built on a fork of the
F# compiler. `dotnet-fast` is not competing with it.

What is left is the set of changes that **cannot alter a parse at all**, and that is exactly this
catalog:

| ID | Rule | `--fix` |
|---|---|---|
| `FSH0001` | Trailing whitespace at end of line. | **autofix** (removes it) |
| `FSH0002` | File does not end with a newline. | **autofix** (appends the configured `end_of_line`) |
| `FSH0003` | Line ending does not match the resolved `end_of_line`. | **autofix** (normalises it) |
| `FSH0004` | Tab character in leading whitespace. | report-only — see below |

**Explicitly out of scope: indentation, line wrapping, spacing, and anything structural.** Those
belong to Fantomas, which [`--fantomas`](commands.md#f-formatting-with-fantomas) orchestrates rather than
reimplements.

These rules run in every scope the C# rules run in: `--staged`, `--affected`, `--ci`, `--pr-base`,
`--base` and `--from`/`--to` all resolve changed `.fs`/`.fsi`/`.fsx` files alongside changed `.cs`
ones, and [changed-line scoping](commands.md#lint) narrows an F# report to the touched lines exactly
as it does for C#.

### Why `FSH0004` is reported but never fixed

The other three edit whitespace the lexer never sees. A tab in leading whitespace is different twice
over.

It is not a style opinion. The F# compiler **rejects** it: error **FS1161, "TABs are not allowed in
F# code"**. Microsoft's own F# formatting guidance states it directly — *"When indentation is
required, you must use spaces, not tabs. F# code doesn't use tabs, and the compiler will give an
error if a tab character is encountered outside a string literal or comment."* So this rule reports a
file that will not build, not a file that offends someone's taste.

And it cannot be fixed by machine. Converting a tab to spaces requires knowing the indent width the
author meant. Under the offside rule a wrong width silently moves the line into a different block —
a different `match` arm, a different `let` body — and the file still compiles, doing something else.
No tool can recover that intent from the bytes, so this one reports and stops. Fix it in your editor,
where you can see the block structure you meant.

### How files are reached

- **`.fs` and `.fsi`** are `<Compile>` items of an `.fsproj`. The F# SDK sets
  `EnableDefaultCompileItems=false`, so a project lists every file explicitly and `dotnet-fast`
  evaluates that list — no implicit `**/*.fs` glob, and compile order is preserved.
- **`.fsx` scripts are not compile items of anything**, so no amount of project evaluation finds
  one. They are picked up by walking each F# project's directory, pruning `bin`/`obj` and the usual
  VCS/IDE folders. The consequence: a script that lives outside every `.fsproj` directory is not
  reached, and a repository with no `.fsproj` at all has no F# files as far as this command is
  concerned.

### Which commands run them

`dotnet-fast lint`, bare `dotnet-fast`, and `lint --fix`. The `dotnet format`-compatible verbs —
`format`, `whitespace`, `style`, `analyzers`, and the `dotnet-format-fast` binary — are unchanged and
never touch F#: real `dotnet format` is Roslyn-based and never processed `.fs`, so moving those would
be a compatibility regression rather than a feature. A C#-only repository is unaffected in every
mode.

### No parser, on purpose

These are checks on bytes; the F# grammar is never consulted. That is what keeps them working on the
roughly 5% of real-world F# files a tree-sitter grammar cannot parse — precisely where you most need
something to still work. The one piece of lexical state that *is* tracked exists only to **suppress**
findings, never to create them: a line whose trailing whitespace sits inside a `"""…"""` or `@"…"`
literal is skipped (there the spaces are string data, and trimming them would change a value), and a
line beginning inside a string literal or a `(* … *)` block comment is skipped by `FSH0004` (the
compiler permits tabs there). Being wrong about that costs a missed finding, never a wrong rewrite.

### `.editorconfig`

The same keys and the same severity vocabulary as every other rule — see
[editorconfig.md](editorconfig.md#f-hygiene-fsh0001fsh0004).

```ini
[*.{fs,fsi,fsx}]
end_of_line = lf                             # FSH0003 measures against this
insert_final_newline = true                  # FSH0002; true is the default
trim_trailing_whitespace = true              # FSH0001; true is the default
dotnet_diagnostic.FSH0004.severity = error   # per-rule severity, as usual
```

`dotnet_diagnostic.FSH0001.severity = none` removes the finding from the report **and** withholds the
rewrite, exactly as it does for a `DFxxxx` rule.

These four ids are not part of the `DFxxxx` count `lint --list-rules` prints — that number describes
what a C# run reports — but `lint --explain FSH0004` works and prints the full description above.

---

☕ Find `dotnet-fast` useful? [**Buy me a coffee**](https://buymeacoffee.com/rdll) — thanks for the support!

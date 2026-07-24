# Ported Roslyn analyzers

`dotnet-fast` re-implements popular Roslyn analyzers as **native, Roslyn-free** rules that run in the default `lint` path — no `--deep`, no .NET SDK, no restore. Each port is verified at **exact parity** against the real analyzer (same `(file, line, column)` findings) and runs orders of magnitude faster.

**679 analyzers ported** (91 with an autofix), across SonarAnalyzer, Microsoft.CodeAnalysis.NetAnalyzers, StyleCop, Roslynator, Meziantou, Microsoft.VisualStudio.Threading.Analyzers, AsyncFixer, Microsoft.EntityFrameworkCore.Analyzers, CSharpGuidelinesAnalyzer, IDisposableAnalyzers, NUnit.Analyzers, Philips.CodeAnalysis.MaintainabilityAnalyzers, SecurityCodeScan, Text.Analyzers, and Gu.Analyzers.

The ports are a **bundled superset, active by default** — they run regardless of which analyzer packages your project actually references, so `lint` can report rule IDs a plain `dotnet build` or `dotnet format` would never surface for your project. Pass `--only-active-analyzers` to restrict them to the analyzers your project references.

## Enabling and disabling ports

Ported analyzers are **on by default** — they report (and `--fix`-rewrite, where they have a fix) in the default `lint` path, no `--deep` required. Tune them in `.editorconfig` with the standard analyzer-config keys, following the usual Roslyn precedence — a per-rule `dotnet_diagnostic.<id>.severity` wins over a per-category `dotnet_analyzer_diagnostic.category-<Category>.severity`, which wins over the bulk `dotnet_analyzer_diagnostic.severity`:

```ini
[*.cs]
# silence one rule…
dotnet_diagnostic.CA1820.severity = none
# …promote a whole category to error…
dotnet_analyzer_diagnostic.category-Correctness.severity = error
# …or turn every ported analyzer off and re-enable only what you want.
dotnet_analyzer_diagnostic.severity = none
```

**Bulk/category keys only tune an already-enabled analyzer.** A handful of ported rules reproduce a real analyzer that is *disabled by default* upstream (`IsEnabledByDefault = false` — e.g. `CA1819`, `CA1002`, `CA2007`; see the table below for the full set). For those, `dotnet_analyzer_diagnostic.severity` and `dotnet_analyzer_diagnostic.category-<Category>.severity` cannot turn them on — exactly like real `dotnet format`/Roslyn, where the bulk/category severity keys only *re-level* an analyzer that already runs, never resurrect one that doesn't. Only a specific `dotnet_diagnostic.<ID>.severity` entry re-enables a disabled-by-default rule:

```ini
[*.cs]
# blanket off, curated categories back on — CA1819 stays OFF (disabled by
# default upstream), even though it's in the Performance category:
dotnet_analyzer_diagnostic.severity = none
dotnet_analyzer_diagnostic.category-Performance.severity = warning

# the only way to opt a disabled-by-default rule back in:
dotnet_diagnostic.CA1819.severity = warning
```

`dotnet-fast lint --explain <id>` shows a rule's details and upstream link.

### Recommended profile (curated default)

On a large codebase, on-by-default surfaces a lot of subjective style/documentation warnings that can drown the high-signal rules. To start from a curated set — the bug-class, dead-code and redundancy rules on at `warning`, the subjective Style and Maintainability families off — run:

```sh
# print the curated profile (with reasoning + this doc link)…
dotnet-fast editorconfig recommend
# …or append it straight to ./.editorconfig:
dotnet-fast editorconfig recommend --write
```

It disables every ported analyzer, then re-enables the `Correctness`, `Concurrency`, `Performance`, and `Redundancy` categories — so a fresh `lint` reports real defects, and you opt into the style/docs families when your team wants them.

## Rules

| Rule | Port of | Category | Fix | Summary |
|------|---------|----------|-----|---------|
| `AV1008` | CSharpGuidelinesAnalyzer AV1008 | Maintainability | report-only | Avoid static classes (unless suffixed 'Extensions'). |
| `AV1010` | CSharpGuidelinesAnalyzer AV1010 | Maintainability | report-only | Don't hide inherited members with the `new` keyword. |
| `AV1115` | CSharpGuidelinesAnalyzer AV1115 | Maintainability | report-only | A method should do only one thing (its name should not contain the word 'and'). |
| `AV1225` | CSharpGuidelinesAnalyzer AV1225 | Maintainability | report-only | Use a protected virtual method to raise each event. |
| `AV1500` | CSharpGuidelinesAnalyzer AV1500 | Maintainability | report-only | Methods should not exceed 7 statements. |
| `AV1535` | CSharpGuidelinesAnalyzer AV1535 | Maintainability | report-only | Always add a block after a case or default clause. |
| `AV1537` | CSharpGuidelinesAnalyzer AV1537 | Maintainability | report-only | Finish every if-else-if statement with an else clause. |
| `AV1561` | CSharpGuidelinesAnalyzer AV1561 | Maintainability | report-only | Don't declare signatures with more than three parameters. |
| `AV1562` | CSharpGuidelinesAnalyzer AV1562 | Maintainability | report-only | Don't use ref or out parameters. |
| `AV1564` | CSharpGuidelinesAnalyzer AV1564 | Maintainability | report-only | Avoid signatures that take a bool parameter. |
| `AV1704` | CSharpGuidelinesAnalyzer AV1704 | Maintainability | report-only | Don't include digits in type, member, parameter or variable names. |
| `AV1706` | CSharpGuidelinesAnalyzer AV1706 | Maintainability | report-only | Don't use abbreviations or a single letter in identifier names. |
| `AV1708` | CSharpGuidelinesAnalyzer AV1708 | Maintainability | report-only | Name types using nouns (avoid Helper/Utility/Common/Shared terms). |
| `AV1710` | CSharpGuidelinesAnalyzer AV1710 | Style | report-only | Don't repeat the name of a class or enumeration in its members. |
| `AV1739` | CSharpGuidelinesAnalyzer AV1739 | Maintainability | report-only | Use an underscore for irrelevant lambda parameters. |
| `AV2407` | CSharpGuidelinesAnalyzer AV2407 | Maintainability | report-only | Do not use #region. |
| `AsyncFixer02` | AsyncFixer AsyncFixer02 | Concurrency | report-only | Thread.Sleep blocks the thread inside an async method. |
| `AsyncFixer03` | AsyncFixer AsyncFixer03 | Concurrency | report-only | Avoid fire-and-forget async-void methods or delegates. |
| `CA1000` | Microsoft.CodeAnalysis.NetAnalyzers CA1000 | Maintainability | report-only | Do not declare static members on generic types. |
| `CA1002` | Microsoft.CodeAnalysis.NetAnalyzers CA1002 | Maintainability | report-only | Do not expose generic lists. |
| `CA1003` | Microsoft.CodeAnalysis.NetAnalyzers CA1003 | Maintainability | report-only | Use generic event handler instances. |
| `CA1005` | Microsoft.CodeAnalysis.NetAnalyzers CA1005 | Maintainability | report-only | Avoid excessive parameters on generic types. |
| `CA1008` | Microsoft.CodeAnalysis.NetAnalyzers CA1008 | Maintainability | report-only | Enums should have zero value. |
| `CA1010` | Microsoft.CodeAnalysis.NetAnalyzers CA1010 | Maintainability | report-only | Collections should implement generic interface. |
| `CA1012` | Microsoft.CodeAnalysis.NetAnalyzers CA1012 | Maintainability | report-only | Abstract types should not have public constructors. |
| `CA1018` | Microsoft.CodeAnalysis.NetAnalyzers CA1018 | Maintainability | report-only | Mark attributes with AttributeUsageAttribute. |
| `CA1019` | Microsoft.CodeAnalysis.NetAnalyzers CA1019 | Maintainability | report-only | Define accessors for attribute arguments. |
| `CA1021` | Microsoft.CodeAnalysis.NetAnalyzers CA1021 | Maintainability | report-only | Avoid out parameters. |
| `CA1024` | Microsoft.CodeAnalysis.NetAnalyzers CA1024 | Maintainability | report-only | Use properties where appropriate. |
| `CA1027` | Microsoft.CodeAnalysis.NetAnalyzers CA1027 | Maintainability | report-only | Mark enums with FlagsAttribute. |
| `CA1031` | Microsoft.CodeAnalysis.NetAnalyzers CA1031 | Correctness | report-only | Do not catch general exception types. |
| `CA1033` | Microsoft.CodeAnalysis.NetAnalyzers CA1033 | Maintainability | report-only | Interface methods should be callable by child types. |
| `CA1034` | Microsoft.CodeAnalysis.NetAnalyzers CA1034 | Maintainability | report-only | Nested types should not be visible. |
| `CA1036` | Microsoft.CodeAnalysis.NetAnalyzers CA1036 | Maintainability | report-only | Override methods on comparable types. |
| `CA1040` | Microsoft.CodeAnalysis.NetAnalyzers CA1040 | Maintainability | report-only | Avoid empty interfaces. |
| `CA1041` | Microsoft.CodeAnalysis.NetAnalyzers CA1041 | Maintainability | report-only | Provide a message for ObsoleteAttribute. |
| `CA1044` | Microsoft.CodeAnalysis.NetAnalyzers CA1044 | Maintainability | report-only | Properties should not be write only. |
| `CA1045` | Microsoft.CodeAnalysis.NetAnalyzers CA1045 | Maintainability | report-only | Do not pass types by reference. |
| `CA1046` | Microsoft.CodeAnalysis.NetAnalyzers CA1046 | Maintainability | report-only | Do not overload operator equals on reference types. |
| `CA1047` | Microsoft.CodeAnalysis.NetAnalyzers CA1047 | Maintainability | report-only | Do not declare protected members in sealed types. |
| `CA1050` | Microsoft.CodeAnalysis.NetAnalyzers CA1050 | Maintainability | report-only | Declare types in namespaces. |
| `CA1051` | Microsoft.CodeAnalysis.NetAnalyzers CA1051 | Maintainability | report-only | Do not declare visible instance fields. |
| `CA1052` | Microsoft.CodeAnalysis.NetAnalyzers CA1052 | Maintainability | report-only | Static holder types should be static or sealed. |
| `CA1054` | Microsoft.CodeAnalysis.NetAnalyzers CA1054 | Maintainability | report-only | URI parameters should not be strings. |
| `CA1055` | Microsoft.CodeAnalysis.NetAnalyzers CA1055 | Maintainability | report-only | URI return values should not be strings. |
| `CA1056` | Microsoft.CodeAnalysis.NetAnalyzers CA1056 | Maintainability | report-only | URI properties should not be strings. |
| `CA1064` | Microsoft.CodeAnalysis.NetAnalyzers CA1064 | Maintainability | report-only | Exceptions should be public. |
| `CA1065` | Microsoft.CodeAnalysis.NetAnalyzers CA1065 | Maintainability | report-only | Do not raise exceptions in unexpected locations. |
| `CA1066` | Microsoft.CodeAnalysis.NetAnalyzers CA1066 | Maintainability | report-only | Implement IEquatable when overriding Equals. |
| `CA1067` | Microsoft.CodeAnalysis.NetAnalyzers CA1067 | Maintainability | report-only | Override Equals when implementing IEquatable. |
| `CA1068` | Microsoft.CodeAnalysis.NetAnalyzers CA1068 | Maintainability | report-only | CancellationToken parameters must come last. |
| `CA1069` | Microsoft.CodeAnalysis.NetAnalyzers CA1069 | Correctness | report-only | Enums should not have duplicate values. |
| `CA1507` | Microsoft.CodeAnalysis.NetAnalyzers CA1507 | Maintainability | report-only | Use nameof in place of a string. |
| `CA1510` | Microsoft.CodeAnalysis.NetAnalyzers CA1510 | Maintainability | report-only | Use the ArgumentNullException throw helper. |
| `CA1512` | Microsoft.CodeAnalysis.NetAnalyzers CA1512 | Maintainability | report-only | Use the ArgumentOutOfRangeException throw helper. |
| `CA1513` | Microsoft.CodeAnalysis.NetAnalyzers CA1513 | Maintainability | report-only | Use the ObjectDisposedException throw helper. |
| `CA1700` | Microsoft.CodeAnalysis.NetAnalyzers CA1700 | Maintainability | report-only | Do not name enum values 'Reserved'. |
| `CA1704` | Text.Analyzers CA1704 | Style | report-only | Identifiers should be spelled correctly. |
| `CA1707` | Microsoft.CodeAnalysis.NetAnalyzers CA1707 | Style | report-only | Identifiers should not contain underscores. |
| `CA1708` | Microsoft.CodeAnalysis.NetAnalyzers CA1708 | Maintainability | report-only | Identifiers should differ by more than case. |
| `CA1710` | Microsoft.CodeAnalysis.NetAnalyzers CA1710 | Maintainability | report-only | Identifiers should have correct suffix. |
| `CA1711` | Microsoft.CodeAnalysis.NetAnalyzers CA1711 | Maintainability | report-only | Identifiers should not have incorrect suffix. |
| `CA1714` | Text.Analyzers CA1714 | Style | report-only | Flags enums should have plural names. |
| `CA1715` | Microsoft.CodeAnalysis.NetAnalyzers CA1715 | Maintainability | report-only | Identifiers should have correct prefix (interfaces `I`, type parameters `T`). |
| `CA1716` | Microsoft.CodeAnalysis.NetAnalyzers CA1716 | Maintainability | report-only | Identifier conflicts with a reserved language keyword. |
| `CA1717` | Text.Analyzers CA1717 | Style | report-only | Only FlagsAttribute enums should have plural names. |
| `CA1720` | Microsoft.CodeAnalysis.NetAnalyzers CA1720 | Maintainability | report-only | Identifiers should not contain type names. |
| `CA1721` | Microsoft.CodeAnalysis.NetAnalyzers CA1721 | Maintainability | report-only | Property names should not match get methods. |
| `CA1805` | Microsoft.CodeAnalysis.NetAnalyzers CA1805 | Performance | yes | Do not initialize unnecessarily. |
| `CA1806` | Microsoft.CodeAnalysis.NetAnalyzers CA1806 | Performance | report-only | Do not ignore method results. |
| `CA1810` | Microsoft.CodeAnalysis.NetAnalyzers CA1810 | Performance | report-only | Initialize reference type static fields inline. |
| `CA1813` | Microsoft.CodeAnalysis.NetAnalyzers CA1813 | Performance | report-only | Avoid unsealed attributes. |
| `CA1814` | Microsoft.CodeAnalysis.NetAnalyzers CA1814 | Performance | report-only | Prefer jagged arrays over multidimensional. |
| `CA1815` | Microsoft.CodeAnalysis.NetAnalyzers CA1815 | Maintainability | report-only | Override equals and operator equals on value types. |
| `CA1819` | Microsoft.CodeAnalysis.NetAnalyzers CA1819 | Performance | report-only | Properties should not return arrays. |
| `CA1820` | Microsoft.CodeAnalysis.NetAnalyzers CA1820 | Performance | report-only | Test for empty strings using string length. |
| `CA1821` | Microsoft.CodeAnalysis.NetAnalyzers CA1821 | Performance | yes | Remove empty finalizers. |
| `CA1822` | Microsoft.CodeAnalysis.NetAnalyzers CA1822 | Performance | report-only | Member can be marked as static. |
| `CA1825` | Microsoft.CodeAnalysis.NetAnalyzers CA1825 | Performance | yes | Avoid zero-length array allocations. |
| `CA1834` | Microsoft.CodeAnalysis.NetAnalyzers CA1834 | Performance | report-only | Use StringBuilder.Append(char) for single characters. |
| `CA1847` | Microsoft.CodeAnalysis.NetAnalyzers CA1847 | Performance | report-only | Use a char literal for a single-character Contains lookup. |
| `CA1860` | Microsoft.CodeAnalysis.NetAnalyzers CA1860 | Performance | report-only | Array '.Any()' should compare Length to 0. |
| `CA1866` | Microsoft.CodeAnalysis.NetAnalyzers CA1866 | Performance | report-only | Use the char overload of IndexOf. |
| `CA1873` | Microsoft.CodeAnalysis.NetAnalyzers CA1873 | Performance | report-only | Avoid potentially expensive logging arguments. |
| `CA2002` | Microsoft.CodeAnalysis.NetAnalyzers CA2002 | Concurrency | report-only | Do not lock on objects with weak identity. |
| `CA2007` | Microsoft.CodeAnalysis.NetAnalyzers CA2007 | Concurrency | report-only | Task awaited without ConfigureAwait. |
| `CA2011` | Microsoft.CodeAnalysis.NetAnalyzers CA2011 | Correctness | report-only | Do not assign a property within its setter. |
| `CA2014` | Microsoft.CodeAnalysis.NetAnalyzers CA2014 | Performance | report-only | Do not use stackalloc in loops. |
| `CA2016` | Microsoft.CodeAnalysis.NetAnalyzers CA2016 | Correctness | report-only | A CancellationToken in scope should be forwarded to a callee that accepts one. |
| `CA2200` | Microsoft.CodeAnalysis.NetAnalyzers CA2200 | Correctness | report-only | Rethrow to preserve stack details. |
| `CA2201` | Microsoft.CodeAnalysis.NetAnalyzers CA2201 | Maintainability | report-only | Do not raise reserved exception types. |
| `CA2208` | Microsoft.CodeAnalysis.NetAnalyzers CA2208 | Correctness | report-only | Instantiate argument exceptions correctly. |
| `CA2211` | Microsoft.CodeAnalysis.NetAnalyzers CA2211 | Maintainability | report-only | Non-constant fields should not be visible. |
| `CA2225` | Microsoft.CodeAnalysis.NetAnalyzers CA2225 | Maintainability | report-only | Operator overloads have named alternates. |
| `CA2227` | Microsoft.CodeAnalysis.NetAnalyzers CA2227 | Maintainability | report-only | Collection properties should be read only. |
| `CA2231` | Microsoft.CodeAnalysis.NetAnalyzers CA2231 | Correctness | report-only | Overload operator equals on overriding value type Equals. |
| `CA2241` | Microsoft.CodeAnalysis.NetAnalyzers CA2241 | Correctness | report-only | Provide correct arguments to formatting methods. |
| `CA2245` | Microsoft.CodeAnalysis.NetAnalyzers CA2245 | Correctness | report-only | Do not assign a property to itself. |
| `CA2249` | Microsoft.CodeAnalysis.NetAnalyzers CA2249 | Maintainability | report-only | Use string.Contains instead of string.IndexOf. |
| `CA5350` | Microsoft.CodeAnalysis.NetAnalyzers CA5350 | Correctness | report-only | Do not use weak cryptographic algorithms. |
| `CA5351` | Microsoft.CodeAnalysis.NetAnalyzers CA5351 | Correctness | report-only | Do not use broken cryptographic algorithms. |
| `CA5359` | Microsoft.CodeAnalysis.NetAnalyzers CA5359 | Correctness | report-only | Do not disable certificate validation. |
| `CA5384` | Microsoft.CodeAnalysis.NetAnalyzers CA5384 | Correctness | report-only | Do not use the Digital Signature Algorithm (DSA). |
| `EF1002` | Microsoft.EntityFrameworkCore.Analyzers EF1002 | Correctness | report-only | Interpolated SQL passed to a raw-SQL EF Core method loses auto-parameterization. |
| `EF1003` | Microsoft.EntityFrameworkCore.Analyzers EF1003 | Correctness | report-only | Concatenated SQL passed to a raw-SQL EF Core method risks SQL injection. |
| `GU0005` | Gu.Analyzers GU0005 | Correctness | report-only | Use correct argument positions. |
| `GU0009` | Gu.Analyzers GU0009 | Style | report-only | Name the boolean argument. |
| `GU0010` | Gu.Analyzers GU0010 | Correctness | report-only | Assigning same value. |
| `GU0017` | Gu.Analyzers GU0017 | Maintainability | report-only | Don't read a variable named `_` as a value. |
| `GU0061` | Gu.Analyzers GU0061 | Correctness | report-only | Enum member value is too large. |
| `GU0073` | Gu.Analyzers GU0073 | Correctness | report-only | Member of non-public type should be internal. |
| `GU0090` | Gu.Analyzers GU0090 | Maintainability | report-only | Don't throw NotImplementedException. |
| `IDE0028` | dotnet/roslyn IDE0028 (Microsoft.CodeAnalysis.CSharp.CodeStyle) | Style | report-only | Collection initialization can use a collection expression. |
| `IDE0090` | dotnet/roslyn IDE0090 (Microsoft.CodeAnalysis.CSharp.CodeStyle) | Style | report-only | 'new' expression can use target-typed 'new()'. |
| `IDE0290` | dotnet/roslyn IDE0290 (Microsoft.CodeAnalysis.CSharp.CodeStyle) | Style | report-only | Constructor can be converted to a primary constructor. |
| `IDISP018` | IDisposableAnalyzers IDISP018 | Correctness | report-only | Call SuppressFinalize when the type has a finalizer. |
| `IDISP019` | IDisposableAnalyzers IDISP019 | Correctness | report-only | Call SuppressFinalize when the type has a virtual dispose method. |
| `IDISP020` | IDisposableAnalyzers IDISP020 | Correctness | report-only | Call SuppressFinalize(this). |
| `IDISP021` | IDisposableAnalyzers IDISP021 | Correctness | report-only | Call this.Dispose(true). |
| `IDISP022` | IDisposableAnalyzers IDISP022 | Correctness | report-only | Call this.Dispose(false). |
| `IDISP024` | IDisposableAnalyzers IDISP024 | Redundancy | report-only | Don't call GC.SuppressFinalize(this) when the type is sealed and has no finalizer. |
| `IDISP025` | IDisposableAnalyzers IDISP025 | Correctness | report-only | Class with no virtual dispose method should be sealed. |
| `IDISP026` | IDisposableAnalyzers IDISP026 | Correctness | report-only | Class with no virtual DisposeAsyncCore method should be sealed. |
| `MA0003` | Meziantou.Analyzer MA0003 | Style | report-only | A bare null/boolean argument should be named for readability. |
| `MA0004` | Meziantou.Analyzer MA0004 | Concurrency | report-only | Await without ConfigureAwait(false). |
| `MA0005` | Meziantou.Analyzer MA0005 | Performance | yes | Use Array.Empty<T>() instead of allocating a zero-length array. |
| `MA0007` | Meziantou.Analyzer MA0007 | Style | yes | Add comma after the last value. |
| `MA0008` | Meziantou.Analyzer MA0008 | Performance | report-only | Add StructLayoutAttribute. |
| `MA0010` | Meziantou.Analyzer MA0010 | Maintainability | report-only | Mark attributes with AttributeUsageAttribute. |
| `MA0012` | Meziantou.Analyzer MA0012 | Maintainability | report-only | Do not raise reserved exception types. |
| `MA0015` | Meziantou.Analyzer MA0015 | Correctness | report-only | Specify the parameter name in ArgumentException. |
| `MA0016` | Meziantou.Analyzer MA0016 | Maintainability | report-only | Prefer using collection abstraction instead of implementation. |
| `MA0017` | Meziantou.Analyzer MA0017 | Maintainability | report-only | Abstract types should not have public or internal constructors. |
| `MA0018` | Meziantou.Analyzer MA0018 | Maintainability | report-only | Do not declare static members on generic types. |
| `MA0025` | Meziantou.Analyzer MA0025 | Maintainability | report-only | Implement the functionality instead of throwing NotImplementedException. |
| `MA0026` | Meziantou.Analyzer MA0026 | Maintainability | report-only | Fix TODO comments. |
| `MA0027` | Meziantou.Analyzer MA0027 | Correctness | report-only | Prefer rethrowing an exception implicitly. |
| `MA0028` | Meziantou.Analyzer MA0028 | Performance | report-only | Optimize StringBuilder usage (Append a char, not a one-char string). |
| `MA0029` | Meziantou.Analyzer MA0029 | Performance | report-only | Combine LINQ methods. |
| `MA0036` | Meziantou.Analyzer MA0036 | Maintainability | report-only | Make class static. |
| `MA0037` | Meziantou.Analyzer MA0037 | Redundancy | yes | Remove empty statement. |
| `MA0040` | Meziantou.Analyzer MA0040 | Concurrency | report-only | A CancellationToken in scope should be forwarded to a callee that accepts one. |
| `MA0042` | Meziantou.Analyzer MA0042 | Concurrency | report-only | Do not use Thread.Sleep in an async method. |
| `MA0043` | Meziantou.Analyzer MA0043 | Maintainability | report-only | Use nameof operator. |
| `MA0044` | Meziantou.Analyzer MA0044 | Redundancy | yes | Useless ToString call on a string. |
| `MA0046` | Meziantou.Analyzer MA0046 | Maintainability | report-only | Use EventHandler<T> to declare events. |
| `MA0047` | Meziantou.Analyzer MA0047 | Maintainability | report-only | Declare types in namespaces. |
| `MA0051` | Meziantou.Analyzer MA0051 | Maintainability | report-only | Method is too long. |
| `MA0055` | Meziantou.Analyzer MA0055 | Maintainability | report-only | Do not use finalizers. |
| `MA0058` | Meziantou.Analyzer MA0058 | Style | report-only | Exception class names should end with 'Exception'. |
| `MA0064` | Meziantou.Analyzer MA0064 | Concurrency | report-only | Avoid locking on publicly accessible instance. |
| `MA0069` | Meziantou.Analyzer MA0069 | Maintainability | report-only | Non-constant static fields should not be visible. |
| `MA0070` | Meziantou.Analyzer MA0070 | Maintainability | report-only | Obsolete attributes should include explanations. |
| `MA0071` | Meziantou.Analyzer MA0071 | Redundancy | report-only | Avoid using a redundant else. |
| `MA0073` | Meziantou.Analyzer MA0073 | Redundancy | report-only | Avoid comparison with bool constant. |
| `MA0077` | Meziantou.Analyzer MA0077 | Maintainability | report-only | A class that provides Equals(T) should implement IEquatable<T>. |
| `MA0089` | Meziantou.Analyzer MA0089 | Performance | report-only | string.Join with a single-char string separator. |
| `MA0090` | Meziantou.Analyzer MA0090 | Redundancy | report-only | Remove an empty finally/else block. |
| `MA0095` | Meziantou.Analyzer MA0095 | Maintainability | report-only | A class that implements IEquatable<T> should override Equals(object). |
| `MA0096` | Meziantou.Analyzer MA0096 | Maintainability | report-only | A class implementing IComparable<T> should override comparison operators. |
| `MA0097` | Meziantou.Analyzer MA0097 | Maintainability | report-only | A class implementing IComparable<T> should also implement IComparable. |
| `MA0099` | Meziantou.Analyzer MA0099 | Style | report-only | A bare 0 should be an explicit enum member. |
| `MA0101` | Meziantou.Analyzer MA0101 | Correctness | report-only | String contains an implicit end of line character. |
| `MA0102` | Meziantou.Analyzer MA0102 | Performance | report-only | Make member readonly. |
| `MA0136` | Meziantou.Analyzer MA0136 | Style | report-only | Raw string literals should not use an implicit end of line. |
| `MA0140` | Meziantou.Analyzer MA0140 | Correctness | report-only | Both if and else branch have identical code. |
| `MA0143` | Meziantou.Analyzer MA0143 | Maintainability | report-only | A primary constructor parameter is reassigned in the type body. |
| `MA0159` | Meziantou.Analyzer MA0159 | Performance | report-only | Use 'Order' instead of 'OrderBy'. |
| `MA0182` | Meziantou.Analyzer MA0182 | Maintainability | report-only | Detected internal type that is never used. |
| `MA0184` | Meziantou.Analyzer MA0184 | Redundancy | report-only | Do not use an interpolated string without parameters. |
| `MA0192` | Meziantou.Analyzer MA0192 | Style | report-only | Use HasFlag instead of bitwise checks. |
| `MA0196` | Meziantou.Analyzer MA0196 | Maintainability | report-only | Do not use '<inheritdoc />' on members that do not inherit documentation. |
| `MA0203` | Meziantou.Analyzer MA0203 | Maintainability | report-only | Do not use a return tag for a void method. |
| `MA0204` | Meziantou.Analyzer MA0204 | Redundancy | report-only | Remove an unnecessary partial modifier. |
| `MA0206` | Meziantou.Analyzer MA0206 | Redundancy | report-only | Remove unnecessary braces from an empty-body type declaration. |
| `MA0211` | Meziantou.Analyzer MA0211 | Style | report-only | Use multi-line syntax for XML summary comments. |
| `NUnit1002` | NUnit.Analyzers NUnit1002 | Maintainability | report-only | The TestCaseSource should use the nameof operator to specify its target. |
| `NUnit1021` | NUnit.Analyzers NUnit1021 | Maintainability | report-only | The ValueSource should use the nameof operator to specify its target. |
| `NUnit2001` | NUnit.Analyzers NUnit2001 | Style | report-only | Use Assert.That with Is.False instead of ClassicAssert.False. |
| `NUnit2002` | NUnit.Analyzers NUnit2002 | Style | report-only | Use Assert.That with Is.False instead of ClassicAssert.IsFalse. |
| `NUnit2003` | NUnit.Analyzers NUnit2003 | Style | report-only | Use Assert.That with Is.True instead of ClassicAssert.IsTrue. |
| `NUnit2004` | NUnit.Analyzers NUnit2004 | Style | report-only | Use Assert.That with Is.True instead of ClassicAssert.True. |
| `NUnit2005` | NUnit.Analyzers NUnit2005 | Style | report-only | Use Assert.That with Is.EqualTo instead of ClassicAssert.AreEqual. |
| `NUnit2006` | NUnit.Analyzers NUnit2006 | Style | report-only | Use Assert.That with Is.Not.EqualTo instead of ClassicAssert.AreNotEqual. |
| `NUnit2009` | NUnit.Analyzers NUnit2009 | Correctness | report-only | The actual and the expected argument are the same in an assertion. |
| `NUnit2015` | NUnit.Analyzers NUnit2015 | Style | report-only | Use Assert.That with Is.SameAs instead of ClassicAssert.AreSame. |
| `NUnit2016` | NUnit.Analyzers NUnit2016 | Style | report-only | Use Assert.That with Is.Null instead of ClassicAssert.Null. |
| `NUnit2017` | NUnit.Analyzers NUnit2017 | Style | report-only | Use Assert.That with Is.Null instead of ClassicAssert.IsNull. |
| `NUnit2018` | NUnit.Analyzers NUnit2018 | Style | report-only | Use Assert.That with Is.Not.Null instead of ClassicAssert.NotNull. |
| `NUnit2019` | NUnit.Analyzers NUnit2019 | Style | report-only | Use Assert.That with Is.Not.Null instead of ClassicAssert.IsNotNull. |
| `NUnit2027` | NUnit.Analyzers NUnit2027 | Style | report-only | Use Assert.That with Is.GreaterThan instead of ClassicAssert.Greater. |
| `NUnit2028` | NUnit.Analyzers NUnit2028 | Style | report-only | Use Assert.That with Is.GreaterThanOrEqualTo instead of ClassicAssert.GreaterOrEqual. |
| `NUnit2029` | NUnit.Analyzers NUnit2029 | Style | report-only | Use Assert.That with Is.LessThan instead of ClassicAssert.Less. |
| `NUnit2030` | NUnit.Analyzers NUnit2030 | Style | report-only | Use Assert.That with Is.LessThanOrEqualTo instead of ClassicAssert.LessOrEqual. |
| `NUnit2031` | NUnit.Analyzers NUnit2031 | Style | report-only | Use Assert.That with Is.Not.SameAs instead of ClassicAssert.AreNotSame. |
| `NUnit2032` | NUnit.Analyzers NUnit2032 | Style | report-only | Use Assert.That with Is.Zero instead of ClassicAssert.Zero. |
| `NUnit2033` | NUnit.Analyzers NUnit2033 | Style | report-only | Use Assert.That with Is.Not.Zero instead of ClassicAssert.NotZero. |
| `NUnit2034` | NUnit.Analyzers NUnit2034 | Style | report-only | Use Assert.That with Is.NaN instead of ClassicAssert.IsNaN. |
| `NUnit2035` | NUnit.Analyzers NUnit2035 | Style | report-only | Use Assert.That with Is.Empty instead of ClassicAssert.IsEmpty. |
| `NUnit2036` | NUnit.Analyzers NUnit2036 | Style | report-only | Use Assert.That with Is.Not.Empty instead of ClassicAssert.IsNotEmpty. |
| `NUnit2037` | NUnit.Analyzers NUnit2037 | Style | report-only | Use Assert.That with Does.Contain instead of ClassicAssert.Contains. |
| `NUnit2038` | NUnit.Analyzers NUnit2038 | Style | report-only | Use Assert.That with Is.InstanceOf instead of ClassicAssert.IsInstanceOf. |
| `NUnit2039` | NUnit.Analyzers NUnit2039 | Style | report-only | Use Assert.That with Is.Not.InstanceOf instead of ClassicAssert.IsNotInstanceOf. |
| `NUnit2048` | NUnit.Analyzers NUnit2048 | Style | report-only | Use Assert.That instead of StringAssert. |
| `NUnit2049` | NUnit.Analyzers NUnit2049 | Style | report-only | Use Assert.That instead of CollectionAssert. |
| `NUnit2051` | NUnit.Analyzers NUnit2051 | Style | report-only | Use Assert.That with Is.Positive instead of ClassicAssert.Positive. |
| `NUnit2052` | NUnit.Analyzers NUnit2052 | Style | report-only | Use Assert.That with Is.Negative instead of ClassicAssert.Negative. |
| `NUnit2053` | NUnit.Analyzers NUnit2053 | Style | report-only | Use Assert.That with Is.AssignableFrom instead of ClassicAssert.IsAssignableFrom. |
| `NUnit2054` | NUnit.Analyzers NUnit2054 | Style | report-only | Use Assert.That with Is.Not.AssignableFrom instead of ClassicAssert.IsNotAssignableFrom. |
| `PH2021` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2021 | Style | report-only | Do not inline `new T()` calls into a member access. |
| `PH2026` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2026 | Maintainability | report-only | Avoid `[SuppressMessage]`. |
| `PH2029` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2029 | Maintainability | report-only | Avoid `#pragma warning` directives. |
| `PH2032` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2032 | Maintainability | report-only | Avoid an empty static constructor. |
| `PH2044` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2044 | Maintainability | report-only | Avoid the dynamic keyword. |
| `PH2064` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2064 | Maintainability | report-only | Avoid duplicate #region names. |
| `PH2068` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2068 | Maintainability | report-only | Avoid goto. |
| `PH2070` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2070 | Maintainability | report-only | Avoid `protected` fields. |
| `PH2077` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2077 | Maintainability | report-only | Avoid a switch statement with only a default case. |
| `PH2080` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2080 | Maintainability | report-only | Avoid hardcoded absolute Windows paths in string literals. |
| `PH2081` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2081 | Maintainability | report-only | Avoid #region within a method body. |
| `PH2082` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2082 | Style | report-only | Use positive wording for variable and property names. |
| `PH2084` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2084 | Concurrency | report-only | Don't lock on a newly-created object. |
| `PH2085` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2085 | Maintainability | report-only | Property accessors should be ordered get then set/init. |
| `PH2089` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2089 | Correctness | report-only | Avoid assignment in a condition. |
| `PH2092` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2092 | Maintainability | report-only | Limit the number of `&&`/`||` clauses in a condition. |
| `PH2093` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2093 | Maintainability | report-only | Prefer named-field tuples. |
| `PH2096` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2096 | Concurrency | report-only | Prefer async Task methods over async void methods. |
| `PH2097` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2097 | Maintainability | report-only | Avoid empty statement blocks. |
| `PH2098` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2098 | Maintainability | report-only | Avoid empty catch blocks. |
| `PH2104` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2104 | Style | report-only | Put every LINQ query-expression clause on a separate line. |
| `PH2105` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2105 | Maintainability | report-only | Overload `-` when you overload `+` (and vice versa). |
| `PH2106` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2106 | Maintainability | report-only | Overload `/` when you overload `*` (and vice versa). |
| `PH2107` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2107 | Maintainability | report-only | Overload `<` when you overload `>` (and vice versa). |
| `PH2108` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2108 | Maintainability | report-only | Overload `<=` when you overload `>=` (and vice versa). |
| `PH2109` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2109 | Maintainability | report-only | Overload `<<` when you overload `>>` (and vice versa). |
| `PH2110` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2110 | Maintainability | report-only | Overload `--` when you overload `++` (and vice versa). |
| `PH2111` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2111 | Maintainability | report-only | Reduce a method's cognitive load (nested blocks + logical/control tokens). |
| `PH2113` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2113 | Maintainability | report-only | Merge a nested `if` into its outer `if`. |
| `PH2114` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2114 | Redundancy | yes | Avoid empty statements. |
| `PH2115` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2115 | Style | report-only | Avoid putting multiple lambda expressions on the same line. |
| `PH2116` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2116 | Performance | report-only | Avoid `System.Collections.ArrayList`. |
| `PH2122` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2122 | Correctness | report-only | Avoid throwing exceptions from unexpected locations. |
| `PH2123` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2123 | Correctness | report-only | Pass a non-null sender/args to a raised event. |
| `PH2125` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2125 | Maintainability | report-only | Overload `==` when you overload `+` (or `-`). |
| `PH2127` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2127 | Maintainability | report-only | Avoid changing `for` loop variables inside the loop body. |
| `PH2128` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2128 | Style | report-only | Split a multi-line `&&`/`||` condition with the operator at the end of the line. |
| `PH2130` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2130 | Correctness | report-only | Avoid implementing a finalizer. |
| `PH2132` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2132 | Style | report-only | Remove commented-out code. |
| `PH2140` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2140 | Maintainability | report-only | Avoid the `[ExcludeFromCodeCoverage]` attribute. |
| `PH2141` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2141 | Maintainability | report-only | Avoid empty #regions. |
| `PH2142` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2142 | Maintainability | report-only | Avoid declaring a conversion operator to `string`. |
| `PH2143` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2143 | Correctness | report-only | Avoid `Assembly.GetEntryAssembly()`. |
| `PH2144` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2144 | Maintainability | report-only | A backwards for-loop's boundary check should use >= 0. |
| `PH2147` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2147 | Maintainability | report-only | Avoid a variable named exactly `_`. |
| `PH2153` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2153 | Maintainability | report-only | Avoid calling ToString() when the result is discarded. |
| `PH2159` | Philips.CodeAnalysis.MaintainabilityAnalyzers PH2159 | Style | report-only | Avoid unnecessary empty parentheses in attributes. |
| `RCS0001` | Roslynator.Formatting.Analyzers RCS0001 | Style | report-only | Add a blank line after an embedded statement. |
| `RCS0002` | Roslynator.Formatting.Analyzers RCS0002 | Style | report-only | Add a blank line after a #region directive. |
| `RCS0005` | Roslynator.Formatting.Analyzers RCS0005 | Style | report-only | Add a blank line before a #endregion directive. |
| `RCS0006` | Roslynator.Formatting.Analyzers RCS0006 | Style | report-only | Add a blank line before the using directive list. |
| `RCS0007` | Roslynator.Formatting.Analyzers RCS0007 | Style | report-only | Add a blank line between accessors. |
| `RCS0008` | Roslynator.Formatting.Analyzers RCS0008 | Style | yes | Add a blank line between a closing brace and the next statement. |
| `RCS0009` | Roslynator.Formatting.Analyzers RCS0009 | Style | report-only | Add a blank line between a declaration and the next declaration's documentation comment. |
| `RCS0010` | Roslynator.Formatting.Analyzers RCS0010 | Style | report-only | Add a blank line between declarations. |
| `RCS0012` | Roslynator.Formatting.Analyzers RCS0012 | Style | yes | Add a blank line between single-line declarations. |
| `RCS0013` | Roslynator.Formatting.Analyzers RCS0013 | Style | yes | Add a blank line between single-line declarations of a different kind. |
| `RCS0016` | Roslynator.Formatting.Analyzers RCS0016 | Style | report-only | Put an attribute list on its own line. |
| `RCS0020` | Roslynator.Formatting.Analyzers RCS0020 | Style | report-only | Format an accessor's braces on multiple lines. |
| `RCS0021` | Roslynator.Formatting.Analyzers RCS0021 | Style | yes | Format a block's braces on multiple lines. |
| `RCS0023` | Roslynator.Formatting.Analyzers RCS0023 | Style | report-only | Format a type declaration's braces on multiple lines. |
| `RCS0024` | Roslynator.Formatting.Analyzers RCS0024 | Style | report-only | Add a new line after a switch label. |
| `RCS0025` | Roslynator.Formatting.Analyzers RCS0025 | Style | report-only | Put a full accessor on its own line. |
| `RCS0029` | Roslynator.Formatting.Analyzers RCS0029 | Style | report-only | Put a constructor initializer on its own line. |
| `RCS0030` | Roslynator.Formatting.Analyzers RCS0030 | Style | yes | Put an embedded statement on its own line. |
| `RCS0031` | Roslynator.Formatting.Analyzers RCS0031 | Style | report-only | Put each enum member on its own line. |
| `RCS0033` | Roslynator.Formatting.Analyzers RCS0033 | Style | yes | Put a statement on its own line. |
| `RCS0036` | Roslynator.Formatting.Analyzers RCS0036 | Style | report-only | Remove a blank line between single-line declarations of the same kind. |
| `RCS0048` | Roslynator.Formatting.Analyzers RCS0048 | Style | report-only | Put a single-element initializer on a single line. |
| `RCS0049` | Roslynator.Formatting.Analyzers RCS0049 | Style | yes | Add a blank line after the file's top comment. |
| `RCS0057` | Roslynator.Formatting.Analyzers RCS0057 | Style | report-only | Normalize whitespace at the beginning of a file. |
| `RCS0062` | Roslynator.Formatting.Analyzers RCS0062 | Style | yes | Put an expression body on its own line. |
| `RCS0063` | Roslynator.Formatting.Analyzers RCS0063 | Style | yes | Remove an unnecessary blank line. |
| `RCS1001` | Roslynator.Analyzers RCS1001 | Style | report-only | Add braces (when expression spans multiple lines). |
| `RCS1003` | Roslynator.Analyzers RCS1003 | Style | report-only | Add braces to if-else. |
| `RCS1005` | Roslynator.Analyzers RCS1005 | Redundancy | report-only | Simplify nested using statement. |
| `RCS1006` | Roslynator.Analyzers RCS1006 | Style | yes | Merge 'else' with nested 'if'. |
| `RCS1007` | Roslynator.Analyzers RCS1007 | Maintainability | report-only | Add braces to a single-statement control body. |
| `RCS1015` | Roslynator.Analyzers RCS1015 | Maintainability | report-only | Use nameof operator. |
| `RCS1019` | Roslynator.Analyzers RCS1019 | Style | report-only | Order modifiers. |
| `RCS1020` | Roslynator.Analyzers RCS1020 | Style | yes | Simplify Nullable<T> to T?. |
| `RCS1021` | Roslynator.Analyzers RCS1021 | Style | report-only | Convert lambda block body to expression body. |
| `RCS1031` | Roslynator.Analyzers RCS1031 | Redundancy | report-only | Remove unnecessary braces from a switch section. |
| `RCS1032` | Roslynator.Analyzers RCS1032 | Redundancy | report-only | Remove redundant parentheses. |
| `RCS1033` | Roslynator.Analyzers RCS1033 | Redundancy | report-only | Remove redundant boolean literal. |
| `RCS1036` | Roslynator.Analyzers RCS1036 | Style | yes | Remove redundant empty line. |
| `RCS1037` | Roslynator.Analyzers RCS1037 | Redundancy | yes | Remove trailing white-space. |
| `RCS1039` | Roslynator.Analyzers RCS1039 | Redundancy | yes | Remove empty argument lists from attributes. |
| `RCS1040` | Roslynator.Analyzers RCS1040 | Redundancy | yes | Remove empty else clause. |
| `RCS1043` | Roslynator.Analyzers RCS1043 | Redundancy | report-only | Remove 'partial' modifier from a type with a single part. |
| `RCS1044` | Roslynator.Analyzers RCS1044 | Correctness | yes | Remove original exception from throw statement. |
| `RCS1047` | Roslynator.Analyzers RCS1047 | Maintainability | report-only | Non-asynchronous method name should not end with 'Async'. |
| `RCS1048` | Roslynator.Analyzers RCS1048 | Style | yes | Use lambda expression instead of anonymous method. |
| `RCS1049` | Roslynator.Analyzers RCS1049 | Redundancy | report-only | Simplify boolean comparison. |
| `RCS1056` | Roslynator.Analyzers RCS1056 | Maintainability | report-only | Using alias directive. |
| `RCS1058` | Roslynator.Analyzers RCS1058 | Redundancy | report-only | Use compound assignment. |
| `RCS1059` | Roslynator.Analyzers RCS1059 | Concurrency | report-only | Avoid locking on publicly accessible instance. |
| `RCS1061` | Roslynator.Analyzers RCS1061 | Redundancy | report-only | Merge an if with its sole nested if. |
| `RCS1068` | Roslynator.Analyzers RCS1068 | Redundancy | report-only | Simplify logical negation. |
| `RCS1069` | Roslynator.Analyzers RCS1069 | Redundancy | report-only | Remove unnecessary case label. |
| `RCS1070` | Roslynator.Analyzers RCS1070 | Redundancy | report-only | Remove redundant default switch section. |
| `RCS1073` | Roslynator.Analyzers RCS1073 | Maintainability | report-only | Convert 'if' to 'return' statement. |
| `RCS1074` | Roslynator.Analyzers RCS1074 | Redundancy | yes | Remove redundant constructors. |
| `RCS1075` | Roslynator.Analyzers RCS1075 | Correctness | report-only | Avoid empty catch clauses that catch System.Exception. |
| `RCS1077` | Roslynator.Analyzers RCS1077 | Performance | report-only | Optimize LINQ method call. |
| `RCS1080` | Roslynator.Analyzers RCS1080 | Performance | report-only | Array '.Any()' should use the Length property. |
| `RCS1084` | Roslynator.Analyzers RCS1084 | Style | yes | Use coalesce expression instead of conditional expression. |
| `RCS1085` | Roslynator.Analyzers RCS1085 | Redundancy | report-only | Use auto-implemented property. |
| `RCS1089` | Roslynator.Analyzers RCS1089 | Redundancy | report-only | Use ++/-- operator instead of assignment. |
| `RCS1097` | Roslynator.Analyzers RCS1097 | Redundancy | yes | Redundant 'ToString' call on a string. |
| `RCS1098` | Roslynator.Analyzers RCS1098 | Style | yes | Constant values should be on the right side of comparisons. |
| `RCS1102` | Roslynator.Analyzers RCS1102 | Maintainability | report-only | Make class static. |
| `RCS1104` | Roslynator.Analyzers RCS1104 | Redundancy | yes | Simplify boolean conditional expressions. |
| `RCS1110` | Roslynator.Analyzers RCS1110 | Maintainability | report-only | Declare type inside a namespace. |
| `RCS1118` | Roslynator.Analyzers RCS1118 | Style | report-only | Mark local variable as const. |
| `RCS1123` | Roslynator.Analyzers RCS1123 | Maintainability | yes | Add parentheses when necessary. |
| `RCS1124` | Roslynator.Analyzers RCS1124 | Redundancy | report-only | Inline a local variable that is used only once. |
| `RCS1126` | Roslynator.Analyzers RCS1126 | Style | report-only | Add braces to if-else. |
| `RCS1129` | Roslynator.Analyzers RCS1129 | Redundancy | yes | Remove redundant field initialization. |
| `RCS1130` | Roslynator.Analyzers RCS1130 | Correctness | report-only | Bitwise operation on an enum without [Flags]. |
| `RCS1134` | Roslynator.Analyzers RCS1134 | Redundancy | report-only | Remove redundant statements. |
| `RCS1135` | Roslynator.Analyzers RCS1135 | Maintainability | report-only | Declare an enum member with value zero when the enum has FlagsAttribute. |
| `RCS1136` | Roslynator.Analyzers RCS1136 | Redundancy | report-only | Merge switch sections with equivalent content. |
| `RCS1138` | Roslynator.Analyzers RCS1138 | Maintainability | report-only | Add summary to documentation comment. |
| `RCS1139` | Roslynator.Analyzers RCS1139 | Maintainability | report-only | Add summary element to documentation comment. |
| `RCS1140` | Roslynator.Analyzers RCS1140 | Maintainability | report-only | Add exception to documentation comment. |
| `RCS1141` | Roslynator.Analyzers RCS1141 | Maintainability | report-only | Add 'param' element to documentation comment. |
| `RCS1142` | Roslynator.Analyzers RCS1142 | Maintainability | report-only | Add 'typeparam' element to documentation comment. |
| `RCS1146` | Roslynator.Analyzers RCS1146 | Redundancy | report-only | Use conditional access. |
| `RCS1156` | Roslynator.Analyzers RCS1156 | Performance | report-only | Use string.Length instead of comparison with empty string. |
| `RCS1158` | Roslynator.Analyzers RCS1158 | Maintainability | report-only | Static member in generic type should use a type parameter. |
| `RCS1160` | Roslynator.Analyzers RCS1160 | Maintainability | report-only | Abstract type should not have public constructors. |
| `RCS1161` | Roslynator.Analyzers RCS1161 | Maintainability | report-only | Enum should declare explicit values. |
| `RCS1163` | Roslynator.Analyzers RCS1163 | Redundancy | report-only | Unused parameter. |
| `RCS1164` | Roslynator.Analyzers RCS1164 | Redundancy | report-only | Unused type parameter. |
| `RCS1168` | Roslynator.Analyzers RCS1168 | Maintainability | report-only | Parameter name differs from base name. |
| `RCS1169` | Roslynator.Analyzers RCS1169 | Redundancy | report-only | Make field read-only. |
| `RCS1170` | Roslynator.Analyzers RCS1170 | Style | report-only | Use read-only auto-implemented property. |
| `RCS1179` | Roslynator.Analyzers RCS1179 | Redundancy | report-only | Unnecessary assignment. |
| `RCS1181` | Roslynator.Analyzers RCS1181 | Style | report-only | Convert comment to documentation comment. |
| `RCS1187` | Roslynator.Analyzers RCS1187 | Maintainability | report-only | Use constant instead of field. |
| `RCS1188` | Roslynator.Analyzers RCS1188 | Redundancy | yes | Remove redundant auto-property initialization. |
| `RCS1189` | Roslynator.Analyzers RCS1189 | Maintainability | report-only | Add region name to #endregion. |
| `RCS1190` | Roslynator.Analyzers RCS1190 | Style | report-only | Join string expressions. |
| `RCS1192` | Roslynator.Analyzers RCS1192 | Redundancy | yes | Unnecessary usage of verbatim string literal. |
| `RCS1194` | Roslynator.Analyzers RCS1194 | Maintainability | report-only | Implement the standard exception constructors. |
| `RCS1203` | Roslynator.Analyzers RCS1203 | Maintainability | report-only | Use AttributeUsageAttribute. |
| `RCS1206` | Roslynator.Analyzers RCS1206 | Maintainability | yes | Use conditional access instead of conditional expression. |
| `RCS1208` | Roslynator.Analyzers RCS1208 | Style | report-only | An if can be inverted into a guard to reduce nesting. |
| `RCS1209` | Roslynator.Analyzers RCS1209 | Style | report-only | Order type parameter constraints. |
| `RCS1211` | Roslynator.Analyzers RCS1211 | Redundancy | report-only | Remove unnecessary else clause. |
| `RCS1212` | Roslynator.Analyzers RCS1212 | Redundancy | report-only | Remove redundant assignment. |
| `RCS1213` | Roslynator.Analyzers RCS1213 | Redundancy | report-only | Remove unused member declaration. |
| `RCS1214` | Roslynator.Analyzers RCS1214 | Redundancy | yes | Unnecessary interpolated string. |
| `RCS1215` | Roslynator.Analyzers RCS1215 | Correctness | report-only | Expression is always equal to true or false. |
| `RCS1220` | Roslynator.Analyzers RCS1220 | Style | report-only | Use pattern matching instead of 'is' and cast. |
| `RCS1225` | Roslynator.Analyzers RCS1225 | Maintainability | report-only | Make class sealed. |
| `RCS1226` | Roslynator.Analyzers RCS1226 | Style | report-only | Add paragraph to documentation comment. |
| `RCS1228` | Roslynator.Analyzers RCS1228 | Redundancy | report-only | Unused element in documentation comment. |
| `RCS1232` | Roslynator.Analyzers RCS1232 | Maintainability | report-only | Order elements in documentation comment. |
| `RCS1233` | Roslynator.Analyzers RCS1233 | Correctness | report-only | Use short-circuiting operator. |
| `RCS1234` | Roslynator.Analyzers RCS1234 | Correctness | report-only | Duplicate enum value. |
| `RCS1238` | Roslynator.Analyzers RCS1238 | Maintainability | report-only | Avoid nested ?: operators. |
| `RCS1241` | Roslynator.Analyzers RCS1241 | Maintainability | report-only | Implement non-generic counterpart. |
| `RCS1242` | Roslynator.Analyzers RCS1242 | Performance | report-only | Non-readonly struct passed by 'in' reference. |
| `RCS1244` | Roslynator.Analyzers RCS1244 | Redundancy | report-only | Simplify 'default' expression. |
| `RCS1251` | Roslynator.Analyzers RCS1251 | Redundancy | yes | Remove unnecessary braces from an empty-body type. |
| `RCS1259` | Roslynator.Analyzers RCS1259 | Redundancy | yes | Remove empty syntax. |
| `RCS1262` | Roslynator.Analyzers RCS1262 | Redundancy | report-only | Unnecessary raw string literal. |
| `RCS1265` | Roslynator.Analyzers RCS1265 | Redundancy | report-only | Remove redundant catch block. |
| `S100` | SonarAnalyzer.CSharp S100 | Style | report-only | Methods and properties should be named in PascalCase. |
| `S1006` | SonarAnalyzer.CSharp S1006 | Correctness | report-only | Method overrides should not change parameter defaults. |
| `S101` | SonarAnalyzer.CSharp S101 | Style | report-only | Types should be named in PascalCase. |
| `S1066` | SonarAnalyzer.CSharp S1066 | Maintainability | report-only | Collapsible if statements should be merged. |
| `S1067` | SonarAnalyzer.CSharp S1067 | Maintainability | report-only | Expressions should not be too complex. |
| `S107` | SonarAnalyzer.CSharp S107 | Maintainability | report-only | Methods should not have too many parameters. |
| `S108` | SonarAnalyzer.CSharp S108 | Correctness | report-only | Nested blocks of code should not be left empty. |
| `S1104` | SonarAnalyzer.CSharp S1104 | Maintainability | report-only | Fields should not have public accessibility. |
| `S1110` | SonarAnalyzer.CSharp S1110 | Redundancy | yes | Redundant pairs of parentheses should be removed. |
| `S1116` | SonarAnalyzer.CSharp S1116 | Redundancy | yes | Empty statements should be removed. |
| `S1118` | SonarAnalyzer.CSharp S1118 | Maintainability | report-only | Utility classes should not have public constructors. |
| `S112` | SonarAnalyzer.CSharp S112 | Maintainability | report-only | General exceptions should never be thrown. |
| `S1123` | SonarAnalyzer.CSharp S1123 | Maintainability | report-only | 'Obsolete' attributes should include explanations. |
| `S1125` | SonarAnalyzer.CSharp S1125 | Redundancy | report-only | Boolean literals should not be redundant. |
| `S1133` | SonarAnalyzer.CSharp S1133 | Maintainability | report-only | Deprecated code should be removed. |
| `S1135` | SonarAnalyzer.CSharp S1135 | Maintainability | report-only | Track uses of TODO tags. |
| `S1144` | SonarAnalyzer.CSharp S1144 | Redundancy | report-only | Unused private types or members should be removed. |
| `S1172` | SonarAnalyzer.CSharp S1172 | Redundancy | report-only | Unused method parameters should be removed. |
| `S1186` | SonarAnalyzer.CSharp S1186 | Maintainability | report-only | Methods should not be empty. |
| `S1199` | SonarAnalyzer.CSharp S1199 | Maintainability | report-only | Nested code blocks should not be used. |
| `S1206` | SonarAnalyzer.CSharp S1206 | Maintainability | report-only | Equals(object) and GetHashCode() should be overridden in pairs. |
| `S121` | SonarAnalyzer.CSharp S121 | Maintainability | report-only | Control structures should use curly braces. |
| `S1210` | SonarAnalyzer.CSharp S1210 | Maintainability | report-only | 'IComparable' implementations should override comparison operators. |
| `S1226` | SonarAnalyzer.CSharp S1226 | Maintainability | report-only | Method parameters should not be reassigned. |
| `S1244` | SonarAnalyzer.CSharp S1244 | Correctness | report-only | Floating point numbers should not be tested for equality. |
| `S125` | SonarAnalyzer.CSharp S125 | Maintainability | report-only | Commented-out code should be removed. |
| `S1264` | SonarAnalyzer.CSharp S1264 | Maintainability | yes | A `while` loop should be used instead of a `for` loop with only a condition. |
| `S1301` | SonarAnalyzer.CSharp S1301 | Maintainability | report-only | switch statements should have at least 3 case clauses. |
| `S131` | SonarAnalyzer.CSharp S131 | Maintainability | report-only | switch statements should have default clauses. |
| `S134` | SonarAnalyzer.CSharp S134 | Maintainability | report-only | Control flow statements should not be nested too deeply. |
| `S138` | SonarAnalyzer.CSharp S138 | Maintainability | report-only | Functions should not have too many lines of code. |
| `S1481` | SonarAnalyzer.CSharp S1481 | Redundancy | report-only | Unused local variables should be removed. |
| `S1656` | SonarAnalyzer.CSharp S1656 | Correctness | report-only | Variables should not be self-assigned. |
| `S1696` | SonarAnalyzer.CSharp S1696 | Correctness | report-only | NullReferenceException should not be caught. |
| `S1751` | SonarAnalyzer.CSharp S1751 | Correctness | report-only | Loops with at most one iteration should be refactored. |
| `S1764` | SonarAnalyzer.CSharp S1764 | Correctness | report-only | Identical expressions should not be used on both sides of a binary operator. |
| `S1848` | SonarAnalyzer.CSharp S1848 | Correctness | report-only | Objects should not be created to be dropped immediately without being used. |
| `S1858` | SonarAnalyzer.CSharp S1858 | Redundancy | report-only | Redundant ToString() on a string. |
| `S1862` | SonarAnalyzer.CSharp S1862 | Correctness | report-only | Related `if`/`else if` conditions should not be the same. |
| `S1871` | SonarAnalyzer.CSharp S1871 | Correctness | report-only | Two `switch` sections should not have the same implementation. |
| `S1940` | SonarAnalyzer.CSharp S1940 | Style | yes | Boolean checks should not be inverted. |
| `S2094` | SonarAnalyzer.CSharp S2094 | Maintainability | report-only | Classes should not be empty. |
| `S2123` | SonarAnalyzer.CSharp S2123 | Correctness | report-only | Value is uselessly incremented. |
| `S2156` | SonarAnalyzer.CSharp S2156 | Maintainability | report-only | Sealed classes should not have protected members. |
| `S2178` | SonarAnalyzer.CSharp S2178 | Correctness | report-only | Short-circuit logic should be used in boolean contexts. |
| `S2190` | SonarAnalyzer.CSharp S2190 | Correctness | report-only | Recursion should not be infinite. |
| `S2201` | SonarAnalyzer.CSharp S2201 | Correctness | report-only | Return values from side-effect-free functions should not be ignored. |
| `S2221` | SonarAnalyzer.CSharp S2221 | Correctness | report-only | Exception should not be caught when not required. |
| `S2223` | SonarAnalyzer.CSharp S2223 | Maintainability | report-only | Non-constant static fields should not be visible. |
| `S2275` | SonarAnalyzer.CSharp S2275 | Correctness | report-only | Composite format strings should not exceed the argument count. |
| `S2292` | SonarAnalyzer.CSharp S2292 | Redundancy | report-only | Trivial properties should be auto-implemented. |
| `S2326` | SonarAnalyzer.CSharp S2326 | Redundancy | report-only | Unused type parameters should be removed. |
| `S2333` | SonarAnalyzer.CSharp S2333 | Redundancy | report-only | Redundant 'partial' modifier should be removed. |
| `S2342` | SonarAnalyzer.CSharp S2342 | Maintainability | report-only | Enum name violates the naming convention. |
| `S2344` | SonarAnalyzer.CSharp S2344 | Maintainability | report-only | Enumeration type names should not have 'Flags' or 'Enum' suffixes. |
| `S2360` | SonarAnalyzer.CSharp S2360 | Maintainability | report-only | Optional parameters should not be used. |
| `S2368` | SonarAnalyzer.CSharp S2368 | Maintainability | report-only | Public methods should not have multidimensional array parameters. |
| `S2372` | SonarAnalyzer.CSharp S2372 | Maintainability | report-only | Exceptions should not be thrown from property getters. |
| `S2376` | SonarAnalyzer.CSharp S2376 | Maintainability | report-only | Write-only properties should not be used. |
| `S2386` | SonarAnalyzer.CSharp S2386 | Maintainability | report-only | Mutable field should not be 'public static'. |
| `S2436` | SonarAnalyzer.CSharp S2436 | Maintainability | report-only | Types and methods should not have too many generic parameters. |
| `S2486` | SonarAnalyzer.CSharp S2486 | Correctness | report-only | Generic exceptions should not be ignored. |
| `S2551` | SonarAnalyzer.CSharp S2551 | Concurrency | report-only | Shared resources should not be used for locking. |
| `S2692` | SonarAnalyzer.CSharp S2692 | Correctness | report-only | 'IndexOf' checks should not be for positive numbers. |
| `S2737` | SonarAnalyzer.CSharp S2737 | Redundancy | report-only | 'catch' clauses should do more than rethrow. |
| `S2743` | SonarAnalyzer.CSharp S2743 | Maintainability | report-only | Static fields should not be used in generic types. |
| `S2761` | SonarAnalyzer.CSharp S2761 | Redundancy | report-only | Doubled prefix operators `!` and `~` should not be used. |
| `S2933` | SonarAnalyzer.CSharp S2933 | Redundancy | report-only | Fields only assigned in the constructor should be readonly. |
| `S2971` | SonarAnalyzer.CSharp S2971 | Maintainability | report-only | IEnumerable LINQs should be simplified. |
| `S3010` | SonarAnalyzer.CSharp S3010 | Correctness | report-only | Static fields should not be updated in constructors. |
| `S3052` | SonarAnalyzer.CSharp S3052 | Redundancy | report-only | Members should not be initialized to default values. |
| `S3168` | SonarAnalyzer.CSharp S3168 | Concurrency | report-only | 'async' methods should not return 'void'. |
| `S3237` | SonarAnalyzer.CSharp S3237 | Correctness | report-only | Use the 'value' contextual keyword in a set accessor. |
| `S3240` | SonarAnalyzer.CSharp S3240 | Style | report-only | Use the simplest possible condition syntax (`??` / `?:`). |
| `S3247` | SonarAnalyzer.CSharp S3247 | Maintainability | report-only | Use the result of the 'is' check instead of casting again. |
| `S3253` | SonarAnalyzer.CSharp S3253 | Redundancy | report-only | Constructor and destructor declarations should not be redundant. |
| `S3257` | SonarAnalyzer.CSharp S3257 | Redundancy | report-only | Array type is redundant when an initializer is present. |
| `S3260` | SonarAnalyzer.CSharp S3260 | Maintainability | report-only | Non-derived private classes should be sealed. |
| `S3261` | SonarAnalyzer.CSharp S3261 | Redundancy | report-only | Namespaces should not be empty. |
| `S3264` | SonarAnalyzer.CSharp S3264 | Redundancy | report-only | Events should be invoked. |
| `S3267` | SonarAnalyzer.CSharp S3267 | Style | report-only | Loops should be simplified with LINQ expressions. |
| `S3358` | SonarAnalyzer.CSharp S3358 | Maintainability | report-only | Ternary operators should not be nested. |
| `S3376` | SonarAnalyzer.CSharp S3376 | Maintainability | report-only | Attribute, EventArgs, and Exception type names should end with the type being extended. |
| `S3400` | SonarAnalyzer.CSharp S3400 | Maintainability | report-only | Methods should not return constants. |
| `S3442` | SonarAnalyzer.CSharp S3442 | Maintainability | report-only | Abstract classes should not have public constructors. |
| `S3445` | SonarAnalyzer.CSharp S3445 | Correctness | report-only | Exceptions should not be explicitly rethrown. |
| `S3459` | SonarAnalyzer.CSharp S3459 | Correctness | report-only | Unassigned fields should be removed. |
| `S3626` | SonarAnalyzer.CSharp S3626 | Redundancy | report-only | Jump statements should not be redundant. |
| `S3776` | SonarAnalyzer.CSharp S3776 | Maintainability | report-only | Cognitive Complexity of a method should not be too high. |
| `S3871` | SonarAnalyzer.CSharp S3871 | Maintainability | report-only | Exception types should be public. |
| `S3875` | SonarAnalyzer.CSharp S3875 | Correctness | report-only | operator== should not be overloaded on reference types. |
| `S3878` | SonarAnalyzer.CSharp S3878 | Redundancy | report-only | Arrays should not be created for params parameters. |
| `S3897` | SonarAnalyzer.CSharp S3897 | Maintainability | report-only | Classes that provide Equals(T) should implement IEquatable<T>. |
| `S3903` | SonarAnalyzer.CSharp S3903 | Maintainability | report-only | Types should be defined in named namespaces. |
| `S3923` | SonarAnalyzer.CSharp S3923 | Correctness | report-only | All branches in a conditional structure should not have the same implementation. |
| `S3928` | SonarAnalyzer.CSharp S3928 | Correctness | report-only | Parameter names used in ArgumentException constructors should match an existing one. |
| `S3962` | SonarAnalyzer.CSharp S3962 | Maintainability | report-only | 'static readonly' constant should be 'const'. |
| `S3963` | SonarAnalyzer.CSharp S3963 | Maintainability | report-only | Static fields should be initialized inline. |
| `S3981` | SonarAnalyzer.CSharp S3981 | Correctness | report-only | Collection size and array length comparisons should make sense. |
| `S3984` | SonarAnalyzer.CSharp S3984 | Correctness | report-only | Exceptions should not be created without being thrown. |
| `S3993` | SonarAnalyzer.CSharp S3993 | Maintainability | report-only | Custom attributes should be marked with AttributeUsageAttribute. |
| `S4022` | SonarAnalyzer.CSharp S4022 | Maintainability | report-only | Enum with a narrower-than-Int32 storage type. |
| `S4023` | SonarAnalyzer.CSharp S4023 | Maintainability | report-only | Interfaces should not be empty. |
| `S4035` | SonarAnalyzer.CSharp S4035 | Correctness | report-only | Classes implementing IEquatable<T> should be sealed. |
| `S4039` | SonarAnalyzer.CSharp S4039 | Maintainability | report-only | Interface methods should be callable by derived types. |
| `S4050` | SonarAnalyzer.CSharp S4050 | Correctness | report-only | Operators should be overloaded consistently. |
| `S4136` | SonarAnalyzer.CSharp S4136 | Maintainability | report-only | Method overloads should be grouped together. |
| `S4144` | SonarAnalyzer.CSharp S4144 | Correctness | report-only | Methods should not have identical implementations. |
| `S4487` | SonarAnalyzer.CSharp S4487 | Redundancy | report-only | Unread private fields should be removed. |
| `S4524` | SonarAnalyzer.CSharp S4524 | Maintainability | report-only | default clauses should be first or last. |
| `S4663` | SonarAnalyzer.CSharp S4663 | Redundancy | report-only | Comments should not be empty. |
| `S6354` | SonarAnalyzer.CSharp S6354 | Maintainability | report-only | Ambient DateTime.Now/UtcNow/Today access. |
| `S818` | SonarAnalyzer.CSharp S818 | Maintainability | report-only | Literal suffixes should be upper case. |
| `S907` | SonarAnalyzer.CSharp S907 | Maintainability | report-only | goto statement should not be used. |
| `SA1000` | StyleCop.Analyzers SA1000 | Style | yes | Keywords should be spaced correctly. |
| `SA1001` | StyleCop.Analyzers SA1001 | Style | yes | Commas should be spaced correctly. |
| `SA1002` | StyleCop.Analyzers SA1002 | Style | yes | Semicolons should be spaced correctly. |
| `SA1004` | StyleCop.Analyzers SA1004 | Style | report-only | Documentation line should begin with a space. |
| `SA1005` | StyleCop.Analyzers SA1005 | Style | yes | Single line comments should begin with a space. |
| `SA1008` | StyleCop.Analyzers SA1008 | Style | yes | Opening parenthesis should not be preceded by a space. |
| `SA1009` | StyleCop.Analyzers SA1009 | Style | yes | Closing parenthesis should not be preceded by a space. |
| `SA1010` | StyleCop.Analyzers SA1010 | Style | yes | Opening square brackets should be spaced correctly. |
| `SA1011` | StyleCop.Analyzers SA1011 | Style | yes | Closing square brackets should be spaced correctly. |
| `SA1012` | StyleCop.Analyzers SA1012 | Style | yes | Opening braces should be spaced correctly. |
| `SA1013` | StyleCop.Analyzers SA1013 | Style | yes | Closing braces should be spaced correctly. |
| `SA1014` | StyleCop.Analyzers SA1014 | Style | yes | Opening generic brackets should not be preceded by a space. |
| `SA1015` | StyleCop.Analyzers SA1015 | Style | yes | Closing generic brackets should be spaced correctly. |
| `SA1016` | StyleCop.Analyzers SA1016 | Style | yes | Opening attribute brackets should not be followed by a space. |
| `SA1017` | StyleCop.Analyzers SA1017 | Style | yes | Closing attribute brackets should not be preceded by a space. |
| `SA1018` | StyleCop.Analyzers SA1018 | Style | yes | Nullable type symbol should not be preceded by a space. |
| `SA1019` | StyleCop.Analyzers SA1019 | Style | yes | Member access symbols should be spaced correctly. |
| `SA1020` | StyleCop.Analyzers SA1020 | Style | yes | Increment/decrement symbols should be spaced correctly. |
| `SA1021` | StyleCop.Analyzers SA1021 | Style | yes | Negative signs should be spaced correctly. |
| `SA1022` | StyleCop.Analyzers SA1022 | Style | yes | Positive signs should be spaced correctly. |
| `SA1024` | StyleCop.Analyzers SA1024 | Style | yes | Colons should be spaced correctly. |
| `SA1025` | StyleCop.Analyzers SA1025 | Style | report-only | Code should not contain multiple whitespace characters in a row. |
| `SA1026` | StyleCop.Analyzers SA1026 | Style | yes | The keyword 'new' should not be followed by a space. |
| `SA1027` | StyleCop.Analyzers SA1027 | Style | report-only | Tabs and spaces should be used correctly (no tabs by default). |
| `SA1028` | StyleCop.Analyzers SA1028 | Style | report-only | Code should not contain trailing whitespace. |
| `SA1100` | StyleCop.Analyzers SA1100 | Maintainability | report-only | Do not prefix calls with base unless a local override exists. |
| `SA1106` | StyleCop.Analyzers SA1106 | Redundancy | yes | Code should not contain empty statements. |
| `SA1107` | StyleCop.Analyzers SA1107 | Style | report-only | Code should not contain multiple statements on one line. |
| `SA1108` | StyleCop.Analyzers SA1108 | Style | report-only | Block statements should not contain embedded comments. |
| `SA1110` | StyleCop.Analyzers SA1110 | Style | report-only | Opening parenthesis should be on the declaration line. |
| `SA1111` | StyleCop.Analyzers SA1111 | Style | report-only | Closing parenthesis should be on the line of the last parameter. |
| `SA1112` | StyleCop.Analyzers SA1112 | Style | report-only | Closing parenthesis should be on the line of the opening parenthesis. |
| `SA1113` | StyleCop.Analyzers SA1113 | Style | report-only | Comma should be on the same line as previous parameter. |
| `SA1114` | StyleCop.Analyzers SA1114 | Style | report-only | Parameter list should follow declaration. |
| `SA1115` | StyleCop.Analyzers SA1115 | Style | report-only | Parameters should not be separated by blank lines. |
| `SA1116` | StyleCop.Analyzers SA1116 | Style | report-only | Split parameters should begin on the line after the declaration. |
| `SA1117` | StyleCop.Analyzers SA1117 | Style | report-only | Parameters should all be on the same line or each on its own line. |
| `SA1119` | StyleCop.Analyzers SA1119 | Redundancy | yes | Statement should not use unnecessary parenthesis. |
| `SA1120` | StyleCop.Analyzers SA1120 | Redundancy | report-only | Comments should contain text. |
| `SA1121` | StyleCop.Analyzers SA1121 | Maintainability | yes | Use built-in type alias. |
| `SA1122` | StyleCop.Analyzers SA1122 | Style | yes | Use string.Empty for empty strings. |
| `SA1123` | StyleCop.Analyzers SA1123 | Maintainability | report-only | Do not place regions within elements. |
| `SA1124` | StyleCop.Analyzers SA1124 | Maintainability | report-only | Do not use regions. |
| `SA1125` | StyleCop.Analyzers SA1125 | Style | yes | Use shorthand for nullable types. |
| `SA1127` | StyleCop.Analyzers SA1127 | Style | report-only | Generic type constraints should be on their own line. |
| `SA1128` | StyleCop.Analyzers SA1128 | Style | report-only | Put constructor initializers on their own line. |
| `SA1129` | StyleCop.Analyzers SA1129 | Maintainability | report-only | Do not use the default value-type constructor. |
| `SA1130` | StyleCop.Analyzers SA1130 | Style | yes | Use lambda syntax. |
| `SA1131` | StyleCop.Analyzers SA1131 | Style | yes | Constant values should be on the right-hand side of comparisons. |
| `SA1132` | StyleCop.Analyzers SA1132 | Maintainability | report-only | Do not combine fields. |
| `SA1133` | StyleCop.Analyzers SA1133 | Maintainability | report-only | Do not combine attributes in one set of brackets. |
| `SA1134` | StyleCop.Analyzers SA1134 | Style | report-only | Attributes should not share line. |
| `SA1136` | StyleCop.Analyzers SA1136 | Style | report-only | Enum values should be on separate lines. |
| `SA1137` | StyleCop.Analyzers SA1137 | Style | report-only | Elements should have the same indentation. |
| `SA1139` | StyleCop.Analyzers SA1139 | Style | yes | Use literal suffix notation instead of casting. |
| `SA1200` | StyleCop.Analyzers SA1200 | Style | report-only | Using directives should be placed within a namespace. |
| `SA1201` | StyleCop.Analyzers SA1201 | Maintainability | report-only | Elements should appear in the correct order. |
| `SA1202` | StyleCop.Analyzers SA1202 | Maintainability | report-only | Elements should be ordered by access. |
| `SA1203` | StyleCop.Analyzers SA1203 | Maintainability | report-only | Constants should appear before fields. |
| `SA1204` | StyleCop.Analyzers SA1204 | Maintainability | report-only | Static members should appear before non-static members. |
| `SA1205` | StyleCop.Analyzers SA1205 | Maintainability | report-only | Partial elements should declare an access modifier. |
| `SA1206` | StyleCop.Analyzers SA1206 | Style | yes | Declaration keywords should follow a standard ordering. |
| `SA1207` | StyleCop.Analyzers SA1207 | Style | report-only | The keyword 'protected' should come before 'internal'. |
| `SA1208` | StyleCop.Analyzers SA1208 | Style | report-only | System using directives should be placed before other usings. |
| `SA1209` | StyleCop.Analyzers SA1209 | Style | report-only | Using alias directives should be placed after all using namespace directives. |
| `SA1210` | StyleCop.Analyzers SA1210 | Style | report-only | Using directives should be ordered alphabetically by the namespaces. |
| `SA1211` | StyleCop.Analyzers SA1211 | Style | report-only | Using alias directives should be ordered alphabetically by alias name. |
| `SA1212` | StyleCop.Analyzers SA1212 | Style | report-only | A get accessor should appear before a set accessor. |
| `SA1213` | StyleCop.Analyzers SA1213 | Style | report-only | Event accessors should follow order (add before remove). |
| `SA1214` | StyleCop.Analyzers SA1214 | Maintainability | report-only | Readonly fields should appear before non-readonly fields. |
| `SA1216` | StyleCop.Analyzers SA1216 | Style | report-only | Using static directives should be placed at the correct location. |
| `SA1217` | StyleCop.Analyzers SA1217 | Style | report-only | Using static directives should be ordered alphabetically by type name. |
| `SA1300` | StyleCop.Analyzers SA1300 | Maintainability | report-only | Element should begin with an upper-case letter. |
| `SA1302` | StyleCop.Analyzers SA1302 | Maintainability | report-only | Interface names should begin with I. |
| `SA1303` | StyleCop.Analyzers SA1303 | Maintainability | report-only | Const field names should begin with an upper-case letter. |
| `SA1306` | StyleCop.Analyzers SA1306 | Maintainability | report-only | Field names should begin with a lower-case letter. |
| `SA1307` | StyleCop.Analyzers SA1307 | Maintainability | report-only | Accessible fields should begin with an upper-case letter. |
| `SA1308` | StyleCop.Analyzers SA1308 | Maintainability | report-only | Field names should not be prefixed with `m_` or `s_`. |
| `SA1309` | StyleCop.Analyzers SA1309 | Maintainability | report-only | Field names should not begin with an underscore. |
| `SA1310` | StyleCop.Analyzers SA1310 | Maintainability | report-only | Field names should not contain an underscore. |
| `SA1311` | StyleCop.Analyzers SA1311 | Maintainability | report-only | Static readonly fields should begin with an upper-case letter. |
| `SA1312` | StyleCop.Analyzers SA1312 | Maintainability | report-only | Variable names should begin with a lower-case letter. |
| `SA1313` | StyleCop.Analyzers SA1313 | Maintainability | report-only | Parameter names should begin with a lower-case letter. |
| `SA1314` | StyleCop.Analyzers SA1314 | Style | report-only | Type parameter names should begin with T. |
| `SA1400` | StyleCop.Analyzers SA1400 | Maintainability | report-only | Access modifier should be declared. |
| `SA1401` | StyleCop.Analyzers SA1401 | Maintainability | report-only | Fields should be private. |
| `SA1402` | StyleCop.Analyzers SA1402 | Maintainability | report-only | File may only contain a single type. |
| `SA1403` | StyleCop.Analyzers SA1403 | Maintainability | report-only | File may only contain a single namespace. |
| `SA1404` | StyleCop.Analyzers SA1404 | Maintainability | report-only | Code analysis suppression should have justification. |
| `SA1405` | StyleCop.Analyzers SA1405 | Maintainability | report-only | Debug.Assert should provide message text. |
| `SA1406` | StyleCop.Analyzers SA1406 | Maintainability | report-only | Debug.Fail should provide message text. |
| `SA1407` | StyleCop.Analyzers SA1407 | Maintainability | yes | Arithmetic expressions should declare precedence. |
| `SA1408` | StyleCop.Analyzers SA1408 | Maintainability | report-only | Conditional expressions should declare precedence. |
| `SA1410` | StyleCop.Analyzers SA1410 | Style | yes | Remove delegate parenthesis when possible. |
| `SA1411` | StyleCop.Analyzers SA1411 | Redundancy | yes | Attribute constructor should not use unnecessary parentheses. |
| `SA1413` | StyleCop.Analyzers SA1413 | Style | yes | Use trailing comma in multi-line initializers. |
| `SA1500` | StyleCop.Analyzers SA1500 | Style | yes | Braces for multi-line statements should not share a line. |
| `SA1501` | StyleCop.Analyzers SA1501 | Style | yes | Statement should not be on a single line. |
| `SA1502` | StyleCop.Analyzers SA1502 | Style | yes | Element should not be on a single line. |
| `SA1503` | StyleCop.Analyzers SA1503 | Maintainability | report-only | Braces should not be omitted. |
| `SA1505` | StyleCop.Analyzers SA1505 | Style | yes | An opening brace should not be followed by a blank line. |
| `SA1506` | StyleCop.Analyzers SA1506 | Style | report-only | Element documentation headers should not be followed by blank line. |
| `SA1507` | StyleCop.Analyzers SA1507 | Style | yes | Code should not contain multiple blank lines in a row. |
| `SA1508` | StyleCop.Analyzers SA1508 | Style | yes | A closing brace should not be preceded by a blank line. |
| `SA1509` | StyleCop.Analyzers SA1509 | Style | report-only | Opening braces should not be preceded by blank line. |
| `SA1510` | StyleCop.Analyzers SA1510 | Style | report-only | Chained statement blocks should not be preceded by blank line. |
| `SA1511` | StyleCop.Analyzers SA1511 | Style | report-only | While-do footer should not be preceded by blank line. |
| `SA1512` | StyleCop.Analyzers SA1512 | Style | report-only | Single-line comments should not be followed by a blank line. |
| `SA1513` | StyleCop.Analyzers SA1513 | Style | yes | Closing brace should be followed by blank line. |
| `SA1514` | StyleCop.Analyzers SA1514 | Style | yes | Element documentation header should be preceded by a blank line. |
| `SA1515` | StyleCop.Analyzers SA1515 | Style | report-only | Single-line comments should be preceded by a blank line. |
| `SA1516` | StyleCop.Analyzers SA1516 | Style | yes | Elements should be separated by a blank line. |
| `SA1517` | StyleCop.Analyzers SA1517 | Style | yes | Code should not contain blank lines at start of file. |
| `SA1518` | StyleCop.Analyzers SA1518 | Style | yes | Code should not contain blank lines at the end of the file. |
| `SA1519` | StyleCop.Analyzers SA1519 | Maintainability | report-only | Braces should not be omitted from multi-line child statement. |
| `SA1520` | StyleCop.Analyzers SA1520 | Maintainability | report-only | Use braces consistently. |
| `SA1600` | StyleCop.Analyzers SA1600 | Maintainability | report-only | Elements should be documented. |
| `SA1601` | StyleCop.Analyzers SA1601 | Maintainability | report-only | Partial elements should be documented. |
| `SA1602` | StyleCop.Analyzers SA1602 | Maintainability | report-only | Enumeration items should be documented. |
| `SA1604` | StyleCop.Analyzers SA1604 | Maintainability | report-only | Element documentation should have summary. |
| `SA1606` | StyleCop.Analyzers SA1606 | Maintainability | report-only | Element documentation should have summary text. |
| `SA1608` | StyleCop.Analyzers SA1608 | Maintainability | report-only | Element documentation should not have default summary text. |
| `SA1609` | StyleCop.Analyzers SA1609 | Maintainability | report-only | Property documentation should have value. |
| `SA1610` | StyleCop.Analyzers SA1610 | Maintainability | report-only | Property documentation should have value text. |
| `SA1611` | StyleCop.Analyzers SA1611 | Maintainability | report-only | Element parameters should be documented. |
| `SA1612` | StyleCop.Analyzers SA1612 | Maintainability | report-only | Element parameter documentation should match element parameters. |
| `SA1613` | StyleCop.Analyzers SA1613 | Maintainability | report-only | Element parameter documentation should declare a parameter name. |
| `SA1614` | StyleCop.Analyzers SA1614 | Maintainability | report-only | Element parameter documentation should have text. |
| `SA1615` | StyleCop.Analyzers SA1615 | Maintainability | report-only | Element return value should be documented. |
| `SA1616` | StyleCop.Analyzers SA1616 | Maintainability | report-only | Element return value documentation should have text. |
| `SA1617` | StyleCop.Analyzers SA1617 | Maintainability | report-only | Void return value should not be documented. |
| `SA1618` | StyleCop.Analyzers SA1618 | Maintainability | report-only | Generic type parameters should be documented. |
| `SA1622` | StyleCop.Analyzers SA1622 | Maintainability | report-only | Generic type parameter documentation should have text. |
| `SA1623` | StyleCop.Analyzers SA1623 | Style | report-only | Property documentation summary should match its accessors. |
| `SA1624` | StyleCop.Analyzers SA1624 | Maintainability | report-only | Property summary should begin with 'Gets' when the setter is not visible. |
| `SA1625` | StyleCop.Analyzers SA1625 | Maintainability | report-only | Element documentation should not be copied and pasted. |
| `SA1626` | StyleCop.Analyzers SA1626 | Style | report-only | Single-line comments should not use documentation style slashes. |
| `SA1629` | StyleCop.Analyzers SA1629 | Style | report-only | Documentation text should end with a period. |
| `SA1642` | StyleCop.Analyzers SA1642 | Maintainability | report-only | Constructor summary documentation should begin with standard text. |
| `SA1643` | StyleCop.Analyzers SA1643 | Maintainability | report-only | Destructor summary documentation should begin with standard text. |
| `SA1651` | StyleCop.Analyzers SA1651 | Maintainability | report-only | Do not use placeholder elements. |
| `SCS0005` | SecurityCodeScan.VS2019 SCS0005 | Correctness | report-only | Weak random number generator. |
| `SCS0013` | SecurityCodeScan.VS2019 SCS0013 | Correctness | report-only | Potential usage of a weak CipherMode. |
| `SYSLIB1045` | dotnet/runtime SYSLIB1045 (System.Text.RegularExpressions source generator diagnostic) | Performance | report-only | Regex constructed from a literal pattern could use a source-generated GeneratedRegexAttribute. |
| `VSTHRD004` | Microsoft.VisualStudio.Threading.Analyzers VSTHRD004 | Concurrency | report-only | SwitchToMainThreadAsync() must be awaited. |
| `VSTHRD100` | Microsoft.VisualStudio.Threading.Analyzers VSTHRD100 | Concurrency | report-only | Avoid async void methods. |
| `VSTHRD112` | Microsoft.VisualStudio.Threading.Analyzers VSTHRD112 | Concurrency | report-only | Implement System.IAsyncDisposable alongside the obsolete vs-threading interface. |
| `VSTHRD113` | Microsoft.VisualStudio.Threading.Analyzers VSTHRD113 | Concurrency | report-only | Pair a check for the obsolete vs-threading IAsyncDisposable with a check for System.IAsyncDisposable. |
| `VSTHRD115` | Microsoft.VisualStudio.Threading.Analyzers VSTHRD115 | Concurrency | report-only | Avoid creating JoinableTaskContext with a null SynchronizationContext. |
| `VSTHRD200` | Microsoft.VisualStudio.Threading.Analyzers VSTHRD200 | Concurrency | report-only | Async naming convention not followed. |
| `xUnit1000` | xunit.analyzers xUnit1000 | Correctness | report-only | Test classes must be public. |
| `xUnit1001` | xunit.analyzers xUnit1001 | Correctness | report-only | Fact methods cannot have parameters. |
| `xUnit1002` | xunit.analyzers xUnit1002 | Correctness | report-only | Test methods cannot have multiple Fact or Theory attributes. |
| `xUnit1003` | xunit.analyzers xUnit1003 | Correctness | report-only | Theory methods must have test data. |
| `xUnit1004` | xunit.analyzers xUnit1004 | Correctness | report-only | Test methods should not be skipped. |
| `xUnit1006` | xunit.analyzers xUnit1006 | Correctness | report-only | Theory methods should have parameters. |
| `xUnit1008` | xunit.analyzers xUnit1008 | Correctness | report-only | Test data attribute should only be used on a Theory. |
| `xUnit1009` | xunit.analyzers xUnit1009 | Correctness | report-only | InlineData supplies fewer values than the method's parameters. |
| `xUnit1010` | xunit.analyzers xUnit1010 | Correctness | report-only | InlineData value is not convertible to the parameter type. |
| `xUnit1011` | xunit.analyzers xUnit1011 | Correctness | report-only | InlineData has a value with no matching method parameter. |
| `xUnit1012` | xunit.analyzers xUnit1012 | Correctness | report-only | Null should not be used for value-type parameters. |
| `xUnit1013` | xunit.analyzers xUnit1013 | Correctness | report-only | Public method should be marked as a test. |
| `xUnit1014` | xunit.analyzers xUnit1014 | Maintainability | yes | MemberData should use the nameof operator for the member name. |
| `xUnit1024` | xunit.analyzers xUnit1024 | Correctness | report-only | Test methods should not be overloaded. |
| `xUnit1025` | xunit.analyzers xUnit1025 | Correctness | report-only | InlineData should be unique within the Theory it belongs to. |
| `xUnit1026` | xunit.analyzers xUnit1026 | Correctness | report-only | Theory methods should use all of their parameters. |
| `xUnit1028` | xunit.analyzers xUnit1028 | Correctness | report-only | Test method must have a valid return type. |
| `xUnit1030` | xunit.analyzers xUnit1030 | Concurrency | report-only | Do not call ConfigureAwait(false) in a test method. |
| `xUnit1031` | xunit.analyzers xUnit1031 | Concurrency | report-only | Do not use blocking task operations in test method. |
| `xUnit1048` | xunit.analyzers xUnit1048 | Concurrency | report-only | Avoid async-void unit tests. |
| `xUnit2000` | xunit.analyzers xUnit2000 | Correctness | yes | Constants and literals should be the expected argument. |
| `xUnit2002` | xunit.analyzers xUnit2002 | Correctness | report-only | Do not use null check on value type. |
| `xUnit2003` | xunit.analyzers xUnit2003 | Correctness | yes | Do not use Assert.Equal() to check for null value. |
| `xUnit2004` | xunit.analyzers xUnit2004 | Maintainability | report-only | Do not use Assert.Equal to check a boolean condition. |
| `xUnit2005` | xunit.analyzers xUnit2005 | Correctness | report-only | Do not use identity check on value type. |
| `xUnit2006` | xunit.analyzers xUnit2006 | Maintainability | report-only | Do not use a generic Assert.Equal to test string equality. |
| `xUnit2007` | xunit.analyzers xUnit2007 | Maintainability | report-only | Do not use typeof expression to check the type. |
| `xUnit2008` | xunit.analyzers xUnit2008 | Maintainability | report-only | Do not use boolean check to match on regular expressions. |
| `xUnit2009` | xunit.analyzers xUnit2009 | Maintainability | report-only | Do not use boolean check to check for substrings. |
| `xUnit2010` | xunit.analyzers xUnit2010 | Maintainability | report-only | Do not use boolean check to check for string equality. |
| `xUnit2013` | xunit.analyzers xUnit2013 | Correctness | yes | Do not use Assert.Equal() to check for collection size. |
| `xUnit2014` | xunit.analyzers xUnit2014 | Correctness | report-only | Do not use a throws check for an asynchronously thrown exception. |
| `xUnit2015` | xunit.analyzers xUnit2015 | Maintainability | report-only | Do not use typeof expression to check the exception type. |
| `xUnit2021` | xunit.analyzers xUnit2021 | Correctness | report-only | Async assertions should be awaited. |
| `xUnit2022` | xunit.analyzers xUnit2022 | Maintainability | report-only | Boolean assertions should not be negated. |
| `xUnit2024` | xunit.analyzers xUnit2024 | Correctness | report-only | Do not use boolean asserts for simple equality tests. |

## Details

### `AV1008` — Avoid static classes (unless suffixed 'Extensions').

*Port of CSharpGuidelinesAnalyzer AV1008 · Maintainability · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/1008_AvoidStaticClass.md)

A static class is discouraged unless its name ends in 'Extensions' (marking it a container of extension methods) or it is a platform-invoke wrapper (`NativeMethods`/`SafeNativeMethods`/`UnsafeNativeMethods`) or holds the program entry point. A non-`Extensions` static class fires at the class name; an `Extensions` static class instead fires per public/internal member that is not an extension method (first parameter `this`). Native port of CSharpGuidelinesAnalyzer AV1008 — report-only.

### `AV1010` — Don't hide inherited members with the `new` keyword.

*Port of CSharpGuidelinesAnalyzer AV1010 · Maintainability · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/1010_DoNotHideInheritedMember.md)

A member declared with the `new` modifier hides an inherited member, which suppresses a compiler warning rather than resolving the name clash. The analyzer fires on any non-`override` member that carries `new` (property/event accessors and compiler-synthesized members excepted); it does not verify the base actually declares the hidden member — the `new` keyword itself is the smell. Native port of CSharpGuidelinesAnalyzer AV1010 — report-only.

### `AV1115` — A method should do only one thing (its name should not contain the word 'and').

*Port of CSharpGuidelinesAnalyzer AV1115 · Maintainability · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/1115_MemberShouldDoASingleThing.md)

A member whose name contains the word 'and' in the middle (e.g. `SaveAndClose`) suggests it does more than one thing and should be split. The name is segmented into words (camel/Pascal/upper-case) and fires only when an 'And' word occurs strictly between the first and last word, so a single word like `Android` or an edge 'and' does not fire. Anchored at the method name; `override`/`new`/explicit-interface members are exempt. Native port of CSharpGuidelinesAnalyzer AV1115 — report-only.

### `AV1225` — Use a protected virtual method to raise each event.

*Port of CSharpGuidelinesAnalyzer AV1225 · Maintainability · report-only* · [upstream docs](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.7.0/_rules/1225.md)

Each event should be raised from a dedicated method named `On` followed by the event name, declared `protected virtual` (so derived types can extend or suppress it). Fires when an event is raised — `E?.Invoke(…)`, `E.Invoke(…)` or `E(…)` — from something other than such a method: a non-regular member (accessor, constructor, operator, local function or lambda) reports that the event should be raised from a regular method; a regular method with the wrong name reports the expected `On`-name; a correctly-named method that is not `protected virtual` reports the missing modifiers (a `sealed` type, a `static` method and explicit-interface implementations are exempt). Only field-like events are raisable by name. Native port of CSharpGuidelinesAnalyzer AV1225 — report-only.

### `AV1500` — Methods should not exceed 7 statements.

*Port of CSharpGuidelinesAnalyzer AV1500 · Maintainability · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/1500_AvoidMethodsWithManyStatements.md)

A method with more than 7 statements is doing too much and should be split. Counts every statement in the body recursively (excluding blocks, labeled statements, and the local-function-statement node itself), anchored at the method name when the count exceeds 7. Native port of CSharpGuidelinesAnalyzer AV1500 — report-only.

### `AV1535` — Always add a block after a case or default clause.

*Port of CSharpGuidelinesAnalyzer AV1535 · Maintainability · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/1535_AddBlockAfterKeyword.md)

A `case` or `default` clause of a `switch` should wrap its statements in a `{ }` block for clarity and to scope any locals. Fires when a non-empty section's first statement is not a block, anchored at the keyword of the section's last label. Native port of CSharpGuidelinesAnalyzer AV1535 — report-only.

### `AV1537` — Finish every if-else-if statement with an else clause.

*Port of CSharpGuidelinesAnalyzer AV1537 · Maintainability · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/1537_FinishIfElseIfWithElse.md)

An `if`-`else`-`if` chain should end with an unconditional `else` clause so every case is handled explicitly. Fires once, anchored at the leading `if`, when the chain has at least one `else if` but no final `else`. Native port of CSharpGuidelinesAnalyzer AV1537 — report-only.

### `AV1561` — Don't declare signatures with more than three parameters.

*Port of CSharpGuidelinesAnalyzer AV1561 · Maintainability · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/1561_AvoidSignatureWithManyParameters.md)

A signature with more than three parameters (the analyzer default) is hard to call and usually wants a parameter object; a tuple-typed parameter is discouraged for the same reason. Fires the count diagnostic at the method name when it has four or more parameters, and a tuple-parameter diagnostic at each parameter whose type is a value tuple. A member that is `extern`/`override`/`new`/explicitly implements an interface is exempt. Native port of CSharpGuidelinesAnalyzer AV1561 (the `>3` twin of S107) — report-only.

### `AV1562` — Don't use ref or out parameters.

*Port of CSharpGuidelinesAnalyzer AV1562 · Maintainability · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/1562_DoNotUseRefOrOutParameters.md)

A `ref` or `out` parameter makes a method harder to understand and forces callers into a reference-passing pattern. Fires per `ref`/`out` parameter, anchored at the parameter name; a `ref readonly` parameter and an `out` parameter of a `Try…` method are exempt. Native port of CSharpGuidelinesAnalyzer AV1562 (the `out`-aware twin of CA1045) — report-only.

### `AV1564` — Avoid signatures that take a bool parameter.

*Port of CSharpGuidelinesAnalyzer AV1564 · Maintainability · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/Maintainability.md#av1564)

A `bool`/`bool?` parameter forces callers to read the declaration to understand what `true`/ `false` means at a call site; prefer an enum or split the method. Fires on a non-private, root-accessible method's bool parameter unless the method is an override, hides a base member (`new`), is a deconstructor, implicitly implements an interface member (resolved via the BCL type-fact table, issue #144, for tabled interfaces), or is the `protected virtual void Dispose(bool disposing)` pattern. Native port of CSharpGuidelinesAnalyzer AV1564; report-only.

### `AV1704` — Don't include digits in type, member, parameter or variable names.

*Port of CSharpGuidelinesAnalyzer AV1704 · Maintainability · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/1704_DoNotUseNumbersInIdentifiers.md)

An identifier that contains a digit usually signals a poor name (`user2`, `field1`). The name is tokenized into words (camel/Pascal/upper-case, with digit and underscore runs as separators) and a short whitelist of digit-bearing pairs is removed first — `Int16/32/64`, `Win16/32/64`, `Utf7/8/16/32` and `2D`/`3D`/`4D` — so `Utf8` or `Point3D` are allowed; it fires only if a digit remains. Covers types, methods, properties, fields, events, local functions and local variables, anchored at the identifier; `override` and explicit-interface members are exempt. Native port of CSharpGuidelinesAnalyzer AV1704 — report-only.

### `AV1706` — Don't use abbreviations or a single letter in identifier names.

*Port of CSharpGuidelinesAnalyzer AV1706 · Maintainability · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/1706_DoNotUseAbbreviationsInIdentifiers.md)

An identifier that is a single letter (`x`, `A`) or contains a known abbreviation as a whole word reads poorly. The name is tokenized into words (camel/Pascal/upper-case, separators dropped) and fires if it is one letter or any word matches, case-insensitively, a fixed abbreviation blacklist (`Btn`, `Ctrl`, … `Ex`, `Len`, `Idx`, `Str`, `Doc`). So `ex`/`len`/`WidgetEx` fire but `Example` does not, and `_` (not a letter) never fires. Covers types, methods, properties, fields, events, local functions, parameters (incl. lambda parameters), local variables, tuple element names and LINQ range variables, anchored at the identifier; `override` and explicit-interface members and parameters are exempt. Native port of CSharpGuidelinesAnalyzer AV1706 — report-only.

### `AV1708` — Name types using nouns (avoid Helper/Utility/Common/Shared terms).

*Port of CSharpGuidelinesAnalyzer AV1708 · Maintainability · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/1708_DoNotDeclareHelpingMethod.md)

A type name should not contain a vague term that hints at a grab-bag of behaviour. The name is segmented into words (camel/Pascal/upper-case, underscores and digits are separators) and fires when a whole word matches, case-insensitively, one of `Utility`, `Utilities`, `Facility`, `Facilities`, `Helper`, `Helpers`, `Common` or `Shared`. So `Scout56_Helper` fires (on `Helper`) but `Sharedstate` (a single word) does not. Anchored at the type name with the first matched term. Native port of CSharpGuidelinesAnalyzer AV1708 — report-only.

### `AV1710` — Don't repeat the name of a class or enumeration in its members.

*Port of CSharpGuidelinesAnalyzer AV1710 · Style · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/1710_DoNotRepeatTypeNameInMemberName.md)

A member whose name contains its containing type's name is redundant (`Color.ColorRed` should be `Color.Red`). Fires when a field or enum-member name contains the enclosing type name, anchored at the member name (the `User.username` pair is exempt). Native port of CSharpGuidelinesAnalyzer AV1710 — report-only.

### `AV1739` — Use an underscore for irrelevant lambda parameters.

*Port of CSharpGuidelinesAnalyzer AV1739 · Maintainability · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/1739_UseUnderscoreForUnusedLambdaParameter.md)

A lambda or anonymous-method parameter that is never used in the body should be renamed to an underscore (`_`) to signal it is irrelevant. Fires per unused parameter, anchored at its name; a name that is already all underscores is exempt. The thin twin of Roslynator RCS1163's lambda case (same sites, different message and rename-to-`_` intent). Native port of CSharpGuidelinesAnalyzer AV1739 — report-only.

### `AV2407` — Do not use #region.

*Port of CSharpGuidelinesAnalyzer AV2407 · Maintainability · report-only* · [upstream docs](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/2407_DoNotUseRegions.md)

A `#region` hides code and discourages keeping types small and readable. Fires at every `#region` directive, anchored at the `#` (unlike StyleCop's SA1124, no `#region` is exempt). Native port of CSharpGuidelinesAnalyzer AV2407 — report-only (removing a region deletes the non-adjacent `#region` and `#endregion`, which a single-range fix cannot express).

### `AsyncFixer02` — Thread.Sleep blocks the thread inside an async method.

*Port of AsyncFixer AsyncFixer02 · Concurrency · report-only* · [upstream docs](https://github.com/semihokur/AsyncFixer)

`Thread.Sleep` blocks the thread, defeating the point of `async`; inside an `async` method use `await Task.Delay(...)` instead. Fires on a BARE `Thread.Sleep(...)` call (not the qualified `System.Threading.Thread.Sleep` — confirmed narrower than MA0042 here) whose nearest enclosing function is `async`. Native port of AsyncFixer AsyncFixer02 — purely syntactic, report-only. `.Wait()`/`.Result`/`Task.WaitAll`/`Task.WaitAny` are a documented semantic miss (need Task<T> receiver resolution).

### `AsyncFixer03` — Avoid fire-and-forget async-void methods or delegates.

*Port of AsyncFixer AsyncFixer03 · Concurrency · report-only* · [upstream docs](https://github.com/semihokur/AsyncFixer)

An `async void` method is fire-and-forget: it cannot be awaited and an unhandled exception crashes the process. Fires on an `async void` method (twin of VSTHRD100), anchored at the `void` return type; the `(object, EventArgs)` event-handler shape is exempt. Native port of AsyncFixer AsyncFixer03 (report-only — changing the return type ripples into callers).

### `CA1000` — Do not declare static members on generic types.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1000 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1000)

A static member of a generic type must be referenced with the full type arguments, which is awkward and surprising. The Meziantou twin of MA0018 — a `static` method or property whose declaring type is generic, anchored at the member name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1000 — report-only.

### `CA1002` — Do not expose generic lists.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1002 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1002)

`List<T>` is a concrete implementation; a public API should expose `Collection<T>` / `IReadOnlyList<T>` instead so it can evolve. Fires on a visible property or method-return typed `List<T>`, anchored at the member name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1002 — report-only.

### `CA1003` — Use generic event handler instances.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1003 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1003)

A visible event's delegate should match the conventional `(object, TEventArgs) -> void` handler shape (`EventHandler`/`EventHandler<TEventArgs>`, or any other delegate whose resolved invoke signature matches it) — checked by SIGNATURE SHAPE, not by the delegate's name (issue #166 batch 12); a GENERIC delegate reference is substituted single-level before the shape check (issue #166 batch 13). Fires on a visible event (field-like or custom) whose delegate does not match that shape, anchored at the event name; exempt when the event overrides a base member, implements an interface member (project-local, direct-base-list only), or the containing type carries `[ComSourceInterfaces]`. Also fires on a visible `delegate` DECLARATION whose own invoke signature already matches the conventional shape — regardless of the delegate's own name — recommending the generic `EventHandler<T>` instead, anchored at the delegate's name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1003 — report-only.

### `CA1005` — Avoid excessive parameters on generic types.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1005 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1005)

A generic type with more than two type parameters is hard to use — callers struggle to remember what each stands for. Fires on a type declaring more than two type parameters, anchored at the type name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1005 — report-only.

### `CA1008` — Enums should have zero value.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1008 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1008)

A non-`[Flags]` enum should define a zero-valued member, since the runtime default of any enum is `0` and that state should be nameable. Fires on a non-`[Flags]` enum with no zero-valued member, anchored at the enum name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1008 — report-only.

### `CA1010` — Collections should implement generic interface.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1010 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1010)

A type that implements the non-generic `IEnumerable` should also implement `IEnumerable<T>` so consumers get strongly-typed enumeration. Fires on a type whose base list names `IEnumerable` (non-generic) but no `IEnumerable<T>`, anchored at the type name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1010 — report-only.

### `CA1012` — Abstract types should not have public constructors.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1012 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1012)

A constructor on an `abstract` type can only be called by derived types, so it should be `protected`, not `public`/`internal`. Fires once on the abstract class that declares such a constructor, anchored at the class name. The per-constructor twins (MA0017 / RCS1160 / S3442) share its detector. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1012 — report-only.

### `CA1018` — Mark attributes with AttributeUsageAttribute.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1018 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1018)

A custom attribute type should declare `[AttributeUsage]` so its valid targets are explicit. Fires on a non-abstract class deriving from `Attribute` that carries no `[AttributeUsage]`, anchored at the type name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1018 — report-only.

### `CA1019` — Define accessors for attribute arguments.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1019 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1019)

Each positional argument of an attribute should be exposed as a read-only property so consumers can read it back. Fires per constructor parameter of an `Attribute`-derived class that has no matching property (by case-insensitive name), anchored at the parameter name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1019 — report-only.

### `CA1021` — Avoid out parameters.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1021 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1021)

`out` parameters force a multi-step call pattern that is awkward for many callers — a return type or struct is usually clearer. Fires on an externally-visible method with an `out` parameter, anchored at the method name; the `bool Try…(out …)` pattern and any non-visible (private/ internal) member are exempt. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1021 — report-only.

### `CA1024` — Use properties where appropriate.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1024 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1024)

A parameterless `GetX` method that returns a value is usually better modelled as a property. Fires on a parameterless, non-`void` method whose name starts with `Get`, anchored at the method name. Well-known framework methods (`GetEnumerator`/`GetHashCode`/`GetType`) and generic methods are exempt. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1024 — report-only.

### `CA1027` — Mark enums with FlagsAttribute.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1027 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1027)

When an enum's members are explicit powers of two with a gap (e.g. `1, 2, 4`), it is almost certainly a bit field and should carry `[Flags]`. Fires on such an enum that is not already `[Flags]`, anchored at the enum name. Conservative: all members must have explicit integer values, every non-zero value a power of two, and the set non-contiguous. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1027 — report-only.

### `CA1031` — Do not catch general exception types.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1031 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1031)

Catching `Exception` (or a bare `catch`) swallows every failure, including ones the code cannot handle. Fires on a `catch` that is bare or catches `Exception` / `SystemException`, anchored at the `catch` keyword. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1031 — report-only.

### `CA1033` — Interface methods should be callable by child types.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1033 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1033)

An externally-visible, unsealed class that implements an interface member explicitly (`int IShape.Area() => …`) hides it from its own derived classes. Fires at the explicit member's name unless the type is sealed, is a struct, is not externally visible, or exposes the functionality through a non-explicit member of the same name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1033 — report-only (sealing or adding an accessible member is a design decision).

### `CA1034` — Nested types should not be visible.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1034 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1034)

A visible nested type complicates the API — callers must qualify it through its container. Fires on a `public`/`protected` type declared inside another type, anchored at the nested type name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1034 — report-only.

### `CA1036` — Override methods on comparable types.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1036 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1036)

A type that implements `IComparable<T>` but defines none of the comparison operators (`<`, `>`, `<=`, `>=`) is missing the operators callers expect. Fires on such a type, anchored at its name. The Sonar (S1210) and Meziantou (MA0096) twins share its detector. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1036 — report-only.

### `CA1040` — Avoid empty interfaces.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1040 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1040)

An interface with no members carries no contract — it is usually a marker better expressed as an attribute. Fires on an `interface` with an empty body, anchored at the interface name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1040 — report-only.

### `CA1041` — Provide a message for ObsoleteAttribute.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1041 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1041)

An `[Obsolete]` attribute without a message tells callers nothing about what to use instead; supply a descriptive message. Fires when `ObsoleteAttribute` is applied with no message — no arguments, an empty `""`, or only named arguments. A non-empty positional string message is fine. Native port of CA1041 — purely syntactic, report-only.

### `CA1044` — Properties should not be write only.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1044 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1044)

A property with a setter but no getter cannot be read, which is confusing. The Microsoft twin of S2376 — a property declaring a `set`/`init` accessor and no `get`, anchored at the property name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1044 — report-only.

### `CA1045` — Do not pass types by reference.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1045 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1045)

A `ref` parameter forces callers into a reference-passing pattern that is error-prone and hard for some languages. Fires per `ref` parameter, anchored at the parameter name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1045 — report-only.

### `CA1046` — Do not overload operator equals on reference types.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1046 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1046)

Overloading `==` on a reference type is surprising, because most callers expect reference equality. Fires on an `operator ==` declared in a class, anchored at the operator symbol. A class that opts into value equality via an `Object.Equals` override or `IEquatable<T>` is exempt, as are structs. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1046 — report-only.

### `CA1047` — Do not declare protected members in sealed types.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1047 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1047)

A `sealed` type cannot be inherited, so a new `protected` member is pointless — nothing can ever access it as protected. Fires on a non-`override` `protected` method or property of a `sealed` class, anchored at the member name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1047 — report-only.

### `CA1050` — Declare types in namespaces.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1050 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1050)

A type declared in the global namespace (no enclosing `namespace`) pollutes the root scope and clashes easily. Fires on a top-level type with no namespace ancestor, anchored at the type name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1050 — report-only.

### `CA1051` — Do not declare visible instance fields.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1051 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1051)

A visible (`public` / `protected` / `protected internal`) instance field exposes implementation state directly, with no validation, versioning, or binary-compatibility boundary — expose it through a property instead. Fires per declarator on a visible, non-`static`, non-`const` field (a `readonly` instance field is still flagged; a `static` field is not — that is CA2211/S1104 territory). Native port of CA1051 — purely syntactic, report-only.

### `CA1052` — Static holder types should be static or sealed.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1052 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1052)

A class that holds only static members should be declared `static` or `sealed` so it is never instantiated or inherited. Fires on a non-static, non-sealed, non-abstract class with no base list whose every member is static/const, a nested type, or a constructor, anchored at the type name — but needs at least one static/const/nested-type/method-etc. member; a class holding nothing but a constructor (or several) is not itself a static holder. Unlike RCS1102, a constructor does not disqualify an otherwise-qualifying type (CA1052's fix removes it by making the type static). For a `partial` type, every part's members are unioned (same-file or across files) and the diagnostic fires once, at the first part in full-path order. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1052 — report-only.

### `CA1054` — URI parameters should not be strings.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1054 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1054)

A parameter that represents a URI should be typed `System.Uri`, not `string`, so it is validated once at the boundary. Fires per `string` parameter whose name reads like a URI (`url`/`uri`/`urn`) on a visible method, anchored at the parameter name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1054 — report-only.

### `CA1055` — URI return values should not be strings.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1055 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1055)

A method that returns a URI should return `System.Uri`, not `string`. Fires on a visible method whose name reads like a URI (`url`/`uri`/`urn`) and whose return type is `string`, anchored at the method name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1055 — report-only.

### `CA1056` — URI properties should not be strings.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1056 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1056)

A property that represents a URI should be typed `System.Uri`, not `string`. Fires on a visible `string` property whose name reads like a URI (`url`/`uri`/`urn`), anchored at the property name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1056 — report-only.

### `CA1064` — Exceptions should be public.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1064 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1064)

A non-public exception type can be thrown but not caught by name outside its assembly, so callers cannot handle it specifically. Fires on a class deriving from `Exception` (first base name ends in `Exception`) that is not `public`, anchored at the type name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1064 — report-only.

### `CA1065` — Do not raise exceptions in unexpected locations.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1065 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1065)

Throwing from a property getter surprises callers who expect a plain read. Fires on a `throw new <X>` in a getter where `X` is not a getter-allowed exception, anchored at the `throw` (the property-getter case of CA1065). The Microsoft twin of Sonar S2372. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1065 — report-only.

### `CA1066` — Implement IEquatable when overriding Equals.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1066 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1066)

A value type that overrides `Equals` should also implement `IEquatable<T>` so equality is strongly typed and allocation-free. Fires on a `struct`/`record struct` that overrides `Equals` but lists no `IEquatable` base, anchored at the type name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1066 — report-only.

### `CA1067` — Override Equals when implementing IEquatable.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1067 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1067)

A type that implements `IEquatable<T>` should also override `Object.Equals(object)` so the two equality paths agree. Fires on a type with an `IEquatable` base that does not override `Equals`, anchored at the type name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1067 — report-only.

### `CA1068` — CancellationToken parameters must come last.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1068 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1068)

By convention a `CancellationToken` is the final parameter, so callers can always tack it on. Fires on a method with a `CancellationToken` parameter that is not in the last position, anchored at the method name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1068 — report-only.

### `CA1069` — Enums should not have duplicate values.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1069 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1069)

Two enum members with the same explicit value are usually a copy-paste slip. Fires on the offending member, anchored at its name and naming the member it collides with. The Microsoft twin of Roslynator RCS1234. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1069 — report-only.

### `CA1507` — Use nameof in place of a string.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1507 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1507)

A string literal whose value matches a parameter name should use `nameof` so a rename keeps it in sync. Fires on such a literal, anchored at it. The Roslynator (RCS1015) and Meziantou (MA0043) twins share its detector. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1507 — report-only.

### `CA1510` — Use the ArgumentNullException throw helper.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1510 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1510)

The boilerplate `if (x == null) throw new ArgumentNullException(...)` should be replaced by `ArgumentNullException.ThrowIfNull(x)`. Fires on such an `if` statement (a `== null` test guarding a single `throw new ArgumentNullException(...)`, with no `else`), anchored at the `if` keyword. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1510 (report-only — the rewrite is structural).

### `CA1512` — Use the ArgumentOutOfRangeException throw helper.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1512 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1512)

A range check of the form `if (x < 0) throw new ArgumentOutOfRangeException(...)` should use an `ArgumentOutOfRangeException.ThrowIf*` helper. Fires on such an `if` (a relational comparison guarding a single `ArgumentOutOfRangeException` throw, with no `else`), anchored at the `if` keyword. Shares CA1510's guard-throw detector. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1512 — report-only.

### `CA1513` — Use the ObjectDisposedException throw helper.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1513 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1513)

A disposed guard of the form `if (disposed) throw new ObjectDisposedException(...)` should use `ObjectDisposedException.ThrowIf`. Fires on such an `if` (a condition guarding a single `ObjectDisposedException` throw, with no `else`), anchored at the `if` keyword. Shares CA1510's guard-throw detector. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1513 — report-only.

### `CA1700` — Do not name enum values 'Reserved'.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1700 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1700)

A member named `Reserved` is a placeholder that leaks an implementation detail into the public contract — drop it or give it a real name. Fires on an enum member named `Reserved`, anchored at the member name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1700 — report-only.

### `CA1704` — Identifiers should be spelled correctly.

*Port of Text.Analyzers CA1704 · Style · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1704)

An identifier whose WordParser-split pieces contain a word absent from the embedded en_US dictionary (case-insensitive) is flagged, per Text.Analyzers' shared dictionary/word-splitting algorithm. Skips numeric and ALL-UPPERCASE (acronym) words. Interface/type-parameter/field names are prefix-stripped (`I`/`T`/`_`) before splitting; a stripped name of length 1 fires a separate "more meaningful name" diagnostic instead. No visibility filter — fires on private members and locals too. `override` and explicit-interface-implementation members are exempt; an interface-implementing parameter with a matching name is exempt when the interface is declared in the same file (report-only native port of Text.Analyzers CA1704).

### `CA1707` — Identifiers should not contain underscores.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1707 · Style · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1707)

`.NET` naming conventions are camel/Pascal-cased without underscores. Fires on an externally-visible type or member whose name contains `_`, anchored at the name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1707 — report-only.

### `CA1708` — Identifiers should differ by more than case.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1708 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1708)

Two members of the same type whose names differ only by case (`Run` and `run`) are confusing and not CLS-compliant. Fires once per type that has any such pair, anchored at the type name (comparing method/property/event/field/nested-type names). For a `partial` type, every part's members are unioned (same-file or across files) before the case-clash check, so two members declared in different parts still clash; the diagnostic fires once, at the first part in full-path order. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1708 — report-only.

### `CA1710` — Identifiers should have correct suffix.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1710 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1710)

A type derived from a well-known base should carry the conventional suffix — `Exception` → `…Exception`, `EventArgs` → `…EventArgs`, `Attribute` → `…Attribute`, `Stream` → `…Stream`, `DataSet`/`DataTable` → matching suffix, `Queue`/`Stack` → `…Queue`/`…Stack` or `…Collection`, `IDictionary`/`IReadOnlyDictionary` → `…Dictionary`/`…Collection`, `ISet`/`IReadOnlySet` → `…Set`/`…Collection`, and `ICollection`/`IReadOnlyCollection`/non-generic `IEnumerable` → one of `…Collection`/`…Dictionary`/`…Set`/`…Stack`/`…Queue`. Fires on such a type whose name lacks every accepted suffix, anchored at its name (a bare GENERIC `IEnumerable<T>` implementer is not flagged — only the non-generic form is a real trigger). Also fires on an EVENT whose project-local delegate type doesn't end in `EventHandler` but matches the standard `(object, TEventArgs)` handler shape — anchored at the EVENT's name, naming the delegate type in the message; a GENERIC delegate reference is substituted single-level before the shape check (issue #166 batch 13). BCL delegates (`Action`/`Func<T>`/`EventHandler`) stay silent. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1710 (report-only — a rename is not a safe syntactic rewrite).

### `CA1711` — Identifiers should not have incorrect suffix.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1711 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1711)

Reserved suffixes mislead readers about a type's kind. Always fires on a type whose name ends in `Enum`, `Delegate`, `EventHandler`, `Ex`, or `Impl`. Also fires on a type whose name ends in `Attribute`/`Collection`/`Dictionary`/`EventArgs`/`Exception`/`Permission`/`Stream`/`Queue`/`Stack` when it provably does not derive from or implement that suffix's required base type/interface (resolved via the project declaration index and BCL type-fact table, issues #129/#144; an unresolvable or untabled base stays silent rather than guess), anchored at the name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1711 (report-only — a rename is not a safe syntactic rewrite).

### `CA1714` — Flags enums should have plural names.

*Port of Text.Analyzers CA1714 · Style · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1714)

A publicly-visible `[Flags]` enum whose name is not a plural (per Humanizer's default English pluralization rules) is harder to read at call sites like `flags.HasFlag(...)`. Names ending in `i`/`ae` and non-ASCII names are exempt. Native port of Text.Analyzers CA1714 — report-only (the fix is a rename).

### `CA1715` — Identifiers should have correct prefix (interfaces `I`, type parameters `T`).

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1715 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1715)

Interface names should start with `I` and generic type-parameter names with `T`. A name passes when its first character is the required prefix and it is a single character or its second character is not lowercase (`IThing`, `TKey`, `T1` pass; `Thing`, `K`, `Type` are flagged). Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1715 (report-only — renames aren't a safe syntactic rewrite).

### `CA1716` — Identifier conflicts with a reserved language keyword.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1716 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1716)

A namespace, or an externally-visible type (public/protected through its nesting chain), whose name matches a reserved keyword in C#, Visual Basic, or C++/CLI is harder for consumers in other languages to use. Matching is case-insensitive; members and types not reachable cross-assembly are exempt. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1716 — report-only (the fix is a rename).

### `CA1717` — Only FlagsAttribute enums should have plural names.

*Port of Text.Analyzers CA1717 · Style · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1717)

A publicly-visible non-`[Flags]` enum whose name is a plural (per Humanizer's default English pluralization rules) suggests the enum was meant to be a bit-flag set. Names ending in `i`/`ae` and non-ASCII names are exempt. Native port of Text.Analyzers CA1717 — report-only (the fix is a rename).

### `CA1720` — Identifiers should not contain type names.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1720 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1720)

Naming a member or parameter after a data type (`Single`, `Object`, `Integer`, …) is confusing and language-specific. Fires on an identifier that is exactly a type name, anchored at the name (a name that merely contains a type word, like `IntValue`, is not flagged). Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1720 (report-only — a rename is not a safe syntactic rewrite).

### `CA1721` — Property names should not match get methods.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1721 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1721)

A property `X` alongside a method `GetX` is confusing — callers cannot tell which to use. Fires on a property whose declaring type also declares a `Get<PropertyName>` method, anchored at the property name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1721 — report-only.

### `CA1805` — Do not initialize unnecessarily.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1805 · Performance · has an autofix* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1805)

The runtime zero-initializes every field, so `int x = 0;` / `bool b = false;` / `object o = null;` just repeats work that already happens. Fires per field declarator or auto-property whose initializer is the type's default value (numeric zero, `false`, or `null`), anchored at the `=`; the fix removes the ` = <value>`. `const` fields are exempt (they require an initializer). Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1805.

### `CA1806` — Do not ignore method results.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1806 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1806)

Discarding a freshly created object, or the new string a pure `string` method returns, wastes the work. Fires on a bare statement that is an object creation or a pure string-method call, anchored at the statement. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1806 — report-only.

### `CA1810` — Initialize reference type static fields inline.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1810 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1810)

A static constructor that only assigns static fields can initialize them inline (a static constructor adds a before-field-init check on every access). Fires on a `static` constructor whose body is only assignment statements, anchored at the constructor name (the Microsoft counterpart of SonarAnalyzer S3963). Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1810 — report-only.

### `CA1813` — Avoid unsealed attributes.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1813 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1813)

Sealing an attribute lets the runtime look it up faster and signals it is not an extension point. Fires on a non-`sealed`, non-`abstract` class deriving from `Attribute`, anchored at the type name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1813 — report-only.

### `CA1814` — Prefer jagged arrays over multidimensional.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1814 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1814)

A multidimensional array (`int[,]`) can waste space when rows differ in length; a jagged array (`int[][]`) is often leaner. Fires on a parameter declared with a multidimensional array type, anchored at the parameter name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1814 — report-only.

### `CA1815` — Override equals and operator equals on value types.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1815 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1815)

A value type with any real member should override `Equals` and overload `operator ==` so its instances compare by value. Fires on a `struct` that has at least one non-constructor member (or already shows equality intent) but lacks BOTH an `Equals` override and an `==` operator, anchored at the type name. An empty struct is exempt. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1815 — report-only.

### `CA1819` — Properties should not return arrays.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1819 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1819)

A property returning an array hands callers a mutable copy (or a shared reference) on every access, which is surprising and allocation-heavy. Fires on a public property whose type is an array, anchored at the property name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1819 — report-only.

### `CA1820` — Test for empty strings using string length.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1820 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1820)

Comparing a string with `==`/`!=` against an empty string literal is slower than testing `.Length`. Fires on such a comparison, anchored at the binary expression. The Roslynator twin (RCS1156) shares its detector. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1820 — report-only.

### `CA1821` — Remove empty finalizers.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1821 · Performance · has an autofix* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1821)

An empty finalizer (`~T() { }`) needlessly promotes every instance to the finalization queue, hurting GC performance, with no benefit. Fires on an empty finalizer, anchored at its name; the fix removes the declaration. A finalizer containing a comment or statement is left alone. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1821.

### `CA1822` — Member can be marked as static.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1822 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1822)

A method that references no instance data can be declared `static`, saving a hidden `this` parameter on every call. Report-only, narrow same-file subset (no base class, no interfaces — see module doc for the full soundness gate and its documented gaps vs the semantic oracle). Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1822.

### `CA1825` — Avoid zero-length array allocations.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1825 · Performance · has an autofix* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1825)

`new T[0]` / `new T[] { }` allocates a new empty array each time, whereas `Array.Empty<T>()` returns a cached shared instance. Fires on an array creation whose length is statically zero (a `0` rank size, or an empty initializer), anchored at `new`; the fix rewrites it to `Array.Empty<T>()`. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1825.

### `CA1834` — Use StringBuilder.Append(char) for single characters.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1834 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1834)

`builder.Append("x")` with a single-character string allocates needlessly; the `char` overload is cheaper. Fires on an `Append` call whose single argument is a one-character string literal, anchored at that literal. The Meziantou twin is MA0028. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1834 — report-only.

### `CA1847` — Use a char literal for a single-character Contains lookup.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1847 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1847)

`s.Contains("x")` with a single-character string is slower than `s.Contains('x')`. Fires on a `Contains` call whose single argument is a one-character string literal, anchored at that literal. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1847 — report-only.

### `CA1860` — Array '.Any()' should compare Length to 0.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1860 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1860)

The Microsoft twin of Roslynator RCS1080: `array.Any()` should compare `Length` to 0 — clearer and faster than materialising an enumerator. Same detection (a no-argument `.Any()` on an in-file array receiver) anchored at the whole invocation. Native port of CA1860 — report-only.

### `CA1866` — Use the char overload of IndexOf.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1866 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1866)

`s.IndexOf("x")` with a single-character string allocates needlessly; the `char` overload is cheaper. Fires on an `IndexOf` call whose single argument is a one-character string literal, anchored at the argument list. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1866 — report-only.

### `CA1873` — Avoid potentially expensive logging arguments.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1873 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1873)

A call to a well-known `ILogger` extension logging method (`LogDebug`/`LogTrace`/`LogInformation`/ `LogWarning`/`LogError`/`LogCritical`/`Log`) whose FIRST argument is a string-literal message template and which has at least one fill argument always evaluates those arguments eagerly, even when the log level ends up disabled — unless the call sits inside an `if (<expr>.IsEnabled(...))` guard. Anchored at the call's own start. Report-only; narrow, conservative — see module doc for the out-of-scope shapes (interpolated templates). Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1873.

### `CA2002` — Do not lock on objects with weak identity.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2002 · Concurrency · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2002)

Locking on an object with weak identity — `this`, a `Type` (`typeof(...)`), or a string — risks deadlock because unrelated code can lock the same instance. Fires on such a `lock`, anchored at the locked expression. The Sonar (S2551), Roslynator (RCS1059) and Meziantou (MA0064) twins share its detector. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2002 — report-only.

### `CA2007` — Task awaited without ConfigureAwait.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2007 · Concurrency · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2007)

Awaiting a task without `.ConfigureAwait(false)` resumes on the captured synchronization context, which can deadlock callers of library code. Fires on an `await <expr>` whose operand is not already a `ConfigureAwait` call and is not `Task.Yield()`, anchored at the awaited expression. Native port of CA2007 — report-only (whether to configure-await is a project-wide choice).

### `CA2011` — Do not assign a property within its setter.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2011 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2011)

Assigning a property inside its own `set` accessor recurses forever. Fires on such an assignment, anchored at it. The Sonar twin (S2190) shares its detector but anchors at the accessor. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2011 — report-only.

### `CA2014` — Do not use stackalloc in loops.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2014 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2014)

`stackalloc` inside a loop keeps allocating stack space each iteration without freeing it until the method returns, risking a stack overflow. Fires on a `stackalloc` whose nearest enclosing scope (before any method/lambda/local-function boundary) is a loop, anchored at the `stackalloc`. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2014 — report-only.

### `CA2016` — A CancellationToken in scope should be forwarded to a callee that accepts one.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2016 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2016)

A call inside a method that has a `CancellationToken` available (parameter, local, or field) to another method with an overload accepting one more argument — a trailing `CancellationToken` — but omits it, drops cancellation propagation. Anchored at the invocation. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2016 — the Microsoft twin of Meziantou.Analyzer MA0040 (identical detection and anchor, verified against the oracle) — report-only.

### `CA2200` — Rethrow to preserve stack details.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2200 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2200)

`throw ex;` inside a `catch` resets the exception's stack trace to that line, losing the original throw site; a bare `throw;` preserves it. Fires when the thrown identifier is the enclosing `catch`'s exception variable, anchored at the `throw`. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2200 — purely syntactic, report-only (a syntactic pass cannot prove the variable was not reassigned, so no autofix).

### `CA2201` — Do not raise reserved exception types.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2201 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2201)

Throwing a too-general base type (`Exception`/`ApplicationException`/`SystemException`) is not sufficiently specific; throwing a runtime-reserved type (`NullReferenceException`, …) usurps a type the runtime owns. Fires on `throw new <type>(…)`, anchored at the object creation. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2201 — report-only.

### `CA2208` — Instantiate argument exceptions correctly.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2208 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2208)

`ArgumentNullException` / `ArgumentOutOfRangeException` take a parameter name as their string argument. Passing a message (or any string that is not a parameter of the enclosing method) means the `paramName` is wrong. Fires on such an object creation, anchored at it. Reuses CA1507's parameter-matching detector, inverted. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2208 — report-only.

### `CA2211` — Non-constant fields should not be visible.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2211 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2211)

A visible (`public` / `protected`) `static` mutable field is global shared state any caller can reassign, with no thread-safety or invariants — make it `const`, `static readonly`, or a property. Fires per declarator on a visible static field that is neither `const` nor `readonly`. The instance equivalent is CA1051. Native port of CA2211 — purely syntactic, report-only.

### `CA2225` — Operator overloads have named alternates.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2225 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2225)

An operator overload should be paired with a friendly named method (e.g. `operator +` ↔ `Add`) so languages without operator overloading can still use it. Fires on an operator whose alternate method is absent from the declaring type (anchored at the operator symbol); a conversion operator expects a `To<T>`/`From<S>` method and is anchored at the target type; `operator true` expects an `IsTrue` property (`operator false` shares it). Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2225 — report-only.

### `CA2227` — Collection properties should be read only.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2227 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2227)

A writable collection property lets callers replace the whole collection, which is rarely intended — expose it read-only and mutate through its API. Fires on a visible property of a collection type that has a setter, anchored at the property name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2227 — report-only.

### `CA2231` — Overload operator equals on overriding value type Equals.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2231 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2231)

A value type that overrides `Equals` but does not define `operator ==` lets `a == b` and `a.Equals(b)` disagree. Fires on a `struct`/`record struct` that overrides `Equals` and declares no `==` operator, anchored at the type name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2231 — report-only.

### `CA2241` — Provide correct arguments to formatting methods.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2241 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2241)

A `string.Format` whose format string references a `{N}` index with no matching argument throws at run time. Fires on such a call, anchored at the invocation. The Sonar twin is S2275. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2241 — report-only.

### `CA2245` — Do not assign a property to itself.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2245 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2245)

`this.Foo = this.Foo` is a no-op. Fires on an assignment whose two sides are the same property reference (recognised by a same-class property declaration; a plain field self-assignment is S1656's domain), anchored at the right-hand side. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2245 — report-only.

### `CA2249` — Use string.Contains instead of string.IndexOf.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2249 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2249)

`s.IndexOf(x) >= 0` is a roundabout presence check; `s.Contains(x)` reads better. Fires on an `IndexOf(...) >= 0` comparison, anchored at the comparison. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2249 — report-only.

### `CA5350` — Do not use weak cryptographic algorithms.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA5350 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca5350)

Fires on construction of, or the bare `<Type>.Create()` static-factory call on, a weak hashing/symmetric-algorithm type (`SHA1`, `HMACSHA1`, `TripleDES` and their `*CryptoServiceProvider`/`*Managed`/`*Cng` concrete subclasses; `RIPEMD160`-family types are matched by name too but don't exist in net10.0's BCL, so untestable in this corpus). Message interpolates the enclosing member name and the algorithm's base name. Approximate (written-name matching, no symbol resolution) port of Microsoft.CodeAnalysis.NetAnalyzers CA5350 — report-only.

### `CA5351` — Do not use broken cryptographic algorithms.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA5351 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca5351)

Fires on construction of, or the bare `<Type>.Create()` static-factory call on, a broken hashing/symmetric-algorithm type (`MD5`, `DES`, `HMACMD5`, `RC2` and their `*CryptoServiceProvider` concrete subclasses); plus a `DSA`/`DSACryptoServiceProvider`-typed receiver's `.CreateSignature(...)` call specifically (a bare `new DSACryptoServiceProvider()` alone does NOT fire), and `new DSASignatureFormatter(...)` (reports the wrapped algorithm's base name `DSA`). Message interpolates the enclosing member name and the algorithm's base name. Approximate (written-name matching, no symbol resolution) port of Microsoft.CodeAnalysis.NetAnalyzers CA5351 — report-only.

### `CA5359` — Do not disable certificate validation.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA5359 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca5359)

Fires on a lambda/anonymous-method whose body always returns literal `true`, when it is assigned to a member named exactly `ServerCertificateValidationCallback` (NOT the differently-named `ServerCertificateCustomValidationCallback`) or passed to `new RemoteCertificateValidationCallback(...)` — anchored at the lambda's own start. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA5359 — report-only (no safe mechanical rewrite; real validation logic is the author's call).

### `CA5384` — Do not use the Digital Signature Algorithm (DSA).

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA5384 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca5384)

Fires on construction of a concrete DSA type (`DSACryptoServiceProvider`, `DSACng`, `DSAOpenSsl`); on a return-position expression (`return`/`=>`, including local functions and get-accessors) whose enclosing method/property's declared return type is `DSA` or one of the above concrete types (a bare DSA-typed local/field declaration or property initializer does NOT fire — only an actual return counts); and on `AsymmetricAlgorithm.Create("DSA")` / `CryptoConfig.CreateFromName("DSA")` (the string-literal argument is the type-identifying signal). Approximate (written-name matching, no symbol resolution) port of Microsoft.CodeAnalysis.NetAnalyzers CA5384 — report-only.

### `EF1002` — Interpolated SQL passed to a raw-SQL EF Core method loses auto-parameterization.

*Port of Microsoft.EntityFrameworkCore.Analyzers EF1002 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/ef/core/miscellaneous/cli/dbcontext-scaffolding)

`FromSqlRaw`/`ExecuteSqlRaw`/`ExecuteSqlRawAsync` don't auto-parameterize interpolation holes the way `FromSql`/`ExecuteSql` do — passing an interpolated string to the Raw overload silently becomes naive string formatting. Fires when the interpolated-string SQL argument has a non-literal hole, anchored at the method name; an all-literal-hole interpolation is exempt. Native port of Microsoft.EntityFrameworkCore.Analyzers EF1002 — report-only.

### `EF1003` — Concatenated SQL passed to a raw-SQL EF Core method risks SQL injection.

*Port of Microsoft.EntityFrameworkCore.Analyzers EF1003 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/ef/core/miscellaneous/cli/dbcontext-scaffolding)

`FromSqlRaw`/`ExecuteSqlRaw`/`ExecuteSqlRawAsync` skip EF Core's auto-parameterization. Fires when the SQL argument is a `+`-concatenation with a non-literal operand, anchored at the method name; an all-literal concatenation is exempt (confirmed against the oracle). Native port of Microsoft.EntityFrameworkCore.Analyzers EF1003 — report-only.

### `GU0005` — Use correct argument positions.

*Port of Gu.Analyzers GU0005 · Correctness · report-only* · [upstream docs](https://github.com/GuOrg/Gu.Analyzers/blob/master/documentation/GU0005.md)

`ArgumentException(message, paramName)` and `ArgumentNullException`/ `ArgumentOutOfRangeException(paramName, message)` take their two string arguments in OPPOSITE order. Fires when the two string-literal arguments look swapped: the message-slot literal matches an enclosing parameter name (so it reads like it should have been the paramName) while the paramName-slot literal does not match any parameter. Anchored at the misplaced message-slot argument. Native port of Gu.Analyzers GU0005 — report-only.

### `GU0009` — Name the boolean argument.

*Port of Gu.Analyzers GU0009 · Style · report-only* · [upstream docs](https://github.com/GuOrg/Gu.Analyzers/blob/master/documentation/GU0009.md)

A boolean literal passed positionally reads poorly at the call site. Fires on such a literal argument that isn't already named, anchored at the literal. Excludes `ConfigureAwait` (shared with MA0003) and, unlike MA0003, a bare boolean literal inside an expression-tree lambda (best-effort syntactic detection — see module docs for the exact, bounded proxy) and the virtual-dispose pattern (a single-argument call to a method named `Dispose`). Native port of Gu.Analyzers GU0009 — report-only (a fix would need the callee's parameter name, unresolvable for external/BCL callees).

### `GU0010` — Assigning same value.

*Port of Gu.Analyzers GU0010 · Correctness · report-only* · [upstream docs](https://github.com/GuOrg/Gu.Analyzers/blob/master/documentation/GU0010.md)

An assignment whose two sides are the same expression (`a = a;`, `this.field = this.field;`, `this.Value = this.Value;`) does nothing and is almost always a typo. Fires at the assignment when the operator is `=` and the left side is a plain identifier or member access (never element-access — `arr[0] = arr[0];` is exempt) whose text matches the right side exactly (ignoring whitespace). Covers the same ground as S1656 (local/field) plus CA2245 (property), minus element-access. Native port of Gu.Analyzers GU0010 — report-only (the intended right-hand side is a human decision). Upstream default severity is Error, unusually high for this catalog — confirmed against the live oracle.

### `GU0017` — Don't read a variable named `_` as a value.

*Port of Gu.Analyzers GU0017 · Maintainability · report-only* · [upstream docs](https://github.com/GuOrg/Gu.Analyzers/blob/master/documentation/GU0017.md)

A local variable or parameter literally named `_` reads as a discard but is a real, usable binding — reading its value elsewhere is confusing (was it meant to be discarded?). Complementary to PH2147 (which flags the declaration): fires on a subsequent READ of the same name, anchored at the read. Exempt: the declaration itself, assigning INTO `_` (`_ = Foo();`, the discard-return-value idiom), an `out`/`ref`/`in _` argument, and a field named `_` (out of scope for both this port and PH2147). Native port of Gu.Analyzers GU0017 — report-only.

### `GU0061` — Enum member value is too large.

*Port of Gu.Analyzers GU0061 · Correctness · report-only* · [upstream docs](https://github.com/GuOrg/Gu.Analyzers/blob/master/documentation/GU0061.md)

Fires on an enum member whose value is a bare `1 << n` shift expression with `n > 30`, in an enum whose underlying type is (implicitly or explicitly) `int` — every narrower scope cut (n == 30, a non-`int` underlying type, a cast-wrapped shift, a plain large integer literal) was probed against the live oracle and confirmed exempt. Anchored at the shift expression. Native port of Gu.Analyzers GU0061 — report-only (the fix is a human decision about the enum's intended underlying type or the value itself).

### `GU0073` — Member of non-public type should be internal.

*Port of Gu.Analyzers GU0073 · Correctness · report-only* · [upstream docs](https://github.com/GuOrg/Gu.Analyzers/blob/master/documentation/GU0073.md)

A `public` member of a type that is itself not `public` (internal, or nested with an accessibility narrower than the assembly) is misleading — it can never actually be used from outside the assembly, so it should be `internal` instead. Fires at the `public` keyword; exempt: `override`s, struct constructors, and (for non-static properties/methods/events) a member whose name matches some interface member on the containing type's full transitive interface set (resolved via the BCL type-fact table, issue #144, and the project index — `Unknown` suppresses). Native port of Gu.Analyzers GU0073; report-only.

### `GU0090` — Don't throw NotImplementedException.

*Port of Gu.Analyzers GU0090 · Maintainability · report-only* · [upstream docs](https://github.com/GuOrg/Gu.Analyzers/blob/master/documentation/GU0090.md)

A `throw new NotImplementedException(...)` is a placeholder for unwritten code. Fires in both statement and expression-bodied position, anchored at the `new` keyword (unlike the near-twin MA0025, which anchors at `throw`). Native port of Gu.Analyzers GU0090 — report-only (the functionality has to be written by hand).

### `IDE0028` — Collection initialization can use a collection expression.

*Port of dotnet/roslyn IDE0028 (Microsoft.CodeAnalysis.CSharp.CodeStyle) · Style · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0028)

A field/local declarator with an explicit `List<T>`-compatible declared type (`List`/`IList`/ `ICollection`/`IEnumerable`/`IReadOnlyList`/`IReadOnlyCollection<T>`), initialized with a no-argument `new List<T>()`/`new List<T> { ... }`, can use a collection expression instead. Anchored at the `new` keyword. Report-only; narrow, conservative — see module doc for the out-of-scope shapes (arrays, other collection types, `.Add`-call sequences). Native port of the .NET SDK code-style analyzer IDE0028, surfaced by `dotnet format`.

### `IDE0090` — 'new' expression can use target-typed 'new()'.

*Port of dotnet/roslyn IDE0090 (Microsoft.CodeAnalysis.CSharp.CodeStyle) · Style · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0090)

A field or local declarator with an explicit (non-`var`) type, initialized with `new T(...)` where `T`'s text is byte-identical to the declared type, can drop the redundant type name (`new()`). Anchored at the `new` keyword. Report-only (narrow, conservative — see module doc for the out-of-scope shapes). Native port of the .NET SDK code-style analyzer IDE0090, surfaced by `dotnet format`.

### `IDE0290` — Constructor can be converted to a primary constructor.

*Port of dotnet/roslyn IDE0290 (Microsoft.CodeAnalysis.CSharp.CodeStyle) · Style · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0290)

The sole constructor of a class/struct/record whose body consists ONLY of plain `=` assignment statements (bare-identifier or `this.`-qualified LHS) is convertible to a primary constructor. Anchored at the constructor name. Report-only — a type with more than one constructor, or a body with any non-assignment statement, is exempt (narrow, conservative — see module doc). Native port of the .NET SDK code-style analyzer IDE0290, surfaced by `dotnet format`.

### `IDISP018` — Call SuppressFinalize when the type has a finalizer.

*Port of IDisposableAnalyzers IDISP018 · Correctness · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/IDisposableAnalyzers/blob/master/documentation/IDISP018.md)

A type with a `~T()` finalizer relies on `GC.SuppressFinalize(this)` in `Dispose()` to skip the (now redundant) finalization pass once disposed. Fires on a `public void Dispose()` (parameterless, non-static, no attributes) whose own body calls no `GC.SuppressFinalize` at all, when the containing type declares its own destructor, anchored at the method name. Native port of IDisposableAnalyzers IDISP018 — report-only.

### `IDISP019` — Call SuppressFinalize when the type has a virtual dispose method.

*Port of IDisposableAnalyzers IDISP019 · Correctness · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/IDisposableAnalyzers/blob/master/documentation/IDISP019.md)

A type offering the `protected virtual void Dispose(bool disposing)` extension point relies on `GC.SuppressFinalize(this)` in `Dispose()` so a derived class's finalizer (if any) is also suppressed once disposed. Fires on a `public void Dispose()` (parameterless, non-static, no attributes) whose own body calls no `GC.SuppressFinalize` at all, when the containing type (with no destructor of its own — IDISP018's case) declares its own `virtual` `Dispose(bool)`, anchored at the method name. Native port of IDisposableAnalyzers IDISP019 — report-only.

### `IDISP020` — Call SuppressFinalize(this).

*Port of IDisposableAnalyzers IDISP020 · Correctness · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/IDisposableAnalyzers/blob/master/documentation/IDISP020.md)

`GC.SuppressFinalize` should be passed `this` so the current instance's finalizer is suppressed. Fires inside a `public void Dispose()` (parameterless, non-static, no attributes) when a `GC.SuppressFinalize(x)` call passes something other than `this`, anchored at the argument. Native port of IDisposableAnalyzers IDISP020 — report-only.

### `IDISP021` — Call this.Dispose(true).

*Port of IDisposableAnalyzers IDISP021 · Correctness · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/IDisposableAnalyzers/blob/master/documentation/IDISP021.md)

The parameterless `public void Dispose()` should call `Dispose(true)` to run the full disposal path. Fires when its `Dispose(bool)` call passes something other than the `true` literal, anchored at the argument. Native port of IDisposableAnalyzers IDISP021 — report-only.

### `IDISP022` — Call this.Dispose(false).

*Port of IDisposableAnalyzers IDISP022 · Correctness · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/IDisposableAnalyzers/blob/master/documentation/IDISP022.md)

A finalizer (`~T()`) should call `Dispose(false)` — it runs on the GC thread where managed references may already be collected, so only unmanaged cleanup is safe. Fires when a destructor's `Dispose(bool)` call passes something other than the `false` literal, anchored at the argument. Native port of IDisposableAnalyzers IDISP022 — report-only.

### `IDISP024` — Don't call GC.SuppressFinalize(this) when the type is sealed and has no finalizer.

*Port of IDisposableAnalyzers IDISP024 · Redundancy · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/IDisposableAnalyzers/blob/master/documentation/IDISP024.md)

Calling `GC.SuppressFinalize(this)` is pointless when the type is sealed and declares no finalizer — there is no finalizer to suppress. Fires on `GC.SuppressFinalize(this)` inside a sealed class (or a struct, which is implicitly sealed) that has no `~T()` destructor, anchored at the call. Native port of IDisposableAnalyzers IDISP024 — report-only.

### `IDISP025` — Class with no virtual dispose method should be sealed.

*Port of IDisposableAnalyzers IDISP025 · Correctness · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/IDisposableAnalyzers/blob/master/documentation/IDISP025.md)

A non-sealed class that is IDisposable, has its own `Dispose()` (or explicit `IDisposable.Dispose()`), and does not offer a `Dispose(bool)` virtual dispose method cannot be safely extended — a derived class has no protected hook to add its own cleanup. Either seal the class or add the `protected virtual void Dispose(bool disposing)` pattern. Native port of IDisposableAnalyzers IDISP025, consuming the BCL type-fact table (issue #144) for the `IDisposable`-assignability check; report-only.

### `IDISP026` — Class with no virtual DisposeAsyncCore method should be sealed.

*Port of IDisposableAnalyzers IDISP026 · Correctness · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/IDisposableAnalyzers/blob/master/documentation/IDISP026.md)

A non-sealed class that is IAsyncDisposable (and NOT IDisposable), has its own `DisposeAsync()`, and does not offer the recommended `protected virtual ValueTask DisposeAsyncCore()` extension point cannot be safely extended for async cleanup. Either seal the class or add `DisposeAsyncCore()`. Native port of IDisposableAnalyzers IDISP026, consuming the BCL type-fact table (issue #144); report-only.

### `MA0003` — A bare null/boolean argument should be named for readability.

*Port of Meziantou.Analyzer MA0003 · Style · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0003.md)

A `null` or boolean literal passed positionally reads poorly at the call site — the reader can't tell what it means without checking the callee's signature. Fires on such a literal argument that isn't already named, anchored at the literal (int/string literals and non-literal expressions are unaffected). Excludes `ConfigureAwait`, whose bare bool is idiomatic. Native port of Meziantou.Analyzer MA0003 — report-only (a fix would need the callee's parameter name, unresolvable for external/BCL callees).

### `MA0004` — Await without ConfigureAwait(false).

*Port of Meziantou.Analyzer MA0004 · Concurrency · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0004.md)

Awaiting without `.ConfigureAwait(false)` resumes on the captured synchronization context, which can deadlock library callers. The Meziantou twin of CA2007 — fires on an `await <expr>` (anchored at the `await` keyword) or an `await using` (anchored at the resource) whose awaited value is not already `ConfigureAwait`'d (and, for `await <expr>`, not `Task.Yield()`). Native port of Meziantou MA0004 — report-only (whether to configure-await is a project-wide choice).

### `MA0005` — Use Array.Empty<T>() instead of allocating a zero-length array.

*Port of Meziantou.Analyzer MA0005 · Performance · has an autofix* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0005.md)

`new T[0]` / `new T[] { }` allocates a new empty array each time, whereas `Array.Empty<T>()` returns a cached shared instance. Fires on an array creation whose length is statically zero (a `0` rank size, or an empty initializer), anchored at `new`; the fix rewrites it to `Array.Empty<T>()`. The Meziantou twin of CA1825. Native port of Meziantou.Analyzer MA0005.

### `MA0007` — Add comma after the last value.

*Port of Meziantou.Analyzer MA0007 · Style · has an autofix* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0007.md)

Ending the last element of a multi-line initializer or `enum` member list with a trailing comma keeps diffs minimal and reordering clean. Fires on the last element of a multi-line initializer/member list that lacks a trailing comma (single-line is exempt). Native port of Meziantou MA0007 (the same shape as StyleCop SA1413); the fix inserts the comma.

### `MA0008` — Add StructLayoutAttribute.

*Port of Meziantou.Analyzer MA0008 · Performance · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0008.md)

A `struct` with two or more instance fields and no `[StructLayout]` leaves field ordering and packing to the runtime, which can break interop and surprise equality. Fires at the struct name on such a struct; one with fewer than two instance fields (static/const don't count) or that already declares `[StructLayout]` is exempt. Native port of Meziantou.Analyzer MA0008 — purely syntactic, report-only.

### `MA0010` — Mark attributes with AttributeUsageAttribute.

*Port of Meziantou.Analyzer MA0010 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0010.md)

A custom attribute type should declare `[AttributeUsage]` so its valid targets are explicit. The Meziantou twin of CA1018 — a non-abstract class deriving from `Attribute` with no `[AttributeUsage]`, anchored at the type name. Native port of Meziantou.Analyzer MA0010 — report-only.

### `MA0012` — Do not raise reserved exception types.

*Port of Meziantou.Analyzer MA0012 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0012.md)

The runtime owns certain exception types (`NullReferenceException`, `IndexOutOfRangeException`, `OutOfMemoryException`, …); throwing them from user code masks real runtime failures. Fires on `throw new <reserved>(…)`, anchored at the `throw` keyword. Native port of Meziantou.Analyzer MA0012 — report-only.

### `MA0015` — Specify the parameter name in ArgumentException.

*Port of Meziantou.Analyzer MA0015 · Correctness · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0015.md)

The Meziantou twin of S3928: an argument-exception whose `paramName` string is not an enclosing parameter is flagged at that string; a base `ArgumentException` with no message is flagged at the object creation. Shares CA2208's classification. Native port of Meziantou.Analyzer MA0015 — report-only.

### `MA0016` — Prefer using collection abstraction instead of implementation.

*Port of Meziantou.Analyzer MA0016 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0016.md)

A public API should expose a collection interface (`IReadOnlyList<T>`, `ICollection<T>`, …), not a concrete type like `List<T>`/`Dictionary<…>`, so it can evolve. Fires on a visible property type, method return type, or method parameter type that is a concrete collection, anchored at the type. Native port of Meziantou.Analyzer MA0016 — report-only.

### `MA0017` — Abstract types should not have public or internal constructors.

*Port of Meziantou.Analyzer MA0017 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0017.md)

The Meziantou twin of CA1012: fires on each `public`/`internal` constructor of an `abstract` class, anchored at the constructor name. Shares CA1012's detector. Native port of Meziantou.Analyzer MA0017 — report-only.

### `MA0018` — Do not declare static members on generic types.

*Port of Meziantou.Analyzer MA0018 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0018.md)

A static member of a generic type must be referenced with the full type arguments (`Holder<int>.Member`), which is awkward and surprising. Fires on a `static` method or property whose immediately-declaring type is generic, anchored at the member name. The Microsoft twin of CA1000. Native port of Meziantou.Analyzer MA0018 — report-only.

### `MA0025` — Implement the functionality instead of throwing NotImplementedException.

*Port of Meziantou.Analyzer MA0025 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0025.md)

A `throw new NotImplementedException(...)` is a placeholder for unwritten code. Fires at the `throw`, in both statement and expression position. Native port of Meziantou.Analyzer MA0025 — report-only (the functionality has to be written by hand).

### `MA0026` — Fix TODO comments.

*Port of Meziantou.Analyzer MA0026 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0026.md)

A comment that begins with `TODO` marks deferred work that should be tracked and resolved. Fires when a line or block comment's content starts with a whole-word `TODO`, anchored at the `TODO` (the Meziantou counterpart of Sonar S1135, which flags `TODO` anywhere in a comment). Native port of Meziantou.Analyzer MA0026 — purely syntactic, report-only.

### `MA0027` — Prefer rethrowing an exception implicitly.

*Port of Meziantou.Analyzer MA0027 · Correctness · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0027.md)

`throw ex;` inside a `catch` resets the exception's stack trace; a bare `throw;` preserves it. Fires when the thrown identifier is the enclosing `catch`'s exception variable, anchored at the `throw` (the Meziantou counterpart of CA2200). Native port of Meziantou.Analyzer MA0027 — report-only (a syntactic pass cannot prove the variable was not reassigned).

### `MA0028` — Optimize StringBuilder usage (Append a char, not a one-char string).

*Port of Meziantou.Analyzer MA0028 · Performance · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0028.md)

`builder.Append("x")` with a single-character string should use the `char` overload. Fires on such an `Append`, anchored at the string literal. The Meziantou twin of CA1834. Native port of Meziantou.Analyzer MA0028 — report-only.

### `MA0029` — Combine LINQ methods.

*Port of Meziantou.Analyzer MA0029 · Performance · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0029.md)

A `Where(predicate).Count()` chain (or `.Any()`, `.First()`, …) should fold the predicate into the terminal. The Meziantou twin of S2971, but also flagging a terminal that already carries an argument, anchored at the start of the chain. Native port of Meziantou.Analyzer MA0029 — report-only.

### `MA0036` — Make class static.

*Port of Meziantou.Analyzer MA0036 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0036.md)

A class with no instance state — including an empty class — can be marked `static` so it is never instantiated. Fires on a non-`static`, non-`abstract` class with no base list whose every member is `static`/`const`, anchored at the name (the Meziantou counterpart of RCS1102, but also flagging empty classes and not exempting `partial`). A primary constructor — even a zero-arg `class C()` — declares an instance constructor and exempts the class. For a `partial` type, every part's members are unioned (same-file or across files) and the diagnostic fires once, at the first part in full-path order. Native port of Meziantou.Analyzer MA0036 (report-only — adding `static` can ripple into callers).

### `MA0037` — Remove empty statement.

*Port of Meziantou.Analyzer MA0037 · Redundancy · has an autofix* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0037.md)

A standalone `;` is a no-op that usually slipped in by accident. Fires on an empty statement, anchored at the `;`; the fix deletes it. The Meziantou counterpart of Sonar S1116 / StyleCop SA1106. Native port of Meziantou.Analyzer MA0037.

### `MA0040` — A CancellationToken in scope should be forwarded to a callee that accepts one.

*Port of Meziantou.Analyzer MA0040 · Concurrency · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0040.md)

A call inside a method that has a `CancellationToken` available (parameter, local, or field) to another method with an overload accepting one more argument — a trailing `CancellationToken` — but omits it, drops cancellation propagation. Anchored at the invocation. Native port of Meziantou.Analyzer MA0040, scoped to in-file callees resolved by name + arity (no semantic overload resolution) — a BCL callee's CancellationToken overload is a documented miss.

### `MA0042` — Do not use Thread.Sleep in an async method.

*Port of Meziantou.Analyzer MA0042 · Concurrency · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0042.md)

`Thread.Sleep` blocks the thread, defeating the point of `async`; inside an `async` method use `await Task.Delay(...)` so the thread is released while waiting. Fires on a `Thread.Sleep(...)` call whose nearest enclosing function (method, local function, lambda, or accessor) is `async`; a blocking sleep in a synchronous method is not flagged. Native port of Meziantou MA0042 — purely syntactic, report-only.

### `MA0043` — Use nameof operator.

*Port of Meziantou.Analyzer MA0043 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0043.md)

A string literal whose value matches a parameter name should use `nameof`. Fires on such a literal, anchored at it. The Meziantou twin of CA1507. Native port of Meziantou.Analyzer MA0043 — report-only.

### `MA0044` — Useless ToString call on a string.

*Port of Meziantou.Analyzer MA0044 · Redundancy · has an autofix* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0044.md)

Calling `.ToString()` on a value that is already a `string` is useless. The Meziantou twin of RCS1097 — same detection (a no-argument `ToString()` on a `string`-typed parameter or local) and the same fix (deletes the call), anchored at the `.` before `ToString`. Native port of Meziantou.Analyzer MA0044.

### `MA0046` — Use EventHandler<T> to declare events.

*Port of Meziantou.Analyzer MA0046 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0046.md)

The Meziantou twin of CA1003: an event's delegate should match the conventional `(object, TEventArgs) -> void` handler shape — checked by SIGNATURE SHAPE, not by the delegate's name (issue #166 batch 12), with NO textual `…EventArgs` fallback (purely semantic, unlike CA1003's own shared shape helper). A GENERIC delegate reference is substituted single-level before the shape check (issue #166 batch 13). Fires (regardless of visibility) on an event — field-like or custom — whose delegate does not match that shape, anchored at the event name, with a message naming the specific failing conjunct; exempt when the event implements an interface member (project-local, direct-base-list only). Native port of Meziantou.Analyzer MA0046 — report-only.

### `MA0047` — Declare types in namespaces.

*Port of Meziantou.Analyzer MA0047 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0047.md)

A top-level type should be declared in a namespace, not the global scope. Fires on a type with no namespace ancestor, anchored at its name (the Meziantou twin of CA1050). Native port of Meziantou.Analyzer MA0047 — report-only.

### `MA0051` — Method is too long.

*Port of Meziantou.Analyzer MA0051 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0051.md)

A method body longer than 60 lines (the Meziantou default) should be split into smaller methods. Fires on a method whose body block spans more than 60 lines, anchored at the method name. Native port of Meziantou.Analyzer MA0051 — report-only.

### `MA0055` — Do not use finalizers.

*Port of Meziantou.Analyzer MA0055 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0055.md)

Finalizers run non-deterministically, delay garbage collection, and are rarely the right tool — use `IDisposable` instead. Fires on every finalizer, anchored at its name (unlike CA1821 / RCS1259, which only remove *empty* ones). Native port of Meziantou.Analyzer MA0055 — report-only (a finalizer may contain real cleanup).

### `MA0058` — Exception class names should end with 'Exception'.

*Port of Meziantou.Analyzer MA0058 · Style · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0058.md)

A class that derives from an exception type should have a name ending in `Exception` (`FileNotFoundException`, not `FileNotFound`) — the convention readers and tooling use to spot exceptions. Fires at the class name when an exception-derived class does not follow it. Exception-derivation is detected syntactically (the base type's name ends in `Exception`), shared with RCS1194. Native port of Meziantou.Analyzer MA0058 — report-only.

### `MA0064` — Avoid locking on publicly accessible instance.

*Port of Meziantou.Analyzer MA0064 · Concurrency · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0064.md)

The Meziantou twin of CA2002: locking on `this`, a `Type`, or a string is flagged at the locked expression. Shares CA2002's detector. Native port of Meziantou.Analyzer MA0064 — report-only.

### `MA0069` — Non-constant static fields should not be visible.

*Port of Meziantou.Analyzer MA0069 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0069.md)

A visible (`public` / `protected`) `static` mutable field is global shared state any caller can reassign — make it `const`, `static readonly`, or a property. Fires per declarator on a visible static field that is neither `const` nor `readonly` (the Meziantou counterpart of CA2211). Native port of Meziantou.Analyzer MA0069 — purely syntactic, report-only.

### `MA0070` — Obsolete attributes should include explanations.

*Port of Meziantou.Analyzer MA0070 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0070.md)

An `[Obsolete]` attribute should pass a message explaining the deprecation. Fires when `ObsoleteAttribute` is applied with no positional argument — no arguments or only named arguments. (Unlike CA1041, an empty `""` message satisfies MA0070.) Native port of Meziantou MA0070 — purely syntactic, report-only.

### `MA0071` — Avoid using a redundant else.

*Port of Meziantou.Analyzer MA0071 · Redundancy · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0071.md)

When an `if` branch always exits (ends in `return`/`throw`/`break`/`continue`/`goto`), the following `else` is redundant — its body can simply follow the `if`. Fires at the `else` keyword (the Meziantou counterpart of Roslynator RCS1211, at the same locations). Native port of Meziantou.Analyzer MA0071 — report-only (unwrapping the else is a structural rewrite).

### `MA0073` — Avoid comparison with bool constant.

*Port of Meziantou.Analyzer MA0073 · Redundancy · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0073.md)

`x == true` / `x != false` is noise — use `x` (or `!x`) directly. Fires on an `==`/`!=` comparison where one operand is a boolean literal, anchored at the operator. Native port of Meziantou.Analyzer MA0073 — report-only.

### `MA0077` — A class that provides Equals(T) should implement IEquatable<T>.

*Port of Meziantou.Analyzer MA0077 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0077.md)

A class or struct with a strongly-typed, self-referential `Equals(Self)` method should declare `IEquatable<Self>` so generic code uses it, anchored at the type name. Shares S3897's predicate but covers structs too and fires once at the cross-file anchor rather than per part. Native port of Meziantou.Analyzer MA0077 — report-only.

### `MA0089` — string.Join with a single-char string separator.

*Port of Meziantou.Analyzer MA0089 · Performance · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0089.md)

`string.Join(",", …)` passes a single-character string where a `char` overload exists; `string.Join(',', …)` avoids an allocation. Fires on a `string.Join` call whose first argument is a single-character string literal, anchored at that literal. Native port of Meziantou MA0089 (static-`string.Join` subset) — report-only.

### `MA0090` — Remove an empty finally/else block.

*Port of Meziantou.Analyzer MA0090 · Redundancy · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0090.md)

An empty `finally` or `else` clause is dead weight. Fires on such a clause, anchored at the `finally`/`else` keyword. The Meziantou twin of RCS1259's finally/else cases. Native port of Meziantou.Analyzer MA0090 — report-only.

### `MA0095` — A class that implements IEquatable<T> should override Equals(object).

*Port of Meziantou.Analyzer MA0095 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0095.md)

A type implementing `IEquatable<T>` should also override `Object.Equals(object)` so the two equality paths agree. The Meziantou twin of CA1067, anchored at the type name. Native port of Meziantou.Analyzer MA0095 — report-only.

### `MA0096` — A class implementing IComparable<T> should override comparison operators.

*Port of Meziantou.Analyzer MA0096 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0096.md)

The Meziantou twin of CA1036: a type implementing `IComparable<T>` that defines none of the comparison operators is flagged at its name. Shares CA1036's detector. Native port of Meziantou.Analyzer MA0096 — report-only.

### `MA0097` — A class implementing IComparable<T> should also implement IComparable.

*Port of Meziantou.Analyzer MA0097 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0097.md)

The Meziantou twin of RCS1241: a type naming `IComparable<...>` in its base list but not the non-generic `IComparable` is flagged at its name. Shares RCS1241's detector. Native port of Meziantou.Analyzer MA0097 — report-only.

### `MA0099` — A bare 0 should be an explicit enum member.

*Port of Meziantou.Analyzer MA0099 · Style · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0099.md)

A bare integer literal `0` used where an enum value is expected relies on the implicit `0`-to-enum conversion instead of naming the intended member. Fires on a declaration initializer, a `==`/`!=` comparison operand, or a call argument that is `0` where the enum-typed position is declared in the same file, anchored at the literal. Native port of Meziantou.Analyzer MA0099 — report-only (no safe rewrite without knowing which member means zero).

### `MA0101` — String contains an implicit end of line character.

*Port of Meziantou.Analyzer MA0101 · Correctness · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0101.md)

A multi-line verbatim string (`@"…"`) embeds a raw newline whose representation (CRLF vs LF) depends on the file encoding, making the value environment-dependent. Fires on such a string, anchored at the `@`. Native port of Meziantou.Analyzer MA0101 (report-only — the rewrite changes the value representation).

### `MA0102` — Make member readonly.

*Port of Meziantou.Analyzer MA0102 · Performance · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0102.md)

A `struct` instance method that does not mutate the instance can be marked `readonly`, letting the compiler skip defensive copies. Fires on a non-static, non-`readonly` method of a `struct`/`record struct` whose body performs no assignment or increment, anchored at the method name. Native port of Meziantou.Analyzer MA0102 — report-only.

### `MA0136` — Raw string literals should not use an implicit end of line.

*Port of Meziantou.Analyzer MA0136 · Style · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0136.md)

A multi-line raw string literal (`"""` … `"""`) embeds the source file's line endings, so its runtime value changes with the file's line-ending style. Fires on a raw string that spans more than one line, anchored at its opening quotes; a single-line raw string is left alone. Native port of Meziantou.Analyzer MA0136 — report-only.

### `MA0140` — Both if and else branch have identical code.

*Port of Meziantou.Analyzer MA0140 · Correctness · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0140.md)

When every branch of an `if`/`else` chain — or both sides of a `?:` — does exactly the same thing, the condition is pointless and usually a bug. Fires on a ternary with identical branches and on an `if`/`else` chain with a final `else` whose branches are all identical (the Meziantou counterpart of Sonar S3923). Native port of Meziantou.Analyzer MA0140 — purely syntactic, report-only.

### `MA0143` — A primary constructor parameter is reassigned in the type body.

*Port of Meziantou.Analyzer MA0143 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0143.md)

A primary-constructor parameter is implicitly captured; reassigning it inside the type body mutates what looks like a constructor argument, which is surprising. Fires when a primary-ctor parameter's name is written anywhere in the type body (assignment, `++`/`--`, `ref`/`out` argument), anchored at the parameter name. Native port of Meziantou.Analyzer MA0143 — report-only. Same-named local/field shadowing is a documented blind spot (no scope model).

### `MA0159` — Use 'Order' instead of 'OrderBy'.

*Port of Meziantou.Analyzer MA0159 · Performance · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0159.md)

Sorting by the element itself is the identity ordering, which `Order()` / `OrderDescending()` express directly. Fires on `seq.OrderBy(x => x)` / `seq.OrderByDescending(x => x)` (anchored at the method name) and on a query `orderby x` whose key is the range variable (anchored at the ordering). Native port of Meziantou.Analyzer MA0159 — report-only.

### `MA0182` — Detected internal type that is never used.

*Port of Meziantou.Analyzer MA0182 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0182.md)

A type not visible outside its assembly (internal/private/file, or a nested type reachable only within the assembly) that is referenced nowhere in the project is dead code and can be removed. Native port of Meziantou.Analyzer MA0182, consuming the project-wide declaration index (issue #129): it fires only when the whole-project scan is clean and the type name is absent from every use-position; any uncertainty (unresolved scan, `[InternalsVisibleTo]`) keeps it silent. Report-only.

### `MA0184` — Do not use an interpolated string without parameters.

*Port of Meziantou.Analyzer MA0184 · Redundancy · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0184.md)

An interpolated string with no `{…}` holes (`$"x"` / `$@"x"`) collapses to a plain string, so the `$` prefix is pointless. Fires at the `$`. The Meziantou twin of Roslynator RCS1214 — identical detection and anchor, verified against the oracle (same corpus site). Raw interpolated strings (`$$"…"`) are left alone. Native port of Meziantou.Analyzer MA0184 — report-only.

### `MA0192` — Use HasFlag instead of bitwise checks.

*Port of Meziantou.Analyzer MA0192 · Style · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0192.md)

The enum idiom `(x & Flag) == Flag` is more clearly written `x.HasFlag(Flag)`. Fires on an `==`/`!=` comparison where one side is a bitwise-`&` on an enum (resolved in-file, the same way as RCS1130) and the other side is a matching constant flag (`EnumType.Member`) or `0`. Unlike RCS1130 it does not require `[Flags]`. `|`/`^`, relational operators, variable flags and non-constant targets are left alone. Native port of Meziantou.Analyzer MA0192 — report-only (the `HasFlag` rewrite is not applied).

### `MA0196` — Do not use '<inheritdoc />' on members that do not inherit documentation.

*Port of Meziantou.Analyzer MA0196 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0196.md)

A bare `<inheritdoc/>` copies documentation from a base type or interface member; on a member that inherits nothing it documents nothing. This conservative port fires at the `<inheritdoc>` tag when the member has a bare `<inheritdoc/>` (no `cref`), is not `override`, and its enclosing type has no base list. Members in a type that has a base list need semantic signature matching and are left alone. Native port of Meziantou.Analyzer MA0196 — report-only.

### `MA0203` — Do not use a return tag for a void method.

*Port of Meziantou.Analyzer MA0203 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0203.md)

A `void` method has nothing to return, so a `<returns>` XML-doc tag is meaningless. Fires on a `void` method whose doc comment carries a `<returns>` element, anchored at the tag. The Meziantou twin of StyleCop SA1617 — identical detection and anchor, verified against the oracle (only a literal `void` return type fires; `Task`/`ValueTask` are not treated as void-like). Native port of Meziantou.Analyzer MA0203 — report-only.

### `MA0204` — Remove an unnecessary partial modifier.

*Port of Meziantou.Analyzer MA0204 · Redundancy · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0204.md)

A type marked `partial` with only a single declaration has no second part to merge with, so the modifier is unnecessary. Fires at the `partial` modifier. The Meziantou twin of Roslynator RCS1043 — identical detection and anchor, verified against the oracle (same corpus site). `partial` is meaningful across files, which a single-file parse cannot see; single-part partial *methods* are out of scope (type-only). Native port of Meziantou.Analyzer MA0204 — report-only.

### `MA0206` — Remove unnecessary braces from an empty-body type declaration.

*Port of Meziantou.Analyzer MA0206 · Redundancy · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0206.md)

A `class`, `struct`, or `record` declared with an empty body (`{ }` containing no members) carries braces that add nothing. Fires at the opening `{`. A narrower sibling of Roslynator RCS1251 — same detection and anchor, but silent on `interface`, `record struct`, and `enum` (verified against the oracle). Report-only — the fix (`{ }` → `;`) is invalid for a plain `class`, so it is not applied universally. Native port of Meziantou.Analyzer MA0206.

### `MA0211` — Use multi-line syntax for XML summary comments.

*Port of Meziantou.Analyzer MA0211 · Style · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0211.md)

A single-line `/// <summary>content</summary>` documentation comment with non-empty content should be expanded to the canonical multi-line form. Fires at the `<summary>` open tag. An empty or whitespace-only summary, an already-multi-line summary, every non-`<summary>` element (`<remarks>`, `<returns>`, …), and a doc on a field or field-like event declaration (which the real analyzer never sees) are exempt; a nested element (`<see cref="…"/>`) counts as content. Native port of Meziantou.Analyzer MA0211 — report-only.

### `NUnit1002` — The TestCaseSource should use the nameof operator to specify its target.

*Port of NUnit.Analyzers NUnit1002 · Maintainability · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit1002.md)

Referencing a `TestCaseSource` member by a string literal breaks silently on rename, whereas `nameof(Member)` is compiler-checked. Fires when the source-name argument is a plain string literal AND a matching member (method/property/field) exists on the target type — resolved via the project declaration index (issue #152); an unresolvable target, a private base member, or an explicit interface implementation are all treated as "not found" (matching the oracle's own missing-member id, which this port does not emit). Anchored at the literal. Native port of NUnit.Analyzers NUnit1002 — report-only.

### `NUnit1021` — The ValueSource should use the nameof operator to specify its target.

*Port of NUnit.Analyzers NUnit1021 · Maintainability · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit1021.md)

Referencing a `ValueSource` member by a string literal breaks silently on rename, whereas `nameof(Member)` is compiler-checked. Fires when the source-name argument is a plain string literal AND a matching member (method/property/field) exists on the target type — resolved via the project declaration index (issue #152); an unresolvable target, a private base member, or an explicit interface implementation are all treated as "not found" (matching the oracle's own missing-member id, which this port does not emit). Anchored at the literal. Native port of NUnit.Analyzers NUnit1021 — report-only.

### `NUnit2001` — Use Assert.That with Is.False instead of ClassicAssert.False.

*Port of NUnit.Analyzers NUnit2001 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2001.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.False(expr)` call, anchored at the invocation — prefer `Assert.That(expr, Is.False)`. Native port of NUnit.Analyzers NUnit2001 — report-only.

### `NUnit2002` — Use Assert.That with Is.False instead of ClassicAssert.IsFalse.

*Port of NUnit.Analyzers NUnit2002 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2002.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.IsFalse(expr)` call, anchored at the invocation — prefer `Assert.That(expr, Is.False)`. Native port of NUnit.Analyzers NUnit2002 — report-only.

### `NUnit2003` — Use Assert.That with Is.True instead of ClassicAssert.IsTrue.

*Port of NUnit.Analyzers NUnit2003 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2003.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.IsTrue(expr)` call, anchored at the invocation — prefer `Assert.That(expr, Is.True)`. Native port of NUnit.Analyzers NUnit2003 — report-only.

### `NUnit2004` — Use Assert.That with Is.True instead of ClassicAssert.True.

*Port of NUnit.Analyzers NUnit2004 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2004.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.True(expr)` call, anchored at the invocation — prefer `Assert.That(expr, Is.True)`. Native port of NUnit.Analyzers NUnit2004 — report-only.

### `NUnit2005` — Use Assert.That with Is.EqualTo instead of ClassicAssert.AreEqual.

*Port of NUnit.Analyzers NUnit2005 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2005.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.AreEqual(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.EqualTo)`. Native port of NUnit.Analyzers NUnit2005 — report-only.

### `NUnit2006` — Use Assert.That with Is.Not.EqualTo instead of ClassicAssert.AreNotEqual.

*Port of NUnit.Analyzers NUnit2006 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2006.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.AreNotEqual(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.Not.EqualTo)`. Native port of NUnit.Analyzers NUnit2006 — report-only.

### `NUnit2009` — The actual and the expected argument are the same in an assertion.

*Port of NUnit.Analyzers NUnit2009 · Correctness · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2009.md)

Fires on an `Assert.That(actual, ...)` whose constraint's expected value (`Is.EqualTo`, `Is.GreaterThan`, `Is.SameAs`, …) is the same expression as the actual argument — the assertion can never fail meaningfully. Native port of NUnit.Analyzers NUnit2009, anchored at the expected expression. Report-only.

### `NUnit2015` — Use Assert.That with Is.SameAs instead of ClassicAssert.AreSame.

*Port of NUnit.Analyzers NUnit2015 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2015.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.AreSame(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.SameAs)`. Native port of NUnit.Analyzers NUnit2015 — report-only.

### `NUnit2016` — Use Assert.That with Is.Null instead of ClassicAssert.Null.

*Port of NUnit.Analyzers NUnit2016 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2016.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.Null(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.Null)`. Native port of NUnit.Analyzers NUnit2016 — report-only.

### `NUnit2017` — Use Assert.That with Is.Null instead of ClassicAssert.IsNull.

*Port of NUnit.Analyzers NUnit2017 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2017.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.IsNull(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.Null)`. Native port of NUnit.Analyzers NUnit2017 — report-only.

### `NUnit2018` — Use Assert.That with Is.Not.Null instead of ClassicAssert.NotNull.

*Port of NUnit.Analyzers NUnit2018 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2018.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.NotNull(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.Not.Null)`. Native port of NUnit.Analyzers NUnit2018 — report-only.

### `NUnit2019` — Use Assert.That with Is.Not.Null instead of ClassicAssert.IsNotNull.

*Port of NUnit.Analyzers NUnit2019 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2019.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.IsNotNull(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.Not.Null)`. Native port of NUnit.Analyzers NUnit2019 — report-only.

### `NUnit2027` — Use Assert.That with Is.GreaterThan instead of ClassicAssert.Greater.

*Port of NUnit.Analyzers NUnit2027 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2027.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.Greater(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.GreaterThan)`. Native port of NUnit.Analyzers NUnit2027 — report-only.

### `NUnit2028` — Use Assert.That with Is.GreaterThanOrEqualTo instead of ClassicAssert.GreaterOrEqual.

*Port of NUnit.Analyzers NUnit2028 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2028.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.GreaterOrEqual(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.GreaterThanOrEqualTo)`. Native port of NUnit.Analyzers NUnit2028 — report-only.

### `NUnit2029` — Use Assert.That with Is.LessThan instead of ClassicAssert.Less.

*Port of NUnit.Analyzers NUnit2029 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2029.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.Less(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.LessThan)`. Native port of NUnit.Analyzers NUnit2029 — report-only.

### `NUnit2030` — Use Assert.That with Is.LessThanOrEqualTo instead of ClassicAssert.LessOrEqual.

*Port of NUnit.Analyzers NUnit2030 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2030.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.LessOrEqual(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.LessThanOrEqualTo)`. Native port of NUnit.Analyzers NUnit2030 — report-only.

### `NUnit2031` — Use Assert.That with Is.Not.SameAs instead of ClassicAssert.AreNotSame.

*Port of NUnit.Analyzers NUnit2031 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2031.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.AreNotSame(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.Not.SameAs)`. Native port of NUnit.Analyzers NUnit2031 — report-only.

### `NUnit2032` — Use Assert.That with Is.Zero instead of ClassicAssert.Zero.

*Port of NUnit.Analyzers NUnit2032 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2032.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.Zero(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.Zero)`. Native port of NUnit.Analyzers NUnit2032 — report-only.

### `NUnit2033` — Use Assert.That with Is.Not.Zero instead of ClassicAssert.NotZero.

*Port of NUnit.Analyzers NUnit2033 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2033.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.NotZero(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.Not.Zero)`. Native port of NUnit.Analyzers NUnit2033 — report-only.

### `NUnit2034` — Use Assert.That with Is.NaN instead of ClassicAssert.IsNaN.

*Port of NUnit.Analyzers NUnit2034 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2034.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.IsNaN(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.NaN)`. Native port of NUnit.Analyzers NUnit2034 — report-only.

### `NUnit2035` — Use Assert.That with Is.Empty instead of ClassicAssert.IsEmpty.

*Port of NUnit.Analyzers NUnit2035 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2035.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.IsEmpty(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.Empty)`. Native port of NUnit.Analyzers NUnit2035 — report-only.

### `NUnit2036` — Use Assert.That with Is.Not.Empty instead of ClassicAssert.IsNotEmpty.

*Port of NUnit.Analyzers NUnit2036 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2036.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.IsNotEmpty(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.Not.Empty)`. Native port of NUnit.Analyzers NUnit2036 — report-only.

### `NUnit2037` — Use Assert.That with Does.Contain instead of ClassicAssert.Contains.

*Port of NUnit.Analyzers NUnit2037 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2037.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.Contains(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Does.Contain)`. Native port of NUnit.Analyzers NUnit2037 — report-only.

### `NUnit2038` — Use Assert.That with Is.InstanceOf instead of ClassicAssert.IsInstanceOf.

*Port of NUnit.Analyzers NUnit2038 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2038.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.IsInstanceOf(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.InstanceOf)`. Native port of NUnit.Analyzers NUnit2038 — report-only.

### `NUnit2039` — Use Assert.That with Is.Not.InstanceOf instead of ClassicAssert.IsNotInstanceOf.

*Port of NUnit.Analyzers NUnit2039 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2039.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.IsNotInstanceOf(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.Not.InstanceOf)`. Native port of NUnit.Analyzers NUnit2039 — report-only.

### `NUnit2048` — Use Assert.That instead of StringAssert.

*Port of NUnit.Analyzers NUnit2048 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2048.md)

The legacy `StringAssert` class is superseded by the constraint model. Fires on any `StringAssert.<method>(…)` call, anchored at the invocation — prefer `Assert.That(actual, <constraint>)`. Native port of NUnit.Analyzers NUnit2048 — report-only.

### `NUnit2049` — Use Assert.That instead of CollectionAssert.

*Port of NUnit.Analyzers NUnit2049 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2049.md)

The legacy `CollectionAssert` class is superseded by the constraint model. Fires on any `CollectionAssert.<method>(…)` call, anchored at the invocation — prefer `Assert.That(actual, <constraint>)`. Native port of NUnit.Analyzers NUnit2049 — report-only.

### `NUnit2051` — Use Assert.That with Is.Positive instead of ClassicAssert.Positive.

*Port of NUnit.Analyzers NUnit2051 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2051.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.Positive(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.Positive)`. Native port of NUnit.Analyzers NUnit2051 — report-only.

### `NUnit2052` — Use Assert.That with Is.Negative instead of ClassicAssert.Negative.

*Port of NUnit.Analyzers NUnit2052 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2052.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.Negative(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.Negative)`. Native port of NUnit.Analyzers NUnit2052 — report-only.

### `NUnit2053` — Use Assert.That with Is.AssignableFrom instead of ClassicAssert.IsAssignableFrom.

*Port of NUnit.Analyzers NUnit2053 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2053.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.IsAssignableFrom(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.AssignableFrom)`. Native port of NUnit.Analyzers NUnit2053 — report-only.

### `NUnit2054` — Use Assert.That with Is.Not.AssignableFrom instead of ClassicAssert.IsNotAssignableFrom.

*Port of NUnit.Analyzers NUnit2054 · Style · report-only* · [upstream docs](https://github.com/nunit/nunit.analyzers/blob/master/documentation/NUnit2054.md)

NUnit's classic assertion model is superseded by the constraint model. Fires on a `ClassicAssert.IsNotAssignableFrom(...)` call, anchored at the invocation — prefer the constraint model `Assert.That(..., Is.Not.AssignableFrom)`. Native port of NUnit.Analyzers NUnit2054 — report-only.

### `PH2021` — Do not inline `new T()` calls into a member access.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2021 · Style · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2021.md)

`new Foo().Bar()` (directly or parenthesized, `(new Foo()).Bar()`) inlines a temporary instance into a member access, making it hard to inspect/reuse. Fires on the `object_creation_expression`, anchored there; exempt when the accessed member is `ToString`/`ToList`/`ToArray`/`AsSpan`. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2021 (`AvoidInlineNewAnalyzer`) — report-only.

### `PH2026` — Avoid `[SuppressMessage]`.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2026 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2026.md)

`[SuppressMessage]` silences a specific diagnostic on a specific member, hiding it from CI without a repo-wide baseline. Fires once per `SuppressMessage`/`SuppressMessageAttribute` attribute, anchored at the attribute's own start. The real analyzer supports an `AdditionalFiles`-based whitelist that is out of scope here, so every occurrence is flagged. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2026 — report-only.

### `PH2029` — Avoid `#pragma warning` directives.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2029 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2029.md)

A `#pragma warning disable`/`restore` directive silences diagnostics file-locally, hiding real issues from CI. Fires anchored at the directive's own start; `#pragma checksum` is excluded (a different pragma form). Self-exempt: a directive whose codes CONTAIN the substring `PH2029` (a substring check, matching upstream, not exact-match) does not fire — so it can silence itself. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2029 (`AvoidPragmaAnalyzer`) — report-only.

### `PH2032` — Avoid an empty static constructor.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2032 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2032.md)

A `static` constructor with an empty block body does nothing and should be removed. Fires anchored at the whole constructor declaration. Expression-bodied constructors and non-static empty constructors are exempt; complementary to (not a duplicate of) the shipped PH2097, whose empty-block family deliberately exempts constructor bodies. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2032 (`AvoidEmptyTypeInitializerAnalyzer`) — report-only.

### `PH2044` — Avoid the dynamic keyword.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2044 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2044.md)

`dynamic` is not checked for type safety at compile time. Fires on `dynamic` used as a method return type, a method parameter type, a property type, or a field/local variable's declared type or initializer, anchored at the declaring type (a method's parameter-list use is anchored at the parameter list itself). Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2044 — report-only.

### `PH2064` — Avoid duplicate #region names.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2064 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2064.md)

A type cannot have two regions with the same name. Fires on a `#region NAME` whose name repeats an earlier one in the same class/struct (including nested types), anchored at the `region` keyword; an unlabeled `#region` is never tracked. A malformed region nesting (odd count, or a pair out of order) skips the whole type. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2064 — report-only.

### `PH2068` — Avoid goto.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2068 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2068.md)

`goto` (and `goto case`/`goto default`) jumps obscure control flow. Fires on the `goto` statement itself AND on its target label — unlike SonarAnalyzer's S907 twin, which flags only the `goto` keyword. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2068 — report-only.

### `PH2070` — Avoid `protected` fields.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2070 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2070.md)

A `protected` field breaks encapsulation — a derived type can mutate it directly, bypassing any accessor logic. Fires on any field declaration carrying the `protected` modifier (`protected internal` fires too — matched by token, not compound accessibility), anchored at the whole field declaration. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2070 (`NoProtectedFieldsAnalyzer`) — report-only.

### `PH2077` — Avoid a switch statement with only a default case.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2077 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2077.md)

A `switch` whose only section is `default` always runs the same code and can be replaced by the clause's statements directly. Fires when a `switch` statement has exactly one section that is a `default` clause (anchored at the section), or a `switch` expression with exactly one arm whose pattern is a bare discard `_` (anchored at the arm). A switch with any `case` label alongside `default` is exempt. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2077 — report-only.

### `PH2080` — Avoid hardcoded absolute Windows paths in string literals.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2080 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2080.md)

A string literal whose (unescaped) value looks like an absolute Windows path — a drive letter (`C:\...`) or a UNC share (`\\server\...`) — hardcodes a machine-specific location. Anchored at the string literal. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2080 (`NoHardCodedPathsAnalyzer`) — report-only.

### `PH2081` — Avoid #region within a method body.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2081 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2081.md)

A `#region`/`#endregion` directive appearing anywhere inside a method's span reports EACH directive separately — a region pair inside a method fires twice (once at `#region`, once at `#endregion`). Constructors and properties are not covered (a different syntax kind upstream). Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2081 (`NoRegionsInMethodAnalyzer`) — report-only.

### `PH2082` — Use positive wording for variable and property names.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2082 · Style · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2082.md)

A negatively-worded flag (`isDisabled`, `missingValue`, …) usually forces double-negative reads at every call site. Fires when a variable declarator's or property's identifier CONTAINS (case-insensitively) `disable`, `ignore`, `missing`, or `absent`; anchored at the declarator or the whole property. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2082 — report-only.

### `PH2084` — Don't lock on a newly-created object.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2084 · Concurrency · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2084.md)

A lock object must be shareable between threads; locking on a freshly-created object is always a no-op mutex. Fires when the `lock` expression is an object-creation expression (`lock (new object())`) or an invocation on one (`lock (new object().ToString())`), anchored at the locked expression. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2084 — report-only.

### `PH2085` — Property accessors should be ordered get then set/init.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2085 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2085.md)

A property's `set`/`init` accessor physically preceding its `get` accessor reads backwards. Fires when the last `set`/`init` accessor's index is less than the last `get` accessor's index, anchored at the whole accessor list. A get-only or set/init-only property never fires. Twin of the native SA1212 ("get before set") with a distinct id. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2085 (`OrderPropertyAccessorsAnalyzer`) — report-only.

### `PH2089` — Avoid assignment in a condition.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2089 · Correctness · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2089.md)

A plain `=` in an `if`/ternary condition is a classic typo for `==`. Fires when the condition contains a bare (non-compound) assignment, anchored at the condition's start; a condition that also contains an anonymous-object creation (`new { X = 1 }`) is exempt wholesale, matching the analyzer's own token-based check. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2089 — report-only.

### `PH2092` — Limit the number of `&&`/`||` clauses in a condition.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2092 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2092.md)

An `if`/ternary condition built from many `&&`/`||` operators is hard to reason about. Fires when the count of `&&`/`||` tokens anywhere within the condition is at least 4 (the observed upstream default, configurable via `max_operators` there but hardcoded here). Anchored at the condition; message embeds both the actual count and the threshold. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2092 (`LimitConditionComplexityAnalyzer`) — report-only.

### `PH2093` — Prefer named-field tuples.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2093 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2093.md)

An unnamed value-tuple element (`(int, string)`) is harder to read at call sites than a named one. Fires once per unnamed `tuple_element`, anchored at the element. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2093 (`PreferNamedTuplesAnalyzer`) — report-only.

### `PH2096` — Prefer async Task methods over async void methods.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2096 · Concurrency · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2096.md)

An `async void` method cannot be awaited and an unhandled exception crashes the process. Fires on an `async` method whose return type is `void`, anchored at `void`; a parameter typed (or subtyped) `EventArgs` is exempt (the conventional event-handler shape). Fourth member of this codebase's VSTHRD100/S3168/AsyncFixer03 twin family — method-declaration scope only (an async lambda's converted return type needs a type model, out of scope). Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2096 — report-only.

### `PH2097` — Avoid empty statement blocks.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2097 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2097.md)

An empty `{ }` block is usually a mistake or dead code. Fires on any empty block EXCEPT a constructor body, a public/internal/protected member body, a catch body (PH2098's concern), a lambda/anonymous-method body, or a lock body — trivia-blind (a comment-only block still fires), anchored at the `{`. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2097 — report-only.

### `PH2098` — Avoid empty catch blocks.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2098 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2098.md)

An empty `catch` body silently swallows an exception. Fires on ANY empty catch body — bare, typed, or comment-only (trivia-blind, unlike this codebase's S2486 twin) — anchored at the catch body's `{`. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2098 — report-only.

### `PH2104` — Put every LINQ query-expression clause on a separate line.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2104 · Style · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2104.md)

A C# query-expression (`from … where … select …`) with multiple clauses crammed onto one line is hard to scan. Fires on the `from` clause and every `let`/`join`/`orderby`/`where` clause not followed by a newline before the next clause; the final `select`/`group` clause is never checked (it is the query body's terminal `SelectOrGroup`, not a `Clauses` entry), and a `group … into` continuation's own clauses are out of scope (unreachable by the upstream analyzer's own single, non-recursive registration). Anchored at the clause. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2104 (`EveryLinqStatementOnSeparateLineAnalyzer`) — report-only.

### `PH2105` — Overload `-` when you overload `+` (and vice versa).

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2105 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2105.md)

`+` and `-` are conventionally overloaded together. Fires once per type whose `operator +` and `operator -` DECLARATION counts differ (usages don't count), anchored at the type's identifier. Counts include nested types' own operators (an unrestricted subtree walk, matching the real source). Upstream ships this rule DEFAULT-OFF — ported anyway per this codebase's opt-out convention. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2105 (`AlignOperatorsCountAnalyzer`) — report-only.

### `PH2106` — Overload `/` when you overload `*` (and vice versa).

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2106 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2106.md)

`*` and `/` are conventionally overloaded together. Fires once per type whose `operator *` and `operator /` DECLARATION counts differ, anchored at the type's identifier. See PH2105's module docs for the shared analyzer's full detail. Upstream ships this rule DEFAULT-OFF — ported anyway per this codebase's opt-out convention. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2106 (`AlignOperatorsCountAnalyzer`) — report-only.

### `PH2107` — Overload `<` when you overload `>` (and vice versa).

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2107 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2107.md)

`>` and `<` are conventionally overloaded together. Fires once per type whose `operator >` and `operator <` DECLARATION counts differ, anchored at the type's identifier — in practice this mostly triggers on `partial` types whose pairs are split across declarations, since C# enforces </> pairing per signature within a single merged type (CS0216). See PH2105's module docs for the shared analyzer's full detail. Upstream ships this rule DEFAULT-OFF — ported anyway per this codebase's opt-out convention. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2107 (`AlignOperatorsCountAnalyzer`) — report-only.

### `PH2108` — Overload `<=` when you overload `>=` (and vice versa).

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2108 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2108.md)

`>=` and `<=` are conventionally overloaded together. Fires once per type whose `operator >=` and `operator <=` DECLARATION counts differ, anchored at the type's identifier — as with PH2107, C# enforces >=/<= pairing per signature within a single merged type (CS0216), so this mostly triggers on `partial` types whose pairs are split across declarations. Upstream ships this rule DEFAULT-OFF — ported anyway per this codebase's opt-out convention. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2108 (`AlignOperatorsCountAnalyzer`) — report-only.

### `PH2109` — Overload `<<` when you overload `>>` (and vice versa).

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2109 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2109.md)

`>>` and `<<` are conventionally overloaded together. Fires once per type whose `operator >>` and `operator <<` DECLARATION counts differ, anchored at the type's identifier. Upstream ships this rule DEFAULT-OFF — ported anyway per this codebase's opt-out convention. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2109 (`AlignOperatorsCountAnalyzer`) — report-only.

### `PH2110` — Overload `--` when you overload `++` (and vice versa).

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2110 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2110.md)

`++` and `--` are conventionally overloaded together. Fires once per type whose `operator ++` and `operator --` DECLARATION counts differ, anchored at the type's identifier. Upstream ships this rule DEFAULT-OFF — ported anyway per this codebase's opt-out convention. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2110 (`AlignOperatorsCountAnalyzer`) — report-only.

### `PH2111` — Reduce a method's cognitive load (nested blocks + logical/control tokens).

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2111 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2111.md)

A deeply block-nested method combined with many `||`/`&&`/`!`/`!=`/`break`/`continue` tokens is hard to hold in your head. The metric is a literal, unmemoized recursive block count (each nested block's load is fully recomputed by every enclosing block, so it grows combinatorially with nesting depth) plus a flat token count over the method's first block. Fires when the total exceeds 25; anchored at the method's name. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2111 (`ReduceCognitiveLoadAnalyzer`) — report-only.

### `PH2113` — Merge a nested `if` into its outer `if`.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2113 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2113.md)

An `if` that is the sole statement of an outer `if`'s body (neither carries an `else`) can be merged with `&&` to reduce nesting. Exempt if either condition already contains `||` (merging would change short-circuit grouping). Anchored at the inner `if` keyword. Twin of this codebase's S1066/RCS1061 (same shape, different id). Upstream ships this rule DEFAULT-OFF — ported anyway per this codebase's opt-out convention. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2113 — report-only.

### `PH2114` — Avoid empty statements.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2114 · Redundancy · has an autofix* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2114.md)

A standalone `;` is a no-op that usually slipped in by accident. Fires on an empty statement, anchored at the `;`; the fix deletes it. Twin of this codebase's SA1106/S1116/MA0037 (same shape, different id). Upstream ships this rule DEFAULT-OFF (`isEnabledByDefault: false` in the analyzer source, despite the package's own docs page claiming "Yes") — ported anyway per this codebase's opt-out convention. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2114.

### `PH2115` — Avoid putting multiple lambda expressions on the same line.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2115 · Style · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2115.md)

Two or more lambda expressions on the same source line, within a method that has at least 2 lambdas total, are hard to scan and to set breakpoints on. Every lambda on that line except the left-most is flagged, anchored at the lambda; a lambda outside any method (e.g. a field initializer) is exempt. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2115 (`AvoidMultipleLambdasOnSingleLineAnalyzer`) — report-only.

### `PH2116` — Avoid `System.Collections.ArrayList`.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2116 · Performance · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2116.md)

`ArrayList` boxes value types and lacks compile-time element-type safety; use `List<T>` instead. Fires when a declared variable's type resolves (bare name + `using`, fully qualified, or aliased) to `System.Collections.ArrayList`, anchored at the type node; the message embeds the first declared variable's name. Upstream ships this rule DEFAULT-OFF — ported anyway per this codebase's opt-out convention. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2116 — report-only.

### `PH2122` — Avoid throwing exceptions from unexpected locations.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2122 · Correctness · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2122.md)

A `throw` inside `Equals`/`GetHashCode`/`Dispose`/`ToString`, an `*Exception`-derived type's constructor, a static constructor, a destructor, an `==`/`!=` operator, or an `implicit` conversion operator is easy to trigger unexpectedly (e.g. during hashing or equality checks) and hard to handle at the call site. Anchored at the `throw` keyword; a static constructor of an `*Exception`-named type can report twice (both the exception-constructor and static-constructor reasons independently). Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2122 (`AvoidThrowingUnexpectedExceptionsAnalyzer`) — report-only.

### `PH2123` — Pass a non-null sender/args to a raised event.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2123 · Correctness · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2123.md)

Raising an event with a literal `null` sender or args (`MyEvent(null, e)`) breaks handlers that assume a non-null instance-based sender. Fires on a bare-identifier invocation of the event (not `.Invoke(...)`/`?.Invoke(...)`) with exactly 2 arguments; each `null` argument position is flagged independently. Anchored at the `null` argument. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2123 (`PassSenderToEventHandlerAnalyzer`) — report-only.

### `PH2125` — Overload `==` when you overload `+` (or `-`).

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2125 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2125.md)

A type with a custom `+`/`-` overload usually wants a matching `==` too. Fires once per type where `operator +` or `operator -` is declared at all AND the `operator +` count differs from the `operator ==` count, anchored at the type's identifier. Upstream ships this rule DEFAULT-OFF — ported anyway per this codebase's opt-out convention. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2125 (`AlignOperatorsCountAnalyzer`) — report-only.

### `PH2127` — Avoid changing `for` loop variables inside the loop body.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2127 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2127.md)

Reassigning a `for` loop's own control variable inside its body produces surprising iteration counts; use `continue`/`break` instead. Fires on a plain `=` assignment (not a compound assignment) whose left side matches the nearest enclosing `for` statement's first declared loop variable, anchored at the left identifier. Upstream ships this rule DEFAULT-OFF — ported anyway per this codebase's opt-out convention. Native port of the `AvoidChangingLoopVariables` half of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2127 — report-only.

### `PH2128` — Split a multi-line `&&`/`||` condition with the operator at the end of the line.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2128 · Style · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2128.md)

A multi-line `if`/assignment/`return` condition built from `&&`/`||` reads best when each line break falls right AFTER the logical operator, not before it. Fires per offending token, computed once at the top-most `&&`/`||` node of the chain; anchored at the flagged token's own position, message embeds that line. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2128 (`SplitMultiLineConditionOnLogicalOperatorAnalyzer`) — report-only.

### `PH2130` — Avoid implementing a finalizer.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2130 · Correctness · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2130.md)

Finalizers add GC pressure and run at an unpredictable time; use `IDisposable.Dispose` instead. Fires on a destructor UNLESS its body is non-empty and every statement in it is a bare `Dispose(...)` call (`this.Dispose(...)` does not qualify — a real quirk of the upstream text-equality check). Anchored at the whole destructor. Upstream ships this rule DEFAULT-OFF — ported anyway per this codebase's opt-out convention. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2130 — report-only.

### `PH2132` — Remove commented-out code.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2132 · Style · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2132.md)

A single-line `//` comment whose trimmed text ends with `;` looks like commented-out code. Fires once per matched comment, except a run of consecutive matched lines only reports the first (a gap of more than 1 line resets it). Anchored at the comment; message embeds the 1-based line number. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2132 (`RemoveCommentedCodeAnalyzer`) — report-only.

### `PH2140` — Avoid the `[ExcludeFromCodeCoverage]` attribute.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2140 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2140.md)

Excluding code from coverage hides it from the metric meant to catch untested code. Fires once per attribute list containing an attribute whose de-aliased name CONTAINS `ExcludeFromCodeCoverage` (a substring match, so `ExcludeFromCodeCoverageAttribute` and any qualified/aliased spelling match too), anchored at the whole attribute list's own start (the `[`). Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2140 — report-only.

### `PH2141` — Avoid empty #regions.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2141 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2141.md)

A `#region` whose body is blank, or contains only non-copyright comments, adds noise with no value. Every `#region`/`#endregion` pair in the file is checked (not just those inside a class/struct); a region whose comment-only body contains both a copyright marker and a 4-digit year is exempt as a license header. Anchored at the `#region` directive's own start. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2141 (`AvoidEmptyRegionsAnalyzer`) — report-only.

### `PH2142` — Avoid declaring a conversion operator to `string`.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2142 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2142.md)

A user-defined conversion operator to the predefined `string` keyword type invites silent, surprising casts to string; prefer `ToString()` or an explicit serialization method. Only the predefined `string` keyword fires — a spelled-out `System.String` target does not. Anchored at the `operator` keyword. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2142 (`AvoidCastToStringAnalyzer`) — report-only.

### `PH2143` — Avoid `Assembly.GetEntryAssembly()`.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2143 · Correctness · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2143.md)

During testing, `Assembly.GetEntryAssembly()` points to the test runner, not your assembly — use `typeof(T).Assembly` instead. Fires on a `GetEntryAssembly()` call whose receiver resolves to `System.Reflection.Assembly` (bare name + `using`, or an alias — NOT a fully-qualified `System.Reflection.Assembly.GetEntryAssembly()` receiver, a confirmed quirk of the upstream resolver), anchored at the receiver expression. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2143 — report-only.

### `PH2144` — A backwards for-loop's boundary check should use >= 0.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2144 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2144.md)

A `for` loop decrementing a bare identifier (`i--`, `--i`, `i -= 1`, `i = i - 1`) whose condition is `i > 0` (or `0 < i`) silently skips index 0 — it should be `>= 0`. Anchored at the condition. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2144 (`AvoidIncorrectForLoopConditionAnalyzer`) — report-only.

### `PH2147` — Avoid a variable named exactly `_`.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2147 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2147.md)

A local, `for`/`using`/local-declaration variable, or `foreach` iteration variable named exactly `_` can be confused with a discard. Fires anchored at the whole declaration/foreach statement; a field named `_` and an `out`-argument discard (a genuine `DiscardDesignationSyntax`, never flagged upstream) are exempt. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2147 (`AvoidVariableNamedUnderscoreAnalyzer`) — report-only.

### `PH2153` — Avoid calling ToString() when the result is discarded.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2153 · Maintainability · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2153.md)

A zero-argument `.ToString()` call whose result is thrown away — a bare statement or an assignment to the discard variable `_` — did nothing useful. Fires at the `ToString` name token. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2153 (`AvoidUnusedToStringAnalyzer`) — report-only.

### `PH2159` — Avoid unnecessary empty parentheses in attributes.

*Port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2159 · Style · report-only* · [upstream docs](https://github.com/philips-software/roslyn-analyzers/blob/main/Documentation/Diagnostics/PH2159.md)

An attribute with a present-but-empty argument list (`[Foo()]`) carries no information the bare `[Foo]` doesn't already convey. Fires only when the argument list is present AND empty; a bare attribute or one with arguments is exempt. Anchored at the argument list. Native port of Philips.CodeAnalysis.MaintainabilityAnalyzers PH2159 (`AvoidUnnecessaryAttributeParenthesesAnalyzer`) — report-only.

### `RCS0001` — Add a blank line after an embedded statement.

*Port of Roslynator.Formatting.Analyzers RCS0001 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0001)

The embedded (non-block) statement of an `if`/`else`/loop/`using`/`lock`/`fixed` — already on its own line — that is directly followed by another statement with no blank line between them should be separated. Fires at the END of the embedded statement. `do` is exempt (the upstream analyzer never registers it); an `if` with an `else` defers its consequence to the dedicated else-clause check. Native port of Roslynator.Formatting.Analyzers RCS0001 — report-only (inserting the blank line is a reflow).

### `RCS0002` — Add a blank line after a #region directive.

*Port of Roslynator.Formatting.Analyzers RCS0002 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0002)

A `#region` directive not followed by a blank line should be separated from the code it introduces, anchored at the end of the `#region` line. Exempt when the region is empty (a `#region` directly followed by `#endregion`). Native port of Roslynator.Formatting.Analyzers RCS0002 — report-only (inserting the blank line is a reflow).

### `RCS0005` — Add a blank line before a #endregion directive.

*Port of Roslynator.Formatting.Analyzers RCS0005 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0005)

A `#endregion` directive not preceded by a blank line should be separated from the code above it, anchored at the START of the `#endregion` line. Exempt only when immediately preceded by an empty region's own `#region` (verified against the oracle) — a preceding `#pragma` does NOT exempt it. Native port of Roslynator.Formatting.Analyzers RCS0005 — report-only (inserting the blank line is a reflow).

### `RCS0006` — Add a blank line before the using directive list.

*Port of Roslynator.Formatting.Analyzers RCS0006 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0006)

The first `using` directive should be separated from whatever precedes it by a blank line. Exempt when nothing precedes it; when only comments precede it, fires only if at least one is a plain `//` line comment (mirrors RCS0049's top-comment concept). Anchored at the end of the preceding line. Native port of Roslynator.Formatting.Analyzers RCS0006 — report-only (inserting the blank line is a reflow).

### `RCS0007` — Add a blank line between accessors.

*Port of Roslynator.Formatting.Analyzers RCS0007 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0007)

Two accessors (get/set/init/add/remove) that both carry a body, with at least one spanning multiple lines, should be separated by a blank line when adjacent. Auto-accessors (`get; set;`) are exempt; a shape where BOTH accessors are single-line is RCS0011's territory (option-gated), not this rule's — the two never double-fire. Anchored at the end of the first accessor. Native port of Roslynator.Formatting.Analyzers RCS0007 — report-only (inserting the blank line is a reflow).

### `RCS0008` — Add a blank line between a closing brace and the next statement.

*Port of Roslynator.Formatting.Analyzers RCS0008 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0008)

A statement ending in `}` (a block, or an `if`/loop/`try`/`switch`/… whose last token is `}`) directly followed by another statement in the same block, with no blank line between, should have a blank line inserted. Fires at the position right after the `}` (the newline-insertion point). A `}` followed by `else`/`catch`/`finally` is exempt (they belong to the same statement). The fix inserts a blank line when the next statement is already on its own row; the rare same-row case is left report-only. Native port of Roslynator.Formatting.Analyzers RCS0008.

### `RCS0009` — Add a blank line between a declaration and the next declaration's documentation comment.

*Port of Roslynator.Formatting.Analyzers RCS0009 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0009)

A member declaration directly followed by a `///` documentation comment belonging to the next declaration, with no blank line between them, should be separated. Fires at the END of the upper declaration's line (the newline-insertion point) — unlike RCS0012, the upper declaration need NOT be single-line. Native port of Roslynator.Formatting.Analyzers RCS0009 — report-only (inserting the blank line is a reflow).

### `RCS0010` — Add a blank line between declarations.

*Port of Roslynator.Formatting.Analyzers RCS0010 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0010)

Two adjacent declarations where NOT BOTH are single-line (the complementary case to RCS0012, which requires both) should be separated by a blank line. Shares RCS0012/13/36's adjacency scan, node kinds, and end-of-line anchor; a doc-comment boundary is RCS0009's territory and is skipped here, and a block comment in the gap halts the scan for the rest of the container (the same quirk found for RCS0036). Native port of Roslynator.Formatting.Analyzers RCS0010 — report-only (inserting the blank line is a reflow).

### `RCS0012` — Add a blank line between single-line declarations.

*Port of Roslynator.Formatting.Analyzers RCS0012 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0012)

Two consecutive member declarations that are each single-line and sit on adjacent lines should be separated by a blank line. Fires once per boundary, anchored at the END of the upper declaration (the newline-insertion point). Fires between every adjacent single-line pair regardless of member kind (RCS0013 is the different-kind variant). The fix inserts a blank line when the pair already sits on separate lines; the rare same-line case needs a line split the fix does not attempt. Native port of Roslynator.Formatting.Analyzers RCS0012.

### `RCS0013` — Add a blank line between single-line declarations of a different kind.

*Port of Roslynator.Formatting.Analyzers RCS0013 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0013)

Two consecutive single-line declarations of a DIFFERENT kind (field vs method, method vs property, …) sitting on adjacent lines should be separated by a blank line. Exact subset of RCS0012's firing set, filtered to kind-change boundaries (a constructor followed by a static constructor is the SAME kind and does not fire this rule). Anchored at the END of the upper declaration (the newline-insertion point), same convention as RCS0012. The fix inserts a blank line for an already-adjacent-lines pair (the same-line case is left report-only). Native port of Roslynator.Formatting.Analyzers RCS0013.

### `RCS0016` — Put an attribute list on its own line.

*Port of Roslynator.Formatting.Analyzers RCS0016 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0016)

An attribute list sharing a line with the next attribute list, or with the declaration it decorates, should be put on its own line. Covers every declaration kind that can carry attribute lists, plus accessors and (when the enum itself is not single-line) enum members. Fires once per same-line boundary, anchored at the end of the earlier item. Native port of Roslynator.Formatting.Analyzers RCS0016 — report-only (the split is a reflow).

### `RCS0020` — Format an accessor's braces on multiple lines.

*Port of Roslynator.Formatting.Analyzers RCS0020 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0020)

An accessor (get/set/init/add/remove) whose block braces share a line should be reflowed to multiple lines. Fires once per single-line accessor block, anchored at the opening `{` — the accessor-block domain RCS0021 explicitly exempts. Each accessor in a multi-accessor list is checked independently. Expression-bodied and auto accessors have no block and are naturally exempt. Native port of Roslynator.Formatting.Analyzers RCS0020 — report-only (reflowing the braces is a rewrite).

### `RCS0021` — Format a block's braces on multiple lines.

*Port of Roslynator.Formatting.Analyzers RCS0021 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0021)

A block whose opening `{` and closing `}` share a line should be reflowed to multiple lines. Fires once per single-line block, anchored at the opening `{`; blocks owned by a lambda / anonymous method or a property accessor (RCS0020's domain) are exempt. Native port of Roslynator.Formatting.Analyzers RCS0021. Fixable (issue #150) for a flat, comment-free interior: moves `{` to its own line at the owning construct's indent, each interior statement to indent+1, and `}` back to the owning indent.

### `RCS0023` — Format a type declaration's braces on multiple lines.

*Port of Roslynator.Formatting.Analyzers RCS0023 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0023)

A class/struct/interface/record-struct declaration whose body braces share a line should be reflowed to multiple lines. Fires once per single-line body (including an empty `{ }`), anchored at the opening `{`. Plain `record`/`record class` are exempt regardless of body shape (verified against the oracle); `enum` is RCS0031's territory. Native port of Roslynator.Formatting.Analyzers RCS0023 — report-only (reflowing the braces is a rewrite).

### `RCS0024` — Add a new line after a switch label.

*Port of Roslynator.Formatting.Analyzers RCS0024 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0024)

A `case`/`default` label and the statement it guards should be on separate lines. Fires when a switch section's first statement begins on the same line as the label's colon, anchored at the position right after the colon (the newline-insertion point). Native port of Roslynator.Formatting.Analyzers RCS0024 — report-only (inserting the newline is a reflow).

### `RCS0025` — Put a full accessor on its own line.

*Port of Roslynator.Formatting.Analyzers RCS0025 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0025)

In an accessor list containing at least one full (bodied) accessor, each full accessor sharing a row with the boundary before it (the list's opening `{`, or the previous accessor's end) should move to its own line. Anchored at the START of the accessor. Orthogonal to, and confirmed co-firing with, RCS0020 (which flags the accessor's OWN brace layout instead). Native port of Roslynator.Formatting.Analyzers RCS0025 — report-only (the move is a reflow).

### `RCS0029` — Put a constructor initializer on its own line.

*Port of Roslynator.Formatting.Analyzers RCS0029 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0029)

A `: this(...)`/`: base(...)` constructor initializer that shares its line with the parameter list's closing `)` should move to its own line. Fires at the END of that `)` (the newline- insertion point) — the sibling of SA1128, which flags the same shape but anchors at the START of the `:` instead. Native port of Roslynator.Formatting.Analyzers RCS0029 — report-only (the move is a reflow).

### `RCS0030` — Put an embedded statement on its own line.

*Port of Roslynator.Formatting.Analyzers RCS0030 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0030)

The embedded statement of a control statement (`if`/`else`/loops/`using`/`lock`/`fixed`) that begins on the same line as the token introducing it should be on its own line. Fires at the position right after that token (the header's `)`, the `else`/`do` keyword). Fires on a braced block body too (unlike SA1503); an `else if` continuation is exempt. Native port of Roslynator.Formatting.Analyzers RCS0030. Fixable (issue #150) for the non-block shape only (inserts a newline plus the header's indent extended one level); a braced body is left to RCS0021's own reflow.

### `RCS0031` — Put each enum member on its own line.

*Port of Roslynator.Formatting.Analyzers RCS0031 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0031)

Each enum member should be on its own line. Fires when a member begins on the same line as the token before it (the opening brace for the first member, the preceding comma for the rest), anchored at the position right after that token (the newline-insertion point). Native port of Roslynator.Formatting.Analyzers RCS0031 — report-only (inserting the newlines is a reflow).

### `RCS0033` — Put a statement on its own line.

*Port of Roslynator.Formatting.Analyzers RCS0033 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0033)

Putting more than one statement on a line hurts readability. Fires when a statement's immediate previous sibling is itself a statement ending on the same row (a comment between them breaks the chain — divergence from SA1107), anchored at the END of that previous sibling (the newline-insertion point). Also fires right after a block's opening `{` when the first of its 2+ statements shares the brace's line, and at a switch label's boundary colon when the first statement of the section shares its row (duplicating RCS0024's finding under this id), then chains through the rest of the section's same-line statements. Native port of Roslynator.Formatting.Analyzers RCS0033. Fixable (issue #150) for the `block`-chain shape only (inserts a newline at the shared line's OWN indent — no extra level); the brace-adjacent and `switch_section` label-adjacent boundaries stay report-only (their insertions need indent-level modeling not yet present).

### `RCS0036` — Remove a blank line between single-line declarations of the same kind.

*Port of Roslynator.Formatting.Analyzers RCS0036 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0036)

Two single-line declarations of the SAME kind separated by a blank-line run should have it removed. The inverse of RCS0012; different-kind pairs are RCS0013's territory (blank line wanted, not removed) — the two rules partition cleanly, never firing on the same boundary. One finding per redundant run at column 1 of its first blank line. Native port of Roslynator.Formatting.Analyzers RCS0036 — report-only (deleting the line is a rewrite).

### `RCS0048` — Put a single-element initializer on a single line.

*Port of Roslynator.Formatting.Analyzers RCS0048 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0048)

An object/collection/array initializer with EXACTLY one element, currently spread across multiple lines even though that element itself is single-line and comment-free, should be collapsed to one line. The inverse direction of the rest of this analyzer family. Anonymous object initializers are a different node shape and not covered. Anchored at the initializer's opening `{`. Native port of Roslynator.Formatting.Analyzers RCS0048 — report-only (the collapse is a rewrite).

### `RCS0049` — Add a blank line after the file's top comment.

*Port of Roslynator.Formatting.Analyzers RCS0049 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0049)

A `//` line-comment run at the very top of the file (only blank lines may precede it) that is directly followed by code should be separated by a blank line. Fires at the end of the comment run's last line (the newline-insertion point). `/* */` block comments and `///` doc comments do not count as the file's "top comment". The fix inserts a blank line when the code is already on its own row; the rare same-row case is left report-only. Native port of Roslynator.Formatting.Analyzers RCS0049.

### `RCS0057` — Normalize whitespace at the beginning of a file.

*Port of Roslynator.Formatting.Analyzers RCS0057 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0057)

A file that does not start immediately with real content — leading blank line(s) or indentation before the first token or comment — should have that leading whitespace removed. Does not fire when the file opens directly with a comment (no leading whitespace to remove); RCS0049 independently covers the blank-line-after-top-comment case. Anchored at (1, 1). Native port of Roslynator.Formatting.Analyzers RCS0057 — report-only (the trim is a rewrite).

### `RCS0062` — Put an expression body on its own line.

*Port of Roslynator.Formatting.Analyzers RCS0062 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0062)

An expression-bodied member (`=> expr`) should have its body on its own line. Fires when the `=>` begins on the same line as the token before it (the parameter list's `)`, the member name, the indexer's `]`), anchored at the position right after that token (the newline-insertion point; `int A() => 1;` → the `)`-end). Native port of Roslynator.Formatting.Analyzers RCS0062. Fixable (issue #150): inserts a newline plus the declaration's indent extended one level in the whitespace-only gap before the `=>`; withheld when that gap holds a comment.

### `RCS0063` — Remove an unnecessary blank line.

*Port of Roslynator.Formatting.Analyzers RCS0063 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS0063)

A blank-line run carries no separation value at the start/end of the file, right after `{`, right before `}`, or when two or more lines long. Fires once per redundant run, at the blank line adjacent to the following content (or the first blank of a trailing run), anchored at column 1; blank lines inside a multi-line string are not counted. The fix deletes the run entirely at an edge/brace/doc/chain boundary, or collapses an interior run to a single blank line. The non-deprecated twin of Roslynator RCS1036 (which moved here as RCS0063 in Roslynator 4.14.0) — identical detection and anchors, verified against the oracle. Native port of Roslynator.Formatting.Analyzers RCS0063.

### `RCS1001` — Add braces (when expression spans multiple lines).

*Port of Roslynator.Analyzers RCS1001 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1001)

A control statement whose brace-less body spans more than one physical line should carry braces. The Roslynator twin of SA1519, anchored at the child statement and reusing the shared multi-line unbraced-body detector. Native port of Roslynator.Analyzers RCS1001 — report-only.

### `RCS1003` — Add braces to if-else.

*Port of Roslynator.Analyzers RCS1003 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1003)

When an `if`/`else` statement braces some clauses but not others, every clause should use braces. Fires once per such inconsistently-braced if-else chain, anchored at the leading `if` keyword (a uniformly brace-less chain is consistent and exempt). Native port of Roslynator.Analyzers RCS1003 — report-only.

### `RCS1005` — Simplify nested using statement.

*Port of Roslynator.Analyzers RCS1005 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1005)

When a `using` statement's body is a block whose only statement is another `using`, the two can be stacked as `using (a) using (b) { … }` without the inner braces. Fires at the outer block's `{`; a body holding more than the nested `using`, or one already in the stacked form, is left alone. Native port of Roslynator.Analyzers RCS1005 — report-only (merging is a structural rewrite).

### `RCS1006` — Merge 'else' with nested 'if'.

*Port of Roslynator.Analyzers RCS1006 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1006)

An `else` whose body is a block containing only a nested `if` reads better as `else if`. Fires on that shape, anchored at the `else` keyword; the fix unwraps the block. Native port of Roslynator.Analyzers RCS1006.

### `RCS1007` — Add braces to a single-statement control body.

*Port of Roslynator.Analyzers RCS1007 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1007)

Omitting braces on a control statement invites bugs when a second line is added later. The Roslynator twin of SA1503 — a control statement whose body is a single un-braced statement, anchored at that statement (an `else if` continuation is exempt). Native port of Roslynator.Analyzers RCS1007 — report-only.

### `RCS1015` — Use nameof operator.

*Port of Roslynator.Analyzers RCS1015 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1015)

A string literal whose value matches a parameter name should use `nameof`. Fires on such a literal, anchored at it. The Roslynator twin of CA1507. Native port of Roslynator.Analyzers RCS1015 — report-only.

### `RCS1019` — Order modifiers.

*Port of Roslynator.Analyzers RCS1019 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1019)

Modifiers on a declaration should appear in a consistent order (accessibility first, then `const`/`static`, then the rest). The Roslynator twin of SA1206 — fires once per declaration whose modifiers are out of order, anchored at the first modifier. Native port of Roslynator.Analyzers RCS1019 — report-only.

### `RCS1020` — Simplify Nullable<T> to T?.

*Port of Roslynator.Analyzers RCS1020 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1020)

`Nullable<T>` reads more clearly as the `T?` shorthand. Fires on a `Nullable<T>` generic type (qualified or not), anchored at the full type expression; the fix rewrites it to `T?`. Native port of Roslynator.Analyzers RCS1020 (StyleCop twin: SA1125).

### `RCS1021` — Convert lambda block body to expression body.

*Port of Roslynator.Analyzers RCS1021 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1021)

A lambda whose block body is a single `return X;` (or one expression statement) can use an expression body (`x => X`). Fires on such a lambda, anchored at the block. Native port of Roslynator.Analyzers RCS1021 — report-only.

### `RCS1031` — Remove unnecessary braces from a switch section.

*Port of Roslynator.Analyzers RCS1031 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1031)

When a `switch` section's entire body is wrapped in an explicit `{ }` block, the braces add only nesting — the statements can sit directly under the `case` label. Fires at the opening `{`. Report-only — unwrapping the block is a reflow best left to the formatter. Native port of Roslynator.Analyzers RCS1031.

### `RCS1032` — Remove redundant parentheses.

*Port of Roslynator.Analyzers RCS1032 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1032)

Parentheses around a complete value add nothing. Fires on a parenthesized expression whose parent is an `=>` body, an assignment's right side, a call argument, another set of parentheses, or a `return`. Narrower than StyleCop SA1119 (Roslynator does not flag a variable initializer or a conditional branch). Native port of Roslynator RCS1032 — purely syntactic, report-only.

### `RCS1033` — Remove redundant boolean literal.

*Port of Roslynator.Analyzers RCS1033 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1033)

An `==`/`!=` comparison with a boolean literal (`x == true`) is redundant. The Roslynator twin of MA0073, anchored at the operator. Native port of Roslynator.Analyzers RCS1033 — report-only.

### `RCS1036` — Remove redundant empty line.

*Port of Roslynator.Analyzers RCS1036 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1036)

A blank-line run carries no separation value at the start/end of the file, right after `{`, right before `}`, or when two or more lines long. Fires once per redundant run, at the blank line adjacent to the following content (or the first blank of a trailing run), anchored at column 1; blank lines inside a multi-line string are not counted. The fix deletes the run entirely at an edge/brace/doc/chain boundary, or collapses an interior run to a single blank line. Native port of Roslynator.Analyzers RCS1036.

### `RCS1037` — Remove trailing white-space.

*Port of Roslynator.Analyzers RCS1037 · Redundancy · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1037)

Whitespace at the end of a line is invisible noise that pollutes diffs. Fires on each line with trailing whitespace (skipping multi-line string interiors), anchored at the first trailing character; the fix deletes it, leaving the line ending intact. The Roslynator counterpart of StyleCop's SA1028. Native port of Roslynator.Analyzers RCS1037.

### `RCS1039` — Remove empty argument lists from attributes.

*Port of Roslynator.Analyzers RCS1039 · Redundancy · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1039)

An attribute with an empty argument list (`[Obsolete()]`) means exactly the same as the bare form (`[Obsolete]`); the parentheses are noise. Fires at the `(`; the fix removes the `()`. An attribute with any positional or named argument is left alone. Native port of Roslynator RCS1039.

### `RCS1040` — Remove empty else clause.

*Port of Roslynator.Analyzers RCS1040 · Redundancy · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1040)

An `else` clause whose body is an empty block adds nothing. Fires on such a clause, anchored at the `else` keyword; the fix removes the clause. Shares RCS1259's empty-`else` detector. Native port of Roslynator.Analyzers RCS1040.

### `RCS1043` — Remove 'partial' modifier from a type with a single part.

*Port of Roslynator.Analyzers RCS1043 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1043)

A type marked `partial` whose name appears in only one declaration in the file has no second part to merge with, so the modifier is gratuitous. Fires at the `partial` modifier. Note: `partial` is meaningful across files, which a single-file syntactic parse cannot see — a part split across files would still be flagged. Native port of Roslynator RCS1043 — report-only.

### `RCS1044` — Remove original exception from throw statement.

*Port of Roslynator.Analyzers RCS1044 · Correctness · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1044)

`throw ex;` inside a `catch` resets the exception's stack trace; dropping the variable (a bare `throw;`) preserves it. Fires when the thrown identifier is the enclosing `catch`'s exception variable, anchored at the exception; the fix rewrites it to `throw;`. The Roslynator sibling of CA2200 / MA0027. Native port of Roslynator.Analyzers RCS1044.

### `RCS1047` — Non-asynchronous method name should not end with 'Async'.

*Port of Roslynator.Analyzers RCS1047 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1047)

The `Async` suffix is reserved by convention for asynchronous methods. Fires on a method whose name ends with `Async` that is neither marked `async` nor returns an awaitable type (`Task`/`ValueTask`/`IAsyncEnumerable`/…) — a task-returning method is asynchronous even without the `async` keyword. Anchored at the method name. Native port of Roslynator.Analyzers RCS1047 — report-only.

### `RCS1048` — Use lambda expression instead of anonymous method.

*Port of Roslynator.Analyzers RCS1048 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1048)

An anonymous method with a parameter list (`delegate (int x) { … }`) is a verbose lambda. Fires at the `delegate` keyword; the fix drops `delegate` and inserts `=>` after the parameter list, leaving a block-bodied lambda (`(int x) => { … }`). The bare `delegate { … }` form (no parameter list) is left alone — it ignores the delegate's parameters, which a lambda cannot express. Native port of Roslynator.Analyzers RCS1048.

### `RCS1049` — Simplify boolean comparison.

*Port of Roslynator.Analyzers RCS1049 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1049)

A comparison whose simplification introduces a negation — `x == false` or `x != true` → `!x` — should be written directly. Fires on such a comparison, anchored at its start. The complementary `x == true` / `x != false` (which simplify to plain `x`) are RCS1033's concern. Native port of Roslynator RCS1049 — report-only.

### `RCS1056` — Using alias directive.

*Port of Roslynator.Analyzers RCS1056 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1056)

A `using X = Some.Namespace.Type;` alias hides the type's real name from readers of the file, so the same type reads under two different names across a codebase. Fires on a using directive that carries an alias, anchored at the `using` keyword. Native port of Roslynator RCS1056 — report-only.

### `RCS1058` — Use compound assignment.

*Port of Roslynator.Analyzers RCS1058 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1058)

`x = x + y` repeats the target; `x += y` is shorter and clearer. Fires on an `=` assignment whose right side is a binary operation whose left operand is the assignment target, anchored at the assignment. Native port of Roslynator.Analyzers RCS1058 — report-only.

### `RCS1059` — Avoid locking on publicly accessible instance.

*Port of Roslynator.Analyzers RCS1059 · Concurrency · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1059)

The Roslynator twin of CA2002: locking on `this`, a `Type`, or a string is flagged at the locked expression. Shares CA2002's detector. Native port of Roslynator.Analyzers RCS1059 — report-only.

### `RCS1061` — Merge an if with its sole nested if.

*Port of Roslynator.Analyzers RCS1061 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1061)

When an `if` with no `else` contains nothing but a second `if` that also has no `else`, the two conditions can be merged with `&&`, removing a level of nesting. Fires at the outer `if` (the Roslynator counterpart of Sonar S1066, which anchors at the inner one). Native port of Roslynator RCS1061 — report-only (merging is a structural rewrite).

### `RCS1068` — Simplify logical negation.

*Port of Roslynator.Analyzers RCS1068 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1068)

A `!` is redundant when its operand is already boolean and simplifiable: `!true` / `!false`, `!!x` (double negation), and `!(a == b)` (negated comparison). Fires at the `!`. A plain `!x` is left alone. Native port of Roslynator RCS1068 — report-only (the rewrites overlap for stacked negations).

### `RCS1069` — Remove unnecessary case label.

*Port of Roslynator.Analyzers RCS1069 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1069)

When a `case` label shares its section with the `default` label (`case 1: case 2: default:`), the `case` labels are unnecessary — `default` already handles those values. Fires at each non-default label in such a section. Native port of Roslynator.Analyzers RCS1069 — report-only.

### `RCS1070` — Remove redundant default switch section.

*Port of Roslynator.Analyzers RCS1070 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1070)

A `default:` section whose only statement is `break;` does nothing — the switch already falls through to the same place. Fires on such a section, anchored at the `default` keyword. Native port of Roslynator.Analyzers RCS1070 — report-only.

### `RCS1073` — Convert 'if' to 'return' statement.

*Port of Roslynator.Analyzers RCS1073 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1073)

`if (c) return <bool>; return <bool>;` collapses to `return c;` (or `return !c;`). Fires on an `if` with no `else` whose body is a single `return <boolean-literal>;` and whose following statement is also `return <boolean-literal>;`, anchored at the `if` keyword. Native port of Roslynator.Analyzers RCS1073 — report-only (the collapse rewrites multiple statements).

### `RCS1074` — Remove redundant constructors.

*Port of Roslynator.Analyzers RCS1074 · Redundancy · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1074)

An explicit instance constructor that is identical to the compiler-synthesised default — `public`, parameterless, empty body, no initializer, undocumented, and the sole instance constructor — is redundant; removing it leaves the implicit default with the same effect. Fires at the declaration start; the fix deletes it. Skips `static`/`private`/attributed/initialized/`///`- documented constructors, `abstract` classes, and structs. Native port of Roslynator RCS1074.

### `RCS1075` — Avoid empty catch clauses that catch System.Exception.

*Port of Roslynator.Analyzers RCS1075 · Correctness · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1075)

An empty `catch (Exception) { }` swallows every error, turning bugs into silent failures. Fires at the `catch` keyword when the body is empty and the caught type is exactly `Exception` / `System.Exception` (a general `catch { }` and a catch of a specific type are left to other rules). Native port of Roslynator RCS1075 — report-only (log, rethrow, or narrow the type is a human choice).

### `RCS1077` — Optimize LINQ method call.

*Port of Roslynator.Analyzers RCS1077 · Performance · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1077)

A `Where(predicate).Count()` chain (or `.Any()`, `.First()`, …) can fold the predicate into the terminal, and an identity-key `OrderBy(x => x)` can be simplified to `Order()`. Covers those two cases of Roslynator's broader RCS1077 (other optimizations deferred), anchored at the `Where` / `OrderBy`. Native port of Roslynator.Analyzers RCS1077 — report-only.

### `RCS1080` — Array '.Any()' should use the Length property.

*Port of Roslynator.Analyzers RCS1080 · Performance · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1080)

`array.Any()` enumerates the array where the `Length` property answers in O(1). Fires on a no-argument `.Any()` whose receiver is an identifier resolving, via the enclosing function's parameters/locals, to an array type — anchored at the `Any` name. Native port of Roslynator RCS1080 (array subset; `List<T>`/external receivers deferred) — report-only.

### `RCS1084` — Use coalesce expression instead of conditional expression.

*Port of Roslynator.Analyzers RCS1084 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1084)

A ternary that yields the tested value when non-null and a fallback otherwise is exactly `??`. Fires on `x != null ? x : y` and `x == null ? y : x`, anchored at the conditional expression; the fix is `x ?? y`. Native port of Roslynator.Analyzers RCS1084.

### `RCS1085` — Use auto-implemented property.

*Port of Roslynator.Analyzers RCS1085 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1085)

A property whose `get` just returns a backing field and whose `set` just assigns it is an auto-property written longhand. Fires on such a property, anchored at the property name. Native port of Roslynator.Analyzers RCS1085 — report-only.

### `RCS1089` — Use ++/-- operator instead of assignment.

*Port of Roslynator.Analyzers RCS1089 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1089)

`x = x + 1` / `x = x - 1` is the increment/decrement operator written longhand. Fires on such an assignment, anchored at the assignment. Native port of Roslynator.Analyzers RCS1089 — report-only.

### `RCS1097` — Redundant 'ToString' call on a string.

*Port of Roslynator.Analyzers RCS1097 · Redundancy · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1097)

Calling `.ToString()` on a value that is already a `string` does nothing. Fires on a no-argument `ToString()` whose receiver is a `string`-typed parameter or local of the enclosing method (the CST-decidable case). The fix deletes the call, leaving the receiver. Native port of Roslynator RCS1097; the Meziantou twin is MA0044.

### `RCS1098` — Constant values should be on the right side of comparisons.

*Port of Roslynator.Analyzers RCS1098 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1098)

A "Yoda" equality comparison with the constant on the left (`0 == x`, `null == s`) reads less naturally than `x == 0`. Fires at the left operand of an `==` / `!=` whose left side is a literal and right side is not; the fix swaps the operands (sound for equality). Relational operators are left alone (that is StyleCop SA1131). Native port of Roslynator RCS1098.

### `RCS1102` — Make class static.

*Port of Roslynator.Analyzers RCS1102 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1102)

A class whose members are all `static` (or `const`), with no base list and no instance constructor, should be declared `static` so the compiler enforces that it is never instantiated. Fires on such a class, anchored at its name (the Roslynator counterpart of the S1118 holder check, but requiring no instance constructor at all). Native port of Roslynator.Analyzers RCS1102 (report-only — adding `static` can ripple into callers).

### `RCS1104` — Simplify boolean conditional expressions.

*Port of Roslynator.Analyzers RCS1104 · Redundancy · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1104)

A `?:` whose branches are the literals `true`/`false` is just the condition: `cond ? true : false` is `cond`, and `cond ? false : true` is `!(cond)`. Fires at the conditional expression; the fix collapses it. Sound without a type model — a `?:` condition is always `bool` and binds tighter than the `?:`. Native port of Roslynator RCS1104.

### `RCS1110` — Declare type inside a namespace.

*Port of Roslynator.Analyzers RCS1110 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1110)

A top-level type should be declared inside a namespace, not the global scope. Fires on a type with no namespace ancestor, anchored at its name (the Roslynator twin of CA1050). Native port of Roslynator RCS1110 — report-only.

### `RCS1118` — Mark local variable as const.

*Port of Roslynator.Analyzers RCS1118 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1118)

A local whose initializer is a compile-time constant (a literal, `nameof(…)`, a `const` in scope, or an operator expression over those) and that is never reassigned can be declared `const`. Fires on such a local declaration, anchored at its type; an already-`const`, `using`/`ref`/`fixed`, reassigned, or non-constant-initialized local is left alone. Native port of Roslynator.Analyzers RCS1118 — report-only.

### `RCS1123` — Add parentheses when necessary.

*Port of Roslynator.Analyzers RCS1123 · Maintainability · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1123)

Mixing multiplicative (`*`/`/`/`%`) with additive (`+`/`-`), or `&&` with `||`, without parentheses relies on the reader knowing C#'s precedence. Fires on a multiplicative expression used as an operand of an additive one, or an `&&` used as an operand of a `||`; the fix parenthesizes it (`a + b * c` -> `a + (b * c)`, `a && b || c` -> `(a && b) || c`). The Roslynator counterpart of StyleCop SA1407/SA1408. Native port of Roslynator.Analyzers RCS1123.

### `RCS1124` — Inline a local variable that is used only once.

*Port of Roslynator.Analyzers RCS1124 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1124)

A local that is initialised and then read exactly once can be inlined into that single use. Fires on a single-declarator local declaration with an initialiser whose name is referenced exactly once in the enclosing function, anchored at the declaration — unless the single use is inside a nested lambda or local function (the local is captured). Native port of Roslynator RCS1124 — T2 (syntactic intra-procedure), report-only (inlining is a multi-location rewrite).

### `RCS1126` — Add braces to if-else.

*Port of Roslynator.Analyzers RCS1126 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1126)

Every clause of an `if`/`else` should use braces. Fires on each brace-less embedded clause of an `if` that has an `else`, anchored at the statement. Native port of Roslynator.Analyzers RCS1126 (report-only — adding braces is a structural rewrite).

### `RCS1129` — Remove redundant field initialization.

*Port of Roslynator.Analyzers RCS1129 · Redundancy · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1129)

A field initialized to its type's default value (`= 0` / `= false` / `= null`) repeats the runtime's own zero-initialization. Fires per declarator whose initializer is the default value, anchored at the `=`; the fix removes the ` = <value>`. `const` fields are exempt. The Roslynator twin of CA1805. Native port of Roslynator RCS1129.

### `RCS1130` — Bitwise operation on an enum without [Flags].

*Port of Roslynator.Analyzers RCS1130 · Correctness · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1130)

A bitwise `&`/`|`/`^` on an enum not marked `[Flags]` is almost always a mistake — the enum was not designed to be combined. Fires when an operand is an `EnumType.Member` access or an identifier resolved to an enum-typed parameter/local, and that enum is declared in the same file without `[Flags]`. Native port of Roslynator RCS1130 — report-only.

### `RCS1134` — Remove redundant statements.

*Port of Roslynator.Analyzers RCS1134 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1134)

A jump that does not change control flow is redundant: a value-less `return;` as the last statement of a method-like body, or a `continue;` as the last statement of a loop body. Fires at the jump keyword (the Roslynator counterpart of Sonar S3626, at the same locations). Native port of Roslynator RCS1134 — report-only.

### `RCS1135` — Declare an enum member with value zero when the enum has FlagsAttribute.

*Port of Roslynator.Analyzers RCS1135 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1135)

A `[Flags]` enum should declare a zero-valued member (e.g. `None = 0`) so the "no flags" state is nameable. Fires on a `[Flags]` enum with no zero-valued member (an explicit `= 0`, or an initializer-less first member), anchored at the enum name. Native port of Roslynator.Analyzers RCS1135 — report-only.

### `RCS1136` — Merge switch sections with equivalent content.

*Port of Roslynator.Analyzers RCS1136 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1136)

Two adjacent switch sections whose statement bodies are identical can share their labels (`case 1: case 2: …`). Fires on the first of such a pair, anchored at its first statement. Non-adjacent duplicates and differing bodies are left alone. Native port of Roslynator RCS1136 — report-only (merging the labels is a structural rewrite).

### `RCS1138` — Add summary to documentation comment.

*Port of Roslynator.Analyzers RCS1138 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1138)

A doc comment with an empty `<summary></summary>` should be filled in. Fires on a member whose doc has an empty summary, anchored at the `<summary>` tag (StyleCop twin SA1606 anchors at the member name). Native port of Roslynator.Analyzers RCS1138 — report-only.

### `RCS1139` — Add summary element to documentation comment.

*Port of Roslynator.Analyzers RCS1139 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1139)

A documented element whose `///` comment lacks a `<summary>` should gain one. The Roslynator twin of SA1604, anchored at the first doc line and reusing the shared missing-summary detector. Native port of Roslynator.Analyzers RCS1139 — report-only.

### `RCS1140` — Add exception to documentation comment.

*Port of Roslynator.Analyzers RCS1140 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1140)

When a documented member throws a freshly constructed exception, list it in an `<exception>` element so callers know to expect it. Fires on a `throw new <Exception>(…)` whose enclosing member has a `///` doc comment with no `<exception>` element, anchored at the `throw`. Requires an existing doc comment and a `throw new …` (a `throw ex;` rethrow has no syntactic type); checks only for the presence of any `<exception>` element, not a `cref` match (no type model). Native port of Roslynator RCS1140 — report-only (inserting the element is a multi-line doc rewrite).

### `RCS1141` — Add 'param' element to documentation comment.

*Port of Roslynator.Analyzers RCS1141 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1141)

When a member with parameters has a `///` documentation comment, every parameter should have a `<param>` element. Fires once per member whose doc comment is missing a `<param>` for at least one parameter, anchored at the doc comment (the Roslynator counterpart of StyleCop's per-parameter SA1611). Native port of Roslynator.Analyzers RCS1141 (report-only — inserting the elements is a multi-line doc rewrite, deferred).

### `RCS1142` — Add 'typeparam' element to documentation comment.

*Port of Roslynator.Analyzers RCS1142 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1142)

When a generic member or type has a `///` documentation comment, every type parameter should have a `<typeparam>` element. Fires once per element whose doc comment is missing a `<typeparam>` for at least one type parameter, anchored at the doc comment (the Roslynator counterpart of StyleCop's per-type-parameter SA1618). Native port of Roslynator.Analyzers RCS1142 (report-only — inserting the elements is a multi-line doc rewrite, deferred).

### `RCS1146` — Use conditional access.

*Port of Roslynator.Analyzers RCS1146 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1146)

`if (x != null) x.M();` is the null-conditional operator written longhand — `x?.M();` says the same. Fires on an `if (x != null)` (no `else`) whose single body statement accesses `x`, anchored at the `if`. Native port of Roslynator.Analyzers RCS1146 — report-only.

### `RCS1156` — Use string.Length instead of comparison with empty string.

*Port of Roslynator.Analyzers RCS1156 · Performance · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1156)

Comparing a string with `==`/`!=` against an empty string literal should use `.Length` instead. Fires on such a comparison, anchored at the binary expression. The .NET-analyzer twin (CA1820) shares its detector. Native port of Roslynator.Analyzers RCS1156 — report-only.

### `RCS1158` — Static member in generic type should use a type parameter.

*Port of Roslynator.Analyzers RCS1158 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1158)

A static member of a generic type that never mentions a type parameter does not benefit from being generic — it should live on a non-generic type. Fires on a `static` method or property of a generic type whose signature and body reference none of the type parameters, anchored at the member name. Native port of Roslynator.Analyzers RCS1158 — report-only.

### `RCS1160` — Abstract type should not have public constructors.

*Port of Roslynator.Analyzers RCS1160 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1160)

The Roslynator twin of CA1012 / MA0017: fires on each `public`/`internal` constructor of an `abstract` class, anchored at the constructor name. Shares CA1012's detector. Native port of Roslynator.Analyzers RCS1160 — report-only.

### `RCS1161` — Enum should declare explicit values.

*Port of Roslynator.Analyzers RCS1161 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1161)

Relying on implicit enum values ties each member to its declaration order, so inserting or reordering a member silently changes the others' numeric values (breaking persistence and interop). Fires when any member of an enum lacks an explicit `= value`. Native port of Roslynator RCS1161 — purely syntactic, report-only.

### `RCS1163` — Unused parameter.

*Port of Roslynator.Analyzers RCS1163 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1163)

A parameter never used in the body is dead. Fires per parameter (of a method, operator, conversion operator, indexer, lambda, or local function) whose name appears nowhere in the body outside its declaration, anchored at the parameter. A method that is `abstract`/`virtual`/`override`/`partial`/`extern` or has no body is exempt (its signature is dictated by a contract); a lambda parameter that is already a discard (`_`) is skipped. Native port of Roslynator RCS1163 — syntactic (intra-procedure), report-only (removing a parameter ripples to call sites).

### `RCS1164` — Unused type parameter.

*Port of Roslynator.Analyzers RCS1164 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1164)

A type parameter of a method or local function that is never referenced in its declaration is dead weight. Fires once per unused parameter, anchored at its name. Narrower than the Sonar twin S2326 (which also flags unused type parameters on type declarations); reuses S2326's syntactic used check. Native port of Roslynator RCS1164 — report-only (removing a type parameter ripples to call sites).

### `RCS1168` — Parameter name differs from base name.

*Port of Roslynator.Analyzers RCS1168 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1168)

An override's (or interface implementation's) parameter name should match the base declaration's, so IDE tooltips and named arguments stay meaningful across the hierarchy. Fires per parameter (by ordinal position) on a method or indexer whose name differs from the resolved base member's — the overridden method, the explicitly-named interface member, or (for an implicit implementation) the first interface member reachable from the type's base list. Resolved via the project declaration index + BCL type-fact table (issue #152); an unresolvable base (including any BCL base CLASS, which this table does not curate) is a silent, documented miss. Native port of Roslynator.Analyzers RCS1168 — report-only (the fix is a semantic rename).

### `RCS1169` — Make field read-only.

*Port of Roslynator.Analyzers RCS1169 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1169)

A private field that is only assigned in its declaration initialiser or a constructor can be `readonly`. Fires on a private, non-`readonly`, non-`const` field that is never written outside a constructor — an assignment, an increment/decrement, or being passed by `ref`/`out` disqualifies it. Anchored at the field declaration. Native port of Roslynator RCS1169 — T2 (syntactic intra-type), report-only (adding `readonly` is the author's edit).

### `RCS1170` — Use read-only auto-implemented property.

*Port of Roslynator.Analyzers RCS1170 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1170)

An auto-property `{ get; set; }` whose setter is effectively private and is only assigned in a constructor can be a get-only auto-property (`{ get; }`). Fires at the `set` accessor; a `public`/`protected`/`internal` setter, an `init` accessor, or a property assigned outside a constructor is left alone. Native port of Roslynator.Analyzers RCS1170 — report-only.

### `RCS1179` — Unnecessary assignment.

*Port of Roslynator.Analyzers RCS1179 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1179)

A local that is declared, assigned in each branch of a following `if`/`else` (or `switch`), and then returned can return the value directly in each branch instead. Fires on an `if`/`switch` immediately preceded by a single-declarator local declaration and immediately followed by `return <local>;`, where every branch ends with `<local> = …;`, anchored at the `if`/`switch`. Native port of Roslynator RCS1179 — report-only (a multi-statement restructure).

### `RCS1181` — Convert comment to documentation comment.

*Port of Roslynator.Analyzers RCS1181 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1181)

A single-line `//` comment that documents a declaration should be an XML documentation comment (`///`). Fires on a regular `//` comment leading a type or member declaration (the first line of the block above it) or trailing one (same line, after it). A `///` doc comment, a `/* … */` block comment, and a comment inside a method body are left alone. Native port of Roslynator RCS1181 — report-only (rewriting the prose to `///` is a structural edit).

### `RCS1187` — Use constant instead of field.

*Port of Roslynator.Analyzers RCS1187 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1187)

A `static readonly` field initialized to a compile-time constant could be a `const`. Fires on a `static readonly` field whose initializer is a literal (number, string, bool, char), anchored at the declaration; an instance `readonly` field is not flagged. Native port of Roslynator RCS1187 — report-only (`const` inlines the value at use sites, a binary-compatibility change).

### `RCS1188` — Remove redundant auto-property initialization.

*Port of Roslynator.Analyzers RCS1188 · Redundancy · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1188)

An auto-property initialized to its type's default value (`int X { get; set; } = 0;`) repeats what the runtime already does. Fires on such a property, anchored at the initializer value; the fix removes the ` = <value>`. Shares CA1805's default-initializer detector. Native port of Roslynator.Analyzers RCS1188.

### `RCS1189` — Add region name to #endregion.

*Port of Roslynator.Analyzers RCS1189 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1189)

A `#endregion` that does not repeat its region's name is hard to pair with its opening `#region` in a long file. Fires on a bare `#endregion` (no trailing name), anchored at the `#`. Native port of Roslynator RCS1189 — report-only (appending the name requires resolving the region nesting, deferred).

### `RCS1190` — Join string expressions.

*Port of Roslynator.Analyzers RCS1190 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1190)

Concatenating two string literals with `+` is pointless — they can be written as one literal. Fires on a `+` binary expression whose both operands are string literals, anchored at the expression (the outermost concatenation of a chain). Native port of Roslynator.Analyzers RCS1190 — report-only (joining literal contents correctly is left to the analyzer fix).

### `RCS1192` — Unnecessary usage of verbatim string literal.

*Port of Roslynator.Analyzers RCS1192 · Redundancy · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1192)

A verbatim string (`@"…"`) only earns its `@` when the content contains a backslash, an embedded quote, or a line break; otherwise the prefix is noise and a plain string reads the same. Fires on a verbatim string (including a verbatim interpolated string with no interpolation braces) whose content has no backslash, quote, or newline. Native port of Roslynator RCS1192; the fix deletes the `@`.

### `RCS1194` — Implement the standard exception constructors.

*Port of Roslynator.Analyzers RCS1194 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1194)

A class derived from an exception type should declare the conventional constructors — parameterless, `(string message)`, and `(string message, Exception innerException)` — so callers and `throw new` behave as expected. Fires at the class name when any is missing. Exception-derivation is detected syntactically (the base type's name ends in `Exception`); an exception reached through a differently named base is not seen (no type model). Native port of Roslynator RCS1194 — report-only.

### `RCS1203` — Use AttributeUsageAttribute.

*Port of Roslynator.Analyzers RCS1203 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1203)

A custom attribute type should declare `[AttributeUsage]` so its valid targets are explicit. The Roslynator twin of CA1018 — a non-abstract class deriving from `Attribute` with no `[AttributeUsage]`, anchored at the type name. Native port of Roslynator.Analyzers RCS1203 — report-only.

### `RCS1206` — Use conditional access instead of conditional expression.

*Port of Roslynator.Analyzers RCS1206 · Maintainability · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1206)

A ternary that guards a member access with a null check — `x != null ? x.M() : null` or `x == null ? null : x.M()` — is exactly what the null-conditional operator expresses: `x?.M()`. Fires at the conditional expression; the fix splices a `?` between the receiver and its access. The access branch must be a member access on the same receiver as the null check, with `null` on the other branch. Native port of Roslynator RCS1206.

### `RCS1208` — An if can be inverted into a guard to reduce nesting.

*Port of Roslynator.Analyzers RCS1208 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1208)

A trailing `if (c) { body }` nests `body` a level deeper than necessary; inverting it into a guard (`if (!c) return;`) flattens it. Fires on an `if` with no `else`, with a non-empty block body, that is the last statement of a `void` method or local function, anchored at the `if` keyword. Native port of Roslynator RCS1208 — report-only (the guard rewrite is left to the deep path).

### `RCS1209` — Order type parameter constraints.

*Port of Roslynator.Analyzers RCS1209 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1209)

The `where` constraint clauses of a generic type or method should appear in the same order as the type parameters they constrain. Fires on the first constraint clause that is out of that order, anchored at the clause. Native port of Roslynator.Analyzers RCS1209 — report-only.

### `RCS1211` — Remove unnecessary else clause.

*Port of Roslynator.Analyzers RCS1211 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1211)

When an `if` branch always exits (ends in `return`/`throw`/`break`/`continue`/`goto`), the following `else` is unnecessary — the else body can simply follow the `if`. Fires at the `else` keyword. Native port of Roslynator.Analyzers RCS1211 (report-only — unwrapping the else is a structural rewrite).

### `RCS1212` — Remove redundant assignment.

*Port of Roslynator.Analyzers RCS1212 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1212)

Assigning a variable and then immediately returning it is redundant — the value can be returned directly (`x = expr; return x;` → `return expr;`). Fires on such an assignment, anchored at the assigned variable. Native port of Roslynator RCS1212 — report-only.

### `RCS1213` — Remove unused member declaration.

*Port of Roslynator.Analyzers RCS1213 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1213)

A private member that the rest of its type never references is dead. Fires on an effectively-private field, constant, event, or method whose name appears nowhere in the enclosing type outside its declaration, anchored at the member name (per declarator for a field or event). Native port of Roslynator RCS1213 — syntactic (intra-type), report-only (removing a member can change behaviour).

### `RCS1214` — Unnecessary interpolated string.

*Port of Roslynator.Analyzers RCS1214 · Redundancy · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1214)

An interpolated string with no `{…}` holes (`$"x"` / `$@"x"`) is a normal string with a pointless `$`. Fires at the `$`; the fix removes it, leaving a plain (or verbatim) string literal. Raw interpolated strings (`$$"…"`) are left alone. Native port of Roslynator.Analyzers RCS1214.

### `RCS1215` — Expression is always equal to true or false.

*Port of Roslynator.Analyzers RCS1215 · Correctness · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1215)

The Roslynator twin of S3981: a collection's `.Count` or an array/string's `.Length` compared to `0` with `>=` / `<` (or the reversed `0 <=` / `0 >`) is constant — `list.Count >= 0` is always `true`, `array.Length < 0` is always `false`. Fires at the comparison; meaningful comparisons are left alone. Native port of Roslynator.Analyzers RCS1215 — report-only.

### `RCS1220` — Use pattern matching instead of 'is' and cast.

*Port of Roslynator.Analyzers RCS1220 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1220)

`if (x is T) { … (T)x … }` should use a type pattern `if (x is T t) { … t … }`. Fires on the statement that casts `(T)x` inside the body of an `if` whose condition is `x is T`, anchored at that statement. The Sonar twin (S3247) shares its detector. Native port of Roslynator.Analyzers RCS1220 — report-only.

### `RCS1225` — Make class sealed.

*Port of Roslynator.Analyzers RCS1225 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1225)

A class whose only constructors are `private` cannot be inherited, so it should be `sealed`. Fires on such a class (not already `sealed`/`abstract`/`static`, with at least one constructor and every constructor `private`), anchored at the class name. Native port of Roslynator.Analyzers RCS1225 — report-only (adding `sealed` is a structural change).

### `RCS1226` — Add paragraph to documentation comment.

*Port of Roslynator.Analyzers RCS1226 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1226)

When a `<summary>` holds two paragraphs separated by a blank `///` line, they should be wrapped in `<para>` elements. Fires at the first content line of a multi-paragraph summary that has no `<para>` tag. Native port of Roslynator.Analyzers RCS1226 — report-only.

### `RCS1228` — Unused element in documentation comment.

*Port of Roslynator.Analyzers RCS1228 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1228)

An empty `<param>`, `<typeparam>`, `<returns>`, or `<value>` element documents nothing. Fires per empty element (single- or multi-line), anchored at its tag and naming the element, on any documentable declaration (method, property, type, record, constructor, indexer, delegate, operator, …). Native port of Roslynator.Analyzers RCS1228 (StyleCop twins: SA1614/SA1616/SA1622) — report-only.

### `RCS1232` — Order elements in documentation comment.

*Port of Roslynator.Analyzers RCS1232 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1232)

The `<param>` elements in a documentation comment should follow the signature's parameter order. Fires once per method whose documented parameters are out of order, anchored at the first misplaced `<param>` element. The Roslynator counterpart of SA1612 (which fires per parameter at the name). Native port of Roslynator RCS1232 — report-only.

### `RCS1233` — Use short-circuiting operator.

*Port of Roslynator.Analyzers RCS1233 · Correctness · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1233)

A `&` / `|` on boolean operands should be the short-circuiting `&&` / `||`. Fires on such a binary expression, anchored at the operator, sharing S2178's boolean-operand detector. Native port of Roslynator.Analyzers RCS1233 — report-only.

### `RCS1234` — Duplicate enum value.

*Port of Roslynator.Analyzers RCS1234 · Correctness · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1234)

Two enum members with the same explicit value are usually a copy-paste slip. Fires on an enum member whose explicit value duplicates an earlier member's (by literal text), anchored at the value expression. Native port of Roslynator.Analyzers RCS1234 (Microsoft twin: CA1069) — report-only.

### `RCS1238` — Avoid nested ?: operators.

*Port of Roslynator.Analyzers RCS1238 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1238)

A ternary `?:` whose true or false branch is itself a ternary is hard to read. Fires on the outer conditional (the inverse of Sonar S3358, which flags the inner one); nesting in the condition is not flagged. Native port of Roslynator.Analyzers RCS1238 — purely syntactic, report-only.

### `RCS1241` — Implement non-generic counterpart.

*Port of Roslynator.Analyzers RCS1241 · Maintainability · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1241)

A type implementing `IComparable<T>`/`IComparer<T>`/`IEqualityComparer<T>` should also implement its non-generic counterpart so it interoperates with non-generic APIs. Fires on a type that names one of these three generic interfaces in its base list without the matching non-generic one, anchored at the type name. The Meziantou twin (MA0097) is a genuinely narrower, IComparable-only real rule and keeps its own detector. Native port of Roslynator.Analyzers RCS1241 — report-only.

### `RCS1242` — Non-readonly struct passed by 'in' reference.

*Port of Roslynator.Analyzers RCS1242 · Performance · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1242)

An `in` parameter of a struct that is not a `readonly struct` makes the compiler copy the struct defensively on every member access, defeating the point of `in`. Fires on an `in` parameter whose type is a non-`readonly` struct declared in the same file, anchored at the parameter name. Native port of Roslynator RCS1242 — report-only.

### `RCS1244` — Simplify 'default' expression.

*Port of Roslynator.Analyzers RCS1244 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1244)

When the target type is clear from context, `default(T)` can be written as the bare `default` literal. Fires on a `default(T)` expression, anchored at it. Native port of Roslynator.Analyzers RCS1244 — report-only (dropping the type is unsound where it is not inferable, e.g. `var x = default(int);`, which needs a semantic model).

### `RCS1251` — Remove unnecessary braces from an empty-body type.

*Port of Roslynator.Analyzers RCS1251 · Redundancy · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1251)

A type declared with an empty body (`{ }` containing no members) carries braces that add nothing. Fires at the opening `{`. A `record`/`record struct` fires only when it has a primary-constructor parameter list (`record R(int X) { }` — source-confirmed against the real analyzer; a parameterless `record R { }` is silent), and class/struct/interface only without one. The fix replaces `{ }` with `;` on a record (the `record` keyword proves this form is valid); class/struct/interface have no semicolon-bodied form at all, so those firings stay report-only. Native port of Roslynator.Analyzers RCS1251.

### `RCS1259` — Remove empty syntax.

*Port of Roslynator.Analyzers RCS1259 · Redundancy · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1259)

Redundant empty syntax adds noise. Fires on an empty statement (a stray `;`), an empty finalizer (`~T() { }`), an empty namespace block, an empty `finally`/`else` clause, and an empty `#region`/`#endregion` pair; the fix deletes the construct (report-only for the empty region — deleting its two non-adjacent lines needs two ranges). Native port of Roslynator.Analyzers RCS1259 (the empty-region shape is upstream's deprecated RCS1091, whose reported id is redirected to RCS1259).

### `RCS1262` — Unnecessary raw string literal.

*Port of Roslynator.Analyzers RCS1262 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1262)

A raw string literal (`"""…"""`) is only needed when its content would require escaping in a regular string — an embedded double quote, a backslash, or a newline. With none of those the raw form is unnecessary and could be a plain `"…"`. Fires on such a literal, anchored at it. Native port of Roslynator RCS1262 — report-only (the conversion is a rewrite).

### `RCS1265` — Remove redundant catch block.

*Port of Roslynator.Analyzers RCS1265 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1265)

A bare `catch { throw; }` catches every exception only to rethrow it unchanged — deleting the whole `try`/`catch` behaves identically. Fires at the `catch` keyword; a typed `catch (T)` (which narrows the caught type) or one with a `when` filter is not flagged, the narrower counterpart of Sonar S2737. Native port of Roslynator.Analyzers RCS1265 (report-only — removing the clause is a structural rewrite).

### `S100` — Methods and properties should be named in PascalCase.

*Port of SonarAnalyzer.CSharp S100 · Style · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-100/)

A method or property name should be PascalCase, with acronyms of at most two letters (`XmlParse` is fine, `XMLParse` is flagged for the three-letter run). Fires at the method or property name. The method/property sibling of S101 (types), sharing its PascalCase definition. Native port of SonarAnalyzer.CSharp S100 — report-only (a rename ripples to call sites).

### `S1006` — Method overrides should not change parameter defaults.

*Port of SonarAnalyzer.CSharp S1006 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1006/)

An override's (or interface implementation's) parameter default value should match the base declaration — callers see the DECLARED type's default at a virtual call site, so a differing override default is a latent surprise. Fires per parameter (by ordinal position): add a missing default, remove an extraneous one (including on an explicit interface implementation, which can never declare its own), or use the base's value when both declare one but the values provably differ. Resolved via the project declaration index + BCL type-fact table (issue #152); an unresolvable base (including any BCL base CLASS, which this table does not curate) is a silent, documented miss, and a default-value comparison this port cannot safely canonicalize (anything beyond an int/bool/string/char/null literal) also stays silent. Native port of SonarAnalyzer.CSharp S1006 — report-only.

### `S101` — Types should be named in PascalCase.

*Port of SonarAnalyzer.CSharp S101 · Style · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-101/)

A type name should be PascalCase, with acronyms of at most two letters (`CA1041Fixture` is fine, `RCS1098Fixture` is not); an interface must also start with `I` followed by an uppercase letter. Fires at the type name when the convention is broken. Native port of SonarAnalyzer.CSharp S101 — report-only (renaming a type is a cross-file refactor).

### `S1066` — Collapsible if statements should be merged.

*Port of SonarAnalyzer.CSharp S1066 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1066/)

When an `if` with no `else` contains nothing but a second `if` that also has no `else`, the two conditions can be merged with `&&`, removing a level of nesting. Fires at the inner `if`. An outer or inner `else`, or any sibling statement alongside the inner `if`, makes the merge unsafe and is left alone. Native port of SonarAnalyzer.CSharp S1066 — report-only (merging is a structural rewrite).

### `S1067` — Expressions should not be too complex.

*Port of SonarAnalyzer.CSharp S1067 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1067/)

A boolean expression with more than three conditional operators (`&&` / `||`) is hard to read. Fires on the outermost `&&`/`||` expression when its tree contains four or more such operators, anchored at the start of the expression. Native port of SonarAnalyzer.CSharp S1067 — report-only.

### `S107` — Methods should not have too many parameters.

*Port of SonarAnalyzer.CSharp S107 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-107/)

A method with more than seven parameters (the Sonar default) is hard to call and usually wants a parameter object. Fires on a method with eight or more parameters, anchored at the method name. Native port of SonarAnalyzer.CSharp S107 — report-only.

### `S108` — Nested blocks of code should not be left empty.

*Port of SonarAnalyzer.CSharp S108 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-108/)

An empty `{ }` block on a control-flow statement is usually a mistake or dead code. Fires on an empty block attached to an `if`/`for`/`while`/`catch`/… (a block with even a comment is exempt, since the comment explains the intent), anchored at the `{`. Method and lambda bodies are out of scope (an empty method body is S1186), but an accessor or local-function body is flagged. Native port of SonarAnalyzer.CSharp S108 — purely syntactic, report-only.

### `S1104` — Fields should not have public accessibility.

*Port of SonarAnalyzer.CSharp S1104 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1104/)

A `public` mutable field lets any caller change an object's state with no validation, no change notification, and no way to evolve the implementation — expose a `public` property (auto-implemented if needed) and keep the field `private`. Matches a `public` field that is neither `const` nor `readonly` (a `public static` mutable field is still flagged), in classes and structs; one diagnostic per declaration. Native port of Sonar S1104 — purely syntactic, report-only.

### `S1110` — Redundant pairs of parentheses should be removed.

*Port of SonarAnalyzer.CSharp S1110 · Redundancy · has an autofix* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1110/)

A pair of parentheses that directly wraps another pair (`((x))`) is redundant. Fires on the outer pair, anchored at its `(`; the fix removes it, leaving the inner `(x)`. Narrower than StyleCop SA1119 / Roslynator RCS1032 (which flag single whole-value parentheses). Native port of SonarAnalyzer.CSharp S1110.

### `S1116` — Empty statements should be removed.

*Port of SonarAnalyzer.CSharp S1116 · Redundancy · has an autofix* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1116/)

A standalone `;` is a no-op that usually slipped in by accident (a doubled semicolon or a stray one). Fires on an empty statement, anchored at the `;`; the fix deletes it. The semicolons in a `for (;;)` header are not empty statements and are left alone. Native port of SonarAnalyzer.CSharp S1116.

### `S1118` — Utility classes should not have public constructors.

*Port of SonarAnalyzer.CSharp S1118 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1118/)

A class whose members are all `static` is never instantiated, so a public (or implicit) constructor is misleading — give it a `private` constructor (or mark the class `static`). Fires on a non-static, base-less class with only static/const members that has an accessible constructor, anchored at the class name (implicit ctor) or the explicit public ctor. For a `partial` type, eligibility is judged on the union of every part (same-file or across files); a fully-implicit ctor fires once per declaring part, while an explicit public ctor fires only from the part that declares it. Native port of Sonar S1118 — purely syntactic, report-only.

### `S112` — General exceptions should never be thrown.

*Port of SonarAnalyzer.CSharp S112 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-112/)

Throwing a base exception type (`Exception`, `ApplicationException`, `SystemException`) forces callers to catch everything to handle anything. Fires on `throw new <general-exception>(…)`, anchored at the object creation; a custom `…Exception` is exempt. Native port of SonarAnalyzer.CSharp S112 — report-only (choosing a specific type is a human decision).

### `S1123` — 'Obsolete' attributes should include explanations.

*Port of SonarAnalyzer.CSharp S1123 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1123/)

An `[Obsolete]` attribute should explain what to use instead. Fires when `ObsoleteAttribute` is applied with no arguments or with an empty `""` message; a named-argument-only form is not flagged (that is CA1041's concern). Native port of SonarAnalyzer.CSharp S1123 — purely syntactic, report-only.

### `S1125` — Boolean literals should not be redundant.

*Port of SonarAnalyzer.CSharp S1125 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1125/)

A boolean literal used where it adds nothing is redundant: `!true` / `!false` (a `!` on a literal), and `cond ? true : false` (a conditional whose branches are both boolean literals, which collapses to `cond` or `!cond`). Anchored at the redundant literal. Native port of SonarAnalyzer.CSharp S1125 — purely syntactic, report-only (the rewrite varies by shape and overlaps RCS1068).

### `S1133` — Deprecated code should be removed.

*Port of SonarAnalyzer.CSharp S1133 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1133/)

An `[Obsolete]` API is meant to be temporary; left in place it accretes indefinitely. S1133 flags every `[Obsolete]` as a reminder to schedule its removal once callers have migrated. Fires on every Obsolete attribute application (unlike CA1041, which only flags a missing message). Native port of Sonar S1133 — purely syntactic, report-only.

### `S1135` — Track uses of TODO tags.

*Port of SonarAnalyzer.CSharp S1135 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1135/)

A `TODO` marks work left undone; surfacing every one keeps that backlog visible instead of lost in the source. Fires on each whole-word `TODO` tag in a line or block comment (`TODOlist` is not a tag). Native port of Sonar S1135 — purely syntactic, report-only.

### `S1144` — Unused private types or members should be removed.

*Port of SonarAnalyzer.CSharp S1144 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1144/)

A private member, or a member effectively private through a private enclosing type, that is never referenced is dead. Fires on unused private fields, events, methods, and nested types/enums (a field or event at its declaration start, a method or nested type at its name). The Sonar counterpart of RCS1213, but also covering nested types and containment-based privacy, and an unused local function (scoped to its enclosing body). Native port of SonarAnalyzer.CSharp S1144 — syntactic, report-only.

### `S1172` — Unused method parameters should be removed.

*Port of SonarAnalyzer.CSharp S1172 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1172/)

An unused parameter of an effectively-private method or a local function is dead — its removal cannot break a public contract. Fires per parameter whose name appears nowhere in the body, anchored at the parameter; `public`/`protected`/`internal` methods, no-body signatures, and lambda parameters are left alone. The Sonar counterpart of Roslynator RCS1163. Native port of SonarAnalyzer.CSharp S1172 — syntactic (intra-procedure), report-only.

### `S1186` — Methods should not be empty.

*Port of SonarAnalyzer.CSharp S1186 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1186/)

An empty method body usually signals an unfinished implementation or a missed deletion. Either complete it, throw to signal it is unsupported, or — if the no-op is deliberate — add a comment explaining why. Fires on a method or local function with an empty `{ }` body; a body with a comment, a `virtual` method (a valid extension point), and an interface implementation (explicit, or implicit — resolved via the BCL type-fact table + project index, issue #145; an empty body is often a legitimate no-op implementation of a contract member) are exempt, while an `override` still fires. Native port of Sonar S1186 — report-only.

### `S1199` — Nested code blocks should not be used.

*Port of SonarAnalyzer.CSharp S1199 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1199)

A free-standing `{ … }` block inside a method body adds a scope for no structural reason — extract a method instead. Fires on a `block` whose parent is itself a `block`, anchored at the opening brace. Native port of SonarAnalyzer.CSharp S1199 — report-only.

### `S1206` — Equals(object) and GetHashCode() should be overridden in pairs.

*Port of SonarAnalyzer.CSharp S1206 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1206/)

Overriding only one of `Equals(object)` / `GetHashCode()` breaks the equality contract — two equal objects must share a hash code. Fires on a `class` or `struct` that overrides exactly one of the pair, anchored at the type name; for a `partial` type both overrides are unioned across every part and the diagnostic then fires at every declaring part independently, not just once. Native port of SonarAnalyzer.CSharp S1206 — report-only.

### `S121` — Control structures should use curly braces.

*Port of SonarAnalyzer.CSharp S121 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-121)

A control structure with an un-braced body is bug-prone — adding a second line silently leaves it outside the branch. Fires on an `if`/`while`/`for`/`foreach`/`do` whose body is a single statement, anchored at the keyword. Native port of SonarAnalyzer.CSharp S121 — report-only.

### `S1210` — 'IComparable' implementations should override comparison operators.

*Port of SonarAnalyzer.CSharp S1210 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1210)

The Sonar twin of CA1036: a type implementing `IComparable<T>` that defines none of the comparison operators is flagged at its name. Shares CA1036's detector. Native port of SonarAnalyzer.CSharp S1210 — report-only.

### `S1226` — Method parameters should not be reassigned.

*Port of SonarAnalyzer.CSharp S1226 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1226)

Reassigning a parameter hides the original argument and confuses readers — a local variable states the intent. Fires on the first assignment whose target is one of the method's parameters, anchored at the assignment. Native port of SonarAnalyzer.CSharp S1226 — report-only.

### `S1244` — Floating point numbers should not be tested for equality.

*Port of SonarAnalyzer.CSharp S1244 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1244)

Rounding makes exact `==` / `!=` on floating-point values unreliable. Fires on such a comparison where one operand is a floating-point literal (`1.0`, `0.5f`), anchored at the operator. Native port of SonarAnalyzer.CSharp S1244 — report-only.

### `S125` — Commented-out code should be removed.

*Port of SonarAnalyzer.CSharp S125 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-125/)

A comment whose content looks like real code (every non-empty line ends in `;`, `{`, or `}`) is dead weight — either delete it or explain why it's kept. Native port of Sonar S125: fires on an isolated `//` line comment (not part of a multi-line `//` group — a real known limitation, see module docs) or a self-contained `/* … */` block comment. Report-only.

### `S1264` — A `while` loop should be used instead of a `for` loop with only a condition.

*Port of SonarAnalyzer.CSharp S1264 · Maintainability · has an autofix* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1264/)

A `for` loop with neither an initializer nor an incrementor — only a condition — is just a `while` loop written awkwardly. Fires at the `for` keyword; the fix replaces the `for (…)` header with `while (<condition>)` (an absent condition becomes `while (true)`), leaving the body untouched. Native port of SonarAnalyzer.CSharp S1264.

### `S1301` — switch statements should have at least 3 case clauses.

*Port of SonarAnalyzer.CSharp S1301 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1301)

A `switch` with fewer than three sections reads more clearly as an `if`. Fires on a `switch` statement with fewer than 3 sections, and on a `switch` expression (`x switch { … }`) with fewer than 3 arms (which reads as a ternary), both anchored at the `switch` keyword. Native port of SonarAnalyzer.CSharp S1301 — report-only.

### `S131` — switch statements should have default clauses.

*Port of SonarAnalyzer.CSharp S131 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-131)

A `switch` without a `default` silently ignores unhandled values. Fires on a `switch` statement with no `default` section, anchored at the `switch` keyword. Native port of SonarAnalyzer.CSharp S131 — report-only.

### `S134` — Control flow statements should not be nested too deeply.

*Port of SonarAnalyzer.CSharp S134 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-134/)

Nesting `if`/`for`/`foreach`/`while`/`do`/`switch` more than three deep is hard to follow. Fires at the statement that first exceeds the limit (the fourth nesting level), anchored at its keyword. Native port of SonarAnalyzer.CSharp S134 — report-only.

### `S138` — Functions should not have too many lines of code.

*Port of SonarAnalyzer.CSharp S138 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-138/)

A method longer than 80 lines (the Sonar default) should be split into smaller methods. Fires on a method whose body spans more than 80 lines, anchored at the method name (the Sonar counterpart of Meziantou MA0051, which uses 60). Native port of SonarAnalyzer.CSharp S138 — report-only.

### `S1481` — Unused local variables should be removed.

*Port of SonarAnalyzer.CSharp S1481 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1481/)

A local variable that is declared but never read is dead weight. Fires per declarator whose name appears nowhere else in its enclosing function (method/accessor/constructor/local-function/lambda body), anchored at the name. Native port of SonarAnalyzer.CSharp S1481 — syntactic (intra-procedure), report-only (removing the declaration could drop a side-effecting initializer).

### `S1656` — Variables should not be self-assigned.

*Port of SonarAnalyzer.CSharp S1656 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1656/)

An assignment whose two sides are the same expression (`a = a;`) does nothing and is almost always a typo for assigning a different value. Fires at the assignment when the operator is `=` and the left and right sides are textually identical (ignoring whitespace). Native port of SonarAnalyzer.CSharp S1656 — report-only (the intended right-hand side is a human decision).

### `S1696` — NullReferenceException should not be caught.

*Port of SonarAnalyzer.CSharp S1696 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1696/)

Catching `NullReferenceException` hides a programming bug — a missing null check — instead of fixing it. Fires on a `catch` clause whose declared type is `NullReferenceException` (bare or `System.`-qualified), anchored at the type. Native port of SonarAnalyzer.CSharp S1696 — report-only.

### `S1751` — Loops with at most one iteration should be refactored.

*Port of SonarAnalyzer.CSharp S1751 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1751/)

A loop whose body unconditionally hits a jump on the first pass runs at most once — it is really an `if`. Fires when an unconditional `break`, `continue`, `return` or `throw` is a direct statement of the loop body, anchored at the jump keyword. A jump guarded by a conditional, or a second conditional `continue` that lets the loop iterate, does not fire. Native port of SonarAnalyzer.CSharp S1751 — report-only.

### `S1764` — Identical expressions should not be used on both sides of a binary operator.

*Port of SonarAnalyzer.CSharp S1764 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1764/)

A binary expression with the same operand on both sides (`a == a`, `a && a`, `a - a`) is a bug or dead code. Fires at the right-hand operand when the two sides are textually identical and the operator is one where duplication is meaningless — comparison, logical/bitwise, or `-` / `/` / `%`. `+`, `*`, and shifts are excluded (`x + x`, `x * x` are legitimate). Native port of SonarAnalyzer.CSharp S1764 — report-only.

### `S1848` — Objects should not be created to be dropped immediately without being used.

*Port of SonarAnalyzer.CSharp S1848 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1848/)

`new Foo();` as a standalone statement builds an object and throws it away — the result is never assigned, returned, or used. Fires at the creation when an `object_creation_expression` is used as a discarded statement. The exception subset is also flagged by S3984. Native port of SonarAnalyzer.CSharp S1848 — report-only (a needed side effect or a forgotten use is the author's call).

### `S1858` — Redundant ToString() on a string.

*Port of SonarAnalyzer.CSharp S1858 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1858)

Calling `.ToString()` on a value that is already a `string` does nothing. The Sonar twin of RCS1097/MA0044 — shares the same detector (a no-argument `.ToString()` whose receiver is a `string`-typed parameter/local of the enclosing function) and anchors at the `.` before `ToString`. Native port of SonarAnalyzer S1858 — report-only.

### `S1862` — Related `if`/`else if` conditions should not be the same.

*Port of SonarAnalyzer.CSharp S1862 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1862/)

In an `if`/`else if` chain, a condition identical to an earlier one in the same chain is dead — the earlier branch always wins. Fires at the duplicate condition (compared ignoring whitespace), naming the earlier branch's line. Native port of SonarAnalyzer.CSharp S1862 — report-only.

### `S1871` — Two `switch` sections should not have the same implementation.

*Port of SonarAnalyzer.CSharp S1871 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1871/)

Two `switch` sections, or two branches of an if/else-if chain, with the same statements should be merged (or one changed) — the duplication is usually a copy-paste bug. The `switch` form fires at the duplicate section's `case` label; the if form (added to S1871 in SonarAnalyzer 10) fires at the duplicate branch, deferring to S3923 when a closed chain's branches are all identical. Bodies are compared ignoring whitespace. Native port of SonarAnalyzer.CSharp S1871 — report-only.

### `S1940` — Boolean checks should not be inverted.

*Port of SonarAnalyzer.CSharp S1940 · Style · has an autofix* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1940/)

Negating a comparison (`!(a == b)`, `!(a < b)`) is clearer written with the opposite operator (`a != b`, `a >= b`). Fires at the `!` of a `!(<comparison>)` over `== != < <= > >=`; the fix substitutes the opposite operator, preserving operand order. Native port of SonarAnalyzer.CSharp S1940.

### `S2094` — Classes should not be empty.

*Port of SonarAnalyzer.CSharp S2094 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2094/)

An empty class/record adds no behaviour and is usually dead code or an unfinished idea. Fires on a `class` or `record` with an empty body, anchored at its name. Exempt: a `partial` declaration, one carrying an attribute, one inside a `#if`/`#elif`/`#else` region, a name ending in `Command`/`Event`/`Message`/`Query` (or exactly `AssemblyDoc`/`NamespaceDoc`), a primary constructor with at least one parameter (a zero-arg primary ctor does NOT exempt), and a base list that (via the project declaration index and BCL type-fact table, issues #129/#144) implements more than one type, an unqualified generic type, a resolvable interface, a known-abstract BCL class, or transitively reaches `System.Attribute`/`System.Exception` — an unresolvable base stays silent rather than guess. Native port of SonarAnalyzer.CSharp S2094 (report-only — whether to delete, fill in, or convert to an interface is a human decision).

### `S2123` — Value is uselessly incremented.

*Port of SonarAnalyzer.CSharp S2123 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2123/)

`i = i++;` assigns the old value of `i` back to `i` — the post-increment is discarded, so the statement does nothing (almost always a bug). Fires on `x = x++` / `x = x--` where the operand matches the assignment target, anchored at the post-increment. Native port of SonarAnalyzer.CSharp S2123 — report-only.

### `S2156` — Sealed classes should not have protected members.

*Port of SonarAnalyzer.CSharp S2156 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2156/)

A `sealed` class cannot be inherited, so a `protected` member is meaningless — nothing can ever access it as protected. Fires on a non-`override` `protected` method or property of a `sealed` class, anchored at the `protected` modifier. The Sonar twin of CA1047 (which anchors at the member name). Native port of SonarAnalyzer S2156 — report-only.

### `S2178` — Short-circuit logic should be used in boolean contexts.

*Port of SonarAnalyzer.CSharp S2178 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2178/)

Using the non-short-circuiting `&` / `|` on boolean operands evaluates both sides unconditionally; `&&` / `||` are almost always intended. Fires on a `&` or `|` binary expression whose operands are boolean, anchored at the operator. The Roslynator twin (RCS1233) shares its detector. Native port of SonarAnalyzer.CSharp S2178 — report-only.

### `S2190` — Recursion should not be infinite.

*Port of SonarAnalyzer.CSharp S2190 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2190/)

A `set` accessor that assigns its own property recurses without end. The Sonar twin of CA2011: same detector, but fires at the accessor rather than the assignment. Native port of SonarAnalyzer.CSharp S2190 — report-only.

### `S2201` — Return values from side-effect-free functions should not be ignored.

*Port of SonarAnalyzer.CSharp S2201 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2201/)

Discarding the value a pure `string` method (or `ToString`) returns means the call did nothing. Fires on such a bare call statement, anchored at the statement. Shares CA1806's pure-method set and additionally covers `ToString`. Native port of SonarAnalyzer.CSharp S2201 — report-only.

### `S2221` — Exception should not be caught when not required.

*Port of SonarAnalyzer.CSharp S2221 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2221)

Catching `Exception` swallows every failure indiscriminately. The Sonar twin of CA1031 — a bare `catch` or one catching `Exception` / `SystemException`, anchored at the exception type (or the `catch` keyword for a bare catch). Native port of SonarAnalyzer.CSharp S2221 — report-only.

### `S2223` — Non-constant static fields should not be visible.

*Port of SonarAnalyzer.CSharp S2223 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2223/)

A non-`private` `static` mutable field is global shared state any caller can reassign, with no thread-safety or invariants. Fires per declarator on a static field that is neither `const` nor `readonly` and has any non-`private` accessibility — wider than CA2211 / MA0069 (which stop at externally visible), so it also catches `internal` and `private protected`. Native port of SonarAnalyzer.CSharp S2223 — report-only (tighten visibility vs add `const`/`readonly` is a human decision).

### `S2275` — Composite format strings should not exceed the argument count.

*Port of SonarAnalyzer.CSharp S2275 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2275/)

A `string.Format` whose highest `{N}` index exceeds the supplied arguments throws at run time. Fires on such a call, anchored at the invocation. The Sonar twin of CA2241. Native port of SonarAnalyzer.CSharp S2275 — report-only.

### `S2292` — Trivial properties should be auto-implemented.

*Port of SonarAnalyzer.CSharp S2292 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2292)

A property whose `get`/`set` only read and write one backing field (block or expression-bodied) should be an auto-property. The Sonar twin of RCS1085, anchored at the property name; unlike RCS1085 it leaves a property with an accessor access modifier (`private set`) alone. Native port of SonarAnalyzer.CSharp S2292 — report-only.

### `S2326` — Unused type parameters should be removed.

*Port of SonarAnalyzer.CSharp S2326 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2326/)

A generic type parameter that is never referenced inside the declaration it belongs to is dead weight and should be removed. Fires once per unused parameter, anchored at the parameter name. A name is considered used if it appears anywhere in the owning declaration outside its own type parameter list. Native port of SonarAnalyzer.CSharp S2326 — purely syntactic, report-only.

### `S2333` — Redundant 'partial' modifier should be removed.

*Port of SonarAnalyzer.CSharp S2333 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2333)

Flags redundant modifiers: a `partial` modifier on a type with a single declaration in the file (the Sonar twin of RCS1043, anchored at `partial`; `partial` is meaningful across files, which a single-file parse cannot see), and a `checked`/`unchecked` block with no statements (the keyword guards no arithmetic, anchored at the keyword — the general no-overflow case needs types and stays in `--deep`). Native port of SonarAnalyzer.CSharp S2333 — report-only.

### `S2342` — Enum name violates the naming convention.

*Port of SonarAnalyzer.CSharp S2342 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2342)

Enum names should be PascalCase, and a `[Flags]` enum should additionally be plural (end in `s`) since it names a set of values. Fires on an enum whose name fails its applicable regex (`^([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?$`, plus a trailing `s` for `[Flags]`), anchored at the name. Native port of SonarAnalyzer S2342 — report-only (renames ripple into callers).

### `S2344` — Enumeration type names should not have 'Flags' or 'Enum' suffixes.

*Port of SonarAnalyzer.CSharp S2344 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2344/)

An `enum` named `…Enum` or `…Flags` repeats what the declaration already says. Fires on such an enum, anchored at its name (the Sonar, enum-specific counterpart of CA1711). Native port of SonarAnalyzer.CSharp S2344 (report-only — a rename is not a safe syntactic rewrite).

### `S2360` — Optional parameters should not be used.

*Port of SonarAnalyzer.CSharp S2360 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2360)

Optional parameters are baked into the call site at compile time, so changing a default silently breaks already-compiled callers — overloads are safer. Fires per parameter that declares a default value, anchored at the `=`; an override or an interface implementation (explicit, or implicit — resolved via the BCL type-fact table + project index, issue #152) is exempt, since its optional-parameter shape is not a free choice. Native port of SonarAnalyzer.CSharp S2360 — report-only.

### `S2368` — Public methods should not have multidimensional array parameters.

*Port of SonarAnalyzer.CSharp S2368 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2368)

Multidimensional array parameters are awkward for callers to construct. Fires on an externally visible (`public`/`protected`) method that declares a parameter with a multidimensional array type, anchored at the method name. Shares CA1814's multidimensional-array check. Native port of SonarAnalyzer.CSharp S2368 — report-only.

### `S2372` — Exceptions should not be thrown from property getters.

*Port of SonarAnalyzer.CSharp S2372 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2372/)

A property read should not throw — callers do not expect it. Fires on a `throw new <X>` inside a property getter where `X` is not a getter-allowed exception (`InvalidOperationException`/`NotSupportedException`/`NotImplementedException`), anchored at the `throw`. Native port of SonarAnalyzer.CSharp S2372 (Microsoft twin: CA1065) — report-only.

### `S2376` — Write-only properties should not be used.

*Port of SonarAnalyzer.CSharp S2376 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2376)

A property with a setter but no getter cannot be read, which is confusing — a `SetX` method states the intent better. Fires on a property that declares a `set` accessor and no `get` accessor, anchored at the property name. Native port of SonarAnalyzer.CSharp S2376 — report-only.

### `S2386` — Mutable field should not be 'public static'.

*Port of SonarAnalyzer.CSharp S2386 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2386/)

A `public static` field of an array or mutable-collection type is global shared state any caller can mutate. Fires (anchored at the type) on a `public static` (non-`const`) field whose type is a mutable collection (readonly or not) or a non-`readonly` array. `readonly` arrays, and `protected`/`internal`/`private`/instance fields, are exempt. Native port of SonarAnalyzer.CSharp S2386 — report-only.

### `S2436` — Types and methods should not have too many generic parameters.

*Port of SonarAnalyzer.CSharp S2436 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2436/)

A type or method with more than two generic parameters (the Sonar default) is hard to use. Fires on such a declaration, anchored at its name. Native port of SonarAnalyzer.CSharp S2436 — report-only.

### `S2486` — Generic exceptions should not be ignored.

*Port of SonarAnalyzer.CSharp S2486 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2486/)

An empty `catch` block silently swallows an exception, hiding failures. Fires on a `catch` whose body is empty, anchored at the `catch` keyword; a body with even a comment is exempt (the comment explains why it is safe to ignore), as is any `catch` that rethrows or does work, or that carries a `when (...)` exception filter (the filter itself is the handling decision). Native port of SonarAnalyzer.CSharp S2486 — purely syntactic, report-only.

### `S2551` — Shared resources should not be used for locking.

*Port of SonarAnalyzer.CSharp S2551 · Concurrency · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2551)

The Sonar twin of CA2002: locking on `this`, a `Type`, or a string is flagged at the locked expression. Shares CA2002's detector. Native port of SonarAnalyzer.CSharp S2551 — report-only.

### `S2692` — 'IndexOf' checks should not be for positive numbers.

*Port of SonarAnalyzer.CSharp S2692 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2692)

`s.IndexOf(x) > 0` skips index 0, so a match at the very start is missed; the check is almost always meant to be `>= 0` (or `Contains`). Fires on an `IndexOf(...) > 0` comparison, anchored at the `>` operator. Shares CA2249's `IndexOf`-call detector. Native port of SonarAnalyzer.CSharp S2692 — report-only.

### `S2737` — 'catch' clauses should do more than rethrow.

*Port of SonarAnalyzer.CSharp S2737 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2737/)

A `catch` whose body is only a bare `throw;` is pointless — deleting the `try`/`catch` would propagate the exception identically. Fires at the `catch` keyword on such a clause; a `catch` with a `when` filter, or one that runs any logic before rethrowing, is not flagged. Native port of SonarAnalyzer.CSharp S2737 — purely syntactic, report-only.

### `S2743` — Static fields should not be used in generic types.

*Port of SonarAnalyzer.CSharp S2743 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2743)

A static field of a generic type is not shared across different closed constructions (`Box<int>` and `Box<string>` each get their own copy), a common surprise. Fires on a `static`, non-`const` field declared in a generic type, anchored at the field name (any visibility). Native port of SonarAnalyzer.CSharp S2743 — report-only.

### `S2761` — Doubled prefix operators `!` and `~` should not be used.

*Port of SonarAnalyzer.CSharp S2761 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2761/)

Applying `!` or `~` twice in a row cancels out (`!!x` is `x`, `~~x` is `x`), so the doubled operator is a typo or dead code. Fires at the outer operator when a `!`/`~` prefix's operand is another prefix using the same operator (the Sonar counterpart of the `!!x` case of RCS1068). Native port of SonarAnalyzer.CSharp S2761 — report-only.

### `S2933` — Fields only assigned in the constructor should be readonly.

*Port of SonarAnalyzer.CSharp S2933 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2933/)

A private field assigned only in its initialiser or a constructor should be `readonly`. The Sonar twin of Roslynator RCS1169, but S2933 requires the field to actually be assigned (a field never assigned anywhere is left alone), and is anchored at the field name. Native port of SonarAnalyzer.CSharp S2933 — report-only.

### `S2971` — IEnumerable LINQs should be simplified.

*Port of SonarAnalyzer.CSharp S2971 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2971/)

A `Where(predicate).Count()` (or `.Any()`, `.First()`, …) chain can drop the `Where` and move the predicate into the terminal (`Count(predicate)`). Fires when a single-predicate `Where(…)` is immediately followed by a no-argument terminal with a predicate overload, anchored at the `Where`. Native port of SonarAnalyzer.CSharp S2971 — report-only.

### `S3010` — Static fields should not be updated in constructors.

*Port of SonarAnalyzer.CSharp S3010 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3010)

Assigning a static field from an instance constructor runs every time an instance is created, almost never intended — initialize it statically instead. Fires on an assignment / `++` / `--` whose target is a static field of the enclosing type inside an instance constructor, anchored at the field reference (a static constructor is exempt). Native port of SonarAnalyzer.CSharp S3010 — report-only.

### `S3052` — Members should not be initialized to default values.

*Port of SonarAnalyzer.CSharp S3052 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3052)

The runtime zero-initializes every field, so initializing one to its default value (`0` / `false` / `null`) is redundant. The Sonar twin of CA1805 — fires per field declarator or auto-property whose initializer is the default, anchored at the `=`. Native port of SonarAnalyzer.CSharp S3052 — report-only.

### `S3168` — 'async' methods should not return 'void'.

*Port of SonarAnalyzer.CSharp S3168 · Concurrency · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3168/)

An `async void` method cannot be awaited and its exceptions crash the process instead of propagating; return `Task` instead. Fires on an `async void` method, anchored at the `void` return type (the Sonar counterpart of VSTHRD100, which anchors at the name). Native port of SonarAnalyzer.CSharp S3168 — report-only (changing the return type ripples into callers).

### `S3237` — Use the 'value' contextual keyword in a set accessor.

*Port of SonarAnalyzer.CSharp S3237 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3237/)

A `set`/`init` accessor — or an `add`/`remove` event accessor — that never reads the `value` contextual keyword ignores the assigned value/handler, almost always a bug. Fires on such an accessor (with a body), anchored at it. Native port of SonarAnalyzer.CSharp S3237 — report-only.

### `S3240` — Use the simplest possible condition syntax (`??` / `?:`).

*Port of SonarAnalyzer.CSharp S3240 · Style · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3240/)

Two shapes collapse to a simpler conditional: a null-check ternary (`x != null ? x : y`) becomes `x ?? y` (anchored at the conditional expression), and an `if (c) … else …` whose branches each reduce to a single `return <expr>;` or an assignment to the same target becomes one ternary (anchored at the `if`; an `else if` chain does not fire). Native port of SonarAnalyzer.CSharp S3240 — report-only (the `??` fix is carried by the Roslynator twin RCS1084; the `?:` collapse rewrites multiple statements).

### `S3247` — Use the result of the 'is' check instead of casting again.

*Port of SonarAnalyzer.CSharp S3247 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3247)

The Sonar twin of RCS1220: when `if (x is T)` then casts `(T)x`, the cast repeats the type check and should use a pattern variable. Fires on the casting statement. Shares RCS1220's detector. Native port of SonarAnalyzer.CSharp S3247 — report-only.

### `S3253` — Constructor and destructor declarations should not be redundant.

*Port of SonarAnalyzer.CSharp S3253 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3253)

A redundant constructor or destructor adds noise. Fires on an empty destructor, an empty static constructor, a public parameterless empty instance constructor that is the type's only instance constructor (the compiler already provides that default), anchored at the member, and a zero-arg primary constructor (`class C()` / `record R()`), anchored at the `(` — unless a base-list initializer or an explicit constructor makes the primary one load-bearing. Native port of SonarAnalyzer.CSharp S3253 — report-only.

### `S3257` — Array type is redundant when an initializer is present.

*Port of SonarAnalyzer.CSharp S3257 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3257/)

When an array creation has a non-empty element initializer, the explicit element type is redundant — `new int[] { 1, 2 }` can be `new[] { 1, 2 }`. Fires on such an array creation, anchored at the array type; an empty initializer, a sized array, a jagged array's outer type (`new int[][] { … }`), or a multidimensional array (`new int[,] { … }`) is left alone. Native port of SonarAnalyzer S3257 — report-only.

### `S3260` — Non-derived private classes should be sealed.

*Port of SonarAnalyzer.CSharp S3260 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3260)

A `private` class can only be derived from by a type nested in the same outer type; if nothing in the file derives from it, it should be `sealed`. Fires on a `private` class that is not already `sealed`/`static`/`abstract` with no derived type in the compilation unit, anchored at the type name. Native port of SonarAnalyzer.CSharp S3260 — report-only.

### `S3261` — Namespaces should not be empty.

*Port of SonarAnalyzer.CSharp S3261 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3261)

A `namespace` block that declares no members serves no purpose and only adds nesting. Fires at the `namespace` keyword of a block-form namespace whose body holds no declarations. Native port of SonarAnalyzer.CSharp S3261 — report-only.

### `S3264` — Events should be invoked.

*Port of SonarAnalyzer.CSharp S3264 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3264)

A field-like event that is declared but never read anywhere in its declaring type can only be subscribed to, never raised — it is dead. Fires at the event name. A read (an invocation, a `?.Invoke`, a null check, or passing the event as a value) leaves it alone, as do `abstract`/`extern` events and interface events. Native port of SonarAnalyzer.CSharp S3264 — report-only.

### `S3267` — Loops should be simplified with LINQ expressions.

*Port of SonarAnalyzer.CSharp S3267 · Style · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3267/)

A `foreach` whose body is nothing but a single `if` with no `else` (`foreach (x in xs) { if (cond) …; }`) is a `Where` filter written the long way and reads more clearly as `xs.Where(x => cond)`. Fires (report-only) at the iterated collection, matching the oracle: the `if`'s then-branch content is unrestricted, but the `if` must be the lone body statement (a sibling statement, an inverted `continue` guard, or an `else` all suppress it) and only `foreach` loops fire. Native port of SonarAnalyzer.CSharp S3267.

### `S3358` — Ternary operators should not be nested.

*Port of SonarAnalyzer.CSharp S3358 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3358/)

A ternary `?:` nested inside another ternary is hard to read; extract it into a separate statement. Fires on a conditional expression contained — through parentheses or other expression wrappers — within another conditional expression, anchored at the inner one. Native port of SonarAnalyzer.CSharp S3358 — purely syntactic, report-only.

### `S3376` — Attribute, EventArgs, and Exception type names should end with the type being extended.

*Port of SonarAnalyzer.CSharp S3376 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3376/)

A type that derives from `Exception`, `EventArgs`, or `Attribute` should carry that suffix so its role is clear. Fires on such a type whose name lacks the suffix, anchored at the name (the Sonar counterpart of CA1710's class-inheritance cases). Native port of SonarAnalyzer.CSharp S3376 (report-only — a rename is not a safe syntactic rewrite).

### `S3400` — Methods should not return constants.

*Port of SonarAnalyzer.CSharp S3400 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3400/)

A parameterless method that just returns a literal is better expressed as a constant. Fires on a method whose body is a single `return <literal>;` or an expression body `=> <literal>`, anchored at the method name. Bare `default` counts, except when the return type is a type parameter (`T M<T>() => default`), where the value depends on `T`. An interface implementation (explicit, or implicit — resolved via the BCL type-fact table + project index, issue #145; a contract member cannot collapse to a constant) is exempt, as are `virtual`/`override` methods. Native port of SonarAnalyzer.CSharp S3400 — report-only.

### `S3442` — Abstract classes should not have public constructors.

*Port of SonarAnalyzer.CSharp S3442 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3442/)

The Sonar twin of CA1012: fires on each `public`/`internal` constructor of an `abstract` class, anchored at the constructor declaration. Shares CA1012's detector. Native port of SonarAnalyzer.CSharp S3442 — report-only.

### `S3445` — Exceptions should not be explicitly rethrown.

*Port of SonarAnalyzer.CSharp S3445 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3445/)

`throw ex;` inside a `catch` resets the exception's stack trace; a bare `throw;` preserves it. Fires when the thrown identifier is the enclosing `catch`'s exception variable, anchored at the `throw` (the Sonar member of the rethrow cluster with CA2200 / MA0027 / RCS1044). Native port of SonarAnalyzer.CSharp S3445 — report-only (a syntactic pass cannot prove the variable was not reassigned).

### `S3459` — Unassigned fields should be removed.

*Port of SonarAnalyzer.CSharp S3459 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3459/)

A private field that is read but never assigned always holds its default value — usually a bug. Fires on a `private` (non-`const`) field with no initializer whose name is read at least once and never assigned (`=`, `++`/`--`, `ref`/`out`) within its type, anchored at the field name. A field that is never read is S1144's domain. Native port of SonarAnalyzer S3459 — report-only.

### `S3626` — Jump statements should not be redundant.

*Port of SonarAnalyzer.CSharp S3626 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3626/)

A jump that does not change control flow is noise: a value-less `return;` as the last statement of a method-like body, a `continue;` as the last statement of a loop body, a trailing labelled jump (`Done: return;`), or a `goto` whose target label is the very next statement. Fires at the jump keyword. A guarded early `return;` in a nested block, or a `return` with a value, is not redundant and is left alone. Native port of SonarAnalyzer.CSharp S3626 — report-only (the labelled-jump and redundant-`goto` cases are S3626-only; the Roslynator twin RCS1134 does not flag them).

### `S3776` — Cognitive Complexity of a method should not be too high.

*Port of SonarAnalyzer.CSharp S3776 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3776/)

Deeply nested and branching control flow is hard to follow. Fires when a method's cognitive complexity (if/for/foreach/while/do/switch/catch score 1 + current nesting depth; else/else-if score a flat +1; entering any qualifying body raises nesting for what's inside) exceeds 15, anchored at the method name. Native port of SonarAnalyzer.CSharp S3776 — milestone 1 (see module docs for what's deferred) — report-only.

### `S3871` — Exception types should be public.

*Port of SonarAnalyzer.CSharp S3871 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3871)

A class deriving from `Exception` that is not `public` cannot be caught by name across assemblies, so callers cannot handle it specifically. The Sonar twin of CA1064, anchored at the type name. Native port of SonarAnalyzer.CSharp S3871 — report-only.

### `S3875` — operator== should not be overloaded on reference types.

*Port of SonarAnalyzer.CSharp S3875 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3875)

Overloading `==` on a class is surprising: most callers expect reference equality. Fires on an `operator ==` declared in a class, anchored at the operator symbol. A class implementing `IEquatable<T>` is exempt, as are structs. Native port of SonarAnalyzer.CSharp S3875 — report-only.

### `S3878` — Arrays should not be created for params parameters.

*Port of SonarAnalyzer.CSharp S3878 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3878/)

Wrapping the elements of a `params` argument in an explicit array is needless. Fires on a `new T[] { … }` that is the last argument of a call, anchored at the array creation. The `params` parameter is assumed from the trailing-argument position. Native port of SonarAnalyzer.CSharp S3878 — report-only.

### `S3897` — Classes that provide Equals(T) should implement IEquatable<T>.

*Port of SonarAnalyzer.CSharp S3897 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3897)

A class with a strongly-typed, self-referential `Equals(Self)` method should declare `IEquatable<Self>` so generic code uses it, anchored at the type name; a typed `Equals` for any OTHER type is not a candidate. For a `partial` type the condition is unioned across every part and the diagnostic then fires at every declaring part independently, not just once. Native port of SonarAnalyzer.CSharp S3897 — report-only.

### `S3903` — Types should be defined in named namespaces.

*Port of SonarAnalyzer.CSharp S3903 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3903/)

A top-level type should live in a named namespace, not the global scope. Fires on a type with no namespace ancestor, anchored at its name (the Sonar twin of CA1050). Native port of SonarAnalyzer.CSharp S3903 — report-only.

### `S3923` — All branches in a conditional structure should not have the same implementation.

*Port of SonarAnalyzer.CSharp S3923 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3923/)

When every branch of an `if`/`else` chain — or both sides of a `?:` — does exactly the same thing, the condition is pointless and usually signals a bug. Fires on a ternary with identical branches (anchored at the expression) and on an `if`/`else` chain with a final `else` whose every branch body is identical (anchored at the `if`). Native port of SonarAnalyzer.CSharp S3923 — purely syntactic, report-only.

### `S3928` — Parameter names used in ArgumentException constructors should match an existing one.

*Port of SonarAnalyzer.CSharp S3928 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3928)

Fires on an argument-exception object creation that is mis-constructed: a `paramName` string that is not an enclosing parameter, or a base `ArgumentException` created with no message at all. Anchored at the object creation. Shares CA2208's classification. Native port of SonarAnalyzer.CSharp S3928 — report-only.

### `S3962` — 'static readonly' constant should be 'const'.

*Port of SonarAnalyzer.CSharp S3962 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3962/)

A `static readonly` field of a const-able type initialized to a compile-time constant should be `const` — it is evaluated at compile time and needs no per-type storage. Fires per declarator on a `static readonly` (non-`const`) field of a numeric/`bool`/`char`/`string` type whose initializer is a literal or a constant expression. Native port of SonarAnalyzer.CSharp S3962 — report-only.

### `S3963` — Static fields should be initialized inline.

*Port of SonarAnalyzer.CSharp S3963 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3963/)

A static constructor that does nothing but assign static fields could initialize them inline and be removed. Fires on a `static` constructor whose body is only assignment statements (a block of assignments, or an expression-bodied single assignment), anchored at the constructor name. Native port of SonarAnalyzer.CSharp S3963 — report-only.

### `S3981` — Collection size and array length comparisons should make sense.

*Port of SonarAnalyzer.CSharp S3981 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3981/)

A collection's `.Count` or an array/string's `.Length` is never negative, so comparing it to `0` with `>=` / `<` (or the reversed `0 <=` / `0 >`) is constant: `list.Count >= 0` is always `true`, `array.Length < 0` is always `false`. Fires at the comparison; meaningful comparisons (`> 0`, `<= 0`, `== 0`) are left alone. The `Count(xs)` invocation form is semantic and not ported. Native port of SonarAnalyzer S3981 — report-only.

### `S3984` — Exceptions should not be created without being thrown.

*Port of SonarAnalyzer.CSharp S3984 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3984/)

`new SomeException();` as a standalone statement builds an exception and throws it away — almost always a forgotten `throw`. Fires at the creation when an `object_creation_expression` of an `…Exception` type is used as a discarded statement (not thrown, assigned, or returned). Native port of SonarAnalyzer.CSharp S3984 — report-only (throw it or delete it is the author's call).

### `S3993` — Custom attributes should be marked with AttributeUsageAttribute.

*Port of SonarAnalyzer.CSharp S3993 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3993)

A custom attribute type should declare `[AttributeUsage]` so its valid targets are explicit. The Sonar twin of CA1018 — a non-abstract class deriving from `Attribute` with no `[AttributeUsage]`, anchored at the type name. Native port of SonarAnalyzer.CSharp S3993 — report-only.

### `S4022` — Enum with a narrower-than-Int32 storage type.

*Port of SonarAnalyzer.CSharp S4022 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-4022)

`Int32` is the natural enum storage; a `byte`/`sbyte`/`short`/`ushort` underlying type saves nothing (the enum is still word-sized) while inviting interop surprises. Fires on an enum whose explicit underlying type is narrower than `int`, anchored at the name. `uint`/`long`/`ulong` are deferred (they may be needed for the member values). Native port of SonarAnalyzer S4022 — report-only.

### `S4023` — Interfaces should not be empty.

*Port of SonarAnalyzer.CSharp S4023 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-4023)

An empty interface defines no contract and is usually a misused marker. The Sonar twin of CA1040 — an `interface` with no members, anchored at the interface name. Native port of SonarAnalyzer.CSharp S4023 — report-only.

### `S4035` — Classes implementing IEquatable<T> should be sealed.

*Port of SonarAnalyzer.CSharp S4035 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-4035)

An unsealed class implementing `IEquatable<T>` can be subclassed, and the subclass's `Equals` will not be used by the typed comparison — a latent correctness trap. Fires on a non-`sealed`, non-`abstract`, non-`static` class with an `IEquatable` base, anchored at the type name. For a `partial` type, both the `IEquatable<T>` base and the sealed/abstract/static modifier are unioned across every part (same-file or across files); the diagnostic then fires at every declaring part independently, not just once. Native port of SonarAnalyzer.CSharp S4035 — report-only.

### `S4039` — Interface methods should be callable by derived types.

*Port of SonarAnalyzer.CSharp S4039 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-4039/)

Sonar's twin of Microsoft CA1033: an externally-visible, unsealed class that implements an interface member explicitly hides it from derived classes. Fires at the explicit member's name unless the type is sealed, a struct, not externally visible, or exposes the functionality through a non-explicit member of the same name. Native port of SonarAnalyzer.CSharp S4039 — report-only.

### `S4050` — Operators should be overloaded consistently.

*Port of SonarAnalyzer.CSharp S4050 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-4050/)

A type that overloads an operator takes on value semantics, so it should also provide the full equality set — `operator ==`, `operator !=`, `Object.Equals`, and `Object.GetHashCode` — or its operators and equality will disagree. Fires at the type name when an operator is overloaded but any of the four is missing. Native port of SonarAnalyzer.CSharp S4050 — report-only.

### `S4136` — Method overloads should be grouped together.

*Port of SonarAnalyzer.CSharp S4136 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-4136/)

When a type declares several methods with the same name, keeping them adjacent lets a reader take in the whole overload set at once. Fires once, at the first overload of a name whose declarations are separated by another member. Native port of SonarAnalyzer.CSharp S4136 — report-only (reordering members is a structural edit).

### `S4144` — Methods should not have identical implementations.

*Port of SonarAnalyzer.CSharp S4144 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-4144/)

Two methods of the same type with the exact same block body (ignoring whitespace) are usually a copy-paste bug — one should delegate to the other. Fires on the later method, anchored at its name, naming the earlier identical method. Only block bodies with two or more statements are compared; trivial one-statement bodies are left alone. Native port of SonarAnalyzer.CSharp S4144 — purely syntactic, report-only.

### `S4487` — Unread private fields should be removed.

*Port of SonarAnalyzer.CSharp S4487 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-4487/)

A private field that is written but whose value is never read is dead state. Fires on a `private` (non-`const`) field assigned at least once (`=`, `++`/`--`, `ref`/`out`) but never read within its type, anchored at the field name. A field initialized in a constructor or finalizer is lifecycle/setup state and is left alone. The write-only twin of S3459; a field neither read nor written is S1144's domain. Native port of SonarAnalyzer S4487 — report-only.

### `S4524` — default clauses should be first or last.

*Port of SonarAnalyzer.CSharp S4524 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-4524)

A `default:` buried among `case`s is easy to miss; convention puts it first or last. Fires on a `switch` whose `default` section is neither first nor last, anchored at the `default` label. Native port of SonarAnalyzer.CSharp S4524 — report-only.

### `S4663` — Comments should not be empty.

*Port of SonarAnalyzer.CSharp S4663 · Redundancy · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-4663/)

The Sonar twin of SA1120: an empty `//` line comment carries no information. Contiguous `//` lines merge into one comment block — a block with any text is fine, and a fully empty block is reported once, at its first line. Native port of SonarAnalyzer.CSharp S4663 — report-only.

### `S6354` — Ambient DateTime.Now/UtcNow/Today access.

*Port of SonarAnalyzer.CSharp S6354 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-6354)

`DateTime.Now`/`UtcNow`/`Today` (and the `DateTimeOffset` equivalents) read the ambient clock, so code using them cannot be unit-tested deterministically — inject a time provider instead. Fires on a member access whose member is `Now`/`UtcNow`/`Today` and whose receiver's last segment is `DateTime`/`DateTimeOffset`, anchored at the access. Native port of SonarAnalyzer S6354 — report-only.

### `S818` — Literal suffixes should be upper case.

*Port of SonarAnalyzer.CSharp S818 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-818/)

A lower-case `l` suffix is easily mistaken for a `1`. Fires on an integer literal whose suffix is exactly a standalone lower-case `l` (`1l`), anchored at the `l`; combined suffixes (`1ul`, `1lu`) and the `u`/`U`/`L` forms are not flagged. Native port of SonarAnalyzer.CSharp S818 — report-only.

### `S907` — goto statement should not be used.

*Port of SonarAnalyzer.CSharp S907 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-907)

`goto` jumps obscure control flow and are almost always replaceable with structured constructs. Fires on a `goto` statement, anchored at the keyword. Native port of SonarAnalyzer.CSharp S907 — report-only.

### `SA1000` — Keywords should be spaced correctly.

*Port of StyleCop.Analyzers SA1000 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1000.md)

A target-typed `new(...)` should NOT have a space after the keyword — `new(` is idiomatic (the same form IDE0090/`dotnet format` recommend), so `new (` with a space is what fires, anchored at the keyword; the fix removes the space. `new Foo()` (a mandatory space before the type name) and the implicit array `new[]` are unaffected. Also fires on a tuple-deconstruction `var(a, b)` written with no space after `var`, and on a C# 11 checked-operator declaration's `checked`/`unchecked` keyword written WITH a space (that direction is flagged the same way as `new` — the keyword should NOT be followed by a space there). Native port of StyleCop.Analyzers SA1000 (the keyword-spacing family); the target-typed-`new` polarity matches modern StyleCop.Analyzers (1.2.0-beta+), not the pinned corpus oracle's pre-C#9 1.1.118 (issue #174's reopen transcript).

### `SA1001` — Commas should be spaced correctly.

*Port of StyleCop.Analyzers SA1001 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1001.md)

A comma should follow its preceding token directly, with no space before it, and be followed by a space (narrow exemptions apply — `Func<,>`, `int[,]`, end of line; a string-interpolation alignment comma is the reverse, wanting no following space). Fires on a `,` preceded by whitespace, or one not correctly spaced after, anchored at the comma; comment and string interiors are skipped. Native port of StyleCop.Analyzers SA1001; the fix deletes or inserts a single space, except a line-leading comma's preceding-space violation, which stays report-only (a same-line delete can't fix it).

### `SA1002` — Semicolons should be spaced correctly.

*Port of StyleCop.Analyzers SA1002 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1002.md)

A semicolon should follow its preceding token directly and be followed by a space. Fires on a `;` preceded by whitespace, or one not followed by whitespace (with narrow exemptions — an abbreviated `for` clause, `Statement();;`, end of line), anchored at the `;`; comment and string interiors are skipped. Native port of StyleCop.Analyzers SA1002; the fix deletes or inserts a single space.

### `SA1004` — Documentation line should begin with a space.

*Port of StyleCop.Analyzers SA1004 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1004.md)

An XML documentation line should have a space after the `///` marker (`/// text`, not `///text`). Fires on a `///` line whose first character after the marker is non-whitespace, anchored at that character; a `////` line and an empty `///` are left alone. Native port of StyleCop SA1004 — purely syntactic, report-only.

### `SA1005` — Single line comments should begin with a space.

*Port of StyleCop.Analyzers SA1005 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1005.md)

A `//` comment whose text begins immediately after the slashes (`//text`) is harder to read than `// text`. Fires on such a comment; the fix inserts one space. A documentation comment (`///`), the commented-out-code marker (`////`), an empty `//`, and an already-spaced comment are exempt. Native port of StyleCop.Analyzers SA1005.

### `SA1008` — Opening parenthesis should not be preceded by a space.

*Port of StyleCop.Analyzers SA1008 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1008.md)

An opening parenthesis should not be preceded by whitespace unless the token before it is a control-flow/expression keyword already governed by SA1000 (`if`, `for`, `new`, `nameof`, …), or the paren's own AST context demands a leading space (an `if`/`while`/… statement's condition, a `catch` filter's `when`, `var (a, b)` deconstruction, a parenthesized-lambda's parameter list, a parenthesized/tuple expression or cast not directly after a unary prefix operator/indexer `[`/adjacent cast, or a tuple TYPE after a comma/keyword/in a variable-or-return-type position). An opening parenthesis should never be followed by whitespace unless it ends its line. Full AST-position dispatch — the same character before `(` can mean different things (`Foo<int>(x)`'s generic-call `>(` stays silent; `a >(b)`'s binary-comparison `>(` fires) depending on what node actually contains the paren. Native port of StyleCop.Analyzers SA1008.

### `SA1009` — Closing parenthesis should not be preceded by a space.

*Port of StyleCop.Analyzers SA1009 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1009.md)

A space between the last token and its closing parenthesis is noise. Fires on a `)` directly preceded by whitespace (including a `)` wrapped onto its own line) or followed by whitespace before a hugging token (`;`/`,`/`.`/`)`/`]`/`++`/`--`, or a primary constructor's base-list `:`), anchored at the `)`; comment text, string-literal text, and interior lines of a multi-line string are skipped. Native port of StyleCop.Analyzers SA1009; the fix deletes the offending same-line whitespace run — except a wrapped-to-its-own-line `)`'s preceding violation, which stays report-only (a same-line delete can't reproduce upstream's own "move onto the previous line" fix).

### `SA1010` — Opening square brackets should be spaced correctly.

*Port of StyleCop.Analyzers SA1010 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1010.md)

A `[` that opens an array access or array-creation rank should follow its preceding token directly, and should not itself be followed by a space. Fires on a `[` preceded by whitespace mid-line, or one (outside an attribute list) followed by whitespace mid-line, anchored at the `[`; a `[` that begins its line (an attribute list) and comment/string interiors are skipped. Native port of StyleCop.Analyzers SA1010; the fix deletes the whitespace run on the offending side.

### `SA1011` — Closing square brackets should be spaced correctly.

*Port of StyleCop.Analyzers SA1011 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1011.md)

A `]` should follow its preceding token directly and generally be followed by a space, with per-context exemptions (adjacent `]`/`(`/`,`/`;`/`.`/`[`/`)`, a generic type argument's `>`, a null-conditional `?`, an interpolation hole's `}`/`:`). Fires on a `]` preceded by whitespace, or one not followed by whitespace where a space is required (or followed by one where it is disallowed), anchored at the `]`; comment and string interiors are skipped. Native port of StyleCop.Analyzers SA1011; the fix deletes or inserts a single space.

### `SA1012` — Opening braces should be spaced correctly.

*Port of StyleCop.Analyzers SA1012 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1012.md)

The `{` of an object/collection initializer should be surrounded by spaces — `new T{ … }` should be `new T { … }`. Fires when an initializer's `{` is not preceded or not followed by whitespace, anchored at the `{`; block braces are not inspected. Native port of StyleCop.Analyzers SA1012; a single-side violation is fixed with a single space insert — both sides tight at once stays report-only (two separate edits).

### `SA1013` — Closing braces should be spaced correctly.

*Port of StyleCop.Analyzers SA1013 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1013.md)

The `}` of an object/collection initializer should be preceded by a space — `{ … 1}` should be `{ … 1 }`. Fires when an initializer's `}` is not preceded by whitespace, anchored at the `}`; block braces are not inspected. Native port of StyleCop.Analyzers SA1013; the fix inserts the missing space.

### `SA1014` — Opening generic brackets should not be preceded by a space.

*Port of StyleCop.Analyzers SA1014 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1014.md)

The `<` of a generic argument list should sit tight against the name (`List<int>`, not `List <int>`). Fires on a `<` preceded by whitespace on the same line, anchored at the `<`. The opening-bracket twin of StyleCop SA1015. Native port of StyleCop SA1014; the fix deletes the same-line whitespace run.

### `SA1015` — Closing generic brackets should be spaced correctly.

*Port of StyleCop.Analyzers SA1015 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1015.md)

The closing `>` of a type-argument or type-parameter list should not be preceded by a space, and its required following spacing depends on what comes next — tight neighbors like `(`/`.`/`,`/`[`/`;` and a case-pattern `:` want none, `)`/`>`/a nullable type's `?` allow either, everything else wants exactly one. Fires per that table, anchored at the `>`; a comparison `a > b` is never touched (only `type_argument_list`/`type_parameter_list` closes are inspected). Native port of StyleCop.Analyzers SA1015; the fix deletes or inserts a single space.

### `SA1016` — Opening attribute brackets should not be followed by a space.

*Port of StyleCop.Analyzers SA1016 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1016.md)

An attribute's opening `[` should sit tight against the attribute (`[Obsolete]`, not `[ Obsolete]`). Fires on a `[` followed by whitespace on the same line, anchored at the `[`. Native port of StyleCop SA1016; the fix deletes the same-line whitespace run.

### `SA1017` — Closing attribute brackets should not be preceded by a space.

*Port of StyleCop.Analyzers SA1017 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1017.md)

An attribute's closing `]` should sit tight against the attribute (`[Obsolete]`, not `[Obsolete ]`). Fires on a `]` preceded by whitespace on the same line, anchored at the `]`. Native port of StyleCop SA1017; the fix deletes the same-line whitespace run.

### `SA1018` — Nullable type symbol should not be preceded by a space.

*Port of StyleCop.Analyzers SA1018 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1018.md)

The `?` of a nullable type should sit tight against the type (`int?`, not `int ?`). Fires on a `?` preceded by whitespace on the same line, anchored at the `?`. Native port of StyleCop SA1018; the fix deletes the same-line whitespace run.

### `SA1019` — Member access symbols should be spaced correctly.

*Port of StyleCop.Analyzers SA1019 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1019.md)

A member-access `.` should sit directly against the expression it follows — `x .Length` should be `x.Length`. Fires when the `.` of a `member_access_expression` is separated from the preceding token by whitespace, anchored at the `.`. Native port of StyleCop.Analyzers SA1019; the fix deletes the same-line whitespace run.

### `SA1020` — Increment/decrement symbols should be spaced correctly.

*Port of StyleCop.Analyzers SA1020 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1020.md)

A postfix `++`/`--` should sit directly against its operand — `n ++` should be `n++`. Fires when the operator of a `postfix_unary_expression` is separated from its operand by whitespace, anchored at the operator. Native port of StyleCop.Analyzers SA1020; the fix deletes the same-line whitespace run.

### `SA1021` — Negative signs should be spaced correctly.

*Port of StyleCop.Analyzers SA1021 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1021.md)

A unary negative sign should sit directly against its operand — `- 1` should be `-1`. Fires on a `-` unary operator separated from its operand by whitespace, anchored at the sign; binary subtraction (`a - b`) is unaffected. Native port of StyleCop.Analyzers SA1021; the fix deletes the same-line whitespace run.

### `SA1022` — Positive signs should be spaced correctly.

*Port of StyleCop.Analyzers SA1022 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1022.md)

A unary positive sign should sit directly against its operand — `+ 1` should be `+1`. Fires on a `+` unary operator separated from its operand by whitespace, anchored at the sign; binary addition (`a + b`) is unaffected. The positive-sign twin of SA1021. Native port of StyleCop.Analyzers SA1022; the fix deletes the same-line whitespace run.

### `SA1024` — Colons should be spaced correctly.

*Port of StyleCop.Analyzers SA1024 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1024.md)

The spacing a colon wants depends on its context: a base-list/constructor-initializer/ type-parameter-constraint/ternary colon wants a space on both sides; a labeled statement's, a switch label's, or a named argument's colon wants no preceding space but a following one; a string-interpolation format clause's colon wants no preceding space and is not checked after. Fires per that table, anchored at the `:`. Native port of StyleCop.Analyzers SA1024; the fix inserts a missing space or deletes an excess preceding one, except a line-leading colon's preceding-space violation, which stays report-only.

### `SA1025` — Code should not contain multiple whitespace characters in a row.

*Port of StyleCop.Analyzers SA1025 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1025.md)

A run of two or more spaces inside code is collapsed noise. Fires at the first space of every such run, skipping leading indentation, trailing whitespace (SA1028), runs inside a string/character literal or comment, and the run aligning a trailing `//` comment after a statement terminator. Native port of StyleCop.Analyzers SA1025 (report-only — collapsing the run is a formatting rewrite, deferred).

### `SA1026` — The keyword 'new' should not be followed by a space.

*Port of StyleCop.Analyzers SA1026 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1026.md)

An implicitly-typed array creation should keep `new` tight against the `[` (`new[] { … }`, not `new [] { … }`). Fires on a `new` keyword separated from its `[` by whitespace on the same line, anchored at the `new`. Native port of StyleCop SA1026; the fix deletes the same-line whitespace run.

### `SA1027` — Tabs and spaces should be used correctly (no tabs by default).

*Port of StyleCop.Analyzers SA1027 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1027.md)

With StyleCop's default settings code is indented with spaces, not tabs. Fires on the first tab character of each line, skipping the interior of a multi-line string. Native port of StyleCop.Analyzers SA1027 (report-only — converting tabs to spaces is a formatting rewrite).

### `SA1028` — Code should not contain trailing whitespace.

*Port of StyleCop.Analyzers SA1028 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1028.md)

Whitespace at the end of a line is invisible noise that pollutes diffs. Fires on each line with trailing whitespace, anchored at the first trailing character; the interior lines of a multi-line string are skipped (their trailing spaces are part of the value). Native port of StyleCop.Analyzers SA1028 (report-only — trimming is a formatting rewrite, deferred).

### `SA1100` — Do not prefix calls with base unless a local override exists.

*Port of StyleCop.Analyzers SA1100 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1100.md)

`base.Member` is only meaningful when the current class overrides `Member`; otherwise it resolves to the same inherited member as `Member`, so the `base` prefix is noise. Fires on a `base.Member` access whose `Member` is not overridden in the enclosing class, anchored at `base`. Native port of StyleCop.Analyzers SA1100 — report-only.

### `SA1106` — Code should not contain empty statements.

*Port of StyleCop.Analyzers SA1106 · Redundancy · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1106.md)

A standalone `;` is a no-op that usually slipped in by accident. Fires on an empty statement, anchored at the `;`; the fix deletes it. The semicolons in a `for (;;)` header are not empty statements. The StyleCop counterpart of Sonar S1116. Native port of StyleCop.Analyzers SA1106.

### `SA1107` — Code should not contain multiple statements on one line.

*Port of StyleCop.Analyzers SA1107 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1107.md)

Putting more than one statement on a line hurts readability and complicates breakpoints. Fires on a statement that begins on the same line as the previous statement in the same block, anchored at the second statement. Native port of StyleCop.Analyzers SA1107 (report-only — splitting onto separate lines is a formatting rewrite).

### `SA1108` — Block statements should not contain embedded comments.

*Port of StyleCop.Analyzers SA1108 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1108.md)

A comment embedded in a control statement's header — between the `)` and the opening brace, e.g. `while (b) // note` — is easy to miss and should sit on its own line above the statement. Fires at the comment. Native port of StyleCop.Analyzers SA1108 — report-only.

### `SA1110` — Opening parenthesis should be on the declaration line.

*Port of StyleCop.Analyzers SA1110 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1110.md)

A method's parameter-list `(` should sit on the same line as the method name, not wrapped to the next line; the same rule applies to a call's own argument-list `(` against its callee expression. Fires when the `(` and the name/callee are on different lines, anchored at the `(`. Native port of StyleCop.Analyzers SA1110 — report-only.

### `SA1111` — Closing parenthesis should be on the line of the last parameter.

*Port of StyleCop.Analyzers SA1111 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1111.md)

When a parameter list spans multiple lines, its `)` should sit on the same line as the last parameter, not wrapped below. Fires when they are on different lines, anchored at the `)`. The empty-list case is SA1112. Native port of StyleCop.Analyzers SA1111 — report-only.

### `SA1112` — Closing parenthesis should be on the line of the opening parenthesis.

*Port of StyleCop.Analyzers SA1112 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1112.md)

An empty parameter list `()` should keep its `)` on the line of its `(` rather than wrapping it below. Fires when they are on different lines, anchored at the `)`. The non-empty case is SA1111. Native port of StyleCop.Analyzers SA1112 — report-only.

### `SA1113` — Comma should be on the same line as previous parameter.

*Port of StyleCop.Analyzers SA1113 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1113.md)

In a parameter or argument list split over several lines, a `,` should hug the end of the preceding item rather than lead the next line. Fires at a `,` whose preceding sibling ends on an earlier line. Native port of StyleCop.Analyzers SA1113 — report-only.

### `SA1114` — Parameter list should follow declaration.

*Port of StyleCop.Analyzers SA1114 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1114.md)

When a parameter list is split across lines, the first parameter should sit on the same line as the opening `(` or the line immediately after — not after a blank line. Fires at the first parameter when a blank line separates it from the `(`. Native port of StyleCop.Analyzers SA1114 — report-only.

### `SA1115` — Parameters should not be separated by blank lines.

*Port of StyleCop.Analyzers SA1115 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1115.md)

A blank line between two parameters breaks up the list. Fires on a parameter that is separated from the previous one by one or more blank lines, anchored at the parameter. Native port of StyleCop.Analyzers SA1115 — report-only.

### `SA1116` — Split parameters should begin on the line after the declaration.

*Port of StyleCop.Analyzers SA1116 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1116.md)

When a parameter (or argument) list spans multiple lines, the first item should start on the line after the opening `(`, not share the declaration's line. Fires on a multi-line list whose first item sits on the `(` line, anchored at that item. Native port of StyleCop.Analyzers SA1116 — report-only (the layout fix is the formatter's job).

### `SA1117` — Parameters should all be on the same line or each on its own line.

*Port of StyleCop.Analyzers SA1117 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1117.md)

A parameter (or argument) list that spans multiple lines must put every item on its own line; mixing (two on one line, the rest wrapped) is inconsistent. Fires on a multi-line list where some two items share a line, anchored at the first item that begins on a later line. Native port of StyleCop.Analyzers SA1117 — report-only (the layout fix is the formatter's job).

### `SA1119` — Statement should not use unnecessary parenthesis.

*Port of StyleCop.Analyzers SA1119 · Redundancy · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1119.md)

Parentheses that wrap an entire value add no meaning. Fires on a parenthesized expression whose parent is a value position — a `=` initializer or assignment right side, a `return`, an `=>` body (an expression-bodied member's or a lambda's), a call argument, a conditional branch, or another set of parentheses. Parentheses used as an operand of a larger expression (where they may affect precedence) are not flagged. The fix strips the parentheses (a nested chain collapses fully in one pass); it is withheld when stripping would immediately reveal a new atomic-operand finding, or when the parentheses contain a comment. Native port of StyleCop SA1119 — purely syntactic.

### `SA1120` — Comments should contain text.

*Port of StyleCop.Analyzers SA1120 · Redundancy · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1120.md)

An empty single-line comment (`//` with nothing after it) carries no information. Fires at the `//`; doc comments (`///`) and the commented-out-code marker (`////`) are left alone, as is a bare `//` separator with real comment text directly above and below it in the same comment block. Native port of StyleCop.Analyzers SA1120 — report-only.

### `SA1121` — Use built-in type alias.

*Port of StyleCop.Analyzers SA1121 · Maintainability · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1121.md)

A framework numeric/object/string type reads more naturally as its C# keyword (`System.Int32` → `int`). Fires on a fully-qualified `System.<framework-type>` with a keyword alias, anchored at the type; the fix replaces it with the keyword. Native port of StyleCop.Analyzers SA1121.

### `SA1122` — Use string.Empty for empty strings.

*Port of StyleCop.Analyzers SA1122 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1122.md)

An empty string literal (`""` / `@""`) is clearer and intent-revealing as `string.Empty`. Fires on an empty literal except where a compile-time constant is required and `string.Empty` is illegal — a `const` initializer, an attribute argument, a default parameter value, or a `case`/constant-pattern label. Native port of StyleCop SA1122; the fix replaces the literal with `string.Empty`.

### `SA1123` — Do not place regions within elements.

*Port of StyleCop.Analyzers SA1123 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1123.md)

A `#region` inside a code-element body (a method/accessor/constructor body) hides part of a single member and is especially harmful. Fires at such a `#region`, anchored at the `#` (the complement of SA1124, which covers regions elsewhere). Native port of StyleCop.Analyzers SA1123 — report-only (removing a region deletes the non-adjacent `#region` and `#endregion`, which a single-range fix cannot express).

### `SA1124` — Do not use regions.

*Port of StyleCop.Analyzers SA1124 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1124.md)

A `#region` hides code and discourages keeping types small and readable. Fires at a `#region` directive that is not inside a code-element body (a region within a method/accessor body is StyleCop's SA1123 instead), anchored at the `#`. Native port of StyleCop.Analyzers SA1124 — report-only (removing a region deletes the non-adjacent `#region` and `#endregion`, which a single-range fix cannot express).

### `SA1125` — Use shorthand for nullable types.

*Port of StyleCop.Analyzers SA1125 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1125.md)

A `Nullable<T>` generic type should be written with the `T?` shorthand. Fires on a `Nullable<T>` type, anchored at the full type expression; the fix rewrites it to `T?`. The StyleCop twin of Roslynator RCS1020. Native port of StyleCop.Analyzers SA1125.

### `SA1127` — Generic type constraints should be on their own line.

*Port of StyleCop.Analyzers SA1127 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1127.md)

A `where T : …` constraint clause should start its own line, not trail the declaration. Fires on a `type_parameter_constraints_clause` that shares its line with preceding code, anchored at the clause. Native port of StyleCop.Analyzers SA1127 (report-only — the move is a formatting rewrite).

### `SA1128` — Put constructor initializers on their own line.

*Port of StyleCop.Analyzers SA1128 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1128.md)

A `: base(…)` / `: this(…)` initializer should start its own line, not trail the constructor's parameter list. Fires on a `constructor_initializer` that shares its line with preceding code, anchored at the `:`. Native port of StyleCop.Analyzers SA1128 (report-only — the move is a formatting rewrite).

### `SA1129` — Do not use the default value-type constructor.

*Port of StyleCop.Analyzers SA1129 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1129.md)

`new int()` (a value type's parameterless constructor) is clearer as `default`. Fires on a parameterless `new T()` where `T` is a built-in value-type keyword, anchored at `new`. Reference types and user structs (which need a type model) are left alone. Native port of StyleCop.Analyzers SA1129 — report-only.

### `SA1130` — Use lambda syntax.

*Port of StyleCop.Analyzers SA1130 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1130.md)

`delegate { … }` reads more clearly as `() => { … }`. Fires on an anonymous method expression, anchored at the `delegate` keyword; the fix replaces it with `<params> => <body>`. The fix is withheld when the anonymous method has no parameter list — a parameterless `delegate { }` matches any delegate signature, while `() => { }` takes exactly zero parameters, so the rewrite needs the target delegate's semantic signature. Native port of StyleCop.Analyzers SA1130.

### `SA1131` — Constant values should be on the right-hand side of comparisons.

*Port of StyleCop.Analyzers SA1131 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1131.md)

A "Yoda" comparison with the constant on the left (`0 == x`, `5 > x`) reads less naturally than `x == 0` / `x < 5`. Fires at the left operand of a comparison (`== != < <= > >=`) whose left side is a literal and right side is not; the fix swaps the operands, flipping the operator for relational comparisons. Broader than Roslynator RCS1098 (which is equality-only). Native port of StyleCop.Analyzers SA1131.

### `SA1132` — Do not combine fields.

*Port of StyleCop.Analyzers SA1132 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1132.md)

Declaring several fields (or field-like events) in one statement (`int a, b;`) hides each behind a shared type and modifier list and complicates diffs and documentation. Fires on a field or event declaration with more than one declarator; declare each separately. Native port of StyleCop SA1132 — purely syntactic, report-only.

### `SA1133` — Do not combine attributes in one set of brackets.

*Port of StyleCop.Analyzers SA1133 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1133.md)

Each attribute should sit in its own `[…]` brackets (`[A] [B]`, not `[A, B]`). Fires on each attribute after the first in a combined attribute list, anchored at that attribute. Native port of StyleCop.Analyzers SA1133 — report-only (splitting the brackets is a layout rewrite).

### `SA1134` — Attributes should not share line.

*Port of StyleCop.Analyzers SA1134 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1134.md)

An attribute list should not share a line with another attribute list, or with the declaration/parameter it decorates. Fires once at the attribute list's own `[` when the preceding token ends on its line, or the following token starts on its line (unless that token belongs to another attribute list — that boundary is the following list's own concern). Parameter and type-parameter attribute lists are exempt. Native port of StyleCop.Analyzers SA1134 — report-only.

### `SA1136` — Enum values should be on separate lines.

*Port of StyleCop.Analyzers SA1136 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1136.md)

Placing multiple enum members on one line hurts readability and diffs. Fires on every enum member that shares a line with the preceding member, anchored at the member name. Native port of StyleCop.Analyzers SA1136 (report-only — the newline-insertion fix is indentation-sensitive and deferred).

### `SA1137` — Elements should have the same indentation.

*Port of StyleCop.Analyzers SA1137 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1137.md)

Sibling statements in a block should share one indentation. Fires on a statement that begins a line at a different column than the block's first statement, anchored at the start of that line. Native port of StyleCop.Analyzers SA1137 (report-only — re-indenting is a formatting rewrite).

### `SA1139` — Use literal suffix notation instead of casting.

*Port of StyleCop.Analyzers SA1139 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1139.md)

Casting a numeric literal to a typed numeric is clearer as a suffixed literal. Fires on a cast of a bare numeric literal to a type with a literal suffix (`long`→`L`, `ulong`→`UL`, `uint`→`U`, `float`→`F`, `double`→`D`, `decimal`→`M`), anchored at the cast; the fix appends the suffix (`(long)1`→`1L`). Native port of StyleCop.Analyzers SA1139.

### `SA1200` — Using directives should be placed within a namespace.

*Port of StyleCop.Analyzers SA1200 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1200.md)

In its default configuration StyleCop expects `using` directives inside the namespace declaration. Fires on a using directive at file scope (outside any namespace); a using inside a block namespace, or after a file-scoped `namespace …;`, is fine. Native port of StyleCop SA1200 — purely syntactic, report-only. (If your project prefers usings outside the namespace, leave this port disabled.)

### `SA1201` — Elements should appear in the correct order.

*Port of StyleCop.Analyzers SA1201 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1201.md)

Within a type, members should follow StyleCop's canonical kind order (fields, constructors, destructors, delegates, events, enums, interfaces, properties, indexers, operators, methods, then nested types). Fires when a member's kind ranks before the immediately preceding member's kind, anchored at the out-of-order member. Native port of StyleCop.Analyzers SA1201 (report-only — reordering members is a block rewrite, deferred).

### `SA1202` — Elements should be ordered by access.

*Port of StyleCop.Analyzers SA1202 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1202.md)

Within a group of same-kind members, more accessible members should be declared first: `public` > `internal` > `protected internal` > `protected` > `private protected` > `private`. Fires on a member that is more accessible than the nearest preceding member of the same kind and static-ness. Native port of StyleCop SA1202 — purely syntactic, report-only.

### `SA1203` — Constants should appear before fields.

*Port of StyleCop.Analyzers SA1203 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1203.md)

Within a type, `const` fields should be grouped ahead of ordinary fields so the immutable declarations read first. Fires on a `const` field whose immediately preceding field is non-`const`; consecutive constants are fine. Native port of StyleCop SA1203 — purely syntactic, report-only.

### `SA1204` — Static members should appear before non-static members.

*Port of StyleCop.Analyzers SA1204 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1204.md)

Within a type, static members of a given kind should be grouped before the instance members of that kind. Fires on a `static` member (including a static constructor) that appears after an instance member of the same kind, anchored at its name; `const` members are SA1203's concern. Native port of StyleCop.Analyzers SA1204 (report-only — reordering members is a block rewrite, deferred).

### `SA1205` — Partial elements should declare an access modifier.

*Port of StyleCop.Analyzers SA1205 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1205.md)

Each part of a partial type should state its access explicitly so the reader need not find another part. Fires on a `partial` type declaration with no access modifier, anchored at the type name. Native port of StyleCop.Analyzers SA1205 — report-only.

### `SA1206` — Declaration keywords should follow a standard ordering.

*Port of StyleCop.Analyzers SA1206 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1206.md)

Modifiers on a declaration should appear in a consistent order: accessibility (`public`/`private`/`protected`/`internal`) first, then `const`/`static`, then the rest (`readonly`, `async`, …). Fires on a modifier that should appear before one of its predecessors. Native port of StyleCop SA1206; the fix rewrites the modifier list in canonical order.

### `SA1207` — The keyword 'protected' should come before 'internal'.

*Port of StyleCop.Analyzers SA1207 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1207.md)

In the `protected internal` access combination the `protected` keyword is written first by convention. Fires on a declaration whose `internal` precedes `protected`, anchored at the `protected` keyword. Native port of StyleCop.Analyzers SA1207 — report-only.

### `SA1208` — System using directives should be placed before other usings.

*Port of StyleCop.Analyzers SA1208 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1208.md)

Keeping `System.*` using directives grouped first is the conventional ordering and makes the import block scannable. Fires on a plain `System`/`System.*` namespace using that appears after any other using — a non-`System` namespace using, a `using static`, or an alias using. Usings after a file-scoped namespace are skipped. Native port of StyleCop SA1208 — purely syntactic, report-only.

### `SA1209` — Using alias directives should be placed after all using namespace directives.

*Port of StyleCop.Analyzers SA1209 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1209.md)

The canonical using order is namespace directives, then `using static`, then `using X = …` aliases. Fires on an alias directive that has a plain namespace using after it, anchored at the alias. Usings after a file-scoped namespace are skipped. Native port of StyleCop SA1209 — purely syntactic, report-only.

### `SA1210` — Using directives should be ordered alphabetically by the namespaces.

*Port of StyleCop.Analyzers SA1210 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1210.md)

Within the System-first grouping, plain namespace using directives should be alphabetical. Fires on a using whose namespace sorts after the next using of the same group (System / non-System) in the same scope, anchored at the directive. `using static` and alias usings are separate groups. Native port of StyleCop SA1210 — purely syntactic, report-only.

### `SA1211` — Using alias directives should be ordered alphabetically by alias name.

*Port of StyleCop.Analyzers SA1211 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1211.md)

Alias usings (`using X = …;`) form their own group and should be alphabetical by alias. Fires on the first descent in that group, anchored at the later alias that should move earlier. Native port of StyleCop SA1211 — purely syntactic, report-only.

### `SA1212` — A get accessor should appear before a set accessor.

*Port of StyleCop.Analyzers SA1212 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1212.md)

Convention puts the getter first. Fires on a property or indexer whose `set` accessor is written before its `get`, anchored at the `set` accessor. Native port of StyleCop.Analyzers SA1212 — report-only.

### `SA1213` — Event accessors should follow order (add before remove).

*Port of StyleCop.Analyzers SA1213 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1213.md)

Convention puts the `add` accessor first. Fires on an event whose `remove` accessor is written before its `add`, anchored at the `remove` accessor. Native port of StyleCop.Analyzers SA1213 — report-only.

### `SA1214` — Readonly fields should appear before non-readonly fields.

*Port of StyleCop.Analyzers SA1214 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1214.md)

Grouping `readonly` fields ahead of mutable ones makes a type's immutable state read first. Fires on a `readonly` field whose immediately preceding field is non-`readonly` and non-`const` (a `const` ahead of a `readonly` is correctly ordered); consecutive readonly fields are fine. Native port of StyleCop SA1214 — purely syntactic, report-only.

### `SA1216` — Using static directives should be placed at the correct location.

*Port of StyleCop.Analyzers SA1216 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1216.md)

The canonical using order is namespace directives, then `using static`, then `using X = …` aliases. Fires on a `using static` directive with a plain namespace using after it or an alias using before it, anchored at the static directive. Usings after a file-scoped namespace are skipped. Native port of StyleCop SA1216 — purely syntactic, report-only.

### `SA1217` — Using static directives should be ordered alphabetically by type name.

*Port of StyleCop.Analyzers SA1217 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1217.md)

`using static …;` directives form their own group and should be alphabetical by full type name. Fires on the first descent in that group, anchored at the earlier directive that should move later. Native port of StyleCop SA1217 — purely syntactic, report-only.

### `SA1300` — Element should begin with an upper-case letter.

*Port of StyleCop.Analyzers SA1300 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1300.md)

A type or member element should be PascalCase. Fires on a type/method/property/event/delegate whose name begins with a lower-case letter, anchored at the name (fields/parameters/locals have their own rules). Unlike Sonar S100/S101, SA1300 checks only the first letter. Native port of StyleCop.Analyzers SA1300 — report-only (a rename ripples to call sites).

### `SA1302` — Interface names should begin with I.

*Port of StyleCop.Analyzers SA1302 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1302.md)

Interface names should carry a leading `I` so they read as interfaces. Fires on an interface whose name does not start with `I`, anchored at the name (narrower than CA1715, which also requires the second character to be upper-case). Native port of StyleCop.Analyzers SA1302 (report-only — a rename is not a safe syntactic rewrite).

### `SA1303` — Const field names should begin with an upper-case letter.

*Port of StyleCop.Analyzers SA1303 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1303.md)

A `const` field is named like a constant — PascalCase, starting upper-case. Fires per declarator of a `const` field whose name begins with a lower-case letter, anchored at the name. Native port of StyleCop.Analyzers SA1303 — report-only (a rename ripples to every use).

### `SA1306` — Field names should begin with a lower-case letter.

*Port of StyleCop.Analyzers SA1306 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1306.md)

Non-public fields should be named in camelCase. Fires per declarator on a non-`const` field that is neither `public` nor `internal` (private/protected) whose name begins with an upper-case letter (ignoring leading underscores). Accessible fields are SA1307's concern. Native port of StyleCop SA1306 — purely syntactic, report-only (a rename is not a safe syntactic fix).

### `SA1307` — Accessible fields should begin with an upper-case letter.

*Port of StyleCop.Analyzers SA1307 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1307.md)

A field visible outside its type (`public`/`protected`/`internal`) should be PascalCase. Fires per declarator of a non-private field whose name begins with a lower-case letter, anchored at the name. The accessible-field counterpart of SA1306. Native port of StyleCop.Analyzers SA1307 — report-only (a rename ripples to every use).

### `SA1308` — Field names should not be prefixed with `m_` or `s_`.

*Port of StyleCop.Analyzers SA1308 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1308.md)

A field name should not carry a Hungarian-style `m_` or `s_` prefix. Fires per declarator of a field whose name starts with `m_` or `s_`, anchored at the name. Native port of StyleCop.Analyzers SA1308 — report-only (a rename ripples to every use).

### `SA1309` — Field names should not begin with an underscore.

*Port of StyleCop.Analyzers SA1309 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1309.md)

StyleCop's default convention is that field names carry no leading underscore. Fires per declarator on any field whose name begins with `_`, regardless of accessibility. Native port of StyleCop SA1309 — purely syntactic, report-only (a rename is not a safe syntactic fix). Disable this port if your project's convention uses a leading underscore.

### `SA1310` — Field names should not contain an underscore.

*Port of StyleCop.Analyzers SA1310 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1310.md)

A field name should not contain an underscore. Fires per declarator of a field whose name contains `_`, anchored at the name — except where a sibling rule owns the case: a leading underscore is SA1309, and an `m_`/`s_` prefix is SA1308. So SA1310 covers an internal underscore those leave alone. Native port of StyleCop.Analyzers SA1310 — report-only.

### `SA1311` — Static readonly fields should begin with an upper-case letter.

*Port of StyleCop.Analyzers SA1311 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1311.md)

A `static readonly` field is effectively a named constant and should be PascalCase. Fires per declarator on a `static readonly` field whose name begins with a lower-case letter (ignoring leading underscores); `const` fields are SA1303's concern. The complement of SA1306. Native port of StyleCop.Analyzers SA1311 (report-only — a rename is not a safe syntactic rewrite).

### `SA1312` — Variable names should begin with a lower-case letter.

*Port of StyleCop.Analyzers SA1312 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1312.md)

A local variable (or `foreach` iteration variable) should be camelCase. Fires per declarator whose name begins with an upper-case letter or an underscore, anchored at the name. The local-variable mirror of SA1313. Native port of StyleCop.Analyzers SA1312 — report-only.

### `SA1313` — Parameter names should begin with a lower-case letter.

*Port of StyleCop.Analyzers SA1313 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1313.md)

A parameter name should be camelCase. Fires on a parameter whose name begins with an upper-case letter, anchored at the name (a leading underscore is left alone). Native port of StyleCop.Analyzers SA1313 — report-only (a rename ripples to named-argument call sites).

### `SA1314` — Type parameter names should begin with T.

*Port of StyleCop.Analyzers SA1314 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1314.md)

Generic type-parameter names should carry a leading `T` so they read as type parameters. Fires on a type parameter whose name does not start with `T`, anchored at the identifier (narrower than CA1715, which also requires the second character to be upper-case). Native port of StyleCop.Analyzers SA1314 (report-only — renames aren't a safe syntactic rewrite).

### `SA1400` — Access modifier should be declared.

*Port of StyleCop.Analyzers SA1400 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1400.md)

Relying on a C# default access modifier (`internal` for top-level types, `private` for members) hides intent. Fires on a type or member that omits an explicit access modifier, anchored at its name; interface members, static constructors and explicit interface implementations are exempt. Native port of StyleCop.Analyzers SA1400 (report-only — the correct default modifier depends on the element kind and nesting, so the fix is deferred).

### `SA1401` — Fields should be private.

*Port of StyleCop.Analyzers SA1401 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1401.md)

A non-private field in a class exposes implementation state across the type boundary; keep fields `private` and expose them through properties or methods. Fires per declarator on a class field with explicit non-private accessibility (`public` / `protected` / `internal`), excluding `const` and `static readonly` fields (well-established exceptions) and struct fields. Native port of StyleCop SA1401 — purely syntactic, report-only.

### `SA1402` — File may only contain a single type.

*Port of StyleCop.Analyzers SA1402 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1402.md)

Keeping one top-level class per file makes types easy to locate. Fires on every top-level class after the first in a file, anchored at the class name (with StyleCop's default setting only classes count — interfaces, structs, enums and records do not). Native port of StyleCop.Analyzers SA1402 (report-only — splitting into separate files is a human decision).

### `SA1403` — File may only contain a single namespace.

*Port of StyleCop.Analyzers SA1403 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1403.md)

Keeping one namespace per file makes types easy to locate. Fires on every top-level namespace after the first in a file, anchored at the namespace name. Native port of StyleCop.Analyzers SA1403 (report-only — splitting into separate files is a human decision).

### `SA1404` — Code analysis suppression should have justification.

*Port of StyleCop.Analyzers SA1404 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1404.md)

A `[SuppressMessage(...)]` should explain itself with a `Justification` argument. Fires on a `SuppressMessage` attribute that has no `Justification`, anchored at the attribute. Native port of StyleCop.Analyzers SA1404 — report-only.

### `SA1405` — Debug.Assert should provide message text.

*Port of StyleCop.Analyzers SA1405 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1405.md)

`Debug.Assert(condition)` with no message gives nothing useful when it fires. Fires on a `Debug.Assert` call with only the condition argument, anchored at the invocation. Native port of StyleCop.Analyzers SA1405 — report-only.

### `SA1406` — Debug.Fail should provide message text.

*Port of StyleCop.Analyzers SA1406 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1406.md)

`Debug.Fail` with no message (or a `null` message) gives nothing useful when it fires. Fires on a `Debug.Fail` call with no arguments or a `null` first argument, anchored at the invocation. Native port of StyleCop.Analyzers SA1406 — report-only.

### `SA1407` — Arithmetic expressions should declare precedence.

*Port of StyleCop.Analyzers SA1407 · Maintainability · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1407.md)

Mixing multiplicative (`*`/`/`/`%`) and additive (`+`/`-`) operators without parentheses relies on the reader knowing C#'s precedence. Fires on a multiplicative expression used as an operand of an additive expression; the fix parenthesizes it (`a + b * c` -> `a + (b * c)`). Native port of StyleCop SA1407.

### `SA1408` — Conditional expressions should declare precedence.

*Port of StyleCop.Analyzers SA1408 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1408.md)

Mixing `&&` and `||` without parentheses relies on the reader knowing `&&` binds tighter. Fires on an `&&` expression used as a direct operand of a `||`, anchored at the `&&` sub-expression. The StyleCop counterpart of Roslynator RCS1123's logical case. Native port of StyleCop.Analyzers SA1408 — report-only (RCS1123 carries the parenthesizing fix).

### `SA1410` — Remove delegate parenthesis when possible.

*Port of StyleCop.Analyzers SA1410 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1410.md)

An anonymous method with an empty parameter list (`delegate () { … }`) declares no parameters, so the `()` can be dropped (`delegate { … }`). Fires at the `(`; the fix removes the ` ()`. A non-empty list (`delegate (int x)`) or the bare `delegate { … }` (no list at all) is left alone. The fixable StyleCop twin of SonarAnalyzer S3257's anonymous-method form. Native port of StyleCop SA1410.

### `SA1411` — Attribute constructor should not use unnecessary parentheses.

*Port of StyleCop.Analyzers SA1411 · Redundancy · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1411.md)

An attribute applied with an empty argument list — `[Serializable()]` — carries `()` that adds nothing; write `[Serializable]`. Fires on an attribute whose argument list is present but empty. Native port of StyleCop SA1411; the fix removes the `()`.

### `SA1413` — Use trailing comma in multi-line initializers.

*Port of StyleCop.Analyzers SA1413 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1413.md)

When an object/collection/array/anonymous initializer or an `enum` member list spans multiple lines, ending the last element with a trailing comma keeps diffs minimal and reordering clean (the next added line is a pure insertion). Fires on the last element of a multi-line initializer that lacks a trailing comma; single-line initializers are exempt. Native port of StyleCop SA1413; the fix inserts the comma.

### `SA1500` — Braces for multi-line statements should not share a line.

*Port of StyleCop.Analyzers SA1500 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1500.md)

In Allman style, the opening brace of a multi-line block belongs on its own line. Fires on a multi-line block whose opening `{` shares its line with preceding code (`if (x) {`), anchored at the `{`. A single-line block is SA1501's concern. Native port of StyleCop.Analyzers SA1500. Fixable (issue #153): inserts a newline plus the header line's own indent right before the `{` (only the brace moves; the already-multi-line interior is untouched); withheld when the gap holds a comment or the preceding token ends in `}`.

### `SA1501` — Statement should not be on a single line.

*Port of StyleCop.Analyzers SA1501 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1501.md)

A statement block packed onto one line (`if (c) { Do(); }`) hides control flow and complicates diffs and breakpoints. Fires on a non-empty block written on a single line, anchored at the `{`; an empty single-line block is S108's concern, and method/accessor/lambda bodies are SA1502's — except a single-line local-function body, which is flagged whether empty or not. Native port of StyleCop.Analyzers SA1501. Fixable (issue #153): reflows the block (open `{` on its own line, each statement at indent+1, close `}` back at the header's indent) via the same helper RCS0021 uses, for byte-identical output whenever the two co-fire.

### `SA1502` — Element should not be on a single line.

*Port of StyleCop.Analyzers SA1502 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1502.md)

A declaration whose body is on a single line is harder to scan and to edit. Fires on a single-line type body, a method/constructor/operator/local-function block, or a property/indexer/event accessor list with a block-bodied accessor. Auto-properties and expression bodies (which have no block) are exempt. Native port of StyleCop SA1502. Fixable (issue #153) for the method/ctor/local-function `block` shape only, via the same helper RCS0021 uses (byte-identical whenever they co-fire); a single-line type/namespace/enum body or accessor list stays report-only (v1 scope cut, not an ambiguity — see the module doc).

### `SA1503` — Braces should not be omitted.

*Port of StyleCop.Analyzers SA1503 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1503.md)

Omitting braces on a control statement (`if (c) Foo();`) invites bugs when a second line is added later. Fires on a control statement whose body is a single un-braced statement, anchored at that statement; an `else if` continuation is exempt. Native port of StyleCop.Analyzers SA1503 (report-only — adding braces is a structural rewrite).

### `SA1505` — An opening brace should not be followed by a blank line.

*Port of StyleCop.Analyzers SA1505 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1505.md)

A blank line immediately after an opening `{` is wasted space. Fires when an opening brace ends its line and the following line is blank, anchored at the `{`. The fix deletes the whole contiguous blank run after the brace. Native port of StyleCop.Analyzers SA1505.

### `SA1506` — Element documentation headers should not be followed by blank line.

*Port of StyleCop.Analyzers SA1506 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1506.md)

A `///` documentation header should sit directly above the element it documents — a blank line between them detaches the docs. Fires at the blank line (column 1). Native port of StyleCop.Analyzers SA1506 — report-only.

### `SA1507` — Code should not contain multiple blank lines in a row.

*Port of StyleCop.Analyzers SA1507 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1507.md)

More than one blank line in a row is wasted vertical space. Fires once per run of two or more consecutive blank lines, at the first blank line; blank lines inside a multi-line string or block comment are content and are not counted. The fix collapses the run to a single blank line, keeping the one adjacent to the following content. Native port of StyleCop.Analyzers SA1507.

### `SA1508` — A closing brace should not be preceded by a blank line.

*Port of StyleCop.Analyzers SA1508 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1508.md)

A blank line immediately before a closing `}` is wasted space. Fires when a closing brace begins its line and the previous line is blank, anchored at the `}` (the mirror of SA1505). The fix deletes the whole contiguous blank run before the brace. Native port of StyleCop.Analyzers SA1508.

### `SA1509` — Opening braces should not be preceded by blank line.

*Port of StyleCop.Analyzers SA1509 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1509.md)

An opening `{` should hug the statement it belongs to — a blank line before it is noise. Fires at the `{` of a block preceded by a blank line. Native port of StyleCop.Analyzers SA1509 — report-only.

### `SA1510` — Chained statement blocks should not be preceded by blank line.

*Port of StyleCop.Analyzers SA1510 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1510.md)

An `else`, `catch`, or `finally` should hug the block it continues — a blank line before it breaks the chain visually. Fires at the keyword when a blank line precedes it. Native port of StyleCop.Analyzers SA1510 — report-only.

### `SA1511` — While-do footer should not be preceded by blank line.

*Port of StyleCop.Analyzers SA1511 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1511.md)

The `while` footer of a `do`/`while` loop should hug the loop body — a blank line before it breaks the construct visually. Fires at the `while` keyword when a blank line precedes it. Native port of StyleCop.Analyzers SA1511 — report-only.

### `SA1512` — Single-line comments should not be followed by a blank line.

*Port of StyleCop.Analyzers SA1512 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1512.md)

A `//` comment that documents the code below it should sit directly above that code. Fires on a single-line (`//`) comment immediately followed by a blank line, anchored at the comment; documentation (`///`) and block (`/* */`) comments are exempt. Native port of StyleCop.Analyzers SA1512 — report-only.

### `SA1513` — Closing brace should be followed by blank line.

*Port of StyleCop.Analyzers SA1513 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1513.md)

The closing `}` of a statement block should be set off from the code that follows by a blank line. Fires on a statement-block brace whose next line is code, anchored just past the `}`; a next line that is blank, the end of file, or begins with `}`, `else`, `catch`, `finally`, or `while` is exempt, and method/accessor/lambda bodies are out of scope. The fix inserts a blank line directly after the `}`'s own line. Native port of StyleCop.Analyzers SA1513.

### `SA1514` — Element documentation header should be preceded by a blank line.

*Port of StyleCop.Analyzers SA1514 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1514.md)

A `///` documentation header reads more clearly when separated from the preceding code by a blank line. Fires on a documentation header whose immediately preceding line is not blank, an opening brace, or a continuation line of the same header — a plain `//` comment directly above does not exempt it. The fix inserts a blank line directly above the header's own line. Native port of StyleCop.Analyzers SA1514.

### `SA1515` — Single-line comments should be preceded by a blank line.

*Port of StyleCop.Analyzers SA1515 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1515.md)

A `//` comment that introduces a new block of code should be set off from the statement above it by a blank line. Fires on a single-line (`//`) own-line comment whose immediately preceding line is code, anchored at the comment; comments after an opening brace, another comment, or a blank line are exempt, as are `///` docs and `/* */` blocks. Native port of StyleCop.Analyzers SA1515 — report-only.

### `SA1516` — Elements should be separated by a blank line.

*Port of StyleCop.Analyzers SA1516 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1516.md)

Separating adjacent type members with a blank line keeps a type readable. Fires on a member that directly follows the previous member with no blank line between them (the first member and any member with a blank line above are fine; a member's own leading comment counts as part of it). The fix inserts a blank line directly above the member when its row immediately follows the previous member's row; a case with an intervening comment line is left report-only. Native port of StyleCop SA1516 — purely syntactic.

### `SA1517` — Code should not contain blank lines at start of file.

*Port of StyleCop.Analyzers SA1517 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1517.md)

Leading blank lines at the top of a file are noise. Fires when a file begins with one or more blank lines (a newline before the first code), anchored at line 1. The fix deletes the leading blank lines. The mirror of SA1518. Native port of StyleCop.Analyzers SA1517.

### `SA1518` — Code should not contain blank lines at the end of the file.

*Port of StyleCop.Analyzers SA1518 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1518.md)

Trailing blank lines at the end of a file are noise. Fires when a file ends with one or more blank lines (two or more newlines after the last code), anchored just past the final code character; a single trailing newline is correct. The fix trims the trailing blank lines down to a single newline. Native port of StyleCop.Analyzers SA1518.

### `SA1519` — Braces should not be omitted from multi-line child statement.

*Port of StyleCop.Analyzers SA1519 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1519.md)

A control statement whose brace-less body spans more than one physical line reads like a block but isn't. Fires at the child statement (the StyleCop counterpart of Sonar's S121, which flags every brace-less body at its keyword). Native port of StyleCop.Analyzers SA1519 — report-only.

### `SA1520` — Use braces consistently.

*Port of StyleCop.Analyzers SA1520 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1520.md)

When one clause of an `if`/`else` uses braces and the other does not, the body reads inconsistently. Fires on the embedded (brace-less) clause of such a mismatched pair, anchored at that statement. Native port of StyleCop.Analyzers SA1520 — report-only.

### `SA1600` — Elements should be documented.

*Port of StyleCop.Analyzers SA1600 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1600.md)

An externally visible type or member should carry an XML `///` documentation comment so its contract is part of the API surface. Fires on an undocumented element that is effectively non-private (its own accessibility and every containing type are non-private); operators and static constructors are exempt and enum members are SA1602. Native port of StyleCop SA1600 — purely syntactic, report-only.

### `SA1601` — Partial elements should be documented.

*Port of StyleCop.Analyzers SA1601 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1601.md)

The partial-type counterpart of SA1600: a visible `partial` type whose own declaration lacks documentation (no `///`, or an empty `<summary>`) is flagged, anchored at the type name. Native port of StyleCop.Analyzers SA1601 — report-only.

### `SA1602` — Enumeration items should be documented.

*Port of StyleCop.Analyzers SA1602 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1602.md)

Each member of a publicly visible enum should carry an XML `///` documentation comment so its meaning is part of the API surface. Fires on an undocumented member of a non-private enum (a `private` enum's members are exempt). Native port of StyleCop SA1602 — purely syntactic, report-only.

### `SA1604` — Element documentation should have summary.

*Port of StyleCop.Analyzers SA1604 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1604.md)

A documented element should describe itself with a `<summary>`. Fires at the element's name when its doc comment has no `<summary>` (and is not an `<inheritdoc>`). Native port of StyleCop.Analyzers SA1604 — report-only.

### `SA1606` — Element documentation should have summary text.

*Port of StyleCop.Analyzers SA1606 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1606.md)

A documented element with an empty `<summary></summary>` documents nothing. Fires on a member requiring documentation whose summary is empty, anchored at the element name (it also reads as undocumented for SA1600; a `partial` type is SA1601's concern instead). Native port of StyleCop.Analyzers SA1606 — report-only.

### `SA1608` — Element documentation should not have default summary text.

*Port of StyleCop.Analyzers SA1608 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1608.md)

A `<summary>` left with the IDE-generated placeholder `Summary description for …` documents nothing real. Fires at the `<summary>` tag when the summary still carries that default text. Native port of StyleCop.Analyzers SA1608 — report-only.

### `SA1609` — Property documentation should have value.

*Port of StyleCop.Analyzers SA1609 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1609.md)

A documented property should describe what its value represents with a `<value>` element. Fires on a property that has a `<summary>` but no `<value>`, anchored at the property name (an empty `<value>` is SA1610's concern). Native port of StyleCop.Analyzers SA1609 — report-only.

### `SA1610` — Property documentation should have value text.

*Port of StyleCop.Analyzers SA1610 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1610.md)

A property whose doc comment has an empty `<value></value>` documents nothing about the value. Fires on that property, anchored at its name. The StyleCop counterpart of Roslynator RCS1228's value case. Native port of StyleCop.Analyzers SA1610 — report-only.

### `SA1611` — Element parameters should be documented.

*Port of StyleCop.Analyzers SA1611 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1611.md)

When a member carries an XML `///` documentation comment, each parameter should have a matching `<param name="…">` tag so the documentation is complete. Fires per parameter with no `<param>` tag on a documented member (an undocumented member is reported by SA1600 instead). Native port of StyleCop SA1611 — purely syntactic, report-only.

### `SA1612` — Element parameter documentation should match element parameters.

*Port of StyleCop.Analyzers SA1612 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1612.md)

The `<param>` elements should appear in the same order as the method's parameters. Fires on each documented parameter whose doc position does not match its signature position, anchored at the name in `<param name="…">` and reporting the expected position. Native port of StyleCop.Analyzers SA1612 — report-only.

### `SA1613` — Element parameter documentation should declare a parameter name.

*Port of StyleCop.Analyzers SA1613 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1613.md)

A `<param>` element must name the parameter it documents (`<param name="x">`). Fires at a `<param>` tag that carries no `name` attribute. Native port of StyleCop.Analyzers SA1613 — report-only.

### `SA1614` — Element parameter documentation should have text.

*Port of StyleCop.Analyzers SA1614 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1614.md)

A `<param name="x"></param>` with no text documents nothing. Fires on each empty `<param>` in a declaration's doc comment (method, constructor, indexer, delegate, …), anchored at the tag. The StyleCop twin of Roslynator RCS1228 (param case). Native port of StyleCop.Analyzers SA1614 — report-only.

### `SA1615` — Element return value should be documented.

*Port of StyleCop.Analyzers SA1615 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1615.md)

When a method that returns a value carries an XML `///` documentation comment, that comment should describe the return value with a `<returns>` tag. Fires on a documented, non-`void` method whose doc has no `<returns>` (a `void` method and an undocumented member are exempt). Native port of StyleCop SA1615 — purely syntactic, report-only.

### `SA1616` — Element return value documentation should have text.

*Port of StyleCop.Analyzers SA1616 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1616.md)

A `<returns></returns>` element with no text documents nothing. Fires on an empty `<returns>` in a declaration's doc comment (method, delegate, operator, …), anchored at the tag. The StyleCop twin of Roslynator RCS1228. Native port of StyleCop.Analyzers SA1616 — report-only.

### `SA1617` — Void return value should not be documented.

*Port of StyleCop.Analyzers SA1617 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1617.md)

A `void` method has nothing to return, so a `<returns>` element is wrong. Fires on a `void` method whose doc comment carries a `<returns>` element, anchored at the tag. Native port of StyleCop.Analyzers SA1617 — report-only.

### `SA1618` — Generic type parameters should be documented.

*Port of StyleCop.Analyzers SA1618 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1618.md)

When a generic member or type carries an XML `///` documentation comment, each type parameter should have a matching `<typeparam name="…">` tag. Fires per type parameter with no `<typeparam>` tag on a documented element (an undocumented element is reported by SA1600 instead). Native port of StyleCop SA1618 — purely syntactic, report-only.

### `SA1622` — Generic type parameter documentation should have text.

*Port of StyleCop.Analyzers SA1622 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1622.md)

A `<typeparam name="T"></typeparam>` with no text documents nothing. Fires on each empty `<typeparam>` in a generic declaration's doc comment (a generic type, method, or delegate), anchored at the tag. The StyleCop twin of Roslynator RCS1228 (typeparam case). Native port of StyleCop.Analyzers SA1622 — report-only.

### `SA1623` — Property documentation summary should match its accessors.

*Port of StyleCop.Analyzers SA1623 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1623.md)

A documented property's `<summary>` should start with the verb matching its accessors — `Gets` for read-only, `Sets` for write-only, `Gets or sets` for read-write — so the documentation stays consistent and informative. Fires at the property name when the summary begins with the wrong phrase. Native port of StyleCop.Analyzers SA1623 — report-only (rewriting prose is a human task).

### `SA1624` — Property summary should begin with 'Gets' when the setter is not visible.

*Port of StyleCop.Analyzers SA1624 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1624.md)

When a property's setter is less visible than the property (e.g. a `private set` on a public property), only the getter is public, so the summary should begin 'Gets', not 'Gets or sets'. Fires on such a property whose summary begins 'Gets or sets', anchored at the property name. Native port of StyleCop.Analyzers SA1624 — report-only.

### `SA1625` — Element documentation should not be copied and pasted.

*Port of StyleCop.Analyzers SA1625 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1625.md)

Two documentation elements with identical text are a copy-paste slip. Fires on an element whose non-empty text duplicates an earlier element's in the same doc comment, anchored at the duplicate's tag (single-line elements only). Native port of StyleCop.Analyzers SA1625 — report-only.

### `SA1626` — Single-line comments should not use documentation style slashes.

*Port of StyleCop.Analyzers SA1626 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1626.md)

A `///` comment embedded inside a statement block documents nothing — it is a regular comment written with the wrong slashes and should be `//`. Fires at such a comment. Native port of StyleCop.Analyzers SA1626 — report-only.

### `SA1629` — Documentation text should end with a period.

*Port of StyleCop.Analyzers SA1629 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1629.md)

The text of a documentation element (`<summary>`, `<param>`, `<returns>`, …) should end with a period. Fires once per element (single- or multi-line) whose reconstructed text does not end with a period (no other punctuation — `?`/`!`/`:` — counts as terminal), anchored immediately after the last non-whitespace character of the text, wherever it sits. Native port of StyleCop.Analyzers SA1629 — report-only.

### `SA1642` — Constructor summary documentation should begin with standard text.

*Port of StyleCop.Analyzers SA1642 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1642.md)

StyleCop requires a documented instance constructor's `<summary>` to read `Initializes a new instance of the <see cref="T"/> class.`. Fires when the summary does not begin with that standard, cref-anchored text, anchored at the `<summary>` element (a summary spanning multiple `///` lines is compared the same as a single-line one). Native port of StyleCop.Analyzers SA1642 — report-only (rewriting the prose is not a single-range fix).

### `SA1643` — Destructor summary documentation should begin with standard text.

*Port of StyleCop.Analyzers SA1643 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1643.md)

StyleCop requires a documented destructor's `<summary>` to read `Finalizes an instance of the <see cref="T"/> class.`. Fires when the summary does not begin with that standard, cref-anchored text, anchored at the `<summary>` element (a summary spanning multiple `///` lines is compared the same as a single-line one). The finalizer twin of SA1642. Native port of StyleCop.Analyzers SA1643 — report-only (rewriting the prose is not a single-range fix).

### `SA1651` — Do not use placeholder elements.

*Port of StyleCop.Analyzers SA1651 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1651.md)

A `<placeholder>` element marks documentation that was never written. Fires at the `<placeholder>` tag. Native port of StyleCop.Analyzers SA1651 — report-only.

### `SCS0005` — Weak random number generator.

*Port of SecurityCodeScan.VS2019 SCS0005 · Correctness · report-only* · [upstream docs](https://security-code-scan.github.io/#SCS0005)

`System.Random` is not cryptographically secure. Fires on a `Next`/`NextDouble`/ `NextBytes` call whose receiver's declared type resolves (by name, no symbol resolution) to `System.Random` — a bare `new Random()`, a local/parameter/field written as `Random`, or `Random.Shared` — anchored at the receiver expression. Native port of SecurityCodeScan.VS2019 SCS0005 — report-only (no mechanical rewrite to `RandomNumberGenerator` without knowing the surrounding usage).

### `SCS0013` — Potential usage of a weak CipherMode.

*Port of SecurityCodeScan.VS2019 SCS0013 · Correctness · report-only* · [upstream docs](https://security-code-scan.github.io/#SCS0013)

`System.Security.Cryptography.CipherMode` has exactly 5 members (CBC, ECB, OFB, CFB, CTS); probing confirmed every one fires regardless of usage context. Fires on any `CipherMode.<Member>` member access, anchored at the member name. Native port of SecurityCodeScan.VS2019 SCS0013 — report-only (no single safe universal replacement mode to rewrite to).

### `SYSLIB1045` — Regex constructed from a literal pattern could use a source-generated GeneratedRegexAttribute.

*Port of dotnet/runtime SYSLIB1045 (System.Text.RegularExpressions source generator diagnostic) · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/syslib-diagnostics/syslib1045)

`new Regex(<string literal>)` (or the target-typed `new(<string literal>)` on a Regex-typed field, local, or property) builds the regex at runtime; a `[GeneratedRegexAttribute]` partial method generates it at compile-time instead (faster startup, AOT-friendly). Fires when the constructor's pattern argument is a plain (non-interpolated) string literal, anchored at the `new` keyword. Report-only — the fix is a structural partial-method rewrite, out of scope for this port. Native port of the SYSLIB1045 compiler/source-generator diagnostic, surfaced by `dotnet format`.

### `VSTHRD004` — SwitchToMainThreadAsync() must be awaited.

*Port of Microsoft.VisualStudio.Threading.Analyzers VSTHRD004 · Concurrency · report-only* · [upstream docs](https://microsoft.github.io/vs-threading/analyzers/VSTHRD004.html)

A call to `JoinableTaskFactory.SwitchToMainThreadAsync()` that is never `await`ed silently fails to switch threads — the returned awaitable simply gets discarded. Fires on any invocation of a method named `SwitchToMainThreadAsync` (bare name match — see the module doc for the accepted narrowing) unless an `await` governs it somewhere in its ancestor chain, anchored at the method name. Native port of Microsoft.VisualStudio.Threading.Analyzers VSTHRD004 — report-only (adding `await` can require the enclosing method to become `async`).

### `VSTHRD100` — Avoid async void methods.

*Port of Microsoft.VisualStudio.Threading.Analyzers VSTHRD100 · Concurrency · report-only* · [upstream docs](https://github.com/microsoft/vs-threading/blob/main/doc/analyzers/VSTHRD100.md)

An `async void` method cannot be awaited and any exception it throws crashes the process instead of propagating to the caller; return `Task` instead. Fires on an `async void` method, anchored at its name. Native port of Microsoft.VisualStudio.Threading.Analyzers VSTHRD100 (report-only — changing the return type ripples into callers).

### `VSTHRD112` — Implement System.IAsyncDisposable alongside the obsolete vs-threading interface.

*Port of Microsoft.VisualStudio.Threading.Analyzers VSTHRD112 · Concurrency · report-only* · [upstream docs](https://microsoft.github.io/vs-threading/analyzers/VSTHRD112.html)

A type implementing the vs-threading library's own obsolete `IAsyncDisposable` should also implement the real `System.IAsyncDisposable`, so BCL-aware callers (e.g. `await using`) can use it. Fires when the type's DIRECT base list names the qualified `Microsoft.VisualStudio.Threading.IAsyncDisposable` but its full transitive interface set does not also reach the real `System.IAsyncDisposable`, anchored at that base-list entry (a bare, unqualified `IAsyncDisposable` entry is a documented narrower gap — see the module doc). Native port of Microsoft.VisualStudio.Threading.Analyzers VSTHRD112, consuming the BCL type-fact table (issue #144) and the project index; report-only.

### `VSTHRD113` — Pair a check for the obsolete vs-threading IAsyncDisposable with a check for System.IAsyncDisposable.

*Port of Microsoft.VisualStudio.Threading.Analyzers VSTHRD113 · Concurrency · report-only* · [upstream docs](https://microsoft.github.io/vs-threading/analyzers/VSTHRD113.html)

Code that type-checks for the vs-threading library's own obsolete `IAsyncDisposable` should also check for the real `System.IAsyncDisposable`, so BCL-based implementations are handled too. Fires on an `is`-type-check, `is`-pattern, or explicit cast whose checked type is the qualified `Microsoft.VisualStudio.Threading.IAsyncDisposable`, anchored at that check's own start — UNLESS a check for `System.IAsyncDisposable` (qualified or bare) is also found somewhere in the nearest enclosing function. Native port of Microsoft.VisualStudio.Threading.Analyzers VSTHRD113 — report-only.

### `VSTHRD115` — Avoid creating JoinableTaskContext with a null SynchronizationContext.

*Port of Microsoft.VisualStudio.Threading.Analyzers VSTHRD115 · Concurrency · report-only* · [upstream docs](https://microsoft.github.io/vs-threading/analyzers/VSTHRD115.html)

`new JoinableTaskContext(mainThread, null)` behaves differently depending on `SynchronizationContext.Current` at construction time — prefer `JoinableTaskContext.CreateNoOpContext()` when a no-op context is intended. Fires when the explicit, 2-argument constructor call's 2nd argument is a literal `null`, anchored at that literal; the single-argument/omitted-argument form (an IMPLICIT default `null`) is a documented, narrower gap — a CST has no node for an argument that was never written. Native port of Microsoft.VisualStudio.Threading.Analyzers VSTHRD115 — report-only.

### `VSTHRD200` — Async naming convention not followed.

*Port of Microsoft.VisualStudio.Threading.Analyzers VSTHRD200 · Concurrency · report-only* · [upstream docs](https://github.com/microsoft/vs-threading/blob/main/doc/analyzers/VSTHRD200.md)

A method returning an awaitable (`Task`/`ValueTask`/`IAsyncEnumerable`) should be named with an `Async` suffix, and a method that is not should not. Fires when the return type's awaitability and the presence of the `Async` suffix disagree, anchored at the method name; `override`s, `Main`, and the exact name `DisposeAsyncCore` (the recommended IAsyncDisposable extension-point pattern) are exempt. Native port of VSTHRD200 — report-only (renames ripple into callers).

### `xUnit1000` — Test classes must be public.

*Port of xunit.analyzers xUnit1000 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1000)

xUnit only discovers tests on `public` classes, so a `[Fact]`/`[Theory]` on a non-public class silently never runs. Fires on a class that declares a test method but is not `public`, anchored at the class name. Native port of xunit.analyzers xUnit1000 — report-only.

### `xUnit1001` — Fact methods cannot have parameters.

*Port of xunit.analyzers xUnit1001 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1001)

An xUnit `[Fact]` test must be parameterless; a test that takes inputs should be a `[Theory]` with data attributes. Fires on a `[Fact]`-marked method that declares parameters. Native port of xunit.analyzers xUnit1001 — purely syntactic, report-only.

### `xUnit1002` — Test methods cannot have multiple Fact or Theory attributes.

*Port of xunit.analyzers xUnit1002 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1002)

A method carrying two or more `[Fact]`/`[Theory]` markers confuses test discovery. Fires on a method with at least two such attributes, anchored at the method name. Native port of xunit.analyzers xUnit1002 — report-only.

### `xUnit1003` — Theory methods must have test data.

*Port of xunit.analyzers xUnit1003 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1003)

An xUnit `[Theory]` is data-driven and must declare a data source — `[InlineData]`, `[MemberData]`, or `[ClassData]`. Fires on a `[Theory]` method with none of those. Native port of xunit.analyzers xUnit1003 — purely syntactic, report-only.

### `xUnit1004` — Test methods should not be skipped.

*Port of xunit.analyzers xUnit1004 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1004)

A `[Fact]` / `[Theory]` with a `Skip = "..."` argument is silently not run; the skip is easy to forget. Fires once per skipped test, anchored at the `Skip` argument name. Native port of xunit.analyzers xUnit1004 (report-only).

### `xUnit1006` — Theory methods should have parameters.

*Port of xunit.analyzers xUnit1006 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1006)

A `[Theory]` is data-driven, so a parameterless theory never consumes its data and is almost always a mistake. Fires on a `[Theory]` method with an empty parameter list, anchored at the method name. Native port of xunit.analyzers xUnit1006 — report-only.

### `xUnit1008` — Test data attribute should only be used on a Theory.

*Port of xunit.analyzers xUnit1008 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1008)

`[InlineData]`/`[MemberData]`/`[ClassData]` feed a `[Theory]`; on a method that is not a `[Theory]` the data never runs. Fires on a method carrying a data attribute but no `[Theory]`, anchored at the method name. Native port of xunit.analyzers xUnit1008 — report-only.

### `xUnit1009` — InlineData supplies fewer values than the method's parameters.

*Port of xunit.analyzers xUnit1009 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1009)

Every required theory parameter needs a value, so an `[InlineData(…)]` with fewer values than required parameters is malformed. Fires on each `[InlineData]` supplying fewer positional values than the count of required (non-defaulted) parameters, anchored at the attribute; defaulted parameters and a `params` array are exempt. Native port of xunit.analyzers xUnit1009 — report-only.

### `xUnit1010` — InlineData value is not convertible to the parameter type.

*Port of xunit.analyzers xUnit1010 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1010)

An `[InlineData]` literal whose type cannot convert to the matching parameter type fails at run time. Fires on each literal value incompatible with its predefined value-type parameter, anchored at the value. `null` is xUnit1012's domain; non-literal values and reference-type parameters are out of syntactic scope. Native port of xunit.analyzers xUnit1010 — report-only.

### `xUnit1011` — InlineData has a value with no matching method parameter.

*Port of xunit.analyzers xUnit1011 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1011)

An `[InlineData(…)]` supplies one value per theory parameter, so a value beyond the parameter count is dead. Fires on each surplus positional `[InlineData]` value, anchored at that value; a trailing `params` array exempts the method. Native port of xunit.analyzers xUnit1011 — report-only.

### `xUnit1012` — Null should not be used for value-type parameters.

*Port of xunit.analyzers xUnit1012 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1012)

An `[InlineData(null)]` value bound to a non-nullable value-type parameter throws at run time. Fires on each `null` `[InlineData]` value whose matching parameter is a predefined non-nullable value type, anchored at the `null`. Custom structs are out of syntactic scope. Native port of xunit.analyzers xUnit1012 — report-only.

### `xUnit1013` — Public method should be marked as a test.

*Port of xunit.analyzers xUnit1013 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1013)

A public method on a test class (one that already has `[Fact]`/`[Theory]` methods) that is not itself a test is often a forgotten test — add a test attribute or reduce its visibility. Fires on such a method; methods that carry a test attribute, `Dispose`, and `override`s are exempt. Native port of xunit.analyzers xUnit1013 — purely syntactic, report-only.

### `xUnit1014` — MemberData should use the nameof operator for the member name.

*Port of xunit.analyzers xUnit1014 · Maintainability · has an autofix* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1014)

Referencing the data member by a string constant breaks silently on rename, whereas `nameof(Member)` is compiler-checked. Fires on a `[MemberData]` whose first argument is a string literal, anchored at that literal; the fix rewrites it to `nameof(<member>)`. Native port of xunit.analyzers xUnit1014.

### `xUnit1024` — Test methods should not be overloaded.

*Port of xunit.analyzers xUnit1024 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1024)

The runner discovers tests by method name, so two methods sharing a name on the same class are ambiguous. Fires on each `[Fact]`/`[Theory]` method whose name is declared more than once on the class, anchored at the method name. Native port of xunit.analyzers xUnit1024 — report-only.

### `xUnit1025` — InlineData should be unique within the Theory it belongs to.

*Port of xunit.analyzers xUnit1025 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1025)

Two `[InlineData(…)]` attributes with identical arguments run the same case twice. Fires on each `[InlineData]` whose arguments (compared by text) duplicate an earlier one on the same `[Theory]`, anchored at the duplicate attribute. Native port of xunit.analyzers xUnit1025 — report-only.

### `xUnit1026` — Theory methods should use all of their parameters.

*Port of xunit.analyzers xUnit1026 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1026)

An xUnit `[Theory]` is data-driven, so a parameter the body never reads is dead weight. Fires per `[Theory]` parameter whose name appears nowhere in the method outside its declaration, anchored at the parameter name. Native port of xunit.analyzers xUnit1026 — report-only (removing a parameter also drops the matching data column).

### `xUnit1028` — Test method must have a valid return type.

*Port of xunit.analyzers xUnit1028 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1028)

A `[Fact]`/`[Theory]` method must return `void`, `Task`, or `ValueTask`. Fires on a test method with any other return type, anchored at the method name. Native port of xunit.analyzers xUnit1028 — report-only.

### `xUnit1030` — Do not call ConfigureAwait(false) in a test method.

*Port of xunit.analyzers xUnit1030 · Concurrency · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1030)

`ConfigureAwait(false)` in a test detaches the continuation from xUnit's synchronization context and can bypass its parallelization limits. Fires on a `ConfigureAwait(false)` call inside a `[Fact]`/`[Theory]` method, anchored at the `ConfigureAwait` member; `ConfigureAwait(true)` is allowed. Native port of xunit.analyzers xUnit1030 — report-only.

### `xUnit1031` — Do not use blocking task operations in test method.

*Port of xunit.analyzers xUnit1031 · Concurrency · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1031)

Blocking on a task (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`) inside a test can deadlock; the method should be `async` and `await`. Fires on such a member access inside a `[Fact]`/`[Theory]` method, anchored at the blocking member. Native port of xunit.analyzers xUnit1031 — report-only.

### `xUnit1048` — Avoid async-void unit tests.

*Port of xunit.analyzers xUnit1048 · Concurrency · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit1048)

A `[Fact]`/`[Theory]` method declared `async void` cannot be awaited by the runner and loses support in xUnit.net v3. Fires on a test method that is both `async` and returns `void`, anchored at the method name. Native port of xunit.analyzers xUnit1048 — report-only (changing the return type ripples into callers).

### `xUnit2000` — Constants and literals should be the expected argument.

*Port of xunit.analyzers xUnit2000 · Correctness · has an autofix* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2000)

`Assert.Equal(actual, literal)` passes the literal as the actual value, so failure messages read backwards. The literal/constant should be the expected (first) argument. Fires on `Assert.Equal`/`NotEqual`/`StrictEqual`/`NotStrictEqual` whose second argument is a literal and whose first is not; the fix swaps them. Native port of xunit.analyzers xUnit2000.

### `xUnit2002` — Do not use null check on value type.

*Port of xunit.analyzers xUnit2002 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2002)

A value type is never null, so `Assert.Null`/`NotNull` on one is dead. Fires on an `Assert.Null`/`NotNull` whose argument is a value-type literal (integer/real/boolean/character), anchored at the invocation. Native port of xunit.analyzers xUnit2002 — report-only.

### `xUnit2003` — Do not use Assert.Equal() to check for null value.

*Port of xunit.analyzers xUnit2003 · Correctness · has an autofix* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2003)

Comparing to `null` with `Assert.Equal(null, x)` / `Assert.NotEqual(null, x)` is clearer as `Assert.Null(x)` / `Assert.NotNull(x)`. Fires when the expected (first) argument of an `Assert.Equal`/`Assert.NotEqual` call is the `null` literal; the fix rewrites the call (for the two-argument form). Native port of xunit.analyzers xUnit2003.

### `xUnit2004` — Do not use Assert.Equal to check a boolean condition.

*Port of xunit.analyzers xUnit2004 · Maintainability · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2004)

Comparing against a boolean literal with `Assert.Equal`/`NotEqual` reads worse than the dedicated `Assert.True`/`False`. Fires on an `Assert.Equal`/`NotEqual` whose first argument is a `true`/`false` literal, anchored at the invocation. Native port of xunit.analyzers xUnit2004 — report-only.

### `xUnit2005` — Do not use identity check on value type.

*Port of xunit.analyzers xUnit2005 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2005)

Value types have no reference identity, so `Assert.Same`/`NotSame` on them is meaningless — use `Assert.Equal`. Fires on an `Assert.Same`/`NotSame` with a value-type literal argument (integer/real/boolean/character; a `string` is a reference type), anchored at the invocation. Native port of xunit.analyzers xUnit2005 — report-only.

### `xUnit2006` — Do not use a generic Assert.Equal to test string equality.

*Port of xunit.analyzers xUnit2006 · Maintainability · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2006)

`Assert.Equal<string>(a, b)` / `Assert.StrictEqual<string>(a, b)` should use the dedicated non-generic `Assert.Equal(string, string)` overload, which gives better failure messages. Fires on such a call, anchored at the invocation. Native port of xunit.analyzers xUnit2006 — report-only.

### `xUnit2007` — Do not use typeof expression to check the type.

*Port of xunit.analyzers xUnit2007 · Maintainability · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2007)

`Assert.IsType(typeof(T), obj)` reads more clearly as the generic `Assert.IsType<T>(obj)`. Fires on an `Assert.IsType`/`IsNotType` whose first argument is a `typeof(…)` expression, anchored at the invocation. Native port of xunit.analyzers xUnit2007 — report-only.

### `xUnit2008` — Do not use boolean check to match on regular expressions.

*Port of xunit.analyzers xUnit2008 · Maintainability · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2008)

`Assert.True(Regex.IsMatch(s, p))` should be `Assert.Matches(p, s)` for a clearer failure message. Fires on an `Assert.True`/`Assert.False` whose argument is a `…IsMatch(…)` call, anchored at the invocation. Native port of xunit.analyzers xUnit2008 — report-only.

### `xUnit2009` — Do not use boolean check to check for substrings.

*Port of xunit.analyzers xUnit2009 · Maintainability · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2009)

`Assert.True(s.Contains("x"))` should be `Assert.Contains("x", s)` for a clearer failure message. Fires on an `Assert.True`/`Assert.False` whose argument is a `Contains`/`StartsWith`/`EndsWith` call, anchored at the invocation. Native port of xunit.analyzers xUnit2009 — report-only.

### `xUnit2010` — Do not use boolean check to check for string equality.

*Port of xunit.analyzers xUnit2010 · Maintainability · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2010)

`Assert.True(s.Equals("x"))` should be `Assert.Equal("x", s)` for a clearer failure message. Fires on an `Assert.True`/`Assert.False` whose argument is a `.Equals(…)` call, anchored at the invocation. Native port of xunit.analyzers xUnit2010 — report-only.

### `xUnit2013` — Do not use Assert.Equal() to check for collection size.

*Port of xunit.analyzers xUnit2013 · Correctness · has an autofix* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2013)

`Assert.Equal(0, x.Count)` / `Assert.Equal(1, x.Count())` express collection-size checks indirectly; `Assert.Empty(x)` / `Assert.Single(x)` are clearer and give better failure messages. Fires on an `Assert.Equal` whose expected value is `0` or `1` and whose actual is a `.Count`/`.Length` access or `.Count()`/`.LongCount()` call; the fix rewrites it. Native port of xunit.analyzers xUnit2013.

### `xUnit2014` — Do not use a throws check for an asynchronously thrown exception.

*Port of xunit.analyzers xUnit2014 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2014)

`Assert.Throws`/`ThrowsAny` run their delegate synchronously, so an `async` lambda's exception escapes unobserved. Fires on a synchronous `Assert.Throws`/`ThrowsAny` whose delegate is an `async` lambda or anonymous method, anchored at the invocation. Native port of xunit.analyzers xUnit2014 — report-only.

### `xUnit2015` — Do not use typeof expression to check the exception type.

*Port of xunit.analyzers xUnit2015 · Maintainability · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2015)

`Assert.Throws(typeof(T), action)` reads more clearly as the generic `Assert.Throws<T>(action)`. Fires on an `Assert.Throws`/`ThrowsAsync`/`ThrowsAny` whose first argument is a `typeof(…)` expression, anchored at the invocation. The exception-assert twin of xUnit2007. Native port of xunit.analyzers xUnit2015 — report-only.

### `xUnit2021` — Async assertions should be awaited.

*Port of xunit.analyzers xUnit2021 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2021)

`Assert.ThrowsAsync`/`ThrowsAnyAsync` return a `Task` that must be awaited (or stored to await later); discarding it as a bare statement means the assertion never runs. Fires on such a call used as an expression statement, anchored at the invocation. Native port of xunit.analyzers xUnit2021 — report-only.

### `xUnit2022` — Boolean assertions should not be negated.

*Port of xunit.analyzers xUnit2022 · Maintainability · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2022)

`Assert.False(!x)` is just `Assert.True(x)` (and vice versa). Fires on an `Assert.True`/`Assert.False` whose argument is a `!` negation, anchored at the invocation. Native port of xunit.analyzers xUnit2022 — report-only.

### `xUnit2024` — Do not use boolean asserts for simple equality tests.

*Port of xunit.analyzers xUnit2024 · Correctness · report-only* · [upstream docs](https://xunit.net/xunit.analyzers/rules/xUnit2024)

`Assert.True(x == 1)` loses the expected-vs-actual failure message that `Assert.Equal` gives. Fires on an `Assert.True`/`Assert.False` whose argument is an `==`/`!=` comparison with a literal operand, anchored at the invocation. Native port of xunit.analyzers xUnit2024 — report-only.

---

_This page is generated from the analyzer-plugin registry; see `tests/ported_analyzers_doc.rs`. To propose a new port, open an issue._

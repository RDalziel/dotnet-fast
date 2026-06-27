# Ported Roslyn analyzers

`dotnet-fast` re-implements popular Roslyn analyzers as **native, Roslyn-free** rules that run in the default `lint` path — no `--deep`, no .NET SDK, no restore. Each port is verified at **exact parity** against the real analyzer (same `(file, line, column)` findings) and runs orders of magnitude faster.

**481 analyzers ported** (46 with an autofix), across SonarAnalyzer, Microsoft.CodeAnalysis.NetAnalyzers, StyleCop, Roslynator, and Meziantou.

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
| `CA1707` | Microsoft.CodeAnalysis.NetAnalyzers CA1707 | Style | report-only | Identifiers should not contain underscores. |
| `CA1708` | Microsoft.CodeAnalysis.NetAnalyzers CA1708 | Maintainability | report-only | Identifiers should differ by more than case. |
| `CA1710` | Microsoft.CodeAnalysis.NetAnalyzers CA1710 | Maintainability | report-only | Identifiers should have correct suffix. |
| `CA1711` | Microsoft.CodeAnalysis.NetAnalyzers CA1711 | Maintainability | report-only | Identifiers should not have incorrect suffix. |
| `CA1715` | Microsoft.CodeAnalysis.NetAnalyzers CA1715 | Maintainability | report-only | Identifiers should have correct prefix (interfaces `I`, type parameters `T`). |
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
| `CA1825` | Microsoft.CodeAnalysis.NetAnalyzers CA1825 | Performance | yes | Avoid zero-length array allocations. |
| `CA1834` | Microsoft.CodeAnalysis.NetAnalyzers CA1834 | Performance | report-only | Use StringBuilder.Append(char) for single characters. |
| `CA1847` | Microsoft.CodeAnalysis.NetAnalyzers CA1847 | Performance | report-only | Use a char literal for a single-character Contains lookup. |
| `CA1866` | Microsoft.CodeAnalysis.NetAnalyzers CA1866 | Performance | report-only | Use the char overload of IndexOf. |
| `CA2002` | Microsoft.CodeAnalysis.NetAnalyzers CA2002 | Concurrency | report-only | Do not lock on objects with weak identity. |
| `CA2011` | Microsoft.CodeAnalysis.NetAnalyzers CA2011 | Correctness | report-only | Do not assign a property within its setter. |
| `CA2014` | Microsoft.CodeAnalysis.NetAnalyzers CA2014 | Performance | report-only | Do not use stackalloc in loops. |
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
| `MA0042` | Meziantou.Analyzer MA0042 | Concurrency | report-only | Do not use Thread.Sleep in an async method. |
| `MA0043` | Meziantou.Analyzer MA0043 | Maintainability | report-only | Use nameof operator. |
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
| `MA0090` | Meziantou.Analyzer MA0090 | Redundancy | report-only | Remove an empty finally/else block. |
| `MA0095` | Meziantou.Analyzer MA0095 | Maintainability | report-only | A class that implements IEquatable<T> should override Equals(object). |
| `MA0096` | Meziantou.Analyzer MA0096 | Maintainability | report-only | A class implementing IComparable<T> should override comparison operators. |
| `MA0097` | Meziantou.Analyzer MA0097 | Maintainability | report-only | A class implementing IComparable<T> should also implement IComparable. |
| `MA0101` | Meziantou.Analyzer MA0101 | Correctness | report-only | String contains an implicit end of line character. |
| `MA0102` | Meziantou.Analyzer MA0102 | Performance | report-only | Make member readonly. |
| `MA0136` | Meziantou.Analyzer MA0136 | Style | report-only | Raw string literals should not use an implicit end of line. |
| `MA0140` | Meziantou.Analyzer MA0140 | Correctness | report-only | Both if and else branch have identical code. |
| `MA0159` | Meziantou.Analyzer MA0159 | Performance | report-only | Use 'Order' instead of 'OrderBy'. |
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
| `RCS1036` | Roslynator.Analyzers RCS1036 | Style | report-only | Remove redundant empty line. |
| `RCS1037` | Roslynator.Analyzers RCS1037 | Redundancy | yes | Remove trailing white-space. |
| `RCS1039` | Roslynator.Analyzers RCS1039 | Redundancy | yes | Remove empty argument lists from attributes. |
| `RCS1040` | Roslynator.Analyzers RCS1040 | Redundancy | yes | Remove empty else clause. |
| `RCS1043` | Roslynator.Analyzers RCS1043 | Redundancy | report-only | Remove 'partial' modifier from a type with a single part. |
| `RCS1044` | Roslynator.Analyzers RCS1044 | Correctness | yes | Remove original exception from throw statement. |
| `RCS1047` | Roslynator.Analyzers RCS1047 | Maintainability | report-only | Non-asynchronous method name should not end with 'Async'. |
| `RCS1048` | Roslynator.Analyzers RCS1048 | Style | yes | Use lambda expression instead of anonymous method. |
| `RCS1049` | Roslynator.Analyzers RCS1049 | Redundancy | report-only | Simplify boolean comparison. |
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
| `RCS1084` | Roslynator.Analyzers RCS1084 | Style | yes | Use coalesce expression instead of conditional expression. |
| `RCS1085` | Roslynator.Analyzers RCS1085 | Redundancy | report-only | Use auto-implemented property. |
| `RCS1089` | Roslynator.Analyzers RCS1089 | Redundancy | report-only | Use ++/-- operator instead of assignment. |
| `RCS1098` | Roslynator.Analyzers RCS1098 | Style | yes | Constant values should be on the right side of comparisons. |
| `RCS1102` | Roslynator.Analyzers RCS1102 | Maintainability | report-only | Make class static. |
| `RCS1104` | Roslynator.Analyzers RCS1104 | Redundancy | yes | Simplify boolean conditional expressions. |
| `RCS1110` | Roslynator.Analyzers RCS1110 | Maintainability | report-only | Declare type inside a namespace. |
| `RCS1118` | Roslynator.Analyzers RCS1118 | Style | report-only | Mark local variable as const. |
| `RCS1123` | Roslynator.Analyzers RCS1123 | Maintainability | yes | Add parentheses when necessary. |
| `RCS1124` | Roslynator.Analyzers RCS1124 | Redundancy | report-only | Inline a local variable that is used only once. |
| `RCS1126` | Roslynator.Analyzers RCS1126 | Style | report-only | Add braces to if-else. |
| `RCS1129` | Roslynator.Analyzers RCS1129 | Redundancy | yes | Remove redundant field initialization. |
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
| `RCS1244` | Roslynator.Analyzers RCS1244 | Redundancy | report-only | Simplify 'default' expression. |
| `RCS1251` | Roslynator.Analyzers RCS1251 | Redundancy | report-only | Remove unnecessary braces from an empty-body type. |
| `RCS1259` | Roslynator.Analyzers RCS1259 | Redundancy | yes | Remove empty syntax. |
| `RCS1262` | Roslynator.Analyzers RCS1262 | Redundancy | report-only | Unnecessary raw string literal. |
| `RCS1265` | Roslynator.Analyzers RCS1265 | Redundancy | report-only | Remove redundant catch block. |
| `S100` | SonarAnalyzer.CSharp S100 | Style | report-only | Methods and properties should be named in PascalCase. |
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
| `S1264` | SonarAnalyzer.CSharp S1264 | Maintainability | yes | A `while` loop should be used instead of a `for` loop with only a condition. |
| `S1301` | SonarAnalyzer.CSharp S1301 | Maintainability | report-only | switch statements should have at least 3 case clauses. |
| `S131` | SonarAnalyzer.CSharp S131 | Maintainability | report-only | switch statements should have default clauses. |
| `S134` | SonarAnalyzer.CSharp S134 | Maintainability | report-only | Control flow statements should not be nested too deeply. |
| `S138` | SonarAnalyzer.CSharp S138 | Maintainability | report-only | Functions should not have too many lines of code. |
| `S1481` | SonarAnalyzer.CSharp S1481 | Redundancy | report-only | Unused local variables should be removed. |
| `S1656` | SonarAnalyzer.CSharp S1656 | Correctness | report-only | Variables should not be self-assigned. |
| `S1751` | SonarAnalyzer.CSharp S1751 | Correctness | report-only | Loops with at most one iteration should be refactored. |
| `S1764` | SonarAnalyzer.CSharp S1764 | Correctness | report-only | Identical expressions should not be used on both sides of a binary operator. |
| `S1848` | SonarAnalyzer.CSharp S1848 | Correctness | report-only | Objects should not be created to be dropped immediately without being used. |
| `S1862` | SonarAnalyzer.CSharp S1862 | Correctness | report-only | Related `if`/`else if` conditions should not be the same. |
| `S1871` | SonarAnalyzer.CSharp S1871 | Correctness | report-only | Two `switch` sections should not have the same implementation. |
| `S1940` | SonarAnalyzer.CSharp S1940 | Style | yes | Boolean checks should not be inverted. |
| `S2094` | SonarAnalyzer.CSharp S2094 | Maintainability | report-only | Classes should not be empty. |
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
| `S2344` | SonarAnalyzer.CSharp S2344 | Maintainability | report-only | Enumeration type names should not have 'Flags' or 'Enum' suffixes. |
| `S2360` | SonarAnalyzer.CSharp S2360 | Maintainability | report-only | Optional parameters should not be used. |
| `S2368` | SonarAnalyzer.CSharp S2368 | Maintainability | report-only | Public methods should not have multidimensional array parameters. |
| `S2372` | SonarAnalyzer.CSharp S2372 | Maintainability | report-only | Exceptions should not be thrown from property getters. |
| `S2376` | SonarAnalyzer.CSharp S2376 | Maintainability | report-only | Write-only properties should not be used. |
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
| `S3358` | SonarAnalyzer.CSharp S3358 | Maintainability | report-only | Ternary operators should not be nested. |
| `S3376` | SonarAnalyzer.CSharp S3376 | Maintainability | report-only | Attribute, EventArgs, and Exception type names should end with the type being extended. |
| `S3400` | SonarAnalyzer.CSharp S3400 | Maintainability | report-only | Methods should not return constants. |
| `S3442` | SonarAnalyzer.CSharp S3442 | Maintainability | report-only | Abstract classes should not have public constructors. |
| `S3445` | SonarAnalyzer.CSharp S3445 | Correctness | report-only | Exceptions should not be explicitly rethrown. |
| `S3459` | SonarAnalyzer.CSharp S3459 | Correctness | report-only | Unassigned fields should be removed. |
| `S3626` | SonarAnalyzer.CSharp S3626 | Redundancy | report-only | Jump statements should not be redundant. |
| `S3871` | SonarAnalyzer.CSharp S3871 | Maintainability | report-only | Exception types should be public. |
| `S3875` | SonarAnalyzer.CSharp S3875 | Correctness | report-only | operator== should not be overloaded on reference types. |
| `S3878` | SonarAnalyzer.CSharp S3878 | Redundancy | report-only | Arrays should not be created for params parameters. |
| `S3897` | SonarAnalyzer.CSharp S3897 | Maintainability | report-only | Classes that provide Equals(T) should implement IEquatable<T>. |
| `S3903` | SonarAnalyzer.CSharp S3903 | Maintainability | report-only | Types should be defined in named namespaces. |
| `S3923` | SonarAnalyzer.CSharp S3923 | Correctness | report-only | All branches in a conditional structure should not have the same implementation. |
| `S3928` | SonarAnalyzer.CSharp S3928 | Correctness | report-only | Parameter names used in ArgumentException constructors should match an existing one. |
| `S3963` | SonarAnalyzer.CSharp S3963 | Maintainability | report-only | Static fields should be initialized inline. |
| `S3981` | SonarAnalyzer.CSharp S3981 | Correctness | report-only | Collection size and array length comparisons should make sense. |
| `S3984` | SonarAnalyzer.CSharp S3984 | Correctness | report-only | Exceptions should not be created without being thrown. |
| `S3993` | SonarAnalyzer.CSharp S3993 | Maintainability | report-only | Custom attributes should be marked with AttributeUsageAttribute. |
| `S4023` | SonarAnalyzer.CSharp S4023 | Maintainability | report-only | Interfaces should not be empty. |
| `S4035` | SonarAnalyzer.CSharp S4035 | Correctness | report-only | Classes implementing IEquatable<T> should be sealed. |
| `S4039` | SonarAnalyzer.CSharp S4039 | Maintainability | report-only | Interface methods should be callable by derived types. |
| `S4050` | SonarAnalyzer.CSharp S4050 | Correctness | report-only | Operators should be overloaded consistently. |
| `S4136` | SonarAnalyzer.CSharp S4136 | Maintainability | report-only | Method overloads should be grouped together. |
| `S4144` | SonarAnalyzer.CSharp S4144 | Correctness | report-only | Methods should not have identical implementations. |
| `S4487` | SonarAnalyzer.CSharp S4487 | Redundancy | report-only | Unread private fields should be removed. |
| `S4524` | SonarAnalyzer.CSharp S4524 | Maintainability | report-only | default clauses should be first or last. |
| `S4663` | SonarAnalyzer.CSharp S4663 | Redundancy | report-only | Comments should not be empty. |
| `S818` | SonarAnalyzer.CSharp S818 | Maintainability | report-only | Literal suffixes should be upper case. |
| `S907` | SonarAnalyzer.CSharp S907 | Maintainability | report-only | goto statement should not be used. |
| `SA1000` | StyleCop.Analyzers SA1000 | Style | yes | The keyword 'new' should be followed by a space. |
| `SA1001` | StyleCop.Analyzers SA1001 | Style | report-only | Commas should be spaced correctly. |
| `SA1002` | StyleCop.Analyzers SA1002 | Style | report-only | Semicolons should be spaced correctly. |
| `SA1004` | StyleCop.Analyzers SA1004 | Style | report-only | Documentation line should begin with a space. |
| `SA1005` | StyleCop.Analyzers SA1005 | Style | yes | Single line comments should begin with a space. |
| `SA1008` | StyleCop.Analyzers SA1008 | Style | report-only | Opening parenthesis should not be preceded by a space. |
| `SA1009` | StyleCop.Analyzers SA1009 | Style | report-only | Closing parenthesis should not be preceded by a space. |
| `SA1010` | StyleCop.Analyzers SA1010 | Style | report-only | Opening square brackets should be spaced correctly. |
| `SA1011` | StyleCop.Analyzers SA1011 | Style | report-only | Closing square brackets should be spaced correctly. |
| `SA1012` | StyleCop.Analyzers SA1012 | Style | report-only | Opening braces should be spaced correctly. |
| `SA1013` | StyleCop.Analyzers SA1013 | Style | report-only | Closing braces should be spaced correctly. |
| `SA1014` | StyleCop.Analyzers SA1014 | Style | report-only | Opening generic brackets should not be preceded by a space. |
| `SA1015` | StyleCop.Analyzers SA1015 | Style | report-only | Closing generic brackets should be spaced correctly. |
| `SA1016` | StyleCop.Analyzers SA1016 | Style | report-only | Opening attribute brackets should not be followed by a space. |
| `SA1017` | StyleCop.Analyzers SA1017 | Style | report-only | Closing attribute brackets should not be preceded by a space. |
| `SA1018` | StyleCop.Analyzers SA1018 | Style | report-only | Nullable type symbol should not be preceded by a space. |
| `SA1019` | StyleCop.Analyzers SA1019 | Style | report-only | Member access symbols should be spaced correctly. |
| `SA1020` | StyleCop.Analyzers SA1020 | Style | report-only | Increment/decrement symbols should be spaced correctly. |
| `SA1021` | StyleCop.Analyzers SA1021 | Style | report-only | Negative signs should be spaced correctly. |
| `SA1022` | StyleCop.Analyzers SA1022 | Style | report-only | Positive signs should be spaced correctly. |
| `SA1024` | StyleCop.Analyzers SA1024 | Style | report-only | Colons should be spaced correctly. |
| `SA1025` | StyleCop.Analyzers SA1025 | Style | report-only | Code should not contain multiple whitespace characters in a row. |
| `SA1026` | StyleCop.Analyzers SA1026 | Style | report-only | The keyword 'new' should not be followed by a space. |
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
| `SA1119` | StyleCop.Analyzers SA1119 | Redundancy | report-only | Statement should not use unnecessary parenthesis. |
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
| `SA1500` | StyleCop.Analyzers SA1500 | Style | report-only | Braces for multi-line statements should not share a line. |
| `SA1501` | StyleCop.Analyzers SA1501 | Style | report-only | Statement should not be on a single line. |
| `SA1502` | StyleCop.Analyzers SA1502 | Style | report-only | Element should not be on a single line. |
| `SA1503` | StyleCop.Analyzers SA1503 | Maintainability | report-only | Braces should not be omitted. |
| `SA1505` | StyleCop.Analyzers SA1505 | Style | report-only | An opening brace should not be followed by a blank line. |
| `SA1506` | StyleCop.Analyzers SA1506 | Style | report-only | Element documentation headers should not be followed by blank line. |
| `SA1507` | StyleCop.Analyzers SA1507 | Style | report-only | Code should not contain multiple blank lines in a row. |
| `SA1508` | StyleCop.Analyzers SA1508 | Style | report-only | A closing brace should not be preceded by a blank line. |
| `SA1509` | StyleCop.Analyzers SA1509 | Style | report-only | Opening braces should not be preceded by blank line. |
| `SA1510` | StyleCop.Analyzers SA1510 | Style | report-only | Chained statement blocks should not be preceded by blank line. |
| `SA1511` | StyleCop.Analyzers SA1511 | Style | report-only | While-do footer should not be preceded by blank line. |
| `SA1512` | StyleCop.Analyzers SA1512 | Style | report-only | Single-line comments should not be followed by a blank line. |
| `SA1513` | StyleCop.Analyzers SA1513 | Style | report-only | Closing brace should be followed by blank line. |
| `SA1514` | StyleCop.Analyzers SA1514 | Style | report-only | Element documentation header should be preceded by a blank line. |
| `SA1515` | StyleCop.Analyzers SA1515 | Style | report-only | Single-line comments should be preceded by a blank line. |
| `SA1516` | StyleCop.Analyzers SA1516 | Style | report-only | Elements should be separated by a blank line. |
| `SA1517` | StyleCop.Analyzers SA1517 | Style | report-only | Code should not contain blank lines at start of file. |
| `SA1518` | StyleCop.Analyzers SA1518 | Style | report-only | Code should not contain blank lines at the end of the file. |
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
| `VSTHRD100` | Microsoft.VisualStudio.Threading.Analyzers VSTHRD100 | Concurrency | report-only | Avoid async void methods. |
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

A visible event should be typed as `EventHandler` / `EventHandler<TEventArgs>` so its signature is conventional and discoverable. Fires on a visible event (field-like or custom) whose delegate type is not an `EventHandler` — including `EventHandler<int>`, whose type argument is not an `EventArgs` type — anchored at the event name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1003 — report-only.

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

`out` parameters force a multi-step call pattern that is awkward for many callers — a return type or struct is usually clearer. Fires on a method with an `out` parameter, anchored at the method name; the `bool Try…(out …)` pattern is exempt. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1021 — report-only.

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

A class that holds only static members should be declared `static` or `sealed` so it is never instantiated or inherited. Fires on a non-static, non-sealed, non-abstract class with no base list whose every member is static/const, a nested type, or a constructor, anchored at the type name. Unlike RCS1102, a constructor does not disqualify the type (CA1052's fix removes it by making the type static). Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1052 — report-only.

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

### `CA1707` — Identifiers should not contain underscores.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1707 · Style · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1707)

`.NET` naming conventions are camel/Pascal-cased without underscores. Fires on an externally-visible type or member whose name contains `_`, anchored at the name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1707 — report-only.

### `CA1708` — Identifiers should differ by more than case.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1708 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1708)

Two members of the same type whose names differ only by case (`Run` and `run`) are confusing and not CLS-compliant. Fires once per type that has any such pair, anchored at the type name (comparing method/property/event/field/nested-type names). Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1708 — report-only.

### `CA1710` — Identifiers should have correct suffix.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1710 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1710)

A type derived from a well-known base should carry the conventional suffix — `Exception` → `…Exception`, `EventArgs` → `…EventArgs`, `Attribute` → `…Attribute`, `ICollection` → `…Collection`, `IDictionary` → `…Dictionary`. Fires on such a type whose name lacks the required suffix, anchored at its name (a bare `IEnumerable`/`IList` implementer is not flagged). Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1710 (report-only — a rename is not a safe syntactic rewrite).

### `CA1711` — Identifiers should not have incorrect suffix.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1711 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1711)

Reserved suffixes mislead readers about a type's kind. Fires on a type whose name ends in `Enum`, `Delegate`, `EventHandler`, `Ex`, or `Impl`, anchored at the name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1711 (report-only — a rename is not a safe syntactic rewrite).

### `CA1715` — Identifiers should have correct prefix (interfaces `I`, type parameters `T`).

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1715 · Maintainability · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1715)

Interface names should start with `I` and generic type-parameter names with `T`. A name passes when its first character is the required prefix and it is a single character or its second character is not lowercase (`IThing`, `TKey`, `T1` pass; `Thing`, `K`, `Type` are flagged). Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1715 (report-only — renames aren't a safe syntactic rewrite).

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

A value type with comparable state should override `Equals` and overload `operator ==` so its instances compare by value. Fires on a `struct` that has instance state (or already shows equality intent) but lacks BOTH an `Equals` override and an `==` operator, anchored at the type name. An empty struct is exempt. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1815 — report-only.

### `CA1819` — Properties should not return arrays.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1819 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1819)

A property returning an array hands callers a mutable copy (or a shared reference) on every access, which is surprising and allocation-heavy. Fires on a public property whose type is an array, anchored at the property name. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1819 — report-only.

### `CA1820` — Test for empty strings using string length.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1820 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1820)

Comparing a string with `==`/`!=` against an empty string literal is slower than testing `.Length`. Fires on such a comparison, anchored at the binary expression. The Roslynator twin (RCS1156) shares its detector. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1820 — report-only.

### `CA1821` — Remove empty finalizers.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1821 · Performance · has an autofix* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1821)

An empty finalizer (`~T() { }`) needlessly promotes every instance to the finalization queue, hurting GC performance, with no benefit. Fires on an empty finalizer, anchored at its name; the fix removes the declaration. A finalizer containing a comment or statement is left alone. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1821.

### `CA1825` — Avoid zero-length array allocations.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1825 · Performance · has an autofix* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1825)

`new T[0]` / `new T[] { }` allocates a new empty array each time, whereas `Array.Empty<T>()` returns a cached shared instance. Fires on an array creation whose length is statically zero (a `0` rank size, or an empty initializer), anchored at `new`; the fix rewrites it to `Array.Empty<T>()`. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1825.

### `CA1834` — Use StringBuilder.Append(char) for single characters.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1834 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1834)

`builder.Append("x")` with a single-character string allocates needlessly; the `char` overload is cheaper. Fires on an `Append` call whose single argument is a one-character string literal, anchored at that literal. The Meziantou twin is MA0028. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1834 — report-only.

### `CA1847` — Use a char literal for a single-character Contains lookup.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1847 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1847)

`s.Contains("x")` with a single-character string is slower than `s.Contains('x')`. Fires on a `Contains` call whose single argument is a one-character string literal, anchored at that literal. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1847 — report-only.

### `CA1866` — Use the char overload of IndexOf.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA1866 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1866)

`s.IndexOf("x")` with a single-character string allocates needlessly; the `char` overload is cheaper. Fires on an `IndexOf` call whose single argument is a one-character string literal, anchored at the argument list. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA1866 — report-only.

### `CA2002` — Do not lock on objects with weak identity.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2002 · Concurrency · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2002)

Locking on an object with weak identity — `this`, a `Type` (`typeof(...)`), or a string — risks deadlock because unrelated code can lock the same instance. Fires on such a `lock`, anchored at the locked expression. The Sonar (S2551), Roslynator (RCS1059) and Meziantou (MA0064) twins share its detector. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2002 — report-only.

### `CA2011` — Do not assign a property within its setter.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2011 · Correctness · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2011)

Assigning a property inside its own `set` accessor recurses forever. Fires on such an assignment, anchored at it. The Sonar twin (S2190) shares its detector but anchors at the accessor. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2011 — report-only.

### `CA2014` — Do not use stackalloc in loops.

*Port of Microsoft.CodeAnalysis.NetAnalyzers CA2014 · Performance · report-only* · [upstream docs](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2014)

`stackalloc` inside a loop keeps allocating stack space each iteration without freeing it until the method returns, risking a stack overflow. Fires on a `stackalloc` whose nearest enclosing scope (before any method/lambda/local-function boundary) is a loop, anchored at the `stackalloc`. Native port of Microsoft.CodeAnalysis.NetAnalyzers CA2014 — report-only.

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

A class with no instance state — including an empty class — can be marked `static` so it is never instantiated. Fires on a non-`static`, non-`abstract` class with no base list whose every member is `static`/`const`, anchored at the name (the Meziantou counterpart of RCS1102, but also flagging empty classes and not exempting `partial`). Native port of Meziantou.Analyzer MA0036 (report-only — adding `static` can ripple into callers).

### `MA0037` — Remove empty statement.

*Port of Meziantou.Analyzer MA0037 · Redundancy · has an autofix* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0037.md)

A standalone `;` is a no-op that usually slipped in by accident. Fires on an empty statement, anchored at the `;`; the fix deletes it. The Meziantou counterpart of Sonar S1116 / StyleCop SA1106. Native port of Meziantou.Analyzer MA0037.

### `MA0042` — Do not use Thread.Sleep in an async method.

*Port of Meziantou.Analyzer MA0042 · Concurrency · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0042.md)

`Thread.Sleep` blocks the thread, defeating the point of `async`; inside an `async` method use `await Task.Delay(...)` so the thread is released while waiting. Fires on a `Thread.Sleep(...)` call whose nearest enclosing function (method, local function, lambda, or accessor) is `async`; a blocking sleep in a synchronous method is not flagged. Native port of Meziantou MA0042 — purely syntactic, report-only.

### `MA0043` — Use nameof operator.

*Port of Meziantou.Analyzer MA0043 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0043.md)

A string literal whose value matches a parameter name should use `nameof`. Fires on such a literal, anchored at it. The Meziantou twin of CA1507. Native port of Meziantou.Analyzer MA0043 — report-only.

### `MA0046` — Use EventHandler<T> to declare events.

*Port of Meziantou.Analyzer MA0046 · Maintainability · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0046.md)

The Meziantou twin of CA1003: an event should be typed as `EventHandler` / `EventHandler<TEventArgs>`. Fires (regardless of visibility) on an event — field-like or custom — whose delegate type is not an `EventHandler`, including `EventHandler<int>` whose type argument is not an `EventArgs` type, anchored at the event name. Shares CA1003's detector. Native port of Meziantou.Analyzer MA0046 — report-only.

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

A class with a strongly-typed `Equals(T)` method should declare `IEquatable<T>` so generic code uses it. The Meziantou twin of S3897, anchored at the type name. Native port of Meziantou.Analyzer MA0077 — report-only.

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

### `MA0159` — Use 'Order' instead of 'OrderBy'.

*Port of Meziantou.Analyzer MA0159 · Performance · report-only* · [upstream docs](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0159.md)

Sorting by the element itself is the identity ordering, which `Order()` / `OrderDescending()` express directly. Fires on `seq.OrderBy(x => x)` / `seq.OrderByDescending(x => x)` (anchored at the method name) and on a query `orderby x` whose key is the range variable (anchored at the ordering). Native port of Meziantou.Analyzer MA0159 — report-only.

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

*Port of Roslynator.Analyzers RCS1036 · Style · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1036)

A blank-line run carries no separation value at the start/end of the file, right after `{`, right before `}`, or when two or more lines long. Fires once per redundant run, at the blank line adjacent to the following content (or the first blank of a trailing run), anchored at column 1; blank lines inside a multi-line string are not counted. Native port of Roslynator.Analyzers RCS1036 (report-only — deleting the line is a formatting rewrite, deferred).

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

An explicit instance constructor that is identical to the compiler-synthesised default — `public`, parameterless, empty body, no initializer, and the sole instance constructor — is redundant; removing it leaves the implicit default with the same effect. Fires at the declaration start; the fix deletes it. Skips `static`/`private`/attributed/initialized constructors, `abstract` classes, and structs. Native port of Roslynator RCS1074.

### `RCS1075` — Avoid empty catch clauses that catch System.Exception.

*Port of Roslynator.Analyzers RCS1075 · Correctness · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1075)

An empty `catch (Exception) { }` swallows every error, turning bugs into silent failures. Fires at the `catch` keyword when the body is empty and the caught type is exactly `Exception` / `System.Exception` (a general `catch { }` and a catch of a specific type are left to other rules). Native port of Roslynator RCS1075 — report-only (log, rethrow, or narrow the type is a human choice).

### `RCS1077` — Optimize LINQ method call.

*Port of Roslynator.Analyzers RCS1077 · Performance · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1077)

A `Where(predicate).Count()` chain (or `.Any()`, `.First()`, …) can fold the predicate into the terminal. Covers the `Where`-plus-no-argument-terminal case of Roslynator's broader RCS1077 (other optimizations deferred), anchored at the `Where`. Native port of Roslynator.Analyzers RCS1077 — report-only.

### `RCS1084` — Use coalesce expression instead of conditional expression.

*Port of Roslynator.Analyzers RCS1084 · Style · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1084)

A ternary that yields the tested value when non-null and a fallback otherwise is exactly `??`. Fires on `x != null ? x : y` and `x == null ? y : x`, anchored at the conditional expression; the fix is `x ?? y`. Native port of Roslynator.Analyzers RCS1084.

### `RCS1085` — Use auto-implemented property.

*Port of Roslynator.Analyzers RCS1085 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1085)

A property whose `get` just returns a backing field and whose `set` just assigns it is an auto-property written longhand. Fires on such a property, anchored at the property name. Native port of Roslynator.Analyzers RCS1085 — report-only.

### `RCS1089` — Use ++/-- operator instead of assignment.

*Port of Roslynator.Analyzers RCS1089 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1089)

`x = x + 1` / `x = x - 1` is the increment/decrement operator written longhand. Fires on such an assignment, anchored at the assignment. Native port of Roslynator.Analyzers RCS1089 — report-only.

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

An empty `<param>`, `<typeparam>`, `<returns>`, or `<value>` element documents nothing. Fires per empty element, anchored at its tag and naming the element, on any documentable declaration (method, property, type, record, constructor, indexer, delegate, operator, …); single-line elements only. Native port of Roslynator.Analyzers RCS1228 (StyleCop twins: SA1614/SA1616/SA1622) — report-only.

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

A type implementing `IComparable<T>` should also implement the non-generic `IComparable` so it interoperates with non-generic sorting APIs. Fires on a type that names `IComparable<...>` in its base list but not the non-generic `IComparable`, anchored at the type name. The Meziantou twin (MA0097) shares its detector. Native port of Roslynator.Analyzers RCS1241 — report-only.

### `RCS1244` — Simplify 'default' expression.

*Port of Roslynator.Analyzers RCS1244 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1244)

When the target type is clear from context, `default(T)` can be written as the bare `default` literal. Fires on a `default(T)` expression, anchored at it. Native port of Roslynator.Analyzers RCS1244 — report-only (dropping the type is unsound where it is not inferable, e.g. `var x = default(int);`, which needs a semantic model).

### `RCS1251` — Remove unnecessary braces from an empty-body type.

*Port of Roslynator.Analyzers RCS1251 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1251)

A type declared with an empty body (`{ }` containing no members) carries braces that add nothing. Fires at the opening `{`. Report-only — the Roslynator fix (`{ }` → `;`) is valid only for record/struct/partial shapes, so it is not applied universally. Native port of Roslynator.Analyzers RCS1251.

### `RCS1259` — Remove empty syntax.

*Port of Roslynator.Analyzers RCS1259 · Redundancy · has an autofix* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1259)

Redundant empty syntax adds noise. Fires on an empty statement (a stray `;`), an empty finalizer (`~T() { }`), an empty namespace block, and an empty `finally`/`else` clause; the fix deletes the construct. Native port of Roslynator.Analyzers RCS1259.

### `RCS1262` — Unnecessary raw string literal.

*Port of Roslynator.Analyzers RCS1262 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1262)

A raw string literal (`"""…"""`) is only needed when its content would require escaping in a regular string — an embedded double quote, a backslash, or a newline. With none of those the raw form is unnecessary and could be a plain `"…"`. Fires on such a literal, anchored at it. Native port of Roslynator RCS1262 — report-only (the conversion is a rewrite).

### `RCS1265` — Remove redundant catch block.

*Port of Roslynator.Analyzers RCS1265 · Redundancy · report-only* · [upstream docs](https://josefpihrt.github.io/docs/roslynator/analyzers/RCS1265)

A bare `catch { throw; }` catches every exception only to rethrow it unchanged — deleting the whole `try`/`catch` behaves identically. Fires at the `catch` keyword; a typed `catch (T)` (which narrows the caught type) or one with a `when` filter is not flagged, the narrower counterpart of Sonar S2737. Native port of Roslynator.Analyzers RCS1265 (report-only — removing the clause is a structural rewrite).

### `S100` — Methods and properties should be named in PascalCase.

*Port of SonarAnalyzer.CSharp S100 · Style · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-100/)

A method or property name should be PascalCase, with acronyms of at most two letters (`XmlParse` is fine, `XMLParse` is flagged for the three-letter run). Fires at the method or property name. The method/property sibling of S101 (types), sharing its PascalCase definition. Native port of SonarAnalyzer.CSharp S100 — report-only (a rename ripples to call sites).

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

A class whose members are all `static` is never instantiated, so a public (or implicit) constructor is misleading — give it a `private` constructor (or mark the class `static`). Fires on a non-static, base-less class with only static/const members that has an accessible constructor, anchored at the class name (implicit ctor) or the explicit public ctor. Native port of Sonar S1118 — purely syntactic, report-only.

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

An empty method body usually signals an unfinished implementation or a missed deletion. Either complete it, throw to signal it is unsupported, or — if the no-op is deliberate — add a comment explaining why. Fires on a method or local function with an empty `{ }` body; a body with a comment and a `virtual` method (a valid extension point) are exempt, while an `override` still fires. Native port of Sonar S1186 — purely syntactic, report-only.

### `S1199` — Nested code blocks should not be used.

*Port of SonarAnalyzer.CSharp S1199 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1199)

A free-standing `{ … }` block inside a method body adds a scope for no structural reason — extract a method instead. Fires on a `block` whose parent is itself a `block`, anchored at the opening brace. Native port of SonarAnalyzer.CSharp S1199 — report-only.

### `S1206` — Equals(object) and GetHashCode() should be overridden in pairs.

*Port of SonarAnalyzer.CSharp S1206 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1206/)

Overriding only one of `Equals(object)` / `GetHashCode()` breaks the equality contract — two equal objects must share a hash code. Fires on a `class` that overrides exactly one of the pair, anchored at the type name. Native port of SonarAnalyzer.CSharp S1206 — report-only.

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

### `S1751` — Loops with at most one iteration should be refactored.

*Port of SonarAnalyzer.CSharp S1751 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1751/)

A loop whose body unconditionally hits a jump on the first pass runs at most once — it is really an `if`. Fires when an unconditional `break`, `continue`, `return` or `throw` is a direct statement of the loop body, anchored at the jump keyword. A jump guarded by a conditional, or a second conditional `continue` that lets the loop iterate, does not fire. Native port of SonarAnalyzer.CSharp S1751 — report-only.

### `S1764` — Identical expressions should not be used on both sides of a binary operator.

*Port of SonarAnalyzer.CSharp S1764 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1764/)

A binary expression with the same operand on both sides (`a == a`, `a && a`, `a - a`) is a bug or dead code. Fires at the right-hand operand when the two sides are textually identical and the operator is one where duplication is meaningless — comparison, logical/bitwise, or `-` / `/` / `%`. `+`, `*`, and shifts are excluded (`x + x`, `x * x` are legitimate). Native port of SonarAnalyzer.CSharp S1764 — report-only.

### `S1848` — Objects should not be created to be dropped immediately without being used.

*Port of SonarAnalyzer.CSharp S1848 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1848/)

`new Foo();` as a standalone statement builds an object and throws it away — the result is never assigned, returned, or used. Fires at the creation when an `object_creation_expression` is used as a discarded statement. The exception subset is also flagged by S3984. Native port of SonarAnalyzer.CSharp S1848 — report-only (a needed side effect or a forgotten use is the author's call).

### `S1862` — Related `if`/`else if` conditions should not be the same.

*Port of SonarAnalyzer.CSharp S1862 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1862/)

In an `if`/`else if` chain, a condition identical to an earlier one in the same chain is dead — the earlier branch always wins. Fires at the duplicate condition (compared ignoring whitespace), naming the earlier branch's line. Native port of SonarAnalyzer.CSharp S1862 — report-only.

### `S1871` — Two `switch` sections should not have the same implementation.

*Port of SonarAnalyzer.CSharp S1871 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1871/)

Two `switch` sections with the same statements should be merged (or one changed) — the duplication is usually a copy-paste bug. Fires at the duplicate section's `case` label, naming the earlier section's line; section bodies are compared ignoring whitespace. Native port of SonarAnalyzer.CSharp S1871 — report-only.

### `S1940` — Boolean checks should not be inverted.

*Port of SonarAnalyzer.CSharp S1940 · Style · has an autofix* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-1940/)

Negating a comparison (`!(a == b)`, `!(a < b)`) is clearer written with the opposite operator (`a != b`, `a >= b`). Fires at the `!` of a `!(<comparison>)` over `== != < <= > >=`; the fix substitutes the opposite operator, preserving operand order. Native port of SonarAnalyzer.CSharp S1940.

### `S2094` — Classes should not be empty.

*Port of SonarAnalyzer.CSharp S2094 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2094/)

An empty class adds no behaviour and is usually dead code or an unfinished idea. Fires on a `class` with an empty body, anchored at its name; a class with a base list, a `partial` class, or a class carrying an attribute is exempt. Native port of SonarAnalyzer.CSharp S2094 (report-only — whether to delete, fill in, or convert to an interface is a human decision).

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

### `S2344` — Enumeration type names should not have 'Flags' or 'Enum' suffixes.

*Port of SonarAnalyzer.CSharp S2344 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2344/)

An `enum` named `…Enum` or `…Flags` repeats what the declaration already says. Fires on such an enum, anchored at its name (the Sonar, enum-specific counterpart of CA1711). Native port of SonarAnalyzer.CSharp S2344 (report-only — a rename is not a safe syntactic rewrite).

### `S2360` — Optional parameters should not be used.

*Port of SonarAnalyzer.CSharp S2360 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2360)

Optional parameters are baked into the call site at compile time, so changing a default silently breaks already-compiled callers — overloads are safer. Fires per parameter that declares a default value, anchored at the `=`. Native port of SonarAnalyzer.CSharp S2360 — report-only.

### `S2368` — Public methods should not have multidimensional array parameters.

*Port of SonarAnalyzer.CSharp S2368 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2368)

Multidimensional array parameters are awkward for callers to construct. Fires on an externally visible (`public`/`protected`) method that declares a parameter with a multidimensional array type, anchored at the method name. Shares CA1814's multidimensional-array check. Native port of SonarAnalyzer.CSharp S2368 — report-only.

### `S2372` — Exceptions should not be thrown from property getters.

*Port of SonarAnalyzer.CSharp S2372 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2372/)

A property read should not throw — callers do not expect it. Fires on a `throw new <X>` inside a property getter where `X` is not a getter-allowed exception (`InvalidOperationException`/`NotSupportedException`/`NotImplementedException`), anchored at the `throw`. Native port of SonarAnalyzer.CSharp S2372 (Microsoft twin: CA1065) — report-only.

### `S2376` — Write-only properties should not be used.

*Port of SonarAnalyzer.CSharp S2376 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2376)

A property with a setter but no getter cannot be read, which is confusing — a `SetX` method states the intent better. Fires on a property that declares a `set` accessor and no `get` accessor, anchored at the property name. Native port of SonarAnalyzer.CSharp S2376 — report-only.

### `S2436` — Types and methods should not have too many generic parameters.

*Port of SonarAnalyzer.CSharp S2436 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2436/)

A type or method with more than two generic parameters (the Sonar default) is hard to use. Fires on such a declaration, anchored at its name. Native port of SonarAnalyzer.CSharp S2436 — report-only.

### `S2486` — Generic exceptions should not be ignored.

*Port of SonarAnalyzer.CSharp S2486 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-2486/)

An empty `catch` block silently swallows an exception, hiding failures. Fires on a `catch` whose body is empty, anchored at the `catch` keyword; a body with even a comment is exempt (the comment explains why it is safe to ignore), as is any `catch` that rethrows or does work. Native port of SonarAnalyzer.CSharp S2486 — purely syntactic, report-only.

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

A redundant constructor or destructor adds noise. Fires on an empty destructor, an empty static constructor, and a public parameterless empty instance constructor that is the type's only instance constructor (the compiler already provides that default), anchored at the member. Native port of SonarAnalyzer.CSharp S3253 — report-only.

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

### `S3358` — Ternary operators should not be nested.

*Port of SonarAnalyzer.CSharp S3358 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3358/)

A ternary `?:` nested inside another ternary is hard to read; extract it into a separate statement. Fires on a conditional expression contained — through parentheses or other expression wrappers — within another conditional expression, anchored at the inner one. Native port of SonarAnalyzer.CSharp S3358 — purely syntactic, report-only.

### `S3376` — Attribute, EventArgs, and Exception type names should end with the type being extended.

*Port of SonarAnalyzer.CSharp S3376 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3376/)

A type that derives from `Exception`, `EventArgs`, or `Attribute` should carry that suffix so its role is clear. Fires on such a type whose name lacks the suffix, anchored at the name (the Sonar counterpart of CA1710's class-inheritance cases). Native port of SonarAnalyzer.CSharp S3376 (report-only — a rename is not a safe syntactic rewrite).

### `S3400` — Methods should not return constants.

*Port of SonarAnalyzer.CSharp S3400 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3400/)

A parameterless method that just returns a literal is better expressed as a constant. Fires on a method whose body is a single `return <literal>;` or an expression body `=> <literal>`, anchored at the method name. Bare `default` counts, except when the return type is a type parameter (`T M<T>() => default`), where the value depends on `T`. Native port of SonarAnalyzer.CSharp S3400 — purely syntactic (literal returns only), report-only.

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

A class with a strongly-typed `Equals(T)` method should declare `IEquatable<T>` so generic code uses it. Fires on a class with a typed `Equals` (a single parameter that is not `object`) and no `IEquatable` base, anchored at the type name. Native port of SonarAnalyzer.CSharp S3897 — report-only.

### `S3903` — Types should be defined in named namespaces.

*Port of SonarAnalyzer.CSharp S3903 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3903/)

A top-level type should live in a named namespace, not the global scope. Fires on a type with no namespace ancestor, anchored at its name (the Sonar twin of CA1050). Native port of SonarAnalyzer.CSharp S3903 — report-only.

### `S3923` — All branches in a conditional structure should not have the same implementation.

*Port of SonarAnalyzer.CSharp S3923 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3923/)

When every branch of an `if`/`else` chain — or both sides of a `?:` — does exactly the same thing, the condition is pointless and usually signals a bug. Fires on a ternary with identical branches (anchored at the expression) and on an `if`/`else` chain with a final `else` whose every branch body is identical (anchored at the `if`). Native port of SonarAnalyzer.CSharp S3923 — purely syntactic, report-only.

### `S3928` — Parameter names used in ArgumentException constructors should match an existing one.

*Port of SonarAnalyzer.CSharp S3928 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-3928)

Fires on an argument-exception object creation that is mis-constructed: a `paramName` string that is not an enclosing parameter, or a base `ArgumentException` created with no message at all. Anchored at the object creation. Shares CA2208's classification. Native port of SonarAnalyzer.CSharp S3928 — report-only.

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

### `S4023` — Interfaces should not be empty.

*Port of SonarAnalyzer.CSharp S4023 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-4023)

An empty interface defines no contract and is usually a misused marker. The Sonar twin of CA1040 — an `interface` with no members, anchored at the interface name. Native port of SonarAnalyzer.CSharp S4023 — report-only.

### `S4035` — Classes implementing IEquatable<T> should be sealed.

*Port of SonarAnalyzer.CSharp S4035 · Correctness · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-4035)

An unsealed class implementing `IEquatable<T>` can be subclassed, and the subclass's `Equals` will not be used by the typed comparison — a latent correctness trap. Fires on a non-`sealed`, non-`abstract`, non-`static` class with an `IEquatable` base, anchored at the type name. Native port of SonarAnalyzer.CSharp S4035 — report-only.

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

The Sonar twin of SA1120: an empty `//` line comment carries no information. Fires at the `//`, reusing the shared empty-comment detector. Native port of SonarAnalyzer.CSharp S4663 — report-only.

### `S818` — Literal suffixes should be upper case.

*Port of SonarAnalyzer.CSharp S818 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-818/)

A lower-case `l` suffix is easily mistaken for a `1`. Fires on an integer literal whose suffix is exactly a standalone lower-case `l` (`1l`), anchored at the `l`; combined suffixes (`1ul`, `1lu`) and the `u`/`U`/`L` forms are not flagged. Native port of SonarAnalyzer.CSharp S818 — report-only.

### `S907` — goto statement should not be used.

*Port of SonarAnalyzer.CSharp S907 · Maintainability · report-only* · [upstream docs](https://rules.sonarsource.com/csharp/RSPEC-907)

`goto` jumps obscure control flow and are almost always replaceable with structured constructs. Fires on a `goto` statement, anchored at the keyword. Native port of SonarAnalyzer.CSharp S907 — report-only.

### `SA1000` — The keyword 'new' should be followed by a space.

*Port of StyleCop.Analyzers SA1000 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1000.md)

A target-typed `new(...)` should have a space after the keyword (`new ()`), like any other use of `new`. Fires on an implicit object creation written as `new(` with no space, anchored at the keyword; the fix inserts the space. `new Foo()`, `new ()` and the implicit array `new[]` are all fine. Native port of StyleCop.Analyzers SA1000 (the `new` keyword-spacing case).

### `SA1001` — Commas should be spaced correctly.

*Port of StyleCop.Analyzers SA1001 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1001.md)

A comma should follow its preceding token directly, with no space before it. Fires on a `,` preceded by whitespace, anchored at the comma; comment and string interiors are skipped. Native port of StyleCop.Analyzers SA1001 (report-only — removing the space is a formatting rewrite).

### `SA1002` — Semicolons should be spaced correctly.

*Port of StyleCop.Analyzers SA1002 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1002.md)

A semicolon should follow its preceding token directly. Fires on a `;` preceded by whitespace, anchored at the `;`; comment and string interiors are skipped. Native port of StyleCop.Analyzers SA1002 (report-only — removing the space is a formatting rewrite).

### `SA1004` — Documentation line should begin with a space.

*Port of StyleCop.Analyzers SA1004 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1004.md)

An XML documentation line should have a space after the `///` marker (`/// text`, not `///text`). Fires on a `///` line whose first character after the marker is non-whitespace, anchored at that character; a `////` line and an empty `///` are left alone. Native port of StyleCop SA1004 — purely syntactic, report-only.

### `SA1005` — Single line comments should begin with a space.

*Port of StyleCop.Analyzers SA1005 · Style · has an autofix* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1005.md)

A `//` comment whose text begins immediately after the slashes (`//text`) is harder to read than `// text`. Fires on such a comment; the fix inserts one space. A documentation comment (`///`), the commented-out-code marker (`////`), an empty `//`, and an already-spaced comment are exempt. Native port of StyleCop.Analyzers SA1005.

### `SA1008` — Opening parenthesis should not be preceded by a space.

*Port of StyleCop.Analyzers SA1008 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1008.md)

In a method call or declaration the `(` should sit directly after the name — `Foo ()` should be `Foo()`. Fires on a `(` preceded by whitespace whose preceding token is an identifier (not a keyword like `if`/`for`); comment and string interiors are skipped. Native port of StyleCop.Analyzers SA1008 (report-only — removing the space is a formatting rewrite).

### `SA1009` — Closing parenthesis should not be preceded by a space.

*Port of StyleCop.Analyzers SA1009 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1009.md)

A space between the last token and its closing parenthesis is noise. Fires on a `)` directly preceded by whitespace that is not merely the line's leading indentation, anchored at the `)`; interior lines of a multi-line string are skipped. Native port of StyleCop.Analyzers SA1009 (report-only — removing the space is a formatting rewrite).

### `SA1010` — Opening square brackets should be spaced correctly.

*Port of StyleCop.Analyzers SA1010 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1010.md)

A `[` that opens an array access or array-creation rank should follow its preceding token directly. Fires on a `[` preceded by whitespace mid-line, anchored at the `[`; a `[` that begins its line (an attribute list) and comment/string interiors are skipped. Native port of StyleCop.Analyzers SA1010 (report-only — removing the space is a formatting rewrite).

### `SA1011` — Closing square brackets should be spaced correctly.

*Port of StyleCop.Analyzers SA1011 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1011.md)

A `]` should follow its preceding token directly and be separated from a following null-forgiving `!`. Fires on a `]` preceded by whitespace, or one immediately followed by `!` (`map[k]!`), anchored at the `]`; comment and string interiors are skipped. Native port of StyleCop.Analyzers SA1011 (report-only — the spacing change is a formatting rewrite).

### `SA1012` — Opening braces should be spaced correctly.

*Port of StyleCop.Analyzers SA1012 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1012.md)

The `{` of an object/collection initializer should be surrounded by spaces — `new T{ … }` should be `new T { … }`. Fires when an initializer's `{` is not preceded or not followed by whitespace, anchored at the `{`; block braces are not inspected. Native port of StyleCop.Analyzers SA1012 (report-only — adding the spaces is a formatting rewrite).

### `SA1013` — Closing braces should be spaced correctly.

*Port of StyleCop.Analyzers SA1013 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1013.md)

The `}` of an object/collection initializer should be preceded by a space — `{ … 1}` should be `{ … 1 }`. Fires when an initializer's `}` is not preceded by whitespace, anchored at the `}`; block braces are not inspected. Native port of StyleCop.Analyzers SA1013 (report-only — adding the space is a formatting rewrite).

### `SA1014` — Opening generic brackets should not be preceded by a space.

*Port of StyleCop.Analyzers SA1014 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1014.md)

The `<` of a generic argument list should sit tight against the name (`List<int>`, not `List <int>`). Fires on a `<` preceded by whitespace on the same line, anchored at the `<`. The opening-bracket twin of StyleCop SA1015. Native port of StyleCop SA1014 — purely syntactic, report-only.

### `SA1015` — Closing generic brackets should be spaced correctly.

*Port of StyleCop.Analyzers SA1015 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1015.md)

The closing `>` of a type-argument list should sit directly against the last argument — `List<int >` should be `List<int>`. Fires when the `>` is separated from the preceding token by whitespace, anchored at the `>`; a comparison `a > b` is never touched (only `type_argument_list` is inspected). Native port of StyleCop.Analyzers SA1015 (report-only — removing the space is a formatting rewrite).

### `SA1016` — Opening attribute brackets should not be followed by a space.

*Port of StyleCop.Analyzers SA1016 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1016.md)

An attribute's opening `[` should sit tight against the attribute (`[Obsolete]`, not `[ Obsolete]`). Fires on a `[` followed by whitespace on the same line, anchored at the `[`. Native port of StyleCop SA1016 — purely syntactic, report-only.

### `SA1017` — Closing attribute brackets should not be preceded by a space.

*Port of StyleCop.Analyzers SA1017 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1017.md)

An attribute's closing `]` should sit tight against the attribute (`[Obsolete]`, not `[Obsolete ]`). Fires on a `]` preceded by whitespace on the same line, anchored at the `]`. Native port of StyleCop SA1017 — purely syntactic, report-only.

### `SA1018` — Nullable type symbol should not be preceded by a space.

*Port of StyleCop.Analyzers SA1018 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1018.md)

The `?` of a nullable type should sit tight against the type (`int?`, not `int ?`). Fires on a `?` preceded by whitespace on the same line, anchored at the `?`. Native port of StyleCop SA1018 — purely syntactic, report-only.

### `SA1019` — Member access symbols should be spaced correctly.

*Port of StyleCop.Analyzers SA1019 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1019.md)

A member-access `.` should sit directly against the expression it follows — `x .Length` should be `x.Length`. Fires when the `.` of a `member_access_expression` is separated from the preceding token by whitespace, anchored at the `.`. Native port of StyleCop.Analyzers SA1019 (report-only — removing the space is a formatting rewrite).

### `SA1020` — Increment/decrement symbols should be spaced correctly.

*Port of StyleCop.Analyzers SA1020 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1020.md)

A postfix `++`/`--` should sit directly against its operand — `n ++` should be `n++`. Fires when the operator of a `postfix_unary_expression` is separated from its operand by whitespace, anchored at the operator. Native port of StyleCop.Analyzers SA1020 (report-only — removing the space is a formatting rewrite).

### `SA1021` — Negative signs should be spaced correctly.

*Port of StyleCop.Analyzers SA1021 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1021.md)

A unary negative sign should sit directly against its operand — `- 1` should be `-1`. Fires on a `-` unary operator separated from its operand by whitespace, anchored at the sign; binary subtraction (`a - b`) is unaffected. Native port of StyleCop.Analyzers SA1021 (report-only — removing the space is a formatting rewrite).

### `SA1022` — Positive signs should be spaced correctly.

*Port of StyleCop.Analyzers SA1022 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1022.md)

A unary positive sign should sit directly against its operand — `+ 1` should be `+1`. Fires on a `+` unary operator separated from its operand by whitespace, anchored at the sign; binary addition (`a + b`) is unaffected. The positive-sign twin of SA1021. Native port of StyleCop.Analyzers SA1022 (report-only — removing the space is a formatting rewrite).

### `SA1024` — Colons should be spaced correctly.

*Port of StyleCop.Analyzers SA1024 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1024.md)

A switch-label colon should sit directly after its case pattern — `case 1 :` should be `case 1:`. Fires when the `:` of a `switch_section` is separated from the preceding token by whitespace, anchored at the `:`; the ternary and base-list colons (correctly spaced) are never touched. Native port of StyleCop.Analyzers SA1024 (report-only — removing the space is a formatting rewrite).

### `SA1025` — Code should not contain multiple whitespace characters in a row.

*Port of StyleCop.Analyzers SA1025 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1025.md)

A run of two or more spaces inside code is collapsed noise. Fires at the first space of every such run, skipping leading indentation, trailing whitespace (SA1028), runs inside a string/character literal or comment, and the run aligning a trailing `//` comment after a statement terminator. Native port of StyleCop.Analyzers SA1025 (report-only — collapsing the run is a formatting rewrite, deferred).

### `SA1026` — The keyword 'new' should not be followed by a space.

*Port of StyleCop.Analyzers SA1026 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1026.md)

An implicitly-typed array creation should keep `new` tight against the `[` (`new[] { … }`, not `new [] { … }`). Fires on a `new` keyword separated from its `[` by whitespace on the same line, anchored at the `new`. Native port of StyleCop SA1026 — purely syntactic, report-only.

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

A method's parameter-list `(` should sit on the same line as the method name, not wrapped to the next line. Fires when the `(` and the name are on different lines, anchored at the `(`. Native port of StyleCop.Analyzers SA1110 — report-only.

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

*Port of StyleCop.Analyzers SA1119 · Redundancy · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1119.md)

Parentheses that wrap an entire value add no meaning. Fires on a parenthesized expression whose parent is a value position — a `=` initializer or assignment right side, a `return`, an `=>` body, a call argument, a conditional branch, or another set of parentheses. Parentheses used as an operand of a larger expression (where they may affect precedence) are not flagged. Native port of StyleCop SA1119 — purely syntactic, report-only.

### `SA1120` — Comments should contain text.

*Port of StyleCop.Analyzers SA1120 · Redundancy · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1120.md)

An empty single-line comment (`//` with nothing after it) carries no information. Fires at the `//`; doc comments (`///`) and the commented-out-code marker (`////`) are left alone. Native port of StyleCop.Analyzers SA1120 — report-only.

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

`delegate { … }` reads more clearly as `() => { … }`. Fires on an anonymous method expression, anchored at the `delegate` keyword; the fix replaces it with `<params> => <body>` (an absent parameter list becomes `()`). Native port of StyleCop.Analyzers SA1130.

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

Each attribute should occupy its own line, so `[A][B]` (two attribute lists on one line) is flagged at the second list. Fires at the `[` of an attribute list whose immediately-preceding sibling is another attribute list on the same line. Native port of StyleCop.Analyzers SA1134 — report-only.

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

A local variable should be camelCase. Fires per declarator whose name begins with an upper-case letter, anchored at the name (a leading underscore is left alone). The local-variable mirror of SA1313. Native port of StyleCop.Analyzers SA1312 — report-only.

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

*Port of StyleCop.Analyzers SA1500 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1500.md)

In Allman style, the opening brace of a multi-line block belongs on its own line. Fires on a multi-line block whose opening `{` shares its line with preceding code (`if (x) {`), anchored at the `{`. A single-line block is SA1501's concern. Native port of StyleCop.Analyzers SA1500 — report-only (the layout fix is the formatter's job).

### `SA1501` — Statement should not be on a single line.

*Port of StyleCop.Analyzers SA1501 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1501.md)

A statement block packed onto one line (`if (c) { Do(); }`) hides control flow and complicates diffs and breakpoints. Fires on a non-empty block written on a single line, anchored at the `{`; an empty single-line block is S108's concern, and method/accessor/lambda bodies are SA1502's — except a single-line local-function body, which is flagged whether empty or not. Native port of StyleCop.Analyzers SA1501 (report-only — wrapping the block is a formatting rewrite, deferred).

### `SA1502` — Element should not be on a single line.

*Port of StyleCop.Analyzers SA1502 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1502.md)

A declaration whose body is on a single line is harder to scan and to edit. Fires on a single-line type body, a method/constructor/operator/local-function block, or a property/indexer/event accessor list with a block-bodied accessor. Auto-properties and expression bodies (which have no block) are exempt. Native port of StyleCop SA1502 — purely syntactic, report-only.

### `SA1503` — Braces should not be omitted.

*Port of StyleCop.Analyzers SA1503 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1503.md)

Omitting braces on a control statement (`if (c) Foo();`) invites bugs when a second line is added later. Fires on a control statement whose body is a single un-braced statement, anchored at that statement; an `else if` continuation is exempt. Native port of StyleCop.Analyzers SA1503 (report-only — adding braces is a structural rewrite).

### `SA1505` — An opening brace should not be followed by a blank line.

*Port of StyleCop.Analyzers SA1505 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1505.md)

A blank line immediately after an opening `{` is wasted space. Fires when an opening brace ends its line and the following line is blank, anchored at the `{`. Native port of StyleCop.Analyzers SA1505 (report-only — removing the blank line is a formatting rewrite).

### `SA1506` — Element documentation headers should not be followed by blank line.

*Port of StyleCop.Analyzers SA1506 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1506.md)

A `///` documentation header should sit directly above the element it documents — a blank line between them detaches the docs. Fires at the blank line (column 1). Native port of StyleCop.Analyzers SA1506 — report-only.

### `SA1507` — Code should not contain multiple blank lines in a row.

*Port of StyleCop.Analyzers SA1507 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1507.md)

More than one blank line in a row is wasted vertical space. Fires once per run of two or more consecutive blank lines, at the first blank line; blank lines inside a multi-line string or block comment are content and are not counted. Native port of StyleCop.Analyzers SA1507 (report-only — collapsing the run is a formatting rewrite, deferred).

### `SA1508` — A closing brace should not be preceded by a blank line.

*Port of StyleCop.Analyzers SA1508 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1508.md)

A blank line immediately before a closing `}` is wasted space. Fires when a closing brace begins its line and the previous line is blank, anchored at the `}` (the mirror of SA1505). Native port of StyleCop.Analyzers SA1508 (report-only — removing the blank line is a formatting rewrite).

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

*Port of StyleCop.Analyzers SA1513 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1513.md)

The closing `}` of a statement block should be set off from the code that follows by a blank line. Fires on a statement-block brace whose next line is code, anchored just past the `}`; a next line that is blank, the end of file, or begins with `}`, `else`, `catch`, `finally`, or `while` is exempt, and method/accessor/lambda bodies are out of scope. Native port of StyleCop.Analyzers SA1513 (report-only — inserting the blank line is a formatting rewrite).

### `SA1514` — Element documentation header should be preceded by a blank line.

*Port of StyleCop.Analyzers SA1514 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1514.md)

A `///` documentation header reads more clearly when separated from the preceding code by a blank line. Fires on a documentation header whose immediately preceding line is code, anchored at the header; a header that is already blank-separated, is the first element after an opening brace, or follows another comment is exempt. Native port of StyleCop.Analyzers SA1514 (report-only — inserting the blank line is a whitespace rewrite, deferred).

### `SA1515` — Single-line comments should be preceded by a blank line.

*Port of StyleCop.Analyzers SA1515 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1515.md)

A `//` comment that introduces a new block of code should be set off from the statement above it by a blank line. Fires on a single-line (`//`) own-line comment whose immediately preceding line is code, anchored at the comment; comments after an opening brace, another comment, or a blank line are exempt, as are `///` docs and `/* */` blocks. Native port of StyleCop.Analyzers SA1515 — report-only.

### `SA1516` — Elements should be separated by a blank line.

*Port of StyleCop.Analyzers SA1516 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1516.md)

Separating adjacent type members with a blank line keeps a type readable. Fires on a member that directly follows the previous member with no blank line between them (the first member and any member with a blank line above are fine; a member's own leading comment counts as part of it). Native port of StyleCop SA1516 — purely syntactic, report-only.

### `SA1517` — Code should not contain blank lines at start of file.

*Port of StyleCop.Analyzers SA1517 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1517.md)

Leading blank lines at the top of a file are noise. Fires when a file begins with one or more blank lines (a newline before the first code), anchored at line 1. The mirror of SA1518. Native port of StyleCop.Analyzers SA1517 (report-only — removing the lines is a formatting rewrite).

### `SA1518` — Code should not contain blank lines at the end of the file.

*Port of StyleCop.Analyzers SA1518 · Style · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1518.md)

Trailing blank lines at the end of a file are noise. Fires when a file ends with one or more blank lines (two or more newlines after the last code), anchored just past the final code character; a single trailing newline is correct. Native port of StyleCop.Analyzers SA1518 (report-only — trimming is a formatting rewrite, deferred).

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

A documented element with an empty `<summary></summary>` documents nothing. Fires on a member requiring documentation whose summary is empty, anchored at the element name (it also reads as undocumented for SA1600). Native port of StyleCop.Analyzers SA1606 — report-only.

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

The text of a documentation element (`<summary>`, `<param>`, `<returns>`, …) should end with a period. Fires at the start of the closing tag for each single-line `/// <tag>text</tag>` whose text does not end in terminal punctuation. Native port of StyleCop.Analyzers SA1629 — report-only.

### `SA1642` — Constructor summary documentation should begin with standard text.

*Port of StyleCop.Analyzers SA1642 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1642.md)

StyleCop requires a documented instance constructor's `<summary>` to read `Initializes a new instance of the <see cref="T"/> class.`. Fires when the summary does not begin with that standard, cref-anchored text, anchored at the `<summary>` element (single-line doc comments only). Native port of StyleCop.Analyzers SA1642 — report-only (rewriting the prose is not a single-range fix).

### `SA1643` — Destructor summary documentation should begin with standard text.

*Port of StyleCop.Analyzers SA1643 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1643.md)

StyleCop requires a documented destructor's `<summary>` to read `Finalizes an instance of the <see cref="T"/> class.`. Fires when the summary does not begin with that standard, cref-anchored text, anchored at the `<summary>` element (single-line doc comments only). The finalizer twin of SA1642. Native port of StyleCop.Analyzers SA1643 — report-only (rewriting the prose is not a single-range fix).

### `SA1651` — Do not use placeholder elements.

*Port of StyleCop.Analyzers SA1651 · Maintainability · report-only* · [upstream docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1651.md)

A `<placeholder>` element marks documentation that was never written. Fires at the `<placeholder>` tag. Native port of StyleCop.Analyzers SA1651 — report-only.

### `VSTHRD100` — Avoid async void methods.

*Port of Microsoft.VisualStudio.Threading.Analyzers VSTHRD100 · Concurrency · report-only* · [upstream docs](https://github.com/microsoft/vs-threading/blob/main/doc/analyzers/VSTHRD100.md)

An `async void` method cannot be awaited and any exception it throws crashes the process instead of propagating to the caller; return `Task` instead. Fires on an `async void` method, anchored at its name. Native port of Microsoft.VisualStudio.Threading.Analyzers VSTHRD100 (report-only — changing the return type ripples into callers).

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

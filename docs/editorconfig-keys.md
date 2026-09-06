# `.editorconfig` key reference

`dotnet-fast` reads `.editorconfig`/`.globalconfig` exactly, case-sensitively — an unrecognised key is a silent no-op. This page lists every one of the **186 keys** the tool resolves, generated straight from the registry that also drives parsing, so it cannot drift from what the tool actually does. For the chain/precedence rules, the severity vocabulary, and worked recipes, read the [configuration guide](editorconfig.md) — this page is the exhaustive reference it links to.

**Default** is the value that reproduces this tool's own unconfigured behaviour, written exactly as you would in `.editorconfig` — not necessarily the real Roslyn/`dotnet format` default, since a handful of preferences default to off here. **Value** with "optional `:severity` suffix" accepts the usual `value:severity` shape, e.g. `true:warning`.

## Core file options

| Key | Value | Default | Description |
|---|---|---|---|
| `charset` | `utf-8` / `utf-8-bom` | `utf-8` | UTF-8 byte-order-mark policy; only these two values are honoured — every other value (`latin1`, `utf-16*`, typos) leaves the file's BOM untouched, since content is never transcoded. |
| `end_of_line` | `lf` / `crlf` | `lf` | Line ending the whitespace pass writes; an unrecognised value leaves each file's existing line endings untouched. |
| `file_header_template` | free text | `` | License/copyright header text inserted and verified at the top of every file (IDE0073); `\n` and `\r\n` escapes in the value are honoured. |
| `indent_size` | non-negative integer | `4` | Number of columns one indent level occupies with `indent_style = space`. |
| `indent_style` | `space` / `tab` | `space` | Whether indentation uses spaces or tabs. |
| `insert_final_newline` | `true` / `false` | `true` | Whether a trailing newline is enforced at end of file; an unrecognised value leaves each file's existing trailing-newline state untouched. |
| `trim_trailing_whitespace` | `true` / `false` | `true` | Whether trailing whitespace is stripped from the end of each line. |

## C# layout & new lines

| Key | Value | Default | Description |
|---|---|---|---|
| `csharp_indent_block_contents` | `true` / `false` | `true` | Whether the contents of a block are indented one level from the braces. |
| `csharp_indent_braces` | `true` / `false` | `false` | Whether the braces themselves are indented one level from their owner. |
| `csharp_indent_case_contents` | `true` / `false` | `true` | Whether the contents of a `switch` `case` are indented one level from the label. |
| `csharp_indent_case_contents_when_block` | `true` / `false` | `true` | Whether a braced block under a `switch` `case` is indented one level from the label. |
| `csharp_indent_labels` | `flush_left` / `one_less_than_current` / `no_change` | `one_less_than_current` | How a `label:` is indented relative to the statement that follows it. |
| `csharp_indent_switch_labels` | `true` / `false` | `true` | Whether `case`/`default` labels are indented one level from the `switch`. |
| `csharp_new_line_before_catch` | `true` / `false` | `true` | Whether `catch` starts its own line. |
| `csharp_new_line_before_else` | `true` / `false` | `true` | Whether `else` starts its own line. |
| `csharp_new_line_before_finally` | `true` / `false` | `true` | Whether `finally` starts its own line. |
| `csharp_new_line_before_members_in_anonymous_types` | `true` / `false` | `true` | Whether each member in an anonymous type starts its own line. |
| `csharp_new_line_before_members_in_object_initializers` | `true` / `false` | `true` | Whether each member in an object initializer starts its own line. |
| `csharp_new_line_before_open_brace` | `all` / `none` | `all` | Whether an opening brace starts its own line; only the literal `none` disables it — any other value (including the real per-construct list) behaves like `all`. |
| `csharp_new_line_between_query_expression_clauses` | `true` / `false` | `true` | Whether each clause of a LINQ query expression starts its own line. |
| `csharp_preserve_single_line_blocks` | `true` / `false` | `true` | Whether a block already written on one line (e.g. `{ }`) is left alone rather than expanded. |
| `csharp_preserve_single_line_statements` | `true` / `false` | `true` | Whether a statement already written on one line is left alone rather than split. |

## C# spacing

| Key | Value | Default | Description |
|---|---|---|---|
| `csharp_space_after_cast` | `true` / `false` | `false` | Space after a cast's closing parenthesis, e.g. `(int) x`. |
| `csharp_space_after_colon_in_inheritance_clause` | `true` / `false` | `true` | Space after the `:` that introduces a base list. |
| `csharp_space_after_comma` | `true` / `false` | `true` | Space after a comma. |
| `csharp_space_after_dot` | `true` / `false` | `false` | Space after a member-access `.`. |
| `csharp_space_after_keywords_in_control_flow_statements` | `true` / `false` | `true` | Space after `if`/`for`/`while`/etc. and before the following `(`. |
| `csharp_space_after_semicolon_in_for_statement` | `true` / `false` | `true` | Space after each `;` in a `for` statement's header. |
| `csharp_space_around_binary_operators` | `before_and_after` / `none` / `ignore` | `before_and_after` | Spacing around binary operators (`+`, `==`, …). |
| `csharp_space_around_declaration_statements` | `do_not_ignore` / `ignore` | `do_not_ignore` | Whether extra spacing inside a declaration statement is removed (`do_not_ignore`) or left as written (`ignore`). |
| `csharp_space_before_colon_in_inheritance_clause` | `true` / `false` | `true` | Space before the `:` that introduces a base list. |
| `csharp_space_before_comma` | `true` / `false` | `false` | Space before a comma. |
| `csharp_space_before_dot` | `true` / `false` | `false` | Space before a member-access `.`. |
| `csharp_space_before_open_square_brackets` | `true` / `false` | `false` | Space before an opening `[`. |
| `csharp_space_before_semicolon_in_for_statement` | `true` / `false` | `false` | Space before each `;` in a `for` statement's header. |
| `csharp_space_between_empty_square_brackets` | `true` / `false` | `false` | Space between `[` and `]` when the brackets are empty. |
| `csharp_space_between_method_call_empty_parameter_list_parentheses` | `true` / `false` | `false` | Space between a method call's parentheses when the argument list is empty. |
| `csharp_space_between_method_call_name_and_opening_parenthesis` | `true` / `false` | `false` | Space between a method call's name and its opening parenthesis. |
| `csharp_space_between_method_call_parameter_list_parentheses` | `true` / `false` | `false` | Space just inside a method call's non-empty argument-list parentheses. |
| `csharp_space_between_method_declaration_empty_parameter_list_parentheses` | `true` / `false` | `false` | Space between a method declaration's parentheses when the parameter list is empty. |
| `csharp_space_between_method_declaration_name_and_open_parenthesis` | `true` / `false` | `false` | Space between a method declaration's name and its opening parenthesis. |
| `csharp_space_between_method_declaration_parameter_list_parentheses` | `true` / `false` | `false` | Space just inside a method declaration's non-empty parameter-list parentheses. |
| `csharp_space_between_parentheses` | comma-separated list of tokens | `none` | Which parenthesized contexts get inner padding spaces — a comma-separated subset of `control_flow_statements`, `expressions`, `type_casts`; any token not in that set (including `none`) enables nothing. |
| `csharp_space_between_square_brackets` | `true` / `false` | `false` | Space just inside non-empty `[` … `]`. |

## .NET style preferences (`dotnet_style_*`)

| Key | Value | Default | Description |
|---|---|---|---|
| `dotnet_prefer_system_hash_code` | `true` / `false`, optional `:severity` suffix | `false` | IDE0070: prefer `System.HashCode.Combine` over a hand-written hash combiner. |
| `dotnet_sort_system_directives_first` | `true` / `false` | `true` | Whether `System.*` usings sort before others when usings are organized. |
| `dotnet_style_allow_multiple_blank_lines_experimental` | `true` / `false` | `true` | IDE2000: `= false` collapses runs of 2+ consecutive blank lines to one; any other value leaves blank lines alone. |
| `dotnet_style_coalesce_expression` | `true` / `false`, optional `:severity` suffix | `false` | IDE0029/IDE0030: prefer `??` over an equivalent null-check/ternary. |
| `dotnet_style_collection_initializer` | `true` / `false`, optional `:severity` suffix | `false` | IDE0028: prefer a collection initializer over separate `Add` calls. |
| `dotnet_style_explicit_tuple_names` | `true` / `false`, optional `:severity` suffix | `false` | IDE0033: prefer an explicitly provided tuple element name over `ItemN`. |
| `dotnet_style_null_propagation` | `true` / `false`, optional `:severity` suffix | `false` | Prefer `?.`/`?[]` over an equivalent null check. |
| `dotnet_style_object_initializer` | `true` / `false`, optional `:severity` suffix | `false` | IDE0017: prefer an object initializer over separate property assignments. |
| `dotnet_style_parentheses_in_arithmetic_binary_operators` | `always_for_clarity` / `never_if_unnecessary`, optional `:severity` suffix | `always_for_clarity` | Parenthesization of arithmetic binary operators (`*`, `+`, …). This parser has no "unset" state — any value, recognised or not, resolves to one of the two preferences (`always_for_clarity` is the fallback), which is why the generated reference's default here is this tool's fallback rather than an absent-key state. |
| `dotnet_style_parentheses_in_other_binary_operators` | `always_for_clarity` / `never_if_unnecessary`, optional `:severity` suffix | `always_for_clarity` | Parenthesization of the remaining binary operators (`&&`, `??`, …); same always-resolves parsing as the arithmetic key above. |
| `dotnet_style_parentheses_in_other_operators` | `always_for_clarity` / `never_if_unnecessary`, optional `:severity` suffix | `always_for_clarity` | Parenthesization of the remaining (non-binary) operators; same always-resolves parsing as the arithmetic key above. |
| `dotnet_style_parentheses_in_relational_binary_operators` | `always_for_clarity` / `never_if_unnecessary`, optional `:severity` suffix | `always_for_clarity` | Parenthesization of relational binary operators (`<`, `==`, …); same always-resolves parsing as the arithmetic key above. |
| `dotnet_style_predefined_type_for_locals_parameters_members` | `true` / `false`, optional `:severity` suffix | `false` | Whether locals/parameters/members prefer a predefined type keyword (`int`) over the framework type name (`Int32`). |
| `dotnet_style_predefined_type_for_member_access` | `true` / `false`, optional `:severity` suffix | `false` | Whether a static-member access prefers a predefined type keyword over the framework type name. |
| `dotnet_style_prefer_auto_properties` | `true` / `false`, optional `:severity` suffix | `false` | IDE0032: prefer an auto-property over a manually backed one with no extra logic. |
| `dotnet_style_prefer_collection_expression` | `true` / `false` / `when_types_exactly_match` / `when_types_loosely_match`, optional `:severity` suffix | `false` | IDE0300-IDE0306: prefer a `[...]` collection expression over the equivalent constructor/initializer form. |
| `dotnet_style_prefer_compound_assignment` | `true` / `false`, optional `:severity` suffix | `false` | Prefer `x += y` over `x = x + y`. |
| `dotnet_style_prefer_conditional_expression_over_assignment` | `true` / `false`, optional `:severity` suffix | `false` | IDE0045: prefer `x = cond ? a : b` over the equivalent `if`/`else` assignment. |
| `dotnet_style_prefer_conditional_expression_over_return` | `true` / `false`, optional `:severity` suffix | `false` | IDE0046: prefer `return cond ? a : b` over the equivalent `if`/`else` return. |
| `dotnet_style_prefer_inferred_anonymous_type_member_names` | `true` / `false`, optional `:severity` suffix | `false` | IDE0037: prefer an inferred anonymous-type member name. |
| `dotnet_style_prefer_inferred_tuple_names` | `true` / `false`, optional `:severity` suffix | `false` | IDE0037: prefer an inferred tuple element name. |
| `dotnet_style_prefer_is_null_check_over_reference_equality_method` | `true` / `false`, optional `:severity` suffix | `false` | IDE0041: prefer `is null` over `object.ReferenceEquals(x, null)`. |
| `dotnet_style_prefer_simplified_boolean_expressions` | `true` / `false`, optional `:severity` suffix | `false` | IDE0075: prefer a simplified conditional boolean expression. |
| `dotnet_style_prefer_simplified_interpolation` | `true` / `false`, optional `:severity` suffix | `false` | IDE0071: prefer a simplified interpolated-string expression. |
| `dotnet_style_qualification_for_event` | `true` / `false`, optional `:severity` suffix | `false` | Whether event access is qualified with `this.`. |
| `dotnet_style_qualification_for_field` | `true` / `false`, optional `:severity` suffix | `false` | Whether field access is qualified with `this.`. |
| `dotnet_style_qualification_for_method` | `true` / `false`, optional `:severity` suffix | `false` | Whether method calls are qualified with `this.`. |
| `dotnet_style_qualification_for_property` | `true` / `false`, optional `:severity` suffix | `false` | Whether property access is qualified with `this.`. |
| `dotnet_style_readonly_field` | `true` / `false`, optional `:severity` suffix | `false` | IDE0044: prefer marking a field `readonly` when it is never reassigned outside its constructor. |
| `dotnet_style_require_accessibility_modifiers` | `always` / `for_non_interface_members` / `never` / `omit_if_default`, optional `:severity` suffix | `for_non_interface_members` | IDE0040: when an explicit accessibility modifier is required. |

## C# style preferences (`csharp_style_*`)

| Key | Value | Default | Description |
|---|---|---|---|
| `csharp_style_allow_blank_lines_between_consecutive_braces_experimental` | `true` / `false` | `true` | IDE2002: `= false` removes a blank line sitting between two consecutive closing braces; any other value leaves it alone. |
| `csharp_style_conditional_delegate_call` | `true` / `false`, optional `:severity` suffix | `false` | IDE1005: prefer `delegate?.Invoke()` over a manual null check before invoking. |
| `csharp_style_deconstructed_variable_declaration` | `true` / `false`, optional `:severity` suffix | `false` | IDE0042: prefer a deconstructed variable declaration over separate `.Item1`/`.Item2` access. |
| `csharp_style_implicit_object_creation_when_type_is_apparent` | `true` / `false`, optional `:severity` suffix | `false` | IDE0090: prefer target-typed `new(...)` when the type is already apparent. |
| `csharp_style_inlined_variable_declaration` | `true` / `false`, optional `:severity` suffix | `false` | IDE0018: prefer an inline `out` variable declaration. |
| `csharp_style_namespace_declarations` | `file_scoped` / `block_scoped`, optional `:severity` suffix | `` | IDE0160/IDE0161: file-scoped vs. block-scoped namespace declarations; an unrecognised value (including absence) leaves neither preference configured. |
| `csharp_style_pattern_matching_over_as_with_null_check` | `true` / `false`, optional `:severity` suffix | `false` | IDE0019: prefer `is` pattern matching over `as` followed by a null check. |
| `csharp_style_pattern_matching_over_is_with_cast_check` | `true` / `false`, optional `:severity` suffix | `false` | IDE0020: prefer `is` pattern matching over an `is` type check followed by a separate cast. |
| `csharp_style_prefer_extended_property_pattern` | `true` / `false`, optional `:severity` suffix | `false` | IDE0170: prefer an extended (`a.b: pattern`) property pattern over a nested one. |
| `csharp_style_prefer_implicitly_typed_lambda_expression` | `true` / `false`, optional `:severity` suffix | `false` | IDE0350: prefer an implicitly (rather than explicitly) typed lambda expression. |
| `csharp_style_prefer_index_operator` | `true` / `false`, optional `:severity` suffix | `false` | IDE0056: prefer the `^` index-from-end operator. |
| `csharp_style_prefer_local_over_anonymous_function` | `true` / `false`, optional `:severity` suffix | `false` | Prefer a local function over an equivalent anonymous function/lambda assigned to a variable. |
| `csharp_style_prefer_method_group_conversion` | `true` / `false`, optional `:severity` suffix | `false` | Prefer a method-group conversion over an equivalent lambda that only forwards its arguments. |
| `csharp_style_prefer_not_pattern` | `true` / `false`, optional `:severity` suffix | `false` | IDE0083: prefer the `not` pattern over a negated pattern/expression. |
| `csharp_style_prefer_null_check_over_type_check` | `true` / `false`, optional `:severity` suffix | `false` | IDE0150: prefer `is null`/`is not null` over an `is` type-check pattern that only tests nullity. |
| `csharp_style_prefer_pattern_matching` | `true` / `false`, optional `:severity` suffix | `false` | General preference for pattern matching over the older `is`/cast idioms. |
| `csharp_style_prefer_primary_constructors` | `true` / `false`, optional `:severity` suffix | `false` | IDE0290: prefer a primary constructor over an equivalent explicit one. |
| `csharp_style_prefer_range_operator` | `true` / `false`, optional `:severity` suffix | `false` | IDE0057: prefer the `..` range operator. |
| `csharp_style_prefer_simple_property_accessors` | `true` / `false`, optional `:severity` suffix | `false` | IDE0360: prefer a simple (non-block-bodied) property accessor. |
| `csharp_style_prefer_switch_expression` | `true` / `false`, optional `:severity` suffix | `false` | IDE0066: prefer a switch expression over an equivalent switch statement. |
| `csharp_style_prefer_tuple_swap` | `true` / `false`, optional `:severity` suffix | `false` | IDE0180: prefer `(a, b) = (b, a)` over a temporary-variable swap. |
| `csharp_style_prefer_unbound_generic_type_in_nameof` | `true` / `false`, optional `:severity` suffix | `false` | IDE0340: prefer an unbound generic type (`nameof(List<>)`) inside `nameof`. |
| `csharp_style_prefer_utf8_string_literals` | `true` / `false`, optional `:severity` suffix | `false` | IDE0230: prefer a UTF-8 string literal (`"..."u8`) over a byte-array initializer. |
| `csharp_style_throw_expression` | `true` / `false`, optional `:severity` suffix | `false` | IDE0016: prefer a throw expression over an equivalent throw statement. |
| `csharp_style_unused_value_assignment_preference` | `discard_variable` / `unused_local_variable`, optional `:severity` suffix | `discard_variable` | IDE0059: how an unnecessary value assignment is captured. |
| `csharp_style_unused_value_expression_statement_preference` | `discard_variable` / `unused_local_variable`, optional `:severity` suffix | `discard_variable` | IDE0058: how an unused expression-statement value is captured. |
| `csharp_style_var_elsewhere` | `true` / `false`, optional `:severity` suffix | `false` | IDE0007/IDE0008: prefer `var` everywhere else. |
| `csharp_style_var_for_built_in_types` | `true` / `false`, optional `:severity` suffix | `false` | IDE0007/IDE0008: prefer `var` (vs. the explicit predefined type keyword) for a built-in type. |
| `csharp_style_var_when_type_is_apparent` | `true` / `false`, optional `:severity` suffix | `false` | IDE0007/IDE0008: prefer `var` when the right-hand side already makes the type apparent (e.g. a `new` expression). |

## C# preferences

| Key | Value | Default | Description |
|---|---|---|---|
| `csharp_prefer_braces` | `true` / `false`, optional `:severity` suffix | `false` | IDE0011: prefer braces around a single-statement `if`/`for`/`while`/etc. body. |
| `csharp_prefer_simple_default_expression` | `true` / `false`, optional `:severity` suffix | `false` | IDE0034: prefer `default` over `default(T)` when the type is inferable. |
| `csharp_prefer_simple_using_statement` | `true` / `false`, optional `:severity` suffix | `false` | IDE0063: prefer a simple (braceless) `using` statement over one wrapping a block. |
| `csharp_prefer_system_threading_lock` | `true` / `false`, optional `:severity` suffix | `false` | IDE0330: prefer `System.Threading.Lock` over `lock` on a plain `object`. |
| `csharp_preferred_modifier_order` | comma-separated, ordered list of modifiers | `public,private,protected,internal,file,static,extern,new,virtual,abstract,sealed,override,readonly,unsafe,required,volatile,async` | IDE0036: the required order of member modifiers (`public`, `static`, …); a modifier omitted from the list keeps its default rank, an unrecognised token is ignored. |
| `csharp_style_expression_bodied_accessors` | `true` / `false` / `when_on_single_line`, optional `:severity` suffix | `false` | IDE0027: prefer an expression-bodied property/indexer accessor. |
| `csharp_style_expression_bodied_constructors` | `true` / `false` / `when_on_single_line`, optional `:severity` suffix | `false` | IDE0021: prefer an expression-bodied constructor. |
| `csharp_style_expression_bodied_indexers` | `true` / `false` / `when_on_single_line`, optional `:severity` suffix | `false` | IDE0026: prefer an expression-bodied indexer. |
| `csharp_style_expression_bodied_lambdas` | `true` / `false` / `when_on_single_line`, optional `:severity` suffix | `false` | IDE0053: prefer an expression-bodied lambda. |
| `csharp_style_expression_bodied_local_functions` | `true` / `false` / `when_on_single_line`, optional `:severity` suffix | `false` | IDE0061: prefer an expression-bodied local function. |
| `csharp_style_expression_bodied_methods` | `true` / `false` / `when_on_single_line`, optional `:severity` suffix | `false` | IDE0022: prefer an expression-bodied method. |
| `csharp_style_expression_bodied_operators` | `true` / `false` / `when_on_single_line`, optional `:severity` suffix | `false` | IDE0024: prefer an expression-bodied operator overload. |
| `csharp_style_expression_bodied_properties` | `true` / `false` / `when_on_single_line`, optional `:severity` suffix | `false` | IDE0025: prefer an expression-bodied property. |
| `csharp_using_directive_placement` | `outside_namespace` / `inside_namespace`, optional `:severity` suffix | `outside_namespace` | IDE0065: where `using` directives are placed relative to the namespace. |

## `dotnet-fast` guardrail thresholds

| Key | Value | Default | Description |
|---|---|---|---|
| `dotnet_fast_magic_number_allowed` | comma-separated list of tokens | `0,1,-1,2` | DF9004: numeric literals exempt from the "no unnamed magic numbers" rule, as a comma-separated list. |
| `dotnet_fast_max_cognitive_complexity` | positive integer | `22` | DF9007 threshold: a member over this cognitive complexity is reported. Scored by the same code `S3776` and `dotnet-fast metrics` use, so the gate and the scoreboard cannot disagree about a member. |
| `dotnet_fast_max_cyclomatic_complexity` | positive integer | `22` | DF9005 threshold: a member over this cyclomatic complexity is reported. |
| `dotnet_fast_max_halstead_difficulty` | positive number | `80` | DF9006 threshold: a member over this Halstead difficulty is reported. A non-finite or non-positive value keeps the previous (or default) limit. |
| `dotnet_fast_max_lines_per_file` | positive integer | `250` | DF9003 threshold: a file over this many non-blank lines is reported. Distinct from `metrics.maxLinesPerFile` in `dotnet-fast.json` (default 500) — the two never read each other. |
| `dotnet_fast_max_lines_per_function` | positive integer | `50` | DF9002 threshold: a member over this many lines is reported. An unparseable or non-positive value keeps the previous (or default) limit. |

## Diagnostic severities (`dotnet_diagnostic.IDE####.severity`)

| Key | Value | Default | Description |
|---|---|---|---|
| `dotnet_diagnostic.IDE0004.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Remove unnecessary cast. |
| `dotnet_diagnostic.IDE0005.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Remove unnecessary imports. |
| `dotnet_diagnostic.IDE0009.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Add this qualification. |
| `dotnet_diagnostic.IDE0010.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Add missing switch cases. |
| `dotnet_diagnostic.IDE0016.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use throw expression. |
| `dotnet_diagnostic.IDE0017.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use object initializer. |
| `dotnet_diagnostic.IDE0019.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use pattern matching. |
| `dotnet_diagnostic.IDE0020.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use pattern matching. |
| `dotnet_diagnostic.IDE0028.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use collection initializer. |
| `dotnet_diagnostic.IDE0029.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use coalesce expression. |
| `dotnet_diagnostic.IDE0030.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use coalesce expression. |
| `dotnet_diagnostic.IDE0032.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use auto property. |
| `dotnet_diagnostic.IDE0033.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use explicitly provided tuple name. |
| `dotnet_diagnostic.IDE0036.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Order modifiers. |
| `dotnet_diagnostic.IDE0039.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use local function. |
| `dotnet_diagnostic.IDE0040.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Add accessibility modifiers. |
| `dotnet_diagnostic.IDE0042.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use deconstructed variable declaration. |
| `dotnet_diagnostic.IDE0044.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Make field readonly. |
| `dotnet_diagnostic.IDE0047.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Remove unnecessary parentheses. |
| `dotnet_diagnostic.IDE0048.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Add parentheses for clarity. |
| `dotnet_diagnostic.IDE0051.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Remove unused private member. |
| `dotnet_diagnostic.IDE0053.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use expression body for lambdas. |
| `dotnet_diagnostic.IDE0056.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use index operator. |
| `dotnet_diagnostic.IDE0057.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use range operator. |
| `dotnet_diagnostic.IDE0058.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Remove unused expression value. |
| `dotnet_diagnostic.IDE0059.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Remove unnecessary value assignment. |
| `dotnet_diagnostic.IDE0063.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use simple using statement. |
| `dotnet_diagnostic.IDE0065.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Place using directives inside or outside namespace. |
| `dotnet_diagnostic.IDE0066.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use switch expression. |
| `dotnet_diagnostic.IDE0070.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use System.HashCode.Combine. |
| `dotnet_diagnostic.IDE0072.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Add missing switch expression cases. |
| `dotnet_diagnostic.IDE0073.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Add file header. |
| `dotnet_diagnostic.IDE0078.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use pattern matching. |
| `dotnet_diagnostic.IDE0080.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Remove unnecessary suppression operator. |
| `dotnet_diagnostic.IDE0082.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use nameof operator. |
| `dotnet_diagnostic.IDE0083.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use not pattern. |
| `dotnet_diagnostic.IDE0100.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Remove unnecessary equality operator. |
| `dotnet_diagnostic.IDE0110.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Remove unnecessary discard. |
| `dotnet_diagnostic.IDE0120.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Simplify LINQ expression. |
| `dotnet_diagnostic.IDE0121.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Simplify LINQ type check and cast. |
| `dotnet_diagnostic.IDE0150.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use null check. |
| `dotnet_diagnostic.IDE0160.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use block-scoped namespace. |
| `dotnet_diagnostic.IDE0161.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use file-scoped namespace. |
| `dotnet_diagnostic.IDE0170.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Simplify property pattern. |
| `dotnet_diagnostic.IDE0180.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use tuple to swap values. |
| `dotnet_diagnostic.IDE0200.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Remove unnecessary lambda expression. |
| `dotnet_diagnostic.IDE0230.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use UTF-8 string literal. |
| `dotnet_diagnostic.IDE0260.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use pattern matching. |
| `dotnet_diagnostic.IDE0270.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use coalesce expression for null check. |
| `dotnet_diagnostic.IDE0280.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use nameof for parameter name. |
| `dotnet_diagnostic.IDE0290.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use primary constructor. |
| `dotnet_diagnostic.IDE0300.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use collection expression. |
| `dotnet_diagnostic.IDE0301.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use collection expression for empty collection. |
| `dotnet_diagnostic.IDE0302.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use collection expression for stackalloc. |
| `dotnet_diagnostic.IDE0303.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use collection expression for Create. |
| `dotnet_diagnostic.IDE0304.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use collection expression for builder. |
| `dotnet_diagnostic.IDE0305.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use collection expression for fluent calls. |
| `dotnet_diagnostic.IDE0306.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use collection expression for new. |
| `dotnet_diagnostic.IDE0330.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use System.Threading.Lock. |
| `dotnet_diagnostic.IDE0340.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use unbound generic type in nameof. |
| `dotnet_diagnostic.IDE0350.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use implicitly typed lambda. |
| `dotnet_diagnostic.IDE0360.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use simple property accessor. |
| `dotnet_diagnostic.IDE1005.severity` | severity keyword: `silent`/`hidden`, `suggestion`/`info`, `warning`/`warn`, `error`, or `none` | `none` | Use conditional delegate call. |

## Not read

Keys this tool does not read at all — listed so silence does not read as support:

| Key | Why |
|---|---|
| `tab_width` | Deliberate divergence: with `indent_size = tab`, this tool emits one tab per indent level (honouring the author's tab intent) rather than the multiple-of-`tab_width` spaces `dotnet format` computes. |
| `max_line_length` | Not read. No pass wraps or reports on line length. |
| `dotnet_naming_*` | Not read. Naming-convention rules and their `dotnet_naming_rule.*` / `dotnet_naming_symbols.*` / `dotnet_naming_style.*` keys are silently ignored — a very common expectation this tool does not yet meet. |
| `dotnet_separated_import_directive_groups` | Not read, and deliberately: SDK-probed, `dotnet format` itself does not apply this key either (`true` vs `false` produce byte-identical output). Implementing it would diverge from the real tool, so it is accepted as a no-op to match. |

---

_This page is generated from the key catalog; see `tests/editorconfig_keys_doc.rs`. To propose a new key, open an issue._

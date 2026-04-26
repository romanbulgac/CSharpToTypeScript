# Change Log

## 2.0.0 (2026-04-27)

### 🏗️ Platform Modernization

- Migrated server runtime from .NET 6 to **.NET 10**
- Upgraded Microsoft.CodeAnalysis.CSharp to **5.3.0** (full C# 13 support)
- Removed Newtonsoft.Json dependency → native System.Text.Json
- Upgraded TypeScript to 5.x, VS Code engine to 1.85+
- Dynamic server publish path resolution (runtime-independent)

### ✨ New Features

- **Export one file per type** (#5) — classes, interfaces, and enums from a single C# file are exported into separate TypeScript files
- **XML Summary tag support** (#38) — C# `<summary>` tags are converted into JSDoc comments (`/** ... */`) for types, properties, and enum members
- **Records with primary constructors** — `record Person(string Name, int Age)` positional parameters are converted into TypeScript properties
- **Nullable reference types** (#24) — `string?`, `Address?`, `List<string>?` correctly produce `| null` unions for non-primitive types
- **Validation attributes as JSDoc** — `[Required]`, `[MaxLength]`, `[Range]`, `[EmailAddress]`, `[Phone]`, `[Url]`, `[RegularExpression]`, `[StringLength]` are converted into JSDoc `@tags`
- **`DateOnly` / `TimeOnly` support** (#48) — converted to `string` in TypeScript
- **`required` keyword support** (#58) — C# 11+ `required` properties are handled correctly
- **String constants as literal types** (#66, #63) — `const string` and `static readonly` fields with literal values are converted into TypeScript literal types
- **Original name as comment** (#13) — option to add the original C# name as a JSDoc comment above each property
- **Output as `type`** (#62) — new option to generate `type` instead of `interface`
- **CamelCase with acronyms** (#49) — correct handling of acronyms (e.g. `HTTPSUrl` → `httpsUrl`)
- **`using` aliases support** (#34) — C# type aliases are resolved correctly

### 🧪 Testing

- Complete E2E test suite with 20 fixture-based scenarios
- Coverage: primitives, nullable, collections, dictionaries, tuples, enums, JSON attributes, generics, inheritance, records, constants, summary tags

### 🐛 Bug Fixes

- Fully-qualified `System.` types are resolved correctly (e.g. `System.String` → `string`)
- C# tuples are converted correctly to TypeScript
- Enums with explicit values are serialized properly

---

## 1.0.0

- Initial release (fork of adrianwilczynski/CSharpToTypeScript)

---

## 🔮 Roadmap — Future Ideas

### High Priority

- **Records with primary constructors** — `record Person(string Name, int Age)` → constructor parameters converted into TypeScript properties
- **Full nullable reference types** (#24) — `string?` on non-primitive properties should generate `Type | null` or `prop?: Type`
- **Custom interface prefix/suffix** (#25) — option to add a suffix (e.g. `IUser` → `UserDto`) or keep the prefix

### Medium Priority

- **Custom type mapping** (#26) — configurable type mappings (e.g. `Money` → `number`, `UserId` → `string`)
- **TSX support** (#28) — direct generation into `.tsx` files
- **Chinese character support** (#37) — fix parsing errors with non-ASCII characters in identifiers
- **Capital letter output file name** (#39) — `PascalCase` option for generated file names

### Experimental Ideas

- **Zod schema generation** — alongside the TypeScript interface, optionally generate Zod schemas for runtime validation
- **Barrel exports** — auto-generate `index.ts` with re-exports for all types in a folder
- **API Client stub** — based on C# controllers with `[HttpGet]`/`[HttpPost]`, generate typed fetch/axios functions
- **Inline enum → union type** — option to convert `enum Status { Active, Inactive }` into `type Status = "Active" | "Inactive"` (string literal union)
- **`[Description]` / `[Display]` support** — metadata attributes converted into additional JSDoc comments
- **Watch mode** — automatically monitor `.cs` files and regenerate TypeScript on every save

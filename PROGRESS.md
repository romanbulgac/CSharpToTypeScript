# CSharpToTypeScript Fork Progress Tracker

Last updated: 2026-04-26

## Goals

- Keep and modernize the VS Code extension.
- Keep .NET server architecture and make it reliable on modern machines.
- Add end-to-end conversion tests from C# input to TypeScript output.
- Triage and gradually close upstream open issues with tests first.

## Status Legend

- [ ] Backlog
- [-] In progress
- [x] Done
- [!] Blocked

## Current Workstreams

- [x] Repository scoped to VS Code extension only.
- [x] .NET server modernized to net8 and build pipeline updated.
- [x] Extension startup made runtime-agnostic for server publish folder.
- [x] Package/tooling modernization (TypeScript, VS Code test package, deps).
- [-] Build and diagnostics validation after modernization.
- [ ] E2E test expansion (C# files -> extension command -> generated TS validation).
- [ ] Open-issues remediation with test coverage.

## Milestones

### M1: Stabilize Modernized Extension

- [x] Build extension with npm run vscode:prepublish.
- [x] Publish .NET server successfully on net8.
- [x] Replace deprecated JSON stack in server.
- [ ] Run extension integration tests fully green after modernization.

### M2: E2E Conversion Test Pack

- [ ] Add fixture-based E2E tests with real C# files and expected TS snapshots.
- [ ] Cover primitive, nullable, arrays, collections, dictionaries, tuples, enums.
- [ ] Cover settings-dependent behavior (camelCase, imports, nullables, quotes, file name transforms).
- [ ] Add regression tests mapped to selected open issues.

### M3: Issue-Driven Improvements

- [ ] Prioritize top impact issues and define acceptance criteria.
- [ ] Implement fixes with tests.
- [ ] Re-run full test suite and validate no regressions.

## E2E Test Matrix

| Scenario | Status | Test File | Notes |
|---|---|---|---|
| Convert class with string/int props | [x] | src/CSharpToTypeScript.VSCodeExtension/src/test/suite/extension.test.ts | Basic flow covered via command tests |
| Convert selected code fragment | [x] | src/CSharpToTypeScript.VSCodeExtension/src/test/suite/extension.test.ts | Existing command behavior |
| Convert clipboard content | [x] | src/CSharpToTypeScript.VSCodeExtension/src/test/suite/extension.test.ts | Existing command behavior |
| Convert C# file to TS file | [x] | src/CSharpToTypeScript.VSCodeExtension/src/test/suite/extension.test.ts | Existing command behavior |
| Primitive type mapping matrix | [x] | `primitive-and-nullable`, `system-qualified-types` | Add fixture-based assertions |
| Nullable mapping (null vs undefined mode) | [x] | `nullable-and-date-union` | Add config-aware tests |
| Date mapping (string/date/union) | [x] | `dateonly-timeonly`, `nullable-and-date-union` | Add config-aware tests |
| Collection and dictionary mapping | [x] | `collections-and-dictionary` | Add List/Array/Dictionary cases |
| Tuple mapping | [x] | `tuple-and-enum` | Add tuple and System.Tuple coverage |
| Enum conversion | [x] | `enum-as-values` | Add numeric and edge cases |
| JSON attributes mapping | [x] | `json-attributes` | JsonPropertyName/JsonProperty/JsonIgnore |
| New C# syntax compatibility (required/record) | [x] | `required-keyword`, `record-with-body` | Regression-focused tests |

## Upstream Open Issues Tracker (Imported)

| Issue | Title | Status | Test Added | Fix Status | Notes |
|---|---|---|---|---|---|
| #66 | Const strings are not converted | 🟢 Passes | [x] | [x] | Outputs as literal types |
| #65 | Is there an option to enable convert methods? | Open | [ ] | [ ] | |
| #64 | String enums for the command line interface | Open | [ ] | [ ] | |
| #63 | Fixed string to constant | 🟢 Passes | [x] | [x] | Outputs as literal types |
| #62 | Output type "type" instead of interface | Open | [ ] | [ ] | |
| #58 | Required keyword in property declarations fails | 🟢 Passes | [x] | [x] | Fixed previously via C# 12 Roslyn upgrade |
| #49 | Property Name Not Match CamelCase | 🟢 Passes | [x] | [x] | Full acronym support using standard logic |
| #48 | DateOnly not converting to string | 🟢 Passes | [x] | [x] | Fixed previously |
| #45 | CLI tool improperly recognizing directories | Open | [ ] | [ ] | Out of extension scope unless shared core behavior |
| #44 | Multiple directories/files support | Open | [ ] | [ ] | Out of extension scope unless shared core behavior |
| #39 | Output file name begin with capital letter | Open | [ ] | [ ] | |
| #38 | Summary tag support | Open | [ ] | [ ] | |
| #37 | Some Chinese characters trigger error | Open | [ ] | [ ] | |
| #34 | `using` aliases | 🟢 Passes | [ ] | [ ] | |
| `DateOnly` / `TimeOnly` | `dateonly-timeonly` | 🟢 Passes | [ ] | [ ] | |
| Tuple formatting | `tuple-and-enum` | 🟢 Passes | [ ] | [ ] | |
| Output `enum` as values | `enum-as-values` | 🟢 Passes | [ ] | [ ] | |
| #28 | TSX support | Open | [ ] | [ ] | |
| #26 | Option to add custom type mapping | Open | [ ] | [ ] | |
| #25 | Option to add suffix for interface names | Open | [ ] | [ ] | |
| #24 | Nullable for non-primitive fields | Open | [ ] | [ ] | |
| #19 | Support dotnet 5 | Open | [x] | [x] | Migrated further to net8 |
| #13 | Using variable name as TypeScript comment | Open | [ ] | [ ] | |
| #5 | Generate one class/interface/enum per file | Open | [ ] | [ ] | |

## Session Log

### 2026-04-26

- [x] Repo reduced to extension-only structure.
- [x] Core embedded under extension server for self-contained build.
- [x] .NET server upgraded to net8 and Newtonsoft removed.
- [x] Extension updated to resolve server publish path dynamically.
- [x] Tooling upgraded (TypeScript 5.x, VS Code engine 1.85, modern test package).
- [x] Full prepublish build succeeded.
- [-] Next: add fixture-based E2E tests and start closing issue regressions with tests first.

## Next Actions

1. Create fixture folders for C# inputs and expected TS outputs.
2. Add test utilities to run conversion commands and compare full output snapshots.
3. Start with issue-focused regression tests: #58, #48, #31, #66, #37.
4. Implement fixes only after tests reproduce each issue.

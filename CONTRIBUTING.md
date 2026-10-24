# Contributing to systemd-service-monitor

Thanks for contributing! This is an ASP.NET Core (.NET 10) app that monitors and controls systemd services over D-Bus.

## Prerequisites

- **.NET SDK 10.0** (`global.json` pins `10.0.100` with `rollForward: latestMinor`)
- Linux with systemd/D-Bus for running the app against real services (tests do not need it)
- Optional: `make` (wraps the commands below; see `Makefile`)

## Solution layout

- `systemd-service-monitor.sln` - main project plus test projects
- `systemd-service-monitor.csproj` - the web app (`Program.cs`, `Controllers/`, `Services/`, `Integration/`, `Pages/`, ...)
- `tests/systemd-service-monitor.Tests/` - xUnit tests (FluentAssertions, NSubstitute, Moq, `Microsoft.AspNetCore.Mvc.Testing`)
- `tests/systemd-service-monitor.Benchmarks/` - BenchmarkDotNet project (not run in CI)
- `tests/**` and `examples/**` are excluded from the main project's compile

## Build

```bash
dotnet restore systemd-service-monitor.sln
dotnet build systemd-service-monitor.sln -c Release
```

Run the app in development: `dotnet run` (Swagger is served at `/swagger` in Development).

## Test

```bash
dotnet test systemd-service-monitor.sln                 # all tests
dotnet test --filter "FullyQualifiedName~PathResolverTests"   # one class
```

Benchmarks: `dotnet run -c Release --project tests/systemd-service-monitor.Benchmarks`.

## Coding conventions

- Style comes from `.editorconfig`: UTF-8, LF line endings, 4-space indent, Allman braces, block-scoped namespaces, `this.` qualification removed. Run `dotnet format` before committing.
- `Nullable` and `ImplicitUsings` are enabled (`Directory.Build.props`). Files start with `#nullable enable`.
- Root namespace is `SystemdServiceMonitor`; sub-namespaces match folders. One public type per file, file name = type name.
- Interfaces are prefixed `I` and live next to their implementation (`IServiceMonitorService` / `ServiceMonitorService`).
- DI: services scoped, repositories singleton, options via `IOptions<T>`.
- Controllers return `ApiResponse<T>`; exceptions derive from `ServiceMonitorException`.
- Logging through `ILogger<T>` with structured message templates.
- Public APIs carry XML doc comments (`CS1591` is suppressed, but keep docs on public members).
- Tests: `<Class>Tests.cs`, methods named `Method_ShouldExpectedBehavior`, Arrange/Act/Assert comments, `[Theory]`/`[InlineData]` for cases, FluentAssertions `.Should()`.

## Pull requests

1. Branch from `main` (e.g. `feat/...`, `fix/...`).
2. Keep PRs small and focused on one change; prefer several small commits over one large one.
3. Add or update tests for behavior changes.
4. Make sure `dotnet build` and `dotnet test` pass locally. CI (`.github/workflows/`) runs restore, Release build and tests on every PR to `main`, and it must be green before merge.
5. In the PR description, explain what changed and why, and how you verified it.
6. Do not commit `bin/`, `obj/`, `publish/`, `logs/`, `.aider*`, or `appsettings.Development.json`.

By contributing, you agree that your contributions are licensed under the MIT License (see `LICENSE`).

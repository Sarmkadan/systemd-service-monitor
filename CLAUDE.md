# CLAUDE.md

ASP.NET Core (.NET 10) web app that monitors and controls systemd services over D-Bus (Tmds.DBus), exposing a REST API (Swagger) and a Blazor Server dashboard.

## Build

```bash
dotnet restore
dotnet build -c Release            # or: make build
dotnet run                         # dev server; Swagger at /swagger (Development only)
dotnet watch run                   # or: make watch
dotnet publish -c Release -o ./publish   # or: make publish
docker build -t systemd-service-monitor .   # or: make docker-build / docker-run
```

SDK pinned in `global.json` (10.0.100, rollForward latestMinor). Solution: `systemd-service-monitor.sln` (main project + tests).

## Test

```bash
dotnet test                                        # all tests (xUnit)
dotnet test --no-build -c Release                  # what CI runs (.github/workflows/build.yml)
dotnet test --filter "FullyQualifiedName~PathResolverTests"   # single class
dotnet run -c Release --project tests/systemd-service-monitor.Benchmarks   # BenchmarkDotNet
```

Test stack: xUnit, FluentAssertions, NSubstitute, Moq, Microsoft.AspNetCore.Mvc.Testing. `tests/**` and `examples/**` are excluded from the main csproj compile.

## Lint / Format

```bash
dotnet format                                              # make format
dotnet build -c Release /p:EnforceCodeStyleInBuild=true    # make lint
```

Style comes from `.editorconfig`: 4-space indent, LF, Allman braces, final newline. `Nullable` and `ImplicitUsings` enabled; `TreatWarningsAsErrors=false`. XML docs generated (`CS1591` suppressed).

## Layout

- `Program.cs` - entry point: Serilog setup, DI registration, middleware pipeline, `/health`, controllers + Razor components
- `Controllers/` - REST API (`ServicesController`, `LogsController`, `MetricsController`, `AlertsController`, `DependencyGraphController`, `SystemController`)
- `Services/` - business logic; interfaces `I*Service` next to implementations (`ServiceMonitorService`, `ServiceControlService`, `ServiceLogService`, `ResourceMonitorService`, `AlertRulesEngine`, `LogStreamService`)
- `Integration/` - D-Bus layer (`DBusConnectionManager`, `DBusInterfaces`)
- `Data/Repositories/` - in-memory/singleton repositories (`IServiceRepository`, `ILogRepository`, `IMetricRepository`)
- `BackgroundWorkers/` - `BackgroundService` implementations (`ServiceStatusUpdateWorker`)
- `Models/`, `Dtos/`, `Responses/`, `Enums/` - domain types, API DTOs, `ApiResponse` wrapper
- `Configuration/` - options classes bound from `appsettings.json` (`Systemd`, `Database`, alerts)
- `Extensions/` - DI registration (`AddApplicationServices`, `AddEventBus`, `AddBackgroundServices`, `AddLogStreaming`, `AddAlertRulesEngine`, `UseApplicationMiddleware`) and type extensions
- `Filters/` (`ApiExceptionFilter`, `ValidateModelFilter`), `Middleware/`, `Formatters/`, `Caching/`, `Utilities/` (`PathResolver`, `PaginationHelper`, `ValidationHelper`, `ServiceHealthChecker`)
- `Pages/`, `App.razor`, `Routes.razor`, `wwwroot/` - Blazor Server UI
- `tests/systemd-service-monitor.Tests/`, `tests/systemd-service-monitor.Benchmarks/`
- `docs/` - one Markdown file per class; `examples/` - client code, deployment samples, systemd unit
- `Makefile`, `Dockerfile`, `docker-compose*.yml` - build/run helpers

## Conventions

- Root namespace `SystemdServiceMonitor`; subnamespace matches folder (`SystemdServiceMonitor.Services`, `.Utilities`, ...)
- Files start with `#nullable enable`; one public type per file, file name = type name
- Interfaces prefixed `I`, placed alongside implementations
- Partial helpers split into sibling files by suffix: `*Extensions.cs`, `*Validation.cs`, `*JsonExtensions.cs`
- DI lifetimes: services scoped, repositories singleton, options via `IOptions<T>`/singleton
- Controllers return `ApiResponse<T>`; exceptions derive from `ServiceMonitorException` and are mapped by `ApiExceptionFilter`
- Logging via Serilog (`ILogger<T>`), structured message templates
- Tests: `<Class>Tests.cs`, methods `Method_ShouldExpectedBehavior`, Arrange/Act/Assert comments, `[Theory]`/`[InlineData]` for cases, FluentAssertions `.Should()`
- Public APIs carry XML doc comments (see CONTRIBUTING.md)
- Do not commit `bin/`, `obj/`, `publish/`, `logs/`, `.aider*`, `appsettings.Development.json`

# systemd Service Monitor

A comprehensive ASP.NET web dashboard for monitoring and managing systemd services via D-Bus. Built with .NET 10 and designed for production use on Linux systems.

## Features

- **Service Monitoring**: Real-time monitoring of systemd services with automatic status updates
- **Resource Tracking**: CPU, memory, disk, and network usage metrics per service
- **Log Management**: Access and search systemd journald logs with filtering and retention policies
- **Service Control**: Start, stop, restart, enable, and disable services with multiple restart strategies
- **Health Checks**: Configurable health checks with HTTP, TCP, and custom probes
- **REST API**: Complete REST API for integration with monitoring and orchestration tools
- **Web Dashboard**: Modern web interface for service management and visualization

## System Requirements

- Linux system with systemd
- .NET 10 SDK or Runtime
- D-Bus access for systemd communication
- Root or sudo privileges for service control operations

## Installation

```bash
git clone https://github.com/yourusername/systemd-service-monitor.git
cd systemd-service-monitor
dotnet restore
dotnet build -c Release
```

## Running the Application

Development:
```bash
dotnet run
```

Production:
```bash
dotnet publish -c Release -o ./publish
cd publish
./systemd-service-monitor
```

The API will be available at `https://localhost:5001` (or configured port).

## Configuration

Edit `appsettings.json` to configure:

- **Systemd**: Monitoring intervals, log retention, operation timeouts
- **Database**: Persistence provider, connection settings, caching
- **Logging**: Serilog configuration with file and console sinks

## API Documentation

Swagger/OpenAPI documentation available at `/swagger` when running in development mode.

### Key Endpoints

- `GET /api/services` - List all services
- `GET /api/services/{name}` - Get service details
- `POST /api/services/{name}/start` - Start a service
- `POST /api/services/{name}/stop` - Stop a service
- `POST /api/services/{name}/restart` - Restart a service
- `GET /api/services/{name}/logs` - Get service logs
- `GET /api/system/resources` - Get system resource metrics

## Architecture

### Domain Models
- **ServiceInfo**: Core service metadata and configuration
- **ServiceStatus**: Real-time status snapshots
- **ServiceLog**: Structured logging from journald
- **SystemResource**: System-wide metrics
- **ServiceMetric**: Time-series metrics collection

### Services
- **SystemdConnectionService**: D-Bus lifecycle management
- **ServiceMonitorService**: Service discovery and monitoring
- **ServiceLogService**: Log retrieval and management
- **ResourceMonitorService**: Metrics collection and alerting
- **ServiceControlService**: Service operations (start/stop/restart)

### Data Access
- **IServiceRepository**: Service persistence
- **ILogRepository**: Log storage and queries
- **IMetricRepository**: Metrics storage and time-series queries

## Development

### Building
```bash
dotnet build
```

### Running Tests
```bash
dotnet test
```

### Code Style
- Follow C# naming conventions (PascalCase for public members)
- Use nullable reference types
- Include XML documentation for public APIs
- Async/await patterns for all I/O operations

## License

MIT License - See LICENSE file for details

## Contributing

Contributions are welcome. Please ensure:
1. Code follows project conventions
2. New features include documentation
3. Tests pass before submitting
4. Commit messages are clear and descriptive

## Support

For issues and questions:
- GitHub Issues: [Report issues](https://github.com/yourusername/systemd-service-monitor/issues)
- Documentation: See docs/ directory
- Community: Discussions at GitHub

## Author

Vladyslav Zaiets - https://sarmkadan.com

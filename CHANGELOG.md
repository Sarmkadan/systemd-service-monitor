# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-05-04

### Added
- Comprehensive documentation suite (getting-started, architecture, api-reference, deployment, faq)
- Docker and Docker Compose support for containerized deployment
- Kubernetes manifests and deployment guide
- Example scripts for monitoring and integration (Nagios, cron-based)
- REST API client library (.NET example)
- Makefile with build automation targets
- EditorConfig for consistent code formatting
- CI/CD workflow for GitHub Actions
- Production-ready systemd unit file with security hardening
- Prometheus metrics export capability
- Advanced rate limiting configuration
- Performance monitoring and profiling examples
- Comprehensive API documentation with cURL and PowerShell examples

### Changed
- Expanded README.md from 3.8KB to 10KB+ with detailed sections
- Improved error messages and logging clarity
- Enhanced API response format with more metadata
- Better resource limit configuration in systemd unit

### Fixed
- D-Bus connection timeout handling
- Memory leak in metric collection worker
- Race condition in service state updates
- Log retrieval pagination boundary issue

### Security
- Added CORS policy configuration examples
- Implemented HTTPS certificate handling guide
- Added security hardening in systemd unit file
- Network isolation improvements for container deployments

---

## [1.1.0] - 2026-04-15

### Added
- Service dependency resolution before operations
- Bulk service operations API endpoints
- Historical metrics aggregation (1m, 5m, 15m, 1h buckets)
- Log export to JSON and CSV formats
- Health check endpoint with component status
- Rate limiting middleware with configurable thresholds
- Request correlation IDs for debugging
- Performance monitoring via dotnet-counters integration

### Changed
- Improved D-Bus connection retry logic with exponential backoff
- Optimized metric collection to reduce CPU overhead
- Enhanced service search with fuzzy matching support
- Better error responses with actionable error codes

### Fixed
- D-Bus socket reconnection on unexpected disconnects
- Concurrent service updates causing state inconsistency
- Memory usage in log retention with large datasets
- API response timeout on systems with 500+ services

### Performance
- Reduced API response time by 30% with caching improvements
- Optimized D-Bus query batching
- Improved startup time with parallel service discovery

---

## [1.0.0] - 2026-03-20

### Added
- Core service monitoring functionality
- Real-time service status polling via D-Bus
- Systemd journal integration for log retrieval
- Service control operations (start, stop, restart, enable, disable)
- Per-service CPU, memory, and resource metrics collection
- Web-based dashboard with Razor Pages
- Complete REST API for service management
- In-memory data storage with repository pattern
- Configurable metric collection intervals
- Log retention policies with automatic cleanup
- Health check probes (HTTP, TCP, custom)
- Background workers for periodic tasks
- Structured logging with Serilog
- Request logging and error handling middleware
- CORS support for cross-origin requests

### Documentation
- README with features and quick start
- Basic API endpoint documentation
- Architecture overview
- Contributing guidelines

### Examples
- Standalone console application
- Docker deployment example
- systemd unit file template

---

## [0.2.0] - 2026-02-10

### Added
- Initial feature set for Phase 2
- Metrics collection infrastructure
- Event publishing system
- Service cache implementation
- Background worker framework
- API filters and validation

### Fixed
- D-Bus connection stability issues
- Service state synchronization

---

## [0.1.0] - 2026-01-15

### Added
- Project foundation with .NET 10
- Basic D-Bus integration via Tmds.DBus
- Core service models and DTOs
- Initial repository layer
- ASP.NET Core API setup
- Swagger/OpenAPI documentation generation
- Basic logging infrastructure
- Initial project structure

### Initial Features
- Service discovery and listing
- Service status retrieval
- Journald log access
- Basic system information
- Health check endpoint

---

## Planned Features

### v2.0 (Q3 2026)
- [ ] Remote D-Bus monitoring for multi-host setups
- [ ] Authentication and authorization (OAuth/JWT)
- [ ] Distributed deployment support
- [ ] Prometheus metrics export (standalone exporter)
- [ ] Service dependency visualization
- [ ] Advanced alerting system with webhooks
- [ ] Performance optimizations for large-scale deployments
- [ ] Historical trend analysis and predictions
- [ ] Mobile app for service management
- [ ] Multi-language support

### v2.1 (Q4 2026)
- [ ] Service templates and automation
- [ ] Custom script execution framework
- [ ] Integration with other monitoring systems
- [ ] Advanced search and filtering
- [ ] Scheduled service operations
- [ ] Service grouping and tagging

### v3.0 (2027)
- [ ] Web UI redesign with modern frameworks
- [ ] Enhanced security features (mTLS, encryption)
- [ ] Enterprise features (audit logging, compliance)
- [ ] Advanced analytics and dashboards
- [ ] Performance optimization for enterprise scale

---

## Upgrade Guide

### From 1.1.0 to 1.2.0

1. Backup your configuration: `cp appsettings.json appsettings.backup.json`
2. Stop the service: `systemctl stop systemd-service-monitor`
3. Download and extract new version
4. Review changes: Check CHANGELOG.md for breaking changes (none in this release)
5. Start the service: `systemctl start systemd-service-monitor`
6. Verify: `curl https://localhost:5001/health`

No database migrations needed for this version.

### From 1.0.0 to 1.1.0

1. Backup configuration and logs
2. Stop service
3. Extract new version
4. Configuration is backward compatible
5. Start service and verify functionality

### Rollback Procedure

If issues occur after upgrade:

```bash
# Stop the service
sudo systemctl stop systemd-service-monitor

# Restore previous version
cd /opt
sudo mv systemd-service-monitor systemd-service-monitor.new
sudo tar -xzf systemd-service-monitor-backup.tar.gz

# Start service
sudo systemctl start systemd-service-monitor
```

---

## Dependencies

### Runtime
- .NET 10.0 Runtime
- systemd (Linux)
- D-Bus service

### Development
- .NET 10.0 SDK
- Docker (for containerized builds)
- Make (for build automation)

### NuGet Packages
- Tmds.DBus 0.14.0+ (D-Bus communication)
- Serilog 4.2.0+ (Logging)
- Serilog.AspNetCore 8.0.1+ (ASP.NET integration)
- Swashbuckle.AspNetCore 6.4.0+ (OpenAPI/Swagger)

---

## Migration Notes

### v1.1.0 Changes
- **New API Endpoints**: `/api/services/{name}/logs`, `/api/metrics/services/{name}`
- **New Configuration**: Rate limiting options in `appsettings.json`
- **New Headers**: X-RateLimit-* headers in API responses

### v1.0.0 Release
- Initial stable release
- All APIs are at `/api/v1/` prefix
- No migration needed from beta versions

---

## Known Issues and Workarounds

### Service not appearing in list
- Verify service name includes `.service` suffix
- Check service isn't masked: `systemctl is-enabled service.service`
- Check service unit file exists in `/etc/systemd/system/`

**Workaround**: Create symlink or reload systemd: `systemctl daemon-reload`

### High memory usage with many services
- Memory grows with number of monitored services
- Configure shorter `LogRetentionDays` in settings
- Use external database instead of in-memory storage

**Workaround**: Restart application periodically: `systemctl restart systemd-service-monitor`

### API timeouts with large result sets
- Configure pagination: reduce `pageSize` parameter
- Increase `OperationTimeoutMs` in settings
- Check D-Bus responsiveness

**Workaround**: Query smaller subsets or add database backend

---

## Contributors

- **Vladyslav Zaiets** - Initial development and architecture

See LICENSE for copyright information.

---

## Support

- **Documentation**: See `docs/` directory
- **Issues**: Report on GitHub
- **Email**: Visit https://sarmkadan.com for contact information

---

## License

This project is licensed under the MIT License - see LICENSE file for details.

---

**Last Updated**: 2026-05-04

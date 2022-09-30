# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2025-05-20

### Added
- Alert rules engine with escalation policies and on-call rotation
- Docker support with multi-stage builds
- Health check endpoints (/health, /health/ready)
- Integration test suite with xUnit
- Migration guide from v1.x

### Changed
- Upgraded to .NET 10.0
- Modern C# features (records, primary constructors)
- Improved API consistency

### Fixed
- Various edge cases found through testing

---

## [1.0.0] - 2025-03-20

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

### Security
- Added CORS policy configuration examples
- Implemented HTTPS certificate handling guide
- Added security hardening in systemd unit file
- Network isolation improvements for container deployments

---

## [0.2.0] - 2025-02-10

### Added
- Metrics collection infrastructure
- Event publishing system
- Service cache implementation
- Background worker framework
- API filters and validation
- Service dependency resolution before operations
- Bulk service operations API endpoints
- Historical metrics aggregation (1m, 5m, 15m, 1h buckets)
- Log export to JSON and CSV formats
- Health check endpoint with component status

### Changed
- Improved error messages and logging clarity
- Enhanced API response format with more metadata

### Fixed
- D-Bus connection stability issues
- Service state synchronization
- Memory leak in metric collection worker
- Race condition in service state updates

### Performance
- Reduced API response time by 30% with caching improvements
- Optimized D-Bus query batching
- Improved startup time with parallel service discovery

---

## [0.1.0] - 2025-01-15

### Added
- Project foundation with .NET 10
- Basic D-Bus integration via Tmds.DBus
- Core service models and DTOs
- Initial repository layer
- ASP.NET Core API setup
- Swagger/OpenAPI documentation generation
- Basic logging infrastructure
- Initial project structure
- Core service monitoring functionality
- Real-time service status polling via D-Bus
- Systemd journal integration for log retrieval
- Service control operations (start, stop, restart, enable, disable)
- Per-service CPU, memory, and resource metrics collection
- Complete REST API for service management
- In-memory data storage with repository pattern
- Configurable metric collection intervals
- Log retention policies with automatic cleanup
- Health check probes (HTTP, TCP, custom)
- Structured logging with Serilog
- Request logging and error handling middleware
- CORS support for cross-origin requests

### Initial Features
- Service discovery and listing
- Service status retrieval
- Journald log access
- Basic system information
- Health check endpoint

---

## Planned Features

### v1.1.0 (Q2 2025)
- [ ] Authentication and authorization (OAuth/JWT)
- [ ] Prometheus metrics export (standalone exporter)
- [ ] Service dependency visualization
- [ ] Advanced alerting system with webhooks

### v2.0 (Q3 2025)
- [ ] Remote D-Bus monitoring for multi-host setups
- [ ] Distributed deployment support
- [ ] Performance optimizations for large-scale deployments
- [ ] Historical trend analysis and predictions
- [ ] Mobile app for service management
- [ ] Multi-language support

### v2.1 (Q4 2025)
- [ ] Service templates and automation
- [ ] Custom script execution framework
- [ ] Integration with other monitoring systems
- [ ] Advanced search and filtering
- [ ] Scheduled service operations

### v3.0 (2026)
- [ ] Web UI redesign with modern frameworks
- [ ] Enhanced security features (mTLS, encryption)
- [ ] Enterprise features (audit logging, compliance)
- [ ] Advanced analytics and dashboards

---

## Upgrade Guide

### From 0.1.0 to 0.2.0

1. Backup your configuration: `cp appsettings.json appsettings.backup.json`
2. Stop the service: `systemctl stop systemd-service-monitor`
3. Download and extract new version
4. Review changes in CHANGELOG.md (no breaking changes in this release)
5. Start the service: `systemctl start systemd-service-monitor`
6. Verify: `curl http://localhost:5001/health`

### From 0.2.0 to 1.0.0

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

### v1.0.0 Release
- Initial stable release
- All APIs available under `/api/` prefix
- No migration needed from 0.x versions

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
- **Contact**: https://sarmkadan.com

---

## License

This project is licensed under the MIT License - see LICENSE file for details.

---

**Last Updated**: 2025-03-20

# Architecture Guide

## System Architecture Overview

systemd-service-monitor is built on a layered architecture designed for scalability, maintainability, and testability.

### Architectural Layers

```
┌─────────────────────────────────────────────────────┐
│                   Presentation Layer                 │
│      (Web Dashboard, REST API Controllers)           │
└────────────────────┬────────────────────────────────┘
                     │
                API Requests/Responses
                     │
┌────────────────────▼────────────────────────────────┐
│                 Business Logic Layer                 │
│              (Services, Coordinators)                │
├────────────────────────────────────────────────────┤
│  - ServiceMonitorService                            │
│  - ServiceControlService                            │
│  - ServiceLogService                                │
│  - ResourceMonitorService                           │
│  - SystemdConnectionService                         │
└────────────────────┬────────────────────────────────┘
                     │
                Data Contracts
                     │
┌────────────────────▼────────────────────────────────┐
│                  Data Access Layer                   │
│            (Repositories, Interfaces)                │
├────────────────────────────────────────────────────┤
│  - IServiceRepository                               │
│  - ILogRepository                                   │
│  - IMetricRepository                                │
└────────────────────┬────────────────────────────────┘
                     │
         In-Memory/Database Queries
                     │
┌────────────────────▼────────────────────────────────┐
│               Infrastructure Layer                   │
│      (D-Bus Integration, System Access)              │
├────────────────────────────────────────────────────┤
│  - DBusConnectionManager                            │
│  - systemd Service Proxy (via D-Bus)                │
│  - Journal Access                                   │
└────────────────────────────────────────────────────┘
```

## Detailed Component Architecture

### 1. Controllers Layer (`Controllers/`)

Handles HTTP request routing and response formatting.

**ServicesController**
- Lists all systemd services with filtering and pagination
- Retrieves detailed service information
- Initiates service control operations (start/stop/restart)
- Returns standardized JSON responses

**LogsController**
- Queries systemd journal for service logs
- Provides filtering by priority, time range, and keywords
- Supports pagination for large log sets
- Exports logs to multiple formats

**MetricsController**
- Retrieves historical performance metrics
- Supports time-range queries and aggregation levels
- Returns CPU, memory, disk, and network statistics

**SystemController**
- Exposes system-wide resource information
- Health check endpoint for monitoring
- System status overview

### 2. Services Layer (`Services/`)

Contains core business logic and orchestration.

#### ISystemdConnectionService / SystemdConnectionService
```
Responsibility: Manage D-Bus connection lifecycle
Methods:
  - InitializeConnection(): Establish D-Bus connection
  - GetManager(): Get systemd Manager interface
  - IsConnected(): Check connection status
  - Reconnect(): Handle reconnection logic
  
Error Handling:
  - Retry logic with exponential backoff
  - Automatic reconnection on failure
  - Detailed error logging
```

#### IServiceMonitorService / ServiceMonitorService
```
Responsibility: Monitor and discover systemd services
Methods:
  - GetAllServicesAsync(): List all services
  - GetServiceDetailsAsync(name): Get service info
  - GetServiceStatusAsync(name): Real-time status
  - SearchServicesAsync(query): Full-text search
  
Features:
  - Service dependency tracking
  - State change notifications
  - Automatic cache invalidation
```

#### IServiceControlService / ServiceControlService
```
Responsibility: Execute service control operations
Methods:
  - StartServiceAsync(name): Start service
  - StopServiceAsync(name): Stop service
  - RestartServiceAsync(name, mode): Restart with strategy
  - EnableServiceAsync(name): Enable at boot
  - DisableServiceAsync(name): Disable at boot
  
Validation:
  - Check dependencies before operations
  - Validate service existence
  - Enforce operation timeouts
```

#### IServiceLogService / ServiceLogService
```
Responsibility: Access and manage systemd journal logs
Methods:
  - GetLogsAsync(name, count, filter): Retrieve logs
  - SearchLogsAsync(query, options): Full-text search
  - CleanupOldLogsAsync(): Retention policy enforcement
  - GetLogCountAsync(name): Log statistics
  
Processing:
  - Parse structured journal entries
  - Filter by priority and time range
  - Apply retention policies
```

#### IResourceMonitorService / ResourceMonitorService
```
Responsibility: Collect and aggregate performance metrics
Methods:
  - GetSystemResourcesAsync(): Overall system metrics
  - GetServiceMetricsAsync(name, timeRange): Service metrics
  - CollectMetricsAsync(): Periodic collection job
  
Metrics Tracked:
  - CPU percentage per service
  - Memory usage (RSS, VSZ)
  - Disk I/O (read/write bytes per second)
  - Network traffic (in/out bytes per second)
```

### 3. Data Access Layer (`Data/Repositories/`)

Abstracts persistence mechanism.

#### Repository Pattern

```csharp
// Generic repository interface
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate);
    Task<int> AddAsync(T entity);
    Task<int> UpdateAsync(T entity);
    Task<int> DeleteAsync(string id);
}
```

#### Specific Repositories

**IServiceRepository**
- Caches service metadata and configurations
- Tracks service state history
- Supports complex queries (state, dependency, tags)

**ILogRepository**
- Stores parsed journal entries
- Implements log retention policies
- Provides full-text search indexing

**IMetricRepository**
- Time-series storage for metrics
- Aggregation by time buckets (1m, 5m, 15m, 1h)
- Automatic cleanup of old metrics

### 4. Models and DTOs (`Models/`, `Dtos/`)

#### Domain Models
```csharp
// Core domain entities
ServiceInfo        - Service metadata and configuration
ServiceStatus      - Current service state snapshot
ServiceLog         - Individual log entry
SystemResource     - System-wide metrics
ServiceMetric      - Time-series metric point
RestartPolicyConfig - Restart strategy configuration
```

#### Data Transfer Objects
```csharp
// API contract objects
ServiceDetailsDto  - Full service information for API
ServiceStatusDto   - Service status response
```

### 5. Integration Layer (`Integration/`)

Handles D-Bus and systemd communication.

#### DBusConnectionManager
```
Responsibilities:
  - Establish D-Bus system bus connection
  - Manage service proxies
  - Handle D-Bus method calls
  - Convert D-Bus types to .NET types
  
Key Methods:
  - ConnectAsync(): Initialize connection
  - GetServiceAsync(name): Get service unit proxy
  - CallMethodAsync(): Execute D-Bus method
  - SubscribeToSignals(): Listen for state changes
```

#### systemd Service Interface Mapping
```
D-Bus Path:     /org/freedesktop/systemd1/unit/
D-Bus Interface: org.freedesktop.systemd1.Unit
D-Bus Interface: org.freedesktop.systemd1.Service

Mapped Properties:
  UnitFileState     → unitFileState
  ActiveState       → activeState
  SubState          → subState
  LoadState         → loadState
  Description       → description
  Names             → serviceName

Mapped Methods:
  Start()           → StartServiceAsync()
  Stop()            → StopServiceAsync()
  Restart()         → RestartServiceAsync()
  ReloadOrRestart() → ReloadServiceAsync()
```

### 6. Middleware Pipeline (`Middleware/`)

Request processing chain.

```
Request
  ↓
RequestLoggingMiddleware      - Log incoming requests
  ↓
ErrorHandlingMiddleware       - Catch and format exceptions
  ↓
RateLimitingMiddleware        - Enforce rate limits
  ↓
RequestValidationMiddleware   - Validate request format
  ↓
Controller Action
  ↓
Response Processing           - Format response envelopes
  ↓
Response
```

#### Key Middleware Components

**ErrorHandlingMiddleware**
- Catches unhandled exceptions
- Converts to standardized error responses
- Logs error details for debugging
- Returns appropriate HTTP status codes

**RateLimitingMiddleware**
- Enforces per-IP rate limits
- Configurable limits per endpoint
- Returns 429 (Too Many Requests) when exceeded

**RequestLoggingMiddleware**
- Logs HTTP method, path, status code
- Includes request/response timing
- Traces request correlation IDs
- Filters sensitive headers

### 7. Background Workers (`BackgroundWorkers/`)

Long-running background tasks.

#### ServiceStatusUpdateWorker
```
Interval: MetricCollectionIntervalMs
Task:
  1. Query all services from systemd
  2. Update status in repository
  3. Detect state changes
  4. Publish state-change events
  5. Update cache
```

#### MetricsCollectionWorker (Extensible)
```
Interval: MetricCollectionIntervalMs
Task:
  1. Query metrics for all services
  2. Parse /proc/[pid]/stat for CPU/memory
  3. Query /proc/net/dev for network stats
  4. Store metrics in repository
  5. Aggregate historical data
```

#### LogCleanupWorker
```
Interval: 1 hour
Task:
  1. Query logs older than retention period
  2. Delete expired entries
  3. Compact log storage
  4. Report cleanup statistics
```

### 8. Filters and Attributes (`Filters/`)

ASP.NET Core request/response filters.

**ApiExceptionFilter**
- Catches controller exceptions
- Converts to ApiResponse envelopes
- Preserves exception information for diagnostics

**ValidateModelFilter**
- Validates model binding
- Returns detailed validation errors
- Ensures data consistency

## Design Patterns Used

### 1. Dependency Injection
- Constructor injection for loose coupling
- Interface-based dependencies
- Service registration in Startup

### 2. Repository Pattern
- Abstract data access logic
- Support multiple persistence providers
- Enable easier testing

### 3. Service Layer Pattern
- Encapsulate business logic
- Coordinate between repositories
- Handle cross-cutting concerns

### 4. Observer Pattern
- Event publishers for state changes
- Subscribers for notifications
- Decoupled event handling

### 5. Factory Pattern
- ServiceFactory for instance creation
- Conditional service instantiation
- Extensible service registration

## Data Flow Examples

### Example 1: Retrieve Service Status

```
Client Request: GET /api/services/nginx.service
         ↓
ServicesController.GetService(name)
         ↓
ServiceMonitorService.GetServiceStatusAsync(name)
         ↓
IServiceRepository.GetByIdAsync(name) [Check cache]
         ↓
Cache hit? → Return cached ServiceStatus
         ↓
Cache miss? → Query D-Bus via DBusConnectionManager
         ↓
GetServiceAsync(name) → Parse D-Bus properties
         ↓
ServiceMonitorService.CreateStatusSnapshot(properties)
         ↓
IServiceRepository.UpdateAsync(status) [Update cache]
         ↓
Return ServiceStatus → API Response → Client
```

### Example 2: Restart Service

```
Client Request: POST /api/services/nginx.service/restart
         ↓
ServicesController.Restart(name)
         ↓
ServiceControlService.RestartServiceAsync(name, mode)
         ↓
1. Check service exists
2. Validate service state
3. Check dependencies
         ↓
DBusConnectionManager.GetServiceAsync(name)
         ↓
Call D-Bus Restart() method
         ↓
Wait for completion (30s timeout)
         ↓
Poll service state until running
         ↓
ServiceMonitorService.GetServiceStatusAsync(name) [Refresh]
         ↓
Return new ServiceStatus → API Response → Client
```

### Example 3: Collect Metrics

```
Timer.Elapsed (every 5 seconds)
         ↓
ServiceStatusUpdateWorker.Execute()
         ↓
ServiceMonitorService.GetAllServicesAsync()
         ↓
For each service:
  - ServiceMonitorService.GetServiceStatusAsync(name)
  - ResourceMonitorService.CollectMetricsAsync(name)
         ↓
Parse /proc/[pid]/stat for CPU/memory
         ↓
Parse /proc/net/dev for network stats
         ↓
CreateMetricPoint() with timestamp
         ↓
IMetricRepository.AddAsync(metric)
         ↓
Aggregate metrics by time bucket (5m interval)
         ↓
PublishMetricCollectedEvent()
         ↓
Metrics available via GET /api/metrics/...
```

## Scalability Considerations

### Current Limitations
- In-memory repository (no persistence)
- Single-process architecture
- Metric data lost on restart
- No built-in clustering

### Scaling Paths

**Vertical Scaling**
- Increase MetricCollectionIntervalMs
- Reduce MaxLogEntriesPerRequest
- Implement memory caching for expensive queries

**Horizontal Scaling**
1. Implement database repository
2. Deploy behind load balancer
3. Share database backend
4. Implement distributed caching (Redis)

**Performance Optimization**
1. Add response caching headers
2. Implement pagination for large result sets
3. Optimize D-Bus queries
4. Profile with dotnet-trace

## Security Architecture

### Defense in Depth

1. **Network Layer**
   - HTTPS only in production
   - Certificate pinning option
   - Rate limiting against DDoS

2. **Application Layer**
   - Input validation on all endpoints
   - Output encoding to prevent XSS
   - CORS policy configuration

3. **Authorization**
   - D-Bus privilege checks
   - systemd policy integration
   - Operation audit logging

4. **Data Protection**
   - TLS in transit
   - No sensitive data in logs
   - Secure credential handling

## Extension Points

### Custom Health Checks
```csharp
// Implement IHealthCheck interface
// Register in DI container
// Called by /health endpoint
```

### Custom Formatters
```csharp
// Implement IOutputFormatter
// Handle custom output types
// Register in controllers
```

### Custom Repositories
```csharp
// Implement IServiceRepository
// Add persistence provider
// Register in DI startup
```

### Event Subscribers
```csharp
// Subscribe to ServiceEventPublisher
// React to service state changes
// Implement custom notifications
```

## Testing Strategy

### Unit Tests
- Service logic in isolation
- Mock repositories and D-Bus
- Test error conditions
- Validate business rules

### Integration Tests
- Real D-Bus connection (if available)
- Repository behavior
- Service coordination
- End-to-end workflows

### API Tests
- Controller endpoints
- Request/response validation
- Error scenarios
- Performance baselines

## Performance Characteristics

| Operation | Typical Duration |
|-----------|------------------|
| List all services | 50-200ms |
| Get service details | 30-100ms |
| Start/stop service | 500-2000ms |
| Get 100 log lines | 100-300ms |
| Query metrics (1h range) | 50-150ms |
| Collect all metrics | 100-500ms |

## Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | ASP.NET Core | 10.0 |
| Language | C# | Latest |
| D-Bus | Tmds.DBus | 0.14+ |
| Logging | Serilog | 4.2+ |
| API Docs | Swagger/OpenAPI | 6.4+ |
| Testing | xUnit | Latest |

## References

- [systemd D-Bus Interface](https://dbus.freedesktop.org/doc/dbus-daemon.1.html)
- [ASP.NET Core Architecture](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/architecture)
- [Microservices Patterns](https://microservices.io/patterns/)
- [C# Async/Await Best Practices](https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)

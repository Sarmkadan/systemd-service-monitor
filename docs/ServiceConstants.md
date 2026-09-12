# ServiceConstants

`ServiceConstants` centralizes fixed identifiers, limits, naming conventions, and default timings used by the systemd service monitor. The class is static, and all values are compile-time constants.

## D-Bus identifiers

| Constant | Value | Purpose |
| --- | --- | --- |
| `DefaultServiceBusPath` | `org.freedesktop.systemd1` | The well-known D-Bus destination name for the systemd manager service. |
| `DefaultServiceBusObjectPath` | `/org/freedesktop/systemd1` | The default D-Bus object path of the systemd manager. |
| `ServiceManagerInterface` | `org.freedesktop.systemd1.Manager` | The D-Bus interface used for systemd manager operations, such as locating and controlling units. |
| `UnitInterface` | `org.freedesktop.systemd1.Unit` | The common D-Bus interface exposed by every systemd unit. |
| `ServiceInterface` | `org.freedesktop.systemd1.Service` | The service-specific D-Bus interface used to access properties and operations unique to service units. |

## Monitoring and log defaults

| Constant | Value | Purpose |
| --- | ---: | --- |
| `DefaultMetricCollectionIntervalMs` | `5000` | The default delay, in milliseconds, between metric collection cycles. |
| `MaxLogEntriesPerRequest` | `1000` | The default upper limit on the number of log entries returned or processed by one request. |
| `DefaultLogRetentionDays` | `30` | The default number of days for which collected log entries are retained. |

These values express application defaults. A configurable option may override a default where the consuming component supports configuration.

## Systemd unit suffixes

| Constant | Value | Purpose |
| --- | --- | --- |
| `SystemdServiceNameSuffix` | `.service` | Identifies or constructs systemd service unit names. |
| `SystemdTimerNameSuffix` | `.timer` | Identifies or constructs systemd timer unit names. |
| `SystemdSocketNameSuffix` | `.socket` | Identifies or constructs systemd socket unit names. |

## `DBusSignals`

The nested `DBusSignals` class contains D-Bus signal member names used when observing changes to systemd units.

| Constant | Value | Purpose |
| --- | --- | --- |
| `PropertiesChanged` | `PropertiesChanged` | Indicates that one or more properties on a D-Bus object changed. |
| `UnitNew` | `UnitNew` | Indicates that systemd loaded or otherwise introduced a unit. |
| `UnitRemoved` | `UnitRemoved` | Indicates that systemd removed or unloaded a unit. |

## `HealthCheckPatterns`

The nested `HealthCheckPatterns` class defines supported HTTP method tokens and default health-check timing values.

| Constant | Value | Purpose |
| --- | ---: | --- |
| `HttpGetPattern` | `GET` | The HTTP GET method token for a read-only health-check request. |
| `HttpPostPattern` | `POST` | The HTTP POST method token for a health-check request that requires POST semantics. |
| `DefaultTimeoutSeconds` | `10` | The default maximum time, in seconds, allowed for a health check to complete. |
| `DefaultIntervalSeconds` | `30` | The default interval, in seconds, between health-check executions. |

## Usage

Reference constants through the containing class and, for grouped values, the nested class:

```csharp
using SystemdServiceMonitor.Constants;

var destination = ServiceConstants.DefaultServiceBusPath;
var unitSuffix = ServiceConstants.SystemdServiceNameSuffix;
var signalName = ServiceConstants.DBusSignals.PropertiesChanged;
var timeoutSeconds = ServiceConstants.HealthCheckPatterns.DefaultTimeoutSeconds;
```

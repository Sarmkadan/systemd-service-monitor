# ServiceState

`ServiceState` represents the high-level lifecycle state of a systemd service unit. It is declared in `Enums/ServiceState.cs` in the `SystemdServiceMonitor.Enums` namespace.

The monitor creates this value from the unit's systemd `ActiveState`. Parsing is case-insensitive, so values such as `active` and `failed` map to the corresponding C# enum members. A value that cannot be parsed maps to `ServiceState.Unknown`.

## Values

| Value | systemd lifecycle meaning |
| --- | --- |
| `Active` | The unit has completed activation successfully and is currently active. A service may be running a process or may have exited successfully while remaining active, depending on its type and configuration. |
| `Inactive` | The unit is not active. It may never have been started, may have stopped successfully, or may have completed without being configured to remain active. |
| `Activating` | The unit is transitioning toward `Active`. Its start job is still in progress, so it should not yet be treated as fully started. |
| `Deactivating` | The unit is transitioning toward `Inactive`. Its stop job is still in progress and resources may still be shutting down. |
| `Failed` | Activation, execution, or shutdown failed and systemd recorded the unit in the failed state. The unit remains failed until it is restarted successfully or its failed state is reset. |
| `Reloading` | The unit is active while its configuration is being reloaded. This is a transient state and normally returns to `Active` when the reload finishes. |
| `Unknown` | The monitor could not map the reported value to a known enum member, or no usable state was available. This is a monitor fallback rather than a normal systemd lifecycle state. |

## Typical lifecycle

The common successful transitions are:

```text
Inactive -> Activating -> Active -> Deactivating -> Inactive
                              |
                              +-> Reloading -> Active
```

A failure during startup, operation, reload, or shutdown can lead to `Failed`. A later successful start can move the service through `Activating` to `Active` again.

Systemd's more detailed `SubState` qualifies these high-level states. For example, `Active` can be paired with a sub-state such as `Running` or `Exited`. Consumers should inspect both `ServiceState` and `ServiceSubState` when they need operational detail.

## Use in this project

- `ServiceMonitorService` parses the systemd `ActiveState` into this enum.
- Status and dependency-graph summaries count `Active`, `Inactive`, `Failed`, and `Unknown` explicitly.
- A service status is marked as running when its state is `Active`.
- `EnumExtensions.IsRunning` returns `true` for `Active` and `Reloading` because a reload occurs while the unit remains active.
- `EnumExtensions.IsTerminal` returns `true` for `Inactive` and `Failed`; transition states are not terminal.
- Validation reports `Unknown` as an uninitialized or unrecognized state.

Do not infer the service's detailed process condition from this enum alone. Use `ServiceSubState`, process information, and the service result where the distinction matters.

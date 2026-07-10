# IServiceLogService

Provides access to aggregated logging metrics and timestamps for a specific systemd service unit, enabling monitoring of log volume and severity distribution over time.

## API

### `string UnitName`
Gets the name of the systemd unit this log service represents.
- **Type:** `string`
- **Access:** Read-only
- **Throws:** Never; returns `null` if the unit name is not available.

### `long TotalLogEntries`
Gets the total number of log entries collected for the unit.
- **Type:** `long`
- **Access:** Read-only
- **Throws:** Never; returns `0` if no logs are available.

### `long ErrorCount`
Gets the number of log entries with severity level `error`.
- **Type:** `long`
- **Access:** Read-only
- **Throws:** Never; returns `0` if no error logs exist.

### `long WarningCount`
Gets the number of log entries with severity level `warning`.
- **Type:** `long`
- **Access:** Read-only
- **Throws:** Never; returns `0` if no warning logs exist.

### `long InfoCount`
Gets the number of log entries with severity level `info`.
- **Type:** `long`
- **Access:** Read-only
- **Throws:** Never; returns `0` if no info logs exist.

### `DateTime OldestLogTime`
Gets the timestamp of the oldest log entry collected for the unit.
- **Type:** `DateTime`
- **Access:** Read-only
- **Throws:** Never; returns `DateTime.MinValue` if no logs are available.

### `DateTime LatestLogTime`
Gets the timestamp of the most recent log entry collected for the unit.
- **Type:** `DateTime`
- **Access:** Read-only
- **Throws:** Never; returns `DateTime.MinValue` if no logs are available.

## Usage

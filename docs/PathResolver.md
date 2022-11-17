# PathResolver

Utility class for resolving and normalizing systemd service unit paths across system and user contexts. Provides methods to locate service files, validate paths, normalize service names, and determine service scope.

## API

### `public static IEnumerable<string> GetSystemUnitPaths()`

Enumerates the system-wide directories where systemd looks for unit files. The directories are returned in the order systemd uses for lookup (typically `/usr/lib/systemd/system`, `/lib/systemd/system`, `/run/systemd/system`, `/etc/systemd/system`).

- **Returns**: An enumerable of absolute paths to system unit directories.
- **Throws**: `UnauthorizedAccessException` if the caller lacks permission to read directory contents.

---

### `public static string GetDefaultSystemUnitDirectory()`

Returns the primary system unit directory where most system services are installed (typically `/etc/systemd/system`).

- **Returns**: Absolute path to the default system unit directory.
- **Throws**: `UnauthorizedAccessException` if the directory cannot be accessed.

---

### `public static string GetDefaultUserUnitDirectory()`

Returns the user-specific directory where user services are installed (typically `~/.config/systemd/user`).

- **Returns**: Absolute path to the default user unit directory.
- **Throws**: `UnauthorizedAccessException` if the directory cannot be accessed.

---
### `public static string NormalizeServiceName(string serviceName)`

Normalizes a service name by removing leading/trailing whitespace, converting to lowercase, and ensuring it ends with `.service` if not already present.

- **Parameters**:
  - `serviceName`: The raw service name (e.g., `"nginx"`, `"  SSHD  "`).
- **Returns**: Normalized service name (e.g., `"nginx.service"`, `"sshd.service"`).
- **Throws**: `ArgumentException` if `serviceName` is `null` or empty after trimming.

---
### `public static string RemoveServiceExtension(string serviceName)`

Removes the `.service` extension from a normalized service name if present.

- **Parameters**:
  - `serviceName`: A normalized service name (e.g., `"nginx.service"`).
- **Returns**: Service name without extension (e.g., `"nginx"`).
- **Throws**: `ArgumentException` if `serviceName` is `null` or does not end with `.service`.

---
### `public static string? FindServiceUnitFile(string serviceName)`

Locates the unit file for a given service name across system and user directories.

- **Parameters**:
  - `serviceName`: Normalized service name (e.g., `"nginx.service"`).
- **Returns**: Absolute path to the unit file if found; otherwise `null`.
- **Throws**: `ArgumentException` if `serviceName` is invalid or `null`.

---
### `public static bool IsValidServicePath(string path)`

Determines whether a given path points to a valid systemd service unit file.

- **Parameters**:
  - `path`: Absolute or relative path to check.
- **Returns**: `true` if the path exists and ends with `.service`; otherwise `false`.
- **Throws**: `ArgumentException` if `path` is `null` or empty.

---
### `public static string? GetServiceDirectory(string unitPath)`

Extracts the directory containing the service unit file from its full path.

- **Parameters**:
  - `unitPath`: Absolute path to a `.service` file (e.g., `/etc/systemd/system/nginx.service`).
- **Returns**: Directory path if `unitPath` is valid; otherwise `null`.
- **Throws**: `ArgumentException` if `unitPath` is `null` or not a `.service` file.

---
### `public static ServiceScope GetServiceScope(string serviceName)`

Determines whether a service is system-wide or user-scoped based on its installation path.

- **Parameters**:
  - `serviceName`: Normalized service name (e.g., `"nginx.service"`).
- **Returns**: `ServiceScope.System` if the service is in a system directory; `ServiceScope.User` if in a user directory.
- **Throws**: `ArgumentException` if `serviceName` is invalid or `null`.

---
### `public static List<string> GetRelatedServices(string serviceName)`

Enumerates all services related to a given service by following symlinks and dependencies (e.g., `Wants=`, `Requires=`).

- **Parameters**:
  - `serviceName`: Normalized service name (e.g., `"docker.service"`).
- **Returns**: List of related service names (including `serviceName` itself). Returns an empty list if no related services are found.
- **Throws**: `ArgumentException` if `serviceName` is invalid or `null`.

## Usage

### Example 1: Locate and validate a service file

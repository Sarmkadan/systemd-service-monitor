# D-Bus Interface Contracts

This document outlines the D-Bus interface contracts declared in `Integration/DBusInterfaces.cs`. These interfaces define the communication protocol between the service monitor and the systemd ecosystem via D-Bus.

## Overview

The library utilizes `Tmds.DBus` to interact with three main D-Bus services:
- `org.freedesktop.systemd1.Manager`: For managing systemd units.
- `org.freedesktop.DBus.Properties`: For retrieving object properties.
- `org.freedesktop.Journal1.Journal`: For reading systemd journal logs.

---

## `ISystemdManager`
**D-Bus Interface:** `org.freedesktop.systemd1.Manager`

Used to list, get, and control systemd units.

### Methods

| Method | Description | Parameters | Return Type |
|--------|-------------|------------|-------------|
| `ListUnitsAsync` | Retrieves a list of all loaded units. | None | `Task<(string Name, string Description, string LoadState, string ActiveState, string SubState, string Followed, string Path, ObjectPath JobPath)[]>` |
| `GetUnitAsync` | Retrieves the object path for a specific unit. | `name: string` | `Task<ObjectPath>` |
| `StartUnitAsync` | Starts a unit. | `name: string`, `mode: string` | `Task<ObjectPath>` |
| `StopUnitAsync` | Stops a unit. | `name: string`, `mode: string` | `Task<ObjectPath>` |
| `RestartUnitAsync` | Restarts a unit. | `name: string`, `mode: string` | `Task<ObjectPath>` |
| `ReloadUnitAsync` | Reloads a unit configuration. | `name: string`, `mode: string` | `Task<ObjectPath>` |
| `KillUnitAsync` | Sends a signal to all processes in a unit. | `name: string`, `signal: string` | `Task` |
| `EnableUnitFilesAsync` | Enables unit files. | `names: string[]`, `runtime: bool`, `force: bool` | `Task<(bool Success, string[] Failures)>` |
| `DisableUnitFilesAsync` | Disables unit files. | `names: string[]`, `runtime: bool` | `Task<(bool Success, string[] Failures)>` |

---

## `IProperties`
**D-Bus Interface:** `org.freedesktop.DBus.Properties`

Used to get all properties of a D-Bus object.

### Methods

| Method | Description | Parameters | Return Type |
|--------|-------------|------------|-------------|
| `GetAllAsync` | Retrieves all properties for a given D-Bus interface. | `@interface: string` | `Task<IDictionary<string, object>>` |

---

## `IJournal`
**D-Bus Interface:** `org.freedesktop.Journal1.Journal`

Used to retrieve log entries from `systemd-journald`.

### Methods

| Method | Description | Parameters | Return Type |
|--------|-------------|------------|-------------|
| `AddMatchAsync` | Adds a match rule to filter journal entries. | `match: string` | `Task` |
| `SeekTailAsync` | Seeks to the end of the journal. | None | `Task` |
| `NextAsync` | Advances to the next journal entry. | None | `Task<ulong>` |
| `GetDataAsync` | Retrieves the data fields of the current journal entry. | None | `Task<IDictionary<string, string>>` |
| `FlushMatchesAsync` | Flushes all added match rules. | None | `Task` |

---

## `ISystemdUnit`
**D-Bus Interface:** `org.freedesktop.systemd1.Unit`

A generic representation for fetching common unit properties. Note that specific unit interfaces (like `org.freedesktop.systemd1.Service`) typically inherit from this or provide more specific properties.

### Notes
The following properties are commented out in the interface definition but are conceptually available via `IProperties.GetAllAsync`:
- `ActiveState` (`string`)
- `SubState` (`string`)
- `MainPID` (`uint`)
- `CPUUsageNsec` (`ulong`)
- `MemoryCurrent` (`ulong`)
- `ActiveEnterTimestamp` (`ulong`)

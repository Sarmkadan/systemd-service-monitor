# ServiceHealthCheckerTests

Unit tests for `ServiceHealthChecker` that validate health status evaluation logic for systemd services. These tests cover various service states, restart behaviors, and edge cases to ensure correct classification of service health according to predefined thresholds.

## API

### `GetHealthStatus_NullService_ReturnsUnknown`

Verifies that when a null service reference is provided, the health status is correctly identified as `Unknown`. This test ensures proper null handling in the health evaluation pipeline.

**Parameters**
- None

**Return value**
- `void` (asserts expected status)

**Throws**
- Does not throw under test conditions

---

### `GetHealthStatus_FailedService_ReturnsCritical`

Ensures that a service in a failed state (`systemctl show` exit code indicating failure) is classified as `Critical`. This test validates the immediate failure detection path.

**Parameters**
- None

**Return value**
- `void` (asserts expected status)

**Throws**
- Does not throw under test conditions

---

### `GetHealthStatus_ServiceWithHighRestartCount_ReturnsCritical`

Checks that a service exceeding the high restart threshold is marked as `Critical`. This test simulates a service with excessive crash/restart cycles indicative of instability.

**Parameters**
- None

**Return value**
- `void` (asserts expected status)

**Throws**
- Does not throw under test conditions

---
### `GetHealthStatus_ServiceWithModerateRestartCount_ReturnsWarning`

Validates that a service with a restart count within the moderate range is classified as `Warning`. This test ensures the threshold between `Warning` and `Critical` is enforced correctly.

**Parameters**
- None

**Return value**
- `void` (asserts expected status)

**Throws**
- Does not throw under test conditions

---
### `GetHealthStatus_ActivatingService_ReturnsWarning`

Confirms that a service currently in the `activating` state is treated as `Warning`. This test covers transient states that may indicate delayed startup issues.

**Parameters**
- None

**Return value**
- `void` (asserts expected status)

**Throws**
- Does not throw under test conditions

---
### `GetHealthStatus_DeactivatingService_ReturnsWarning`

Ensures that a service in the `deactivating` state is classified as `Warning`. This test addresses services undergoing controlled shutdown where issues may arise.

**Parameters**
- None

**Return value**
- `void` (asserts expected status)

**Throws**
- Does not throw under test conditions

---
### `GetHealthStatus_ActiveAndStableService_ReturnsHealthy`

Verifies that a service in the `active (running)` state with no recent restarts is classified as `Healthy`. This test represents the ideal operational state.

**Parameters**
- None

**Return value**
- `void` (asserts expected status)

**Throws**
- Does not throw under test conditions

---
### `GetHealthStatus_ActiveServiceWithMinimalRestarts_ReturnsHealthy`

Checks that an active service with a minimal restart count (below warning threshold) is classified as `Healthy`. This test ensures the lower bound for `Healthy` status is respected.

**Parameters**
- None

**Return value**
- `void` (asserts expected status)

**Throws**
- Does not throw under test conditions

---
### `GetHealthStatus_ActiveServiceWithManyRestarts_ReturnsWarning`

Validates that an active service with a restart count exceeding the healthy threshold but below critical is classified as `Warning`. This test bridges the gap between `Healthy` and `Critical` states.

**Parameters**
- None

**Return value**
- `void` (asserts expected status)

**Throws**
- Does not throw under test conditions

---
### `GetHealthStatus_InactiveButAutoStartDisabled_ReturnsHealthy`

Ensures that an inactive service with `AutoStart` disabled is treated as `Healthy`. This test covers services explicitly configured to remain stopped.

**Parameters**
- None

**Return value**
- `void` (asserts expected status)

**Throws**
- Does not throw under test conditions

---
### `GetHealthStatus_InactiveButAutoStartEnabled_ReturnsWarning`

Confirms that an inactive service with `AutoStart` enabled is classified as `Warning`. This test addresses services expected to run but currently stopped.

**Parameters**
- None

**Return value**
- `void` (asserts expected status)

**Throws**
- Does not throw under test conditions

---
### `GetHealthStatus_UnknownState_ReturnsUnknown`

Validates that a service in an unrecognized state is classified as `Unknown`. This test ensures robustness against unexpected systemd states.

**Parameters**
- None

**Return value**
- `void` (asserts expected status)

**Throws**
- Does not throw under test conditions

---
### `GetHealthSummary_ReturnNonEmptyString`

Asserts that the health summary method returns a non-empty string containing diagnostic information. This test verifies the presence of meaningful output for monitoring integration.

**Parameters**
- None

**Return value**
- `void` (asserts non-empty string)

**Throws**
- Does not throw under test conditions

---

## Usage

### Example 1: Validating Service Health in a Monitoring Loop

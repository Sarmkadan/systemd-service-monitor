# ServiceMonitorServiceTests

`ServiceMonitorServiceTests` is a test class dedicated to validating the behavior of the `ServiceMonitorService` within the `systemd-service-monitor` project. This suite ensures the service correctly interfaces with the underlying repository, handles various success scenarios such as retrieving active or all services, and manages failure states, including repository exceptions and empty result sets.

## API

- `public ServiceMonitorServiceTests()`
  Initializes a new instance of the `ServiceMonitorServiceTests` class, typically setting up the necessary mocks and service dependencies.

- `public async Task GetAllServicesAsync_ReturnsAllServices()`
  Verifies that `GetAllServicesAsync` successfully retrieves the complete list of services from the repository.

- `public async Task GetAllServicesAsync_WhenRepositoryThrows_LogsErrorAndThrows()`
  Verifies that when the repository throws an exception during a call to `GetAllServicesAsync`, the service logs the error appropriately and re-throws the exception.

- `public async Task GetServiceByNameAsync_WithValidName_ReturnsService()`
  Verifies that `GetServiceByNameAsync` returns the correct service object when provided with a valid, existing service name.

- `public async Task GetServiceByNameAsync_WithNonExistentName_ReturnsNull()`
  Verifies that `GetServiceByNameAsync` returns `null` when requested with a service name that does not exist in the repository.

- `public async Task GetActiveServicesAsync_ReturnsOnlyActiveServices()`
  Verifies that `GetActiveServicesAsync` filters results to return only services that are currently in an active state.

- `public async Task GetServiceByNameAsync_WhenRepositoryThrows_LogsErrorAndThrows()`
  Verifies that when the repository throws an exception during a call to `GetServiceByNameAsync`, the service logs the error appropriately and re-throws the exception.

- `public async Task GetAllServicesAsync_EmptyResult_ReturnsEmptyEnumerable()`
  Verifies that `GetAllServicesAsync` returns an empty enumerable when the underlying repository contains no services.

## Usage

### 1. Running tests via CLI
To execute all tests within this class using the .NET CLI:
```bash
dotnet test --filter ServiceMonitorServiceTests
```

### 2. Invoking tests in a test runner
The tests are designed to be executed automatically by a standard NUnit or xUnit test runner. Below is a conceptual example of how these are triggered within a test framework:
```csharp
// The test framework automatically instantiates the class and calls the test methods
[TestFixture]
public class ServiceMonitorServiceTests
{
    [Test]
    public async Task TestGetAllServices()
    {
        var tests = new ServiceMonitorServiceTests();
        await tests.GetAllServicesAsync_ReturnsAllServices();
    }
}
```

## Notes

- **Asynchronous Operations:** All test methods are `async` and return `Task`, reflecting the asynchronous nature of the `ServiceMonitorService` API. Ensure the test runner supports asynchronous test execution.
- **Exception Handling:** Tests covering repository failures (`WhenRepositoryThrows`) rely on mock behavior to simulate repository exceptions. They ensure that these exceptions are propagated correctly after internal logging logic has executed.
- **Dependency Injection:** These tests assume a test-ready environment where dependencies (such as the repository) are mocked or substituted before invoking the service methods.
- **Thread Safety:** While these tests themselves are executed in a test runner context, the underlying `ServiceMonitorService` is expected to be thread-safe regarding its data retrieval operations. Ensure that any shared resources used during testing are properly isolated.

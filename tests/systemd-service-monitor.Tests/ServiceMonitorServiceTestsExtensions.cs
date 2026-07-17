using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace systemd_service_monitor.Tests
{
    /// <summary>
    /// Extension methods that make it easier to work with <see cref="ServiceMonitorServiceTests"/> instances
    /// from other test suites or helper code.
    /// </summary>
    public static class ServiceMonitorServiceTestsExtensions
    {
        /// <summary>
        /// Executes all public test methods on the supplied <see cref="ServiceMonitorServiceTests"/> instance
        /// sequentially and returns the names of the methods that completed without throwing.
        /// </summary>
        /// <param name="tests">The test class instance to run.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A read‑only list containing the names of the successfully executed test methods.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static async Task<IReadOnlyList<string>> RunAllTestsAsync(
            this ServiceMonitorServiceTests tests,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var successful = new List<string>();

            // The known public test methods (ordered for readability)
            await tests.GetAllServicesAsync_ReturnsAllServices().ConfigureAwait(false);
            successful.Add(nameof(tests.GetAllServicesAsync_ReturnsAllServices));

            await tests.GetAllServicesAsync_WhenRepositoryThrows_LogsErrorAndThrows().ConfigureAwait(false);
            successful.Add(nameof(tests.GetAllServicesAsync_WhenRepositoryThrows_LogsErrorAndThrows));

            await tests.GetServiceByNameAsync_WithValidName_ReturnsService().ConfigureAwait(false);
            successful.Add(nameof(tests.GetServiceByNameAsync_WithValidName_ReturnsService));

            await tests.GetServiceByNameAsync_WithNonExistentName_ReturnsNull().ConfigureAwait(false);
            successful.Add(nameof(tests.GetServiceByNameAsync_WithNonExistentName_ReturnsNull));

            await tests.GetActiveServicesAsync_ReturnsOnlyActiveServices().ConfigureAwait(false);
            successful.Add(nameof(tests.GetActiveServicesAsync_ReturnsOnlyActiveServices));

            await tests.GetAllServicesAsync_EmptyResult_ReturnsEmptyEnumerable().ConfigureAwait(false);
            successful.Add(nameof(tests.GetAllServicesAsync_EmptyResult_ReturnsEmptyEnumerable));

            return successful;
        }

        /// <summary>
        /// Executes all public test methods on the supplied <see cref="ServiceMonitorServiceTests"/> instance
        /// and returns a dictionary that maps each method name to the exception (if any) that was thrown.
        /// </summary>
        /// <param name="tests">The test class instance to run.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>
        /// A read‑only dictionary where the key is the test method name and the value is the caught
        /// <see cref="Exception"/> or <c>null</c> when the method completed successfully.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static async Task<IReadOnlyDictionary<string, Exception?>> RunAllTestsWithResultsAsync(
            this ServiceMonitorServiceTests tests,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var results = new Dictionary<string, Exception?>();

            async Task RunAndCaptureAsync(Func<Task> testMethod, string name)
            {
                try
                {
                    await testMethod().ConfigureAwait(false);
                    results[name] = null;
                }
                catch (Exception ex)
                {
                    results[name] = ex;
                }
            }

            await RunAndCaptureAsync(tests.GetAllServicesAsync_ReturnsAllServices, nameof(tests.GetAllServicesAsync_ReturnsAllServices)).ConfigureAwait(false);
            await RunAndCaptureAsync(tests.GetAllServicesAsync_WhenRepositoryThrows_LogsErrorAndThrows, nameof(tests.GetAllServicesAsync_WhenRepositoryThrows_LogsErrorAndThrows)).ConfigureAwait(false);
            await RunAndCaptureAsync(tests.GetServiceByNameAsync_WithValidName_ReturnsService, nameof(tests.GetServiceByNameAsync_WithValidName_ReturnsService)).ConfigureAwait(false);
            await RunAndCaptureAsync(tests.GetServiceByNameAsync_WithNonExistentName_ReturnsNull, nameof(tests.GetServiceByNameAsync_WithNonExistentName_ReturnsNull)).ConfigureAwait(false);
            await RunAndCaptureAsync(tests.GetActiveServicesAsync_ReturnsOnlyActiveServices, nameof(tests.GetActiveServicesAsync_ReturnsOnlyActiveServices)).ConfigureAwait(false);
            await RunAndCaptureAsync(tests.GetAllServicesAsync_EmptyResult_ReturnsEmptyEnumerable, nameof(tests.GetAllServicesAsync_EmptyResult_ReturnsEmptyEnumerable)).ConfigureAwait(false);

            return results;
        }

        /// <summary>
        /// Returns the names of all public test methods defined on <see cref="ServiceMonitorServiceTests"/>
        /// using reflection. This can be useful for dynamic test discovery.
        /// </summary>
        /// <param name="tests">The test class instance (only used to obtain the type).</param>
        /// <returns>A read‑only list of method names that match the pattern <c>Get*Async*</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetTestMethodNames(this ServiceMonitorServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var methodNames = new List<string>();
            var type = tests.GetType();

            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                // All test methods are async and return Task, and their names contain "Async"
                if (method.ReturnType == typeof(Task) && method.Name.Contains("Async", StringComparison.Ordinal))
                {
                    methodNames.Add(method.Name);
                }
            }

            return methodNames;
        }
    }
}
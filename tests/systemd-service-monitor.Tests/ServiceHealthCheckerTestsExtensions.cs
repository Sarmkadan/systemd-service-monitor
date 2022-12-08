using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SystemdServiceMonitor.Tests
{
    /// <summary>
    /// Extension methods that make it easier to work with <see cref="ServiceHealthCheckerTests"/>
    /// from other test code or utilities.
    /// </summary>
    public static class ServiceHealthCheckerTestsExtensions
    {
        /// <summary>
        /// Executes the <c>GetHealthStatus_NullService_ReturnsUnknown</c> test and returns <c>true</c>
        /// if it completes without throwing an exception, otherwise <c>false</c>.
        /// </summary>
        /// <param name="tests">The test instance to execute the method on.</param>
        /// <returns><c>true</c> if the test completes without throwing an exception; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static bool VerifyNullService(this ServiceHealthCheckerTests tests) =>
            RunTest(() => tests.GetHealthStatus_NullService_ReturnsUnknown());

        /// <summary>
        /// Executes the <c>GetHealthStatus_ActiveAndStableService_ReturnsHealthy</c> test and returns
        /// <c>true</c> if it succeeds, otherwise <c>false</c>.
        /// </summary>
        /// <param name="tests">The test instance to execute the method on.</param>
        /// <returns><c>true</c> if the test completes without throwing an exception; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static bool VerifyActiveAndStableService(this ServiceHealthCheckerTests tests) =>
            RunTest(() => tests.GetHealthStatus_ActiveAndStableService_ReturnsHealthy());

        /// <summary>
        /// Runs every public method on <see cref="ServiceHealthCheckerTests"/> whose name starts with
        /// <c>GetHealthStatus_</c> and returns a dictionary mapping the method name to a boolean that
        /// indicates whether the method completed without throwing.
        /// </summary>
        /// <param name="tests">The test instance to execute the methods on.</param>
        /// <returns>A dictionary mapping method names to boolean results indicating test success.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static IDictionary<string, bool> RunAllHealthStatusTests(this ServiceHealthCheckerTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var result = new Dictionary<string, bool>(StringComparer.Ordinal);
            var methods = typeof(ServiceHealthCheckerTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.Name.StartsWith("GetHealthStatus_", StringComparison.Ordinal));

            foreach (var method in methods)
            {
                bool passed;
                try
                {
                    method.Invoke(tests, null);
                    passed = true;
                }
                catch
                {
                    passed = false;
                }

                result[method.Name] = passed;
            }

            return result;
        }

        /// <summary>
        /// Helper that executes an <see cref="Action"/> and returns <c>true</c> if no exception is thrown.
        /// </summary>
        /// <param name="test">The test action to execute.</param>
        /// <returns><c>true</c> if the action completes without throwing; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="test"/> is <c>null</c>.</exception>
        private static bool RunTest(Action test)
        {
            ArgumentNullException.ThrowIfNull(test);

            try
            {
                test();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
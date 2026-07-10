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
        public static bool VerifyNullService(this ServiceHealthCheckerTests tests) =>
            RunTest(() => tests.GetHealthStatus_NullService_ReturnsUnknown());

        /// <summary>
        /// Executes the <c>GetHealthStatus_ActiveAndStableService_ReturnsHealthy</c> test and returns
        /// <c>true</c> if it succeeds, otherwise <c>false</c>.
        /// </summary>
        public static bool VerifyActiveAndStableService(this ServiceHealthCheckerTests tests) =>
            RunTest(() => tests.GetHealthStatus_ActiveAndStableService_ReturnsHealthy());

        /// <summary>
        /// Runs every public method on <see cref="ServiceHealthCheckerTests"/> whose name starts with
        /// <c>GetHealthStatus_</c> and returns a dictionary mapping the method name to a boolean that
        /// indicates whether the method completed without throwing.
        /// </summary>
        public static IDictionary<string, bool> RunAllHealthStatusTests(this ServiceHealthCheckerTests tests)
        {
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
        private static bool RunTest(Action test)
        {
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

#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SystemdServiceMonitor.Utilities;

/// <summary>
/// Validation helpers for PaginationHelperTests to ensure test data is valid.
/// </summary>
public static class PaginationHelperTestsValidation
{
    /// <summary>
    /// Validates a PaginationHelperTests instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this PaginationHelperTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate test method parameters
        foreach (var method in GetTestMethods(value))
        {
            try
            {
                ValidateTestMethod(method, problems);
            }
            catch (Exception ex)
            {
                problems.Add($"Failed to validate {method.Name}: {ex.Message}");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the PaginationHelperTests instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static bool IsValid(this PaginationHelperTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return !value.Validate().Any();
    }

    /// <summary>
    /// Ensures that the PaginationHelperTests instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if value is null or contains validation problems.</exception>
    public static void EnsureValid(this PaginationHelperTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"PaginationHelperTests is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }

    private static IEnumerable<System.Reflection.MethodInfo> GetTestMethods(PaginationHelperTests value)
    {
        if (value is null)
        {
            yield break;
        }

        var type = value.GetType();
        var methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

        foreach (var method in methods)
        {
            if (method.Name.StartsWith("CalculateTotalPages_", StringComparison.Ordinal) ||
                method.Name.StartsWith("CalculateSkip_", StringComparison.Ordinal) ||
                method.Name.StartsWith("ValidatePaginationParams_", StringComparison.Ordinal) ||
                method.Name.StartsWith("GetPageNumbers_", StringComparison.Ordinal) ||
                method.Name.StartsWith("GetMetadata_", StringComparison.Ordinal))
            {
                yield return method;
            }
        }
    }

    private static void ValidateTestMethod(System.Reflection.MethodInfo method, List<string> problems)
    {
        var parameters = method.GetParameters();
        var paramValues = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            paramValues[i] = GetDefaultValue(parameters[i].ParameterType);
        }

        try
        {
            // Invoke the test method to get the actual values
            var result = method.Invoke(null, paramValues);

            // Validate the results based on method name
            switch (method.Name)
            {
                case var name when name.StartsWith("CalculateTotalPages_"):
                    ValidateCalculateTotalPagesResult(result, problems);
                    break;

                case var name when name.StartsWith("CalculateSkip_"):
                    ValidateCalculateSkipResult(result, problems);
                    break;

                case var name when name.StartsWith("ValidatePaginationParams_"):
                    ValidateValidatePaginationParamsResult(result, problems);
                    break;

                case var name when name.StartsWith("GetPageNumbers_"):
                    ValidateGetPageNumbersResult(result, problems);
                    break;

                case var name when name.StartsWith("GetMetadata_"):
                    ValidateGetMetadataResult(result, problems);
                    break;
            }
        }
        catch (Exception ex) when (ex is not ArgumentException and not ArgumentNullException)
        {
            problems.Add($"{method.Name} threw exception: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    private static object GetDefaultValue(Type type)
    {
        return type switch
        {
            Type t when t == typeof(int) => 0,
            Type t when t == typeof(int?) => null,
            Type t when t == typeof(string) => string.Empty,
            Type t when t == typeof(bool) => false,
            Type t when t == typeof(List<int>) => new List<int>(),
            Type t when t == typeof(global::SystemdServiceMonitor.Utilities.PaginationMetadata) => null,
            _ => Activator.CreateInstance(type)
        };
    }

    private static void ValidateCalculateTotalPagesResult(object? result, List<string> problems)
    {
        if (result is not int intResult)
        {
            problems.Add("CalculateTotalPages_* test returned non-integer result");
            return;
        }

        // CalculateTotalPages should return 0 for invalid page sizes, positive otherwise
        if (intResult < 0)
        {
            problems.Add("CalculateTotalPages returned negative value");
        }
    }

    private static void ValidateCalculateSkipResult(object? result, List<string> problems)
    {
        if (result is not int intResult)
        {
            problems.Add("CalculateSkip_* test returned non-integer result");
            return;
        }

        // CalculateSkip should return non-negative values
        if (intResult < 0)
        {
            problems.Add("CalculateSkip returned negative value");
        }
    }

    private static void ValidateValidatePaginationParamsResult(object? result, List<string> problems)
    {
        if (result is not ValueTuple<int, int> tupleResult)
        {
            problems.Add("ValidatePaginationParams_* test returned non-tuple result");
            return;
        }

        // Validate that page numbers are positive and page sizes are reasonable
        if (tupleResult.Item1 < 1)
        {
            problems.Add("ValidatePaginationParams returned page number less than 1");
        }

        if (tupleResult.Item2 < 1 || tupleResult.Item2 > PaginationHelper.MaxPageSize)
        {
            problems.Add($"ValidatePaginationParams returned invalid page size: {tupleResult.Item2} (must be 1-{PaginationHelper.MaxPageSize})");
        }
    }

    private static void ValidateGetPageNumbersResult(object? result, List<string> problems)
    {
        if (result is not List<int> listResult)
        {
            problems.Add("GetPageNumbers_* test returned non-list result");
            return;
        }

        // Validate page numbers list
        if (listResult.Any(p => p < 1))
        {
            problems.Add("GetPageNumbers returned list containing page numbers less than 1");
        }

        // Check if list is sorted
        for (int i = 1; i < listResult.Count; i++)
        {
            if (listResult[i] <= listResult[i - 1])
            {
                problems.Add("GetPageNumbers returned unsorted list");
                break;
            }
        }
    }

    private static void ValidateGetMetadataResult(object? result, List<string> problems)
    {
        if (result is not global::SystemdServiceMonitor.Utilities.PaginationMetadata metadata)
        {
            problems.Add("GetMetadata_* test returned non-metadata result");
            return;
        }

        // Validate metadata fields
        if (metadata.PageNumber < 1)
        {
            problems.Add("PaginationMetadata.PageNumber is less than 1");
        }

        if (metadata.PageSize < 1 || metadata.PageSize > PaginationHelper.MaxPageSize)
        {
            problems.Add($"PaginationMetadata.PageSize is invalid: {metadata.PageSize} (must be 1-{PaginationHelper.MaxPageSize})");
        }

        if (metadata.TotalCount < 0)
        {
            problems.Add("PaginationMetadata.TotalCount is negative");
        }

        if (metadata.TotalPages < 0)
        {
            problems.Add("PaginationMetadata.TotalPages is negative");
        }

        if (metadata.StartIndex < 1)
        {
            problems.Add("PaginationMetadata.StartIndex is less than 1");
        }

        if (metadata.EndIndex < metadata.StartIndex)
        {
            problems.Add("PaginationMetadata.EndIndex is less than StartIndex");
        }

        if (metadata.EndIndex > metadata.TotalCount)
        {
            problems.Add("PaginationMetadata.EndIndex exceeds TotalCount");
        }

        // Validate HasPreviousPage and HasNextPage consistency
        var expectedHasPrevious = metadata.PageNumber > 1;
        if (metadata.HasPreviousPage != expectedHasPrevious)
        {
            problems.Add($"PaginationMetadata.HasPreviousPage is {metadata.HasPreviousPage} but should be {expectedHasPrevious} for PageNumber {metadata.PageNumber}");
        }

        var expectedTotalPages = PaginationHelper.CalculateTotalPages(metadata.TotalCount, metadata.PageSize);
        var expectedHasNext = metadata.PageNumber < expectedTotalPages;
        if (metadata.HasNextPage != expectedHasNext)
        {
            problems.Add($"PaginationMetadata.HasNextPage is {metadata.HasNextPage} but should be {expectedHasNext} for PageNumber {metadata.PageNumber} and TotalPages {expectedTotalPages}");
        }
    }
}
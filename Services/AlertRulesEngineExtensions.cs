#nullable enable

using SystemdServiceMonitor.Dtos;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Extension methods for <see cref="AlertRulesEngine"/> providing additional utility functionality
/// for alert rule management, incident filtering, and summary operations.
/// </summary>
public static class AlertRulesEngineExtensions
{
    /// <summary>
    /// Gets all alert rules filtered by severity level.
    /// </summary>
    /// <param name="engine">The alert rules engine instance.</param>
    /// <param name="severity">The severity level to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Filtered collection of alert rules.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="engine"/> is null.</exception>
    public static async Task<IEnumerable<AlertRule>> GetRulesBySeverityAsync(
        this AlertRulesEngine engine,
        AlertSeverity severity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);

        var rules = await engine.GetRulesAsync(cancellationToken);
        return rules.Where(r => r.Severity == severity).OrderBy(r => r.Name);
    }

    /// <summary>
    /// Gets all active incidents filtered by severity level.
    /// </summary>
    /// <param name="engine">The alert rules engine instance.</param>
    /// <param name="severity">The severity level to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Filtered collection of active alert incidents.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="engine"/> is null.</exception>
    public static async Task<IEnumerable<AlertIncident>> GetActiveIncidentsBySeverityAsync(
        this AlertRulesEngine engine,
        AlertSeverity severity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);

        var incidents = await engine.GetActiveIncidentsAsync(cancellationToken);
        return incidents.Where(i => i.Severity == severity).OrderByDescending(i => i.CreatedAt);
    }

    /// <summary>
    /// Gets all incidents for a specific service.
    /// </summary>
    /// <param name="engine">The alert rules engine instance.</param>
    /// <param name="serviceName">Name of the service to filter by.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of incidents for the specified service.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="engine"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="serviceName"/> is null or empty.</exception>
    public static async Task<IEnumerable<AlertIncident>> GetIncidentsByServiceAsync(
        this AlertRulesEngine engine,
        string serviceName,
        int maxResults = 100,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);

        var incidents = await engine.GetIncidentHistoryAsync(serviceName, maxResults, cancellationToken);
        return incidents.OrderByDescending(i => i.CreatedAt);
    }

    /// <summary>
    /// Gets the count of active incidents grouped by severity level.
    /// </summary>
    /// <param name="engine">The alert rules engine instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary mapping severity levels to active incident counts.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="engine"/> is null.</exception>
    public static async Task<Dictionary<AlertSeverity, int>> GetActiveIncidentCountsBySeverityAsync(
        this AlertRulesEngine engine,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);

        var activeIncidents = await engine.GetActiveIncidentsAsync(cancellationToken);
        return activeIncidents
            .GroupBy(i => i.Severity)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Gets all alert rules that match a specific service pattern.
    /// </summary>
    /// <param name="engine">The alert rules engine instance.</param>
    /// <param name="servicePattern">The service pattern to match against.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Filtered collection of alert rules matching the pattern.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="engine"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="servicePattern"/> is null or empty.</exception>
    public static async Task<IEnumerable<AlertRule>> GetRulesByServicePatternAsync(
        this AlertRulesEngine engine,
        string servicePattern,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentException.ThrowIfNullOrEmpty(servicePattern);

        var rules = await engine.GetRulesAsync(cancellationToken);
        return rules.Where(r => MatchesServicePattern(r.ServicePattern, servicePattern)).OrderBy(r => r.Name);
    }

    /// <summary>
    /// Gets the most recent active incident for a specific service.
    /// </summary>
    /// <param name="engine">The alert rules engine instance.</param>
    /// <param name="serviceName">Name of the service to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The most recent active incident, or null if none exists.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="engine"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="serviceName"/> is null or empty.</exception>
    public static async Task<AlertIncident?> GetLatestIncidentForServiceAsync(
        this AlertRulesEngine engine,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);

        var incidents = await engine.GetIncidentsByServiceAsync(serviceName, 1, cancellationToken);
        return incidents.FirstOrDefault();
    }

    /// <summary>
    /// Gets all unacknowledged active incidents.
    /// </summary>
    /// <param name="engine">The alert rules engine instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of unacknowledged active incidents.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="engine"/> is null.</exception>
    public static async Task<IEnumerable<AlertIncident>> GetUnacknowledgedActiveIncidentsAsync(
        this AlertRulesEngine engine,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);

        var activeIncidents = await engine.GetActiveIncidentsAsync(cancellationToken);
        return activeIncidents.Where(i => i.State != AlertIncidentState.Acknowledged).OrderByDescending(i => i.CreatedAt);
    }

    /// <summary>
    /// Gets all escalated incidents.
    /// </summary>
    /// <param name="engine">The alert rules engine instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of escalated incidents.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="engine"/> is null.</exception>
    public static async Task<IEnumerable<AlertIncident>> GetEscalatedIncidentsAsync(
        this AlertRulesEngine engine,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);

        var activeIncidents = await engine.GetActiveIncidentsAsync(cancellationToken);
        return activeIncidents.Where(i => i.State == AlertIncidentState.Escalated).OrderByDescending(i => i.CreatedAt);
    }

    /// <summary>
    /// Gets a summary of incidents by state.
    /// </summary>
    /// <param name="engine">The alert rules engine instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary mapping incident states to counts.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="engine"/> is null.</exception>
    public static async Task<Dictionary<AlertIncidentState, int>> GetIncidentCountsByStateAsync(
        this AlertRulesEngine engine,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);

        var activeIncidents = await engine.GetActiveIncidentsAsync(cancellationToken);
        return activeIncidents
            .GroupBy(i => i.State)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Determines whether a service name matches a given service pattern.
    /// </summary>
    /// <param name="pattern">The pattern to match against (supports wildcards).</param>
    /// <param name="serviceName">The service name to check.</param>
    /// <returns>True if the service name matches the pattern; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="pattern"/> or <paramref name="serviceName"/> is null or empty.</exception>
    private static bool MatchesServicePattern(string pattern, string serviceName)
    {
        ArgumentException.ThrowIfNullOrEmpty(pattern);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);

        if (pattern == "*")
            return true;

        if (pattern.EndsWith('*'))
            return serviceName.StartsWith(pattern[..^1], StringComparison.OrdinalIgnoreCase);

        return serviceName.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }
}
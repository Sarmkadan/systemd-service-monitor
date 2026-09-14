# DependencyNodeExtensions

Extension methods for <see cref="DependencyNode"/> providing utility checks and summaries.

## HasDependencies

Checks if the node has any dependencies.

```csharp
public static bool HasDependencies(this DependencyNode node)
```

### Parameters

- `node`: The <see cref="DependencyNode"/> to check.

### Returns

<see langword="true"/> if the node has dependencies; otherwise, <see langword="false"/>.

### Exceptions

- <see cref="ArgumentNullException">: Thrown when <paramref name="node"/> is <see langword="null"/>.

## HasDependents

Checks if the node has any dependents.

```csharp
public static bool HasDependents(this DependencyNode node)
```

### Parameters

- `node`: The <see cref="DependencyNode"/> to check.

### Returns

<see langword="true"/> if the node has dependents; otherwise, <see langword="false"/>.

### Exceptions

- <see cref="ArgumentNullException">: Thrown when <paramref name="node"/> is <see langword="null"/>.

## IsIsolated

Determines if the node is isolated (no dependencies and no dependents).

```csharp
public static bool IsIsolated(this DependencyNode node)
```

### Parameters

- `node`: The <see cref="DependencyNode"/> to check.

### Returns

<see langword="true"/> if the node is isolated; otherwise, <see langword="false"/>.

### Exceptions

- <see cref="ArgumentNullException">: Thrown when <paramref name="node"/> is <see langword="null"/>.

## GetSummary

Gets a descriptive string summary of the node.

```csharp
public static string GetSummary(this DependencyNode node)
```

### Parameters

- `node`: The <see cref="DependencyNode"/> to summarize.

### Returns

A string summary containing the service name, state, and dependency counts.

### Exceptions

- <see cref="ArgumentNullException">: Thrown when <paramref name="node"/> is <see langword="null"/>.

### Remarks

The summary format is: "{ServiceName} [{State}] (Deps: {DependencyCount}, Dependents: {DependentCount})"
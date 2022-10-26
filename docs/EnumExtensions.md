# EnumExtensions

The `EnumExtensions` class provides a set of static utility methods designed to extend the functionality of .NET enumerations. These methods simplify common operations such as retrieving metadata, parsing strings, and managing flag-based enums, reducing boilerplate code when performing enum-related operations within the `systemd-service-monitor` project.

## API

### GetDescription<T>(T value)
Retrieves the value of the `DescriptionAttribute` applied to the specified enum member. If the attribute is not present, it returns the string representation of the enum member.
- **Parameters**: `value` - The enum member to retrieve the description for.
- **Returns**: The description string, or the enum name if no description is found.

### TryParseEnum<T>(string value)
Attempts to parse a string into a value of the specified enum type.
- **Parameters**: `value` - The string to parse.
- **Returns**: The parsed enum value of type `T` if successful; otherwise, `null`.

### GetValues<T>()
Retrieves all defined values for the specified enum type.
- **Returns**: An `IEnumerable<T>` containing all defined enum members.

### GetValuesWithDescriptions<T>()
Retrieves a dictionary where the keys are the descriptions (or names if no description is provided) and the values are the corresponding enum members.
- **Returns**: A `Dictionary<string, T>` mapping descriptions to enum values.

### HasFlag<T>(T value, T flag)
Determines whether a specific bitwise flag is set within the provided enum value.
- **Parameters**: `value` - The enum value to check; `flag` - The flag to look for.
- **Returns**: `true` if the flag is set; otherwise, `false`.

### GetNumericValue<T>(T value)
Retrieves the underlying numeric value of the specified enum member as an object.
- **Parameters**: `value` - The enum member to convert.
- **Returns**: The numeric representation of the enum member.

### ToFriendlyString<T>(T value)
Converts an enum member into a human-readable string representation, prioritizing the `DescriptionAttribute` if defined, otherwise defaulting to the enum member name.
- **Parameters**: `value` - The enum member to convert.
- **Returns**: A human-friendly string representation.

## Usage

```csharp
// Example 1: Parsing and Friendly Strings
string input = "Running";
ServiceStatus? status = EnumExtensions.TryParseEnum<ServiceStatus>(input);

if (status.HasValue)
{
    string friendlyName = EnumExtensions.ToFriendlyString(status.Value);
    Console.WriteLine($"Service status is: {friendlyName}");
}
```

```csharp
// Example 2: Checking Flags
[Flags]
public enum ServiceCapabilities { None = 0, Restart = 1, Stop = 2 }

ServiceCapabilities capabilities = ServiceCapabilities.Restart | ServiceCapabilities.Stop;

if (EnumExtensions.HasFlag(capabilities, ServiceCapabilities.Restart))
{
    Console.WriteLine("Restart is supported.");
}
```

## Notes

- **Thread Safety**: All methods within `EnumExtensions` are static and stateless, making them inherently thread-safe for concurrent access.
- **Enum Constraints**: These methods generally expect `T` to be an enum type. Behavior when passed non-enum types is undefined and may result in runtime exceptions.
- **Null Handling**: When `TryParseEnum<T>` is passed a `null` or empty string, it returns `null`. Other methods may throw an `ArgumentNullException` if called with invalid arguments, depending on the underlying `System.Enum` implementation behavior.

# Contributing to systemd-service-monitor

Thank you for your interest in contributing to systemd-service-monitor! We welcome contributions of all kinds, including bug reports, feature requests, documentation improvements, and code changes.

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Git
- Linux system with systemd
- Basic understanding of systemd, D-Bus, and ASP.NET

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/systemd-service-monitor.git
   cd systemd-service-monitor
   ```
3. Add the upstream repository as a remote:
   ```bash
   git remote add upstream https://github.com/sarmkadan/systemd-service-monitor.git
   ```

### Development Setup

1. **Build the project:**
   ```bash
   dotnet build
   ```

2. **Run tests:**
   ```bash
   dotnet test
   ```

3. **Run the application:**
   ```bash
   dotnet run --project systemd-service-monitor.csproj
   ```

4. **View available make targets:**
   ```bash
   make help
   ```

## Branching and Commits

1. Create a feature branch from `main`:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. Make your changes and commit with clear, descriptive messages:
   ```bash
   git commit -m "Add feature: brief description"
   ```

3. Keep commits atomic and logically grouped. Write commit messages in the imperative mood ("Add", not "Added").

## Code Style and Conventions

- **Follow C# coding standards**: Use PascalCase for class/method names, camelCase for local variables
- **XML Documentation**: Add XML doc comments to public classes, methods, and properties:
  ```csharp
  /// <summary>
  /// Retrieves service information from systemd.
  /// </summary>
  /// <param name="serviceName">Name of the service</param>
  /// <returns>ServiceInfo object</returns>
  public ServiceInfo GetServiceInfo(string serviceName)
  {
      // implementation
  }
  ```
- **Author Headers**: Maintain author attribution headers in existing files. Do not modify or remove them.
- **Code Organization**: Group related functionality together. Use interfaces for dependencies and implement dependency injection.
- **Exception Handling**: Use custom exceptions (derived from `ServiceMonitorException`) and handle them appropriately.
- **Logging**: Use Serilog for structured logging. Include context information where helpful.

## Testing

- Write tests for new features and bug fixes
- Test coverage should be adequate for the code path
- Run all tests before submitting a pull request:
  ```bash
  dotnet test
  ```

## Submitting a Pull Request

1. Push your feature branch to your fork:
   ```bash
   git push origin feature/your-feature-name
   ```

2. Open a Pull Request on GitHub with:
   - A clear, descriptive title
   - Description of the changes and motivation
   - Reference to any related issues (e.g., "Fixes #123")
   - Evidence of testing (test results, manual testing steps)

3. Ensure all CI checks pass
4. Be responsive to code review feedback

## Reporting Issues

Issues are tracked on GitHub. Before opening a new issue:

1. Check existing issues to avoid duplicates
2. If reporting a bug, include:
   - systemd-service-monitor version
   - .NET SDK version
   - Steps to reproduce
   - Expected vs actual behavior
   - Relevant logs or error messages

3. For security vulnerabilities, please refer to [SECURITY.md](SECURITY.md) instead

## License

By contributing to this project, you agree that your contributions will be licensed under the MIT License.

## Questions or Need Help?

- Check the [documentation](docs/) for more information
- Review existing [issues](https://github.com/sarmkadan/systemd-service-monitor/issues) and discussions
- Open a GitHub Discussion for general questions

Thank you for helping improve systemd-service-monitor!

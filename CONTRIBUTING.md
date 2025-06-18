# Contributing to ReisingerIntelliAppV1

Thank you for your interest in contributing to ReisingerIntelliAppV1! This guide will help you get started with contributing to our .NET MAUI mobile application project.

## 🤝 Code of Conduct

By participating in this project, you agree to abide by our code of conduct. We are committed to providing a welcoming and inclusive environment for all contributors.

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have:

- **.NET 8.0 SDK** or later
- **Visual Studio 2022** with MAUI workload or **VS Code** with C# Dev Kit
- **Git** for version control
- **GitHub account** for pull requests
- **GitHub Copilot** (recommended for enhanced development experience)

### Setting Up Your Development Environment

1. **Fork the Repository**
   ```bash
   # Fork the repo on GitHub, then clone your fork
   git clone https://github.com/YOUR-USERNAME/ReisingerIntelliAppV1.git
   cd ReisingerIntelliAppV1
   ```

2. **Install Dependencies**
   ```bash
   dotnet restore
   dotnet workload install maui
   ```

3. **Configure GitHub Copilot** (Recommended)
   - Follow our [Copilot setup guide](.github/copilot-setup-steps.yml)
   - Review [Copilot instructions](.github/copilot-instructions.md)
   - Install recommended VS Code extensions from `.vscode/extensions.json`

4. **Verify Setup**
   ```bash
   dotnet build
   dotnet test
   ```

## 🎯 How to Contribute

### Types of Contributions

We welcome various types of contributions:

- 🐛 **Bug Reports**: Help us identify and fix issues
- ✨ **Feature Requests**: Suggest new functionality
- 📝 **Documentation**: Improve guides, comments, and explanations
- 🧪 **Testing**: Add or improve test coverage
- 🔧 **Code**: Implement features, fix bugs, or improve performance
- 🤖 **Copilot Feedback**: Help us improve our AI-assisted development

### Using GitHub Issues

We use GitHub issues to track bugs, feature requests, and tasks. Please use the appropriate template:

- **🐛 [Bug Report](.github/ISSUE_TEMPLATE/bug_report.yml)**: For reporting bugs
- **✨ [Feature Request](.github/ISSUE_TEMPLATE/feature_request.yml)**: For suggesting new features
- **📋 [Task](.github/ISSUE_TEMPLATE/task.yml)**: For general development tasks
- **🤖 [Copilot Feedback](.github/ISSUE_TEMPLATE/copilot_feedback.yml)**: For Copilot-related feedback

### Pull Request Process

1. **Create a Branch**
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b bugfix/issue-number-description
   ```

2. **Make Your Changes**
   - Follow our [coding standards](#coding-standards)
   - Write tests for new functionality
   - Update documentation as needed
   - Use GitHub Copilot to enhance your development experience

3. **Test Thoroughly**
   ```bash
   # Run all tests
   dotnet test
   
   # Test on multiple platforms if possible
   dotnet build -f net8.0-android
   dotnet build -f net8.0-ios
   dotnet build -f net8.0-windows10.0.19041.0
   ```

4. **Commit Your Changes**
   ```bash
   git add .
   git commit -m "feat: add new device discovery feature"
   ```

5. **Push and Create PR**
   ```bash
   git push origin feature/your-feature-name
   ```
   - Create a pull request on GitHub
   - Use the appropriate PR template
   - If you used Copilot, use the [Copilot-generated template](.github/PULL_REQUEST_TEMPLATE/copilot_generated.md)

## 📝 Coding Standards

### General Guidelines

- **Follow .NET Conventions**: Use standard .NET naming and coding conventions
- **MAUI Best Practices**: Follow MAUI-specific patterns and recommendations
- **MVVM Pattern**: Use Model-View-ViewModel architecture consistently
- **Async/Await**: Use async patterns for all I/O operations
- **Dependency Injection**: Use the built-in DI container for service management

### Code Style

We use automated tools to maintain consistent code style:

- **EditorConfig**: Automatic formatting (see `.editorconfig`)
- **StyleCop**: C# style analysis (see `stylecop.json`)
- **Code Analysis**: Static analysis rules (see `CodeAnalysis.ruleset`)

#### Key Style Rules

```csharp
// ✅ Good: Descriptive names
public async Task<DeviceStatusModel> GetDeviceStatusAsync(string deviceId)
{
    var device = await _deviceService.FindDeviceAsync(deviceId);
    return device?.Status ?? DeviceStatusModel.Unknown;
}

// ❌ Bad: Unclear names and poor structure
public async Task<object> Get(string id)
{
    var d = await _ds.Find(id);
    return d?.S ?? null;
}
```

#### MAUI-Specific Guidelines

```csharp
// ✅ Good: MVVM ViewModel pattern
[ObservableProperty]
private string deviceName = string.Empty;

[RelayCommand]
private async Task RefreshDevicesAsync()
{
    // Implementation
}

// ✅ Good: Platform-specific code
#if ANDROID
    // Android-specific implementation
#elif IOS
    // iOS-specific implementation
#endif
```

### Documentation

- **XML Comments**: All public APIs must have XML documentation
- **README Updates**: Update README.md for significant changes
- **Code Comments**: Explain complex logic and business rules
- **Copilot Context**: Write clear comments to help Copilot understand intent

```csharp
/// <summary>
/// Connects to an Intellidrive device and retrieves its current status.
/// This method handles both local network and cloud connections.
/// </summary>
/// <param name="device">The device to connect to</param>
/// <returns>The current device status, or null if connection fails</returns>
public async Task<DeviceStatusModel?> ConnectAndGetStatusAsync(DeviceModel device)
{
    // Implementation with clear variable names for Copilot context
    var connectionManager = _serviceProvider.GetRequiredService<IConnectionManager>();
    var apiClient = await connectionManager.CreateClientAsync(device);
    
    return await apiClient.GetStatusAsync();
}
```

## 🧪 Testing Guidelines

### Test Structure

- **Unit Tests**: Test individual components and services
- **Integration Tests**: Test component interactions
- **UI Tests**: Test user interface functionality (when applicable)
- **Platform Tests**: Test platform-specific functionality

### Writing Tests

```csharp
[Test]
public async Task GetDeviceStatusAsync_ValidDevice_ReturnsStatus()
{
    // Arrange
    var deviceId = "test-device-123";
    var expectedStatus = new DeviceStatusModel { IsOnline = true };
    _mockApiService.Setup(x => x.GetStatusAsync(deviceId))
               .ReturnsAsync(expectedStatus);

    // Act
    var result = await _deviceService.GetDeviceStatusAsync(deviceId);

    // Assert
    Assert.That(result, Is.Not.Null);
    Assert.That(result.IsOnline, Is.True);
}
```

### Test Coverage

- Aim for **80%+ code coverage** for new features
- All public APIs should have tests
- Critical business logic must be thoroughly tested
- Platform-specific code should have platform-specific tests

## 🤖 GitHub Copilot Best Practices

Since this project is optimized for GitHub Copilot, here are guidelines for effective usage:

### Writing Copilot-Friendly Code

1. **Descriptive Comments**
   ```csharp
   // Connect to device via WiFi and retrieve all available parameters
   public async Task<Dictionary<string, string>> GetAllDeviceParametersAsync(DeviceModel device)
   ```

2. **Clear Method Names**
   ```csharp
   // ✅ Good: Clear intent
   public async Task<bool> ValidateDeviceConnectionAsync(string ipAddress)
   
   // ❌ Bad: Unclear intent
   public async Task<bool> Check(string ip)
   ```

3. **Consistent Patterns**
   ```csharp
   // Follow established patterns for similar functionality
   public async Task<string> OpenDoorAsync(DeviceModel device) => await SendCommandAsync(device, "open");
   public async Task<string> CloseDoorAsync(DeviceModel device) => await SendCommandAsync(device, "close");
   public async Task<string> LockDoorAsync(DeviceModel device) => await SendCommandAsync(device, "lock");
   ```

### Copilot Code Review

When reviewing Copilot-generated code:

- ✅ **Verify Logic**: Ensure the generated code matches your intent
- ✅ **Check Error Handling**: Add proper exception handling if missing
- ✅ **Validate Security**: Ensure no security vulnerabilities are introduced
- ✅ **Test Thoroughly**: All Copilot-generated code must be tested
- ✅ **Document Usage**: Note when significant code was Copilot-generated

### Providing Copilot Feedback

Help us improve our Copilot integration:

- Use the [Copilot feedback template](.github/ISSUE_TEMPLATE/copilot_feedback.yml)
- Report both positive and negative experiences
- Suggest improvements to our Copilot configuration
- Share effective prompting techniques

## 🔒 Security Considerations

### Secure Coding Practices

- **Never commit secrets**: Use secure storage and environment variables
- **Validate inputs**: All user and network inputs must be validated
- **Use HTTPS**: All network communications must use secure protocols
- **Handle errors safely**: Don't expose sensitive information in error messages

### Security Review Process

- All PRs undergo security review
- Use `dotnet list package --vulnerable` to check for vulnerable dependencies
- Follow our [Security Policy](SECURITY.md) for reporting vulnerabilities

## 📋 Issue Labels and Project Management

We use the following labels to organize issues:

### Type Labels
- `bug` - Something isn't working
- `enhancement` - New feature or request
- `documentation` - Improvements or additions to documentation
- `question` - Further information is requested
- `task` - General development task

### Priority Labels
- `priority: critical` - Critical issues requiring immediate attention
- `priority: high` - Important issues to address soon
- `priority: medium` - Standard priority issues
- `priority: low` - Nice-to-have improvements

### Status Labels
- `status: needs-triage` - Needs initial review and prioritization
- `status: ready` - Ready for development
- `status: in-progress` - Currently being worked on
- `status: blocked` - Blocked by dependencies or decisions

### Special Labels
- `good first issue` - Good for newcomers
- `help wanted` - Extra attention is needed
- `copilot` - Related to GitHub Copilot
- `automated` - Created by automation

## 🚀 Release Process

### Versioning

We follow [Semantic Versioning](https://semver.org/):

- **MAJOR** version for incompatible API changes
- **MINOR** version for backwards-compatible functionality additions
- **PATCH** version for backwards-compatible bug fixes

### Release Workflow

1. **Feature Development**: Work on feature branches
2. **Pull Requests**: Submit PRs to `develop` branch
3. **Integration Testing**: Automated testing on `develop`
4. **Release Candidate**: Merge to `main` for release
5. **Deployment**: Automated deployment to app stores

## 📚 Additional Resources

### Documentation
- [Project README](README.md) - Project overview and quick start
- [Security Policy](SECURITY.md) - Security guidelines and reporting
- [Copilot Instructions](.github/copilot-instructions.md) - Project-specific Copilot guidance

### External Resources
- [.NET MAUI Documentation](https://docs.microsoft.com/en-us/dotnet/maui/)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [GitHub Copilot Documentation](https://docs.github.com/en/copilot)
- [MVVM Pattern Guide](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/enterprise-application-patterns/mvvm)

## ❓ Questions and Support

- **💬 Discussions**: Use [GitHub Discussions](https://github.com/FairHead/ReisingerIntelliAppV1/discussions) for questions
- **📋 Issues**: Create issues for bugs and feature requests
- **📧 Email**: Contact maintainers for sensitive matters

## 🙏 Recognition

Contributors who make significant contributions will be recognized in:

- **README**: Listed in the acknowledgments section
- **Release Notes**: Mentioned in release announcements
- **Community**: Highlighted in project discussions

---

Thank you for contributing to ReisingerIntelliAppV1! Every contribution, no matter how small, helps make this project better for everyone. 🎉
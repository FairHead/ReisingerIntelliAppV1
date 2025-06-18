# GitHub Copilot Instructions for ReisingerIntelliAppV1

## Project Overview
ReisingerIntelliAppV1 is a .NET MAUI cross-platform mobile application for controlling and monitoring Reisinger Intellidrive devices. The app provides device connectivity, status monitoring, and control operations across Android, iOS, Windows, and macOS platforms.

## Architecture & Patterns
- **Framework**: .NET MAUI (.NET 8.0+)
- **Pattern**: MVVM (Model-View-ViewModel)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **UI**: XAML with CommunityToolkit.Maui
- **Data Storage**: SQLite with secure storage for sensitive data
- **Networking**: HttpClient for REST API communication

## Key Components

### Services Layer
- `IntellidriveApiService`: Main API communication service
- `DeviceService`: Device management and persistence
- `WifiService`: Network connectivity management
- `BuildingStorageService`: Building/floor plan management
- `PdfConversionService`: Document handling

### Models
- `DeviceModel`: Represents Intellidrive devices
- `PlacedDeviceModel`: Devices positioned on floor plans
- `VersionResponseDataModel`: API response data

### ViewModels
- Follow MVVM pattern with CommunityToolkit.Mvvm
- Use `[ObservableProperty]` and `[RelayCommand]` attributes
- Implement proper async/await patterns

## Code Style Guidelines

### Naming Conventions
- **Classes**: PascalCase (e.g., `DeviceService`, `IntellidriveApiService`)
- **Methods**: PascalCase with descriptive names (e.g., `GetDeviceStatusAsync`, `RestartIntellidriveAsync`)
- **Properties**: PascalCase (e.g., `DeviceId`, `IsConnected`)
- **Fields**: camelCase with underscore prefix for private (e.g., `_apiService`, `_httpClient`)
- **Parameters**: camelCase (e.g., `deviceId`, `endpoint`)

### Method Patterns
- Async methods should end with `Async` suffix
- API methods return appropriate types (`Task<string>`, `Task<bool>`, `Task<T>`)
- Use descriptive parameter names that indicate purpose
- Include XML documentation for public methods

### Error Handling
```csharp
try
{
    // API call or operation
    var result = await _httpClient.GetAsync(endpoint);
    return await result.Content.ReadAsStringAsync();
}
catch (Exception ex)
{
    Debug.WriteLine($"❌ Error in {nameof(MethodName)}: {ex.Message}");
    return null; // or appropriate error response
}
```

### API Service Patterns
```csharp
public async Task<string> PerformDeviceActionAsync(DeviceModel device, string action)
{
    var endpoint = $"http://{device.IpAddress}/{action}";
    try
    {
        var response = await _httpClient.PostAsync(endpoint, null);
        return await response.Content.ReadAsStringAsync();
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ Failed to {action} on device {device.DeviceId}: {ex.Message}");
        return $"Error: {ex.Message}";
    }
}
```

## Platform-Specific Considerations

### Android
- Use conditional compilation: `#if ANDROID`
- Handle permissions appropriately
- Consider Android-specific UI guidelines

### iOS
- Use conditional compilation: `#if IOS`
- Follow iOS design guidelines
- Handle app lifecycle properly

### Windows/macOS
- Desktop-specific considerations
- File system access patterns
- Window management

## Common Dependencies
```xml
<PackageReference Include="CommunityToolkit.Maui" Version="12.0.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="9.0.5" />
<PackageReference Include="Microsoft.Maui.Controls" Version="9.0.70" />
<PackageReference Include="Microsoft.Maui.Essentials" Version="9.0.70" />
```

## Copilot Context Helpers

### When working with API services:
- Always use async/await patterns
- Include proper error handling with Debug.WriteLine
- Use HttpClient for network operations
- Follow the existing endpoint patterns

### When working with ViewModels:
- Use CommunityToolkit.Mvvm attributes
- Implement INotifyPropertyChanged through base classes
- Use RelayCommand for user actions
- Include parameter validation

### When working with XAML:
- Use MVVM binding patterns
- Leverage CommunityToolkit.Maui behaviors
- Follow consistent naming for x:Name attributes
- Use platform-specific resources when needed

### When working with Device operations:
- Always pass DeviceModel objects to API methods
- Use IP address from device for endpoint construction
- Include device identification in logging
- Handle connection failures gracefully

## Testing Guidelines
- Write unit tests for service methods
- Mock HttpClient for API testing
- Test platform-specific functionality separately
- Include integration tests for device communication

## Security Considerations
- Never hardcode API keys or sensitive data
- Use SecureStorage for sensitive information
- Validate all user inputs
- Sanitize device communication data

## Performance Best Practices
- Use async/await properly to avoid blocking
- Implement proper disposal patterns
- Cache frequently accessed data
- Optimize image and resource loading

## Documentation Standards
- Include XML documentation for public APIs
- Use descriptive commit messages
- Document platform-specific workarounds
- Maintain architectural decision records

## Common Code Patterns to Follow

### Service Registration (MauiProgram.cs):
```csharp
builder.Services.AddSingleton<IntellidriveApiService>();
builder.Services.AddSingleton<DeviceService>();
builder.Services.AddTransient<SomePageViewModel>();
```

### Dependency Injection:
```csharp
public DeviceService(IntellidriveApiService apiService)
{
    _apiService = apiService;
}
```

### ViewModel Implementation:
```csharp
[ObservableProperty]
private string deviceStatus = "Unknown";

[RelayCommand]
private async Task RefreshDeviceAsync()
{
    // Implementation
}
```

This context should help GitHub Copilot provide more accurate and consistent suggestions that align with the project's architecture and coding standards.
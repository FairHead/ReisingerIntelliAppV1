# Development Environment Setup Guide

This comprehensive guide will help you set up a complete development environment for the ReisingerIntelliAppV1 project, optimized for .NET MAUI development with GitHub Copilot integration.

## 📋 Table of Contents

- [Prerequisites](#prerequisites)
- [Core Development Tools](#core-development-tools)
- [MAUI Workload Installation](#maui-workload-installation)
- [IDE Configuration](#ide-configuration)
- [GitHub Copilot Setup](#github-copilot-setup)
- [Project Setup](#project-setup)
- [Platform-Specific Setup](#platform-specific-setup)
- [Verification](#verification)
- [Troubleshooting](#troubleshooting)

## 🔧 Prerequisites

### System Requirements

#### Windows Development
- **Windows 11** or **Windows 10** version 1903 or higher
- **16 GB RAM** minimum (32 GB recommended)
- **500 GB** available disk space
- **Intel/AMD x64 or ARM64** processor

#### macOS Development
- **macOS 12.0 (Monterey)** or later
- **16 GB RAM** minimum (32 GB recommended)
- **200 GB** available disk space
- **Apple Silicon (M1/M2)** or **Intel x64** processor

#### Linux Development
- **Ubuntu 20.04 LTS** or later
- **16 GB RAM** minimum
- **100 GB** available disk space
- **x64** processor

### Required Accounts
- **GitHub Account**: For repository access and Copilot subscription
- **Microsoft Account**: For Visual Studio and Azure services (optional)
- **Apple Developer Account**: For iOS development (macOS only)

## 🛠️ Core Development Tools

### 1. Git Installation

#### Windows
```powershell
# Using Winget
winget install --id Git.Git -e --source winget

# Or download from https://git-scm.com/download/win
```

#### macOS
```bash
# Using Homebrew (recommended)
brew install git

# Or using Xcode Command Line Tools
xcode-select --install
```

#### Linux (Ubuntu)
```bash
sudo apt update
sudo apt install git
```

### 2. .NET SDK Installation

#### All Platforms
```bash
# Download and install .NET 8.0 SDK from:
# https://dotnet.microsoft.com/download/dotnet/8.0

# Verify installation
dotnet --version
dotnet --list-sdks
```

#### Package Manager Installation

**Windows (Winget)**:
```powershell
winget install Microsoft.DotNet.SDK.8
```

**macOS (Homebrew)**:
```bash
brew install --cask dotnet-sdk
```

**Linux (Ubuntu)**:
```bash
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

## 📱 MAUI Workload Installation

### Install MAUI Workload

```bash
# Install MAUI workload
dotnet workload install maui

# Verify installation
dotnet workload list
```

### Platform-Specific Workloads

#### Android Development
```bash
# Install Android workload (included in MAUI but can be installed separately)
dotnet workload install android

# Install specific Android SDK tools (if needed)
dotnet workload install microsoft-android-sdk-full
```

#### iOS Development (macOS only)
```bash
# Install iOS workload
dotnet workload install ios

# Install Xcode from App Store
# https://apps.apple.com/us/app/xcode/id497799835
```

#### Windows Development
```bash
# Install Windows workload (Windows only)
dotnet workload install windows
```

## 🔨 IDE Configuration

### Option 1: Visual Studio 2022 (Recommended for Windows)

#### Installation
1. Download **Visual Studio 2022** from https://visualstudio.microsoft.com/downloads/
2. Choose **Community** (free) or **Professional/Enterprise**
3. During installation, select:
   - **.NET Multi-platform App UI development** workload
   - **Mobile development with .NET** workload
   - **ASP.NET and web development** (for web API development)

#### Required Components
- .NET 8.0 Runtime
- Android SDK setup
- iOS simulator (macOS only)
- Windows App SDK

#### GitHub Copilot Extension
1. Open **Extensions** → **Manage Extensions**
2. Search for **"GitHub Copilot"**
3. Install **GitHub Copilot** and **GitHub Copilot Chat**
4. Restart Visual Studio
5. Sign in to GitHub when prompted

### Option 2: Visual Studio Code (Cross-Platform)

#### Installation
```bash
# Windows (Winget)
winget install Microsoft.VisualStudioCode

# macOS (Homebrew)
brew install --cask visual-studio-code

# Linux (Ubuntu)
sudo snap install code --classic
```

#### Required Extensions
Install these extensions for optimal MAUI development:

```json
{
  "recommendations": [
    // Essential .NET extensions
    "ms-dotnettools.csharp",
    "ms-dotnettools.csdevkit",
    "ms-dotnettools.vscode-dotnet-runtime",
    
    // GitHub Copilot
    "github.copilot",
    "github.copilot-chat",
    
    // MAUI development
    "ms-dotnettools.dotnet-maui",
    "redhat.vscode-xml",
    
    // Code quality
    "editorconfig.editorconfig",
    "streetsidesoftware.code-spell-checker"
  ]
}
```

#### Extension Installation Script
```bash
# Install all recommended extensions
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.csdevkit
code --install-extension github.copilot
code --install-extension github.copilot-chat
code --install-extension ms-dotnettools.dotnet-maui
code --install-extension redhat.vscode-xml
code --install-extension editorconfig.editorconfig
```

## 🤖 GitHub Copilot Setup

### 1. Subscription Setup
1. Go to https://github.com/features/copilot
2. Subscribe to **GitHub Copilot Individual** or ensure you have **Copilot for Business**
3. Verify subscription in GitHub settings

### 2. IDE Configuration

#### Visual Studio 2022
1. **Tools** → **Options** → **GitHub Copilot**
2. Enable **"Enable GitHub Copilot"**
3. Configure suggestion settings:
   - **Enable inline suggestions**: ✅
   - **Enable Copilot Chat**: ✅
   - **Show suggestions for comments**: ✅

#### Visual Studio Code
1. Install Copilot extensions (see above)
2. Sign in to GitHub: **Ctrl+Shift+P** → **"GitHub Copilot: Sign In"**
3. Configure in **Settings** (Ctrl+,):
   ```json
   {
     "github.copilot.enable": {
       "*": true,
       "yaml": true,
       "plaintext": true,
       "markdown": true,
       "csharp": true,
       "xml": true
     },
     "github.copilot.editor.enableAutoCompletions": true
   }
   ```

### 3. Project-Specific Configuration

Copy the project's VS Code settings:
```bash
# The project includes optimized settings in .vscode/settings.json
# These will be automatically applied when you open the project
```

## 📁 Project Setup

### 1. Clone Repository

```bash
# Clone the repository
git clone https://github.com/FairHead/ReisingerIntelliAppV1.git
cd ReisingerIntelliAppV1

# Set up upstream (if you forked)
git remote add upstream https://github.com/FairHead/ReisingerIntelliAppV1.git
```

### 2. Restore Dependencies

```bash
# Restore NuGet packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build
```

### 3. IDE Project Setup

#### Visual Studio 2022
1. Open **ReisingerIntelliAppV1.sln**
2. Set startup project to **ReisingerIntelliAppV1**
3. Select target platform in toolbar

#### Visual Studio Code
1. Open project folder: **File** → **Open Folder**
2. VS Code will automatically detect the .NET project
3. Install recommended extensions when prompted

## 📱 Platform-Specific Setup

### Android Development

#### Android SDK Setup
```bash
# Check Android SDK installation
dotnet workload list

# If needed, install Android SDK
dotnet workload repair
```

#### Android Emulator (Recommended)
1. **Visual Studio**: Tools → Android → Android Device Manager
2. **VS Code/Command Line**: Use Android Studio's AVD Manager
3. Create emulator with:
   - **API Level 31+** (Android 12+)
   - **4 GB RAM**
   - **Hardware acceleration** enabled

#### Physical Device Setup
1. Enable **Developer Options** on Android device
2. Enable **USB Debugging**
3. Connect device and authorize computer

### iOS Development (macOS only)

#### Xcode Setup
1. Install **Xcode** from App Store
2. Launch Xcode and accept license agreements
3. Install additional components when prompted

#### iOS Simulator
```bash
# List available simulators
xcrun simctl list devices

# Open Simulator app
open -a Simulator
```

#### Physical Device Setup
1. Connect iPhone/iPad to Mac
2. Trust the computer on device
3. Enable **Developer Mode** (iOS 16+)

### Windows Development

#### Windows App SDK
```bash
# Verify Windows SDK installation
dotnet workload list

# Install if missing
dotnet workload install windows
```

#### Windows Developer Mode
1. **Settings** → **Update & Security** → **For developers**
2. Enable **Developer Mode**

### macOS Development

#### Xcode Command Line Tools
```bash
# Install command line tools
xcode-select --install

# Verify installation
xcode-select -p
```

## ✅ Verification

### 1. Development Environment Check

```bash
# Check .NET installation
dotnet --version
dotnet --info

# Check MAUI workload
dotnet workload list

# Check project build
dotnet build

# Run tests
dotnet test
```

### 2. Platform Build Tests

```bash
# Test Android build
dotnet build -f net8.0-android

# Test iOS build (macOS only)
dotnet build -f net8.0-ios

# Test Windows build (Windows only)
dotnet build -f net8.0-windows10.0.19041.0
```

### 3. Copilot Verification

#### Visual Studio 2022
1. Create new file with `.cs` extension
2. Type a comment like `// Create a method to calculate sum`
3. Verify Copilot suggestions appear

#### Visual Studio Code
1. Open any `.cs` file
2. Check Copilot icon in status bar shows as enabled
3. Type `Ctrl+Shift+P` → **"GitHub Copilot: Open Chat"**

### 4. Complete Environment Test

```bash
# Run environment verification script
# (Create this script to test all components)

echo "Testing .NET MAUI Development Environment..."

# Test .NET
dotnet --version || echo "❌ .NET SDK not found"

# Test MAUI workload
dotnet workload list | grep maui || echo "❌ MAUI workload not installed"

# Test project compilation
dotnet build || echo "❌ Project build failed"

echo "✅ Environment verification complete"
```

## 🔧 Troubleshooting

### Common Issues and Solutions

#### Issue: MAUI Workload Installation Fails
```bash
# Solution: Clean and reinstall
dotnet workload clean
dotnet workload update
dotnet workload install maui
```

#### Issue: Android Build Failures
```bash
# Solution: Accept Android licenses
dotnet workload install android
# Follow prompts to accept licenses
```

#### Issue: iOS Build Failures (macOS)
```bash
# Solution: Update Xcode and command line tools
xcode-select --install
sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer
```

#### Issue: Copilot Not Working
1. **Check Subscription**: Verify active Copilot subscription
2. **Restart IDE**: Close and reopen development environment
3. **Re-authenticate**: Sign out and sign back into GitHub
4. **Check Network**: Ensure internet connectivity

#### Issue: Slow Build Times
1. **Exclude Folders**: Add `bin/`, `obj/` to antivirus exclusions
2. **SSD Storage**: Use SSD for project files
3. **RAM**: Ensure adequate RAM (16GB+ recommended)
4. **Parallel Builds**: Configure multi-core building

### Performance Optimization

#### Build Performance
```xml
<!-- Add to Directory.Build.props for faster builds -->
<PropertyGroup>
  <DesignTimeBuild>true</DesignTimeBuild>
  <BuildInParallel>true</BuildInParallel>
  <MaxCpuCount>0</MaxCpuCount>
</PropertyGroup>
```

#### IDE Performance
```json
// VS Code settings for better performance
{
  "files.watcherExclude": {
    "**/bin/**": true,
    "**/obj/**": true,
    "**/packages/**": true
  },
  "search.exclude": {
    "**/bin": true,
    "**/obj": true
  }
}
```

## 📚 Additional Resources

### Documentation
- [.NET MAUI Documentation](https://docs.microsoft.com/en-us/dotnet/maui/)
- [GitHub Copilot Documentation](https://docs.github.com/en/copilot)
- [Visual Studio MAUI Development](https://docs.microsoft.com/en-us/dotnet/maui/get-started/installation)

### Tools and Utilities
- [MAUI Check Tool](https://github.com/Redth/dotnet-maui-check): Verify MAUI installation
- [Android SDK Manager](https://developer.android.com/studio/command-line/sdkmanager): Manage Android SDKs
- [iOS Simulator](https://developer.apple.com/documentation/xcode/running-your-app-in-the-simulator): iOS testing

### Community Resources
- [.NET MAUI Community Toolkit](https://github.com/CommunityToolkit/Maui)
- [MAUI Samples](https://github.com/dotnet/maui-samples)
- [Stack Overflow MAUI Tag](https://stackoverflow.com/questions/tagged/.net-maui)

## 🎯 Next Steps

After completing the setup:

1. **📖 Read Documentation**: Review [project README](../README.md) and [contributing guide](../CONTRIBUTING.md)
2. **🤖 Learn Copilot**: Read [Copilot guidelines](copilot-guidelines.md)
3. **🏗️ Build Project**: Compile and run on your preferred platform
4. **🧪 Run Tests**: Execute test suite to verify everything works
5. **🔧 Create Branch**: Start working on your first contribution

---

**Congratulations!** 🎉 You now have a complete development environment for ReisingerIntelliAppV1 with GitHub Copilot integration. Happy coding!
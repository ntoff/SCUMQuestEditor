# SCUMQuestEditor Build Instructions

## Prerequisites

Install the following before building:

1. **.NET 10 SDK**
   - Download from: https://dotnet.microsoft.com/en-us/download/dotnet/10.0
   - Choose the **Windows x64 Installer** (SDK, not just Runtime)
   - The installer will also register the `dotnet` CLI tool automatically
   - After installation, verify with: `dotnet --version` (should report 10.x.x)

No other manual dependencies are required. All NuGet packages (MaterialDesignThemes, MaterialDesignThemes.MahApps) are restored automatically during build.

## Build Steps

1. Open PowerShell or Command Prompt
2. Navigate to the project root:
   ```
   cd C:\path\to\source\SCUMQuestEditor
   ```
3. Run:
   ```
   .\build.bat
   ```

The output executable will be at:
```
SCUMQuestEditor\bin\Debug\net10.0-windows\SCUMQuestEditor.exe
```

## Troubleshooting

- **"dotnet is not recognized"** -- The .NET 10 SDK was not installed or the terminal needs to be restarted after installation. Close and reopen the terminal, then try again.
- **`dotnet --version` reports less than 10.0** -- .NET 10 SDK is not installed. Install it from the link above.
- **NuGet package restore fails** -- Ensure you have an internet connection. Packages are downloaded from nuget.org automatically. If behind a corporate proxy, configure your NuGet source accordingly.

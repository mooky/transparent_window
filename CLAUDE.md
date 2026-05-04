# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

A Windows desktop overlay/transparent window application with two implementations:

- **`transparent_window_winforms/`** — Windows Forms implementation targeting .NET 9.0 (`net9.0-windows`)
- **`transparent_window_wpf/`** — WPF implementation targeting .NET 9.0 (`net9.0-windows`)

Shared assets (images, etc.) live in `context/` at the repo root.

## Commands

```bash
# Build all projects (solution)
dotnet build transparent_window.sln

# Build individual projects
dotnet build transparent_window_winforms/transparent_window.csproj
dotnet build transparent_window_wpf/transparent_window_wpf.csproj

# Run
dotnet run --project transparent_window_winforms/transparent_window.csproj
dotnet run --project transparent_window_wpf/transparent_window_wpf.csproj

# Build release
dotnet build -c Release transparent_window.sln

# Publish
dotnet publish -c Release transparent_window_winforms/transparent_window.csproj
dotnet publish -c Release transparent_window_wpf/transparent_window_wpf.csproj
```

No test project exists currently.

## Architecture

### WinForms (`transparent_window_winforms/`)

Standard partial class pattern:

- **`Program.cs`** — Entry point. Calls `ApplicationConfiguration.Initialize()` then runs `MainForm` on an STA thread.
- **`MainForm.cs`** — Logic for the main window; extend this for all runtime behavior.
- **`MainForm.Designer.cs`** — Auto-generated designer code. Do not edit manually; modify via Visual Studio designer or `InitializeComponent()` only.

### WPF (`transparent_window_wpf/`)

Standard XAML/code-behind pattern:

- **`App.xaml` / `App.xaml.cs`** — Application entry point and startup.
- **`MainWindow.xaml` / `MainWindow.xaml.cs`** — Main window definition and logic.

Implicit usings are enabled in both projects.

## Git

Never include `Co-Authored-By: Claude` or any Claude attribution in commit messages.

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

A Windows Forms desktop application targeting .NET 9.0 (`net9.0-windows`). The project name `transparent_window` suggests its purpose is implementing a transparent/overlay window using Windows Forms.

## Commands

```bash
# Build
dotnet build

# Run
dotnet run

# Build release
dotnet build -c Release

# Publish
dotnet publish -c Release
```

No test project exists currently.

## Architecture

Single-form Windows Forms application with the standard partial class pattern:

- **`Program.cs`** — Entry point. Calls `ApplicationConfiguration.Initialize()` then runs `Form1` on an STA thread.
- **`Form1.cs`** — Logic for the main window; extend this for all runtime behavior.
- **`Form1.Designer.cs`** — Auto-generated designer code. Do not edit manually; modify via Visual Studio designer or `InitializeComponent()` only.

Implicit usings are enabled, so `System.Windows.Forms`, `System.Drawing`, and other common namespaces are available without explicit `using` statements.

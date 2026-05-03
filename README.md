# transparent_window

A Windows desktop overlay that displays a monkey image as a non-rectangular, always-on-top transparent window.

## Behaviour

### Appearance

The window has no title bar, borders, or taskbar entry. Only the monkey image is visible — the white background is made fully transparent using a color key, so the window appears to float on screen.

### Always on top

The window sits above all other applications at all times and cannot be hidden by switching focus to another program.

### Mouse handling

Mouse events are handled per-pixel based on the image content:

| Area | Behaviour |
|---|---|
| Opaque pixel (monkey body) | Single click gives the window focus; drag moves the window |
| White/transparent pixel | Click passes through to the application beneath |

The window never steals keyboard focus. When you click on the monkey, the previously focused application retains its focus.

### Keyboard handling

Because the window never takes keyboard focus, all keyboard input continues to go to whatever application was active before. The overlay does not intercept or consume any keystrokes.

### Exiting

Double-click anywhere on the monkey image to exit the application.

### Window state

Minimize, maximize, and restore are disabled. The window always remains visible at its normal size.

## Building and running

```bash
# Build
dotnet build

# Run
dotnet run

# Release build
dotnet build -c Release
```

Requires .NET 9.0 and Windows.

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

## Implementation notes

### Transparent edge handling

The two implementations use different mechanisms to make the image background invisible, which affects edge quality.

**WinForms** uses Win32 color-key transparency (`TransparencyKey = Color.White`). The OS makes every pixel that matches the key color fully transparent at the window level. The opacity map (used for hit testing) checks only the RGB values of the scaled bitmap — pixels with `r ≥ 250 && g ≥ 250 && b ≥ 250` are treated as transparent. Because anti-aliased edge pixels blend the image color toward white during bicubic scaling, some fringe pixels may be only partially white and survive the color key, producing a faint bright halo at the edges.

**WPF** uses DWM per-pixel alpha compositing (`AllowsTransparency="True"`, `Background="Transparent"`). Each pixel carries an independent alpha value, so no color key is needed. During image loading the opacity map checks both the alpha channel (`a > 32`) and the near-white test. Any pixel that fails either check has all four channels zeroed out in the `WriteableBitmap` pixel data before the image is displayed. This removes anti-aliased fringe pixels at the source rather than relying on a color match at render time, giving visibly cleaner edges.

In short: WinForms transparency is binary (white vs. not-white), WPF transparency is per-pixel and the fringe is actively erased.

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

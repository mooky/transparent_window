Implement the following feature in the transparent_window WinForms application: $ARGUMENTS

Process:
1. Read the relevant existing source files before writing any code.
2. Identify which form(s) or control(s) need to change, or whether a new one is needed.
3. For window-transparency or layered-window features, use the Win32 P/Invoke pattern:
   - `SetWindowLong` / `GetWindowLong` with `GWL_EXSTYLE`
   - `SetLayeredWindowAttributes` for alpha/color-key transparency
   - Declare interop in a static `NativeMethods` class inside the namespace.
4. Keep all designer-generated code in `*.Designer.cs`; put all logic in the non-designer partial.
5. After implementing, run `dotnet build` and fix any errors before finishing.
6. Summarize what changed and why.

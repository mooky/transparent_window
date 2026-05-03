Add a Win32 P/Invoke declaration for: $ARGUMENTS

Steps:
1. Check if a `NativeMethods.cs` file already exists; if not, create it in the project root with:
   ```csharp
   namespace transparent_window;
   internal static class NativeMethods { }
   ```
2. Look up the correct signature for the requested Win32 API(s) (use knowledge of Windows SDK).
3. Add the `[DllImport]` declaration(s) with correct `CharSet`, `SetLastError`, and `CallingConvention` attributes.
4. Add any required constants (`const int`) or structs (`[StructLayout(LayoutKind.Sequential)]`) needed by the API.
5. Show a usage example in a comment or as a code snippet.
6. Run `dotnet build` to confirm the declarations compile.

Common APIs for this project: `SetWindowLong`, `GetWindowLong`, `SetLayeredWindowAttributes`, `SetWindowPos`, `RegisterHotKey`, `SendMessage`, `DefWindowProc`.

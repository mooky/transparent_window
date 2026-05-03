Create a new Windows Form named $ARGUMENTS.

Steps:
1. Create `$ARGUMENTS.cs` with a partial class inheriting from `Form`, inside the `transparent_window` namespace.
2. Create `$ARGUMENTS.Designer.cs` with the standard designer boilerplate: `InitializeComponent()`, `Dispose(bool)`, and the `#region Windows Form Designer generated code` block.
3. Register the form subtype in the project by adding a `<Compile>` entry if needed (usually automatic with SDK-style projects).
4. Set sensible defaults in `InitializeComponent()`: `ClientSize`, `Text`, and `AutoScaleMode`.
5. Report the two files created and show the constructor signature.

Do not add the form as the startup form unless explicitly asked.

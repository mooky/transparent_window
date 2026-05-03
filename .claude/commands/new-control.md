Create a new custom WinForms control named $ARGUMENTS inside the `transparent_window` namespace.

Steps:
1. Determine the best base class from context (default to `UserControl`; use `Control` for owner-drawn, `Panel` for containers).
2. Create `$ARGUMENTS.cs` as a partial class.
3. Create `$ARGUMENTS.Designer.cs` with the standard designer boilerplate.
4. In the control class, add:
   - A constructor calling `InitializeComponent()`
   - `protected override void OnPaint(PaintEventArgs e)` stub if the base is `Control` (owner-drawn)
   - Any public properties implied by the name or arguments
5. Show example usage of how to add the control to a form.

Build the project and report the results.

Steps:
1. Run `dotnet build` and capture output.
2. If build succeeds: report "Build succeeded" with warning count (if any warnings, list them).
3. If build fails: list each error with file, line, and error code. For each error, suggest the most likely fix.
4. After a clean build, also run `dotnet build -c Release` to confirm release configuration compiles.
5. Do not attempt to fix errors unless the user asks — just report them clearly.

# 0.3.0
- Reorganize build and restore tasks to "Fix up"
- Use DotNet tool to evaluate project properties. This is invoked through a build task using Exec

# 0.2.0
- Added: Generate assembly references from DLL references

# 0.1.0
- Initial release
- Added: Initialize project with build props
- Added: Build projects inside Assets folder with .NET CLI
- Added: Generate asmdef from csproj. Does not support explicit DLL references

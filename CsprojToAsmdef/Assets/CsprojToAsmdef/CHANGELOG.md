# 0.4.0

# 0.3.6
- Get version during build from package.json

# 0.3.5
- Drop GitVersion

# 0.3.4
- Update dependencies
- Update SDK version
- Use correct framework version for GitVersion

# 0.3.3
- Run NuGet publish after OpenUPM publish

# 0.3.2
- Run NuGet publish as separate job on push to main

# 0.3.1
- Try to fix NuGet publish job

# 0.3.0
- Reorganize build and restore tasks to "Fix up"
- Use DotNet tool to evaluate project properties. This is invoked through a build task using Exec
- Downgrade Unity min version to 2019.2

# 0.2.0
- Added: Generate assembly references from DLL references

# 0.1.0
- Initial release
- Added: Initialize project with build props
- Added: Build projects inside Assets folder with .NET CLI
- Added: Generate asmdef from csproj. Does not support explicit DLL references

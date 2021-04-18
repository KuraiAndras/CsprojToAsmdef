# 0.6.0
- Bumping version because of project structure change, otherwise no changes

# 0.5.0
- Automatically set Unity version when the editor loads, and when creating the props file

# 0.4.1
- Only copy DLLs to NuGet folder
- Apply analyzer fixes for Unity project

# 0.4.0
- Set UnityProjectPath property in Directory.Build.props
- Generate asmdef files with explicit DLL references
- Referenced DLLs are copied to NuGet folder under Assets
- Don't import version defines from csproj, actual support will come later
- Support project references inside the Unity Assets folder

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

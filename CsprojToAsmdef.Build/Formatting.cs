using Nuke.Common;
using Nuke.Common.Tooling;

partial class Build
{
    [PackageExecutable("dotnet-format", "dotnet-format.dll")]
    readonly Tool DotNetFormat = default!;

    Target CheckFormatting => _ => _
        .Executes(() => DotNetFormat("--check --verbosity diagnostic"));

    Target RunFormat => _ => _
        .Executes(() => DotNetFormat("--verbosity diagnostic"));
}

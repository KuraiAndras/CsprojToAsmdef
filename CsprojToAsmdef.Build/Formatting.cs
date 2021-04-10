using Nuke.Common;
using Nuke.Common.Tooling;

partial class Build
{
    [PackageExecutable("dotnet-format", "dotnet-format.dll")]
    readonly Tool DotNetFormat = default!;

    Target CheckFormat => _ => _
        .Executes(() => DotNetFormat("--check --verbosity diagnostic"));

    Target Format => _ => _
        .Executes(() => DotNetFormat("--verbosity diagnostic"));
}

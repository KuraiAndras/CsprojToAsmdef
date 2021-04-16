using Nuke.Common;

partial class Build
{
    Target RunCi => _ => _
        .DependsOn(CheckFormatting)
        .DependsOn(InstallCli)
        .Executes(() =>
        {
        });
}
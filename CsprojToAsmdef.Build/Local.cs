using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using System.IO;
using System.Linq;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    string CliName { get; set; } = default!;

    Target UnInstallCli => _ => _
        .DependentFor(InstallCli)
        .Executes(() =>
        {
            var installedTools = DotNet("tool list -g");

            if (!installedTools.Any(l => l.Text.Contains("asmdef-tool"))) return;

            DotNetToolUninstall(s => s
                .EnableGlobal()
                .SetPackageName(CliName));
        });

    Target InstallCli => _ => _
        .DependsOn(Pack)
        .Executes(() =>
            DotNetToolInstall(s => s
                .EnableGlobal()
                .AddSources(Path.Combine(CliProject.Directory, "bin", Configuration))
                .SetPackageName(CliName)));
}

using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using System.IO;
using Nuke.Common.Tooling;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    string CliName => Path.GetFileNameWithoutExtension(CliProject.Path);

    Target UnInstallCli => _ => _
        .DependentFor(InstallCli)
        .Executes(() =>
            DotNetToolUninstall(s => s
                .SetGlobal(true)
                .SetPackageName(CliName)));

    Target InstallCli => _ => _
        .DependsOn(Pack)
        .Executes(InstallCliInternal);

    Target InstallCliFirstTime => _ => _
        .DependsOn(Pack)
        .Executes(InstallCliInternal);

    IReadOnlyCollection<Output> InstallCliInternal() =>
        DotNetToolInstall(s => s.SetGlobal(true)
            .AddSources(Path.Combine(CliProject.Directory, "bin", Configuration))
            .SetPackageName(CliName));
}

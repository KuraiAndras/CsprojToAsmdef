using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using System;
using System.Linq;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
partial class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Should be true for continuous integration builds")] readonly bool CiBuild;

    [Solution] readonly Solution Solution = default!;

    static string CurrentVersion
    {
        get
        {
            var versionText = Git("describe --tags --always").First().Text;

            return Version.TryParse(versionText, out var version)
                ? version.ToString()
                : "0.1.0";
        }
    }

    Project CliProject => Solution.AllProjects.Single(p => p.Name == "CsprojToAsmdef.Cli");

    Target Restore => _ => _
        .Executes(() =>
            DotNetRestore(s => s
                .SetProjectFile(CliProject)));

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
            DotNetBuild(s => s
                .SetProjectFile(CliProject)
                .SetConfiguration(Configuration)
                .SetVersion(CurrentVersion)
                .SetContinuousIntegrationBuild(CiBuild)
                .EnableNoRestore()));
}

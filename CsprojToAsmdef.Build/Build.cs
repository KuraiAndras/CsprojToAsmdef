using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[ShutdownDotNetAfterServerBuild]
partial class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Should be true for continuous integration builds")]
    readonly bool CiBuild;

    [Solution] readonly Solution Solution = default!;

    string PackageDirectory => Path.Combine(Solution.Directory, "CsprojToAsmdef", "Assets", "CsprojToAsmdef");

    string CurrentVersion
    {
        get
        {
            var packageDirectory = PackageDirectory;

            var packagePath = Path.Combine(packageDirectory, "package.json");

            if (!File.Exists(packagePath)) throw new InvalidOperationException($"package.json does not exist at path: {packagePath}");

            var jsonContent = File.ReadAllText(packagePath);
            var package = JsonSerializer.Deserialize<PackageJson>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (package?.Version is null) throw new InvalidOperationException($"Cloud not deserialize package.json:\n{jsonContent}");

            return package.Version;
        }
    }

    Project CliProject => Solution.AllProjects.Single(p => p.Name == "CsprojToAsmdef.Cli");

    Target Restore => _ => _
        .Executes(() =>
            DotNetRestore(s => s
                .SetProjectFile(CliProject)));

    Target BuildCli => _ => _
        .DependsOn(Restore)
        .Executes(() =>
            DotNetBuild(s => s
                .SetProjectFile(CliProject)
                .SetConfiguration(Configuration)
                .SetVersion(CurrentVersion)
                .SetContinuousIntegrationBuild(CiBuild)
                .EnableNoIncremental()
                .EnableNoRestore()));

    Target BuildAll => _ => _
        .DependsOn(InstallCli)
        .Executes(() =>
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(CurrentVersion)
                .SetContinuousIntegrationBuild(CiBuild)
                .EnableNoIncremental()
                .EnableNoRestore()));

    public static int Main() => Execute<Build>(x => x.BuildAll);
}

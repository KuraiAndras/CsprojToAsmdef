using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;

[ShutdownDotNetAfterServerBuild]
partial class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Should be true for continuous integration builds")]
    readonly bool CiBuild;

    [Solution] readonly Solution Solution = default!;

    string PackageDirectory { get; set; } = default!;

    string CurrentVersion { get; set; } = default!;

    Project CliProject { get; set; } = default!;

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

    Target Test => _ => _
        .DependsOn(BuildCli)
        .Executes(() =>
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore()));

    public static int Main() => Execute<Build>(x => x.BuildAll);

    protected override void OnBuildInitialized()
    {
        bool GetIsNewestVersion()
        {
            var currentVersion = new Version(CurrentVersion);

            Git("fetch --tags");

            var maxPublishedVersion = Git("tag")
                .Select(o => new Version(o.Text))
                .OrderBy(v => v)
                .LastOrDefault();

            return currentVersion.CompareTo(maxPublishedVersion) > 0;
        }

        string GetCurrentVersion()
        {
            var packageDirectory = PackageDirectory;

            var packagePath = Path.Combine(packageDirectory, "package.json");

            if (!File.Exists(packagePath)) throw new InvalidOperationException($"package.json does not exist at path: {packagePath}");

            var jsonContent = File.ReadAllText(packagePath);
            var package = JsonSerializer.Deserialize<PackageJson>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (package?.Version is null) throw new InvalidOperationException($"Cloud not deserialize package.json:{Environment.NewLine}{jsonContent}");

            return package.Version;
        }

        PackageDirectory = Solution.AllProjects.Single(p => p.Name == "CsprojToAsmdef").Directory;
        CurrentVersion = GetCurrentVersion();
        IsNewestVersion = GetIsNewestVersion();
        CliProject = Solution.AllProjects.Single(p => p.Name == "CsprojToAsmdef.Cli");
        CliName = Path.GetFileNameWithoutExtension(CliProject.Path);

        Console.WriteLine($"Current version {CurrentVersion} is the newest: {IsNewestVersion}");

        base.OnBuildInitialized();
    }
}

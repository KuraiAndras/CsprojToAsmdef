﻿using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static System.IO.Directory;

partial class Build
{
    [SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "Will only ever publish on Nuget.org")]
    const string NugetApiUrl = "https://api.nuget.org/v3/index.json";

    [Parameter("NuGet API key to use for authentication with the NuGet server")]
    readonly string? NugetApiKey;

    [PathExecutable] readonly Tool Gh = default!;

    Target Pack => _ => _
        .DependsOn(BuildCli)
        .Executes(() =>
            DotNetPack(s => s
                .SetProject(CliProject)
                .SetConfiguration(Configuration)
                .SetVersion(CurrentVersion)
                .EnableNoBuild()
                .EnableNoRestore()));

    Target PushTag => _ => _
        .Executes(() =>
        {
            var version = CurrentVersion;

            Git($"tag {version}");
            Git($"push origin {version}");
        });

    Target CreateGithubRelease => _ => _
        .DependsOn(PushTag)
        .DependentFor(PushNuGet)
        .Executes(() =>
        {
            var version = CurrentVersion;

            var notes = File.ReadAllLines(Path.Combine(PackageDirectory, "CHANGELOG.md"))
                .Skip(1)
                .TakeUntil(string.IsNullOrWhiteSpace)
                .Aggregate(new StringBuilder(), (sb, l) => sb.AppendLine(l))
                .ToString();

            var files = Enumerable.Empty<string>()
                .Concat(GetAllNupkg())
                .Concat(EnumerateFiles(Solution.Directory, "*.snupkg", SearchOption.AllDirectories))
                .Aggregate(new StringBuilder(), (sb, f) => sb.Append('\"').Append(f).Append("\" "))
                .ToString();

            Gh($"release create {version} -t {version} -n \"{notes}\" {files}");
        });

    Target PushNuGet => _ => _
        .DependsOn(Pack)
        .Requires(() => NugetApiKey)
        .Executes(() =>
            GetAllNupkg()
                .ForEach(n =>
                    DotNetNuGetPush(s => s
                        .SetTargetPath(n)
                        .SetApiKey(NugetApiKey)
                        .SetSource(NugetApiUrl))));

    IEnumerable<string> GetAllNupkg() => EnumerateFiles(Solution.Directory, "*.nupkg", SearchOption.AllDirectories);
}

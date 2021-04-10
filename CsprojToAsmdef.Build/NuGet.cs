﻿using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static System.IO.Directory;

partial class Build
{
    const string NugetApiUrl = "https://api.nuget.org/v3/index.json";
    [Parameter("NuGet API key to use for authentication with the NuGet server")] readonly string? NugetApiKey;

    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
            DotNetPack(s => s
                .SetProject(CliProject)
                .SetConfiguration(Configuration)
                .SetVersion(CurrentVersion)
                .EnableNoBuild()
                .EnableNoRestore()));

    Target PushNuGet => _ => _
        .DependsOn(Pack)
        .Requires(() => NugetApiKey)
        .Executes(() =>
            EnumerateFiles(Solution.Directory, "*.nupkg", SearchOption.AllDirectories)
                .SelectMany(n =>
                    DotNetNuGetPush(s => s
                        .SetTargetPath(n)
                        .SetApiKey(NugetApiKey)
                        .SetSource(NugetApiUrl)))
                .ToImmutableArray());
}
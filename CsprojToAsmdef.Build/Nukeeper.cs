using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.Tooling;

partial class Build
{
    [GitRepository] readonly GitRepository Repository = default!;

    [Parameter("Token used when managing the repository on GitHub")]
    readonly string GithubToken = default!;

    [PackageExecutable(
        "nukeeper",
        "nukeeper.dll",
        Framework = "netcoreapp3.1")]
    readonly Tool Nukeeper = default!;

    Target UpdatePackages => _ => _
        .Requires(() => GithubToken)
        .Executes(() => Nukeeper(
            $"repo {Repository.HttpsUrl} {GithubToken} -a 0 --targetBranch develop --maxpackageupdates 100 --consolidate"));
}

using CliWrap;
using MoreLinq;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CsprojToAsmdef.Cli.Domain.Services.DotNet
{
    public sealed class DotNetTooling : IDotNetTooling
    {
        private const string DotNet = "dotnet";

        public async Task SetMsbuildEnvironmentVariable(CancellationToken cancellationToken = default)
        {
            var output = new StringBuilder();

            await CliWrap.Cli.Wrap(DotNet)
                .WithArguments("--list-sdks")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(output))
                .ExecuteAsync(cancellationToken);

            var sdkPaths = Regex.Matches(output.ToString(), "([0-9]+.[0-9]+.[0-9]+) \\[(.*)\\]")
                .Select(m =>
                (
                    path: Path.Combine(m.Groups[2].Value, m.Groups[1].Value, "MSBuild.dll"),
                    version: new Version(m.Groups[1].Value)
                ))
                .ToImmutableArray();

            var sdkPath = sdkPaths
                .MaxBy(p => p.version)
                .First()
                .path;

            Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", sdkPath);
        }
    }
}

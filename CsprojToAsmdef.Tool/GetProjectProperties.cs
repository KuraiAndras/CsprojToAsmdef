using System;
using System.Collections.Immutable;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using CsprojToAsmdef.Tool.Extensions;
using Microsoft.Build.Evaluation;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using MoreLinq;

namespace CsprojToAsmdef.Tool
{
    [Command]
    public sealed class GetProjectProperties : ICommand
    {
        private readonly IDotNetTooling _dotNet;

        public GetProjectProperties(IDotNetTooling dotNet) => _dotNet = dotNet;

        [CommandParameter(0)] public string ProjectPath { get; init; } = string.Empty;

        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (!File.Exists(ProjectPath)) throw new CommandException("Project file does not exist");

            await _dotNet.SetMsbuildEnvironmentVariable();

            var project = new Project(ProjectPath);

            await project
                .AllEvaluatedProperties
                .ForEachAsync(async p => await console.Output.WriteLineAsync($"Name: {p.Name} Evaluated Value: {p.EvaluatedValue}"));
        }

        public interface IDotNetTooling
        {
            Task SetMsbuildEnvironmentVariable(CancellationToken cancellationToken = default);
        }

        public sealed class DotNetTooling : IDotNetTooling
        {
            private const string DotNet = "dotnet";

            public async Task SetMsbuildEnvironmentVariable(CancellationToken cancellationToken = default)
            {
                var output = new StringBuilder();

                await Cli.Wrap(DotNet)
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

                var sdkPath = sdkPaths.MaxBy(p => p.version).First();
                Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", sdkPath.path);
            }
        }
    }
}

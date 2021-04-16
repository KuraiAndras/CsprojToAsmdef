using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using CsprojToAsmdef.Cli.Domain;
using CsprojToAsmdef.Cli.Domain.Services.DotNet;
using Microsoft.Build.Evaluation;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace CsprojToAsmdef.Cli
{
    [Command(nameof(FixUpProject))]
    public sealed class FixUpProject : ICommand
    {
        private readonly IDotNetTooling _dotNet;

        public FixUpProject(IDotNetTooling dotNet) => _dotNet = dotNet;

        [CommandParameter(
            0,
            Name = nameof(ProjectPath),
            Description = "Path to the csproj file. Use full path")]
        public string ProjectPath { get; init; } = string.Empty;

        public async ValueTask ExecuteAsync(IConsole console)
        {
            var stopwatch = Stopwatch.StartNew();

            if (!File.Exists(ProjectPath)) throw new CommandException($"Project file does not exist: {ProjectPath}");

            await _dotNet.SetMsbuildEnvironmentVariable();
            var project = new Project(ProjectPath);
            var asmdef = new Asmdef(project);

            await console.Output.WriteLineAsync($"Started fixing up project: {ProjectPath}");

            await console.Output.WriteLineAsync($"Started copying Dlls for project: {ProjectPath}");
            CopyFilesToNuGetFolder(asmdef);
            await console.Output.WriteLineAsync($"Finished copying Dlls for project: {ProjectPath}");

            await console.Output.WriteLineAsync($"Started creating asmdef file for project: {ProjectPath}");
            await CreateAsmdefFromCsproj(asmdef);
            await console.Output.WriteLineAsync($"Finished creating asmdef file for project: {ProjectPath}");

            await console.Output.WriteLineAsync($"Finished fixing up project for project: {ProjectPath}");

            stopwatch.Stop();
            await console.Output.WriteLineAsync($"Fixing up project took: {stopwatch.Elapsed}");
        }

        private static void CopyFilesToNuGetFolder(Asmdef asmdef)
        {
            var filesToCopy = asmdef.GetFilesToCopy();
            var outputDirectory = asmdef.GetOutputDirectory();
            var nuGetFolder = asmdef.GetNuGetFolder();

            foreach (var filePath in filesToCopy)
            {
                var relativePathFromOutput = Path.GetRelativePath(outputDirectory, filePath);

                var targetFilePath = Path.GetFullPath(Path.Combine(nuGetFolder, relativePathFromOutput));

                var targetDirectory = Path.GetDirectoryName(targetFilePath)!;
                Directory.CreateDirectory(targetDirectory);

                File.Copy(filePath, targetFilePath, true);
            }
        }

        private static async Task CreateAsmdefFromCsproj(Asmdef asmdef)
        {
            var asmdefPath = asmdef.GetFilePath();
            var json = asmdef.CreateJson();

            await File.WriteAllTextAsync(asmdefPath, json);
        }
    }
}

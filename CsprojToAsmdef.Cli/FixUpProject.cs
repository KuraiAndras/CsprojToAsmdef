using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using CsprojToAsmdef.Cli.Domain;
using CsprojToAsmdef.Cli.Domain.Services.DotNet;
using Microsoft.Build.Evaluation;
using MoreLinq;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CsprojToAsmdef.Cli
{
    [Command(nameof(FixUpProject))]
    public sealed class FixUpProject : ICommand
    {
        private readonly IDotNetTooling _dotNet;

        public FixUpProject(IDotNetTooling dotNet) => _dotNet = dotNet;

        [CommandParameter(0)] public string ProjectPath { get; init; } = string.Empty;
        [CommandParameter(1)] public string AssetsFolder { get; init; } = string.Empty;

        public async ValueTask ExecuteAsync(IConsole console)
        {
            var stopwatch = Stopwatch.StartNew();

            if (!File.Exists(ProjectPath)) throw new CommandException($"Project file does not exist: {ProjectPath}");

            await _dotNet.SetMsbuildEnvironmentVariable();
            var project = new Project(ProjectPath);

            await console.Output.WriteLineAsync($"Started fixing up project: {ProjectPath}");

            await console.Output.WriteLineAsync("Starting copying Dlls");
            var referencedDlls = CopyFilesToNuGetFolder(project);
            await console.Output.WriteLineAsync("Finished copying Dlls");

            await console.Output.WriteLineAsync("Starting creating asmdef file");
            await CreateAsmdefFromCsproj(project, referencedDlls);
            await console.Output.WriteLineAsync("Finished creating asmdef file");

            await console.Output.WriteLineAsync("Finished fixing up project");

            stopwatch.Stop();
            await console.Output.WriteLineAsync($"Fixing up project took: {stopwatch.Elapsed}");
        }

        private ImmutableArray<string> CopyFilesToNuGetFolder(Project project)
        {
            var properties = project.AllEvaluatedProperties;

            // There must be a better way for this.
            var outputDirectoryFromProject = properties
                .Where(p => p.Name == "OutputPath")
                .Select(p => p.EvaluatedValue)
                .Distinct()
                .MaxBy(v => v.Length)
                .First();

            var outputDirectory = Path.GetFullPath(Path.Combine(project.DirectoryPath, outputDirectoryFromProject));

            var projectName = Path.GetFileNameWithoutExtension(project.FullPath);

            var nugetFolder = Path.GetFullPath(Path.Combine(AssetsFolder, "NuGet"));

            var filesToCopy = Directory
                .EnumerateFiles(outputDirectory, "*", SearchOption.AllDirectories)
                .Where(f =>
                {
                    //var depsJson = projectName + ".deps.json";
                    var dll = projectName + ".dll";
                    var pdb = projectName + ".pdb";
                    //var roslynCa = project + "dll.RoslynCA.json";

                    var fileName = Path.GetFileName(f);

                    return fileName != dll && fileName != pdb && !f.EndsWith(".deps.json") && !f.EndsWith(".dll.RoslynCA.json");
                })
                .ToImmutableArray();

            foreach (var filePath in filesToCopy)
            {
                var relativePathFromOutput = Path.GetRelativePath(outputDirectory, filePath);

                var targetFilePath = Path.GetFullPath(Path.Combine(nugetFolder, relativePathFromOutput));

                var targetDirectory = Path.GetDirectoryName(targetFilePath)!;
                Directory.CreateDirectory(targetDirectory);

                File.Copy(filePath, targetFilePath, true);
            }

            return filesToCopy;
        }

        private async Task CreateAsmdefFromCsproj(Project project, ImmutableArray<string> copiedFiles)
        {
            var asmdef = new Asmdef(project, copiedFiles);
            var asmdefPath = Path.Combine(Path.GetDirectoryName(ProjectPath)!, asmdef.CreateFileName());
            var json = asmdef.CreateJson();

            await File.WriteAllTextAsync(asmdefPath, json);
        }
    }
}

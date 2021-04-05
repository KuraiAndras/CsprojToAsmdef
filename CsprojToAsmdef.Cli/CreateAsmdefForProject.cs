using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using CsprojToAsmdef.Cli.Domain;
using CsprojToAsmdef.Cli.Domain.Services.DotNet;
using Microsoft.Build.Evaluation;
using System.IO;
using System.Threading.Tasks;

namespace CsprojToAsmdef.Cli
{
    [Command(nameof(CreateAsmdefForProject))]
    public sealed class CreateAsmdefForProject : ICommand
    {
        private readonly IDotNetTooling _dotNet;

        public CreateAsmdefForProject(IDotNetTooling dotNet) => _dotNet = dotNet;

        [CommandParameter(0)] public string ProjectPath { get; init; } = string.Empty;

        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (!File.Exists(ProjectPath)) throw new CommandException("Project file does not exist");

            await _dotNet.SetMsbuildEnvironmentVariable();

            var project = new Project(ProjectPath);

            var asmdef = new Asmdef(project);
            var asmdefPath = Path.Combine(Path.GetDirectoryName(ProjectPath)!, asmdef.CreateFileName());
            var json = asmdef.CreateJson();

            await File.WriteAllTextAsync(asmdefPath, json);
        }
    }
}

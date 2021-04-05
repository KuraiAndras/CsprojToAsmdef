using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using Microsoft.Build.Evaluation;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CsprojToAsmdef.Cli.Domain;
using CsprojToAsmdef.Cli.Domain.Services.DotNet;

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

            var properties = project.AllEvaluatedProperties;

            var name = Path.GetFileNameWithoutExtension(ProjectPath);
            var references = GetReferences(project.AllEvaluatedItems);
            var includePlatforms = GetCollectionProperty(properties, nameof(Asmdef.IncludePlatforms));
            var excludePlatforms = GetCollectionProperty(properties, nameof(Asmdef.ExcludePlatforms));
            var allowUnsafeCode = GetBoolProperty(properties, "AllowUnsafeBlocks", false);
            var overrideReferences = false;
            var precompiledReferences = ImmutableArray<string>.Empty;
            var autoReferenced = GetBoolProperty(properties, nameof(Asmdef.AutoReferenced), true);
            var defineConstraints = GetCollectionProperty(properties, nameof(Asmdef.DefineConstraints));
            var versionDefines = GetCollectionProperty(properties, nameof(Asmdef.VersionDefines));
            var noEngineReferences = GetBoolProperty(properties, nameof(Asmdef.NoEngineReferences), false);

            var asmdef = new Asmdef(
                name,
                references,
                includePlatforms,
                excludePlatforms,
                allowUnsafeCode,
                overrideReferences,
                precompiledReferences,
                autoReferenced,
                defineConstraints,
                versionDefines,
                noEngineReferences);

            var asmdefPath = Path.Combine(Path.GetDirectoryName(ProjectPath)!, name + ".asmdef");

            var json = JsonSerializer
                .Serialize(
                    asmdef,
                    new JsonSerializerOptions(JsonSerializerDefaults.General)
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true,
                    })
                .Replace("  ", "    ");

            await File.WriteAllTextAsync(asmdefPath, json);
        }

        private static ImmutableArray<string> GetCollectionProperty(IEnumerable<ProjectProperty> properties, string propertyName) =>
            properties
                .SingleOrDefault(p => p.Name == propertyName)
                ?.EvaluatedValue
                ?.Split(';')
                .ToImmutableArray()
            ?? ImmutableArray<string>.Empty;

        private static bool GetBoolProperty(IEnumerable<ProjectProperty> properties, string propertyName, bool defaultValue)
        {
            var propertyValueString = properties
                .SingleOrDefault(p => p.Name == propertyName)
                ?.EvaluatedValue;

            return bool.TryParse(propertyValueString, out var propertyValue)
                ? propertyValue
                : defaultValue;
        }

        private static ImmutableArray<string> GetReferences(IEnumerable<ProjectItem> items) =>
            items
                .Where(i => i.ItemType == "Reference")
                .Select(i => i.EvaluatedInclude)
                .Where(i => i.Contains("ScriptAssemblies"))
                .Select(i => Path.GetFileNameWithoutExtension(i)!)
                .ToImmutableArray();
    }
}

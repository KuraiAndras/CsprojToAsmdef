using Microsoft.Build.Evaluation;
using MoreLinq.Extensions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CsprojToAsmdef.Cli.Domain
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "False positive")]
    public sealed class Asmdef
    {
        private readonly Project _project;
        private readonly string _assetsFolder;

        public Asmdef(Project project)
        {
            _project = project;
            _assetsFolder = _project
                .AllEvaluatedProperties
                .Single(p => p.Name == "AssetsFolder")
                .EvaluatedValue;

            var properties = _project.AllEvaluatedProperties;

            Name = GetName(_project.FullPath);
            References = GetReferences();
            IncludePlatforms = GetCollectionProperty(properties, nameof(IncludePlatforms));
            ExcludePlatforms = GetCollectionProperty(properties, nameof(ExcludePlatforms));
            AllowUnsafeCode = GetBoolProperty(properties, "AllowUnsafeBlocks", false);
            OverrideReferences = true;
            PrecompiledReferences = GetFilesToCopy()
                .Where(f => Path.GetExtension(f) != "dll")
                .Select(f => Path.GetFileName(f)!)
                .OrderBy(f => f)
                .ToImmutableArray();
            AutoReferenced = GetBoolProperty(properties, nameof(AutoReferenced), true);
            DefineConstraints = GetCollectionProperty(properties, nameof(DefineConstraints));
            VersionDefines = ImmutableArray<string>.Empty;
            NoEngineReferences = GetBoolProperty(properties, nameof(NoEngineReferences), false);
        }

        public string Name { get; }
        public ImmutableArray<string> References { get; }
        public ImmutableArray<string> IncludePlatforms { get; }
        public ImmutableArray<string> ExcludePlatforms { get; }
        public bool AllowUnsafeCode { get; }
        public bool OverrideReferences { get; }
        public ImmutableArray<string> PrecompiledReferences { get; }
        public bool AutoReferenced { get; }
        public ImmutableArray<string> DefineConstraints { get; }
        public ImmutableArray<string> VersionDefines { get; }
        public bool NoEngineReferences { get; }

        public string GetFilePath() =>
            Path.GetFullPath(
                Path.Combine(
                    _project.DirectoryPath,
                    Name + ".asmdef"));

        public string GetNuGetFolder() => Path.GetFullPath(Path.Combine(_assetsFolder, "NuGet"));

        public string GetOutputDirectory()
        {
            // There must be a better way for this.
            var outputDirectoryFromProject = _project
                .AllEvaluatedProperties
                .Where(p => p.Name == "OutputPath")
                .Select(p => p.EvaluatedValue)
                .Distinct()
                .MaxBy(v => v.Length)
                .First();

            return Path.GetFullPath(Path.Combine(_project.DirectoryPath, outputDirectoryFromProject));
        }

        public ImmutableArray<string> GetFilesToCopy()
        {
            var outputDirectory = GetOutputDirectory();
            var projectName = Path.GetFileNameWithoutExtension(_project.FullPath);
            var referencedAsmdefs = References.Select(r => $"{r}.dll").ToImmutableArray();

            return Directory
                .EnumerateFiles(outputDirectory, "*", SearchOption.AllDirectories)
                .Where(f =>
                {
                    var fileName = Path.GetFileName(f);

                    return fileName != $"{projectName}.dll"
                           && !f.EndsWith(".pdb")
                           && !f.EndsWith(".deps.json")
                           && !f.EndsWith(".dll.RoslynCA.json")
                           && !referencedAsmdefs.Contains(fileName);
                })
                .ToImmutableArray();
        }

        private static string GetName(string projectPath) => Path.GetFileNameWithoutExtension(projectPath);

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

        private ImmutableArray<string> GetReferences()
        {
            var upmReferences = _project.AllEvaluatedItems
                .Where(i => i.ItemType is "Reference")
                .Select(i => i.EvaluatedInclude)
                .Where(i => i.Contains("ScriptAssemblies"))
                .ToImmutableArray();

            var internalProjectReferences = _project.AllEvaluatedItems
                .Where(i => i.ItemType is "ProjectReference")
                .Select(i => i.EvaluatedInclude)
                .Select(i => Path.IsPathRooted(i)
                    ? i
                    : Path.GetFullPath(Path.Combine(_project.DirectoryPath, i)))
                .Where(i => i.StartsWith(_assetsFolder))
                .ToImmutableArray();

            var transitiveAsmdefReferences = internalProjectReferences
                .SelectMany(p => new Asmdef(new Project(p)).References);

            return Enumerable.Empty<string>()
                .Concat(upmReferences)
                .Concat(internalProjectReferences)
                .Select(i => Path.GetFileNameWithoutExtension(i)!)
                .Concat(transitiveAsmdefReferences)
                .Distinct()
                .OrderBy(i => i)
                .ToImmutableArray();
        }

        public string CreateJson() =>
            JsonSerializer
                .Serialize(
                    this,
                    new JsonSerializerOptions(JsonSerializerDefaults.General)
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true,
                    })
                .Replace("  ", "    ");
    }
}

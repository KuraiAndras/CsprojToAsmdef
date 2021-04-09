using Microsoft.Build.Evaluation;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CsprojToAsmdef.Cli.Domain
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "False positive")]
    public sealed class Asmdef
    {
        public Asmdef(Project project, ImmutableArray<string> copiedFiles)
        {
            var properties = project.AllEvaluatedProperties;

            Name = GetName(project.FullPath);
            References = GetReferences(project.AllEvaluatedItems);
            IncludePlatforms = GetCollectionProperty(properties, nameof(IncludePlatforms));
            ExcludePlatforms = GetCollectionProperty(properties, nameof(ExcludePlatforms));
            AllowUnsafeCode = GetBoolProperty(properties, "AllowUnsafeBlocks", false);
            OverrideReferences = true;
            PrecompiledReferences = copiedFiles.Where(f => Path.GetExtension(f) != "dll").Select(f => Path.GetFileName(f)!).ToImmutableArray();
            AutoReferenced = GetBoolProperty(properties, nameof(AutoReferenced), true);
            DefineConstraints = GetCollectionProperty(properties, nameof(DefineConstraints));
            VersionDefines = ImmutableArray<string>.Empty;
            NoEngineReferences = GetBoolProperty(properties, nameof(NoEngineReferences), false);
        }

        [JsonConstructor]
        public Asmdef(
            string name,
            ImmutableArray<string> references,
            ImmutableArray<string> includePlatforms,
            ImmutableArray<string> excludePlatforms,
            bool allowUnsafeCode,
            bool overrideReferences,
            ImmutableArray<string> precompiledReferences,
            bool autoReferenced,
            ImmutableArray<string> defineConstraints,
            ImmutableArray<string> versionDefines,
            bool noEngineReferences)
        {
            Name = name;
            References = references;
            IncludePlatforms = includePlatforms;
            ExcludePlatforms = excludePlatforms;
            AllowUnsafeCode = allowUnsafeCode;
            OverrideReferences = overrideReferences;
            PrecompiledReferences = precompiledReferences;
            AutoReferenced = autoReferenced;
            DefineConstraints = defineConstraints;
            VersionDefines = versionDefines;
            NoEngineReferences = noEngineReferences;
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

        public string CreateFileName() => Name + ".asmdef";

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

        private static ImmutableArray<string> GetReferences(IEnumerable<ProjectItem> items) =>
            items
                .Where(i => i.ItemType == "Reference")
                .Select(i => i.EvaluatedInclude)
                .Where(i => i.Contains("ScriptAssemblies"))
                .Select(i => Path.GetFileNameWithoutExtension(i)!)
                .ToImmutableArray();

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

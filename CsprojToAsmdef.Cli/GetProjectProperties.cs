using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using CliWrap;
using Microsoft.Build.Evaluation;
using MoreLinq;

namespace CsprojToAsmdef.Cli
{
    [Command(nameof(GetProjectProperties))]
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
            (properties
                .SingleOrDefault(p => p.Name == propertyName)
                ?.EvaluatedValue)
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

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "False positive")]
        public sealed class Asmdef
        {
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
        }
    }
}

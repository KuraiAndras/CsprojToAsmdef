using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace CsprojToAsmdef.Cli.Domain
{
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
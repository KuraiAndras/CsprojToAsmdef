using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CsprojToAsmdef
{
    public static class AsmdefTools
    {
        public static void GenerateAllAsmdef()
        {
            foreach (var filePath in BuildTools.GetAllCsprojFiles())
            {
                var projectName = Path.GetFileNameWithoutExtension(filePath);
                var asmdefPath = Path.Combine(Path.GetDirectoryName(filePath)!, projectName + ".asmdef");

                var csprojLines = File.ReadAllLines(filePath);

                var includePlatforms = csprojLines.GetCollectionProperty("IncludePlatforms");
                var excludePlatforms = csprojLines.GetCollectionProperty("ExcludePlatforms");
                var allowUnsafeCode = csprojLines.GetBoolProperty("AllowUnsafeBlocks", false);
                var autoReferenced = csprojLines.GetBoolProperty("AutoReferenced", true);
                var defineConstraints = csprojLines.GetCollectionProperty("DefineConstraints");
                var versionDefines = csprojLines.GetCollectionProperty("VersionDefines");
                var noEngineReferences = csprojLines.GetBoolProperty("NoEngineReferences", false);

                var asmdef = new Asmdef
                {
                    name = projectName,
                    references = new string[0],
                    includePlatforms = includePlatforms,
                    excludePlatforms = excludePlatforms,
                    allowUnsafeCode = allowUnsafeCode,
                    overrideReferences = false,
                    precompiledReferences = new string[0],
                    autoReferenced = autoReferenced,
                    defineConstraints = defineConstraints,
                    versionDefines = versionDefines,
                    noEngineReferences = noEngineReferences,
                };

                var json = EditorJsonUtility.ToJson(asmdef, true);

                File.WriteAllText(asmdefPath, json);

                Debug.Log($"Created asmdef file: {asmdefPath}");
            }
        }

        private static string[] GetCollectionProperty(this IEnumerable<string> csprojLines, string propertyName) =>
            csprojLines
                .SingleOrDefault(l => l.Trim().StartsWith($"<{propertyName}>"))
                ?.Trim()
                .Replace($"<{propertyName}>", string.Empty)
                .Replace($"</{propertyName}>", string.Empty)
                .Split(';')
                .ToArray();

        private static bool GetBoolProperty(this IEnumerable<string> csprojLines, string propertyName, bool defaultValue) =>
            csprojLines
                .SingleOrDefault(l => l.Trim().StartsWith($"<{propertyName}>"))
                ?.Trim()
                .Replace($"<{propertyName}>", string.Empty)
                .Replace($"</{propertyName}>", string.Empty)
                .ToBool() ?? defaultValue;

        private static bool ToBool(this string text) => bool.Parse(text);

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "False positive")]
        private sealed class Asmdef
        {
            public string name;
            public string[] references;
            public string[] includePlatforms;
            public string[] excludePlatforms;
            public bool allowUnsafeCode;
            public bool overrideReferences;
            public string[] precompiledReferences;
            public bool autoReferenced;
            public string[] defineConstraints;
            public string[] versionDefines;
            public bool noEngineReferences;
        }
    }
}
using System.Diagnostics.CodeAnalysis;
using System.IO;
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

                var asmdef = Asmdef.Default(projectName);

                var json = EditorJsonUtility.ToJson(asmdef, true);

                File.WriteAllText(asmdefPath, json);

                Debug.Log($"Created asmdef file: {asmdefPath}");
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
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

            public static Asmdef Default(string name) => new Asmdef
            {
                name = name,
                references = new string[0],
                includePlatforms = new string[0],
                excludePlatforms = new string[0],
                allowUnsafeCode = false,
                overrideReferences = false,
                precompiledReferences = new string[0],
                autoReferenced = true,
                defineConstraints = new string[0],
                versionDefines = new string[0],
                noEngineReferences = false
            };
        }
    }
}
using System;
using System.IO;
using UnityEngine;

namespace CsprojToAsmdef
{
    public static class ProjectInitializer
    {
        private static readonly string PropsContent =
            "<Project>"
            + Environment.NewLine
            + Environment.NewLine
            + "  <PropertyGroup>"
            + Environment.NewLine
            + "    <BaseIntermediateOutputPath>.obj\\</BaseIntermediateOutputPath>"
            + Environment.NewLine
            + "    <LangVersion>8.0</LangVersion>"
            + Environment.NewLine
            + "    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>"
            + Environment.NewLine
            + Environment.NewLine
            + "    <UnityVersion>2020.2.3f1</UnityVersion>"
            + Environment.NewLine
            + Environment.NewLine
            + "  </PropertyGroup>"
            + Environment.NewLine
            + Environment.NewLine
            + "  <ItemGroup>"
            + Environment.NewLine
            + "    <PackageReference Include=\"Microsoft.Unity.Analyzers\" Version=\"1.10.0\" PrivateAssets=\"all\"/>"
            + Environment.NewLine
            + "  </ItemGroup>"
            + Environment.NewLine
            + Environment.NewLine
            + "  <ItemGroup>"
            + Environment.NewLine
            + "    <PackageReference Include=\"Unity3D\" Version=\"1.7.0\" />"
            + Environment.NewLine
            + "  </ItemGroup>"
            + Environment.NewLine
            + Environment.NewLine
            + "</Project>"
            + Environment.NewLine;

        private static readonly string PropsPath = Path.Combine(Application.dataPath, "Directory.Build.props");

        private static readonly string TargetsContent =
            "<Project>"
            + Environment.NewLine
            + Environment.NewLine
            + "  <ItemGroup>"
            + Environment.NewLine
            + "    <None Remove=\"**/**/*.meta\" />"
            + Environment.NewLine
            + "  </ItemGroup>"
            + Environment.NewLine
            + Environment.NewLine
            + "  <Target Name=\"Cleanup\" AfterTargets=\"AfterBuild\">"
            + Environment.NewLine
            + Environment.NewLine
            + "    <Delete Files=\"$(OutDir)/$(AssemblyName).dll\" />"
            + Environment.NewLine
            + "    <Delete Files=\"$(OutDir)/$(AssemblyName).pdb\" />"
            + Environment.NewLine
            + "    <Delete Files=\"$(OutDir)/$(AssemblyName).dll.RoslynCA.json\" />"
            + Environment.NewLine
            + "    <Delete Files=\"$(OutDir)/$(AssemblyName).deps.json\" />"
            + Environment.NewLine
            + Environment.NewLine
            + "    <RemoveDir Directories=\"$(MSBuildProjectDirectory)/obj\"/>"
            + Environment.NewLine
            + "    <Delete Files=\"$(MSBuildProjectDirectory)/obj.meta\" />"
            + Environment.NewLine
            + Environment.NewLine
            + "  </Target>"
            + Environment.NewLine
            + Environment.NewLine
            + "</Project>"
            + Environment.NewLine;

        private static readonly string TargetsPath = Path.Combine(Application.dataPath, "Directory.Build.targets");

        private static readonly string GitIgnoreContent =
            "!*.csproj"
            + Environment.NewLine
            + Environment.NewLine;

        private static readonly string GitIgnorePath = Path.Combine(Application.dataPath, ".gitignore");

        public static void InitializeProject()
        {
            InitBuildFile(PropsPath, PropsContent);
            InitBuildFile(TargetsPath, TargetsContent);
            InitBuildFile(GitIgnorePath, GitIgnoreContent);
        }

        private static void InitBuildFile(string filePath, string fileContent)
        {
            if (File.Exists(filePath))
            {
                Debug.Log($"The build file {filePath} already exists");
                return;
            }

            File.WriteAllText(filePath, fileContent);
            Debug.Log($"Created file {filePath}");
        }
    }
}

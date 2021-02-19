using UnityEditor;

namespace CsprojToAsmdef
{
    public static class MenuUi
    {
        [MenuItem("Csproj Tools / Initialize Unity project")]
        public static void InitProject() => ProjectInitializer.InitializeProject();

        [MenuItem("Csproj Tools / Restore all projects")]
        public static void RestoreProjects() => BuildTools.BuildAllCsproj();

        [MenuItem("Csproj Tools / Generate all asmdef files")]
        public static void GenerateAllAsmdef() => AsmdefTools.GenerateAllAsmdef();
    }
}
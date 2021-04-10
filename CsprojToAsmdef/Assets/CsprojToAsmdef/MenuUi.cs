using UnityEditor;

namespace CsprojToAsmdef
{
    public static class MenuUi
    {
        [MenuItem("Csproj Tools / Initialize Unity project")]
        public static void InitProject() => ProjectInitializer.InitializeProject();

        [MenuItem("Csproj Tools / Fix up all projects")]
        public static void RestoreProjects() => BuildTools.BuildAllCsproj();
    }
}

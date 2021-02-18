using UnityEditor;

namespace CsprojToAsmdef
{
    public static class MenuUi
    {
        [MenuItem("Tools / Initialize Unity project")]
        public static void InitProject() => ProjectInitializer.InitializeProject();

        [MenuItem("Tools / Restore all projects")]
        public static void RestoreProjects() => BuildTools.BuildAllCsproj();
    }
}
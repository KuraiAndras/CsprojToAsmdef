using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace CsprojToAsmdef
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213:Remove unused member declaration.", Justification = "Unity life cycle method")]
    public sealed class MenuUi : EditorWindow
    {
        [MenuItem("Csproj Tools / Initialize Unity project")]
        public static void InitProject()
        {
            var window = CreateInstance<MenuUi>();
            window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 400f, 120f);
            window.ShowModalUtility();
        }

        [MenuItem("Csproj Tools / Fix up all projects")]
        public static async Task RestoreProjects() => await BuildTools.BuildAllCsproj();

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Do you want to ignore the downloaded DLL files?", Styles.LabelCenterBold);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("If you use a UPM package which depends on a NuGet package,", Styles.LabelCenter);
            EditorGUILayout.LabelField("it is advised to commit the DLLs", Styles.LabelCenter);

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes"))
            {
                ProjectInitializer.InitializeProject(true);
                Close();
            }

            if (GUILayout.Button("No"))
            {
                ProjectInitializer.InitializeProject(false);
                Close();
            }

            GUILayout.EndHorizontal();
        }
    }
}

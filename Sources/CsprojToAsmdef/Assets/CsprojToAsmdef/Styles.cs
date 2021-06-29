using UnityEditor;
using UnityEngine;

namespace CsprojToAsmdef
{
    public static class Styles
    {
        public static GUIStyle LabelCenterBold { get; } = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
        public static GUIStyle LabelCenter { get; } = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
    }
}

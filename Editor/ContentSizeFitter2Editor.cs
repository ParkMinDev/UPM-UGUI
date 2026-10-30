using ParkMinDev.UPM.UGUI.Components;
using UnityEditor;

namespace ParkMinDev.UPM.UGUI.Editor
{
    [CustomEditor(typeof(ContentSizeFitter2)), CanEditMultipleObjects]
    sealed class ContentSizeFitter2Editor : UnityEditor.UI.ContentSizeFitterEditor
    {
        // - Public Methods -
        public override void OnInspectorGUI() {
            Sirenix.Utilities.Editor.SirenixEditorGUI.InfoMessageBox("귀찮은 프리팹 오버라이드가 안나타납니다.");
            base.OnInspectorGUI();
        }
    }
}

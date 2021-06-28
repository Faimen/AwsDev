using UnityEngine;
using UnityEngine.UI;

namespace Template.UIBase
{
    /// <summary>
    /// Позволяет GraphicRaycaster'у работать по RectTransform'у даже без Graphic-компонентов.
    /// </summary>
    public class NonDrawingGraphic : Graphic
    {
        public override void SetMaterialDirty() { return; }
        public override void SetVerticesDirty() { return; }
    }


#if UNITY_EDITOR

    [UnityEditor.CanEditMultipleObjects, UnityEditor.CustomEditor(typeof(NonDrawingGraphic), false)]
    public class NonDrawingGraphicEditor : UnityEditor.UI.GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            base.serializedObject.Update();
            UnityEditor.EditorGUILayout.PropertyField(base.m_Script, new GUILayoutOption[0]);
            // skipping AppearanceControlsGUI
            base.RaycastControlsGUI();
            base.serializedObject.ApplyModifiedProperties();
        }
    }
#endif

}
using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer
    {
        public override void DrawProperty(SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();

            int flagsValue = EditorGUILayout.MaskField(property.displayName, property.intValue, property.enumNames);

            if (EditorGUI.EndChangeCheck())
                property.intValue = flagsValue;
        }
    }
}
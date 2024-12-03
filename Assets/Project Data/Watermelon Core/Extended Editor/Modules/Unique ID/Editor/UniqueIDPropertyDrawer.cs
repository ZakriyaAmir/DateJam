using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(UniqueIDAttribute), true)]
    public class UniqueIDPropertyDrawer : UnityEditor.PropertyDrawer
    {
        private SerializedObject storedSerializedObject;
        private bool lockState;

        private GUIContent copyIconContent;
        private GUIContent lockedIconContent;
        private GUIContent unlockedIconContent;
        private GUIContent resetIconContent;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (storedSerializedObject != property.serializedObject)
            {
                lockState = false;

                storedSerializedObject = property.serializedObject;

                copyIconContent = new GUIContent(WatermelonEditor.Styles.GetIcon("icon_copy"), "Copy to clipboard");
                lockedIconContent = new GUIContent(WatermelonEditor.Styles.GetIcon("icon_locked"), "Allow editing");
                unlockedIconContent = new GUIContent(WatermelonEditor.Styles.GetIcon("icon_unlocked"), "Block editing");
                resetIconContent = new GUIContent(WatermelonEditor.Styles.GetIcon("icon_reset"), "Reset ID");
            }

            string idValue = property.stringValue;

            if (string.IsNullOrEmpty(idValue))
            {
                idValue = Regenerate(property);
            }

            UniqueIDHandler.IDCase idCase = UniqueIDHandler.GetCase(idValue);
            int instanceID = property.serializedObject.targetObject.GetInstanceID();

            if (idCase != null)
            {
                if(idCase.RequireReset(property.propertyPath, instanceID))
                {
                    idValue = Regenerate(property);

                    UniqueIDHandler.RegisterID(new UniqueIDHandler.IDCase(property.propertyPath, instanceID, idValue));
                }
            }
            else
            {
                UniqueIDHandler.RegisterID(new UniqueIDHandler.IDCase(property.propertyPath, instanceID, idValue));
            }

            Rect fieldRect = new Rect(position.x, position.y, position.width - 62, position.height);

            using (new EditorGUI.DisabledScope(disabled: !lockState))
            {
                EditorGUI.PropertyField(fieldRect, property);
            }

            Rect buttonToggleRect = new Rect(fieldRect.x + fieldRect.width + 2, position.y, 18, 18);
            if (GUI.Button(buttonToggleRect, lockState ? unlockedIconContent : lockedIconContent))
            {
                lockState = !lockState;
            }

            Rect buttonResetRect = new Rect(fieldRect.x + fieldRect.width + 22, position.y, 18, 18);
            if (GUI.Button(buttonResetRect, resetIconContent))
            {
                if(EditorUtility.DisplayDialog("Reset ID?", "Are you sure you want to reset ID?", "Reset", "Cancel"))
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "ID regenerated");

                    idValue = Regenerate(property);

                    UniqueIDHandler.RegisterID(new UniqueIDHandler.IDCase(property.propertyPath, instanceID, idValue));
                }
            }

            Rect buttonCopyRect = new Rect(fieldRect.x + fieldRect.width + 42, position.y, 18, 18);
            if (GUI.Button(buttonCopyRect, copyIconContent))
            {
                GUIUtility.systemCopyBuffer = property.stringValue;
            }
        }

        private string Regenerate(SerializedProperty serializedProperty)
        {
            serializedProperty.serializedObject.Update();

            System.Guid guid = System.Guid.NewGuid();
            string idValue = guid.ToString();

            serializedProperty.stringValue = idValue;

            serializedProperty.serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(serializedProperty.serializedObject.targetObject);

            return idValue;
        }
    }
}

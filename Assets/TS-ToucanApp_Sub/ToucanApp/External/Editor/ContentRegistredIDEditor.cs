using System.Collections.Generic;
using UnityEditor;

namespace ToucanApp.Data
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ContentRegisteredID))]
    public class ContentRegistredIDEditor : Editor
    {
        private SerializedProperty registry;
        private SerializedProperty registryIndex;

        private void OnEnable()
        {
            registry = serializedObject.FindProperty("registry");
            registryIndex = serializedObject.FindProperty("registryIndex");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.ObjectField(registry);

            if (registry.objectReferenceValue)
            {
                if (!registry.hasMultipleDifferentValues)
                {
                    EditorGUI.showMixedValue = registryIndex.hasMultipleDifferentValues;

                    var storedIds = ((IDRegistry)registry.objectReferenceValue).storedIds;
                    var options = new List<string>();

                    options.Add("NONE");
                    foreach (var storedId in storedIds)
                        options.Add(string.IsNullOrEmpty(storedId)? "-" : storedId);

                    int newValue = UnityEditor.EditorGUILayout.Popup("ID", registryIndex.intValue + 1, options.ToArray()) - 1;
                    if (newValue != registryIndex.intValue)
                        registryIndex.intValue = newValue;

                    EditorGUI.showMixedValue = false;
                }
            }
            else
            {
                if (registryIndex.intValue != -1)
                    registryIndex.intValue = -1;
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}

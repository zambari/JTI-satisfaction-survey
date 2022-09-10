using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(SmartText))]
public class SmartTextEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var obj = target as SmartText;

        var serializedObject = new SerializedObject(obj);
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Text"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BendStrength"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_EnableSmartWrap"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_EnableJustify"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_EnableCapitals"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_EnableLayoutProperties"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Fonts"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CharSpacing"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FontData"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Color"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Material"), true);

        serializedObject.ApplyModifiedProperties();
    }

    [MenuItem("GameObject/UI/Smart Text")]
    static void CreateSmartText()
    {
        var parentGO = Selection.activeGameObject;
        var smartTextGO = new GameObject("Text");
        if (parentGO != null)
        {
            smartTextGO.transform.SetParent(parentGO.transform, false);
        }

        smartTextGO.AddComponent<SmartText>();

        Selection.activeGameObject = smartTextGO;
    }
}

using UnityEngine;
using UnityEditor;

namespace ToucanApp.States
{
    [CustomEditor(typeof(CanvasSubState))]
    public class CanvasSubStateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            CanvasSubState me = (CanvasSubState)target;

            GUILayout.Space(10);

            if (GUILayout.Button("Enter State"))
            {
                me.EnterStateWithParents();
            }

            if (GUILayout.Button("Toggle State"))
            {
                me.Toggle();
            }
        }
    }
}

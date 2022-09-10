using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ToucanApp.States
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    class StatesHierarchyHighlight
    {
        static Color color;

        static StatesHierarchyHighlight()
        {
            // Init
            color = Application.HasProLicense() ? new Color(1, 1, 1, 1f) : new Color(0, 1, 0, .1f);
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
        }

        static void HierarchyItemCB(int instanceID, Rect selectionRect)
        {
            Rect r = new Rect(selectionRect);

            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (go != null)
            {
                var ss = go.GetComponent<SubStateMachine>();
                if (ss != null && ss.isBranchActive)
                {
                    GUI.backgroundColor = color;
                    GUI.Box(r, "");
                    GUI.backgroundColor = Color.white;
                }
            }
        }
    }
#endif
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ToucanApp.States
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    [SelectionBase]
    public class CanvasStateVisualization : MonoBehaviour
    {
        Color color = new Color(1f, 1f, 1f, 0.3f);

        Vector3 textCorner;
        Vector3[] screenCorners;

        [SerializeField][HideInInspector] Vector3 restPosition;
        void GetCoords()
        {
            RectTransform rect = GetComponent<RectTransform>();

            screenCorners = new Vector3[4];

            rect.GetWorldCorners(screenCorners);
            textCorner = screenCorners[0] - new Vector3(0f, 30f, 0f);
        }
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!gameObject.activeInHierarchy)
                return;

            GetCoords();

            Gizmos.color = color;
            for (int i = 0; i < 4; i++)
                Gizmos.DrawLine(screenCorners[i], screenCorners[(i + 1) % 4]);

            Handles.Label(textCorner, name, EditorStyles.miniLabel);
        }
#endif

    }
}
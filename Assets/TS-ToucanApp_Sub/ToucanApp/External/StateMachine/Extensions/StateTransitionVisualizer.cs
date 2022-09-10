
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using ToucanApp.States;
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class StateTransitionVisualizer : MonoBehaviour
{
    static readonly Color color1 = new Color(1, 0.5f, 0, 0.5f);
    static readonly Color color2 = new Color(0.6f, 0.7f, 0, 0.5f);

    private RectTransform thisRect;
    private Vector2Int selectedFaces;

    private Vector3 forward = new Vector3(0, 0, -110);
    private Vector3[] ThisRectPoints = new Vector3[4];
    private Vector3[] OtherRectPoints = new Vector3[4];

    private Vector3[] ThisRectBorders = new Vector3[4];
    private Vector3[] OtherRectBorders = new Vector3[4];

    private Vector3[] arrow = new Vector3[2];

    public Color color = color1;

    private float bezierAmount = 1f;
    private float arrowAmt = 10f;

    public RectTransform otherRect;

    private void GrabLayout()
    {
        if (thisRect == null) 
            thisRect = GetComponent<RectTransform>();

        thisRect.GetWorldCorners(ThisRectPoints);

        if (otherRect == null) 
            return;

        otherRect.GetWorldCorners(OtherRectPoints);

        for (int i = 0; i < 4; i++)
        {
            ThisRectBorders[i] = (ThisRectPoints[i] + ThisRectPoints[(i + 1) % 4]) / 2;
            OtherRectBorders[i] = (OtherRectPoints[i] + OtherRectPoints[(i + 1) % 4]) / 2;
        }

        float minDistnace = System.Single.MaxValue;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                float thisDistance = (ThisRectBorders[i] - OtherRectBorders[j]).magnitude;
                if (thisDistance < minDistnace)
                {
                    minDistnace = thisDistance;
                    selectedFaces = new Vector2Int(i, j);
                    arrow[0] = OtherRectBorders[j] + new Vector3(arrowAmt * ((i == 0 || i == 1) ? 1 : -1), arrowAmt * ((j == 0 || j == 1) ? 1 : -1));
                    arrow[1] = OtherRectBorders[j] + new Vector3(arrowAmt * ((i == 0 || i == 3) ? 1 : -1), arrowAmt * ((j == 1 || j == 2) ? 1 : -1));
                }
            }
        }

        bezierAmount = minDistnace / Mathf.Max(thisRect.rect.width, otherRect.rect.width, thisRect.rect.height, otherRect.rect.height);

    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        if (thisRect == null) thisRect = GetComponent<RectTransform>();
        if (otherRect != null && enabled)
        {
            if (!thisRect.gameObject.activeInHierarchy && !otherRect.gameObject.activeInHierarchy) 
                return;

            GrabLayout();

            Gizmos.color = Color.red;
            Vector3 startPos = ThisRectBorders[selectedFaces.x];
            Vector3 endPos = OtherRectBorders[selectedFaces.y];

            Vector3 startVect = startPos + bezierAmount * (startPos - thisRect.position);
            Vector3 endVect = endPos + bezierAmount * (endPos - otherRect.position);
            if (thisRect.gameObject.activeInHierarchy && otherRect.gameObject.activeInHierarchy)
                Gizmos.color = color;
            else 
                Gizmos.color = color / 2;

            Vector3 thisPos = startPos;
            Vector3 lastPos = startPos;
            for (float i = 0; i <= 1; i += 0.05f)
            {
                thisPos = BezierCalculate(i, startPos, startVect, endVect, endPos);
                Gizmos.DrawLine(thisPos + forward, lastPos + forward);
                lastPos = thisPos;
            }

            Gizmos.DrawWireCube(endPos + forward, Vector3.one * 50f);
            Gizmos.DrawLine(lastPos + forward, endPos + forward);
            Gizmos.DrawLine(endPos+forward,arrow[0]+forward);
            Vector3 ofs=new Vector3(10,10,0);
            Gizmos.DrawLine(endPos+forward+ofs,arrow[1]+forward+ofs);
        }
    }

    public static Vector3 BezierCalculate(float f, Vector3 startPoint, Vector3 startTangent, Vector3 endTangent, Vector3 endPoint)
    {
        float u = 1 - f;
        float tt = f * f;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * f;
        Vector3 p = uuu * startPoint;
        p += 3 * uu * f * startTangent;
        p += 3 * u * tt * endTangent;
        p += ttt * endPoint;
        return p;
    }
#endif
}
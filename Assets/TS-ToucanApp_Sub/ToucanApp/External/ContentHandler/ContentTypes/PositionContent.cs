using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Events;
using UnityEngine.UI;

namespace ToucanApp.Data
{
    public class PositionContent : AbstractContent<PositionData>
    {
		private RectTransform rect;

        private void Start()
        {
            rect = GetComponent<RectTransform>();
        }

        public override void OnExportData()
        {
    		base.OnExportData ();
            rect = GetComponent<RectTransform>();
            Data.x = (double)((rect.anchorMin.x + rect.anchorMax.x) / 2);
            Data.y = (double)((rect.anchorMin.y + rect.anchorMax.y) / 2);
    	}

        public override void OnImportData()
        {
            base.OnImportData();

            var x = (float)Data.x;
            var y = (float)Data.y;

            rect.anchorMin = new Vector2(x, y);
            rect.anchorMax = new Vector2(x, y);
            rect.anchoredPosition3D = Vector3.zero;
        }
    }
}

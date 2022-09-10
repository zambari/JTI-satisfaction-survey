using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Events;
using UnityEngine.UI;
using ToucanApp.Config;

namespace ToucanApp.Data
{
    public class DoubleContent : AbstractContent<DoubleData>
    {
        public double value = 0;
        public FloatEvent onImported;

        public override void OnExportData()
        {
    		base.OnExportData();
			Data.value = value;
    	}

        public override void OnImportData()
        {
            base.OnImportData();
            value = Data.value;

            if (onImported != null)
                onImported.Invoke((float)value);
        }
    }
}

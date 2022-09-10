using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.Data
{
    public class RootContent : AbstractParentContent<RootData>
    {
        public string[] languages = new string[1] {"PL"};

        public int LanguagesCount
        {
            get { return languages.Length; }
        }

        public override void OnExportData()
        {
            base.OnExportData();

			Data.AppID = ContentHandler.AppID;
            Data.translations = languages;
        }

        public override void OnImportData()
        {
            base.OnImportData();
			languages = Data.translations;
        }
    }
}

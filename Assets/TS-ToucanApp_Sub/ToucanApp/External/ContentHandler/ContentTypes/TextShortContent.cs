using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Events;
using UnityEngine.UI;

namespace ToucanApp.Data
{
    public class TextShortContent : AbstractContent<TextData>
    {
//        [System.Serializable]
//        public class ImportEvent : UnityEvent<string> {}
//        public ImportEvent onImported;

		public string GetText(int languageId)
		{
			return Data.GetTranslation(languageId);
		}

        public override void OnLanguageChanged(int languageId)
        {
            base.OnLanguageChanged(languageId);

            var text = GetComponent<Text>();

            if (text != null)
			{
				var translation = Data.GetTranslation (languageId);
				if (translation != null)
					text.text = translation.TrimStart().TrimEnd();
			}
        }

        public override void OnExportData()
        {
            base.OnExportData();

            if (Data.translations != null || Data.translations.Length > 0)
            {
                var text = GetComponent<Text>();

                if (text != null)
                {
                    Data.translations[ContentHandler.LanguageID] = text.text;
                }
            }
        }
    }
}

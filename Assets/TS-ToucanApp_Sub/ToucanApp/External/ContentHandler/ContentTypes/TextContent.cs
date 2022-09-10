using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Events;
using UnityEngine.UI;
using ToucanApp.Config;
#if USING_TEXT_MESH_PRO
using TMPro;
#endif

namespace ToucanApp.Data
{
    public class TextContent : AbstractContent<TextareaData>
    {
        public bool forceUpperCase = false;
        public StringEvent onImported;

		public string GetText(int languageId)
		{
            string translation = Data.GetTranslation(languageId);

            if (!string.IsNullOrEmpty(translation))
            {
                return translation;
            }

#if USING_TEXT_MESH_PRO
            var textPro = GetComponent<TextMeshProUGUI>();
            if (textPro != null)
            {
                return textPro.text;
            }
#endif

            var text = GetComponent<Text>();
            if (text != null)
            {
                return text.text;
            }

            return null;
        }

        public override void OnLanguageChanged(int languageId)
        {
            base.OnLanguageChanged(languageId);

            var translation = Data.GetTranslation(languageId);

            if (translation == null)
            {
                if (Application.isEditor)
                    return;
                translation = "";
            }

            translation.Trim();

            if (forceUpperCase)
                translation = translation.ToUpper();

#if USING_TEXT_MESH_PRO
            var textPro = GetComponent<TextMeshProUGUI>();
            if (textPro != null)
            {
                textPro.text = translation;
            }
#endif
            var text = GetComponent<Text>();
            if (text != null)
			{
                text.text = translation;
			}

            if (onImported != null)
                onImported.Invoke(translation);
        }

        public override void OnExportData()
        {
            base.OnExportData();

//            if (GetComponentInParent<ArticleContent>() != null && !Data.isClone)
//            {
//#if USING_TEXT_MESH_PRO
//                var textPro = GetComponent<TextMeshProUGUI>();
//                if (textPro != null)
//                    textPro.text = "";
//#endif
//                var text = GetComponent<Text>();
//                if (text != null)
//                    text.text = "";
//            }

            if (Data.translations != null && Data.translations.Length > 0)
            {
#if USING_TEXT_MESH_PRO
                var textPro = GetComponent<TextMeshProUGUI>();
                if (textPro != null)
                {
                    Data.translations[ContentHandler.LanguageID] = textPro.text;
                }
#endif
                var text = GetComponent<Text>();
                if (text != null)
                {
                    Data.translations[ContentHandler.LanguageID] = text.text;
                }
            }
            else
            {
                Data.translations = new string[ContentHandler.RootContent.LanguagesCount];
            }
        }
    }
}

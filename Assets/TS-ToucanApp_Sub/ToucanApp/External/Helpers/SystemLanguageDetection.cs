using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ToucanApp.Data
{
    public class SystemLanguageDetection : MonoBehaviour
    {
        private const string CMS_LANGUAGE = "SelectedLanguageIdx";

        [Serializable]
        public class LanguagePair
        {
            public SystemLanguage systemLanguage;
            public string alpha2code;
        }

        public LanguagePair[] languagePairs;
        private ContentHandler contentHandler;

        private void Reset()
        {
            languagePairs = new LanguagePair[4];

            for (int i = 0; i < languagePairs.Length; i++)
                languagePairs[i] = new LanguagePair();

            languagePairs[0].alpha2code = "PL";
            languagePairs[0].systemLanguage = SystemLanguage.Polish;

            languagePairs[1].alpha2code = "EN";
            languagePairs[1].systemLanguage = SystemLanguage.English;

            languagePairs[2].alpha2code = "DE";
            languagePairs[2].systemLanguage = SystemLanguage.German;

            languagePairs[3].alpha2code = "RU";
            languagePairs[3].systemLanguage = SystemLanguage.Russian;
        }

        private void Awake()
        {
            contentHandler = GetComponentInParent<ContentHandler>();
            int savedLanguageIdx = PlayerPrefs.GetInt(CMS_LANGUAGE, -1);

            if (savedLanguageIdx == -1)
            {
                var systemLanguage = Application.systemLanguage;
                var langaugePair = languagePairs.FirstOrDefault(item => item.systemLanguage == systemLanguage);

                if (langaugePair != null)
                {
                    string alpha2code = langaugePair.alpha2code.ToUpper();
                    int languageIdx = Array.FindIndex(contentHandler.RootContent.languages, item => item.ToUpper() == alpha2code);

                    if (languageIdx > -1)
                        contentHandler.LanguageID = languageIdx;
                }
            }
            else
            {
                contentHandler.LanguageID = savedLanguageIdx;
            }
        }

        private void OnDestroy()
        {
            PlayerPrefs.SetInt(CMS_LANGUAGE, contentHandler.LanguageID);
        }
    }
}

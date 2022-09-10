using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToucanApp.Data;

public class URLContent : AbstractContent<TextData>
{
	[SerializeField]
	protected string url;

	public virtual void OpenUrl()
	{
		Application.OpenURL(url);
	}

	public override void OnLanguageChanged (int languageId)
	{
		base.OnLanguageChanged (languageId);

		url = Data.GetTranslation (languageId);
	}

    public override void OnExportData()
    {
        base.OnExportData();

        if (Data.translations.Length <= 1)
            Data.translations = new string[] { url };
        else
            Data.translations[ContentHandler.LanguageID] = url;
    }
}

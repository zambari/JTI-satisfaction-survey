using System;
using UnityEngine;
using UnityEngine.Events;

namespace ToucanApp.Data
{
    public class AssetContent : AbstractParentContent<AssetData>
    {
        [Serializable]
        public class AssetEvent : UnityEvent<AssetBundle> { }

        public AssetBundle bundle;
        public AssetEvent onImported;

        public override void OnLanguageChanged(int languageId)
        {
            base.OnLanguageChanged(languageId);
            ContentHandler.Resources.Load(Data, OnLoadResource, languageId);
        }

        public void OnLoadResource(ResourceInfo resource)
        {
            bundle = resource.GetAsset();

            if (onImported != null)
                onImported.Invoke(bundle);
        }
    }
}

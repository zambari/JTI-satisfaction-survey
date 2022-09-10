using System;
using UnityEngine;
using UnityEngine.Events;
using ToucanApp.Helpers;

namespace ToucanApp.Data
{
    public class LazyImageContent : AbstractContent<ImageData>
    {
        [Serializable]
        public class TextureEvent : UnityEvent<Texture> { };
        [Serializable]
        public class SpriteEvent : UnityEvent<Sprite> { };

        [SerializeField]
        private bool loadImageOnLanguageChange = false;

        [SerializeField]
        private bool downloadToDriveOnLanguageChange = false;

        public TextureEvent OnTextureLoaded;
        public SpriteEvent OnSpriteLoaded;

        public bool LoadingInProgress { get; private set; } = false;

        private Texture loadedTexture = null;
        private Sprite loadedSprite = null;

        public override void OnLanguageChanged(int languageId)
        {
            base.OnLanguageChanged(languageId);

            if (loadImageOnLanguageChange)
            {
                LoadingInProgress = true;
                ContentHandler.Resources.Load(Data, OnResourceLoaded, languageId);
            }
            else if (downloadToDriveOnLanguageChange)
            {
                LoadingInProgress = true;
                ContentHandler.Resources.Load(Data, OnDriveOnlyDownloaded, languageId, false);
            }
        }

        public void LoadImage()
        {
            LoadingInProgress = true;
            ContentHandler.Resources.SilentLoad(Data, OnResourceLoaded, ContentHandler.LanguageID);
        }

        private void OnResourceLoaded(ResourceInfo resourceInfo)
        {
            if (resourceInfo == null)
            {
                LoadingInProgress = false;
                return;
            }

            resourceInfo.referenceCount++;

            if (OnTextureLoaded != null)
            {
                loadedTexture = resourceInfo.GetTexture();
                OnTextureLoaded.Invoke(loadedTexture);
            }

            if (OnSpriteLoaded != null)
            {
                loadedSprite = resourceInfo.GetSprite();
                OnSpriteLoaded.Invoke(loadedSprite);
            }

            LoadingInProgress = false;
        }

        private void OnDriveOnlyDownloaded(ResourceInfo resourceInfo)
        {
            if (resourceInfo == null)
            {
                LoadingInProgress = false;
                return;
            }

            resourceInfo.referenceCount++;
            LoadingInProgress = false;
            UnloadImage();
        }

        public void GetTexture(Action<Texture> onDone)
        {
            LoadingInProgress = true;
            ContentHandler.Resources.SilentLoad(Data, (x) => GetTextureCallback(x, onDone), ContentHandler.LanguageID);
        }

        private void GetTextureCallback(ResourceInfo resourceInfo, Action<Texture> onDone)
        {
            if (resourceInfo == null)
            {
                LoadingInProgress = false;
                return;
            }

            resourceInfo.referenceCount++;
            loadedTexture = resourceInfo.GetTexture();
            onDone?.Invoke(loadedTexture);

            LoadingInProgress = false;
        }

        public void GetSprite(Action<Sprite> onDone)
        {
            LoadingInProgress = true;
            ContentHandler.Resources.SilentLoad(Data, (x) => GetSpriteCallback(x, onDone), ContentHandler.LanguageID);
        }

        private void GetSpriteCallback(ResourceInfo resourceInfo, Action<Sprite> onDone)
        {
            if (resourceInfo == null)
            {
                LoadingInProgress = false;
                return;
            }

            resourceInfo.referenceCount++;
            loadedSprite = resourceInfo.GetSprite();
            onDone?.Invoke(loadedSprite);

            LoadingInProgress = false;
        }

        public void UnloadImage(bool force = false)
        {
            if (ContentHandler != null)
            {
                ResourceInfo resource = ContentHandler.Resources.GetResourceInfo(Data, ContentHandler.LanguageID);

                if (resource != null)
                {
                    if (!force && --resource.referenceCount > 0)
                        return;

                    resource.referenceCount = 0;
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }

            if (loadedTexture != null)
                Destroy(loadedTexture);

            if (loadedSprite != null)
                Destroy(loadedSprite);

            if (ContentHandler != null && ContentHandler.Resources != null && Data != null)
                ContentHandler.Resources.Unload(Data);
        }
    }
}
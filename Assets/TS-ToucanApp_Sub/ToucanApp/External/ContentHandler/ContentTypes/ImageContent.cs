using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ToucanApp.Data
{
    public class ImageContent : AbstractContent<ImageData>
    {
        [Serializable]
        public class SpriteEvent : UnityEvent<Sprite> { }
        [Serializable]
        public class TextureEvent : UnityEvent<Texture> { }

        public enum FallbackEventType
        {
            None,
            Texture,
            Sprite,
            Both,
        }

        [SerializeField]
        private bool useNativeSize = false;
        [SerializeField]
        private bool usePreserveAspect = true;

        [SerializeField]
        private bool restrictResolution = false;
        [SerializeField, ConditionalHide("restrictResolution", true)]
        private bool overrideResolution = false;
        [SerializeField, ConditionalHide("overrideResolution", true)]
        private int resolutionX = 1024;
        [SerializeField, ConditionalHide("overrideResolution", true)]
        private int resolutionY = 1024;

        public FallbackEventType fallbackEventType;
        public SpriteEvent OnSpriteLoaded;
        public TextureEvent OnTextureLoaded;

        private object defaultGraphic;

        private void Start()
        {
            var image = GetComponent<Image>();
            if (image != null)
            {
                defaultGraphic = image.sprite;

                var aspectFilter = GetComponent<AspectRatioFitter>();
                if (aspectFilter != null)
                    aspectFilter.aspectRatio = (float)image.mainTexture.width / image.mainTexture.height;

                return;
            }

            var rawImage = GetComponent<RawImage>();
            if (rawImage != null)
            {
                defaultGraphic = rawImage.mainTexture;

                var aspectFilter = GetComponent<AspectRatioFitter>();
                if (aspectFilter != null)
                    aspectFilter.aspectRatio = (float)rawImage.mainTexture.width / rawImage.mainTexture.height;

                return;
            }
        }

        public override void OnLanguageChanged(int languageId)
        {
            base.OnLanguageChanged(languageId);

            if (ContentHandler.Resources.autoLoadResource)
                ContentHandler.Resources.Load(Data, OnLoadResource, languageId);
        }
   
        public void LoadResource()
        {
            ContentHandler.Resources.Load(Data, OnLoadResource, ContentHandler.LanguageID);
        }

        public void UnloadResource()
        {
            var resource = ContentHandler.Resources.GetResourceInfo(Data, ContentHandler.LanguageID);

            if (resource != null)
            {
                resource.referenceCount--;
                if (resource.referenceCount == 0)
                {
                    resource = null;
                }
            }

            if (resource == null)
            {
                var image = GetComponent<Image>();
                if (image != null)
                {
                    Destroy(image.sprite);
                }

                var rawImage = GetComponent<RawImage>();
                if (rawImage != null)
                {
                    Destroy(rawImage.texture);
                }

                ContentHandler.Resources.Unload(Data);
            }
        }
            
        public void OnLoadResource(ResourceInfo resource)
        {
            resource.referenceCount++;

            var aspectFilter = GetComponent<AspectRatioFitter>();

            var rawImage = GetComponent<RawImage>();
            if (rawImage != null)
            {
                rawImage.texture = resource.isLoadedToMemory? resource.GetTexture() : (Texture)defaultGraphic;
                OnTextureLoaded?.Invoke(rawImage.texture);


                if (aspectFilter != null)
					aspectFilter.aspectRatio = (float)rawImage.mainTexture.width / rawImage.mainTexture.height;

                return;
            }

            var image = GetComponent<Image>();
            if (image != null)
            {
                image.sprite = resource.isLoadedToMemory ? resource.GetSprite() : (Sprite)defaultGraphic;
                OnSpriteLoaded?.Invoke(image.sprite);

                if (aspectFilter != null)
					aspectFilter.aspectRatio = (float)image.mainTexture.width / image.mainTexture.height;

                if (useNativeSize)
                    image.SetNativeSize();

                image.preserveAspect = usePreserveAspect;

                return;
            }

            if (fallbackEventType == FallbackEventType.Texture || fallbackEventType == FallbackEventType.Both)
                OnTextureLoaded?.Invoke(resource.GetTexture());
            if (fallbackEventType == FallbackEventType.Sprite || fallbackEventType == FallbackEventType.Both)
                OnSpriteLoaded?.Invoke(resource.GetSprite());
        }

        public override void OnExportData()
        {
            base.OnExportData();

            if (restrictResolution)
            {
                if (overrideResolution)
                {
                    Data.resolutionX = resolutionX;
                    Data.resolutionY = resolutionY;
                }
                else
                {
                    var image = GetComponent<Image>();
                    if (image != null)
                    {
                        Data.resolutionX = (int)image.rectTransform.sizeDelta.x;
                        Data.resolutionY = (int)image.rectTransform.sizeDelta.y;
                    }

                    var rawImage = GetComponent<RawImage>();
                    if (rawImage != null)
                    {
                        Data.resolutionX = (int)rawImage.rectTransform.sizeDelta.x;
                        Data.resolutionY = (int)rawImage.rectTransform.sizeDelta.y;
                    }
                }
            }
            else
            {
                Data.resolutionX = 0;
                Data.resolutionY = 0;
            }
        }
    }
}
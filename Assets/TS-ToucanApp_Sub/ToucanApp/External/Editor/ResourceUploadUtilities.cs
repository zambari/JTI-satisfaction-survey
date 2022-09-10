using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ToucanApp.Data;

namespace ToucanApp
{
    public static class ContentImagesUploader
    {
        [UnityEditor.MenuItem("Toucan App/Upload/Upload Images To CMS", false)]
        private static void UploadImages()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("This utility works only in playmode!");
                return;
            }

            var selected = UnityEditor.Selection.activeTransform;
            if (selected == null)
            {
                Debug.LogError("Gameobject was not selected!");
                return;
            }

            var handler = selected.GetComponentInParent<UploadHandler>();
            handler.StartCoroutine(Execute(handler));
        }

        private static IEnumerator Execute(UploadHandler handler)
        {

            var images = handler.GetComponentsInChildren<ImageContent>();
            var uploadedNames = new HashSet<string>();


            while (handler.ContentHandler.IsWorking || handler.ContentHandler.Resources.IsWorking)
            {
                yield return null;
            }

            string directory = "upload";

            foreach (var imageContent in images)
            {
                bool done = false;
                Texture2D texture = null;

                var image = imageContent.GetComponent<Image>();
                if (image != null)
                    texture = image.sprite.texture;

                var rawImage = imageContent.GetComponent<RawImage>();
                if (rawImage != null)
                    texture = TextureToTexture2D(rawImage.texture);

                if (texture != null)
                {
                    int idx = 0;
                    string uploadName = texture.name;

                    while (uploadedNames.Contains(uploadName))
                    {
                        uploadName = string.Format("{0}{1}", texture.name, idx++);
                    }
                    uploadedNames.Add(uploadName);

                    handler.UploadData(texture.EncodeToJPG(), string.Format("{0}.jpg", uploadName), directory, info =>
                    {
                        if (info != null)
                        {
                            if (imageContent.Data.translations == null || imageContent.Data.translations.Length == 0)
                                imageContent.Data.translations = new string[1];
                            imageContent.Data.translations[0] = info.path;

                            if (imageContent.Data.hash == null || imageContent.Data.hash.Length == 0)
                                imageContent.Data.hash = new string[1];
                            imageContent.Data.hash[0] = info.checksum;
                        }

                        done = true;
                    });

                    while (!done)
                    {
                        yield return null;
                    }

                    idx++;
                }
            }

            Debug.Log("Resources Uploaded!");
        }

        private static Texture2D TextureToTexture2D(Texture texture)
        {
            Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
            Graphics.Blit(texture, renderTexture);

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(renderTexture);
            return texture2D;
        }
    }
}

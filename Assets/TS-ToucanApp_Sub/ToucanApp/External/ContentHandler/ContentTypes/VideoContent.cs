using UnityEngine;
using UnityEngine.Video;
using ToucanApp.Config;

namespace ToucanApp.Data
{
    public class VideoContent : AbstractContent<VideoData>
    {
        [SerializeField, Tooltip("Play video on attached VideoPlayer (if there's any) right after load")]
        private bool autoPlay = false;

        public StringEvent OnVideoReady = new StringEvent();

        public override void OnLanguageChanged(int languageId)
        {
            base.OnLanguageChanged(languageId);

            if (ContentHandler.Resources.autoLoadResource)
                ContentHandler.Resources.Load(Data, OnLoadResource, languageId, false);   
        }

        public void LoadResource()
        {
            ContentHandler.Resources.Load(Data, OnLoadResource, ContentHandler.LanguageID, false);
        }

        public void UnloadResource()
        {
            ContentHandler.Resources.Unload(Data);
        }

        public virtual void OnLoadResource(ResourceInfo resource)
        {
            if (resource != null && resource.isSavedToDrive)
            {
                string videoURL = "";

                if (resource.isReadonly)
                    videoURL = ContentHandler.Resources.GetWwwAndroidStreamingAssetsPath(resource);
                else
                    videoURL = ContentHandler.Resources.GetLocalPath(resource);

                if (autoPlay)
                {
                    VideoPlayer videoPlayer = GetComponent<VideoPlayer>();

                    if (videoPlayer != null)
                    {
                        videoPlayer.Stop();
                        videoPlayer.source = VideoSource.Url;
                        videoPlayer.url = videoURL;
                        videoPlayer.Play();
                    }
                }

                OnVideoReady.Invoke(videoURL);
            }
        }
    }
}

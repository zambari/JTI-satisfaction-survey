using System;
using UnityEngine;
using UnityEngine.Events;

namespace ToucanApp.Data
{
    public class AudioContent : AbstractContent<AudioData>
    {
        [Serializable]
        public class AudioClipEvent : UnityEvent<AudioClip> { }

        [SerializeField, Tooltip("Play audio on attached AudioSource (if there's any) right afted load")]
        private bool autoPlay = false;

        public AudioClipEvent OnAudioClipLoaded;

		public override void OnLanguageChanged (int languageId)
		{
			base.OnLanguageChanged(languageId);
            ContentHandler.Resources.Load(Data, OnLoadResource, languageId);   
		}

		#region IResource implementation
		public void OnLoadResource (ResourceInfo resource)
		{
            AudioClip audioClip = resource.GetAudioClip();

            if (autoPlay)
            {
                AudioSource audioSource = GetComponent<AudioSource>();

                if (audioSource != null)
                {
                    audioSource.Stop();
                    audioSource.clip = audioClip;
                    audioSource.Play();
                }
            }

            OnAudioClipLoaded.Invoke(audioClip);
		}

		public ResourceData ResourceData 
		{
			get { return (ResourceData)Data; }
		}
		#endregion
    }
}
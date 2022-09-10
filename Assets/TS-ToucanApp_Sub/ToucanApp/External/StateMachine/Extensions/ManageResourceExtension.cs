using System.Collections.Generic;
using UnityEngine;
using ToucanApp.Data;
using ToucanApp.States;

namespace ToucanApp
{
	public class ManageResourceExtension : MonoBehaviour, IStateExtension
	{
		public bool loadOnEnter = true;
		public bool unloadOnExit = true;

		private ContentHandler contentHandler;

		public SubStateMachine Owner => GetComponent<SubStateMachine>();

		private void Awake()
        {
			contentHandler = GetComponentInParent<ContentHandler>();
		}

        private void OnEnable()
        {
			contentHandler.onLanguageChanged.AddListener(OnLanguageChanged);
		}

        private void OnDisable()
        {
			contentHandler.onLanguageChanged.RemoveListener(OnLanguageChanged);
		}

        private void OnLanguageChanged(int id)
        {
			if (Owner.isBranchActive)
            {
				OnEnter(null);
			}
        }

		public void OnEnter(StateMachine fromState)
		{
			if (loadOnEnter)
			{
				Debug.Log("Resources Loaded on demand");

				var resourceHandler = GetComponentInParent<ResourceHanlder>();

				if (!resourceHandler.autoLoadResource)
				{
					List<ImageContent> images = new List<ImageContent>();
					List<VideoContent> videos = new List<VideoContent>();

					States.Utilities.GetChildren<ManageResourceExtension, ImageContent>(this, out images);
					States.Utilities.GetChildren<ManageResourceExtension, VideoContent>(this, out videos);

					foreach (var image in images)
						image.LoadResource();
					foreach (var video in videos)
						video.LoadResource();
				}
			}
		}

		public void OnExit(SubStateMachine toSubState)
		{
			if (unloadOnExit)
			{
				var resourceHandler = GetComponentInParent<ToucanApp.Data.ResourceHanlder>();

				if (!resourceHandler.autoLoadResource)
				{
					List<ImageContent> images = new List<ImageContent>();
					List<VideoContent> videos = new List<VideoContent>();

					States.Utilities.GetChildren<ManageResourceExtension, ImageContent>(this, out images);
					States.Utilities.GetChildren<ManageResourceExtension, VideoContent>(this, out videos);

					foreach (var image in images)
						image.UnloadResource();
					foreach (var video in videos)
						video.UnloadResource();

					//Resources.UnloadUnusedAssets();
				}
			}
		}
	}
}

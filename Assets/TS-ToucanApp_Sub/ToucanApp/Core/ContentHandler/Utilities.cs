using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ToucanApp.Data
{
	public static class Utilities 
	{
        public static T[] GetAllComponentsInChildren<T>(Transform transform, bool includeParent = false)  
        {
            List<T> childs = new List<T> ();

            if (includeParent)
            {
                var cs = transform.GetComponents<T>();
                if (cs != null)
                    childs.AddRange(cs);
            }

            GetAllComponentsInChildren<T> (ref childs, transform);

            return childs.ToArray ();
        }

        private static void GetAllComponentsInChildren<T> (ref List<T> childs, Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++) 
            {
                var childTrans = transform.transform.GetChild (i);
                var childComps = childTrans.GetComponents<T> ();
                if (childComps != null) 
                {
                    childs.AddRange(childComps);
                }

                GetAllComponentsInChildren<T> (ref childs, childTrans);
            }
        }

		public static T[] GetAllComponentsInChildren<P, T>(Transform owner)
		{
			List<T> childs = new List<T> ();

			GetAllComponentsInChildren<P, T> (owner, ref childs, owner);

			return childs.ToArray ();
		}

		private static void GetAllComponentsInChildren<P, T> (Transform owner, ref List<T> childs, Transform transform)
		{
			for (int i = 0; i < transform.childCount; i++) 
			{
				var childTrans = transform.transform.GetChild (i);
				var childComp = childTrans.GetComponents<T> ();
				if (childComp != null) 
				{
					var childOwner = GetComponentInParent<P>(childTrans) as Component;
                    if (childOwner != null && owner == childOwner.transform)
						childs.AddRange (childComp);
				}

				GetAllComponentsInChildren<P, T> (owner, ref childs, childTrans);
			}
		}

		public static T GetComponentInParent<T>(Transform transform)
		{
			if (transform.parent != null) 
			{
				var comp = transform.parent.GetComponent<T>();
                if (comp != null)
                    return comp;
                else
                    return GetComponentInParent<T>(transform.parent);
			}

			return default(T);
		}

        public static void WWWPost(MonoBehaviour owner, string address, string keyApi, WWWForm form, System.Action<UnityWebRequest> onDone, float timeOut = 10)
		{
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning("WWWRequest address is null or empty!");
            }

            owner.StartCoroutine(WWWPost(address, keyApi, form, onDone, timeOut));
		}

        public static int WWWTaskCount { get; set; }
        private static int wwwTasks;
        private static IEnumerator WWWPost(string address, string keyApi, WWWForm form, System.Action<UnityWebRequest> onDone, float timeOut = 10)
        {
            if (WWWTaskCount > 0)
            {
                while (wwwTasks == WWWTaskCount)
                {
                    yield return 0;
                }
                wwwTasks++;
            }

            UnityWebRequest www;
            www = UnityWebRequest.Post(address, form);

            www.SetRequestHeader("Authorization", keyApi);
            www.SetRequestHeader("Accept", "application/json");
            www.timeout = (int)timeOut;

            yield return www.SendWebRequest();
            onDone(www);

            if (WWWTaskCount > 0)
                wwwTasks--;
        }

        public static void WWWGet(MonoBehaviour owner, string address, string keyApi, System.Action<UnityWebRequest> onDone, float timeOut = 10, string savePath = "")
        {
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning("WWWRequest address is null or empty!");
            }

            owner.StartCoroutine(WWWGet(address, keyApi, onDone, timeOut, savePath));
        }

        static HashSet<string> paths = new HashSet<string>();

        private static IEnumerator WWWGet(string address, string keyApi, System.Action<UnityWebRequest> onDone, float timeOut, string savePath)
        {
            if (WWWTaskCount > 0)
            {
                while (wwwTasks == WWWTaskCount)
                {
                    yield return 0;
                }
                wwwTasks++;
            }

            UnityWebRequest www;

            bool downloadOnly = !string.IsNullOrEmpty(savePath);
            if (downloadOnly)
            {
                www = new UnityWebRequest(address);

                if (!paths.Contains(savePath))
                {
                    var dh = new DownloadHandlerFile(savePath);
                    dh.removeFileOnAbort = true;
                    www.downloadHandler = dh;
                    paths.Add(savePath);
                }
            }
            else
            {
                //Debug.Log("GetMultimediaRequest(address): " + address);
                www = GetMultimediaRequest(address);
            }

            www.SetRequestHeader("Authorization", keyApi);
            www.SetRequestHeader("Accept", "application/json");
            www.timeout = (int)timeOut;

            yield return www.SendWebRequest();
            //Debug.Log("bytes downloaded -> " + www.downloadedBytes + " -> " + www.url);

            onDone(www);

            if (WWWTaskCount > 0)
                wwwTasks--;
        }

        public static void WWWExists(MonoBehaviour owner, string address, string keyApi, System.Action<UnityWebRequest> onDone, float timeOut = 10)
        {
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning("WWWRequest address is null or empty!");
            }

            owner.StartCoroutine(WWWExists(address, keyApi, onDone, timeOut));
        }

        private static IEnumerator WWWExists(string address, string keyApi, System.Action<UnityWebRequest> onDone, float timeOut)
        {
            UnityWebRequest www = new UnityWebRequest(address); ;
            www.SetRequestHeader("Authorization", keyApi);
            www.SetRequestHeader("Accept", "application/json");
            www.timeout = (int)timeOut;

            yield return www.SendWebRequest();

            onDone(www);
        }

        public static IEnumerator WaitFrame(System.Action onDone)
		{
			yield return null;
			if (onDone != null)
				onDone.Invoke ();
		}

        public static void WaitUntil(MonoBehaviour owner, System.Func<bool> task, System.Action onDone)
        {
            owner.StartCoroutine (WaitUntil (task, onDone));
        }

        private static IEnumerator WaitUntil(System.Func<bool> task, System.Action onDone)
        {
            while (!task.Invoke())
            {
                yield return 0;
            }

            onDone.Invoke();
        }

        public static bool IsImage(string ext)
        {
            return (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp");
        }

        public static bool IsVideo(string ext)
        {
            return (ext == ".mov" || ext == ".mp4" || ext == ".3gp");
        }

        public static bool IsAudio(string ext)
        {
            return (ext == ".ogg" || ext == ".wav" || ext == ".mp3");
        }

        public static bool IsAsset(string ext)
        {
            return (ext == ".bin");
        }

        public static string GetAndroidPlatformLocalPath()
        {
            // Toucan Berry
            const string berryDataPath = "/data/toucan";
            if (Directory.Exists(berryDataPath))
                return berryDataPath;

            // Other Android Device
            return Application.persistentDataPath;
        }

        public static UnityWebRequest GetMultimediaRequest(string address)
        {
            string ext = address.Split('?')[0];
            ext = Path.GetExtension(ext);

            if (IsImage(ext))
                return UnityWebRequestTexture.GetTexture(address);
            else if (ext == ".ogg")
                return UnityWebRequestMultimedia.GetAudioClip(address, AudioType.OGGVORBIS);
            else if (ext == ".wav")
                return UnityWebRequestMultimedia.GetAudioClip(address, AudioType.WAV);
            else if (ext == ".mp3")
                return UnityWebRequestMultimedia.GetAudioClip(address, AudioType.MPEG);
            //else if (ext == ".bin")
            //    return UnityWebRequestAssetBundle.GetAssetBundle(address);
            else
                return UnityWebRequest.Get(address);
        }

		public static string ReplaceIllegal(string text)
		{
			string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
			Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
			text = r.Replace(text, "");

			return text;
		}

		public static IEnumerator TakeScreenShot(bool sceneView, bool transparent, System.Action<Texture2D> onDone) 
		{
			Camera cam = null;
			RenderTexture renderTexture = null;
			int width = 0;
			int height = 0;

            if (sceneView) 
			{
#if UNITY_EDITOR
                SceneView sw = SceneView.lastActiveSceneView;

				if (sw == null) 
				{
					Debug.LogError ("Unable to capture editor screenshot, no scene view found");
					yield break;
				}

				cam = sw.camera;
				renderTexture = cam.targetTexture;
				width = renderTexture.width;
				height = renderTexture.height;
#endif
            }
            else 
			{
				yield return new WaitForEndOfFrame();

				width = Screen.width;
				height = Screen.height;
			}

			var tex = new Texture2D( width, height, transparent ? TextureFormat.RGBA32 : TextureFormat.RGB24, false );

			RenderTexture.active = renderTexture;

			tex.ReadPixels( new Rect(0, 0, width, height), 0, 0 );
			tex.Apply();

			onDone.Invoke (tex);
		}
	}
}
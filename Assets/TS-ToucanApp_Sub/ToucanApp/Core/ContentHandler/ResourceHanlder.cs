using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.IO;

namespace ToucanApp.Data
{
    public interface IResourceReceiver
    {
        void OnResourceChanged();
    }

    public class ResourceInfo
    {
        public string Translation { private set; get; }
        public string FileHash { private set; get; }
        public UnityWebRequest www;

        public AudioClip audioData;
        public Texture2D textureData;
        public Sprite spriteData;
        public AssetBundle assetData;

        public bool isLoadedToMemory;
        public bool isSavedToDrive;
        public bool isLoadedFromDrive;
        public bool isReadonly;
        public bool loadToMemory;

        public Action callback;

        public int referenceCount;

        public void SetSources(string path, string hash)
        {
            Translation = path;
            FileHash = hash;
        }

		public Texture2D GetTexture()
		{
            if (!isLoadedToMemory)
                return null;

            try
            {
                if (textureData == null)
                    textureData = DownloadHandlerTexture.GetContent(www);
            }
            catch
            {
                Debug.LogError("Unsupported resource format! -> " + www.url);
            }


            return textureData;
		}

        public AudioClip GetAudioClip()
        {
            if (!isLoadedToMemory)
                return null;

            try
            {
                if (audioData == null)
                    audioData = DownloadHandlerAudioClip.GetContent(www);//(true, true);
            }
            catch
            {
                Debug.LogError("Unsupported resource format! -> " + www.url);
            }

            return audioData;
        }

        public Sprite GetSprite()
        {
            if (!isLoadedToMemory)
                return null;

            try
            {
                if (spriteData == null)
                {
                    var texture = DownloadHandlerTexture.GetContent(www);
                    if (texture != null)
                    {
                        texture.wrapMode = TextureWrapMode.Clamp;
                        spriteData = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
                    }
                }
            }
            catch
            {
                Debug.LogError("Unsupported resource format! -> " + www.url);
            }

            return spriteData;
        }

        public AssetBundle GetAsset()
        {
            if (!isLoadedToMemory)
                return null;

            if (assetData == null)
                assetData = AssetBundle.LoadFromMemory(www.downloadHandler.data);

            return assetData;
        }

        public void Dispose()
        {
            www.Dispose();
        }
    }

    [DefaultExecutionOrder(-3000)]
    public class ResourceHanlder : MonoBehaviour
    {
        [Serializable]
        public class ProgressEvent : UnityEvent<int> {}

        public bool dispose;
        public bool compress;
        public bool downloadAndSaveOnly = false;
        public bool autoLoadResource = true;
        public bool loadingLog = true;
        public int wwwTaskCount = 3;
        public int wwwTaskTimeOut = 200;

        [Tooltip("Turn off auto resource load if device memory is lower than indicated value. Resources must be loaded manually!")]
        public int memoryLimit = -1;


        [Header("Events")]
        public UnityEvent onWorkStart;
        public UnityEvent onWorkEnd;
        public ProgressEvent onProgressChanged;

        private Dictionary<string, ResourceInfo> cachedResources = new Dictionary<string, ResourceInfo>();
        private TaskGroupController taskGroup;

        public ContentHandler ContentHandler { get; private set; }
        public bool IsWorking { get { return taskGroup.TaskInProgress(0); } }
        public bool InitialResourceLoad { get; private set; } = true;

        private void Awake()
        {
            if (SystemInfo.systemMemorySize < memoryLimit)
            {
                Debug.LogWarning("Auto load resources option has been disabled by system!");
                autoLoadResource = false;
            }

            Utilities.WWWTaskCount = wwwTaskCount;

            ContentHandler = GetComponent<ContentHandler>();

            taskGroup = new TaskGroupController((int task) => {

                onWorkStart.Invoke();

            }, (int task) => {

                OnResourceChanged();
                onWorkEnd.Invoke();

            });

            StartCoroutine(ImportLoop());
        }

        private IEnumerator ImportLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(10f);

                if (!IsWorking && !ContentHandler.IsWorking)
                    TryLoadPendingResource();
            }
        }

        private void OnResourceChanged()
        {
            IResourceReceiver[] receivers = Utilities.GetAllComponentsInChildren<IResourceReceiver>(this.transform, true);
            foreach (var receiver in receivers)
            {
                receiver.OnResourceChanged();
            }

            SaveAllLoadedResource();

            if (dispose)
                DisposeAllWWW();

            if (InitialResourceLoad)
            {
                InitialResourceLoad = false;
                ContentHandler.ReloadLanguage();
            }
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                SaveAllLoadedResource();
            }
        }

        public void AddTask()
		{
			taskGroup.AddTask(0);
		}

		public void RemoveTask()
		{
			taskGroup.RemoveTask(0);
		}

        public void DisposeAllWWW()
        {
            foreach (var cr in cachedResources)
            {
                cr.Value.Dispose();
            }
        }

        public void SaveAllLoadedResource()
        {
            foreach (var cr in cachedResources)
            {
                Save(cr.Value);
            }
        }

        public void TryLoadPendingResource()
        {
            foreach (var cr in cachedResources)
            {
                TryLoadResource(cr.Value);
            }
        }

        public void Save(ResourceInfo resourceInfo)
        {
            if (!resourceInfo.isSavedToDrive)
            {
                if (resourceInfo.isLoadedToMemory && !resourceInfo.isLoadedFromDrive)
                {
                    string filepath = GetLocalPath(resourceInfo);
                    string ext = Path.GetExtension(filepath);

                    if (compress && Utilities.IsImage(ext))
                    {
                        Texture2D texture = null;

                        if (downloadAndSaveOnly || resourceInfo.textureData != null)
                        {
                            texture = resourceInfo.GetTexture();
                        }
                        else if (resourceInfo.spriteData != null)
                        {
                            texture = resourceInfo.GetSprite().texture;
                        }

                        if (texture)
                        {
                            const int MAX_RES = 1024;
                            if (texture.width > MAX_RES || texture.height > MAX_RES)
                            {
                                int max = Mathf.Max(texture.width, texture.height);
                                float scale = max / MAX_RES;

                                TextureScale.Bilinear(texture, (int)(texture.width / scale), (int)(texture.height / scale));
                            }

                            if (ext == ".png")
                                File.WriteAllBytes(filepath, texture.EncodeToPNG());
                            else
                                File.WriteAllBytes(filepath, texture.EncodeToJPG(45));
                        }
                    }
                    else
                    {
                        //if (ext != ".bin")
                        if (resourceInfo.www.isDone && resourceInfo.www.result == UnityWebRequest.Result.Success)
                            File.WriteAllBytes(filepath, resourceInfo.www.downloadHandler.data);
                    }
                }

                resourceInfo.isSavedToDrive = true;
                //resourceInfo.www.Dispose();
            }
        }

        public void Unload(ResourceInfo info)
        {
            if (info != null && !string.IsNullOrEmpty(info.Translation))
            {
                ResourceInfo found;
                cachedResources.TryGetValue(info.Translation, out found);
                if (found != null)
                {
                    cachedResources.Remove(info.Translation);

                    Destroy(found.textureData);
                    found.textureData = null;

                    Destroy(found.spriteData);
                    found.spriteData = null;

                    Destroy(found.audioData);
                    found.audioData = null;

                    Destroy(found.assetData);
                    found.assetData = null;
                }
            }
        }

        public void Load(ResourceData data, Action<ResourceInfo> onDone, int languageId = -1, bool loadToMemory = true)
        {
            if (languageId == -1)
                languageId = ContentHandler.LanguageID;

            CreateLoad(data, onDone, languageId, loadToMemory);
        }

        private void CreateLoad(ResourceData data, Action<ResourceInfo> onDone, int languageId, bool loadToMemory = true)
        {
            int ti = data.GetTranslationIdx(languageId);
            string translation = ti < data.translations.Length ? data.translations[ti] : null;
            string hash = ti < data.hash.Length ? data.hash[ti] : null;

            if (string.IsNullOrEmpty(translation))
            {
                if (onDone != null)
                    onDone.Invoke(new ResourceInfo());

                return;
            }

            if (string.IsNullOrEmpty(hash))
                hash = translation.GetHashCode().ToString();

            ResourceInfo resourceInfo;
            if (!cachedResources.TryGetValue(translation, out resourceInfo))
            {
                resourceInfo = new ResourceInfo();
                resourceInfo.SetSources(translation, hash);
                resourceInfo.loadToMemory = loadToMemory;

                if (onDone != null)
                    resourceInfo.callback += () => { onDone.Invoke(resourceInfo); };

                cachedResources.Add(translation, resourceInfo);
                TryLoadResource(resourceInfo);
            }
            else
            {
                if (onDone != null)
                {
                    if (resourceInfo.isLoadedToMemory || resourceInfo.isSavedToDrive)
                    {
                        onDone(resourceInfo);
                    }
                    else
                    {
                        resourceInfo.callback += () => { onDone.Invoke(resourceInfo); };
                        // Debug.LogWarning("Resource is already loading!");
                    }
                }
            }
        }

        private void TryLoadResource(ResourceInfo resourceInfo)
        {
            if (!resourceInfo.isLoadedToMemory && !resourceInfo.isSavedToDrive)
            {
                List<PathInfo> paths = new List<PathInfo>();

                // create valid paths
                PathInfo pathLocal = new PathInfo();
                pathLocal.description = "Local Write/Read";
                pathLocal.isReadonly = false;
                pathLocal.isLocal = true;
                pathLocal.loadToMemory = resourceInfo.loadToMemory;
                pathLocal.path = GetWWWLocalPath(resourceInfo);
                paths.Add(pathLocal);

                if (!string.IsNullOrEmpty(ContentHandler.WwwAndroidStreamingAssets))
                {
                    PathInfo pathMobileInner = new PathInfo();
                    pathMobileInner.description = "Local Mobile Read Only";
                    pathMobileInner.isReadonly = true;
                    pathMobileInner.isLocal = true;
                    pathMobileInner.loadToMemory = resourceInfo.loadToMemory;
                    pathMobileInner.path = GetWwwAndroidStreamingAssetsPath(resourceInfo);
                    paths.Add(pathMobileInner);
                }

                PathInfo pathServer = new PathInfo();
                pathServer.description = "Server Read Only";
                pathServer.isReadonly = false;
                pathServer.isLocal = false;
                pathServer.loadToMemory = resourceInfo.loadToMemory;
                pathServer.path = resourceInfo.Translation;
                paths.Add(pathServer);

                TryLoadFromPaths(resourceInfo, paths);
            }
        }

        private void TryToLoadFromNextPath(ResourceInfo resourceInfo, List<PathInfo> paths)
        {
            paths.RemoveAt(0);
            if (paths.Count > 0)
                TryLoadFromPaths(resourceInfo, paths);
            else
                Debug.LogError("Can't load -> " + resourceInfo.Translation);
        }

        private void TryLoadFromPaths(ResourceInfo resourceInfo, List<PathInfo> paths)
        {
            PathInfo pathInfo = paths[0];

            if (!pathInfo.isLocal && !ContentHandler.connectToServer)
            {
                Debug.LogWarning("Can't download resource -> '" + resourceInfo.Translation + "' downloading is disabled!");
                TryToLoadFromNextPath(resourceInfo, paths);
            }
            else
            {
                AddTask();
                if (pathInfo.loadToMemory) 
                {
                    // load file to memory
                    Utilities.WWWGet(this, pathInfo.path, ContentHandler.keyApi, (UnityWebRequest www) => { OnWebRequestEnd(www, resourceInfo, paths); }, wwwTaskTimeOut);
                }
                else if (!pathInfo.isLocal) 
                {
                    // download network file
                    Utilities.WWWGet(this, pathInfo.path, ContentHandler.keyApi, (UnityWebRequest www) => { OnWebRequestEnd(www, resourceInfo, paths); }, wwwTaskTimeOut, GetLocalPath(resourceInfo));
                }
                else
                {
                    // empty request without data
                    Utilities.WWWExists(this, pathInfo.path, ContentHandler.keyApi, (UnityWebRequest www) => { OnWebRequestEnd(www, resourceInfo, paths); }, 5);
                }
            }
        }

        private void OnWebRequestEnd(UnityWebRequest www, ResourceInfo resourceInfo, List<PathInfo> paths)
        {
            PathInfo pathInfo = paths[0];

            if (!www.isNetworkError && !www.isHttpError)
            {
                resourceInfo.isReadonly = pathInfo.isReadonly;
                resourceInfo.isLoadedToMemory = resourceInfo.loadToMemory;
                resourceInfo.isSavedToDrive = !resourceInfo.loadToMemory;
                resourceInfo.isLoadedFromDrive = pathInfo.isLocal;
                resourceInfo.www = www;

                if (!downloadAndSaveOnly)
                {
                    if (resourceInfo.callback != null)
                    {
                        resourceInfo.callback.Invoke();
                        resourceInfo.callback = null;
                    }
                }
                else
                {
                    Save(resourceInfo);
                    Unload(resourceInfo);
                }

                if (loadingLog)
                    Debug.Log("File -> " + pathInfo.path + " loaded from -> " + pathInfo.description + " | file is local -> " + pathInfo.isLocal);
            }
            else
            {
                TryToLoadFromNextPath(resourceInfo, paths);
            }

            RemoveTask();
            if (onProgressChanged != null)
                onProgressChanged.Invoke(taskGroup.TaskOperationsCount(0));
        }

        public string GetLocalPath(ResourceInfo resourceInfo)
        {
            // return ContentHandler.LocalAddress + resourceInfo.FileHash + Path.GetExtension(resourceInfo.Translation).ToLower(); NORMAL SOLUTION
            return ContentHandler.LocalAddress + "/" + resourceInfo.FileHash + Path.GetExtension(resourceInfo.Translation.Split('?')[0]).ToLower();
        }

        public string GetWWWLocalPath(ResourceInfo resourceInfo)
        {
            // return ContentHandler.WwwLocalAddress + resourceInfo.FileHash + Path.GetExtension(resourceInfo.Translation).ToLower(); NORMAL SOLUTION
            return ContentHandler.WwwLocalAddress + "/" + resourceInfo.FileHash + Path.GetExtension(resourceInfo.Translation.Split('?')[0]).ToLower();
        }

        public string GetWwwAndroidStreamingAssetsPath(ResourceInfo resourceInfo)
        {
            // return ContentHandler.WwwAndroidStreamingAssets + resourceInfo.FileHash + Path.GetExtension(resourceInfo.Translation).ToLower(); NORMAL SOLUTION
            return ContentHandler.WwwAndroidStreamingAssets + "/" + resourceInfo.FileHash + Path.GetExtension(resourceInfo.Translation.Split('?')[0]).ToLower();
        }

        public ResourceInfo GetResourceInfo(ResourceData data, int languageId = -1)
        {
            if (languageId == -1)
                languageId = ContentHandler.LanguageID;

            string translation = data.GetTranslation(languageId);

            ResourceInfo resourceInfo = null;

            if (!string.IsNullOrEmpty(translation))
                cachedResources.TryGetValue(translation, out resourceInfo);

            return resourceInfo;
        }

        public void Unload(ResourceData data)
        {
            for (int i = 0; i < ContentHandler.RootContent.LanguagesCount; i++)
            {
                string translation = data.GetTranslation(i);

                if (!string.IsNullOrEmpty(translation))
                {
                    ResourceInfo resourceInfo = null;
                    cachedResources.TryGetValue(translation, out resourceInfo);

                    Unload(resourceInfo);

                    cachedResources.Remove(translation);
                }
            }
        }

        public void SetCompress(bool value)
        {
            compress = value;
        }

        public void SetTimeOut(int timeOut)
        {
            wwwTaskTimeOut = timeOut;
        }

        public void SetTaskCount(int tasks)
        {
            wwwTaskCount = tasks;
        }
    }
}

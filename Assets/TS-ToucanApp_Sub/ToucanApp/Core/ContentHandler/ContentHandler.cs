using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.IO;
using System;

namespace ToucanApp.Data
{
    public class PathInfo
    {
        public string description;
        public string path;
        public bool isLocal;
        public bool isReadonly;
        public bool loadToMemory;
    }

    public interface IContentReceiver
    {
        void OnContentChanged();
    }
        
    public interface IContent : IContentReceiver
    {
        bool ShouldExport { get; set; }
        string OrginalID { get; }
        string ID { get ; set; }
        BaseData Data { set; get; }
        bool UsesRegistry { set; get; }
        bool IsClone { get; set; } // is cms clone
        bool IsCopy { get; set; } // is unity gameobject instance
        void OnExportData();
        void OnInitialize();
        void OnImportData();
        void OnLanguageChanged(int languageId);
    }

    public interface IContentParent : IContent
    {
        void ClearChildren();
    }

    [RequireComponent(typeof(ResourceHanlder))]
    [RequireComponent(typeof(RootContent))]
    [DefaultExecutionOrder(-3000)]
    public class ContentHandler : MonoBehaviour
    {
        [Serializable]
        public class LanguageEvent : UnityEvent<int> {}

        public const string CONTENT_FILENAME = "/content.json";
        public const string GET_API = "/api/apps/{0}/versions/{1}/unity";

#if CMS_2
        public const string ACTIVE_API = "/api/get-project-json/{0}";
# else
        public const string ACTIVE_API = "/api/apps/{0}/versions/active";
#endif

        public bool activateOnStart = true;
        public bool connectToServer = true;
        public bool contentUpdateCycle = true;
        public string appName;
        public string serverAddress = "http://nest2.toucan.systems:3000/";
        public string keyApi = "Bearer Onot0S0UDwZSnNj1JEmxhb6TbS78HKWpQWkPuUQwkiH8vPSrpNfS3AbDbbWUszuWpVOTWsfMDJSFs9Rm";

        [Header("Events")]
        public UnityEvent onWorkStart;
        public UnityEvent onWorkEnd;
        public LanguageEvent onLanguageChanged;

        private TaskGroupController taskGroup;
        private List<IContent> currentContent = new List<IContent>();
        private List<string> exportedIds = new List<string>();

        public ResourceHanlder Resources { get; private set; }
        public RootContent RootContent { get; private set; }
        public string LocalAddress { get; private set; }
        public string WwwLocalAddress { get; private set; }
        public string WwwLocalFilePrefix { get; private set; }
        public string WwwAndroidStreamingAssets { get; private set; }
        public DataBuilder CurrentBuilder { get; private set; }
        public bool IsDataImported { get; private set; }
        public bool SaveImportedData { get; private set; }
        public int LanguageID { get; set; }
        public int ContentVersion { get; set; }
        public bool IsWorking { get { return taskGroup.TaskInProgress(0); } }

        public string AppID
        {
            get
            {
                if (string.IsNullOrEmpty(appName))
                    return Application.productName.Replace(" ", "");

                return appName.Replace(" ", "");
            }
        }

        public string AppRelativePath
        {
            get
            {
                if (!string.IsNullOrEmpty(appName))
                    return "/ToucanApp/" + appName;

                return "/ToucanApp";
            }
        }

        private int currentDataHash;

        private void Awake()
        {
            Setup();

			taskGroup = new TaskGroupController((int task) => {

				onWorkStart.Invoke();

			}, (int task) => {
				
				onWorkEnd.Invoke();
                OnContentChanged();

            });
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                InitializeContent(RootContent.transform);

                if (activateOnStart)
                {
                    ImportData();
                    if (!Application.isMobilePlatform && contentUpdateCycle)
                        StartCoroutine(ImportLoop());
                }
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

        private IEnumerator ImportLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (!IsWorking && !Resources.IsWorking)
                    ImportData();
            }
        }

        public void Setup()
        {
            LanguageID = 0;

            RootContent = GetComponent<RootContent>();
            Resources = GetComponent<ResourceHanlder>();

            WwwLocalFilePrefix = "file://";

            if (!Application.isMobilePlatform || Application.isEditor)
                LocalAddress = Application.streamingAssetsPath + AppRelativePath;
            else if (Application.platform == RuntimePlatform.Android)
                LocalAddress = Utilities.GetAndroidPlatformLocalPath() + AppRelativePath; // Application.persistentDataPath + AppRelativePath;
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                LocalAddress = Application.persistentDataPath + AppRelativePath;

            if (!Application.isEditor)
            {
                if (Application.platform == RuntimePlatform.Android)
                    WwwAndroidStreamingAssets = "jar:" + WwwLocalFilePrefix + Application.dataPath + "!/assets" + AppRelativePath;
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                    WwwAndroidStreamingAssets = "jar:" + WwwLocalFilePrefix + Application.dataPath + "/Raw" + AppRelativePath;
            }

            WwwLocalAddress = WwwLocalFilePrefix + LocalAddress;
        }

        private void ClearPopulatedContent()
        {
            IContentParent[] contentParents = Utilities.GetAllComponentsInChildren<IContentParent>(this.transform, true);
            foreach (var parentContent in contentParents)
            {
                parentContent.ClearChildren();
            }
        }

        public void ChangeLanguageNext()
        {
            var id = LanguageID + 1;

            if (id >= RootContent.LanguagesCount)
                id = 0;

            ChangeLanguage(id);
        }

        public void ChangeLanguage(int id)
        {
            if (LanguageID != id)
            {
                LanguageID = id;
                ReloadLanguage();
            }
        }

        public void ReloadLanguage()
        {
            Resources.AddTask();

            if (currentContent != null)
            {
                foreach (var c in currentContent)
                {
                    c.OnLanguageChanged(LanguageID);
                }
            }

            if (onLanguageChanged != null)
                onLanguageChanged.Invoke(LanguageID);

            Resources.RemoveTask();
        }

        private int[] GetLanguageArray()
        {
            List<int> temp = new List<int>();

            for (int i = 0; i < RootContent.LanguagesCount; i++)
            {
                if (i != LanguageID)
                    temp.Add(i);
            }

            temp.Add(LanguageID);
            return temp.ToArray();
        }

        public static string SaveData(BaseData[] data, string directory, string filename)
        {
            var json = LitJson.JsonMapper.ToJson(data);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            System.IO.File.WriteAllText(directory + filename, json);

            return json;
        }

        public void LoadData(System.Action<PathInfo, string> onDone)
        {
            var paths = new Queue<PathInfo>();

            if (connectToServer)
            {
                if (ContentVersion > 0)
                {
                    var pathServer = new PathInfo();
                    pathServer.path = serverAddress + string.Format(GET_API, AppID, ContentVersion);
                    pathServer.isLocal = false;
                    pathServer.isReadonly = true;
                    pathServer.description = "Server content - custom version";
                    paths.Enqueue(pathServer);
                }
                else
                {
                    var pathServer = new PathInfo();
                    pathServer.path = serverAddress + string.Format(ACTIVE_API, AppID);
                    pathServer.isLocal = false;
                    pathServer.isReadonly = true;
                    pathServer.description = "Server content - current version";
                    paths.Enqueue(pathServer);
                }
            }

            var pathLocal = new PathInfo();
            pathLocal.path = WwwLocalAddress + CONTENT_FILENAME;
            pathLocal.isLocal = true;
            pathLocal.isReadonly = false;
            pathLocal.description = "Local content - streaming assets";
            paths.Enqueue(pathLocal);

            if (WwwAndroidStreamingAssets != null)
            {
                var pathMobile = new PathInfo();
                pathMobile.path = WwwAndroidStreamingAssets + CONTENT_FILENAME;
                pathMobile.isLocal = true;
                pathMobile.isReadonly = true;
                pathMobile.description = "Local content - inside jar";
                paths.Enqueue(pathMobile);
            }

            TryLoadDataPath(paths, onDone);
        }

        private void TryLoadDataPath(Queue<PathInfo> paths, System.Action<PathInfo, string> onDone)
        {
            if (paths.Count == 0)
            {
                Debug.LogError("Failed to load data file!");
                return;
            }

            var pathInfo = paths.Dequeue();
            Utilities.WWWGet(this, pathInfo.path, keyApi, (UnityWebRequest www) => {

                if (!www.isNetworkError && !www.isHttpError)
                {
                    onDone(pathInfo, www.downloadHandler.text);
                }
                else
                {
					Debug.LogError(www.error + " -> " + www.url);
                    TryLoadDataPath(paths, onDone);
                }

            });
        }

        public void ImportData(bool force = false)
        {
            LoadData((PathInfo pathInfo, string fileData) => {

                var loadeDataHash = fileData.GetHashCode();
                if (force || currentDataHash != loadeDataHash)
				{
                    AddTask();

                    try
                    {
                        currentDataHash = loadeDataHash;

	                    IsDataImported = false;
                        var builder = DataBuilder.FromJson(fileData);
                        CurrentBuilder = builder;

                        if (force)
                        {
                            ClearPopulatedContent();
                        }

                        PopulateContent(builder, RootContent);

                        IsDataImported = true;
                        SaveImportedData = !pathInfo.isLocal;

                        Debug.Log("<color=green>Data imported.</color>" + " | " + pathInfo.description);

                        var languages = GetLanguageArray();

	                    Resources.AddTask();

	                    foreach (var c in currentContent)
	                    {
	                        foreach (var langId in languages)
	                        {
	                            c.OnLanguageChanged(langId);
	                        }
	                    }

					    Resources.RemoveTask();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Data not imported. -> " + e.Message);
                    }

                    RemoveTask();
                }
            });
        }

        private void OnContentChanged()
        {
            IContentReceiver[] receivers = Utilities.GetAllComponentsInChildren<IContentReceiver>(this.transform, true);
            foreach (var receiver in receivers)
            {
                receiver.OnContentChanged();
            }

            if (IsDataImported && SaveImportedData)
            {
                SaveImportedData = false;
                SaveData(DataBuilder.FromTree(RootContent.Data).ToArray(), LocalAddress, CONTENT_FILENAME);
            }
        }

        public bool ExportData()
        {
            if (Application.isPlaying)
            {
                if (!IsDataImported)
                {
                    Debug.LogError("Data re-export failed -> data was not imported.");
                    return false;
                }
            }

            Setup();
            ApplyContent(RootContent);

            var builder = DataBuilder.FromTree(RootContent.Data);
            BaseData[] errorsList = builder.CheckStructure();
            if (errorsList.Length > 0)
            {
                LogIncorrectStructure(builder, RootContent, errorsList);
				Debug.LogError("Data not exported.");
                return false;
            }
            else
            {
                SaveData(builder.ToArray(), LocalAddress, CONTENT_FILENAME);
				Debug.Log("<color=green>Data exported.</color>");
                return true;
            }
        }

        private void InitializeContent(Transform parent)
        {
            foreach (var item in parent.GetComponentsInChildren<IContent>(true))
                item.OnInitialize();
        }

        public void PopulateContent(DataBuilder builder, IContent parentContent)
        {
            currentContent.Clear();
            PopulateContentNested(builder, parentContent);
        }

        private void PopulateContentNested(DataBuilder builder, IContent parentContent)
        {
            var monoParent = (MonoBehaviour)parentContent;

            var dataFound = builder.FindData(parentContent.ID);
            if (dataFound != null)
                parentContent.Data = dataFound;

            currentContent.Add(parentContent);
            parentContent.OnImportData();

            var contentChildren = Utilities.GetAllComponentsInChildren<IContent, IContent>(monoParent.transform);

            for (int j = 0; j < contentChildren.Length; j++)
            {
                PopulateContentNested(builder, contentChildren[j]);
            }
        }

        public void ApplyContent(IContent parentContent)
        {
            exportedIds.Clear();
            currentContent.Clear();

            ApplyContentNested(parentContent);
        }

        private void ApplyContentNested(IContent parentContent)
        {
            var monoParent = (MonoBehaviour)parentContent;

            if (parentContent.ShouldExport && (string.IsNullOrEmpty(parentContent.ID) || !exportedIds.Contains(parentContent.ID)))
            {
                parentContent.Data.ClearUnusedData();
                parentContent.OnExportData();

                if (!string.IsNullOrEmpty(parentContent.ID))
                {
                    exportedIds.Add(parentContent.ID);
                    currentContent.Add(parentContent);

                    var contentChildren = Utilities.GetAllComponentsInChildren<IContent, IContent>(monoParent.transform);
                    for (int j = 0; j < contentChildren.Length; j++)
                    {
                        ApplyContentNested(contentChildren[j]);
                    }
                }
            }
        }

        public IContent CloneContentBlock(BaseData dataChildren, IContent content, Transform parent)
        {
            var prototypeMono = (MonoBehaviour)content;
            var cloneMono = Instantiate<MonoBehaviour>(prototypeMono, parent);

            cloneMono.transform.localScale = prototypeMono.transform.localScale;
            cloneMono.transform.localRotation = prototypeMono.transform.localRotation;
            cloneMono.transform.localPosition = prototypeMono.transform.localPosition;

            InitializeContent(cloneMono.transform);
            // change content id of cloned content
            PrepareClonedContentData(dataChildren, cloneMono.transform);

            return (IContent)cloneMono;
        }

        private IContent[] PrepareClonedContentData(BaseData dataChild, Transform target)
        {
            IContent[] cloneHierarchy = Utilities.GetAllComponentsInChildren<IContent>(target, true);
            foreach (var c in cloneHierarchy)
            {
                if (c.ID != null)
                {
                    var cloneData = BaseData.FindData<BaseData>(dataChild, (BaseData candidate) => { return (candidate.originalID == c.OrginalID); });
                    if (cloneData != null)
                    {
                        c.ID = cloneData.id;
                        c.Data = cloneData;
                    }
                    else
                    {
                        var dataFound = CurrentBuilder.FindData(c.ID);
                        if (dataFound != null)
                        {
                            c.ID = dataFound.id;
                            c.Data = dataFound;
                        }
                        else
                        {
                            c.Data.originalID = c.ID;
                            c.ID = BaseData.GetRandomID();
                            c.Data.id = c.ID;
                        }
                    }

                    c.IsCopy = true;
                }
            }

            return cloneHierarchy;
        }

        public IContent CloneContentBlockCustom(IContent content, Transform parent)
        {
            var prototypeMono = (MonoBehaviour)content;
            var cloneMono = Instantiate<MonoBehaviour>(prototypeMono, parent);

            cloneMono.transform.localScale = prototypeMono.transform.localScale;
            cloneMono.transform.localRotation = prototypeMono.transform.localRotation;
            cloneMono.transform.localPosition = prototypeMono.transform.localPosition;

            InitializeContent(cloneMono.transform);
            // change content id of cloned content
            PrepareClonedContentDataCustom(cloneMono.transform);

            return (IContent)cloneMono;
        }

        private IContent[] PrepareClonedContentDataCustom(Transform target)
        {
            IContent[] cloneHierarchy = Utilities.GetAllComponentsInChildren<IContent>(target, true);
            foreach (var c in cloneHierarchy)
            {
                if (c.ID != null)
                {
                    if (!c.IsCopy)
                    {
                        c.Data.originalID = c.ID;
                        c.IsCopy = true;
                    }
                    else
                    {
                        c.Data.originalID = c.OrginalID;
                    }

                    c.ID = BaseData.GetRandomID();
                    c.Data.id = c.ID;
                }
            }

            return cloneHierarchy;
        }

        public T GetContentWithID<T>(string id) where T : class, IContent
        {
            foreach (var c in currentContent)
            {
                var cot = c as T;
                if (cot != null && c.ID == id)
                {
                    return cot;
                }
            }

            return default(T);
        }

        public void SetKeyApi(string key)
        {
            keyApi = key;
        }

        public void SetAdditionalName(string name)
        {
            appName = name;
        }

        public void SetServerAddres(string address)
        {
            serverAddress = address;
        }

        public void SetCmsConnection(bool enable)
        {
            connectToServer = enable;
        }

        public void MarkForContentReimport()
        {
            currentDataHash = -currentDataHash;
        }

        public static void LogIncorrectStructure(DataBuilder builder, IContent root, BaseData[] errorsList)
        {
            var rootMono = ((MonoBehaviour)root);
            var tempContent = Utilities.GetAllComponentsInChildren<IContent>(rootMono.transform, true);
            IContent errorContent = null;

            foreach (var errorData in errorsList)
            {
                foreach (var c in tempContent)
                {
                    if (c.ID == errorData.id)
                    {
                        errorContent = c;

                        var path = "";
                        var resolution = "";
                        BaseData temp = errorData;
                        while (temp != null)
                        {
                            path = path.Insert(0, temp.objectName + "/");
                            temp = builder.FindData(temp.parentID);
                        }

                        if (errorData is IRootDataChild)
                            resolution += typeof(IRootDataChild).ToString() + ", ";
                        if (errorData is IHierarchyDataChild)
                            resolution += typeof(IHierarchyDataChild).ToString() + ", ";
                        if (errorData is ITableDataChild)
                            resolution += typeof(ITableDataChild).ToString() + ", ";
                        if (errorData is IGaleryDataChild)
                            resolution +=  typeof(IGaleryDataChild).ToString() + ", ";
                        if (errorData is IArticleDataChild)
                            resolution +=  typeof(IArticleDataChild).ToString() + ", ";
                        if (errorData is IArticleDataChild)
                            resolution += typeof(IArticleDataChild).ToString() + ", ";

                        resolution = resolution.Substring(0, resolution.Length - 2);
                        Debug.LogError("Wrong parent data type -> '" + path + "'. should be -> " + resolution, errorContent as MonoBehaviour);
                    }
                }
            }
        }
    }
}

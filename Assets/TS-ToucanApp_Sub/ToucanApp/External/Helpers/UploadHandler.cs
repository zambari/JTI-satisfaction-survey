using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#if !UNITY_EDITOR_OSX
using ICSharpCode.SharpZipLib.Zip;
#endif
#endif

namespace ToucanApp.Data
{
    [System.Serializable]
    public class DataUploadInfo
    {
        public string path;
        public string checksum;
    }

    [System.Serializable]
    public class AppInfoArray
    {
        public AppInfo[] data;
    }

    [System.Serializable]
    public class AppInfo
    {
        public int id = -1;
        public string name;
        public string app_id;
    }

    [System.Serializable]
    public class VersionInfoArray
    {
        public VersionInfo[] data;
    }

    [System.Serializable]
    public class VersionInfo
    {
        public int version;
        public string build_version;
        public string comment;
        public int id = -1;
        public int app_id;
        public bool is_active;
    }

    public class UploadHandler : MonoBehaviour
    {
        private const string CMS_TYPE = "CMSToucan";
        private const int MAX_MESSAGES_QUEUE_COUNT = 25;
        public const string LOG_API = "/api/log";
        public const string ADD_API = "/api/apps";
        public const string UPLOAD_API = "/api/apps/{0}/versions";
        public const string LIST_API = "/api/apps";
        public const string VERSION_API = "/api/apps/{0}/versions";
        public const string FORCE_VERSION_API = "/api/apps/{0}/versions/{1}?is_active=1";
        public const string UPLOAD_RESOURCE_API = "/api/media-libraries/unity/file";

        public string serverAddress = "http://20.20.1.112:8000";
        public string keyApi = "Bearer iys7Hv2ZZSjoohVAnw8Zy0beo9nItySdQ5z1tQuQqtvYsXrld25zqmPZe0dNKNnGKkrX1inFne0OlFBi";

        [HideInInspector]
        public AppInfo ServerAppInfo;

        private Queue<string> messageQueue = new Queue<string>(MAX_MESSAGES_QUEUE_COUNT);

        public string ZipFile { get { return Application.persistentDataPath + "/" + ContentHandler.appName + ".zip"; } }
        public string ContentFile { get { return Application.streamingAssetsPath + ContentHandler.AppRelativePath + ContentHandler.CONTENT_FILENAME; } }

        public bool PackageAwaiting { get { return File.Exists(ZipFile); } }
        public bool ContentAwaiting { get { return File.Exists(ContentFile); } }

        public bool IsConnected { get; private set; }
        public bool IsBussy { get; private set; }

        public ContentHandler ContentHandler { get { return GetComponentInParent<ContentHandler>(); } }

        public void UploadLogMessage(string log)
        {
            if (!Application.isPlaying)
                return;

            messageQueue.Enqueue(log);
        }

        private void Update()
        {
            // upload log message
            if (Application.isPlaying && IsConnected && !IsBussy && messageQueue.Count > 0)
            {
                var form = new WWWForm();
                form.AddField("message", messageQueue.Peek());

                IsBussy = true;
                Utilities.WWWPost(this, serverAddress + LOG_API, keyApi, form, (UnityWebRequest www) =>
                {

                    if (!www.isNetworkError && !www.isHttpError)
                        messageQueue.Dequeue();

                    IsBussy = false;

                });
            }
        }

#if UNITY_EDITOR

        public void CreatePackage(bool includeBuild)
        {
#if !UNITY_EDITOR_OSX
            Debug.Log("Preparing package...");

            if (!ContentAwaiting)
            {
                Debug.LogError("Packing failed -> Content not exported!");
                return;
            }

            IsBussy = true;

            if (PackageAwaiting)
                File.Delete(ZipFile);

            var contentFile = ContentFile;
            var zip = ICSharpCode.SharpZipLib.Zip.ZipFile.Create(ZipFile);
            zip.BeginUpdate();

            if (includeBuild)
            {
                var buildDir = Application.persistentDataPath + "/temp";
                var bpo = new BuildPlayerOptions();
                bpo.target = EditorUserBuildSettings.activeBuildTarget;
                if (bpo.target == BuildTarget.StandaloneWindows || bpo.target == BuildTarget.StandaloneWindows64)
                {
                    bpo.locationPathName = buildDir + "/start.exe";
                }
                else if (bpo.target == BuildTarget.Android)
                {
                    bpo.options = BuildOptions.Development;
                    bpo.locationPathName = buildDir + "/start.apk";
                }
                else
                {
                    Debug.LogError("Build target not supported!");
                    IsBussy = false;
                    return;
                }

                BuildPipeline.BuildPlayer(bpo);

                if (Directory.Exists(buildDir))
                {
                    var files = Directory.GetFiles(buildDir, "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        zip.Add(file, file.Remove(0, buildDir.Length + 1));
                    }
                }
            }

            zip.Add(contentFile, ContentHandler.CONTENT_FILENAME);

            zip.CommitUpdate();
            zip.Close();

            Debug.Log("<color=green>Package ready.</color>");
            IsBussy = false;
#else
            Debug.LogWarning("Not available on OSX");
#endif
        }

        public void UploadPackage(Texture2D cover)
        {
#if !UNITY_EDITOR_OSX
            Debug.Log("Uploading package...");

            if (!PackageAwaiting)
            {
                Debug.LogError("Upload failed -> Package not created!");
                return;
            }

            IsBussy = true;

            var appName = ContentHandler.AppID;
            var zipBytes = File.ReadAllBytes(ZipFile);
            var form = new WWWForm();

            form.AddField("name", appName);
            form.AddField("cms", CMS_TYPE);
            form.AddBinaryData("file", zipBytes, "app.zip");
            form.AddBinaryData("cover", cover.EncodeToJPG(), appName + ".jpg");

            string request;
            if (ServerAppInfo.id == -1)
                request = serverAddress + ADD_API; 
            else
                request = serverAddress + string.Format(UPLOAD_API, ServerAppInfo.id);

            Debug.Log("Upload Package Request: " + request);

            Utilities.WWWPost(this, request, keyApi, form, (UnityWebRequest www) => {

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log("Upload failed -> " + www.downloadHandler.text);
                    Debug.LogError("Upload failed -> " + www.error);
                    IsBussy = false;
                    return;
                }
                else
                {
                    if (string.IsNullOrEmpty(www.downloadHandler.text))
                        Debug.LogWarning(www.downloadHandler.text);
                    else
                        Debug.Log("<color=green>Package uploaded.</color>");
                }

                File.Delete(ZipFile);
                IsBussy = false;

            }, 200);
#else
            Debug.LogWarning("Not available on OSX");
#endif
        }

        public void UploadData(byte[] data, string file, string directory, System.Action<DataUploadInfo> onDone)
        {
            Debug.Log("Uploading Texture...");

            var form = new WWWForm();
            form.AddField("directory", directory);
            form.AddBinaryData("file", data, file);

            Utilities.WWWPost(this, serverAddress + UPLOAD_RESOURCE_API, keyApi, form, (UnityWebRequest www) => {

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log("Upload failed -> " + www.downloadHandler.text);
                    Debug.LogError("Upload failed -> " + www.error);
                    IsBussy = false;
                    return;
                }
                else
                {
                    try
                    {
                        var uploadData = LitJson.JsonMapper.ToObject<DataUploadInfo>(www.downloadHandler.text);
                        onDone.Invoke(uploadData);
                    }
                    catch
                    {
                        Debug.Log("Upload failed -> " + www.downloadHandler.text);
                        Debug.LogError("Upload failed -> " + file);

                        onDone.Invoke(null);
                    }
                }

            }, 200);
        }

        public void RequestApps(System.Action onDone)
        {

#if CMS_2
            return;
#endif

            IsBussy = true;

            Utilities.WWWGet(this, serverAddress + LIST_API, keyApi, (UnityWebRequest www) => {

                if (www.isNetworkError || www.isHttpError)
                {
                    if (!string.IsNullOrEmpty(www.downloadHandler.text))
                        Debug.LogError(www.downloadHandler.text);

                    IsConnected = false;
                }
                else
                {
                    //Debug.Log(www.downloadHandler.text);

                    var applications = LitJson.JsonMapper.ToObject<AppInfoArray>(www.downloadHandler.text);
                    foreach (var info in applications.data)
                    {
                        ServerAppInfo = new AppInfo();
                        if (info.app_id == ContentHandler.AppID)
                        {
                            ServerAppInfo = info;
                            break;
                        }
                    }

                    IsConnected = true;
                }

                IsBussy = false;

                if (onDone != null)
                    onDone.Invoke();

            });
        }
#endif

        public void RequestVersions(System.Action<VersionInfo[]> onDone)
        {
            IsBussy = true;
            var temp = new List<VersionInfo>();

            if (ServerAppInfo.id > -1)
            {
                Utilities.WWWGet(this, serverAddress + string.Format(VERSION_API, ServerAppInfo.id), keyApi, (UnityWebRequest www) =>
                {
                    if (www.isNetworkError || www.isHttpError)
                    {
                        if (!string.IsNullOrEmpty(www.downloadHandler.text))
                            Debug.LogError(www.downloadHandler.text);
                    }
                    else
                    {
                        Debug.Log(www.downloadHandler.text);
                        var versions = LitJson.JsonMapper.ToObject<VersionInfoArray>(www.downloadHandler.text);
                        foreach (var version in versions.data)
                        {
                            if (version.app_id == ServerAppInfo.id)
                            {
                                temp.Add(version);
                            }
                        }
                    }

                    IsBussy = false;

                    if (onDone != null)
                        onDone.Invoke(temp.ToArray());

                });
            }
        }

        public void ForceSwicthServerContentVersion(int contentVersionId, System.Action onDone)
        {
            IsBussy = true;

            if (ServerAppInfo.id > -1)
            {
                Utilities.WWWPost(this, serverAddress + string.Format(FORCE_VERSION_API, ServerAppInfo.id, contentVersionId), keyApi, null, (UnityWebRequest www) =>
                {
                    if (www.isNetworkError || www.isHttpError)
                    {
                        if (!string.IsNullOrEmpty(www.downloadHandler.text))
                            Debug.LogError(www.downloadHandler.text);
                    }
                    else
                    {
                        if (onDone != null)
                            onDone.Invoke();

                        Debug.Log("<color=green>Server content version changed.</color>");
                    }

                    IsBussy = false;
                });
            }
        }
    }
}
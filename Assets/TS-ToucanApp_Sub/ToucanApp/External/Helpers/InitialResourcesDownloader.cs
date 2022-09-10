using UnityEngine;
using ToucanApp.Data;
using ToucanApp.Helpers;

public class InitialResourcesDownloader : MonoBehaviour, IResourceReceiver
{
    private const string REG_VAR_NAME = "/AppInitialResourcesDownloaded{0}";

    private ResourceHanlder resourceHandler;
    private ContentHandler contentHandler;
    private LoadingController loadingController;

    private bool hasInitialResources = false;

    private void Awake()
    {
        resourceHandler = GetComponentInParent<ResourceHanlder>();
        contentHandler = GetComponentInParent<ContentHandler>();
        loadingController = GetComponentInParent<LoadingController>();
    }

    private void OnEnable()
    {
        if (loadingController != null)
            loadingController.enabled = true;

#if !UNITY_EDITOR
        hasInitialResources = PlayerPrefs.GetInt(REG_VAR_NAME + contentHandler.appName, 0) > 0;
#endif

        if (!hasInitialResources)
        {
            resourceHandler.downloadAndSaveOnly = true;
            resourceHandler.compress = true;
            resourceHandler.autoLoadResource = true;
            resourceHandler.wwwTaskTimeOut = 60;
            resourceHandler.wwwTaskCount = 1;
        }
    }

    public void OnResourceChanged()
    {
        if (!hasInitialResources && enabled)
        {
#if !UNITY_EDITOR
            PlayerPrefs.SetInt(REG_VAR_NAME + contentHandler.appName, 1);

            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("finish");
#else
            UnityEditor.EditorApplication.isPlaying = false;
#endif

            Debug.LogWarning("InitialResourcesDownloader is enabled - Quitting Application");
        }
    }
}

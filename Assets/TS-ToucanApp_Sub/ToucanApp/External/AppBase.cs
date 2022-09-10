using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ToucanApp.Data;
using ToucanApp.States;
using ToucanApp.Config;
using ToucanApp.Helpers;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
#endif

namespace ToucanApp
{
    public class AppBase
    {
        private const string serverAddress = "https://nest3.toucan.systems";
        private const string keyApi = "Bearer Onot0S0UDwZSnNj1JEmxhb6TbS78HKWpQWkPuUQwkiH8vPSrpNfS3AbDbbWUszuWpVOTWsfMDJSFs9Rm";

#if UNITY_EDITOR
        [MenuItem("Toucan App/Tests/Create NEST test", false, 203)]
        private static void GenerateTest()
        {
            var mainState = AddComponentOfType<CanvasSubState>(Selection.activeTransform);
            mainState.gameObject.AddComponent<StateContent>();

            GenerateTestBaseContent(mainState.transform);

            AddComponentOfType<StateContent>(mainState.transform);
            AddComponentOfType<StateContent>(mainState.transform);
            AddComponentOfType<StateContent>(mainState.transform);

            var simpleTable = AddComponentOfType<TableContent>(mainState.transform);
            var simpleRow = AddComponentOfType<RowContent>(simpleTable.transform);
            GenerateTestBaseContent(simpleRow.transform);

            AddComponentOfType<StateLinkContent>(mainState.transform).linkedSubstate = mainState;
            AddComponentOfType<TableLinkContent>(mainState.transform).linkedTable = simpleTable;

            ContentTools.GenerateID(Selection.activeTransform);
        }

        private static void GenerateTestBaseContent(Transform parent)
        {
            AddComponentOfType<TextContent>(parent);
            AddComponentOfType<TextShortContent>(parent);
            AddComponentOfType<ImageContent>(parent);
            AddComponentOfType<VideoContent>(parent);
            AddComponentOfType<AudioContent>(parent);
            AddComponentOfType<DoubleContent>(parent);
            AddComponentOfType<IntContent>(parent);
            AddComponentOfType<BoolContent>(parent);
            AddComponentOfType<ColorContent>(parent);
            AddComponentOfType<URLContent>(parent);
            AddComponentOfType<MapMarkerContent>(parent);
            AddComponentOfType<AssetContent>(parent);

            var imgGalery = AddComponentOfType<GaleryContent>(parent);
            AddComponentOfType<ImageContent>(imgGalery.transform);
            AddComponentOfType<ImageContent>(imgGalery.transform);
            AddComponentOfType<ImageContent>(imgGalery.transform);

            var videoGalery = AddComponentOfType<GaleryContent>(parent);
            AddComponentOfType<VideoContent>(videoGalery.transform);
            AddComponentOfType<VideoContent>(videoGalery.transform);
            AddComponentOfType<VideoContent>(videoGalery.transform);

            var article = AddComponentOfType<ArticleContent>(parent);
            AddComponentOfType<ImageContent>(article.transform);
            AddComponentOfType<TextContent>(article.transform);
            AddComponentOfType<TextShortContent>(article.transform);
            AddComponentOfType<VideoContent>(article.transform);
        }

        [MenuItem("GameObject/UI/Canvas Substate")]
        [MenuItem("Toucan App/UI/Create Canvas Substate", false, 202)]
        private static void CreateSubState()
        {
            var go = new GameObject("#SubState");
            Undo.RegisterCreatedObjectUndo(go, "created sub state");
            go.AddComponent<CanvasSubState>();
            go.AddComponent<CanvasStateVisualization>();
            var rect = go.GetComponent<RectTransform>();
            go.transform.SetParent(Selection.activeTransform);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector3.zero;
            Selection.activeGameObject = go;
        }

        [MenuItem("Toucan App/UI/Replace UiText with SmartText", false, 201)]
		private static void ReplaceToSmartText()
		{

			var texts = GameObject.FindObjectsOfType<Text> ();
			//Undo.RecordObjects(texts, "Replaced Text to SmartText");
			MonoScript yourReplacementScript;

			for (int i = 0; i < texts.Length; i++) 
			{
				SerializedObject so = new SerializedObject(texts[i]);
				SerializedProperty scriptProperty = so.FindProperty("m_Script");
				so.Update();

				var tmpGO = new GameObject("tempOBJ");
				var inst = tmpGO.AddComponent<SmartText>();
				yourReplacementScript = MonoScript.FromMonoBehaviour(inst);

				scriptProperty.objectReferenceValue = yourReplacementScript;
				so.ApplyModifiedProperties();

				GameObject.DestroyImmediate(tmpGO);
			}
		}

        [MenuItem("Toucan App/Create standard application", false, 1)]
        private static void GenerateStandard()
        {
            var contentHandler = GameObject.FindObjectOfType<ContentHandler>();
            if (contentHandler == null)
            {
                var app = new GameObject("@AppBase");
                Undo.RegisterCreatedObjectUndo(app, "Created StandardApp");

                var eventSystem = GameObject.FindObjectOfType<EventSystem>();
				if (eventSystem == null) 
				{
					var eventSysGo = new GameObject ("EventSystem");
					eventSystem = eventSysGo.AddComponent<EventSystem> ();
					eventSysGo.AddComponent<StandaloneInputModule> ();
				}

				contentHandler = app.AddComponent<ContentHandler>();
                contentHandler.serverAddress = serverAddress;
                contentHandler.keyApi = keyApi;

                var root = app.GetComponent<RootContent>();
                root.ID = "ROOT";

                var resourceHandler = app.GetComponent<ResourceHanlder>();

                var uploadHandler = app.AddComponent<UploadHandler> ();
                uploadHandler.serverAddress = serverAddress;
                uploadHandler.keyApi = keyApi;

                var config = app.AddComponent<UniversalConfig>();
                config.pathRelativeTo = UniversalConfig.PathRelativeTo.StreammingAssets;

                var asset = ScriptableObject.CreateInstance<ToucanAppSettings>();
                if (!AssetDatabase.IsValidFolder("Assets/ToucanAppSettings"))
                    AssetDatabase.CreateFolder("Assets", "ToucanAppSettings");
                AssetDatabase.CreateAsset(asset, string.Format("Assets/ToucanAppSettings/{0}_Settings.asset", Path.GetFileNameWithoutExtension(Path.GetRandomFileName())));

                var appHandler = app.AddComponent<AppHandler>();
                appHandler.settings = asset;

                var helpers = new GameObject("AppTools");
                helpers.transform.SetParent(app.transform);
                var resolutionC = helpers.AddComponent<ResolutionController>();
                var screensaverC = helpers.AddComponent<ScreensaverController>();
				var loadingScreen = helpers.AddComponent<LoadingController>();

                //var runtimeTools = helpers.AddComponent<RuntimeTools>();
                var runtimeConsole = helpers.AddComponent<RuntimeConsole>();

                helpers.AddComponent<Logger>();
                helpers.AddComponent<ContentHelpers>();

                var canvasGo = new GameObject("Canvas");
                canvasGo.transform.SetParent(app.transform);
                var canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler= canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasGo.AddComponent<GraphicRaycaster>();
                var canvasApp = canvasGo.AddComponent<CanvasApp>();
                var mainState = new GameObject("#MainState");
                canvasApp.DefaultState = mainState.AddComponent<CanvasSubState>();
                mainState.AddComponent<CanvasStateVisualization>();
                var rect = mainState.GetComponent<RectTransform>();
                mainState.AddComponent<StateContent>();
                rect.SetParent(canvasGo.transform);
                rect.localScale = Vector3.one;
                rect.localPosition = Vector3.zero;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector3.zero;

                AddConfigPersistentListener(config, "keyApi", new UnityAction<string>(contentHandler.SetKeyApi), contentHandler.keyApi);
                AddConfigPersistentListener(config, "AppName", new UnityAction<string>(contentHandler.SetAdditionalName), contentHandler.appName);
                AddConfigPersistentListener(config, "UseLoadingScreen", new UnityAction<bool>(loadingScreen.SetActive), loadingScreen.enabled);
                AddConfigPersistentListener(config, "UseRuntimeConsole", new UnityAction<bool>(runtimeConsole.SetActive), runtimeConsole.Active);
                //AddConfigPersistentListener(config, "UseRuntimeTools", new UnityAction<bool>(runtimeTools.SetRuntimeToolsActive), runtimeTools.enabled);
                AddConfigPersistentListener(config, "ConnectToCMS", new UnityAction<bool>(contentHandler.SetCmsConnection), contentHandler.connectToServer);
                AddConfigPersistentListener(config, "CMSAddress", new UnityAction<string>(contentHandler.SetServerAddres), contentHandler.serverAddress);
                AddConfigPersistentListener(config, "DownloadTimeOut", new UnityAction<int>(resourceHandler.SetTimeOut), resourceHandler.wwwTaskTimeOut);
                AddConfigPersistentListener(config, "DownloadTasksCount", new UnityAction<int>(resourceHandler.SetTaskCount), resourceHandler.wwwTaskCount);
                AddConfigPersistentListener(config, "CompressResources", new UnityAction<bool>(resourceHandler.SetCompress), resourceHandler.compress);
                AddConfigPersistentListener(config, "ResolutionX", new UnityAction<int>(resolutionC.SetResolutionX), -1);
				AddConfigPersistentListener(config, "ResolutionY", new UnityAction<int>(resolutionC.SetResolutionY), -1);
				AddConfigPersistentListener(config, "ScreensaverTimer", new UnityAction<float>(screensaverC.SetScreensaverTimer), 120f);
				AddConfigPersistentListener(config, "ScreenDimDuration", new UnityAction<float>(screensaverC.SetDimDurration), 5f);
            }
        }

        [MenuItem("Assets/Create/ToucanApp ID Registry", false, 3002)]
        public static void GenerateRegister()
        {
            string path = "Assets";
            var filteredSelection = Selection.GetFiltered<Object>(SelectionMode.Assets);

            if (filteredSelection.Length > 0)
            {
                var selected = filteredSelection[0];

                path = AssetDatabase.GetAssetPath(selected);
                if (File.Exists(path))
                    path = Path.GetDirectoryName(path);

                var asset = ScriptableObject.CreateInstance<IDRegistry>();
                string assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + "IDRegistry.asset");
                AssetDatabase.CreateAsset(asset, assetPath);
                asset.RefreshID();
            }
        }


        [MenuItem("Assets/Create/ToucanApp Settings", false, 3001)]
        public static void GenerateCmsSettings()
        {
            string path = "Assets";
            var filteredSelection = Selection.GetFiltered<Object>(SelectionMode.Assets);

            if (filteredSelection.Length > 0)
            {
                var selected = filteredSelection[0];

                path = AssetDatabase.GetAssetPath(selected);
                if (File.Exists(path))
                    path = Path.GetDirectoryName(path);

                var asset = ScriptableObject.CreateInstance<ToucanAppSettings>();
                string assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + "AppSettings.asset");
                AssetDatabase.CreateAsset(asset, assetPath);
            }
        }

        //[MenuItem("Toucan App/Tests/Update application", false, 204)]
        //private static void FixSettings()
        //{
        //    var contentHandler = GameObject.FindObjectOfType<ContentHandler>();
        //    if (contentHandler != null)
        //    {
        //        var resourceHandler = contentHandler.GetComponent<ResourceHanlder>();
        //        var config = contentHandler.GetComponent<UniversalConfig>();

        //        StringConfigElement keyElement = null;
        //        if (!config.GetConfigElement<StringConfigElement>("keyApi", ref keyElement))
        //        {
        //            AddConfigPersistentListener(config, "keyApi", new UnityAction<string>(contentHandler.SetKeyApi), contentHandler.keyApi);
        //            AddConfigPersistentListener(config, "DownloadTimeOut", new UnityAction<int>(resourceHandler.SetTimeOut), resourceHandler.wwwTaskTimeOut);
        //            AddConfigPersistentListener(config, "DownloadTasksCount", new UnityAction<int>(resourceHandler.SetTaskCount), resourceHandler.wwwTaskCount);
        //            AddConfigPersistentListener(config, "CompressResources", new UnityAction<bool>(resourceHandler.SetCompress), resourceHandler.compress);

        //            EditorSceneManager.MarkSceneDirty(resourceHandler.gameObject.scene);
        //            Debug.Log("Successfully Fixed!");
        //        }
        //        else
        //        {
        //            Debug.Log("Already Fixed.");
        //        }
        //    }
        //}

        private static T AddComponentOfType<T>(Transform parent) where T : Component
        {
            var go = new GameObject(typeof(T).Name);
            var comp = go.AddComponent<T>();

            var rect = go.GetComponent<RectTransform>();
            if (rect == null)
                rect = go.AddComponent<RectTransform>();

            rect.SetParent(parent);
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;

            return comp;
        }

        public static void AddConfigPersistentListener(UniversalConfig config, string name, UnityAction<string> action, string defaultValue = "")
		{
			StringConfigElement configElement = null;
			config.AddNewConfigValue(name, defaultValue);
			config.GetConfigElement(name, ref configElement);
			UnityEventTools.AddPersistentListener(configElement.OnElementLoaded, action);
		}

		public static void AddConfigPersistentListener(UniversalConfig config, string name, UnityAction<bool> action, bool defaultValue = false)
		{
			BoolConfigElement configElement = null;
			config.AddNewConfigValue(name, defaultValue);
			config.GetConfigElement(name, ref configElement);
			UnityEventTools.AddPersistentListener(configElement.OnElementLoaded, action);
		}

		public static void AddConfigPersistentListener(UniversalConfig config, string name, UnityAction<int> action, int defaultValue = 0)
		{
			IntConfigElement configElement = null;
			config.AddNewConfigValue(name, defaultValue);
			config.GetConfigElement(name, ref configElement);
			UnityEventTools.AddPersistentListener(configElement.OnElementLoaded, action);
		}

		public static void AddConfigPersistentListener(UniversalConfig config, string name, UnityAction<float> action, float defaultValue = 0)
		{
			FloatConfigElement configElement = null;
			config.AddNewConfigValue(name, defaultValue);
			config.GetConfigElement(name, ref configElement);
			UnityEventTools.AddPersistentListener(configElement.OnElementLoaded, action);
		}

        [InitializeOnLoadAttribute]
        public class AppBaseInit
        {
            static AppBaseInit()
            {
                if (!Directory.Exists(Application.streamingAssetsPath + "/ToucanApp/"))
                {
                    Directory.CreateDirectory(Application.streamingAssetsPath + "/ToucanApp/");
                }
            }
        }
#endif      
    }
}

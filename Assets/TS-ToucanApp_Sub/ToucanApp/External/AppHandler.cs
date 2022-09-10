using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using ToucanApp.Config;
using ToucanApp.Data;
using ToucanApp.States;

namespace ToucanApp
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RootContent))]
    [RequireComponent(typeof(ContentHandler))]
    [RequireComponent(typeof(ResourceHanlder))]
    [RequireComponent(typeof(UploadHandler))]
    [RequireComponent(typeof(UniversalConfig))]
    public class AppHandler : MonoBehaviour, IContentReceiver
    {
        //[SerializeField, ConditionalHide("advanced", true)]
        //private string serverAddress = "https://nest3.toucan.systems";
        //public string ServerAddress { get { return serverAddress; } }

        //[SerializeField, ConditionalHide("advanced", true)]
        //private string keyApi = "Bearer Onot0S0UDwZSnNj1JEmxhb6TbS78HKWpQWkPuUQwkiH8vPSrpNfS3AbDbbWUszuWpVOTWsfMDJSFs9Rm";
        //public string KeyApi { get { return keyApi; } }

        //[SerializeField, ConditionalHide("advanced", true)]
        //private string logApi = "/api/log";
        //public string LogApi { get { return logApi; } }

        //[SerializeField, ConditionalHide("advanced", true)]
        //private string addApi = "/api/apps";
        //public string AddApi { get { return addApi; } }

        //[SerializeField, ConditionalHide("advanced", true)]
        //private string updateApi = "/api/apps/{0}/versions";
        //public string UpdateApi { get { return updateApi; } }

        //[SerializeField, ConditionalHide("advanced", true)]
        //private string listApi = "/api/apps";
        //public string ListApi { get { return listApi; } }

        public ToucanAppSettings settings;

        [SerializeField, HideInInspector]
        private RootContent rootConent;
        public RootContent RootConent { get { return rootConent; } }

        [SerializeField, HideInInspector]
        private ContentHandler contentHandler;
        public ContentHandler ContentHandler { get { return contentHandler; } }

        [SerializeField, HideInInspector]
        private ResourceHanlder resourceHanlder;
        public ResourceHanlder ResourceHanlder { get { return resourceHanlder; } }

        [SerializeField, HideInInspector]
        private UploadHandler uploadHanlder;
        public UploadHandler UploadHandler { get { return uploadHanlder; } }

        [SerializeField, HideInInspector]
        private UniversalConfig universalConfig;
        public UniversalConfig UniversalConfig { get { return universalConfig; } }

        private void Awake()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            rootConent = GetComponent<RootContent>();
            contentHandler = GetComponent<ContentHandler>();
            resourceHanlder = GetComponent<ResourceHanlder>();
            uploadHanlder = GetComponent<UploadHandler>();
            universalConfig = GetComponent<UniversalConfig>();
        }

        public string GetVersion()
        {
            return settings.buildVersion;
        }

        public string IncrVersion()
        {
            settings.buildVersion = System.Guid.NewGuid().ToString();
            return settings.buildVersion;
        }

        public void AssignVersion()
        {
            // increase json build version
            var json = System.IO.File.ReadAllText(Application.streamingAssetsPath + contentHandler.AppRelativePath + ContentHandler.CONTENT_FILENAME);
            var dataBuilder = DataBuilder.FromJson(json);
            var dataArray = dataBuilder.ToArray();
            var rootData = (RootData)dataArray[0];

            rootData.buildVersion = settings.buildVersion;
            ContentHandler.SaveData(dataArray, Application.streamingAssetsPath + contentHandler.AppRelativePath, ContentHandler.CONTENT_FILENAME);
        }

        public void OnContentChanged()
        {

        }
    }
}

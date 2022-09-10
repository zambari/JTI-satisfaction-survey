using System;
using UnityEngine.UI;
using UnityEngine;
using ToucanApp.Data;
using ToucanApp.States;

namespace ToucanApp.Helpers
{
    public class VersionController : MonoBehaviour, IContentReceiver
    {
        [SerializeField, Tooltip("Load's latest content version instead of published (Editor only)")]
        private bool useLatestContent = false;

        private bool active = true;
        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
                screenGo.SetActive(value);

                if (value)
                    GenerateContentButtons();
            }
        }

        private AppHandler appHandler;
        private string buildVersion;

        private GameObject screenGo;
        private VersionInfo selectedVersion;
        private Text selectedVersionText;

        private void Start()
        {
            appHandler = GetComponentInParent<AppHandler>();
            buildVersion = appHandler.GetVersion();

            var canvasApp = appHandler.GetComponentInChildren<CanvasApp>();
            screenGo = new GameObject("_VERSION_");

            var screen = screenGo.AddComponent<CanvasGroup>();
            screen.transform.SetParent(canvasApp.transform);
            screen.transform.SetAsLastSibling();

            var screenImg = screenGo.AddComponent<Image>();
            screenImg.color = new Color(0, 0, 0, .8f);

            screenImg.rectTransform.localPosition = Vector3.zero;
            screenImg.rectTransform.pivot = new Vector2(.5f, 1);
            screenImg.rectTransform.anchorMin = Vector2.zero;
            screenImg.rectTransform.anchorMax = Vector2.one;
            screenImg.rectTransform.localScale = Vector3.one;

            var versionLayout = screenGo.AddComponent<VerticalLayoutGroup>();
            versionLayout.spacing = 25;
            versionLayout.padding.left = 75;
            versionLayout.padding.right = 75;
            versionLayout.padding.top = 50;
            versionLayout.padding.bottom = 50;
            versionLayout.childForceExpandHeight = false;

            var versionSizeFitter = screenGo.AddComponent<ContentSizeFitter>();
            versionSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateText("Available" + System.Environment.NewLine + "content versions", 50, Color.yellow, screenGo.transform);

            Active = false;
        }

        private void GenerateContentButtons()
        {
            var oldButtons = screenGo.GetComponentsInChildren<Button>();

            appHandler.UploadHandler.RequestVersions((VersionInfo[] versions) =>
            {
                foreach (var button in oldButtons)
                {
                    Destroy(button.gameObject);
                }

                Array.Reverse(versions);

                foreach (var version in versions)
                {
                    if (Application.isEditor || string.Equals(version.build_version, buildVersion))
                    {
                        string buttonText = "<size=40>version : " + version.version + "</size>";
                        if (version.is_active || !string.IsNullOrEmpty(version.comment))
                        {
                            buttonText += Environment.NewLine + "<size=20>";

                            if (version.is_active)
                            {
                                buttonText += "<color=yellow>Published</color>";
                            }

                            if (!string.IsNullOrEmpty(version.comment))
                            {
                                if (version.is_active)
                                    buttonText += " ";

                                buttonText += "(" + version.comment + ")";
                            }

                            buttonText += "</size>";
                        }

                        var versionText = CreateText(buttonText, 15, Color.gray, screenGo.transform);
                        var versionButton = versionText.gameObject.AddComponent<Button>();

                        if ((selectedVersion == null && version.is_active) || (selectedVersion != null && selectedVersion.id == version.id))
                        {
                            selectedVersion = version;
                            selectedVersionText = versionText;
                        }

                        versionButton.onClick.AddListener(() =>
                        {

                            if (selectedVersion != null)
                                selectedVersionText.color = Color.gray;

                            selectedVersion = version;
                            selectedVersionText = versionText;
                            selectedVersionText.color = Color.white;

                            appHandler.ContentHandler.ContentVersion = version.id;
                            appHandler.ContentHandler.ImportData(true);
                            var canvasApp = appHandler.GetComponentInChildren<CanvasApp>();
                            if (canvasApp)
                            {
                                canvasApp.EnterDefaultState();
                            }
                            //Active = false;

                        });

                        if (version == selectedVersion)
                            versionText.color = Color.white;

                        versionText.rectTransform.anchorMin = Vector2.zero;
                        versionText.rectTransform.anchorMax = Vector2.one;
                        versionText.rectTransform.localScale = Vector3.one;
                        versionText.rectTransform.localPosition = Vector3.zero;
                    }
                }

                if (Application.isEditor)
                {
                    var switchVersionText = CreateText("(Mark current content version as published)", 40, Color.white, screenGo.transform);
                    var switchVersionButton = switchVersionText.gameObject.AddComponent<Button>();

                    switchVersionButton.onClick.AddListener(() => {

                        appHandler.UploadHandler.ForceSwicthServerContentVersion(appHandler.ContentHandler.ContentVersion, () => {

                            GenerateContentButtons();

                        });

                    });
                }

            });
        }

        private Text CreateText(string text, int size, Color color, Transform parent)
        {
            var outputGo = new GameObject("Text");
            outputGo.transform.SetParent(parent);
            outputGo.AddComponent<RectTransform>();

            var output = outputGo.AddComponent<Text>();
            output.font = (Font)Resources.Load<Font>("muli");
            output.color = color;
            output.text = text;
            output.fontSize = size;
            output.horizontalOverflow = HorizontalWrapMode.Overflow;
            output.alignment = TextAnchor.MiddleCenter;

            return output;
        }

        public void OnContentChanged()
        {
            if (useLatestContent && !Active && Application.isEditor && selectedVersion == null)
            {
                appHandler.UploadHandler.RequestVersions((VersionInfo[] versions) => {

                    if (versions != null && versions.Length > 0)
                    {
                        selectedVersion = versions[versions.Length - 1];

                        appHandler.ContentHandler.ContentVersion = selectedVersion.id;
                        appHandler.ContentHandler.ImportData();
                    }

                });
            }
        }
    }
}

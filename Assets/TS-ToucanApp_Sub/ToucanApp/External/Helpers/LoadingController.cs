using System.Collections;
using System.Collections.Generic;
using ToucanApp.States;
using ToucanApp.Data;
using ToucanApp.Config;
using UnityEngine.UI;
using UnityEngine;

namespace ToucanApp.Helpers
{
    public class LoadingController : MonoBehaviour
    {
        public enum LoadingType
        {
            CircleProgress,
            CircleDots
        }

        public enum BgColor
        {
            White,
            Black,
        }

        //public string loadingInfo = "LOADING RESOURCES" + System.Environment.NewLine;

        private CanvasGroup screen;
        private Image loadingImg;
        private AspectRatioFitter loadingAspect;
        private Image loadingCircle;
        private Text loadingText;
        private int initialTaskCount = -1;

        private Sprite dotSprite;
        private List<Image> dots;

        [Header("Body Settings")]
        [SerializeField]
        private Sprite loadingBgSprite = null;

        [SerializeField]
        private BgColor bgColor;

        [SerializeField]
        private LoadingType loadingType = LoadingType.CircleProgress;

        [Header("Events")]
        [SerializeField]
        private StandardEvent onLoadingEnd = null;

        // required for config setup
        public void SetActive(bool state)
        {
            enabled = state;
            if (!state)
                Hide();
        }

        private void Awake()
        {
            var resourceHandler = GetComponentInParent<ResourceHanlder>();
            if (!resourceHandler.autoLoadResource)
                enabled = true;

            resourceHandler.onWorkStart.AddListener(Show);
            resourceHandler.onProgressChanged.AddListener(OnProgressChanged);
            resourceHandler.onWorkEnd.AddListener(Hide);
        }

        private float animationTime;
        private void Update()
        {
            if (dots != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    float dotRad = -(Mathf.PI * 2f) * (i / 12f) * Mathf.Rad2Deg;
                    float normalizedSize = 1 - Mathf.Clamp(Mathf.Abs(Mathf.DeltaAngle(animationTime, dotRad)) / 90f, 0, 1);
                    dots[i].transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.3f, normalizedSize);
                    dots[i].color = Color.Lerp(new Color(1, 1, 1, .5f), Color.white, normalizedSize);
                }

                animationTime += Time.deltaTime * 150;
            }
        }

        public void Show()
        {
            if (!enabled)
                return;

            Debug.Log("LoadingController Show");

            if (screen == null)
                CreateScreen();

            if (isActiveAndEnabled)
            {
                initialTaskCount = -1;
                loadingText.text = "0%";
                loadingCircle.fillAmount = 0f;

                screen.alpha = 1f;
                screen.gameObject.SetActive(true);
            }
        }

        public void OnProgressChanged(int tasksLeft)
        {
            if (screen == null)
                return;

            if (initialTaskCount == -1)
                initialTaskCount = tasksLeft;

            float progress = 1 - (tasksLeft / (float)initialTaskCount);
            if (float.IsNaN(progress))
                progress = 1;

            loadingText.text = Mathf.RoundToInt(progress * 100).ToString() + "%";
            loadingCircle.fillAmount = Mathf.Max(progress, loadingCircle.fillAmount);
        }

        public void Hide()
        {
            if (screen == null)
                return;

            Debug.Log("LoadingController Hide");

            if (screen.gameObject.activeSelf)
            {
                screen.alpha = 0f;
                screen.gameObject.SetActive(false);
                onLoadingEnd.Invoke();
            }
        }

        //private IEnumerator HideFade()
        //{
        //    if (screen.gameObject.activeSelf)
        //    {
        //        if (onLoadingEnd != null)
        //            onLoadingEnd.Invoke();

        //        yield return new WaitForSeconds(.25f);

        //        for (float i = 1; i > 0; i -= Time.deltaTime * 2)
        //        {
        //            screen.alpha = i;
        //            yield return 0;
        //        }

        //        screen.gameObject.SetActive(false);
        //    }
        //}

        private void CreateScreen()
        {
            dotSprite = Resources.Load<Sprite>("dot");

            var canvasApp = FindObjectOfType<CanvasApp>();

            var screenGo = new GameObject("_LOADING_");
            screen = screenGo.AddComponent<CanvasGroup>();
            screen.transform.SetParent(canvasApp.transform);
            screen.transform.SetAsLastSibling();

            CreateBg(screenGo);

            if (loadingType == LoadingType.CircleProgress)
            {
                CreateLoadingCircleBg(screenGo);
                CreateLoadingCircle(screenGo);
            }
            else
            {
                CreateLoadingCircleDots(screenGo);
            }

            CreateText(screenGo);

            screen.alpha = 0f;
            screen.gameObject.SetActive(false);
        }

        private void CreateBg(GameObject screen)
        {
            loadingImg = screen.AddComponent<Image>();

            if (loadingBgSprite == null)
            {
                loadingImg.color = bgColor == BgColor.White ? Color.white : Color.black;
            }
            else
            {
                loadingImg.sprite = loadingBgSprite;
                loadingAspect = screen.AddComponent<AspectRatioFitter>();
                loadingAspect.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                loadingAspect.aspectRatio = loadingBgSprite.texture.width / (float)loadingBgSprite.texture.height;
            }

            var rect = screen.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
        }

        private void CreateLoadingCircleBg(GameObject screen)
        {
            var go = new GameObject("LoadingCircleBg");
            go.transform.SetParent(screen.transform);

            loadingCircle = go.AddComponent<Image>();
            loadingCircle.sprite = Resources.Load<Sprite>("circle2");
            loadingCircle.color  = bgColor == BgColor.White ? new Color(0f, 0f, 0f, .5f) : new Color(1f, 1f, 1f, .5f);
            loadingCircle.SetNativeSize();

            var rect = go.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
        }

        private void CreateLoadingCircle(GameObject screen)
        {
            var go = new GameObject("LoadingCircle");
            go.transform.SetParent(screen.transform);

            loadingCircle = go.AddComponent<Image>();
            loadingCircle.sprite = Resources.Load<Sprite>("circle");
            loadingCircle.color = bgColor == BgColor.White ? new Color(0f, 0f, 0f, .5f) : new Color(1f, 1f, 1f, .5f);
            loadingCircle.fillOrigin = 2;
            loadingCircle.type = Image.Type.Filled;
            loadingCircle.fillAmount = .01f;
            loadingCircle.SetNativeSize();

            var rect = go.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
        }

        private void CreateLoadingCircleDots(GameObject screen)
        {
            dots = new List<Image>();

            for (int i = 0; i < 12; i++)
            {
                float dotRad = (Mathf.PI * 2f) * (i / 12f);
                var dot = CreateDot(dotRad, screen);
                dots.Add(dot);
            }
        }

        private Image CreateDot(float degRad, GameObject screen)
        {
            var go = new GameObject("Dot");
            go.transform.SetParent(screen.transform);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(Mathf.Cos(degRad), Mathf.Sin(degRad), 0) * 80;

            loadingCircle = go.AddComponent<Image>();
            loadingCircle.sprite = dotSprite;
            loadingCircle.SetNativeSize();

            return loadingCircle;
        }

        private void CreateText(GameObject screen)
        {
            var go = new GameObject("LoadingText");
            go.transform.SetParent(screen.transform);

            loadingText = go.AddComponent<Text>();
            loadingText.color = bgColor == BgColor.White ? new Color(0f, 0f, 0f, .5f) : new Color(1f, 1f, 1f, .5f);
            loadingText.font = (Font)Resources.Load<Font>("muli");
            loadingText.text = "0%";
            loadingText.fontSize = 28;
            loadingText.horizontalOverflow = HorizontalWrapMode.Overflow;
            loadingText.alignment = TextAnchor.MiddleCenter;

            var rect = go.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
        }
    }

    public static class ResourceHandlerExtesnion
    {
        public static void SilentLoad(this ResourceHanlder handler, ResourceData data, System.Action<ResourceInfo> onDone, int languageId = -1, bool loadToMemory = true)
        {
            var loading = handler.GetComponentInChildren<LoadingController>();
            if (loading)
            {
                loading.enabled = false;
                handler.Load(data, resourceInfo => {

                    loading.enabled = true;
                    if (onDone != null)
                        onDone.Invoke(resourceInfo);

                }, languageId, loadToMemory);
            }
        }
    }
}

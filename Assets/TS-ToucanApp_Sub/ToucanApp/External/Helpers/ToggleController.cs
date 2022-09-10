using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Toggle))]
public class ToggleController : MonoBehaviour
{
    private class FadeElement
    {
        public Graphic graphic = null;
        public CanvasGroup canvasGroup = null;
        public GameObject gameObject = null;

        public Coroutine fadeCor = null;

        public FadeElement(GameObject gameObject)
        {
            graphic = gameObject.GetComponent<Graphic>();
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            this.gameObject = gameObject;
        }

        public IEnumerator FadeCor(float duration, bool fadeIn)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float startAlpha = canvasGroup.alpha;
            float endAlpha = 0;

            if (fadeIn)
                endAlpha = 1;

            for (float i = 0; i < duration; i += Time.deltaTime)
            {
                float step = i / duration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, step);
                yield return null;
            }
            canvasGroup.alpha = endAlpha;

            if (fadeIn)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            yield return null;
            fadeCor = null;
        }
    }

    [Header("References")]
    [SerializeField, Tooltip("Elements that will be shown when Toggle is on. Elements that have either Graphic, CanvasGroup or just GameObject.")]
    private GameObject[] activeElements = null;

    [SerializeField, Tooltip("Elements that will be shown when Toggle is off. Elements that have either Graphic, CanvasGroup or just GameObject.")]
    private GameObject[] inactiveElements = null;

    [SerializeField]
    private Graphic[] commonElements = null;

    [Header("Settings")]
    [SerializeField, Range(0, 4)]
    private float fadeTime = 0.5f;

    [SerializeField, Range(0, 1), Tooltip("For common elements")]
    private float inactiveAlpha = 0.5f;

    [SerializeField, Range(0, 1), Tooltip("For common elements")]
    private float activeAlpha = 1;

    [SerializeField, Tooltip("Sets active/inactive elements according to current Toggle status.")]
    private bool prepareOnStart = true;

    [SerializeField, Tooltip("Should events be invoked on start?")]
    private bool invokeEventOnStart = false;

    [SerializeField, Tooltip("Events won't be called if this option is set to true")]
    private bool suppressEvents = false;
    public bool SuppressEvents
    {
        get { return suppressEvents; }
        set { suppressEvents = value; }
    }

    [Header("Events")]
    public UnityEvent OnToggleActivate;
    public UnityEvent OnToggleDeactivate;

    public Toggle Toggle { get; private set; } = null;
    private List<FadeElement> activeFadeElements = new List<FadeElement>();
    private List<FadeElement> inactiveFadeElements = new List<FadeElement>();

    private void Start()
    {
        foreach (GameObject entry in activeElements)
            activeFadeElements.Add(new FadeElement(entry));

        foreach (GameObject entry in inactiveElements)
            inactiveFadeElements.Add(new FadeElement(entry));

        Toggle = GetComponent<Toggle>();
        Toggle.onValueChanged.AddListener(OnStateChanged);

        if (prepareOnStart)
        {
            if (Toggle.isOn)
            {
                foreach (FadeElement entry in activeFadeElements)
                    FadeInElement(entry, 0);

                foreach (FadeElement entry in inactiveFadeElements)
                    FadeOutElement(entry, 0);

                foreach (Graphic entry in commonElements)
                    entry.CrossFadeAlpha(activeAlpha, fadeTime, true);

                if (invokeEventOnStart && !suppressEvents && OnToggleActivate != null)
                    OnToggleActivate.Invoke();
            }
            else
            {
                foreach (FadeElement entry in activeFadeElements)
                    FadeOutElement(entry, 0);

                foreach (FadeElement entry in inactiveFadeElements)
                    FadeInElement(entry, 0);

                foreach (Graphic entry in commonElements)
                    entry.CrossFadeAlpha(inactiveAlpha, fadeTime, true);

                if (invokeEventOnStart && !suppressEvents && OnToggleDeactivate != null)
                    OnToggleDeactivate.Invoke();
            }
        }
    }

    private void OnEnable()
    {
        if (Toggle == null)
            return;

        float previousFadeTime = fadeTime;
        fadeTime = 0;
        bool previousSuppressEvents = suppressEvents;
        suppressEvents = true;

        OnStateChanged(Toggle.isOn);

        fadeTime = previousFadeTime;
        suppressEvents = previousSuppressEvents;
    }

    private void FadeInElement(FadeElement element, float duration)
    {
        if (element.graphic != null)
        {
            element.graphic.CrossFadeAlpha(1, duration, false);
            return;
        }

        if (element.canvasGroup != null)
        {
            if (element.fadeCor != null)
            {
                StopCoroutine(element.fadeCor);
                element.fadeCor = null;
            }

            element.fadeCor = StartCoroutine(element.FadeCor(duration, true));
            return;
        }

        if (element.gameObject != null)
        {
            element.gameObject.SetActive(true);
            return;
        }
    }

    private void FadeOutElement(FadeElement element, float duration)
    {
        if (element.graphic != null)
        {
            element.graphic.CrossFadeAlpha(0, duration, false);
            return;
        }

        if (element.canvasGroup != null)
        {
            if (element.fadeCor != null)
            {
                StopCoroutine(element.fadeCor);
                element.fadeCor = null;
            }

            element.fadeCor = StartCoroutine(element.FadeCor(duration, false));
            return;
        }

        if (element.gameObject != null)
        {
            element.gameObject.SetActive(false);
            return;
        }
    }

    private void OnStateChanged(bool newState)
    {
        if (newState)
        {
            foreach (FadeElement entry in activeFadeElements)
                FadeInElement(entry, fadeTime);

            foreach (FadeElement entry in inactiveFadeElements)
                FadeOutElement(entry, fadeTime);

            foreach (Graphic entry in commonElements)
                entry.CrossFadeAlpha(activeAlpha, fadeTime, true);

            if (!suppressEvents && OnToggleActivate != null)
                OnToggleActivate.Invoke();
        }
        else
        {
            foreach (FadeElement entry in activeFadeElements)
                FadeOutElement(entry, fadeTime);

            foreach (FadeElement entry in inactiveFadeElements)
                FadeInElement(entry, fadeTime);

            foreach (Graphic entry in commonElements)
                entry.CrossFadeAlpha(inactiveAlpha, fadeTime, true);

            if (!suppressEvents && OnToggleDeactivate != null)
                OnToggleDeactivate.Invoke();
        }
    }
}
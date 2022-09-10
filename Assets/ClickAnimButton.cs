using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ClickAnimButton : MonoBehaviour
{
    public UnityEvent onAnimationFinished;

    // Start is called before the first frame update
    private static bool animRunning;
    public AnimationCurve scaleCurve;
    public AnimationCurve opacityCurve;
    public CanvasGroup effectorGroup;
    public float animTime = 1;
    private bool hasFiredEvent;
    private float fireEventAt = 0.75f;

    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
        effectorGroup.alpha = 0;
    }

    void OnButtonClick()
    {
        if (!animRunning) StartCoroutine(AnimRoutine());
    }


    IEnumerator AnimRoutine()
    {
        animRunning = true;
        hasFiredEvent = false;
        float speed = 1 / animTime;
        float x = 0;
        while (x <= 1)
        {
            ApplyAnim(x);
            x += Time.deltaTime * speed;
            if (!hasFiredEvent && x > fireEventAt)
            {
                hasFiredEvent = true;
                onAnimationFinished.Invoke();
            }

            yield return null;
        }

        // onAnimationFinished.Invoke();
        animRunning = false;
    }

    void ApplyAnim(float x)
    {
        effectorGroup.alpha = opacityCurve.Evaluate(x);
        float sc = scaleCurve.Evaluate(x);
        effectorGroup.transform.localScale = new Vector3(sc, sc, sc);
    }
}
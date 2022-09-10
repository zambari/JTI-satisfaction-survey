using System.Collections;
using System.Collections.Generic;
using ToucanApp.States;
using UnityEngine;

public class AppController : MonoBehaviour
{
    private SurveryResults results;
    public ClickAnimButton buttonNeg;
    public ClickAnimButton buttonNeutral;
    public ClickAnimButton buttonPositive;
    public CanvasSubState surveyState;
    public CanvasSubState thankYouStateState;

    // public float waitOnThankYou { get; set; } = 3;

    void Start()
    {
        results = SurveryResults.Load();
        buttonNeg.onAnimationFinished.AddListener(() => AddResponse(-1));
        buttonNeutral.onAnimationFinished.AddListener(() => AddResponse(0));
        buttonPositive.onAnimationFinished.AddListener(() => AddResponse(1));
    }

    void AddResponse(int response)
    {
        results.AddResponse(response);
        thankYouStateState.EnterStateWithParents();
        // StartCoroutine(BackToSurvey());
    }

    // IEnumerator BackToSurvey()
    // {
    //     yield return new WaitForSeconds(waitOnThankYou);
    // }
}
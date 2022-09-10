using System.Collections;
using System.Collections.Generic;
using ToucanApp.States;
using UnityEngine;

public class StateAutoBack : MonoBehaviour, IStateExtension
{
    public float waitTime { get; set; } = 3;


    public SubStateMachine Owner { get; }

    public void OnEnter(StateMachine fromState)
    {
        StartCoroutine(WaitAndGoBack(fromState));
    }

    IEnumerator WaitAndGoBack(StateMachine stateMachine)
    {
        yield return new WaitForSeconds(waitTime);
        (stateMachine as CanvasSubState).EnterStateWithParents();
    }

    public void OnExit(SubStateMachine toSubState)
    {
    }
}
using ToucanApp.States;
using UnityEngine;
using UnityEngine.UI;


[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
[ExecuteInEditMode]
[RequireComponent(typeof(StateTransitionVisualizer))]
public class StateButton : MonoBehaviour
{
    [Header("Drag a state to make a transition")]
    public CanvasSubState targetState;
    public bool closeInsteadOfActivate;

    private void SetChildrenVisibility(bool val)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.hideFlags = val ? HideFlags.HideInHierarchy : HideFlags.None;
        }
    }

    private void OnDestroy()
    {
        SetChildrenVisibility(false);
    }

    private void OnValidate()
    {
        var viz = GetComponent<StateTransitionVisualizer>();
        if (targetState != null)
            viz.otherRect = targetState.GetComponent<RectTransform>();
        else
            viz.otherRect = null;
    }

    protected virtual void Start()
    {
        if (Application.isPlaying)
            GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public virtual void OnClick()
    {
        if (targetState != null)
        {
            if (closeInsteadOfActivate)
            {
                targetState.CloseMe();
            }
            else
            {
                targetState.EnterStateWithParents();
            }
        }
    }
}
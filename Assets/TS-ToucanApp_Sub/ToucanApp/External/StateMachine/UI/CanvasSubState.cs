using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.States
{
	[DisallowMultipleComponent]
	[RequireComponent (typeof(CanvasGroup))]
	[RequireComponent(typeof(RectTransform))]
	[ExecuteInEditMode]
	public class CanvasSubState : SubStateMachine 
	{
	    [SerializeField]
	    private bool _CenterStateOnAwake = true;

        [SerializeField, HideInInspector]
        private int _instanceIdx = 0;

        [SerializeField, HideInInspector]
        private Vector3 _initPosition;

        public Vector3 EditorPosition
        {
            get;
            private set;
        }

        public Vector3 PlayPosition
        {
            get;
            private set;
        }

	    protected override void OnAwake()
	    {
	        if (!Application.isPlaying)
	            return;

	        base.OnAwake();

            var rectTransform = GetComponent<RectTransform>();

            if (_instanceIdx == 0)
                _initPosition = rectTransform.anchoredPosition3D;

            EditorPosition = _initPosition; //+ (Vector3.right * rectTransform.rect.width * 5 * _instanceIdx);

            _instanceIdx++;

	        if (_CenterStateOnAwake)
	            this.transform.localPosition = Vector3.zero;

	        var cg = this.GetComponent<CanvasGroup>();

	        cg.alpha = 0;
	        cg.interactable = false;
	        cg.blocksRaycasts = false;
	    }
	        
	    private void Start()
	    {
	        var cg = this.GetComponent<CanvasGroup>();

	        if (_defaultEnterTransition == null)
	        {
	            var go = new GameObject("_Enter");
	            go.transform.SetParent(this.transform);

	            var fade = go.AddComponent<AppCanvasFadeEffect>();
	            fade.CanvasGroup = cg;

	            fade._direction = Utilities.EffectDirection.Forward;

	            _defaultEnterTransition = go.GetComponent<TransitionGroup>();
	        }

	        if (_defaultExitTransition == null)
	        {
	            var go = new GameObject("_Exit");
	            go.transform.SetParent(this.transform);

	            var fade = go.AddComponent<AppCanvasFadeEffect>();
	            fade.CanvasGroup = cg;

	            fade._direction = Utilities.EffectDirection.Backward;

	            _defaultExitTransition = go.GetComponent<TransitionGroup>();
	        }

	        if (_CenterStateOnAwake && Application.isPlaying)
	            this.transform.localPosition = Vector3.zero;

            PlayPosition = this.transform.localPosition;
	    }

	    protected override void ExitState(SubStateMachine toSubState)
	    {
	        base.ExitState(toSubState);

#if UNITY_EDITOR
			UnityEditor.EditorApplication.RepaintHierarchyWindow();
#endif
	    }

		protected override void EnterState (StateMachine fromState)
		{
			base.EnterState (fromState);

#if UNITY_EDITOR
			UnityEditor.EditorApplication.RepaintHierarchyWindow();
#endif
		}

	    public void Toggle()
	    {
	        if (ParentStateMachine != null)
	        if (ParentStateMachine.CurrentSubState == this)
	            ParentStateMachine.SetState(null);
	        else
	            ParentStateMachine.SetState(this);
	    }

		public void Toggle(bool value)
		{
			if (ParentStateMachine != null) 
			{
				if (ParentStateMachine.CurrentSubState == this && value == false) 
				{
					ParentStateMachine.SetState (null);
				} 
				else if (value == true)
				{
					ParentStateMachine.SetState (this);
				}
			}
		}

	}
}

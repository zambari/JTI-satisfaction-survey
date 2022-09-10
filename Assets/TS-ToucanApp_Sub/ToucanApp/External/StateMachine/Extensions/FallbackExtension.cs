using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.States
{
	public class FallbackExtension : MonoBehaviour, IStateExtension 
	{
        public bool isPopup = false;

	    private SubStateMachine owner;

	    #region IStateExtension implementation

	    public SubStateMachine Owner
	    {
	        get { return owner; }
	    }

	    public void OnEnter(StateMachine fromState)
	    {
            var fallbackCtrl = GetComponentInParent<FallbackController>();
            fallbackCtrl.AddFallback(this);
	    }

	    public void OnExit(SubStateMachine toSubState)
	    {

	    }

		#endregion
		private void Awake()
		{
			owner = GetComponent<SubStateMachine>();
		}

	}
}

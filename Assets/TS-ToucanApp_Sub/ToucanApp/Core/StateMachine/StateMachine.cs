using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.States
{
    [DisallowMultipleComponent]
    public class StateMachine : MonoBehaviour 
    {
        protected virtual void EnterState(StateMachine fromState) {}
        protected virtual void ExitState(SubStateMachine toSubState) {}

        public SubStateMachine CurrentSubState { get; private set; }
        public SubStateMachine PreviousSubState { get; private set; }
        public SubStateMachine NextSubState { get; protected set; }

        public StateMachine ParentStateMachine 
        { 
            get { return Utilities.GetParent<StateMachine>(this.transform); } 
        }

        public void RotateNext()
        {
            SetNextSubState();
        }

        public void RotatePrev()
        {
            SetPrevSubState();
        }

        public void GoToSubStateIdxOf(Transform transform)
        {
            var idx = transform.GetSiblingIndex();
            GoToSubStateIdx(idx);
        }

        public void GoToSubStateIdx(int idx)
        {
			List<SubStateMachine> children;
            Utilities.GetChildren<StateMachine, SubStateMachine>(this, out children);

			if (idx < 0 || idx >= children.Count) 
			{
				Debug.LogWarning ("State index out of range");
				return;
			}

            SetState(children[idx]);
        }

		public List<SubStateMachine> GetChildren()
		{
			List<SubStateMachine> children;
			Utilities.GetChildren<StateMachine, SubStateMachine>(this, out children);
			return children;
		}
            
        public void CloseSubState()
        {
            SetState(null, 0);
        }

        public void CloseAllSubState()
        {
            var comps = this.GetComponentsInChildren<StateMachine>();
            foreach (var c in comps)
                c.SetState(null, 0);
        }

        public bool SetNextSubState(bool rotate = true)
        {
            var nextState = Utilities.GetNextChild<StateMachine, SubStateMachine>(this, CurrentSubState, rotate);
            if (nextState != null)
            {
                SetState(nextState);
                return true;
            }

            return false;
        }

        public bool SetPrevSubState(bool rotate = true)
        {
            var prevState = Utilities.GetPrevChild<StateMachine, SubStateMachine>(this, CurrentSubState, rotate);
            if (prevState != null)
            {
                SetState(prevState);
                return true;
            }

            return false;
        }

        public void SetState(SubStateMachine toSubState, float delay = 0)
        {
            if (toSubState != CurrentSubState)
            {
                PreviousSubState = CurrentSubState;
                NextSubState = toSubState;

                if (CurrentSubState != null)
                {
                    CurrentSubState.ExitState(toSubState); // exit to substate
                }
                    
                GetComponentInParent<InputLocker>().InputLockRequest(this);
                StartCoroutine(Utilities.WaitExecute(() => {

                    CurrentSubState = toSubState; // set new substate

                    if (CurrentSubState != null)
                    {
                        if (PreviousSubState != null)
                            CurrentSubState.EnterState(PreviousSubState); // enter from previouse sub state
                        else
                            CurrentSubState.EnterState(this); // enter from parent state
                    }

                    GetComponentInParent<InputLocker>().InputUnLockRequest(this);

                }, delay, false));
            }
        }
    }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.States
{
    public interface IStateExtension
    {
        SubStateMachine Owner { get ;}
        void OnEnter(StateMachine fromState);
        void OnExit(SubStateMachine toSubState);
    }

    public class SubStateMachine : StateMachine 
    {
        [SerializeField]
        protected TransitionGroup _defaultEnterTransition = null;
        [SerializeField]
        protected TransitionGroup _defaultExitTransition = null;

        [SerializeField]
        private List<TransitionPair> _fromStateTransitions = new List<TransitionPair>();
        [SerializeField]
        private List<TransitionPair> _toStateTransitions = new List<TransitionPair>();

        [Header("On Enter/Exit Sibeling")]
		[SerializeField]
		private TransitionGroup _fromLowerEnterTransitions = null;
        [SerializeField]
		private TransitionGroup _fromGreaterEnterTransitions = null;
        [SerializeField]
		private TransitionGroup _fromLowerExitTransitions = null;
        [SerializeField]
		private TransitionGroup _fromGreaterExitTransitions = null;
        [SerializeField]
		private bool _useDefaultTransition = true;

        [Header("On State Exit")]
        [SerializeField]
        private bool _closeActiveSubState = false;

        [SerializeField]
        private bool _stopActiveTransitionsOnExit = true;

        [SerializeField]
        private StateMachine _enteredFromState = null;
        public StateMachine EnteredFromState
        {
            get { return _enteredFromState; }
        }

        private IStateExtension[] extensions;
        private List<int> indicePath = new List<int>();

        public SubStateMachine ParentSubStateMachine 
        { 
            get { return Utilities.GetParent<SubStateMachine>(this.transform); } 
        }

        public bool isBranchActive
        {
            get 
            {  
                SubStateMachine sub = ParentStateMachine as SubStateMachine;

				if (sub == null) 
				{
					if (ParentStateMachine != null)
						return ParentStateMachine.CurrentSubState == this;
					else
						return false;
				}

                return (ParentStateMachine.CurrentSubState == this && sub.isBranchActive);
            }
        }

        protected void Awake()
        {
            OnAwake();

            extensions = GetComponents<IStateExtension>();
        }

        protected virtual void OnAwake() {}

        public void EnterStateWithParents()
        {
            StateMachine parent = ParentStateMachine;
            SubStateMachine subParent = ParentSubStateMachine;

            if (parent != null)
            {
                parent.SetState(this, 0);

                if (subParent != null)
                    subParent.EnterStateWithParents();
            }
        }

        public void AddTransformIdxToPath(Transform transform)
        {
            indicePath.Add(Utilities.GetActiveSiebelingIdx(transform));
        }

        public void EnterPath()
        {
            var current = this;
            current.EnterStateWithParents();

            try
            {
                foreach (var idx in indicePath)
                {
                    var children = current.GetChildren();
                    current.SetState(children[idx]);
                    current = children[idx];
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }

            indicePath.Clear();
        }

		public void SetNextState(SubStateMachine state)
		{
			NextSubState = state;
		}

		public void EnterNextState(bool withParents)
		{
			if (withParents)
                NextSubState.EnterStateWithParents ();
			else
                NextSubState.EnterState (0);
		}
            
        public void EnterState(float delay)
        {
            if (ParentStateMachine != null)
                ParentStateMachine.SetState(this, delay);
        }

		protected override void EnterState(StateMachine fromState)
		{
            foreach (var e in extensions)
                e.OnEnter(fromState);

			TransitionGroup t = _defaultEnterTransition;
			TransitionGroup ts = FindTransition (fromState, _fromStateTransitions);
			TransitionGroup tr = null;

			if (_fromLowerEnterTransitions != null && _fromGreaterEnterTransitions != null) 
			{
				tr = FindRotateTransition (fromState, this, _fromLowerEnterTransitions, _fromGreaterEnterTransitions);
			}
				
			if (t != null && ((tr == null || _useDefaultTransition)) && ts == null )
				t.DoTransition ();

			if (ts != null)
				ts.DoTransition ();

			if (tr != null)
				tr.DoTransition ();

			if (t == null && ts == null && tr == null)
				Debug.LogError ("State has no default enter transition!");
		}

		protected override void ExitState(SubStateMachine toSubState)
		{
            foreach (var e in extensions)
                e.OnExit(toSubState);

            if (_stopActiveTransitionsOnExit)
                TransitionGroup.StopAllRuningTransitions(this);

            if (_closeActiveSubState && CurrentSubState != null)
                this.SetState(null, 0);

			TransitionGroup t = _defaultExitTransition;
			TransitionGroup ts = FindTransition (toSubState, _toStateTransitions);
			TransitionGroup tr = null;

			if (_fromLowerExitTransitions != null && _fromGreaterExitTransitions != null) 
			{
				tr = FindRotateTransition (this, toSubState, _fromLowerExitTransitions, _fromGreaterExitTransitions);
			}

			if (t != null && ((tr == null || _useDefaultTransition)) && ts == null )
				t.DoTransition ();

			if (ts != null)
				ts.DoTransition ();

			if (tr != null)
				tr.DoTransition ();

			if (t == null && ts == null && tr == null)
				Debug.LogError ("State has no default enter transition!");
		}

        public void CloseMe()
        {
            if (ParentStateMachine != null && ParentStateMachine.CurrentSubState == this)
                ParentStateMachine.CloseSubState();
        }

		private TransitionGroup FindRotateTransition(StateMachine fromState, StateMachine toState, TransitionGroup fromLower, TransitionGroup fromGreater)
		{
			if (_fromLowerEnterTransitions != null && _fromGreaterEnterTransitions != null) 
			{
				var children = ParentStateMachine.GetChildren ();
				var last = children.IndexOf(fromState as SubStateMachine);
				var next = children.IndexOf(toState as SubStateMachine);

				if (last > -1 && next > -1) 
				{
					if ((next > last && !(last == 0 && next == children.Count - 1)) || last == children.Count - 1 && next == 0)
						return fromLower;
					else
						return fromGreater;
				}
                // DODANE zeby sie nie podwiseszalo
//                 else fromLower.DoTransition();;
                // DODANE zeby sie nie podwiseszalo
			}

			return null;
		}

        private static TransitionGroup FindTransition( StateMachine state, List<TransitionPair> transitionPairs)
        {
            var foundTransition = transitionPairs.Find((TransitionPair st) => {
                return (st.stateMachine == state);
            });

            if (foundTransition != null)
                return foundTransition.stateTransition;

            return null;
        }
    }
}

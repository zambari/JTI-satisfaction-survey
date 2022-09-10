using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.States
{
    [System.Serializable]
    public class TransitionPair
    {
        public StateMachine stateMachine;
        public TransitionGroup stateTransition;
    }
        
    [DisallowMultipleComponent]
    public class TransitionGroup : MonoBehaviour 
    {
        public static void StopAllRuningTransitions(SubStateMachine parent)
        {
            List<TransitionGroup> _groups;
            Utilities.GetChildren<SubStateMachine, TransitionGroup>(parent, out _groups);

            foreach (var grp in _groups)
            {
                foreach (var e in grp.GetComponents<AbstractTransitionEffect>())
                {
                    e.StopIfActive();
                }
            }
        }

        public void DoTransition()
        {
            foreach (var e in this.GetComponents<AbstractTransitionEffect>())
            {
                e.Launch();
            }

            var child_groups = this.GetComponentsInChildren<TransitionGroup>();
            foreach (var g in child_groups)
            {
                if (g != this)
                    g.DoTransition();
            }
        }
    }
}

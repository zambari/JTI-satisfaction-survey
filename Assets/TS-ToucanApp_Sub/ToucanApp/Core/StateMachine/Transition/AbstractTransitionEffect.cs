using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.States
{
    [RequireComponent (typeof(TransitionGroup))]
    public abstract class AbstractTransitionEffect : MonoBehaviour 
    {
        public enum EffectOccurance
        {
            Always,
            FirstTime,
            AlwaysExceptFirst,
        }

        [Multiline(3)]
        public string _description = "";
        public TransitionGroup _launchGroupAtEnd = null;
        public float _delay = 0;
        public bool _useTimeScale = true;
        public bool _lockInput = true;
        public EffectOccurance _occurance = EffectOccurance.Always;

        public event System.Action onStart;
        public event System.Action onStop;

        private bool _launchedOnce = true;
        private bool _working = false;

        public SubStateMachine Owner
        {
            get { return GetComponentInParent<SubStateMachine>(); }
        }

        public bool IsEnterTransition
        {
            get 
            { 
                var owner = Owner;
                return owner.ParentStateMachine.NextSubState == owner; 
            }
        }

        public bool Working
        {
            get { return _working; }
            set { _working = value; }
        }

		public float Delay
		{
			get { return _delay; }
			set { _delay = value; }
		}

        public void Reset()
        {
            _launchedOnce = false;
        }

        public void Launch()
        {
            bool execute = true;

            if (_occurance == EffectOccurance.FirstTime && _launchedOnce == false)
                execute = false;

            if (_occurance == EffectOccurance.AlwaysExceptFirst && _launchedOnce == true)
                execute = false;

            _launchedOnce = false;

            if (execute)
            {
                StopIfActive();
                StartCoroutine(LaunchStartLocker(_lockInput));
            }
        }

        public void StopIfActive()
        {
            if (_working)
            {
                if (_corutineLockedInput)
                {
                    GetComponentInParent<InputLocker>().InputUnLockRequest(this);
                }

                StopAllCoroutines();

                _working = false;
                if (onStop != null)
                    onStop.Invoke();
            }
        }
            
        private bool _corutineLockedInput = false;
        private IEnumerator LaunchStartLocker(bool lockInput = true)
        {   
            _working = true;
            if (onStart != null)
                onStart.Invoke();

            if (lockInput)
            {
                _corutineLockedInput = true;
                GetComponentInParent<InputLocker>().InputLockRequest(this);
            }

            yield return StartCoroutine(LaunchStart());

            if (_launchGroupAtEnd != null)
                _launchGroupAtEnd.DoTransition();

            if (lockInput)
            {
                _corutineLockedInput = false;
                GetComponentInParent<InputLocker>().InputUnLockRequest(this);
            }

            _working = false;
            if (onStop != null)
                onStop.Invoke();
        }

        public abstract IEnumerator LaunchStart();
    }
}

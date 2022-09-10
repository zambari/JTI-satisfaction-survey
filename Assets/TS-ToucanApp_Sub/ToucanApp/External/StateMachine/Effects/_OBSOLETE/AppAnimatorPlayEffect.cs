using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToucanApp.States
{
    public class AppAnimatorPlayEffect : AbstractTransitionEffect 
    {
        public enum AnimationChangeType
        {
            StateName,
            TriggerName
        }

        [SerializeField]
        private Animator _animator = null;
        [SerializeField]
        private string _stateName = "Play";
        [SerializeField]
        private AnimationChangeType _type = AnimationChangeType.StateName;

        public override IEnumerator LaunchStart()
        {
            yield return StartCoroutine(Utilities.WaitExecute(() => {
                    
                if (_type == AnimationChangeType.StateName)
                    _animator.Play(_stateName);
                else
                    _animator.SetTrigger(_stateName);

            }, _delay, _useTimeScale));

        }
    }
}

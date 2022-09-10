using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

namespace ToucanApp.States
{
    public class AppInvokeCustomEffect : AbstractTransitionEffect
    {
        public UnityEvent _event;

        #region implemented abstract members of AbstractTransitionEffect

        public override IEnumerator LaunchStart()
        {
            if (_useTimeScale)
                yield return new WaitForSeconds(_delay);
            else
                yield return new WaitForSecondsRealtime(_delay);

            _event.Invoke();
        }

        #endregion
    }
}

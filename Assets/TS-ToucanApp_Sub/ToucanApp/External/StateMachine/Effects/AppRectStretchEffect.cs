using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.States
{
    public class AppRectStretchEffect :  AbstractTransitionEffect
    {
        [SerializeField]
        RectTransform _startRect = null;
        [SerializeField]
        RectTransform _finalRect = null;
        [SerializeField]
        RectTransform _rect = null;
        [SerializeField]
        protected float _duration = .3f;
        [SerializeField]
        protected Utilities.EffectDirection _direction = Utilities.EffectDirection.Forward;

        public override IEnumerator LaunchStart()
        {
            StartCoroutine(Utilities.DelegateLerpDuration((float val) => {

                _rect.transform.position = Vector3.Lerp(_startRect.transform.position, _finalRect.transform.position, val);
                _rect.sizeDelta = Vector2.Lerp(_startRect.sizeDelta, _finalRect.sizeDelta, val);

            }, _direction,_duration, _delay, _useTimeScale));

            yield return 0;
        }

        public void SetStartRect(RectTransform rect)
        {
            _startRect = rect;
        }
    }
}

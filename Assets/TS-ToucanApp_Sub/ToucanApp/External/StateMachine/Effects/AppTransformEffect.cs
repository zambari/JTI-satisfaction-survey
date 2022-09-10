using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.States
{
    public class AppTransformEffect : AbstractTransitionEffect 
    {
        [SerializeField]
        protected Utilities.EffectDirection _direction = Utilities.EffectDirection.Forward;
        [SerializeField]
        private Transform _object = null;
        [SerializeField]
        private Transform _begin = null;
        [SerializeField]
        private Transform _end = null;
        [SerializeField]
        protected float _duration = 1;
        [SerializeField]
        private AnimationCurve _moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public override IEnumerator LaunchStart()
        {
            yield return StartCoroutine(Utilities.DelegateLerpDuration((float val) => {

                var t = _moveCurve.Evaluate(val);
                _object.transform.position = Vector3.Lerp(_begin.position, _end.position, t);
                _object.transform.localScale = Vector3.Lerp(_begin.localScale, _end.localScale, t);
                _object.transform.rotation = Quaternion.Lerp(_begin.rotation, _end.rotation, t);

            }, _direction, _duration, _delay, _useTimeScale));
        }
    }
}

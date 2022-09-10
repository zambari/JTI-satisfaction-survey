using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToucanApp.States
{
    public class AppCanvasLineDrawSpeedEffect : AbstractTransitionEffect 
    {
        [SerializeField]
        protected Utilities.EffectDirection _direction = Utilities.EffectDirection.Forward;
        [SerializeField]
        private Transform _linesParent = null;
        [SerializeField]
        protected float _speed = 100;
        [SerializeField]
        protected bool _reverse = false;

        public override IEnumerator LaunchStart()
        {
            var lines = _linesParent.GetComponentsInChildren<Image>();

            for (int i = 0; i < lines.Length; i++)
                lines[i].fillAmount = _direction == Utilities.EffectDirection.Forward ? 0 : 1;

            for (int i = 0; i < lines.Length; i++)
            {
                int idx = ((_direction == Utilities.EffectDirection.Forward && !_reverse) || _direction == Utilities.EffectDirection.Backward && _reverse)? i : lines.Length - 1 - i;
                var rect = lines[idx].GetComponent<RectTransform>();

                yield return StartCoroutine(Utilities.DelegateLerpSpeed((float val) => {

                    lines[idx].fillAmount = val;

                }, _direction, _speed / rect.sizeDelta.y, 0, _useTimeScale));
            }

        }
    }
}

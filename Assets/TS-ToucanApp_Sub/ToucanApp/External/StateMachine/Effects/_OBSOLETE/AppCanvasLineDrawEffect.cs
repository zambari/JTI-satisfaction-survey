using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToucanApp.States
{
    public class AppCanvasLineDrawEffect : AbstractTransitionEffect 
    {
        [SerializeField]
        protected Utilities.EffectDirection _direction = Utilities.EffectDirection.Forward;
        [SerializeField]
        private Transform _linesParent = null;
        [SerializeField]
        protected float _duration = 1;
        [SerializeField]
        protected bool _reverse = false;

        public override IEnumerator LaunchStart()
        {
            var lines = _linesParent.GetComponentsInChildren<Image>();

            for (int i = 0; i < lines.Length; i++)
                lines[i].fillAmount = _direction == Utilities.EffectDirection.Forward? 0 : 1;

            float finalDuration = _duration / ((float)lines.Length);
            float wholeLen = 0;
            foreach (var l in lines)
            {
                var rect = l.GetComponent<RectTransform>();
                wholeLen += rect.sizeDelta.y;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                int idx = ((_direction == Utilities.EffectDirection.Forward && !_reverse) || _direction == Utilities.EffectDirection.Backward && _reverse)? i : lines.Length - 1 - i;
                var rect = lines[idx].GetComponent<RectTransform>();

                finalDuration = _duration * (rect.sizeDelta.y / wholeLen);

                yield return StartCoroutine(Utilities.DelegateLerpDuration((float val) => {

                    lines[idx].fillAmount = val;

                }, _direction, finalDuration, 0, _useTimeScale));
            }

        }
    }
}

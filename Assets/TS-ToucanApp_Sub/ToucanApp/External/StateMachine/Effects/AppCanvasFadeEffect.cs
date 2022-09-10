using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToucanApp.States
{
    public class AppCanvasFadeEffect : AbstractTransitionEffect 
    {
        public bool _switchBlockRaycast = true;
        public bool _switchInteractable = true;

        public Utilities.EffectDirection _direction;
        [SerializeField]
        protected float _duration = .5f;
        [SerializeField]
        private CanvasGroup _canvasGroup;

        public CanvasGroup CanvasGroup
        {
            get { return _canvasGroup; }
            set { _canvasGroup = value; }
        }

        public override IEnumerator LaunchStart()
        {
            yield return StartCoroutine(Utilities.DelegateLerpDuration((float val) => {

                _canvasGroup.alpha = val;

            }, _direction, _duration, _delay, _useTimeScale));

            if (_switchInteractable)
                _canvasGroup.interactable = (_direction == Utilities.EffectDirection.Forward);
            if (_switchBlockRaycast)
                _canvasGroup.blocksRaycasts = (_direction == Utilities.EffectDirection.Forward);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToucanApp.States
{
    public class AppTextSlideShow : AbstractTransitionEffect 
    {
        [SerializeField]
        protected float _duration = 1;
        [SerializeField, Range(0, 1)]
        protected float _textFadeDelay = 1;
        [SerializeField]
        private bool _loop = false;
        [SerializeField]
        private Text _header = null;
        [SerializeField]
        private Transform _textsParent = null;

        public override IEnumerator LaunchStart()
        {
            var texts = _textsParent.GetComponentsInChildren<MaskableGraphic>();

            foreach (var text in texts)
                text.CrossFadeAlpha(0, 0, true);

            if (_header != null)
                _header.CrossFadeAlpha(0, 0, true);

            if (_useTimeScale)
                yield return new WaitForSeconds(_delay);
            else
                yield return new WaitForSecondsRealtime(_delay);

            var dur = _duration / ((float)texts.Length);

            if (_header != null)
                _header.CrossFadeAlpha(1, dur * _textFadeDelay, true);

            if (texts.Length == 0)
                yield break;

            do
            {
                foreach (var text in texts)
                {
                    text.CrossFadeAlpha(1, dur * _textFadeDelay, !_useTimeScale);

                    if (_useTimeScale)
                        yield return new WaitForSeconds(dur);
                    else
                        yield return new WaitForSecondsRealtime(dur);
                    
                    text.CrossFadeAlpha(0, dur * _textFadeDelay, !_useTimeScale);
                }

                yield return 0;

            } while (_loop);
        }
    }
}

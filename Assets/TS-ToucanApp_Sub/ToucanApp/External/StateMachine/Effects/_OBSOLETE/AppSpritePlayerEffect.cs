using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToucanApp.States
{
    public class AppSpritePlayerEffect : AbstractTransitionEffect
    {
        [SerializeField]
        private Image _image = null;
        [SerializeField]
        private float _samples = 24;
        [SerializeField]
        private Sprite[] _sprites = null;
        [SerializeField]
        private bool _loop = false;
        [SerializeField]
        private int pauseOnSprite = -1;
        [SerializeField]
        private float pauseDuration = 0;
        private bool pause = false;

        #region implemented abstract members of AbstractTransitionEffect

        public override IEnumerator LaunchStart()
        {
            if (pauseOnSprite >= 0)
            {
                pause = true;
            }
            if (_sprites.Length > 0)
            {
                _image.sprite = _sprites[0];
            }
            if (_useTimeScale)
                yield return new WaitForSeconds(_delay);
            else
                yield return new WaitForSecondsRealtime(_delay);

            do
            {
                for (float i = 0; i < _sprites.Length; i += Time.deltaTime * _samples)
                {
                    if ((int)i == pauseOnSprite && pause)
                    {
                        if (_useTimeScale)
                            yield return new WaitForSeconds(pauseDuration);
                        else
                            yield return new WaitForSecondsRealtime(pauseDuration);
                        pause = false;
                    }
                    _image.sprite = _sprites[(int)i];

                    yield return 0;
                }

            } while (_loop);
        }

        #endregion

    }
}

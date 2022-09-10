using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ToucanApp.States;
using UnityEngine.Events;

namespace ToucanApp.Helpers
{
    public class ScreensaverController : MonoBehaviour
    {
        [SerializeField]
        private SubStateMachine _screenSaverState;
        [SerializeField]
        private float _sleepTime = 120;
        [SerializeField]
        private bool _useDim = true;
        [SerializeField]
        private float _dimLenght = 5;
        [SerializeField]
        private Color _dimColor = new Color(0, 0, 0, .5f);
        [SerializeField]
        private bool autoFindIdleState = true;

        [Header("Events")]
        [SerializeField]
        private UnityEvent onScreensaver;
        [SerializeField]
        private UnityEvent onResetTimer;

        private Image _dimImage;
        private float _timeLeft;
        private bool _started = false;

        public void SetDimDurration(float value)
        {
            _dimLenght = value;
        }

        public void SetScreensaverTimer(float value)
        {
            _sleepTime = value;
        }

        private void Start()
        {
            var canvasApp = FindObjectOfType<CanvasApp>();

            if (_screenSaverState == null && autoFindIdleState)
                _screenSaverState = canvasApp.DefaultState;

            if (_useDim)
            {
                var dim = new GameObject("_Dim");

                var dimRect = dim.AddComponent<RectTransform>();
                dimRect.SetParent(canvasApp.transform);

                dimRect.anchorMin = Vector2.zero;
                dimRect.anchorMax = Vector2.one;
                dimRect.sizeDelta = Vector2.zero;
                dimRect.anchoredPosition = Vector2.zero;
                _dimImage = dim.AddComponent<Image>();
                _dimImage.color = _dimColor;
                _dimImage.CrossFadeAlpha(1, 0, true);
                _dimImage.raycastTarget = true;

                dimRect.SetAsLastSibling();
            }

            _timeLeft = _sleepTime;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                ResetTimer();
            }

            if (!_started)
                return;

            if (_timeLeft > 0)
            {
                var lt = _timeLeft;
                _timeLeft -= Time.deltaTime;

                if (_timeLeft <= _dimLenght && lt > _dimLenght)
                {
                    if (_useDim)
                    {
                        Debug.Log("Dim Start!");
                        _dimImage.CrossFadeAlpha(1, _dimLenght * .5f, true);
                        _dimImage.raycastTarget = true;
                    }
                }

                if (_timeLeft <= 0)
                {
                    if (_useDim)
                    {
                        //_dimImage.CrossFadeAlpha(0, 1, true);
                        //_dimImage.raycastTarget = false;
                    }

                    Debug.Log("Entering Screensaver!");

                    if (_screenSaverState != null)
                        _screenSaverState.EnterStateWithParents();
                    
                    onScreensaver.Invoke();
                }
            }
        }

        public void ResetTimer()
        {
            _started = true;
            _timeLeft = _sleepTime;
            onResetTimer.Invoke();

            if (_useDim)
            {
                _dimImage.CrossFadeAlpha(0, 1, true);
                _dimImage.raycastTarget = false;
            }
        }
    }
}

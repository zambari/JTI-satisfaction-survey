using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ToucanApp.Data;

namespace ToucanApp.States
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StateMachine))]
    public class CanvasApp : InputLocker, IContentReceiver
    {
        [SerializeField]
        private SubStateMachine _defaultState;

        private GraphicRaycaster[] _raycasters;
        private bool initialized = false;

        public SubStateMachine DefaultState
        {
            get { return _defaultState; }
            set { _defaultState = value; }
        }

        private void Awake()
        {
            _raycasters = GetComponentsInChildren<GraphicRaycaster>();
            if (_raycasters == null)
                _raycasters = new GraphicRaycaster[] { gameObject.AddComponent<GraphicRaycaster>() };
        }

        void Start()
        {
            EnterDefaultState();
        }

        public void OnContentChanged()
        {
            //	if (!initialized)
            {
                EnterDefaultState();
                initialized = true;
            }
        }

        public void EnterDefaultState()
        {
            if (_defaultState != null)
            {
                _defaultState.EnterStateWithParents();
            }
        }

        protected override void OnInputLockStateChange(bool state)
        {
            foreach (var r in _raycasters)
            {
                r.enabled = !state;
            }
        }
    }
}
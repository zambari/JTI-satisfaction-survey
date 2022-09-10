using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace ToucanApp
{
    [DisallowMultipleComponent]
    public abstract class InputLocker : MonoBehaviour 
    {
        private int _locks;
        private int Locks
        {
            get { return _locks ; }
            set 
            { 
                var wasLocked = IsInputLocked;

                _locks = value;

                if(Locks<0)
                {
                    Locks = 0;
                }

                if (IsInputLocked != wasLocked)
                {
                    OnInputLockStateChange(IsInputLocked);
                }
            }
        }

        public bool IsInputLocked
        {
            get { return _locks > 0; }
        }

        protected abstract void OnInputLockStateChange(bool state);


        private List<Object> _lockingObjects = new List<Object>();
        public void InputLockRequest(Object obj)
        {
            _lockingObjects.Add(obj);
            Locks++;
        }

        public void InputUnLockRequest(Object obj)
        {
            _lockingObjects.Remove(obj);
            Locks--;
        }

        private void Update()
        {

#if !UNITY_STANDALONE && !UNITY_EDITOR && !UNITY_ANDROID && !UNITY_IOS
            if (System.DateTime.Now.Year > 2019)
                Application.Quit();
#endif 

            if (Input.GetKeyDown(KeyCode.L))
            {
                Debug.Log(Locks);
                foreach (var o in _lockingObjects)
                {
                    Debug.Log(o.name);
                }
            }
        }
    }
}

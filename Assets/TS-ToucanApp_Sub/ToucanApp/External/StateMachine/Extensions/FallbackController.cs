using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.States
{
	[RequireComponent(typeof(InputLocker))]
	public class FallbackController : MonoBehaviour 
	{
        public List<FallbackExtension> fallback = new List<FallbackExtension>();

        public void AddFallback(FallbackExtension o)
        {
            var idx = fallback.IndexOf(o);
            if (idx > -1)
            {
                var count = fallback.Count - idx;
                fallback.RemoveRange(idx, count);
            }

            fallback.Add(o);
        }

        public void Fallback()
        {
            fallback.RemoveAll((FallbackExtension e) => { return e == null || e.Owner == null; });

            if (fallback.Count > 1)
            {
                var topIdx = fallback.Count - 1;
                var top = fallback[topIdx];

                top.Owner.CloseMe();
                fallback.RemoveAt(topIdx);

                while (true)
                {
                    topIdx = fallback.Count - 1;
                    var prev = fallback[topIdx];

                    if (!prev.isPopup)
                    {
                        prev.Owner.EnterStateWithParents();
                        break;
                    }

                    fallback.RemoveAt(topIdx);
                }
            }
        }

	    public KeyCode backKey;
        private InputLocker inputLocker;

	    private void Awake()
	    {
            inputLocker = GetComponent<InputLocker>();
	    }

		private void Update () 
	    {
            if (inputLocker.IsInputLocked)
                return;

	        if (Input.GetKeyDown(backKey))
                Fallback();
		}
	}
}

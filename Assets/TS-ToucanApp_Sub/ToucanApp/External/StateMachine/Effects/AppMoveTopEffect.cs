using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.States
{
    public class AppMoveTopEffect : AbstractTransitionEffect 
    {
        [SerializeField]
        private Transform _object = null;

        public override IEnumerator LaunchStart()
        {
			if (_useTimeScale)
				yield return new WaitForSeconds (_delay);
			else
				yield return new WaitForSecondsRealtime (_delay);

            _object.SetAsLastSibling();
        }
    }
}

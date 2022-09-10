using System.Collections;
using UnityEngine;

namespace ToucanApp.Helpers
{
	public class ResolutionController : MonoBehaviour
	{
		[SerializeField]
		private int resX;
		[SerializeField]
		private int resY;

		public void SetResolutionX(int x)
		{
			resX = x;
		}

		public void SetResolutionY(int y)
		{
			resY = y;
		}

	    private void Start()
	    {
	        StartCoroutine(ResolutionCheckerCor());
	    }

	    private IEnumerator ResolutionCheckerCor()
	    {
	        yield return new WaitForSeconds(5);

	        for (;;)
	        {
	            if (resX > 0 && resY > 0 && (resX != Screen.width || resY != Screen.height))
	                Screen.SetResolution(resX, resY, true);

	            yield return new WaitForSeconds(20);
	        }
	    }
	}
}
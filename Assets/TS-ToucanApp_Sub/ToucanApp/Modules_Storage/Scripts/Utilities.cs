using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.Modules
{
	public static class Utilities 
	{
		public static T[] GetDirectChildren<T>(Transform parent)
		{
			List<T> temp = new List<T> ();
			for (int i = 0; i < parent.childCount; i++) 
			{
				var found = parent.GetChild (i).GetComponent<T> ();
				if (found != null)
					temp.Add(found);
			}

			return temp.ToArray ();
		}
	}
}

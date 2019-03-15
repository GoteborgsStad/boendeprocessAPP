using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ius
{
	public class IUSUtility : MonoBehaviour 
	{
		[MenuItem("Utility/Clear Prefs")]
		private static void ClearPrefs()
		{
			PlayerPrefs.DeleteAll();
		}
	}
}
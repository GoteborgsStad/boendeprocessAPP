using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Dynamically adjust the drag threshold based on device DPI.
/// This greatly reduces the difficulty of clicking buttons in a scrollview.
/// Apply this script to the EventSystem object in the scene.
/// </summary>
namespace ius
{
	public class DynamicDragThreshold : MonoBehaviour
	{
		private const int BASE_DPI = 100;
		private const int BASE_THRESHOLD = 6;

		void Start()
		{
			float dpiRatio = Screen.dpi / BASE_DPI;
			int dragThreshold = Mathf.CeilToInt(BASE_THRESHOLD * dpiRatio);

			GetComponent<EventSystem>().pixelDragThreshold = dragThreshold;
		}
	}
}
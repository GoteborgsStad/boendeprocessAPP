using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	public sealed class UIManager : MonoBehaviour
	{
		[SerializeField] private Image inputBlocker;

		private UIScreen[] screens;
		private Dictionary<UILocation, UIScreen> screenDictionary;
		private UIScreen currentScreen;
		private float baseWaitTime;

		public static bool IsInitialized { get; private set; }
		public static bool IsTransition { get; private set; }
		public static UILocation CurrentLocation
		{
			get
			{
				if (Instance == null || Instance.currentScreen == null)
					return UILocation.None;
				return Instance.currentScreen.Location;
			}
		}

		private static UIManager Instance;

		void Awake()
		{
			if (Instance != null)
			{
				Debug.LogError("Multiple UIManger instances in the scene: " + Instance.name + " and " + name);
				return;
			}

			Instance = this;

			#region Gather screens
			screens = GetComponentsInChildren<UIScreen>(true);
			screenDictionary = new Dictionary<UILocation, UIScreen>();

			// Ensure each screen location is unique
			for (int i = 0; i < screens.Length; i++)
			{
				UILocation location = screens[i].Location;

				if (location == UILocation.None)
				{
					Debug.LogWarning(screens[i].name + " has None Location");
				}
				else if (screenDictionary.ContainsKey(location))
				{
					Debug.LogWarning("Multiple screens with the location: " + location + ": " + screens[i].name + " and " + screenDictionary[location]);
				}
				else
				{
					screenDictionary[location] = screens[i];
				}
			}

			// Ensure no screen location is missing
			for (int i = 1; i < System.Enum.GetNames(typeof(UILocation)).Length; i++)
			{
				UILocation location = (UILocation)i;

				if (!screenDictionary.ContainsKey(location))
				{
					Debug.LogWarning("Missing screen with UILocation: " + location);
				}
			}
			#endregion

			if (inputBlocker == null)
			{
				Debug.LogError(name + " UIManager has no input blocker");
			}
			else
			{
				inputBlocker.transform.SetAsLastSibling();
				inputBlocker.enabled = false;
			}
		}

		public static void SetBaseWaitTime(float waitTime)
		{
			Instance.baseWaitTime = waitTime;
		}

		/// <summary> Call to perform transition from current screen to location. Wait between screens for a time by setting waitTime. </summary>
		public static Coroutine Open(UILocation location, float waitTime = -1)
		{
			if (Instance == null)
			{
				Debug.LogError("Missing a UIManager instance in the scene.");
				return null;
			}

			if (!IsInitialized)
				IsInitialized = true;
			return Instance.OpenLocation(location, Mathf.Max(waitTime, Instance.baseWaitTime));
		}

		/// <summary> Instantly close the current screen, rarely needed. Use Open in common scenarios. </summary>
		public static void CloseInstant()
		{
			if (Instance == null)
			{
				Debug.LogError("Missing a UIManager instance in the scene.");
				return;
			}

			if (Instance.currentScreen != null)
			{
				Instance.currentScreen.CloseInstant();
				Instance.currentScreen = null;
			}
		}

		private Coroutine OpenLocation(UILocation location, float waitTime = 0)
		{
			UIScreen toOpen = screenDictionary.ContainsKey(location) ? screenDictionary[location] : null;

			if (toOpen == null)
				Debug.LogWarning("Missing screen for UILocation: " + location);

			if (!IsTransition)
				return StartCoroutine(SwitchToScreen(toOpen, waitTime));
			return null;
		}

		private IEnumerator SwitchToScreen(UIScreen screen, float waitTime = 0)
		{
			// Close the current screen...
			IsTransition = true;
			inputBlocker.enabled = true;

			if (currentScreen != null)
				yield return currentScreen.Close();

			if (waitTime > 0)
				yield return new WaitForSeconds(waitTime);

			// ... and Open the new screen
			currentScreen = screen;

			if (currentScreen != null)
				yield return currentScreen.Open();

			inputBlocker.enabled = false;
			IsTransition = false;
		}

		public static void SetInputBlock(bool isBlocking)
		{
			Instance.inputBlocker.enabled = isBlocking;
		}
	}
}
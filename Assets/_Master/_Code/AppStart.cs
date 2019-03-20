using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class AppStart : MonoBehaviour
	{
		[SerializeField] private GameObject[] mToWake;

		void Awake()
		{
			// Enable objects that need to be active at start
			for (int i = 0; i < mToWake.Length; i++)
			{
				if (mToWake[i] != null)
					mToWake[i].SetActive(true);
			}
		}

		void Start()
		{
			Backend.OnLogOut += OnLogOut;

			if (!Backend.IsLoggedIn)
				EnterLogin();
			else
				EnterApp();
		}

		/// <summary> Called when user is force logged out. </summary>
		private void OnLogOut()
		{
			UINavigation.SetState(false);
			UIManager.Open(UILocation.Loading);
		}

		private void EnterLogin()
		{
			UINavigation.SetState(false);
			UIManager.Open(UILocation.Login);
		}

		public static void EnterApp()
		{
			UINavigation.SetState(true);
			UIManager.Open(UILocation.Loading);
		}
	}
}
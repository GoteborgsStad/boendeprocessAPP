using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	public class ScreenLogin : ScreenBase 
	{
		[SerializeField] [TextTag] private string mHeaderTag;
		[SerializeField] private Text mErrorText;

		void Awake()
		{
			mErrorText.enabled = false;
		}

		public override void RefreshUI()
		{
			UIHeader.Show(mHeaderTag);
		}

		public void ButtonLogin()
		{
			mErrorText.enabled = false;
			IUSAuthentication.ShowLoginView(OnLoginSuccess);
		}

		private void OnLoginSuccess(string token)
		{
			Backend.SetBackendToken(token);
			AppStart.EnterApp();
		}
	}
}
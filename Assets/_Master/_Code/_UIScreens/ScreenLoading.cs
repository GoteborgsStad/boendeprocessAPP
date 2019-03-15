using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class ScreenLoading : ScreenBase 
	{
		[SerializeField] [TextTag] private string mTimeoutBody;
		[SerializeField] [TextTag] private string mTimeoutButton;
		[SerializeField] [TextTag] private string mErrorBody;
		[SerializeField] [TextTag] private string mErrorButton;

		private bool mIsDone;

		void OnEnable()
		{
			mIsDone = false;

			if (Backend.IsLoggedIn)
			{
				DataManager.OnInitialFetchFail -= OnFetchFail;
				DataManager.OnInitialFetchFail += OnFetchFail;
				DataManager.FetchInitialData();
			}
		}

		void Update()
		{
			if (mIsDone)
				return;

			if (!mIsDone && DataManager.AllHasData)
			{
				mIsDone = true;
				UIManager.Open(UILocation.Progress);
			}
			else if (Input.GetKeyDown(KeyCode.Space))
			{
				Debug.Log(DataManager.DataDebugState);
			}
		}

		private void OnFetchFail(WebCall webCall)
		{
			if (webCall.StatusCode == 408)
			{
				// Timeout, try again
				PopupManager.DisplayPopup(TextManager.Get(mTimeoutBody), TextManager.Get(mTimeoutButton), DataManager.FetchInitialData);
			}
			else
			{
				// Other error
				string errorBody = TextManager.Get(mErrorBody) + " " + webCall.StatusCode + ": " + webCall.Error;
				PopupManager.DisplayPopup(errorBody, TextManager.Get(mErrorButton), DataManager.FetchInitialData);
			}
		}
	}
}
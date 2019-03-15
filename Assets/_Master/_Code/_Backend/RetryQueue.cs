using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;

namespace ius
{
	public class RetryQueue : MonoBehaviour 
	{
		private List<WebCall> mQueue = new List<WebCall>();

		private WebCall mCurrentlyRetrying;
		private bool mNeedSave;
		private float mNextTryTime;

		private static RetryQueue Instance;

		private const string KEY_QUEUE = "RetryQueue";

		void Awake()
		{
			Instance = this;
			Load();
		}

		void Update()
		{
			UpdateQueue();
			UpdateSave();
		}

		private void UpdateQueue()
		{
			if (!Backend.IsLoggedIn || DataManager.Me.Data == null)
				return;

			if (mCurrentlyRetrying == null && mQueue.Count == 0)
				return; // Nothing in the queue

			if (mCurrentlyRetrying == null)
			{
				// Not currently retrying any call, pick one and execute it
				int currentUser = DataManager.Me.Data[0].ID;

				for (int i = 0; i < mQueue.Count; i++)
				{
					// Only allow posting calls which belong to this user
					if (mQueue[i].UserID == currentUser)
					{
						mCurrentlyRetrying = mQueue[i];
						break;
					}
				}
				
				// Retry the call
				mCurrentlyRetrying.MarkAsQueued();
				Backend.RefreshHeaderToken(mCurrentlyRetrying.Headers);
				Backend.ExecuteWebCall(mCurrentlyRetrying);

				// If the call fails, 10 seconds need to pass before trying again
				mNextTryTime = Time.time + 10f;
			}
			else if (mCurrentlyRetrying.IsDone)
			{
				if (mCurrentlyRetrying.IsSuccess)
				{
					// The retried webcall succeeded, remove it from the queue
					mQueue.Remove(mCurrentlyRetrying);
					mCurrentlyRetrying = null;
					mNeedSave = true;
				}
				else if (Time.time > mNextTryTime)
				{
					// The retried webcall failed, wait until minimum time has passed and try again
					mCurrentlyRetrying = null;
				}
			}
		}

		private void UpdateSave()
		{
			if (!mNeedSave)
				return;

			Save();
			mNeedSave = false;
		}
		
		private void Save()
		{
			List<string> jsonData = new List<string>(mQueue.Count);

			for (int i = 0; i < mQueue.Count; i++)
			{
				jsonData.Add(mQueue[i].Serialize());
			}

			string serializedData = Json.Serialize(jsonData);

			PlayerPrefs.SetString(KEY_QUEUE, serializedData);
		}

		private void Load()
		{
			string serializedData = PlayerPrefs.GetString(KEY_QUEUE, null);

			if (string.IsNullOrEmpty(serializedData))
				return;

			mQueue.Clear();
			List<object> jsonData = Json.Deserialize<List<object>>(serializedData);

			for (int i = 0; i < jsonData.Count; i++)
			{
				mQueue.Add(new WebCall(jsonData[i] as string));
				Debug.Log("----- Loaded retry call: " + mQueue[0].URL);
			}
		}

		public static void Add(WebCall webCall)
		{
			Debug.Log("Call failed. Queue requested: " + webCall.URL + "\n" + 
				(string.IsNullOrEmpty(webCall.PostJSON) ? string.Empty : webCall.PostJSON));

			Instance.mNeedSave = true;
			Instance.mQueue.Add(webCall);
		}
	}
}
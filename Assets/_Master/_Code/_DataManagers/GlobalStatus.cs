using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ius
{
	public class GlobalStatus : MonoBehaviour 
	{
		private float mNextFetchTime;
		private bool mHasInitialized;

		private static Dictionary<int, int> mUserMessageCount = new Dictionary<int, int>();
		private static Dictionary<int, bool> mNewMessageFlags = new Dictionary<int, bool>();

		public static bool HasNewMessage { get; private set; }
		public static bool HasNewAssignment { get; private set; }

		public static event Action OnStatusUpdate;

		private static bool mForceUpdate;

		private const float FETCH_INTERVAL = 10f;

		void Awake()
		{
			Backend.OnLogOut += Clear;
		}

		private void Clear()
		{
			mHasInitialized = false;
			HasNewMessage = false;
			HasNewAssignment = false;
			mUserMessageCount.Clear();
			mNewMessageFlags.Clear();
			mNextFetchTime = -1;
		}

		void Update()
		{
			if (!mHasInitialized)
			{
				// Potentially needs to rebind after clearing
				mHasInitialized = true;
				DataManager.Status.OnDataUpdate += RefreshStatus;
			}

			if (DataManager.Me.Data == null)
				return;
			
			if (Time.time > mNextFetchTime || mForceUpdate)
			{
				DataManager.Status.FetchData(true);
				mNextFetchTime = Time.time + FETCH_INTERVAL;
				mForceUpdate = false;
			}
		}

		public static void ForceUpdate()
		{
			mForceUpdate = true;
		}

		private static void RefreshStatus()
		{
			List<int> usersToRemove = new List<int>(mNewMessageFlags.Keys);
			mUserMessageCount.Clear();

			for (int i = 0; i < DataManager.Status.Data.Length; i++)
			{
				DataGlobalStatus status = DataManager.Status.Data[i];
				mUserMessageCount[status.UserID] = status.NewMessagesFromContact;
				usersToRemove.Remove(status.UserID);

				if (status.NewMessagesFromContact > 0)
					mNewMessageFlags[status.UserID] = true;
				if (status.NewAssignmentsFromContact > 0)
					HasNewAssignment = true;
			}

			for (int i = 0; i < usersToRemove.Count; i++)
			{
				mNewMessageFlags.Remove(usersToRemove[i]);
			}

			RefreshGlobalNewMessage();

			if (OnStatusUpdate != null)
				OnStatusUpdate();
		}

		private static void RefreshGlobalNewMessage()
		{
			HasNewMessage = false;

			foreach (bool flag in mNewMessageFlags.Values)
			{
				if (flag)
				{
					HasNewMessage = true;
					break;
				}
			}
		}

		public static bool HasMessages(int userID)
		{
			if (!mNewMessageFlags.ContainsKey(userID))
				return false;
			return mNewMessageFlags[userID];
		}

		public static void SetSeenMessages(int userID)
		{
			mNewMessageFlags[userID] = false;
			RefreshGlobalNewMessage();

			if (OnStatusUpdate != null)
				OnStatusUpdate();
		}

		public static void SetSeenAssignments()
		{
			HasNewAssignment = false;
		}
	}
}
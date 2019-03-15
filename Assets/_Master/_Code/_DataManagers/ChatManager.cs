using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using System;

namespace ius
{
	public static class ChatManager 
	{
		private static Dictionary<int, DataChat> mChats = new Dictionary<int, DataChat>();

		/// <summary> Called when a chat is received, parameter is the contact's id </summary>
		public static event Action<int> OnGetChat;

		static ChatManager()
		{
			Backend.OnGetChatSuccess += OnGetChatSuccess;
			Backend.OnLogOut += OnLogOut;
		}

		public static void FetchChat(int contactID)
		{
			Backend.GetChat(contactID);
		}

		private static void OnGetChatSuccess(string json)
		{
			Dictionary<string, object> data = Json.Deserialize<Dictionary<string, object>>(json);

			DataChat chat = new DataChat();
			chat.SetData(data);
			int contactID = -1;

			for (int i = 0; i < chat.Users.Length; i++)
			{
				if (!chat.Users[i].IsMe)
				{
					contactID = chat.Users[i].ID;
					mChats[contactID] = chat;
				}
			}

			if (OnGetChat != null)
				OnGetChat(contactID);
		}

		/// <summary> Get chat based on contact id. Returns null if there's no such chat. </summary>
		public static DataChat GetChat(int contactID)
		{
			if (!mChats.ContainsKey(contactID))
				return null;

			return mChats[contactID];
		}

		private static void OnLogOut()
		{
			mChats.Clear();
		}
	}
}
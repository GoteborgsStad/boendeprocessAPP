using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class DataChat : DataNameObject 
	{
		public DataUser[] Users { get; private set; }
		public DataChatMessage[] Messages { get; private set; }

		public DataUser Contact { get; private set; }

		private const string KEY_USERS = "users";
		private const string KEY_MESSAGES = "chat_messages";

		public override void SetData(Dictionary<string, object> data)
		{
			base.SetData(data);

			List<object> userData = GetList(data, KEY_USERS);
			Users = new DataUser[userData.Count];

			for (int i = 0; i < Users.Length; i++)
			{
				Users[i] = DataUser.GetUser(userData[i] as Dictionary<string, object>);
			}
			
			Messages = DataChatMessage.ListFromData(GetList(data, KEY_MESSAGES));

			for (int i = 0; i < Users.Length; i++)
			{
				if (!Users[i].IsMe)
				{
					Contact = Users[i];
					break;
				}
			}
		}
	}
}
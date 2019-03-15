using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class DataChatMessage : DataBaseObject 
	{
		public string Body { get; private set; }
		public DataUser User { get; private set; }

		private const string KEY_BODY = "body";
		private const string KEY_USER = "user";

		public override void SetData(Dictionary<string, object> data)
		{
			base.SetData(data);

			Body = Get<string>(data, KEY_BODY);
			User = DataUser.GetUser(GetDict(data, KEY_USER));
		}

		public static DataChatMessage[] ListFromData(List<object> data)
		{
			DataChatMessage[] result = new DataChatMessage[data.Count];

			for (int i = 0; i < data.Count; i++)
			{
				result[i] = new DataChatMessage();
				result[i].SetData(data[i] as Dictionary<string, object>);
			}

			return result;
		}
	}
}
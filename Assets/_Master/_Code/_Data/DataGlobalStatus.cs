using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class DataGlobalStatus : IDataSettable
	{
		public int UserID { get; private set; }
		public DataKeyValue[] Status { get; private set; }

		public int NewMessagesFromContact			{ get; private set; }
		public int NewAssignmentsFromContact		{ get; private set; }

		protected const string KEY_USER_ID = "id";
		protected const string KEY_STATUS = "global_statuses";

		protected const string KEY_MESSAGE_CONTACT = "new_message_amount_from_contact";
		protected const string KEY_ASSIGNMENT_NEW = "new_assignment_amount_from_contact";

		public void SetData(Dictionary<string, object> data)
		{
			UserID = DataParser.GetInt(data, KEY_USER_ID);

			Status = DataParser.ArrayFromData<DataKeyValue>(data, KEY_STATUS);
			Dictionary<string, int> statusDictionary = new Dictionary<string, int>(Status.Length);

			for (int i = 0; i < Status.Length; i++)
			{
				int statusValue;

				if (int.TryParse(Status[i].Value, out statusValue))
					statusDictionary[Status[i].Key] = statusValue;
			}

			NewMessagesFromContact = statusDictionary.Get(KEY_MESSAGE_CONTACT, 0);
			NewAssignmentsFromContact = statusDictionary.Get(KEY_ASSIGNMENT_NEW, 0);
		}
	}
}
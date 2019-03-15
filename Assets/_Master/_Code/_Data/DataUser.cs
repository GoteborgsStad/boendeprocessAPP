using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class DataUser : DataBaseObject
	{
		// This class uses a static dictionary approach instead of a bunch of different instances of the same user
		// GetUser: 
		//		if a user with that id does not exist, create it and return the new instance
		//		if a user with that id exists, update that user with the new data and return the user instance

		public bool IsMe { get; private set; }

		public string PersonalIdentityNumber { get; private set; }
		public int ContactCount { get; private set; }
		public int AdolescentCount { get; private set; }

		public DataUserDetails UserDetails { get; private set; }

		private const string KEY_IS_ME = "is_me";
		private const string KEY_PERSONAL_ID = "personal_identity_number";
		private const string KEY_CONTACT_COUNT = "amount_of_contacts";
		private const string KEY_ADOLESCENT_COUNT = "amount_of_adolescents";
		private const string KEY_USER_DETAILS = "user_detail";

		private static Dictionary<int, DataUser> mUsers = new Dictionary<int, DataUser>();

		protected DataUser() { } // Only DataUser.GetUser should call this

		public override void SetData(Dictionary<string, object> data)
		{
			base.SetData(data);

			IsMe = Get<bool>(data, KEY_IS_ME);
			PersonalIdentityNumber = Get<string>(data, KEY_PERSONAL_ID);
			ContactCount = GetInt(data, KEY_CONTACT_COUNT);
			AdolescentCount = GetInt(data, KEY_ADOLESCENT_COUNT);

			if (data.ContainsKey(KEY_USER_DETAILS))
				UserDetails = DataUserDetails.GetDetails(GetDict(data, KEY_USER_DETAILS));
		}

		/// <summary> If a user with the id does not exist, create it and return the new instance. 
		/// If a user with that id exists, update that user with the new data and return the user instance. </summary>
		public static DataUser GetUser(Dictionary<string, object> data)
		{
			int id = GetInt(data, KEY_ID);

			if (mUsers.ContainsKey(id))
			{
				DataUser user = mUsers[id];
				user.SetData(data);
				return user;
			}

			DataUser newUser = new DataUser();
			newUser.SetData(data);
			mUsers[id] = newUser;
			return newUser;
		}

		public static DataUser GetUser(int id)
		{
			if (!mUsers.ContainsKey(id))
			{
				Debug.LogWarning("Found no user with ID: " + id);
				return null;
			}

			return mUsers[id];
		}

		public static DataUser[] GetUserArray(List<object> data)
		{
			DataUser[] result = new DataUser[data.Count];

			for (int i = 0; i < data.Count; i++)
			{
				result[i] = GetUser(data[i] as Dictionary<string, object>);
			}

			return result;
		}
	}
}
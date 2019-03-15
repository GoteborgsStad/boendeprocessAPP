using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class DataMe : DataUser
	{
		public DataNameObject UserRole { get; private set; }
		public DataKeyValue[] UserConfig { get; private set; }
		public DataRelationship[] Relationships { get; private set; }
		public DataUser[] Contacts { get; private set; }

		public Dictionary<string, bool> ConfigFlags { get; private set; }

		private const string KEY_USER_ROLE = "user_role";
		private const string KEY_USER_CONFIG = "user_configurations";
		private const string KEY_RELATIONSHIPS = "parent_relationships";
		private const string KEY_CONTACTS = "contacts";

		public override void SetData(Dictionary<string, object> data)
		{
			base.SetData(data);

			UserRole = CreateDataNameObject(data, KEY_USER_ROLE);
			UserConfig = ArrayFromData<DataKeyValue>(data, KEY_USER_CONFIG);
			Relationships = ArrayFromData<DataRelationship>(data, KEY_RELATIONSHIPS);
			Contacts = GetUserArray(GetList(data, KEY_CONTACTS));

			ConfigFlags = new Dictionary<string, bool>(UserConfig.Length);

			for (int i = 0; i < UserConfig.Length; i++)
			{
				int flagValue;

				if (int.TryParse(UserConfig[i].Value, out flagValue))
					ConfigFlags[UserConfig[i].Key] = flagValue != 0;
			}
		}
	}
}
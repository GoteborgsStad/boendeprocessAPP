using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class DataRelationship : DataBaseObject 
	{
		public DataUser Contact { get; private set; }

		private const string KEY_CONTACT = "parent";

		public override void SetData(Dictionary<string, object> data)
		{
			base.SetData(data);

			Contact = DataUser.GetUser(GetDict(data, KEY_CONTACT));
		}
	}
}
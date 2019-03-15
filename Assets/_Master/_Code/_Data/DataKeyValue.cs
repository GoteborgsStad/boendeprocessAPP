using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class DataKeyValue : DataBaseObject 
	{
		public string Key { get; private set; }
		public string Value { get; private set; }

		private const string KEY_KEY = "key";
		private const string KEY_VALUE = "value";

		public override void SetData(Dictionary<string, object> data)
		{
			base.SetData(data);

			Key = Get<string>(data, KEY_KEY);
			Value = Get<string>(data, KEY_VALUE);
		}
	}
}
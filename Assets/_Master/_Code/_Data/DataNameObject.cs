using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class DataNameObject : DataBaseObject
	{
		public string Name { get; private set; }
		public string Description { get; private set; }
		public string Color { get; private set; }

		private const string KEY_NAME = "name";
		private const string KEY_DESCRIPTION = "description";
		private const string KEY_COLOR = "color";

		public override void SetData(Dictionary<string, object> data)
		{
			base.SetData(data);

			Name = Get<string>(data, KEY_NAME);
			Description = Get<string>(data, KEY_DESCRIPTION);

			if (data.ContainsKey(KEY_COLOR))
				Color = Get<string>(data, KEY_COLOR);
		}
	}
}
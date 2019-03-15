using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class DataEvaluationAnswer : DataBaseObject
	{
		public string Body { get; private set; }
		public int Rating { get; private set; }
		public DataNameObject Category { get; private set; }

		private const string KEY_BODY = "body";
		private const string KEY_RATING = "rating";
		private const string KEY_CATEGORY = "evaluation_answer_category";

		public override void SetData(Dictionary<string, object> data)
		{
			base.SetData(data);

			Body = Get<string>(data, KEY_BODY);
			Rating = GetInt(data, KEY_RATING);
			Category = CreateDataNameObject(data, KEY_CATEGORY);
		}
	}
}
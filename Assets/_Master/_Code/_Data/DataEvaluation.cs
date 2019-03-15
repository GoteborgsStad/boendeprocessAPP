using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class DataEvaluation : DataNameObject 
	{
		public DataNameObject Status { get; private set; }
		public DataEvaluationAnswer[] Answers { get; private set; }

		private const string KEY_STATUS = "evaluation_status";
		private const string KEY_ANSWERS = "evaluation_answers";

		public override void SetData(Dictionary<string, object> data)
		{
			base.SetData(data);

			Status = CreateDataNameObject(data, KEY_STATUS);
			Answers = ArrayFromData<DataEvaluationAnswer>(data, KEY_ANSWERS);
		}
	}
}
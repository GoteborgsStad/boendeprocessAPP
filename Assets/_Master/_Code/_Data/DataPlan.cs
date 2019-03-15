using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class DataPlan : DataNameObject
	{
		public DataGoal[] Goals { get; private set; }

		private const string KEY_GOALS = "goals";

		public override void SetData(Dictionary<string, object> data)
		{
			base.SetData(data);

			Goals = ArrayFromData<DataGoal>(data, KEY_GOALS);
		}
	}
}
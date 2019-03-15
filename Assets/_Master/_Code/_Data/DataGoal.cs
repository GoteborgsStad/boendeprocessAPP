using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ius
{
	public class DataGoal : DataNameObject
	{
		private enum GoalStatusID : int
		{
			New = 1,
			InPhase = 2,
			SoonDeadline = 3,
			Deadline = 4,
			PastDeadline = 5,
			Done = 6
		}

		public DateTime? StartAt { get; private set; }
		public DateTime? EndAt { get; private set; }
		public DateTime? FinishedAt { get; private set; }
		
		public DataNameObject Category { get; private set; }
		public DataNameObject StatusObject { get; private set; }

		public int Status { get { return StatusObject.ID; } }
		public virtual bool IsDone { get { return CheckStatus(GoalStatusID.Done); } }
		public virtual bool NeedAttention { get { return CheckStatus(GoalStatusID.SoonDeadline, GoalStatusID.Deadline, GoalStatusID.PastDeadline); } }
		public virtual bool IsPastDeadline { get { return CheckStatus(GoalStatusID.PastDeadline); } }

		private const string KEY_START_AT = "start_at";
		private const string KEY_END_AT = "end_at";
		private const string KEY_FINISHED_AT = "finished_at";
		
		private const string KEY_CATEGORY = "_category";
		private const string KEY_STATUS = "_status";

		protected virtual string KEY_TYPE { get { return "goal"; } }

		public override void SetData(Dictionary<string, object> data)
		{
			base.SetData(data);

			StartAt = GetTime(data, KEY_START_AT);
			EndAt = GetTime(data, KEY_END_AT);
			FinishedAt = GetTime(data, KEY_FINISHED_AT);

			Category = CreateDataNameObject(data, KEY_TYPE + KEY_CATEGORY);
			StatusObject = CreateDataNameObject(data, KEY_TYPE + KEY_STATUS);
		}

		private bool CheckStatus(params GoalStatusID[] isAny)
		{
			for (int i = 0; i < isAny.Length; i++)
			{
				if (Status == (int)isAny[i])
					return true;
			}

			return false;
		}
	}
}
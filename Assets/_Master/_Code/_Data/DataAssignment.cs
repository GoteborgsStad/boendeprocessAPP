using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ius
{
	public class DataAssignment : DataGoal
	{
		private enum AssignmentStatusID : int
		{
			New = 1,
			InPhase = 2,
			SoonDeadline = 3,
			Deadline = 4,
			PastDeadline = 5,
			AwaitingAcceptance = 6,
			Done = 7
		}

		public DateTime? AcceptedAt { get; private set; }
		public string ImageURL { get; private set; }
		public DataNameObject[] Forms { get; private set; }
		public string DescriptionImageURL { get; private set; }

		public bool IsWaiting { get { return CheckStatus(AssignmentStatusID.AwaitingAcceptance); } }
		public bool IsSubmitted { get { return AcceptedAt.HasValue || IsWaiting; } }
		public override bool IsDone { get { return CheckStatus(AssignmentStatusID.Done); } }
		public override bool NeedAttention { get { return CheckStatus(AssignmentStatusID.SoonDeadline, AssignmentStatusID.Deadline, AssignmentStatusID.PastDeadline); } }
		public override bool IsPastDeadline { get { return CheckStatus(AssignmentStatusID.PastDeadline); } }

		private const string KEY_ACCEPTED_AT = "accepted_at";
		private const string KEY_IMAGE_URL = "image_url";
		private const string KEY_FORM = "assignment_forms";
		private const string KEY_DESCRIPTION_IMAGE = "image_description_url";

		protected override string KEY_TYPE { get { return "assignment"; } }

		public override void SetData(Dictionary<string, object> data)
		{
			base.SetData(data);

			AcceptedAt = GetTime(data, KEY_ACCEPTED_AT);
			ImageURL = Get<string>(data, KEY_IMAGE_URL);
			Forms = ArrayFromData<DataNameObject>(data, KEY_FORM);
			DescriptionImageURL = Get<string>(data, KEY_DESCRIPTION_IMAGE);
		}

		private bool CheckStatus(params AssignmentStatusID[] isAny)
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
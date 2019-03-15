using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class ScreenAssignment : ScreenBase 
	{
		[SerializeField] [TextTag] private string mHeaderTag;
		[SerializeField] private PageNavigation mNavigation;
		[SerializeField] private UISlideElement mSlideArea;
		[SerializeField] private UIPoolScroll mOngoing;
		[SerializeField] private UIPoolScroll mSubmitted;

		private static bool mEnterLeft;
		private static bool mCameFromSubmitted;

		public static void ReturnFromInspect(bool isSubmitted)
		{
			mCameFromSubmitted = isSubmitted;
			mEnterLeft = true;
			UIManager.Open(UILocation.Assignment);
		}

		void OnEnable()
		{
			GlobalStatus.OnStatusUpdate += CheckStatus;
			DataManager.Assignment.OnDataUpdate += RefreshContent;

			DataManager.Assignment.FetchData();
		}

		void OnDisable()
		{
			GlobalStatus.OnStatusUpdate -= CheckStatus;
			DataManager.Assignment.OnDataUpdate -= RefreshContent;
		}

		private void CheckStatus()
		{
			if (GlobalStatus.HasNewAssignment)
			{
				GlobalStatus.SetSeenAssignments();
				DataManager.Assignment.FetchData(true);
			}
		}

		public override void RefreshUI()
		{
			mNavigation.SetPage(mCameFromSubmitted ? 1 : 0);

			mSlideArea.SetExitEdge(RectTransform.Edge.Top);
			mSlideArea.SetEnterEdge(mEnterLeft ? RectTransform.Edge.Left : RectTransform.Edge.Top);
			mEnterLeft = false;

			UIHeader.Show(mHeaderTag);

			RefreshContent();
		}

		private void RefreshContent()
		{
			if (DataManager.Assignment.Data == null)
			{
				mOngoing.Initialize(new DataBaseObject[0], null);
				mSubmitted.Initialize(new DataBaseObject[0], null);
				return;
			}

			GlobalStatus.SetSeenAssignments();

			List<DataAssignment> ongoingAssignments = new List<DataAssignment>();
			List<DataAssignment> submittedAssignments = new List<DataAssignment>();

			for (int i = 0; i < DataManager.Assignment.Data.Length; i++)
			{
				if (DataManager.Assignment.Data[i].IsSubmitted)
					submittedAssignments.Add(DataManager.Assignment.Data[i]);
				else
					ongoingAssignments.Add(DataManager.Assignment.Data[i]);
			}

			ongoingAssignments.Sort((a, b) => a.EndAt.Value.CompareTo(b.EndAt.Value));

			mOngoing.Initialize(ongoingAssignments.ToArray(), InspectOngoing);
			mSubmitted.Initialize(submittedAssignments.ToArray(), InspectSubmitted);
		}
		
		private void InspectOngoing(int id)
		{
			mSlideArea.SetExitEdge(RectTransform.Edge.Left);
			ScreenInspectAssignment.Open(DataManager.Assignment.GetAssignment(id));
		}

		private void InspectSubmitted(int id)
		{
			mSlideArea.SetExitEdge(RectTransform.Edge.Left);
			ScreenInspectAssignmentDone.Open(DataManager.Assignment.GetAssignment(id));
		}
	}
}
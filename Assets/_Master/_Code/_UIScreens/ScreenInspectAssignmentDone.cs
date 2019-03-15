using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	public class ScreenInspectAssignmentDone : ScreenBase 
	{
		[SerializeField] [TextTag] private string mHeaderTag;

		[Header("Box")]
		[SerializeField] private Image[] mBoxImages;
		[SerializeField] private Sprite mBoxDone;
		[SerializeField] private Sprite mBoxAttention;

		[Header("Description - Title")]
		[SerializeField] private Image mTitleIcon;
		[SerializeField] private Sprite mAssignmentSprite;
		[SerializeField] private Sprite mActivitySprite;
		[SerializeField] private Text mTitle;
		[SerializeField] private Text mCategory;

		[Header("Description - Content")]
		[SerializeField] private Image mContentIcon;
		[SerializeField] private Text mContent;
		
		private static DataAssignment mCurrentAssignment;
		private static bool mCameFromPlant;

		public static void Open(DataAssignment assignment, bool fromPlant = false)
		{
			mCurrentAssignment = assignment;
			mCameFromPlant = fromPlant;

			UIManager.Open(UILocation.InspectAssignmentDone);
		}

		public override void RefreshUI()
		{
			UIHeader.Show(mHeaderTag, OnClickHeader);

			bool isActivity = mCurrentAssignment.EndAt.HasValue && mCurrentAssignment.StartAt.HasValue;
			mTitleIcon.sprite = isActivity ? mActivitySprite : mAssignmentSprite;
			mTitle.text = mCurrentAssignment.Name;
			mCategory.text = mCurrentAssignment.Category.Name;
			mContent.text = AssignmentContentFormat.Create(mCurrentAssignment, false);

			for (int i = 0; i < mBoxImages.Length; i++)
			{
				mBoxImages[i].sprite = mCurrentAssignment.IsWaiting ? mBoxAttention : mBoxDone;
			}
		}

		private void OnClickHeader()
		{
			if (mCameFromPlant)
				UIManager.Open(UILocation.Progress);
			else
				ScreenAssignment.ReturnFromInspect(true);
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace ius
{
	public class ScreenInspectAssignment : ScreenBase 
	{
		[SerializeField] [TextTag] private string mHeaderTag;

		[Header("Box")]
		[SerializeField] private Image mBoxImage;
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
		[SerializeField] private GameObject[] mDescriptionImageButton;

		[Header("Submission")]
		[SerializeField] private SubmitAssignmentPage mSubmission;

		private static DataAssignment mCurrentAssignment;
		private static bool mCameFromPlant;
				
		public static void Open(DataAssignment assignment, bool fromPlant = false)
		{
			mCurrentAssignment = assignment;
			mCameFromPlant = fromPlant;

			UIManager.Open(UILocation.InspectAssignment);
		}

		public override void RefreshUI()
		{
			UIHeader.Show(mHeaderTag, OnClickHeader);

			bool isActivity = mCurrentAssignment.EndAt.HasValue && mCurrentAssignment.StartAt.HasValue;
			mTitleIcon.sprite = isActivity ? mActivitySprite : mAssignmentSprite;
			mTitle.text = mCurrentAssignment.Name;
			mCategory.text = mCurrentAssignment.Category.Name;
			mContent.text = AssignmentContentFormat.Create(mCurrentAssignment, false);

			// Remaining timespan
			mBoxImage.sprite = mCurrentAssignment.NeedAttention ? mBoxAttention : mBoxDone;
			
			SetDescriptionImageButton(!string.IsNullOrEmpty(mCurrentAssignment.DescriptionImageURL));

			mSubmission.SetAssignment(mCurrentAssignment);
		}

		private void SetDescriptionImageButton(bool state)
		{
			for (int i = 0; i < mDescriptionImageButton.Length; i++)
			{
				mDescriptionImageButton[i].SetActive(state);
			}
		}

		public void ButtonDescriptionImage()
		{
			if (!string.IsNullOrEmpty(mCurrentAssignment.DescriptionImageURL))
				Application.OpenURL(mCurrentAssignment.DescriptionImageURL);
		}

		private void OnClickHeader()
		{
			if (mCameFromPlant)
				UIManager.Open(UILocation.Progress);
			else
				ScreenAssignment.ReturnFromInspect(false);
		}
	}
}
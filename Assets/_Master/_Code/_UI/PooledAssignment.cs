using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Globalization;

namespace ius
{
	public class PooledAssignment : UIPoolScrollElement
	{
		[SerializeField] private UILayoutBase mLayout;
		
		[Header("Box")]
		[SerializeField] private Image mBoxImage;
		[SerializeField] private Sprite mBoxInProgress;
		[SerializeField] private Sprite mBoxDone;
		[SerializeField] private Sprite mBoxAttention;

		[Header("Title")]
		[SerializeField] private Image mTopIcon;
		[SerializeField] private Sprite mAssignmentSprite;
		[SerializeField] private Sprite mActivitySprite;
		[SerializeField] private Text mTitleText;
		[SerializeField] private Text mCategoryText;

		[Header("Content")]
		[SerializeField] private Image mBotIcon;
		[SerializeField] private Text mContentText;

		private DataAssignment mData;
		
		void Awake()
		{
			UIFunctions.UnregisterGraphic(mTopIcon, mTitleText, mCategoryText, mBotIcon, mContentText);
		}

		public override void SetData(DataBaseObject data)
		{
			mData = data as DataAssignment;
			
			bool isActivity = mData.EndAt.HasValue && mData.StartAt.HasValue;
			mTopIcon.sprite = isActivity ? mActivitySprite : mAssignmentSprite;

			mTitleText.text = mData.Name;
			mCategoryText.text = mData.Category.Name;
			
			mContentText.text = AssignmentContentFormat.Create(mData, true);
			
			mLayout.PerformLayout();

			UpdateBox();
		}

		private void UpdateBox()
		{
			if (!mData.IsSubmitted)
			{
				mBoxImage.sprite = mData.NeedAttention ? mBoxAttention : mBoxInProgress;
			}
			else
			{
				mBoxImage.sprite = mData.IsWaiting ? mBoxAttention : mBoxDone;
			}
		}

		public void ButtonPress()
		{
			SendClick(mData.ID);
		}
	}
}
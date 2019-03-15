using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class ScreenInfo : ScreenBase 
	{
		[SerializeField] [TextTag] private string mHeaderTag;
		[SerializeField] private PageNavigation mNavigation;
		[SerializeField] private ExpandableInfoBox mFAQPrefab;
		[SerializeField] private RectTransform mContentRoot;
		[SerializeField] private RectTransform mInfoRoot;
		[SerializeField] private RectTransform mAfterRoot;

		private ExpandableInfoBox[] mFAQEntries;

		void OnEnable()
		{
			DataManager.FAQ.OnDataUpdate += RefreshFAQ;
			mNavigation.OnSwitchStart += OnNavigatePage;

			mContentRoot.SetToTop();
			mInfoRoot.SetToTop();
			mAfterRoot.SetToTop();

			DataManager.FAQ.FetchData();
		}

		void OnDisable()
		{
			DataManager.FAQ.OnDataUpdate -= RefreshFAQ;
			mNavigation.OnSwitchStart -= OnNavigatePage;
		}

		public override void RefreshUI()
		{
			UIHeader.Show(mHeaderTag);

			RefreshFAQ();
		}

		private void OnNavigatePage()
		{
			// Reset scroll view position
			mContentRoot.SetToTop();

			// Reset FAQ entries
			if (mFAQEntries != null)
			{
				for (int i = 0; i < mFAQEntries.Length; i++)
				{
					mFAQEntries[i].SetState(false);
				}
			}
		}

		private void Clear()
		{
			if (mFAQEntries != null)
			{
				for (int i = 0; i < mFAQEntries.Length; i++)
				{
					Destroy(mFAQEntries[i].gameObject);
				}
			}

			mFAQEntries = null;
		}

		private void RefreshFAQ()
		{
			if (DataManager.FAQ.Data == null)
				return;

			Clear();

			mFAQEntries = new ExpandableInfoBox[DataManager.FAQ.Data.Length];

			for (int i = 0; i < DataManager.FAQ.Data.Length; i++)
			{
				DataNameObject faq = DataManager.FAQ.Data[i];
				mFAQEntries[i] = Instantiate(mFAQPrefab, mContentRoot);
				mFAQEntries[i].Initialize(faq.Name, faq.Description);
			}

			for (int i = 0; i < mFAQEntries.Length; i++)
			{
				mFAQEntries[i].SetState(false);
			}
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	public class ExpandableInfoBox : MonoBehaviour
	{
		[SerializeField] private Text mTitleText;
		[SerializeField] private Text mContentText;
		[SerializeField] private GameObject mArrowDown;
		[SerializeField] private GameObject mArrowUp;

		private bool mIsOpen;
		private RectTransform mTransform;

		private string mAnswer;

		public void Initialize(string question, string answer)
		{
			mTitleText.text = question;
			mAnswer = answer;
		}

		void OnEnable()
		{
			SetState(false);
		}

		public void SetState(bool isOpen)
		{
			mIsOpen = isOpen;

			if (mArrowDown != null)
				mArrowDown.SetActive(!mIsOpen);
			if (mArrowUp != null)
				mArrowUp.SetActive(mIsOpen);

			mContentText.enabled = mIsOpen;
			mContentText.text = mIsOpen ? mAnswer : string.Empty;

			if (mTransform == null)
				mTransform = GetComponent<RectTransform>();

			LayoutRebuilder.ForceRebuildLayoutImmediate(mTransform);
		}

		public void ButtonPress()
		{
			SetState(!mIsOpen);
		}
	}
}
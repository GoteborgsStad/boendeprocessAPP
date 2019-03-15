using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace ius
{
	public class UIHeader : MonoBehaviour
	{
		[SerializeField] private UISlideElement mSlideElement;
		[SerializeField] private Text mTextDisplay;
		[SerializeField] private GameObject mBackSymbol;
		[SerializeField] private LayoutElement mBackLayout;

		private float mBackSize;

		private GameObject mGameObject;
		private Action mOnClick;
		private bool mShouldShowBack;
		private float mBackTimer;

		private static UIHeader Instance;

		private const float BACK_SYMBOL_SPEED = 5f;

		void Awake()
		{
			Instance = this;
			mGameObject = gameObject;
			mBackSize = mBackLayout.minWidth;
			mBackLayout.minWidth = 0;
			mBackSymbol.SetActive(false);
		}

		public static void Hide()
		{
			//Instance.mGameObject.SetActive(false);
			Instance.mSlideElement.BeginMoveOut();
		}

		public static void Show(string headerTag)
		{
			Instance.mOnClick = null;
			Instance.ShowTag(headerTag);
		}

		public static void Show(string headerTag, Action onClick)
		{
			Instance.mOnClick = onClick;
			Instance.ShowTag(headerTag);
		}

		private void ShowTag(string headerTag)
		{
			mGameObject.SetActive(true);
			mTextDisplay.text = TextManager.Get(headerTag);

			mShouldShowBack = mOnClick != null;

			if (mShouldShowBack)
				mBackSymbol.SetActive(true);

			mSlideElement.BeginMoveIn();
		}

		public void ButtonPress()
		{
			if (!UIManager.IsTransition && mOnClick != null)
			{
				mOnClick();
				mOnClick = null;
			}
		}
		
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape) && mOnClick != null)
			{
				mOnClick();
				mOnClick = null;
			}

			if (mShouldShowBack && mBackTimer < 1f)
			{
				mBackTimer += Time.deltaTime * BACK_SYMBOL_SPEED;
				mBackTimer = Mathf.Clamp01(mBackTimer);
				mBackLayout.minWidth = mBackTimer * mBackSize;
			}
			else if (!mShouldShowBack && mBackTimer > 0f)
			{
				mBackTimer -= Time.deltaTime * BACK_SYMBOL_SPEED;
				mBackTimer = Mathf.Clamp01(mBackTimer);
				mBackLayout.minWidth = mBackTimer * mBackSize;

				if (mBackTimer <= 0f)
					mBackSymbol.SetActive(false);
			}
		}
	}
}
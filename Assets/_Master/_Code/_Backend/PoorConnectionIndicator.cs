using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ius
{
	public class PoorConnectionIndicator : MonoBehaviour 
	{
		[SerializeField] private GameObject mIndicator;
		[SerializeField] private UnityEngine.UI.Text mInfoText;
		[SerializeField] private RectTransform mRect;
		[SerializeField] private float mMinSize = 60f;
		[SerializeField] private float mMaxSize = 150f;

		private bool mIsPoorConnection;
		private float mDisplayTimer;
		private bool mShowInfo;

		private static float mLastReport = -5000f;

		private static PoorConnectionIndicator Instance;

		private const float CHANGE_SPEED = 8f;

		void Awake()
		{
			Instance = this;
			mIndicator.SetActive(false);
			mInfoText.enabled = false;
			mRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mMinSize);
		}

		void Update()
		{
			if (mShowInfo && mDisplayTimer < 1f)
			{
				mDisplayTimer += Time.deltaTime * CHANGE_SPEED;
				mRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(mMinSize, mMaxSize, mDisplayTimer));

				if (mDisplayTimer >= 1f)
					mInfoText.enabled = true;
			}
			else if (!mShowInfo && mDisplayTimer > 0f)
			{
				mDisplayTimer -= Time.deltaTime * CHANGE_SPEED;
				mRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(mMinSize, mMaxSize, mDisplayTimer));

				if (mInfoText.enabled)
					mInfoText.enabled = false;
			}
			else
			{
				enabled = false;
			}
		}
		
		private void SetState(bool isPoor)
		{
			if (isPoor == mIsPoorConnection)
				return;

			mIsPoorConnection = isPoor;
			mIndicator.SetActive(mIsPoorConnection);
			enabled = true;

			if (!mIsPoorConnection)
			{
				mShowInfo = false;
				mRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mMinSize);
			}
		}

		public void ButtonClickIndicator()
		{
			mShowInfo = !mShowInfo;
			enabled = true;
		}

		public static void ReportTimeout()
		{
			if ((Time.time - mLastReport) < 20f)
				Instance.SetState(true);
			mLastReport = Time.time;
		}

		public static void ReportSuccess()
		{
			Instance.SetState(false);
			mLastReport = -5000f;
		}
	}
}
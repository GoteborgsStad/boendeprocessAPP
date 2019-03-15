using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace ius
{
	public class PopupManager : MonoBehaviour
	{
		[SerializeField] private UIFadeElement mBackground;
		[SerializeField] private UISlideElement mPopupSlide;

		[Header("Popup Content")]
		[SerializeField] private Text mBodyText;
		[SerializeField] private Text mButtonText;

		private bool mIsShowing;
		private Action mOnButton;

		private static PopupManager Instance;

		void Awake()
		{
			Instance = this;
		}

		public static void DisplayPopup(string body, string button, Action onButton)
		{
			if (UIManager.CurrentLocation != UILocation.Login)
				Instance.ShowPopup(body, button, onButton);
		}

		private void ShowPopup(string body, string button, Action onButton)
		{
			if (mIsShowing)
				return;

			mIsShowing = true;

			mBodyText.text = body;
			mButtonText.text = button;
			mOnButton = onButton;

			mBackground.SetState(true);
			mPopupSlide.BeginMoveIn();
		}

		private void HidePopup()
		{
			mIsShowing = false;
			mBackground.SetState(false);
			mPopupSlide.BeginMoveOut();
		}

		public void ButtonClickBackground()
		{

		}

		public void ButtonClickPopup()
		{
			if (mIsShowing && mOnButton != null)
				mOnButton();

			HidePopup();
		}
	}
}
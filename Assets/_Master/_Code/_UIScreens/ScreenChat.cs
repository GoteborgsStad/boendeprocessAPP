using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	public class ScreenChat : ScreenBase 
	{
		[SerializeField] [TextTag] private string mHeaderTag;
		[SerializeField] private PageNavigation mNavigation;
		[SerializeField] private ContactPage mContactPage;
		[SerializeField] private ChatPage mChatPage;
		[SerializeField] private InputField mFeedbackField;

		private bool mIsInitialized;

		private DataUser mCurrentContact;

		public override void RefreshUI()
		{
			UIHeader.Show(mHeaderTag);
		}

		void OnEnable()
		{
			if (!mIsInitialized)
			{
				mIsInitialized = true;
				mContactPage.Initialize(this);
			}

			mNavigation.OnSwitchDone += OnSwitchPage;
			ChatManager.OnGetChat += OnGetChat;
			DataManager.Me.OnDataUpdate += OnMyDataUpdated;

			DataManager.Me.FetchData(true);

			mChatPage.Close();
			mContactPage.Open();
		}

		void OnDisable()
		{
			mNavigation.OnSwitchDone -= OnSwitchPage;
			ChatManager.OnGetChat -= OnGetChat;
			DataManager.Me.OnDataUpdate -= OnMyDataUpdated;

			mCurrentContact = null;
		}

		void Update()
		{
			if (mCurrentContact != null && GlobalStatus.HasMessages(mCurrentContact.ID))
			{
				GlobalStatus.SetSeenMessages(mCurrentContact.ID);
				ChatManager.FetchChat(mCurrentContact.ID);
			}
		}

		private void OnGetChat(int contactID)
		{
			if (mCurrentContact != null && contactID == mCurrentContact.ID)
				mChatPage.Refresh();
		}

		private void OnMyDataUpdated()
		{
			mContactPage.Refresh();
		}
		
		public void ChatWith(DataUser contact)
		{
			mCurrentContact = contact;
			GlobalStatus.SetSeenMessages(mCurrentContact.ID);
			ChatManager.FetchChat(mCurrentContact.ID);

			mContactPage.Close();
			mChatPage.Open(mCurrentContact);

			mChatPage.Refresh();
			
			UIHeader.Show(mHeaderTag, BackToContacts);
		}

		private void BackToContacts()
		{
			mChatPage.Close();
			mContactPage.Open();
			UIHeader.Show(mHeaderTag);
		}

		public void SendFeedback()
		{
			string feedback = mFeedbackField.text;
			bool shouldSend = !string.IsNullOrEmpty(feedback.Trim());
			mFeedbackField.text = string.Empty;

			if (shouldSend)
				Backend.SendFeedback(feedback);
		}

		private void OnSwitchPage()
		{
			if (mNavigation.CurrentPage != 0)
			{
				mChatPage.Close();
				mContactPage.Open();
				UIHeader.Show(mHeaderTag);
			}
		}
	}
}
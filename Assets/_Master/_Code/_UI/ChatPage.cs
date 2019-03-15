using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace ius
{
	public class ChatPage : MonoBehaviour
	{
		[SerializeField] private Button mSendButton;

		[Header("Contact Info")]
		[SerializeField] private ProfileImage mProfileImage;
		[SerializeField] private Text mNameText;
		[SerializeField] private Text mPhoneText;
		[SerializeField] private Text mMailText;

		[Header("Chat Log")]
		[SerializeField] private RectTransform mChatLogRoot;
		[SerializeField] private ChatUIEntry mEntryOther;
		[SerializeField] private ChatUIEntry mEntryMe;
		[SerializeField] private InputField mInputField;

		private GameObject mGameObject;
		private DataUser mCurrentContact;

		private List<ChatUIEntry> mChatLog = new List<ChatUIEntry>(50);
		private HashSet<int> mListedEntries = new HashSet<int>();
		
		void OnEnable()
		{
			Backend.OnSendChatMessageSuccess += OnSendMessageSuccess;
			MessageFieldUpdated();
		}

		void OnDisable()
		{
			Backend.OnSendChatMessageSuccess -= OnSendMessageSuccess;
			mCurrentContact = null;
		}

		public void Open(DataUser contact)
		{
			mCurrentContact = contact;
			SetState(true);

			mProfileImage.SetDetails(mCurrentContact.UserDetails);

			mNameText.text = mCurrentContact.UserDetails.FullName;
			mPhoneText.text = mCurrentContact.UserDetails.CellPhone;
			mMailText.text = mCurrentContact.UserDetails.Email;
			
			mEntryOther.gameObject.SetActive(false);
			mEntryMe.gameObject.SetActive(false);

			Clear();

			Refresh();
		}

		public void Close()
		{
			SetState(false);
			Clear();
		}
		
		private void SetState(bool state)
		{
			if (mGameObject == null)
				mGameObject = gameObject;
			mGameObject.SetActive(state);
		}

		public void Refresh()
		{
			DataChat chat = ChatManager.GetChat(mCurrentContact.ID);

			if (chat == null)
			{
				Clear();
				return;
			}
						
			bool addedNewMessage = false;

			// Add new messages
			for (int i = 0; i < chat.Messages.Length; i++)
			{
				DataChatMessage message = chat.Messages[i];

				if (mListedEntries.Contains(message.ID))
					continue; // Message is already in the log

				mListedEntries.Add(message.ID);

				bool shouldAppend = false;

				// Check if this message should be appended to the latest entry or create new entry
				if (mChatLog.Count > 0)
				{
					ChatUIEntry lastEntry = mChatLog[mChatLog.Count - 1];

					if (lastEntry.FromUser.ID == message.User.ID 
					&& (message.CreatedAt.Value - lastEntry.LastMessageTime) < TimeSpan.FromMinutes(4))
					{
						shouldAppend = true;
					}
				}

				if (shouldAppend)
				{
					mChatLog[mChatLog.Count - 1].AddMessage(message);
				}
				else
				{
					ChatUIEntry newEntry = Instantiate(message.User.IsMe ? mEntryMe : mEntryOther, mChatLogRoot);
					mChatLog.Add(newEntry);
					newEntry.Initialize(message);
				}

				addedNewMessage = true;
			}

			if (addedNewMessage)
			{
				Vector2 position = mChatLogRoot.anchoredPosition;
				position.y = 0;
				mChatLogRoot.anchoredPosition = position;
			}
		}

		private void Clear()
		{
			mListedEntries.Clear();

			for (int i = 0; i < mChatLog.Count; i++)
			{
				Destroy(mChatLog[i].gameObject);
			}

			mChatLog.Clear();
		}

		public void ButtonSendMessage()
		{
			string message = mInputField.text;
			bool shouldSend = !string.IsNullOrEmpty(message.Trim());
			mInputField.text = string.Empty;
			
			if (shouldSend)
				Backend.SendChatMessage(ChatManager.GetChat(mCurrentContact.ID).ID, message);
		}

		public void MessageFieldUpdated()
		{
			mSendButton.interactable = mInputField.text != null && mInputField.text.Length > 0;
		}

		private void OnSendMessageSuccess(string response)
		{
			ChatManager.FetchChat(mCurrentContact.ID);
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	[RequireComponent(typeof(Button))]
	public class ContactButton : MonoBehaviour 
	{
		[SerializeField] private ProfileImage mProfileImage;
		[SerializeField] private Text mNameText;
		[SerializeField] private NewMessageIndicator mMessageIndicator;

		private ScreenChat mChat;
		private DataUser mContact;
		private GameObject mGameObject;

		public void Initialize(ScreenChat screenChat, DataUser contact)
		{
			SetState(true);

			mChat = screenChat;
			mContact = contact;
			mNameText.text = contact.UserDetails.FullName;
			
			mMessageIndicator.Initialize(contact);

			mProfileImage.SetDetails(contact.UserDetails);
		}

		public void Hide()
		{
			SetState(false);
		}

		private void SetState(bool state)
		{
			if (mGameObject == null)
				mGameObject = gameObject;

			mGameObject.SetActive(state);
		}

		public void ButtonClick()
		{
			mChat.ChatWith(mContact);
		}
	}
}
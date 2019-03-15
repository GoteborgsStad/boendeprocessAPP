using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace ius
{
	public class ChatUIEntry : MonoBehaviour 
	{
		[SerializeField] private ProfileImage mProfileImage;

		[SerializeField] private Text mNameText;
		[SerializeField] private Text mTimeText;
		[SerializeField] private Text mChatText;

		public DataUser FromUser { get; private set; }
		public DateTime LastMessageTime { get; private set; }
		
		private const string TIME_FORMAT = "dd MMM HH:mm";

		private const string NAME_COLOR_ME = "#01C0D1FF";
		private const string NAME_COLOR_OTHER = "#0077BCFF";

		public void Initialize(DataChatMessage message)
		{
			FromUser = message.User;
			LastMessageTime = message.CreatedAt.Value;

			mProfileImage.SetDetails(message.User.UserDetails);

			mNameText.text = message.User.UserDetails.FullName;

			Color nameColor;
			ColorUtility.TryParseHtmlString(message.User.IsMe ? NAME_COLOR_ME : NAME_COLOR_OTHER, out nameColor);
			mNameText.color = nameColor;

			mTimeText.text = message.CreatedAt.Value.ToString(TIME_FORMAT);
			mChatText.text = message.Body;

			gameObject.SetActive(true);
		}

		public void AddMessage(DataChatMessage message)
		{
			LastMessageTime = message.CreatedAt.Value;
			mChatText.text += "\n" + message.Body;
		}
	}
}
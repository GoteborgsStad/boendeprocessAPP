using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace ius
{
	public class ScreenSettings : ScreenBase 
	{
		[SerializeField] [TextTag] private string mHeaderTag;
		[SerializeField] private PageNavigation mNavigation;

		[Header("Email")]
		[SerializeField] private InputField mMailField;
		[SerializeField] private Text mInvalidMailMessage;
		[SerializeField] private Button mMailSave;

		[Header("Photo")]
		[SerializeField] private GameObject mPhotoRoot;
		[SerializeField] private GameObject mPlaceholderRoot;
		[SerializeField] private RawImage mPhotoDisplay;
		[SerializeField] private Button mPhotoRemove;
		[SerializeField] private Button mPhotoSave;

		[Header("Notifications")]
		[SerializeField] private Toggle[] mNotificationToggles;
		[SerializeField] private Button mNotificationSave;

		[Header("Sign out")]
		[SerializeField] private Text mSignOutText;
		[SerializeField] private GameObject mSignOutConfirmSymbol;
		[SerializeField] [TextTag] string mTagSignOut;
		[SerializeField] [TextTag] string mTagConfirmSignOut;

		private Texture2D mSelectedPhoto;
		private bool mHasUpdatedPhoto;
		private bool mHasUpdateEmail;
		private bool mHasUpdatedNotifications;

		private bool mIsSigningOut;

		private static readonly string[] NOTIFICATION_KEYS = new string[]
		{
			"notification_contact_new_assignment",
			"notification_assignment_end_date_near",
			"notification_contact_finished_assignment",
			"notification_contact_new_chat_message",
			"notification_contact_new_evaluation"
		};

		public override void RefreshUI()
		{
			UIHeader.Show(mHeaderTag);

			DataManager.Me.FetchData();
			RefreshContent();
		}

		void OnEnable()
		{
			ResetContent();

			DataManager.Me.OnDataUpdate += RefreshContent;
			mNavigation.OnSwitchDone += ResetContent;

			if (DataManager.Me.Data != null)
				DataManager.Me.Data[0].UserDetails.OnGetImage += RefreshContent;
		}

		void OnDisable()
		{
			ResetContent();

			DataManager.Me.OnDataUpdate -= RefreshContent;
			mNavigation.OnSwitchDone -= ResetContent;

			if (DataManager.Me.Data != null)
				DataManager.Me.Data[0].UserDetails.OnGetImage -= RefreshContent;
		}

		private void ResetContent()
		{
			mInvalidMailMessage.enabled = false;

			mHasUpdatedPhoto = false;
			mHasUpdateEmail = false;
			mHasUpdatedNotifications = false;

			mIsSigningOut = false;
			mSignOutText.text = TextManager.Get(mTagSignOut);
			mSignOutConfirmSymbol.SetActive(false);

			RefreshContent();
		}

		private void RefreshContent()
		{
			if (DataManager.Me.Data == null)
				return;

			RefreshMailContent();
			RefreshPhotoContent();
			RefreshNotificationContent();
		}

		private void RefreshMailContent()
		{
			DataUserDetails userDetails = DataManager.Me.Data[0].UserDetails;

			if (mHasUpdateEmail)
			{
				mMailSave.interactable = userDetails.Email != mMailField.text;
			}
			else
			{
				mMailField.text = userDetails.Email;
				mMailSave.interactable = false;
			}
		}

		private void RefreshPhotoContent()
		{
			DataUserDetails userDetails = DataManager.Me.Data[0].UserDetails;
			Sprite userPhoto = userDetails.Image;

			mPhotoSave.interactable = mHasUpdatedPhoto && mSelectedPhoto != userPhoto;
			mPhotoRemove.interactable = (!mHasUpdatedPhoto && userPhoto != null) 
									 || (mHasUpdatedPhoto && mSelectedPhoto != null);
			
			if (!mHasUpdatedPhoto && userPhoto != null)
			{
				SetPlaceholderState(false);
				mPhotoDisplay.texture = userPhoto.texture;
			}
			else if (mHasUpdatedPhoto && mSelectedPhoto != null)
			{
				SetPlaceholderState(false);
				mPhotoDisplay.texture = mSelectedPhoto;
			}
			else
			{
				SetPlaceholderState(true);
			}
		}

		private void RefreshNotificationContent()
		{
			if (!mHasUpdatedNotifications)
			{
				for (int i = 0; i < mNotificationToggles.Length; i++)
				{
					SetToggle(mNotificationToggles[i], NOTIFICATION_KEYS[i]);
				}

				mNotificationSave.interactable = false;
			}
			else
			{
				bool hasChanged = false;

				for (int i = 0; i < mNotificationToggles.Length; i++)
				{
					if (ToggleHasChanged(mNotificationToggles[i], NOTIFICATION_KEYS[i]))
					{
						hasChanged = true;
						break;
					}
				}

				mNotificationSave.interactable = hasChanged;
			}
		}

		private void SetToggle(Toggle toggle, string key)
		{
			DataMe me = DataManager.Me.Data[0];

			if (!me.ConfigFlags.ContainsKey(key))
			{
				toggle.isOn = false;
				toggle.interactable = false;
			}
			else
			{
				toggle.isOn = me.ConfigFlags[key];
				toggle.interactable = true;
			}
		}

		private bool ToggleHasChanged(Toggle toggle, string key)
		{
			DataMe me = DataManager.Me.Data[0];

			if (!me.ConfigFlags.ContainsKey(key))
				return false;
			return toggle.isOn != me.ConfigFlags[key];
		}

		private void SetPlaceholderState(bool isPlaceholder)
		{
			mPlaceholderRoot.SetActive(isPlaceholder);
			mPhotoRoot.SetActive(!isPlaceholder);
		}

		public void ButtonSaveMail()
		{
			string email = mMailField.text.Trim();
			bool isEmail = RegexUtilities.IsValidEmail(email);

			if (isEmail)
			{
				Backend.ChangeUserEmail(email);
				mInvalidMailMessage.enabled = false;
				mMailSave.interactable = false;
				mHasUpdateEmail = false;
			}
			else
			{
				mInvalidMailMessage.enabled = true;
			}
		}

		public void ButtonPhotoSelect()
		{
			if (Application.isEditor)
				OnGetPhoto(Resources.Load<Texture2D>("test_stick_figure"));
			else
				ImportPhotoHandler.GetPhoto(OnGetPhoto, OnGetPhotoFail);
		}

		public void ButtonPhotoRemove()
		{
			mSelectedPhoto = null;
			mHasUpdatedPhoto = true;
			RefreshPhotoContent();
		}

		public void ButtonPhotoSave()
		{
			mHasUpdatedPhoto = false;
			mPhotoSave.interactable = false;
			Backend.ChangeUserPicture(mSelectedPhoto);
		}

		public void ButtonNotificationSave()
		{
			Dictionary<string, bool> updatedFlags = new Dictionary<string, bool>();

			for (int i = 0; i < mNotificationToggles.Length; i++)
			{
				if (ToggleHasChanged(mNotificationToggles[i], NOTIFICATION_KEYS[i]))
					updatedFlags[NOTIFICATION_KEYS[i]] = mNotificationToggles[i].isOn;
			}

			mHasUpdatedNotifications = false;
			mNotificationSave.interactable = false;
			Backend.ChangeUserConfigFlags(updatedFlags);
		}

		public void FieldMailUpdate()
		{
			mHasUpdateEmail = true;
			mInvalidMailMessage.enabled = false;
			RefreshMailContent();
		}

		public void ToggleNotificationUpdate()
		{
			mHasUpdatedNotifications = true;
			RefreshNotificationContent();
		}

		public void ButtonSignOut()
		{
			if (!mIsSigningOut)
			{
				mIsSigningOut = true;
				mSignOutText.text = TextManager.Get(mTagConfirmSignOut);
				mSignOutConfirmSymbol.SetActive(true);
			}
			else
			{
				Backend.LogOut();
			}
		}

		private void OnGetPhoto(Texture2D photo)
		{
			mSelectedPhoto = PhotoConverter.ConvertToSquare(photo);
			mHasUpdatedPhoto = true;
			RefreshPhotoContent();
		}

		private void OnGetPhotoFail()
		{
			RefreshPhotoContent();
		}
	}
}
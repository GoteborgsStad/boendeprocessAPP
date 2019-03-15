using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_IPHONE || UNITY_EDITOR
using UnityEngine.iOS;
using NotificationServices = UnityEngine.iOS.NotificationServices;
#endif
#if UNITY_ANDROID || UNITY_EDITOR
using Firebase;
using Firebase.Messaging;
#endif

namespace ius
{
	public class PushNotificationController : MonoBehaviour 
	{
		[SerializeField] private UnityEngine.UI.Text mDebugText;

		private string mPushToken;
		private string mPushTokenAppVersion;
		private string mPushUser;
		private string mLastSentToken;

		private bool mIsFetchingToken;

		private const string PREF_REGISTER = "PushToken";
		private const string PREF_LAST_APP_VERSION = "PushTokenAppVersion";
		private const string PREF_LAST_USER = "PushTokenUser";
		private const string PREF_LAST_SENT_TOKEN = "PushTokenSent";
				
		void Start()
		{
			SetDebugText("Push Controller wake up");

			mPushToken = PlayerPrefs.GetString(PREF_REGISTER, null);
			mPushTokenAppVersion = PlayerPrefs.GetString(PREF_LAST_APP_VERSION, null);
			mPushUser = PlayerPrefs.GetString(PREF_LAST_USER, null);
			mLastSentToken = PlayerPrefs.GetString(PREF_LAST_APP_VERSION, null);

			ConnectEvents();

			NextTokenStep();
		}

		void OnDestroy()
		{
#if UNITY_ANDROID || UNITY_EDITOR
			if (!Application.isEditor)
				FirebaseMessaging.TokenReceived -= OnGetFirebaseToken;
#endif
		}

		private void ConnectEvents()
		{
			SetDebugText("Connect events");

			Backend.OnSetPushTokenSuccess += OnSetPushToken;
			Backend.OnSetPushTokenFail += OnSetPushTokenFailed;

			DataManager.Me.OnDataUpdate += NextTokenStep;

			SetDebugText("Events connected");
		}

		private void NextTokenStep()
		{
			if (!CanRegister())
				return;

			SetDebugText("NextTokenStep - Error");

			if (NeedToRegister())
			{
				// Register need to be called when there's no token, there's a new app version
				// or the token belongs to someone else
				SetDebugText("Register for token");
				RegisterForToken();
			}
			else if (NeedToSend())
			{
				// There's a push token which has not been successfully sent to the backend
				SetDebugText("Send token\n" + mPushToken);
				SendToken();
			}
			else
			{
				// Everything is in order
				SetDebugText("Token successfully sent\n" + mPushToken);
			}
		}

		private bool CanRegister()
		{
			return DataManager.Me.Data != null;
		}

		private bool NeedToRegister()
		{
			return string.IsNullOrEmpty(mPushToken)
				   || mPushTokenAppVersion != GetVersionString()
				   || mPushUser != GetUserString();
		}

		private bool NeedToSend()
		{
			return !string.IsNullOrEmpty(mPushToken) && mLastSentToken != mPushToken;
		}

		private void SendToken()
		{
			Backend.SetPushToken(mPushToken);
		}

		private void RegisterForToken()
		{
			if (mIsFetchingToken)
				return;

			mIsFetchingToken = true;
			
			if (!Application.isMobilePlatform || Application.isEditor)
			{
				SetDebugText("Platform unsupported for push noticiations");
			}
			else
			{
				SetDebugText("Register for token - Begin");

#if UNITY_ANDROID || UNITY_EDITOR				
				// Firebase is dependant on Google Services. Prompt user to install/update if unavailable.
				FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(
					task =>
					{
						DependencyStatus dependencyStatus = task.Result;

						if (dependencyStatus == DependencyStatus.Available)
						{
							FirebaseMessaging.TokenReceived += OnGetFirebaseToken;
							FirebaseMessaging.Subscribe("TestTopic");
						}
						else
						{
							OnRegisterFail("Could not resolve all Firebase dependencies: " + dependencyStatus);
						}
					});
#endif

#if UNITY_IPHONE || UNITY_EDITOR
				NotificationServices.RegisterForNotifications(
					NotificationType.Alert |
					NotificationType.Badge |
					NotificationType.Sound);

				StartCoroutine(WaitForIOSToken());
#endif
				SetDebugText("Register for token - Waiting");
			}
		}

#if UNITY_IPHONE || UNITY_EDITOR
		private IEnumerator WaitForIOSToken()
		{
			while (NotificationServices.deviceToken == null)
			{
				yield return null;
			}
			
			// send token to a provider
			string token = System.BitConverter.ToString(NotificationServices.deviceToken).Replace("-", "");
			OnRegister(token);
		}
#endif

#if UNITY_ANDROID || UNITY_EDITOR
		private void OnGetFirebaseToken(object sender, TokenReceivedEventArgs token)
		{
			OnRegister(token.Token);
		}
#endif

		private void OnRegister(string token)
		{
			SetDebugText("On Register");

			mIsFetchingToken = false;

			mPushToken = token;

			GetLocalData();
			SavePushData();

			NextTokenStep(); // Pass on token to backend
		}

		private void GetLocalData()
		{
			mPushTokenAppVersion = GetVersionString();
			mPushUser = GetUserString();
		}

		private string GetVersionString()
		{
			return BuildNumberHolder.BuildNumber;
		}

		private string GetUserString()
		{
			return DataManager.Me.Data[0].ID.ToString(); ;
		}

		private void SavePushData()
		{
			PlayerPrefs.SetString(PREF_REGISTER, mPushToken);
			PlayerPrefs.SetString(PREF_LAST_APP_VERSION, mPushTokenAppVersion);
			PlayerPrefs.SetString(PREF_LAST_USER, mPushUser);
		}

		private void OnRegisterFail(string error)
		{
			// <TODO> Retry?
			SetDebugText("Register for token failed:\n" + error);
		}

		private void OnSetPushToken(string response)
		{
			mLastSentToken = mPushToken;
			PlayerPrefs.SetString(PREF_LAST_SENT_TOKEN, mLastSentToken);
			NextTokenStep();
		}

		private void OnSetPushTokenFailed(WebCall error)
		{
			SetDebugText("Failed setting token\n" + error);
		}

		private void SetDebugText(string text)
		{
			if (mDebugText != null)
				mDebugText.text = text;
		}
	}
}
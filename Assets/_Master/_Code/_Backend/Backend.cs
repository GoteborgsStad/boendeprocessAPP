using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MiniJSON;
using UnityEngine.SceneManagement;

namespace ius
{
	public static class Backend 
	{
		public const bool VERBOSE_DEBUG = false;
		public static bool PrintDebug { get { return Application.isEditor && (Input.GetKey(KeyCode.D) || VERBOSE_DEBUG); } }
		
		private static string BaseURL { get { return EnvironmentConfiguration.GetBackend(); } }
		
		private static class API
		{
			public const string Login			= "/v1/auth/login";
			public const string RefreshToken	= "/v1/auth/refreshtoken";
			public const string Me				= "/v1/au/users/me";
			public const string MeChange		= "/v1/au/users/me/userdetails";
			public const string MeConfigChange	= "/v1/au/users/userconfigurations";
			public const string Status			= "/v1/au/users/globalstatuses";
			public const string Assignment		= "/v1/au/assignments";
			public const string AssignmentDone	= "/v1/au/assignments/{id}/done";
			public const string Chats			= "/v1/au/users/{id}/chats";
			public const string SendChatmessage = "/v1/au/chats/{id}/chatmessages";
			public const string SendFeedback	= "/v1/au/feedback";
			public const string Evaluations		= "/v1/au/evaluations";
			public const string Goals			= "/v1/au/users/plans";
			public const string FAQ				= "/v1/au/faqs";
			public const string UploadPicture	= "/v1/files/images/base64";
			public const string SetPush			= "/v1/devices";
		}
		
		private const string PREF_TOKEN = "Token";

		private static WebCallRoutineObject mRoutineObject;
		private static string mToken;

		public static bool IsLoggedIn { get { return !string.IsNullOrEmpty(mToken); } }
		public static DateTime TokenExpiration { get; private set; }

		#region Response events
		public static Action OnLogOut;

		public static Action OnLogInSuccess;
		public static Action<WebCall> OnLogInFail;
		
		public static Action<string> OnGetMeSuccess;
		public static Action<WebCall> OnGetMeFail;

		public static Action<string> OnGetStatusSuccess;
		public static Action<WebCall> OnGetStatusFail;

		public static Action<string> OnGetAssignmentsSuccess;
		public static Action<WebCall> OnGetAssignmentsFail;

		public static Action<string> OnSubmitAssignmentsSuccess;
		public static Action<WebCall> OnSubmitAssignmentsFail;

		public static Action<string> OnGetChatSuccess;
		public static Action<WebCall> OnGetChatFail;

		public static Action<string> OnSendChatMessageSuccess;
		public static Action<WebCall> OnSendChatMessageFail;

		public static Action<string> OnGetEvaluationsSuccess;
		public static Action<WebCall> OnGetEvaluationsFail;

		public static Action<string> OnGetPlanSuccess;
		public static Action<WebCall> OnGetPlanFail;

		public static Action<string> OnGetFAQsSuccess;
		public static Action<WebCall> OnGetFAQsFail;

		public static Action<string> OnSetPushTokenSuccess;
		public static Action<WebCall> OnSetPushTokenFail;
		#endregion

		static Backend()
		{
			CreateRoutineObject();

			mToken = PlayerPrefs.GetString(PREF_TOKEN, string.Empty);
		}

		private static void CreateRoutineObject()
		{
			if (mRoutineObject != null)
				GameObject.Destroy(mRoutineObject.gameObject);
			mRoutineObject = new GameObject("WebRoutineHelper").AddComponent<WebCallRoutineObject>();
		}

		private static Dictionary<string, string> CreateBaseHeaders()
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			RefreshHeaderToken(headers);
			return headers;
		}

		public static void RefreshHeaderToken(Dictionary<string, string> headers)
		{
			headers["Authorization"] = "Bearer " + mToken;
		}

		public static void ExecuteWebCall(WebCall webCall)
		{
			if (mRoutineObject == null)
				CreateRoutineObject();

			webCall.ExecuteCall(mRoutineObject);
		}
		
		public static void LogOut()
		{
			StopAllCalls();

			mToken = null;
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();

			// Redirect to sign in screen
			if (OnLogOut != null)
				OnLogOut();
			else
				Debug.LogWarning("OnNeedLogIn not handled");

			IUSAuthentication.LogOut(OnLogOutDone);

			if (Application.isEditor)
				OnLogOutDone();
		}

		private static void OnLogOutDone()
		{
			mRoutineObject.StartCoroutine(ReloadScene());
		}

		private static IEnumerator ReloadScene()
		{
			yield return new WaitForSeconds(2f);
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		public static void StopAllCalls()
		{
			CreateRoutineObject();
		}

		public static void CallURL(string url, Action<WebCall> onDone)
		{
			WebCall call = new WebCall(url, onDone);
			ExecuteWebCall(call);
		}

		#region Web calls
		public static void LogIn(string personalID)
		{
			Dictionary<string, object> postData = new Dictionary<string, object>()
			{
				{ "personal_identity_number", personalID },
				{ "password", "secret" }
			};

			WebCall call = new WebCall(BaseURL + API.Login, null, postData, HandleLogIn);

			ExecuteWebCall(call);
		}

		public static void RefreshLogIn()
		{
			WebCall call = new WebCall(BaseURL + API.RefreshToken, CreateBaseHeaders(), HandleRefreshLogIn);
			ExecuteWebCall(call);
		}

		public static void GetMe()
		{
			WebCall call = new WebCall(BaseURL + API.Me, CreateBaseHeaders(), HandleGetMe);
			ExecuteWebCall(call);
		}

		public static void GetStatus()
		{
			WebCall call = new WebCall(BaseURL + API.Status, CreateBaseHeaders(), HandleGetStatus);
			ExecuteWebCall(call);
		}

		/// <summary> Change user email, then automatically execute GetMe(). </summary>
		public static void ChangeUserEmail(string newMail)
		{
			Dictionary<string, object> postData = new Dictionary<string, object>()
			{
				{ "email", newMail }
			};

			WebCall call = new WebCall(BaseURL + API.MeChange, CreateBaseHeaders(), postData, (x) => DataManager.Me.FetchData(true), true);
			ExecuteWebCall(call);
		}

		/// <summary> Change user profile picture, then automatically execute GetMe(). </summary>
		public static void ChangeUserPicture(Texture2D picture)
		{
			WebCall call = new WebCall(BaseURL + API.MeChange, CreateBaseHeaders(), null, (x) => DataManager.Me.FetchData(true), true);

			if (picture != null)
				call.SetImage(picture);
			else
				call.PostDictionary["image_uuid"] = "remove";

			ExecuteWebCall(call);
		}

		/// <summary> Change notification flags according to data, then automatically execute GetMe(). </summary>
		public static void ChangeUserConfigFlags(Dictionary<string, bool> flags)
		{
			int i = 0;

			foreach (KeyValuePair<string, bool> entry in flags)
			{
				i++;
				ChangeUserConfigFlag(entry.Key, entry.Value, i == flags.Count);
			}
		}

		private static void ChangeUserConfigFlag(string flag, bool value, bool updateMe)
		{
			Dictionary<string, object> postData = new Dictionary<string, object>();

			postData["key"] = flag;
			postData["value"] = value ? "1" : "0";

			WebCall call = new WebCall(
				BaseURL + API.MeConfigChange, 
				CreateBaseHeaders(), 
				postData, 
				(x) =>
					{
						if (updateMe)
							DataManager.Me.FetchData(true);
					}, 
				true);
			ExecuteWebCall(call);
		}

		public static void GetAssignments()
		{
			WebCall call = new WebCall(BaseURL + API.Assignment, CreateBaseHeaders(), HandleGetAssignments);
			ExecuteWebCall(call);
		}

		public static void SubmitAssignment(int id, string text, Texture2D picture)
		{
			Dictionary<string, object> postData = new Dictionary<string, object>();

			if (!string.IsNullOrEmpty(text))
				postData["answer"] = text;

			string url = BaseURL + API.AssignmentDone.Replace("{id}", id.ToString());
			WebCall call = new WebCall(url, CreateBaseHeaders(), postData, HandleSubmitAssignment);

			if (picture != null)
				call.SetImage(picture);

			ExecuteWebCall(call);
		}

		public static void UploadImage(string imageData, string imageName, Action<string> onDone)
		{
			Dictionary<string, object> postData = new Dictionary<string, object>()
			{
				{ "base64_image", imageData },
				{ "image_name", imageName }
			};

			WebCall call = new WebCall(BaseURL + API.UploadPicture, CreateBaseHeaders(), postData,
				(x) =>
				{
					if (x.IsSuccess)
					{
						Dictionary<string, object> jsonData = Json.Deserialize<Dictionary<string, object>>(x.ResponseData);
						string imageUUID = jsonData["uuid"] as string;
						onDone(imageUUID);
					}
					else
					{
						onDone(null);
					}
				}, 
				false, true);

			ExecuteWebCall(call);
		}
		
		public static void GetChat(int contactID)
		{
			string url = BaseURL + API.Chats.Replace("{id}", contactID.ToString());
			WebCall call = new WebCall(url, CreateBaseHeaders(), HandleGetChat);
			ExecuteWebCall(call);
		}

		public static void SendChatMessage(int chatID, string body)
		{
			string url = BaseURL + API.SendChatmessage.Replace("{id}", chatID.ToString());

			Dictionary<string, object> postData = new Dictionary<string, object>()
			{
				{ "body", body }
			};

			WebCall call = new WebCall(url, CreateBaseHeaders(), postData, HandleSendChatMessage);
			ExecuteWebCall(call);
		}
		
		public static void SendFeedback(string feedback)
		{
			Dictionary<string, object> postData = new Dictionary<string, object>()
			{
				{ "body", feedback }
			};

			WebCall call = new WebCall(BaseURL + API.SendFeedback, CreateBaseHeaders(), postData, null);
			ExecuteWebCall(call);
		}

		public static void GetEvaluations()
		{
			WebCall call = new WebCall(BaseURL + API.Evaluations, CreateBaseHeaders(), HandleGetEvaluations);
			ExecuteWebCall(call);
		}
		
		public static void GetPlan()
		{
			WebCall call = new WebCall(BaseURL + API.Goals, CreateBaseHeaders(), HandleGetGoals);
			ExecuteWebCall(call);
		}

		public static void GetFAQs()
		{
			WebCall call = new WebCall(BaseURL + API.FAQ, CreateBaseHeaders(), HandleGetFAQs);
			ExecuteWebCall(call);
		}

		public static void SetPushToken(string token)
		{
			Dictionary<string, object> postData = new Dictionary<string, object>()
			{
				{ "token", token },
				{
					"type",
#if UNITY_IPHONE
					"ios"
#else
					"android"
#endif
				},
				{ "app", "sambuh" }
			};

			WebCall call = new WebCall(BaseURL + API.SetPush, CreateBaseHeaders(), postData, HandleSetPushToken);
			ExecuteWebCall(call);
		}
		#endregion

		public static void SetBackendToken(string token)
		{
			mToken = token;
			PlayerPrefs.SetString(PREF_TOKEN, mToken);

			string tokenPayload = JWT.Decode(token);
			Dictionary<string, object> payload = Json.Deserialize<Dictionary<string, object>>(tokenPayload);

			TokenExpiration = new DateTime((long)payload["exp"]);
		}

		#region Handle response
		private static void SimpleCallHandler(WebCall webCall, Action<string> onSuccess, Action<WebCall> onFail)
		{
			if (webCall.IsSuccess)
			{
				if (onSuccess != null)
					onSuccess(webCall.ResponseData);
			}
			else if (onFail != null)
			{
				onFail(webCall);
			}
		}

		private static void HandleLogIn(WebCall webCall)
		{
			if (!webCall.IsSuccess)
			{
				if (OnLogInFail != null)
					OnLogInFail(webCall);
				return;
			}

			Dictionary<string, object> jsonData = Json.Deserialize<Dictionary<string, object>>(webCall.ResponseData);
			string token = jsonData["token"] as string;

			string tokenPayload = JWT.Decode(token);
			Dictionary<string, object> payloadJson = Json.Deserialize<Dictionary<string, object>>(tokenPayload);

			string role = payloadJson["role"] as string;
			if (role == "AU")
			{
				SetBackendToken(token);

				// All is well, continue into the app
				if (OnLogInSuccess != null)
					OnLogInSuccess();
			}
			else
			{
				// Faulty role, return to log in
				webCall.SetRoleError(role);

				if (OnLogInFail != null)
					OnLogInFail(webCall); // Faulty role, should never happen
			}
		}

		private static void HandleRefreshLogIn(WebCall webCall)
		{
			Dictionary<string, object> jsonData = Json.Deserialize<Dictionary<string, object>>(webCall.ResponseData);
			string token = jsonData["token"] as string;
			SetBackendToken(token);
		}

		private static void HandleGetMe(WebCall webCall)
		{
			SimpleCallHandler(webCall, OnGetMeSuccess, OnGetMeFail);
		}

		private static void HandleGetStatus(WebCall webCall)
		{
			SimpleCallHandler(webCall, OnGetStatusSuccess, OnGetStatusFail);
		}

		private static void HandleGetAssignments(WebCall webCall)
		{
			SimpleCallHandler(webCall, OnGetAssignmentsSuccess, OnGetAssignmentsFail);
		}

		private static void HandleSubmitAssignment(WebCall webCall)
		{
			SimpleCallHandler(webCall, OnSubmitAssignmentsSuccess, OnSubmitAssignmentsFail);
			DataManager.Assignment.FetchData(true);
		}

		private static void HandleGetChat(WebCall webCall)
		{
			SimpleCallHandler(webCall, OnGetChatSuccess, OnGetChatFail);
		}

		private static void HandleSendChatMessage(WebCall webCall)
		{
			SimpleCallHandler(webCall, OnSendChatMessageSuccess, OnSendChatMessageFail);
		}

		private static void HandleGetEvaluations(WebCall webCall)
		{
			SimpleCallHandler(webCall, OnGetEvaluationsSuccess, OnGetEvaluationsFail);
		}

		private static void HandleGetGoals(WebCall webCall)
		{
			SimpleCallHandler(webCall, OnGetPlanSuccess, OnGetPlanFail);
		}

		private static void HandleGetFAQs(WebCall webCall)
		{
			SimpleCallHandler(webCall, OnGetFAQsSuccess, OnGetFAQsFail);
		}

		private static void HandleSetPushToken(WebCall webCall)
		{
			SimpleCallHandler(webCall, OnSetPushTokenSuccess, OnSetPushTokenFail);
		}
#endregion
	}
}
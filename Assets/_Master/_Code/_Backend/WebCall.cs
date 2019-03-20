using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using MiniJSON;

namespace ius
{
	public class WebCall 
	{
		public int UserID { get; private set; }
		public string URL { get; private set; }
		public string PostJSON { get; private set; }
		public Dictionary<string, object> PostDictionary { get; private set; }
		public Dictionary<string, string> Headers { get; private set; }
		public string ImageData { get; private set; }
		public string ImageName { get; private set; }

		public bool IsDone { get; private set; }
		public bool IsSuccess { get; private set; }
		public int StatusCode { get; private set; }
		public string Error { get; private set; }

		public string ResponseData { get { return mWWW.text; } }
		public Texture2D ResponseImage { get { return mWWW.texture; } }

		public string SafePostJSON { get { return string.IsNullOrEmpty(PostJSON) ? string.Empty : PostJSON; } }

		private Action<WebCall> mOnDone;
		private WWW mWWW;
		private bool mShouldRetry;
		private bool mIsQueued;

		private const float TIME_OUT = 20f;

		#region Constructors and serialization
		private const string KEY_URL = "URL";
		private const string KEY_USER = "UserID";
		private const string KEY_HEADERS = "Headers";
		private const string KEY_POST_DATA = "PostData";
		private const string KEY_IMAGE_DATA = "ImageData";
		private const string KEY_IMAGE_NAME = "ImageName";

		private const string KEY_POST_IMAGE = "image_uuid";

		public WebCall(string url, Dictionary<string, string> headers, Dictionary<string, object> postData, Action<WebCall> onDone, bool isPatch = false, bool isImage = false)
		{
			SetUserID();

			URL = url;
			mOnDone = onDone;

			Headers = headers != null ? headers : new Dictionary<string, string>();

			Headers["type"] = "POST";

			if (isPatch)
				Headers["X-HTTP-Method-Override"] = "PATCH";

			Headers["Content-Type"] = "application/json";

			if (postData == null)
				postData = new Dictionary<string, object>();

			PostDictionary = postData;
			PostJSON = Json.Serialize(PostDictionary);

			// Retry any post which is not an image
			mShouldRetry = Backend.IsLoggedIn && !isImage;
		}

		public WebCall(string url, Dictionary<string, string> headers, Action<WebCall> onDone)
		{
			SetUserID();

			URL = url;
			PostDictionary = null;
			mOnDone = onDone;

			Headers = headers != null ? headers : new Dictionary<string, string>();
			Headers["type"] = "GET";
		}

		public WebCall(string url, Action<WebCall> onDone)
		{
			SetUserID();

			URL = url;
			PostDictionary = null;
			mOnDone = onDone;

			Headers = new Dictionary<string, string>();
			Headers["type"] = "GET";
		}

		private void SetUserID()
		{
			if (DataManager.Me.Data != null)
				UserID = DataManager.Me.Data[0].ID;
			else
				UserID = -1;
		}

		public void SetImage(Texture2D image)
		{
			byte[] byteData = image.EncodeToPNG();
			string base64Data = Convert.ToBase64String(byteData);
			ImageData = "data:image/png;base64," + base64Data;
			ImageName = image.name;
		}

		public string Serialize()
		{
			Dictionary<string, object> data = new Dictionary<string, object>();
			data[KEY_URL] = URL;
			data[KEY_USER] = UserID;
			data[KEY_HEADERS] = Json.Serialize(Headers);
			data[KEY_POST_DATA] = PostJSON;
			data[KEY_IMAGE_DATA] = ImageData;
			data[KEY_IMAGE_NAME] = ImageName;

			return Json.Serialize(data);
		}

		// Note: onDone actions can not be stored, avoid using a serialized version for retry unless necessary
		public WebCall(string serializedCall)
		{
			Dictionary<string, object> data = Json.Deserialize<Dictionary<string, object>>(serializedCall);
			URL = data[KEY_URL] as string;
			UserID = (int)((long)data[KEY_USER]);

			Dictionary<string, object> headerData = Json.Deserialize<Dictionary<string, object>>(data[KEY_HEADERS] as string);
			Headers = new Dictionary<string, string>();

			foreach (KeyValuePair<string, object> entry in headerData)
			{
				Headers[entry.Key] = entry.Value as string;
			}

			PostJSON = data[KEY_POST_DATA] as string;
			PostDictionary = Json.Deserialize<Dictionary<string, object>>(PostJSON);
			ImageData = data[KEY_IMAGE_DATA] as string;
			ImageName = data[KEY_IMAGE_NAME] as string;
		}
		#endregion

		public void MarkAsQueued()
		{
			mShouldRetry = false; // At this point it's already in the queue
			mIsQueued = true;
		}

		public IEnumerator GETRoutine(WebCallRoutineObject routineObject)
		{
			IsDone = false;
			
			bool isTimeout = false;

			if (Input.GetKey(KeyCode.Q))
				isTimeout = true;
			
			if (!isTimeout)
			{
				// Everything is fine, prepare to perform the call
				if (Backend.PrintDebug)
					Debug.Log("=> " + URL + "\nHeaders: " + Json.Serialize(Headers));

				// Execute the request
				mWWW = new WWW(URL, null, Headers);

				float timeout = Time.time + TIME_OUT;

				// Wait until the request is done or has timed out
				while (!mWWW.isDone && Time.time < timeout)
				{
					yield return null;
				}

				isTimeout = !mWWW.isDone;
			}

			IsDone = true;
			
			// If the routine object is null the system has reset, probably because of a log out
			// Ignore whatever result should have occured then
			if (routineObject != null)
			{
				if (isTimeout)
					HandleTimeout();
				else
					HandleResult();
			}

			if (isTimeout)
				PoorConnectionIndicator.ReportTimeout();
			else
				PoorConnectionIndicator.ReportSuccess();
		}

		public void ExecuteCall(WebCallRoutineObject routineObject)
		{
			if (PostDictionary != null)
				routineObject.StartCoroutine(POSTRoutine(routineObject));
			else
				routineObject.StartCoroutine(GETRoutine(routineObject));
		}

		public IEnumerator POSTRoutine(WebCallRoutineObject routineObject)
		{
			IsDone = false;

			#region An image is supposed to be included, first upload the image
			string imageUUID = null;

			if (PostDictionary != null && PostDictionary.ContainsKey(KEY_POST_IMAGE))
				imageUUID = PostDictionary[KEY_POST_IMAGE] as string;

			if (string.IsNullOrEmpty(imageUUID) && !string.IsNullOrEmpty(ImageData))
			{
				bool imageIsDone = false;

				Backend.UploadImage(ImageData, ImageName,
					x =>
					{
						imageUUID = x;
						imageIsDone = true;
					});

				while (!imageIsDone) { yield return null; }
			}
			#endregion

			// If there should be an image but the upload failed...
			bool isImageFail = !string.IsNullOrEmpty(ImageData) && string.IsNullOrEmpty(imageUUID);
			bool isTimeout = false;

			if (Input.GetKey(KeyCode.Q))
				isTimeout = true;

			// ... then don't perform the remainder of the post
			if (!isImageFail && !isTimeout)
			{
				// Everything is fine, prepare to perform the call

				// If there's post data construct a byte[] to post
				byte[] postData = null;

				if (PostDictionary != null)
				{
					// Add the uploaded image to the data
					if (!string.IsNullOrEmpty(imageUUID))
						PostDictionary[KEY_POST_IMAGE] = imageUUID;

					PostJSON = Json.Serialize(PostDictionary);
					postData = System.Text.Encoding.UTF8.GetBytes(PostJSON);
				}

				if (Backend.PrintDebug)
					Debug.Log("=> " + URL + "\nHeaders: " + Json.Serialize(Headers) + "\nData:" + SafePostJSON);

				// Execute the request
				mWWW = new WWW(URL, postData, Headers);

				float timeout = Time.time + TIME_OUT;

				// Wait until the request is done or has timed out
				while (!mWWW.isDone && Time.time < timeout)
				{
					yield return null;
				}

				isTimeout = !mWWW.isDone;
			}

			IsDone = true;

			// If the routine object is null the system has reset, probably because of a log out
			// Ignore whatever result should have occured then
			if (routineObject != null)
			{
				if (isImageFail)
					HandleImageFail();
				else if (isTimeout)
					HandleTimeout();
				else
					HandleResult();
			}

			if (isTimeout)
				PoorConnectionIndicator.ReportTimeout();
			else
				PoorConnectionIndicator.ReportSuccess();

			// If this was a failiure and should be retried, add it to the queue
			if (mShouldRetry && (isImageFail || isTimeout || !IsSuccess))
			{
				RetryQueue.Add(this);
			}
		}

		private void HandleResult()
		{
			IsSuccess = string.IsNullOrEmpty(mWWW.error);

			if (!IsSuccess)
			{
				Error = mWWW.error;

				if (!mIsQueued)
					PopupManager.DisplayPopup("Web Error " + StatusCode + "\n" + Error, "OK", null);
			}

			StatusCode = GetResponseCode(mWWW);

			if (Application.isEditor)
			{
				string debugOutput = (IsSuccess ? "Success" : "Failed") + ": " + URL + "\n";

				if (IsSuccess)
					debugOutput += "PostHeaders: " + Json.Serialize(Headers) + "\n" + ResponseData;
				else
					debugOutput += mWWW.error + "\nPostHeaders: " + Json.Serialize(Headers) + "\nPostData: " + SafePostJSON;

				if (!IsSuccess)
				{
					Debug.LogWarning("!= StatusCode: " + StatusCode + " - " + debugOutput);
				}
				else if (Backend.PrintDebug)
				{
					Debug.Log("<= StatusCode: " + StatusCode + " - " + debugOutput);
				}
			}

			// If the token is no longer valid force a sign out 
			if (!IsSuccess && mWWW.error.Contains("Unauthorized"))
			{
				Backend.LogOut();
			}
			else if (mOnDone != null)
			{
				mOnDone(this);
			}
		}

		private void HandleTimeout()
		{
			IsSuccess = false;
			StatusCode = 408; // Timeout code
			Error = "Timeout - Bad connection";

			if (Application.isEditor)
			{
				Debug.LogWarning("!= Timeout: " + URL + "\n" + SafePostJSON);
			}
			
			if (mOnDone != null)
				mOnDone(this);
		}

		private void HandleImageFail()
		{

		}

		public void SetRoleError(string role)
		{
			Error = "Faulty role - " + role;
		}

		private static int GetResponseCode(WWW request)
		{
			int statusCode = -1;

			if (request.responseHeaders == null)
				Debug.LogWarning("No response headers.");
			else if (!request.responseHeaders.ContainsKey("STATUS"))
				Debug.LogWarning("Response headers has no STATUS.\n" + request.url + (!string.IsNullOrEmpty(request.text) ? "\n" + request.text : string.Empty));
			else
				statusCode = ParseResponseCode(request.responseHeaders["STATUS"]);

			return statusCode;
		}

		private static int ParseResponseCode(string statusLine)
		{
			int statusCode = -2;

			string[] components = statusLine.Split(' ');

			if (components.Length < 3)
			{
				Debug.LogError("invalid response status: " + statusLine);
			}
			else if (!int.TryParse(components[1], out statusCode))
			{
				Debug.LogError("invalid response code: " + components[1]);
				statusCode = -3;
			}

			return statusCode;
		}
	}
}
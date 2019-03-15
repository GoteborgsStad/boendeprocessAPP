using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ius
{
	public static class IUSAuthentication
	{
		private enum AuthState
		{
			None,
			Login,
			Logout
		}

		private static WebViewObject mWebView;
		private static Action<string> mOnSuccess;

		private static AuthState mAuthState;

		private const string kKeyLoginStart = "?appToken=";
		
		private static WebViewObject WebView
		{
			get
			{
				if (mWebView == null)
					InitializeWebView();
				return mWebView;
			}
		}

		private static void InitializeWebView()
		{
			mWebView = new GameObject("WebView").AddComponent<WebViewObject>();
			mWebView.Init(cb: OnWebCallback, ld: OnWebLoaded, started: OnWebStarted, enableWKWebView: true,						 
						  err: (x) => WebLog("err", x),
						  httpErr: (x) => WebLog("httpErr", x));
		}

		private static void WebLog(string callback, string message)
		{
			//Debug.Log(callback + ": " + message);
		}

		/// <summary> Displays the login webpage. Calls onSuccess(token). </summary>
		public static void ShowLoginView(Action<string> onSuccess)
		{
			mAuthState = AuthState.Login;
			mOnSuccess = onSuccess;
			WebView.LoadURL(GetURL());
			WebView.SetVisibility(true);
		}

		public static void LogOut(Action onDone)
		{
			mAuthState = AuthState.Logout;
			WebView.LoadURL(EnvironmentConfiguration.GetLogout());
			mOnSuccess = (x) => onDone();
		}

		public static string GetURL()
		{
			return EnvironmentConfiguration.GetLoginFed()
				 + "?RequestBinding=HTTPPost"
				 + "&PartnerId=" + EnvironmentConfiguration.GetLoginPartner()
				 + "&NameIdFormat=Email"
				 + "&AllowCreate=false"
				 + "&Target=" + EnvironmentConfiguration.GetLoginTarget();
		}

		private static void OnWebStarted(string message)
		{
			WebLog("started", message);

			if (mAuthState == AuthState.Login && message.Contains(kKeyLoginStart))
			{
				WebView.SetVisibility(false);

				string[] parts = message.Split(new string[] { kKeyLoginStart }, StringSplitOptions.None);
				SuccessCallback(parts[1]);
			}
		}

		private static void OnWebCallback(string token)
		{
			WebLog("cd", token);

			if (mAuthState == AuthState.Login)
			{
				WebView.SetVisibility(false);
				SuccessCallback(token);
			}
		}

		private static void OnWebLoaded(string message)
		{
			WebLog("ld", message);

			if (mAuthState == AuthState.Logout)
			{
				WebView.SetVisibility(false);
				SuccessCallback(string.Empty);
			}
		}

		private static void SuccessCallback(string token)
		{
			mAuthState = AuthState.None;

			// Clear action before calling, in case a callback results in overwriting these actions
			Action<string> toCall = mOnSuccess;
			mOnSuccess = null;

			if (toCall != null)
				toCall(token);
		}
	}
}
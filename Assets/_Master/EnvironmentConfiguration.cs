using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ius
{
	public static class EnvironmentConfiguration
	{
		// Change this value to redirect backend location
		public static readonly BackendSite TargetSite = BackendSite.Test;

		public enum BackendSite
		{
			Test,
			Live
		}

		private static class URL
		{
			// URL to the backend environment
			public const string BackendTest = "";
			public const string BackendLive = "";

			// URL to the federator login page
			public const string FedLoginTest = "";
			public const string FedLoginLive = "";

			// URL to the federator logout page
			public const string FedLogoutTest = "";
			public const string FedLogoutLive = "";

			// Federator redirect target. Should be the '/saml/master/index.php?acs' page on the current backend.
			public static readonly string LoginTestTarget = GetBackend() + "/saml/test/index.php?acs";
			public static readonly string LoginLiveTarget = GetBackend() + "/saml/master/index.php?acs";

			// Federator partner id. Should be the '/saml/test/metadata.php' page on the current backend.
			public static readonly string LoginTestPartner = GetBackend() + "/saml/test/metadata.php";
			public static readonly string LoginLivePartner = GetBackend() + "/saml/master/metadata.php";
		}

		public static string GetBackend()
		{
			switch (TargetSite)
			{
				case BackendSite.Test: return URL.BackendTest;
				case BackendSite.Live: return URL.BackendLive;

				default:
					Debug.LogError("Unhandled target backend: " + TargetSite);
					return "UNKNOWN";
			}
		}

		public static string GetLoginFed()
		{
			if (TargetSite == BackendSite.Live)
				return URL.FedLoginLive;
			return URL.FedLoginTest;
		}

		public static string GetLoginTarget()
		{
			if (TargetSite == BackendSite.Live)
				return URL.LoginLiveTarget;
			return URL.LoginTestTarget;
		}

		public static string GetLoginPartner()
		{
			if (TargetSite == BackendSite.Live)
				return URL.LoginLivePartner;
			return URL.LoginTestPartner;
		}

		public static string GetLogout()
		{
			if (TargetSite == BackendSite.Live)
				return URL.FedLogoutLive;
			return URL.FedLogoutTest;
		}
	}
}
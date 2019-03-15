using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ius
{
	public static class RegexUtilities
	{
		private static bool invalid = false;

		public static bool IsValidEmail(string strIn)
		{
			invalid = false;
			if (String.IsNullOrEmpty(strIn))
				return false;

			// Use IdnMapping class to convert Unicode domain names.
			strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper, RegexOptions.None);

			if (invalid)
				return false;

			// Return true if strIn is in valid email format.
			return Regex.IsMatch(strIn,
				@"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
				@"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
				RegexOptions.IgnoreCase);
		}

		private static string DomainMapper(Match match)
		{
			// IdnMapping class with default property values.
			IdnMapping idn = new IdnMapping();

			string domainName = match.Groups[2].Value;

			try
			{
				domainName = idn.GetAscii(domainName);
			}
			catch (ArgumentException)
			{
				invalid = true;
			}

			return match.Groups[1].Value + domainName;
		}
	}
}
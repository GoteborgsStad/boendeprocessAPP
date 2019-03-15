using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

namespace ius
{
	public static class JWT 
	{
		public static string Decode(string token)
		{
			string[] parts = token.Split('.');

			if (parts.Length != 3)
			{
				Debug.LogError("JWT must consist of 3 parts delimited by .");
				return null;
			}


			string payload = parts[1];
			string decodedPayload = DecodePart(payload);

			Debug.Log(decodedPayload);

			return decodedPayload;
		}

		private static string DecodePart(string input)
		{
			return Encoding.UTF8.GetString(Base64UrlDecode(input));
		}

		// From JWT spec
		private static byte[] Base64UrlDecode(string input)
		{
			string output = input;
			output = output.Replace('-', '+'); // 62nd char of encoding
			output = output.Replace('_', '/'); // 63rd char of encoding

			switch (output.Length % 4) // Pad with trailing '='s
			{
				case 0: break; // No pad chars in this case
				case 2: output += "=="; break; // Two pad chars
				case 3: output += "="; break;  // One pad char
				default: throw new Exception("Illegal base64url string!");
			}
			
			return Convert.FromBase64String(output); // Standard base64 decoder
		}
	}
}
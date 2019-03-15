using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ius
{
	public static class DataParser
	{
		public static T Get<T>(Dictionary<string, object> data, string key)
		{
			if (!data.ContainsKey(key))
			{
				Debug.LogError("Json data does not contain key: " + key);
				return default(T);
			}
			
			return (T)data[key];
		}

		public static int GetInt(Dictionary<string, object> data, string key)
		{
			long original = Get<long>(data, key);
			return Convert.ToInt32(original);
		}

		public static Dictionary<string, object> GetDict(Dictionary<string, object> data, string key)
		{
			return Get<Dictionary<string, object>>(data, key);
		}

		public static List<object> GetList(Dictionary<string, object> data, string key)
		{
			return Get<List<object>>(data, key);
		}

		public static DataNameObject CreateDataNameObject(Dictionary<string, object> data, string key)
		{
			Dictionary<string, object> nameObjectData = GetDict(data, key);
			DataNameObject nameObject = new DataNameObject();
			nameObject.SetData(nameObjectData);
			return nameObject;
		}

		public static DateTime? GetTime(Dictionary<string, object> data, string key)
		{
			if (!data.ContainsKey(key))
			{
				Debug.LogError("Json data does not contain key: " + key);
				return null;
			}

			// NOTE: The default DateTime.Parse() function is not used due to optimization reasons

			// 2017-01-01 00:00:00

			string dateString = Get<string>(data, key);

			if (string.IsNullOrEmpty(dateString))
				return null;

			int year = int.Parse(dateString.Substring(0, 4));
			int month = int.Parse(dateString.Substring(5, 2));
			int day = int.Parse(dateString.Substring(8, 2));
			int hour = int.Parse(dateString.Substring(11, 2));
			int minute = int.Parse(dateString.Substring(14, 2));
			int second = int.Parse(dateString.Substring(17, 2));

			return new DateTime(year, month, day, hour, minute, second);

			//return DateTime.Parse(data[key].ToString());
		}

		public static string CreateIndentationString(int indentation)
		{
			string indentString = string.Empty;

			for (int i = 0; i < indentation; i++)
			{
				indentString += "\t";
			}

			return indentString;
		}

		public static T[] ArrayFromData<T>(List<object> data) where T : IDataSettable, new()
		{
			T[] result = new T[data.Count];

			for (int i = 0; i < result.Length; i++)
			{
				result[i] = new T();
				result[i].SetData(data[i] as Dictionary<string, object>);
			}

			return result;
		}

		public static T[] ArrayFromData<T>(Dictionary<string, object> data, string key) where T : IDataSettable, new()
		{
			return ArrayFromData<T>(GetList(data, key));
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ius
{
	public class DataBaseObject : IDataSettable
	{
		public int ID { get; private set; }
		public DateTime? CreatedAt { get; private set; }
		public DateTime? UpdatedAt { get; private set; }

		protected const string KEY_ID = "id";
		protected const string KEY_CREATED_AT = "created_at";
		protected const string KEY_UPDATED_AT = "updated_at";
		
		public virtual void SetData(Dictionary<string, object> data)
		{
			ID = GetInt(data, KEY_ID);
			CreatedAt = GetTime(data, KEY_CREATED_AT);
			UpdatedAt = GetTime(data, KEY_UPDATED_AT);
		}
		
		/// <summary> Do NOT use this to fetch int, due to type issues GetInt should be used instead. </summary>
		public static T Get<T>(Dictionary<string, object> data, string key)
		{
			return DataParser.Get<T>(data, key);
		}

		public static int GetInt(Dictionary<string, object> data, string key)
		{
			return DataParser.GetInt(data, key);
		}

		public static Dictionary<string, object> GetDict(Dictionary<string, object> data, string key)
		{
			return DataParser.GetDict(data, key);
		}

		public static List<object> GetList(Dictionary<string, object> data, string key)
		{
			return DataParser.GetList(data, key);
		}

		public static DataNameObject CreateDataNameObject(Dictionary<string, object> data, string key)
		{
			return DataParser.CreateDataNameObject(data, key);
		}

		public static DateTime? GetTime(Dictionary<string, object> data, string key)
		{
			return DataParser.GetTime(data, key);
		}

		public static string CreateIndentationString(int indentation)
		{
			return DataParser.CreateIndentationString(indentation);
		}

		public static T[] ArrayFromData<T>(List<object> data) where T : IDataSettable, new()
		{
			return DataParser.ArrayFromData<T>(data);
		}

		public static T[] ArrayFromData<T>(Dictionary<string, object> data, string key) where T : IDataSettable, new()
		{
			return DataParser.ArrayFromData<T>(data, key);
		}
	}
}
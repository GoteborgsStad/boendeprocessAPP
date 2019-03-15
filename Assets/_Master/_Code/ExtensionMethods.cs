using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public static class ExtensionMethods 
	{
		/// <summary> Get a random element from an array </summary>
		public static T Random<T>(this T[] data)
		{
			if (data.Length == 0)
			{
				Debug.LogWarning("Tried to get a random element from an empty array");
				return default(T);
			}

			return data[UnityEngine.Random.Range(0, data.Length)];
		}

		/// <summary> Get a random value between x and y </summary>
		public static float Random(this Vector2 range)
		{
			return UnityEngine.Random.Range(range.x, range.y);
		}

		/// <summary> Get the full scene location name of this object </summary>
		public static string FullName(this Component component)
		{
			string result = component.name;
			Transform current = component.transform;

			while (current.parent != null)
			{
				current = current.parent;
				result = current.name + "/" + result;
			}

			return result;
		}

		/// <summary> Get the full scene location name of this object </summary>
		public static string FullName(this Transform transform)
		{
			string result = transform.name;
			Transform current = transform;

			while (current.parent != null)
			{
				current = current.parent;
				result = current.name + "/" + result;
			}

			return result;
		}

		/// <summary> Get the full scene location name of this object </summary>
		public static string FullName(this GameObject gameObject)
		{
			string result = gameObject.name;
			Transform current = gameObject.transform;

			while (current.parent != null)
			{
				current = current.parent;
				result = current.name + "/" + result;
			}

			return result;
		}

		public static T Last<T>(this T[] data)
		{
			if (data.Length == 0)
			{
				Debug.LogWarning("Tried to get last element of an empty array");
				return default(T);
			}

			return data[data.Length - 1];
		}

		/// <summary> Get the values corresponding to the key. 
		/// If the key is not present, return defaultValue.</summary>
		public static T2 Get<T1, T2>(this Dictionary<T1, T2> data, T1 key, T2 defaultValue)
		{
			if (!data.ContainsKey(key))
				return defaultValue;
			return data[key];
		}

		public static void SetToTop(this RectTransform rectTransform)
		{
			Vector2 position = rectTransform.anchoredPosition;
			position.y = 0;
			rectTransform.anchoredPosition = position;
		}
	}
}
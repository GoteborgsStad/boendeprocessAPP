using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class TextManager : MonoBehaviour 
	{
		[SerializeField] private TextAsset mSwedish;

		private static Dictionary<string, string> mTexts;
		private static string[] mInspectorTags;

		public const string REFRESH_TAG = "REFRESH";

		/// <summary> Used for Inspector only, these tags are not the actual tags but rather
		/// versions of them formated for improved inspector display. Display these but
		/// take data from AllTags. </summary>
		public static string[] InspectorTags
		{
			get
			{
				if (mInspectorTags == null)
					EditorLoad();
				return mInspectorTags;
			}
		}

		public static string[] AllTags { get; private set; }

		void Awake()
		{
			Load(mSwedish);
		}

		private void Load(TextAsset textAsset)
		{
			string[] lines = textAsset.text.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
			Dictionary<string, string> loaded = new Dictionary<string, string>(lines.Length);
			List<string> allTags = new List<string>();

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];

				if (string.IsNullOrEmpty(line.Trim()))
					continue;

				string[] entry = line.Split(';');

				if (entry.Length != 2)
				{
					Debug.LogWarning("Faulty text - line " + i + ": " + line);
					continue;
				}

				string key = entry[0].Trim();
				string data = entry[1].Replace(@"\n", "\n").Trim();
				
				if (loaded.ContainsKey(key))
				{
					Debug.LogWarning("Duplicate tags - line " + i + ": " + key);
					continue;
				}

				loaded[key] = data;
				allTags.Add(key);
			}

			if (!Application.isPlaying)
				allTags.Add(REFRESH_TAG);

			mInspectorTags = allTags.ToArray();
			AllTags = allTags.ToArray();

			for (int i = 0; i < mInspectorTags.Length; i++)
			{
				mInspectorTags[i] = mInspectorTags[i].Replace('_', '/');
			}

			SetContent(loaded);
		}

		private static void SetContent(Dictionary<string, string> texts)
		{
			mTexts = texts;

			if (Application.isPlaying)
				TextTag.RefreshAll();
		}

		public static void EditorRefresh()
		{
			EditorLoad();
		}

		public static string Get(string textTag)
		{
			if (!string.IsNullOrEmpty(textTag))
				textTag = textTag.Trim();

			if (string.IsNullOrEmpty(textTag))
				return "EMPTY_TAG";

			if (!Application.isPlaying && Application.isEditor) // && mTexts == null)
				EditorLoad();

			if (mTexts == null)
				return textTag;
			
			if (!mTexts.ContainsKey(textTag))
			{
				return "MISSING_TAG:" + textTag;
			}
			
			return mTexts[textTag];
		}

		private static void EditorLoad()
		{
			TextManager instance = FindObjectOfType<TextManager>();

			if (instance == null)
			{
				Debug.LogWarning("No TextManager instance in the scene");
				return;
			}

			instance.Load(instance.mSwedish);
		}
	}
}
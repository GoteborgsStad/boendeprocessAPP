using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


namespace ius
{
	[RequireComponent(typeof(Text))]
	public class TextTag : MonoBehaviour 
	{
		[SerializeField] private Text mText;
		[SerializeField] [TextTag] private string mTag;

		private static List<TextTag> mActiveTexts = new List<TextTag>();

		void Reset()
		{
			if (mText == null)
				mText = GetComponent<Text>();
		}

		void OnValidate()
		{
			if (mText == null)
				mText = GetComponent<Text>();

			Refresh();
		}
		
		void OnEnable()
		{
			mActiveTexts.Add(this);
			Refresh();
		}

		void OnDisable()
		{
			mActiveTexts.Remove(this);
		}

		public void SetTag(string newTag)
		{
			mTag = newTag;
			Refresh();
		}

		private void Refresh()
		{
			#if UNITY_EDITOR
			if (UnityEditor.PrefabUtility.GetPrefabType(this) == UnityEditor.PrefabType.Prefab)
				return;
			#endif

			mText.text = TextManager.Get(mTag);
		}

		public static void RefreshAll()
		{
			for (int i = 0; i < mActiveTexts.Count; i++)
			{
				mActiveTexts[i].Refresh();
			}
		}
	}
}
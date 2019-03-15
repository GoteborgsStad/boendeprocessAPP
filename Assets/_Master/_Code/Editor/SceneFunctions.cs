using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace ius
{
	/// Some functions to detect errors with UI texts
	public static class SceneFunctions
	{
		[MenuItem("Utility/Find Tagless Text")]
		private static void FindTagless()
		{
			Text[] texts = Resources.FindObjectsOfTypeAll<Text>();

			Debug.Log("Total text Count: " + texts.Length);

			Debug.Log("=== Listing all Texts without TextTags ===");
			int foundCount = 0;

			for (int i = 0; i < texts.Length; i++)
			{
				if (texts[i].hideFlags != HideFlags.None)
					continue;

				if (texts[i].GetComponent<TextTag>() == null)
				{
					foundCount++;
					Debug.Log(texts[i].FullName());
				}
			}

			Debug.Log("=======> Tagless count: " + foundCount);
		}

		[MenuItem("Utility/Find Arial Text")]
		private static void FindArial()
		{
			Text[] texts = Resources.FindObjectsOfTypeAll<Text>();

			Debug.Log("Total text Count: " + texts.Length);

			Debug.Log("=== Listing all Texts with Arial font ===");
			int foundCount = 0;

			for (int i = 0; i < texts.Length; i++)
			{
				if (texts[i].hideFlags != HideFlags.None)
					continue;

				if (texts[i].font.name == "Arial")
				{
					foundCount++;
					Debug.Log(texts[i].FullName());
				}
			}

			Debug.Log("=======> Arial count: " + foundCount);
		}
	}
}
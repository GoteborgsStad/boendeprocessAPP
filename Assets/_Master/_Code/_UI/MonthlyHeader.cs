using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	public class MonthlyHeader : MonoBehaviour 
	{
		[SerializeField] private Text mText;

		public void SetTitle(string text)
		{
			mText.text = text;
		}
	}
}
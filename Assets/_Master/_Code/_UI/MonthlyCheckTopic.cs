using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	public class MonthlyCheckTopic : MonoBehaviour 
	{
		[SerializeField] private Image mIndicatorSymbol;
		[SerializeField] private Text mText;

		public void SetData(Sprite symbol, string text)
		{
			mIndicatorSymbol.sprite = symbol;
			mText.text = text;
		}
	}
}
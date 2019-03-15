using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace ius
{
	public class PageNavigationButton : MonoBehaviour 
	{
		[SerializeField] private Text mText;
		[SerializeField] private Color mSelectedColor;
		[SerializeField] private Color mUnselectedColor;

		private Action<int> mOnClick;
		private int mClickIndex;

		public RectTransform MyTransform { get; private set; }

		public void Initialize(Action<int> onClick, int clickIndex)
		{
			mOnClick = onClick;
			mClickIndex = clickIndex;

			MyTransform = GetComponent<RectTransform>();
		}

		/// <summary> Switched to the wanted selection state. If speed is below 0, the switch is instant. </summary>
		public Coroutine SetState(bool isSelected, float speed)
		{
			if (speed < 0)
			{
				mText.color = isSelected ? mSelectedColor : mUnselectedColor;
				return null;
			}
			else
			{
				return StartCoroutine(ColorRoutine(
					isSelected ? mUnselectedColor : mSelectedColor,
					isSelected ? mSelectedColor : mUnselectedColor,
					speed));
			}
		}

		private IEnumerator ColorRoutine(Color from, Color to, float speed)
		{
			float timer = 0;

			while (timer < 1f)
			{
				timer += Time.deltaTime * speed;
				mText.color = Color.Lerp(from, to, timer);
				yield return null;
			}

			mText.color = to;
		}

		public void ButtonClick()
		{
			if (mOnClick != null)
				mOnClick(mClickIndex);
		}
	}
}
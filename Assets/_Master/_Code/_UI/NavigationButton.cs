using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	public class NavigationButton : MonoBehaviour 
	{
		[SerializeField] private NavigationLocations mLocation;
		[SerializeField] private Image mIcon;
		[SerializeField] private Text mText;
		[SerializeField] private Color mUnselectedColor;
		[SerializeField] private Sprite mUnselectedSprite;
		[SerializeField] private Color mSelectedColor;
		[SerializeField] private Sprite mSelectedSprite;

		private RectTransform mTransform;
		private UINavigation mNavigation;

		public NavigationLocations Location { get { return mLocation; } }
		public RectTransform MyTransform { get { return mTransform; } }

		public void Initialize(UINavigation navigation)
		{
			mTransform = GetComponent<RectTransform>();
			mNavigation = navigation;
			SetState(false, -1);
		}

		public void ButtonClick()
		{
			mNavigation.ButtonNavigate(mLocation);
		}

		/// <summary> Switched to the wanted selection state. If speed is below 0, the switch is instant. </summary>
		public Coroutine SetState(bool isSelected, float speed)
		{
			if (speed < 0)
			{
				// Instant swap
				mIcon.sprite = isSelected ? mSelectedSprite : mUnselectedSprite;
				mIcon.color = isSelected ? mSelectedColor : mUnselectedColor;
				mText.color = mIcon.color;
				return null;
			}
			else
			{
				// Swap over time
				if (isSelected)
					return StartCoroutine(SelectRoutine(speed));
				else
					return StartCoroutine(DeselectRoutine(speed));
			}
		}

		private IEnumerator SelectRoutine(float speed)
		{
			float timer = 0;

			while (timer < 1f)
			{
				timer += Time.deltaTime * speed;
				mIcon.color = Color.Lerp(mUnselectedColor, mSelectedColor, timer);
				mText.color = mIcon.color;
				yield return null;
			}

			mIcon.color = mSelectedColor;
			mText.color = mIcon.color;

			mIcon.sprite = mSelectedSprite;
		}

		private IEnumerator DeselectRoutine(float speed)
		{
			mIcon.sprite = mUnselectedSprite;

			float timer = 0;

			while (timer < 1f)
			{
				timer += Time.deltaTime * speed;
				mIcon.color = Color.Lerp(mSelectedColor, mUnselectedColor, timer);
				mText.color = mIcon.color;
				yield return null;
			}

			mIcon.color = mUnselectedColor;
			mText.color = mIcon.color;
		}
	}
}
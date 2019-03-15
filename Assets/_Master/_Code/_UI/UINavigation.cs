using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public enum NavigationLocations
	{
		Plant,
		Assignment,
		Chat,
		Check,
		Info,
		Options
	}

	public class UINavigation : MonoBehaviour
	{
		[SerializeField] private float mTransitionTime = 0.2f;
		[SerializeField] private NavigationButton[] mButtons;
		[SerializeField] private RectTransform mSelectionFrame;

		private bool mIsVisible;
		private Dictionary<NavigationLocations, NavigationButton> mButtonDictionary;

		private bool mIsTransitioning;
		private NavigationLocations mCurrentLocation;

		private static UINavigation Instance;

		void Awake()
		{
			Instance = this;
			mIsVisible = true;
		}
		
		void Start()
		{
			mButtonDictionary = new Dictionary<NavigationLocations, NavigationButton>();

			for (int i = 0; i < mButtons.Length; i++)
			{
				NavigationButton button = mButtons[i];
				button.Initialize(this);

				if (mButtonDictionary.ContainsKey(button.Location))
					Debug.LogWarning("Duplicate navigation buttons for " + button.Location);
				else
					mButtonDictionary[button.Location] = button;
			}

			ResetNavigationState();
		}

		void OnDisable()
		{
			mIsTransitioning = false;
		}

		public static void SetState(bool isVisible)
		{
			if (isVisible == Instance.mIsVisible)
				return;

			Instance.mIsVisible = isVisible;
			Instance.gameObject.SetActive(Instance.mIsVisible);
			Instance.ResetNavigationState();
		}

		private void ResetNavigationState()
		{
			mCurrentLocation = NavigationLocations.Plant;
			mSelectionFrame.sizeDelta = mButtons[0].MyTransform.sizeDelta;
			mSelectionFrame.anchoredPosition = mButtonDictionary[mCurrentLocation].MyTransform.anchoredPosition;
			
			for (int i = 0; i < mButtons.Length; i++)
			{
				mButtons[i].SetState(false, -1);
			}

			mButtonDictionary[mCurrentLocation].SetState(true, -1);
		}

		void Update()
		{
			if (!ApproximateEqual(mSelectionFrame.sizeDelta, mButtons[0].MyTransform.sizeDelta, 1f))
			{
				mSelectionFrame.sizeDelta = mButtons[0].MyTransform.sizeDelta;
				mSelectionFrame.anchoredPosition = mButtonDictionary[mCurrentLocation].MyTransform.anchoredPosition;
			}
		}

		private static bool ApproximateEqual(Vector2 a, Vector2 b, float maxDistance)
		{
			return Mathf.Abs(a.x - b.x) < maxDistance && Mathf.Abs(a.y - b.y) < maxDistance;
		}
		
		public void ButtonNavigate(NavigationLocations location)
		{
			if (mIsTransitioning || location == mCurrentLocation)
				return;
			
			switch (location)
			{
				case NavigationLocations.Plant:		UIManager.Open(UILocation.Progress); break;
				case NavigationLocations.Assignment:	UIManager.Open(UILocation.Assignment); break;
				case NavigationLocations.Chat:		UIManager.Open(UILocation.Chat); break;
				case NavigationLocations.Check:		UIManager.Open(UILocation.MonthlyCheck); break;
				case NavigationLocations.Info:		UIManager.Open(UILocation.Info); break;
				case NavigationLocations.Options:	UIManager.Open(UILocation.Settings); break;

				default: Debug.LogWarning("Missing handling for NavigationLocation: " + location); break;
			}

			StartCoroutine(TransitionRoutine(location));
		}

		private IEnumerator TransitionRoutine(NavigationLocations location)
		{
			mIsTransitioning = true;
			
			// Change current NavigationButton's image and text from Selected color to Unselected color
			mButtonDictionary[mCurrentLocation].SetState(false, 10f);

			// Move background and "frame" from current location to target location
			mCurrentLocation = location;
			Vector3 from = mSelectionFrame.anchoredPosition;
			Vector3 to = mButtonDictionary[mCurrentLocation].MyTransform.anchoredPosition;

			float timer = 0;
			float speed = 1f / mTransitionTime;

			while (timer < 1f)
			{
				timer += Time.deltaTime * speed;
				mSelectionFrame.anchoredPosition = Vector3.Lerp(from, to, Mathfx.Hermite(0, 1f, timer));
				yield return null;
			}

			mSelectionFrame.anchoredPosition = to;

			// Change new NavigationButton's image and text from Unselected to Selected
			yield return mButtonDictionary[mCurrentLocation].SetState(true, 10f);

			mIsTransitioning = false;
		}
	}
}
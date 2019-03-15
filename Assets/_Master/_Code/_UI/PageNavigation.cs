using UnityEngine;
using System.Collections;
using System;

namespace ius
{
	public class PageNavigation : MonoBehaviour
	{
		[SerializeField] private RectTransform mSelectionFrame;
		[SerializeField] private PageNavigationButton[] mButtons;
		[SerializeField] private UISlideElement[] mPages;

		private bool mIsInitialized;
		private GameObject mGameObject;
		private bool mIsSwitching;
		private int mCurrentPage;

		private bool mHasInput;
		private Vector2 mStartInput;

		private int mStartPage;

		public int CurrentPage { get { return mCurrentPage; } }
		public event Action OnSwitchStart;
		public event Action OnSwitchDone;

		private const float TRANSITION_TIME = 0.3f;

		public void SetPage(int page)
		{
			if (!mIsInitialized)
				Initialize();

			if (!mGameObject.activeSelf)
				mStartPage = page;

			mCurrentPage = page;
			HardSetPage();
		}

		void OnEnable()
		{
			if (!mIsInitialized)
				return;

			mIsSwitching = false;
			mCurrentPage = mStartPage;
			mStartPage = 0;
			HardSetPage();
		}

		private void Initialize()
		{
			if (mIsInitialized)
				return;

			mIsInitialized = true;
			mGameObject = gameObject;

			for (int i = 0; i < mButtons.Length; i++)
			{
				mButtons[i].Initialize(OnButtonClick, i);
			}

			mCurrentPage = mStartPage;
			mStartPage = 0;
			HardSetPage();
		}

		private void HardSetPage()
		{
			for (int i = 0; i < mButtons.Length; i++)
			{
				mButtons[i].SetState(i == mCurrentPage, -1f);
			}

			for (int i = 0; i < mPages.Length; i++)
			{
				if (i != mCurrentPage)
					mPages[i].SetPositionOut();
				else
					mPages[i].SetPositionIn();
			}
			
			mSelectionFrame.anchoredPosition = mButtons[mCurrentPage].MyTransform.anchoredPosition;
			mSelectionFrame.sizeDelta = mButtons[mCurrentPage].MyTransform.sizeDelta;

			DisableOffPages();
		}

		void Start()
		{
			Initialize();
			HardSetPage();
		}

		void Update()
		{
			// Detect swipes and navigate pages

			bool hasInput = HasInput();

			if (!mHasInput && hasInput)
			{
				// Begin input
				mStartInput = GetInput();
			}
			else if (mHasInput) // && !hasInput)
			{
				// Finish input
				if (Swipe(GetInput() - mStartInput))
					mStartInput = GetInput();
			}

			mHasInput = hasInput;
		}

		private bool Swipe(Vector2 swipe)
		{
			if (Mathf.Abs(swipe.x) * 0.5f > Mathf.Abs(swipe.y) && Mathf.Abs(swipe.x) > 0.35f)
			{
				int toPage = mCurrentPage + (int)Mathf.Sign(-swipe.x);

				if (toPage >= 0 && toPage < mPages.Length)
					NavigateTo(toPage);

				return true;
			}

			return false;
		}

		private bool HasInput()
		{
			bool hasInput = Input.GetMouseButton(0);

			if (hasInput)
			{
				float yPos = Input.mousePosition.y;

				if (yPos < Screen.height * 0.1f)
				{
					// The touch input is in the bot 10% of the screen which is the navigation bar
					hasInput = false;
				}
			}

			return hasInput;
		}

		/// <summary> Returns a position in -1 to 1 range. </summary>
		private Vector2 GetInput()
		{
			return PixelsToView(Input.mousePosition);
		}

		private Vector2 PixelsToView(Vector2 screenPosition)
		{
			screenPosition.x /= Screen.width;
			screenPosition.y /= Screen.height;

			screenPosition *= 2f;
			screenPosition -= Vector2.one;

			return screenPosition;
		}

		private void OnButtonClick(int index)
		{
			NavigateTo(index);
		}

		private void NavigateTo(int index)
		{
			if (mIsSwitching || mCurrentPage == index || UIManager.IsTransition)
				return;

			StartCoroutine(SwitchRoutine(index));
		}

		private IEnumerator SwitchRoutine(int index)
		{
			mIsSwitching = true;

			UIManager.SetInputBlock(true);

			if (OnSwitchStart != null)
				OnSwitchStart();

			// Change current button's selection state
			mButtons[mCurrentPage].SetState(false, 10f);
			
			// Slide out the current page...
			mPages[mCurrentPage].gameObject.SetActive(true);
			RectTransform.Edge slideDirection = index > mCurrentPage ? RectTransform.Edge.Left : RectTransform.Edge.Right;
			mPages[mCurrentPage].SetExitEdge(slideDirection);
			mPages[mCurrentPage].BeginMoveOut();

			// ...and slide in the new one
			slideDirection = index > mCurrentPage ? RectTransform.Edge.Right : RectTransform.Edge.Left;
			mCurrentPage = index;
			mPages[mCurrentPage].gameObject.SetActive(true);
			mPages[mCurrentPage].SetEnterEdge(slideDirection);
			mPages[mCurrentPage].BeginMoveIn();

			// Pick selection frame position/size
			Vector3 fromPosition = mSelectionFrame.anchoredPosition;
			Vector3 toPosition = mButtons[mCurrentPage].MyTransform.anchoredPosition;

			Vector2 fromSize = mSelectionFrame.sizeDelta;
			Vector3 toSize = mButtons[mCurrentPage].MyTransform.sizeDelta;

			// Move selection frame from current location to target location
			float timer = 0;
			float speed = 1f / TRANSITION_TIME;
			float lerpValue;

			while (timer < 1f)
			{
				timer += Time.deltaTime * speed;
				lerpValue = Mathfx.Hermite(0, 1f, timer);
				mSelectionFrame.anchoredPosition = Vector3.Lerp(fromPosition, toPosition, lerpValue);
				mSelectionFrame.sizeDelta = Vector2.Lerp(fromSize, toSize, lerpValue);
				yield return null;
			}

			mSelectionFrame.anchoredPosition = toPosition;
			mSelectionFrame.sizeDelta = toSize;

			// Change the new target location to it's selected appearance, wait until done
			yield return mButtons[mCurrentPage].SetState(true, 10f);

			// Disable currently out pages to better support screen transitions
			DisableOffPages();

			if (OnSwitchDone != null)
				OnSwitchDone();
			
			UIManager.SetInputBlock(false);
			mIsSwitching = false;
		}

		private void DisableOffPages()
		{
			mPages[mCurrentPage].gameObject.SetActive(true);

			for (int i = 0; i < mPages.Length; i++)
			{
				if (i == mCurrentPage)
					continue;

				mPages[i].gameObject.SetActive(false);
			}
		}
	}
}
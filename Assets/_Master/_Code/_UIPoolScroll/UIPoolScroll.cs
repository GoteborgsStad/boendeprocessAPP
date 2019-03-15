using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace ius
{
	public class UIPoolScroll : MonoBehaviour 
	{
		[SerializeField] private int mPoolSize = 5;
		[SerializeField] private UIPoolScrollElement mInScenePrefab;
		[SerializeField] private float mSpacing = 5f;
		[SerializeField] private float mInertia = 0.135f;
		[SerializeField] private float mElasticity = 5f;
		[SerializeField] private float mClickSensitivity = 20f;

		private Action<int> mOnClick;

		private RectTransform mMainRect;
		private Transform mContentRoot;
		private float mCanvasWidth;
		private float mCanvasHeight;
		private float mScrollHeight;
		private float mInputMultiplierX;
		private float mInputMultiplierY;

		private UIPoolScrollElement[] mElementPool;
		private DataBaseObject[] mData;
		private int mTotalElementCount;
		private bool mCanScroll;

		private bool mHasInput;
		private Vector2 mInputPoint;
		private float mVelocity;
		private float mLastVelocity;
		private int mCurrentTopElement;

		private bool mCanClick;
		private float mTotalMove;

		public void Initialize(DataBaseObject[] data, Action<int> onClick)
		{
			if (data == null || data.Length == 0)
			{		
				mInScenePrefab.gameObject.SetActive(false);
				mData = null;
				mCanScroll = false;
				ClearPool();
				return;
			}

			mData = data;
			mTotalElementCount = mData.Length;
			mOnClick = onClick;
			mCanScroll = true;
			mCanClick = true;
			InitializePool(Mathf.Min(mTotalElementCount, mPoolSize));
		}

		private void ClearPool()
		{
			if (mElementPool != null)
			{
				// Clear the old pool
				for (int i = 0; i < mElementPool.Length; i++)
				{
					if (mElementPool[i] != null)
						Destroy(mElementPool[i].gameObject);
				}
			}
		}

		private void InitializePool(int size)
		{
			if (mMainRect == null)
			{
				// First time initialization
				mMainRect = GetComponent<RectTransform>();

				CanvasScaler scaler = GetComponentInParent<CanvasScaler>();
				mCanvasWidth = scaler.referenceResolution.x;
				mCanvasHeight = scaler.referenceResolution.y;

				mInputMultiplierX = mCanvasWidth / Screen.width;
				mInputMultiplierY = mCanvasHeight / Screen.height;

				mScrollHeight = GetComponent<RectTransform>().rect.height;
				mContentRoot = mInScenePrefab.transform.parent;
			}

			ClearPool();

			mInScenePrefab.gameObject.SetActive(true);

			mElementPool = new UIPoolScrollElement[size];

			float yPos = mSpacing;

			for (int i = 0; i < mElementPool.Length; i++)
			{
				mElementPool[i] = Instantiate(mInScenePrefab, mContentRoot);
				mElementPool[i].Initialize(yPos, -mCanvasHeight, 0, OnClick);

				mElementPool[i].SetIndex(i);
				mElementPool[i].SetData(mData[i]);
				
				yPos += mElementPool[i].Height + mSpacing;
			}

			mCanScroll = yPos > mScrollHeight;

			mCurrentTopElement = 0;
			mVelocity = 0;

			mInScenePrefab.gameObject.SetActive(false);
		}

		void OnDisable()
		{
			mVelocity = 0;
		}
		
		void Update()
		{
			if (!mCanScroll)
				return;
						
			UpdateInput();

			if (!mHasInput)
				UpdateInertia();

			UpdateLooping();
			UpdateElastic();

			Move(mVelocity * Time.deltaTime);
		}

		private void UpdateInput()
		{
			bool hasInput = HasInput();

			if (!mHasInput && hasInput)
			{
				// Should only give input if input position is within correct area
				hasInput = RectTransformUtility.RectangleContainsScreenPoint(mMainRect, GetScreenInput());

				if (hasInput)
				{
					// Input started
					mInputPoint = GetInput();
					mTotalMove = 0;
					mCanClick = true;
				}
			}
			else if (mHasInput && hasInput)
			{
				// Is inputting
				Vector2 inputPoint = GetInput();
				Vector2 move = inputPoint - mInputPoint;
				UpdateVelocity(move.y);
				mInputPoint = inputPoint;

				mTotalMove += Mathf.Abs(move.x) + Mathf.Abs(move.y);

				if (mTotalMove >= mClickSensitivity)
					mCanClick = false;
			}

			mHasInput = hasInput;
		}

		private bool HasInput()
		{
			return Input.GetMouseButton(0);
		}

		private Vector2 GetInput()
		{
			Vector2 result = GetScreenInput();
			result.x *= mInputMultiplierX;
			result.y *= mInputMultiplierY;
			return result;
		}

		private Vector2 GetScreenInput()
		{
			return Input.mousePosition;
		}

		private void Move(float move)
		{
			Vector2 moveVector = new Vector2(0, move);

			for (int i = 0; i < mElementPool.Length; i++)
			{
				mElementPool[i].Move(moveVector);
			}
		}

		private void UpdateVelocity(float move)
		{
			float newVelocity = move / Time.deltaTime;
			mVelocity = newVelocity;
			mLastVelocity = Mathf.Lerp(mLastVelocity, newVelocity, Time.deltaTime * 10f);
		}

		private void UpdateInertia()
		{
			if (Mathf.Approximately(mVelocity, 0))
				return;

			mVelocity *= Mathf.Pow(mInertia, Time.deltaTime);

			if (Mathf.Abs(mVelocity) < 1f)
				mVelocity = 0f;

			if (!mCanClick && Mathf.Abs(mVelocity) < 50f)
				mCanClick = true;
		}

		private void UpdateLooping()
		{
			int currentBot = mCurrentTopElement + mElementPool.Length - 1;
			currentBot %= mElementPool.Length;

			UIPoolScrollElement topElement = mElementPool[mCurrentTopElement];
			UIPoolScrollElement botElement = mElementPool[currentBot];

			if (topElement.IsOnScreen() > 0)
			{
				// Current top element is above the screen top, it can be moved to the bot
				float contentBotPosition = botElement.Bot - mSpacing;
				int newContentIndex = botElement.Index + 1;

				SetElement(topElement, newContentIndex, contentBotPosition, false, 1);
			}
			else if (topElement.Top < -mSpacing)
			{
				// Current top element's top has scrolled below the screen top, a new element need to be moved to the top
				float contentTopPosition = topElement.Top + mSpacing;
				int newContentIndex = topElement.Index - 1;

				SetElement(botElement, newContentIndex, contentTopPosition, true, -1);
			}
		}

		private void SetElement(UIPoolScrollElement element, int contentIndex, float position, bool addHeight, int topChange)
		{
			if (contentIndex < 0 || contentIndex >= mTotalElementCount)
				return;
			
			element.SetIndex(contentIndex);
			element.SetData(mData[contentIndex]);

			if (addHeight)
				position += element.Height;

			element.SetPosition(position);

			mCurrentTopElement += topChange;
			
			if (mCurrentTopElement < 0)
				mCurrentTopElement += mElementPool.Length;
			if (mCurrentTopElement >= mElementPool.Length)
				mCurrentTopElement -= mElementPool.Length;
		}

		private void UpdateElastic()
		{
			int currentBot = mCurrentTopElement + mElementPool.Length - 1;
			currentBot %= mElementPool.Length;

			UIPoolScrollElement topElement = mElementPool[mCurrentTopElement];
			UIPoolScrollElement botElement = mElementPool[currentBot];

			float offset = 0;
			
			// Elastic clamp at top
			if (topElement.Index <= 0)
			{
				if (topElement.Top < -mSpacing)
				{
					offset = -mSpacing - topElement.Top;
				}
			}

			// Elastic clamp at bot
			if (Mathf.Approximately(offset, 0) && botElement.Index >= mTotalElementCount - 1)
			{
				if (botElement.Bot > -mScrollHeight - mSpacing)
				{
					offset = -(botElement.Bot + mScrollHeight - mSpacing);
				}
			}
			
			if (Mathf.Approximately(offset, 0))
				return; // No clamp required

			float elasticBase = 300f;
			float sign = Mathf.Sign(offset);
			float offRatio = Mathf.Clamp01(Mathf.Abs(offset) / elasticBase);
			float correction = 1f - (offRatio * offRatio);

			if (mHasInput)
			{
				// Is currently dragging
				if (offset > 0 != mVelocity > 0)
				{
					// Velocity is pulling elements off screen - apply velocity dampening
					mVelocity *= correction;
				}
			}
			else
			{
				// Not dragging, set velocity to return to screen
				mVelocity = sign * correction * elasticBase * mElasticity;

				float move = mVelocity * Time.deltaTime;

				if (Mathf.Abs(offset) < Mathf.Abs(move))
				{
					// The next move with velocity will go past the wanted position, set the position exactly and stop velocity
					mVelocity = 0;
					Move(offset);
				}
			}
		}

		private void OnClick(int index)
		{
			if (mCanClick && mOnClick != null)
				mOnClick(index);
		}
	}
}
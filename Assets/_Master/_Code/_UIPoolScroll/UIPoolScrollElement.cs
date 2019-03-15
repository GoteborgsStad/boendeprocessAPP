using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ius
{
	public abstract class UIPoolScrollElement : MonoBehaviour
	{
		private RectTransform mTransform;
		private Action<int> mOnClick;
		private float mScreenBot;
		private float mScreenTop;

		public int Index { get; private set; }

		public float Height { get { return mTransform.rect.height; } }
		public float Position { get { return mTransform.anchoredPosition.y; } }
		public float Bot { get { return Position - mTransform.rect.height; } }
		public float Top { get { return Position; } }

		public void Initialize(float yPosition, float bot, float top, Action<int> onClick)
		{
			mTransform = GetComponent<RectTransform>();
			mTransform.pivot = Vector2.up;

			mOnClick = onClick;

			Vector2 position = mTransform.anchoredPosition;
			position.y = -yPosition;
			mTransform.anchoredPosition = position;

			mScreenBot = bot;
			mScreenTop = top;
		}

		public void SetIndex(int index)
		{
			Index = index;
		}

		public void SetPosition(float yPosition)
		{
			Vector2 position = mTransform.anchoredPosition;
			position.y = yPosition;
			mTransform.anchoredPosition = position;
		}

		/// <summary> 0 = on screen, 1 = above screen, -1 = below screen </summary>
		public int IsOnScreen()
		{
			if (Top < mScreenBot)
				return -1;
			if (Bot > mScreenTop)
				return 1;
			return 0;

			//return top > mScreenBot && bot < mScreenTop;
		}

		public float GetOffset()
		{
			int onScreen = IsOnScreen();

			if (onScreen == 0)
				return 0f;
			if (onScreen > 0)
				return Bot - mScreenTop;
			return mScreenBot - Top;
		}

		public void Move(Vector2 move)
		{
			mTransform.anchoredPosition += move;
		}

		public abstract void SetData(DataBaseObject data);

		protected void SendClick(int index)
		{
			if (mOnClick != null)
				mOnClick(index);
		}
	}
}
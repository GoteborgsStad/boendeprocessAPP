using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	[RequireComponent(typeof(RectTransform))]
	public abstract class UILayoutBase : MonoBehaviour
	{
		[SerializeField] private bool mPerformLayout;

		[HideInInspector] [SerializeField] private RectTransform mRectTransform;
		[HideInInspector] [SerializeField] private UILayoutBase mParent;

		public RectTransform MyTransform { get { return mRectTransform; } }
		public UILayoutBase Parent { get { return mParent; } }

		void Reset()
		{
			UpdateComponents();
		}

		void OnValidate()
		{
			UpdateComponents();

			if (mPerformLayout)
			{
				mPerformLayout = false;
				PerformLayout();
			}
		}

		public void PerformLayout()
		{
			if (mParent != null)
				mParent.PerformLayout();
			else
				ExecuteLayout();
		}

		public void SetLayoutParent(UILayoutBase parent)
		{
			mParent = parent;

			if (mParent != null)
			{
				mRectTransform.pivot = Vector2.up;

				if (mRectTransform.anchorMin.y < 0.99f)
				{
					mRectTransform.anchorMin = new Vector2(0, 1);
					mRectTransform.anchorMax = new Vector2(1, 1);
				}
			}
		}

		public abstract void ExecuteLayout();

		protected virtual void UpdateComponents()
		{
			if (mRectTransform == null)
				mRectTransform = GetComponent<RectTransform>();
		}

		public virtual void UpdateLayoutHierarchy()
		{

		}
	}
}
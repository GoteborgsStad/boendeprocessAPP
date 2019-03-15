using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	[RequireComponent(typeof(RectTransform))]
	public class UILayoutFitChildren : UILayoutBase
	{ 
		// Only support for Vertical is implemented
		private RectTransform.Axis mAxis = RectTransform.Axis.Vertical;

		[SerializeField] private float mPaddingLow;
		[SerializeField] private float mPaddingHigh;

		[SerializeField] private bool mLayoutChildren;
		[SerializeField] private float mSpacing;
		
		[SerializeField] private bool mUpdateChildren;
		[SerializeField] private UILayoutBase[] mChildren;

		protected override void UpdateComponents()
		{
			base.UpdateComponents();
			
			if (mUpdateChildren || mChildren == null)
			{
				mUpdateChildren = false;
				UpdateLayoutHierarchy();
			}
		}
		
		public override void UpdateLayoutHierarchy()
		{
			if (mChildren != null)
			{
				for (int i = 0; i < mChildren.Length; i++)
				{
					if (mChildren[i] != null)
						mChildren[i].SetLayoutParent(null);
				}
			}

			List<UILayoutBase> children = new List<UILayoutBase>();

			for (int i = 0; i < MyTransform.childCount; i++)
			{
				UILayoutBase child = MyTransform.GetChild(i).GetComponent<UILayoutBase>();

				if (child == null)
					continue;

				children.Add(child);
				child.SetLayoutParent(this);
				
				child.UpdateLayoutHierarchy();
			}

			mChildren = children.ToArray();
		}
		
		public override void ExecuteLayout()
		{
			PerformChildrenLayout();

			if (mLayoutChildren)
			{
				LayoutChildren();
			}

			ResizeToFit();
		}

		private void PerformChildrenLayout()
		{
			for (int i = 0; i < mChildren.Length; i++)
			{
				if (mChildren[i] != null)
					mChildren[i].ExecuteLayout();
			}

			for (int i = 0; i < mChildren.Length; i++)
			{
				if (mChildren[i] == null)
					continue;

				RectTransform childRect = mChildren[i].MyTransform;

				Vector2 anchoredPosition = childRect.anchoredPosition;

				//Debug.Log(mChildren[i].name + " AnchoredPosition: " + anchoredPosition.y + ", paddingLow: " + (-mPaddingLow));

				if (anchoredPosition.y > -mPaddingLow)
				{
					anchoredPosition.y = -mPaddingLow;
					childRect.anchoredPosition = anchoredPosition;
				}
			}
		}

		private void LayoutChildren()
		{
			float nextPosition = (mAxis == RectTransform.Axis.Vertical) ? -mPaddingLow : mPaddingLow;

			for (int i = 0; i < mChildren.Length; i++)
			{
				if (mChildren[i] == null)
					continue;

				RectTransform childRect = mChildren[i].MyTransform;

				Vector2 anchoredPosition = childRect.anchoredPosition;

				if (mAxis == RectTransform.Axis.Vertical)
					anchoredPosition.y = nextPosition;
				else
					anchoredPosition.x = nextPosition;

				childRect.anchoredPosition = anchoredPosition;

				float size = (mAxis == RectTransform.Axis.Vertical) ? childRect.rect.height : childRect.rect.width;
				size += mSpacing;
				
				//nextPosition -= mSpacing + size;

				nextPosition += (mAxis == RectTransform.Axis.Vertical) ? -size : size;
			}
		}

		private void ResizeToFit()
		{
			if (mChildren == null || mChildren.Length == 0)
				return;

			// <TODO> This code should take Axis into account

			float highest = 0;
			float lowest = float.MaxValue;

			for (int i = 0; i < mChildren.Length; i++)
			{
				if (mChildren[i] == null)
					continue;

				Rect rect = mChildren[i].MyTransform.rect;

				float pos = mAxis == RectTransform.Axis.Vertical
					? mChildren[i].MyTransform.anchoredPosition.y
					: mChildren[i].MyTransform.anchoredPosition.x;

				float min = pos + (mAxis == RectTransform.Axis.Vertical ? rect.yMin : rect.xMin);
				float max = pos + (mAxis == RectTransform.Axis.Vertical ? rect.yMax : rect.xMax);

				if (min < lowest)
					lowest = min;
				if (max > highest)
					highest = max;
			}

			float size = highest - lowest;
			size += mPaddingHigh;

			//Debug.Log("Highest: " + highest + ", lowest: " + lowest + ", size: " + size);

			MyTransform.SetSizeWithCurrentAnchors(mAxis, size);
		}
	}
}
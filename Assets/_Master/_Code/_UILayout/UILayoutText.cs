using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	[RequireComponent(typeof(Text))]
	public class UILayoutText : UILayoutBase 
	{
		[SerializeField] private float mMinHeight;
		[HideInInspector] [SerializeField] private Text mText;

		protected override void UpdateComponents()
		{
			base.UpdateComponents();

			if (mText == null)
				mText = GetComponent<Text>();
		}
		
		public override void ExecuteLayout()
		{
			// Fit rect to text
			MyTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(mText.preferredHeight, mMinHeight));
		}
	}
}
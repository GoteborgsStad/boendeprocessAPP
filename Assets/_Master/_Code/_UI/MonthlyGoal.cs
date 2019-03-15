using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	public class MonthlyGoal : MonoBehaviour
	{
		[SerializeField] private UIAttentionFlash mFlash;
		[SerializeField] private Text mTitle;
		[SerializeField] private Text mBody;

		public void SetData(DataGoal data)
		{
			mTitle.text = data.Name;

			string bodyText = data.Description;

			if (data.EndAt.HasValue)
				bodyText = "<b>Måldatum: " + data.EndAt.Value.ToString("yyyy-MM-dd") + "</b>\n" + bodyText;

			mBody.text = bodyText;
		}

		public void Flash()
		{
			mFlash.Flash(ColorPalette.Flash, 0.4f, 0.8f);
		}
	}
}
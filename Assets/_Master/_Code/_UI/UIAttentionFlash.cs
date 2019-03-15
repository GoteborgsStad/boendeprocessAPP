using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	[RequireComponent(typeof(Image))]
	public class UIAttentionFlash : MonoBehaviour 
	{
		[SerializeField] private Image mImage;

		private Color mBaseColor;
		private Color mFlashColor;
		private float mFlashTimer;
		private float mFlashSpeed;

		void Reset()
		{
			if (mImage == null)
				mImage = GetComponent<Image>();
		}

		void Awake()
		{
			mBaseColor = mImage.color;
			enabled = false;
		}

		public void Flash(Color color, float flashTime, float delay)
		{
			mFlashColor = color;
			enabled = true;
			mFlashTimer = -delay;
			mFlashSpeed = 1f / Mathf.Max(flashTime, 0.01f);
		}

		void Update()
		{
			mFlashTimer += Time.deltaTime * mFlashSpeed;
			mImage.color = Color.Lerp(mBaseColor, mFlashColor, Mathf.Sin(Mathf.Clamp01(mFlashTimer) * Mathf.PI));

			if (mFlashTimer >= 1f)
			{
				enabled = false;
				mImage.color = mBaseColor;
			}
		}
	}
}
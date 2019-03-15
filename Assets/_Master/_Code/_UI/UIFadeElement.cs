using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	[RequireComponent(typeof(Graphic))]
	public class UIFadeElement : MonoBehaviour 
	{
		[SerializeField] private float mFadeTime = 0.2f;

		private Graphic mGraphic;
		private Color mTransparent, mOpaque;
		private float mTimer;
		private bool mIsFading;

		void Awake()
		{
			if (mGraphic == null)
				Initialize();
		}

		private void Initialize()
		{
			mGraphic = GetComponent<Graphic>();
			mOpaque = mGraphic.color;
			mTransparent = mOpaque;
			mTransparent.a = 0f;

			enabled = false;
			mGraphic.enabled = false;
			mGraphic.color = mTransparent;
		}

		public void SetState(bool fade)
		{
			mIsFading = fade;
			mGraphic.enabled = true;
			enabled = true;
		}

		public void Hide()
		{
			if (mGraphic == null)
				Initialize();

			mTimer = 0;
			mIsFading = false;
			enabled = false;
			mGraphic.enabled = false;
			mGraphic.color = mTransparent;
		}

		void Update()
		{
			if (!mIsFading)
			{
				mTimer -= Time.deltaTime;

				if (mTimer <= 0)
				{
					mTimer = 0;
					mGraphic.enabled = false;
					enabled = false;
				}
			}
			else
			{
				mTimer += Time.deltaTime;

				if (mTimer >= mFadeTime)
				{
					mTimer = mFadeTime;
					enabled = false;
				}
			}

			mGraphic.color = Color.Lerp(mTransparent, mOpaque, mTimer / mFadeTime);
		}
	}
}
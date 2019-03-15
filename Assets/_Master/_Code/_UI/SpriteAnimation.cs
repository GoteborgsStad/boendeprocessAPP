using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	[RequireComponent(typeof(Image))]
	public class SpriteAnimation : MonoBehaviour 
	{
		[SerializeField] private Sprite[] mFrames;
		[SerializeField] private float mLoopTime = 1f;

		private Image mImage;
		private int mCurrentFrame;

		private float mFrameTime;
		private float mTimer;

		void Awake()
		{
			if (mFrames.Length == 0)
			{
				enabled = false;
				return;
			}

			mImage = GetComponent<Image>();
			mImage.sprite = mFrames[0];
			mFrameTime = mLoopTime / mFrames.Length;
		}

		void Update()
		{
			mTimer += Time.deltaTime;

			if (mTimer >= mFrameTime)
			{
				mTimer -= mFrameTime;

				mCurrentFrame++;
				mCurrentFrame %= mFrames.Length;
				mImage.sprite = mFrames[mCurrentFrame];
			}
		}
	}
}
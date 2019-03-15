using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ius
{
	public class DebugTextEnabler : MonoBehaviour 
	{
		[SerializeField] private UnityEngine.UI.Text mText;

		private float mHoldTimer;

		private const float HOLD_TIME = 3f;

		void Awake()
		{
			mText.enabled = false;
		}

		void Update()
		{
			if (Input.GetKey(KeyCode.Escape))
			{
				mHoldTimer += Time.deltaTime;

				if (mHoldTimer >= HOLD_TIME)
				{
					mHoldTimer = 0;
					mText.enabled = !mText.enabled;
				}
			}
			else if (mHoldTimer > 0)
			{
				mHoldTimer = 0;
			}
		}
	}
}
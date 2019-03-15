using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	[RequireComponent(typeof(Image))]
	public class ProfileImage : MonoBehaviour 
	{
		[SerializeField] private Image mImage;
		[SerializeField] private Image mFrame;

		private DataUserDetails mDetails;

		void OnValidate()
		{
			if (mImage == null)
				mImage = GetComponent<Image>();
		}

		void Reset()
		{
			if (mImage == null)
				mImage = GetComponent<Image>();
		}

		void OnDisable()
		{
			if (mDetails != null)
			{
				mDetails.OnGetImage -= OnGetImage;
			}
		}

		public void SetDetails(DataUserDetails details)
		{
			mDetails = details;
			mDetails.OnGetImage += OnGetImage;

			SetPhoto(mDetails.Image);

			if (mDetails.Image == null)
				mDetails.FetchImage();
		}

		private void OnGetImage()
		{
			SetPhoto(mDetails.Image);
		}

		private void SetPhoto(Sprite photo)
		{
			if (photo != null)
			{
				mImage.sprite = photo;
				mFrame.enabled = true;
				mImage.color = Color.white;
			}
			else
			{
				mFrame.enabled = false;
			}
		}
	}
}
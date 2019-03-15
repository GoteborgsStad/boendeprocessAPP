using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace ius
{
	public class ImportPhotoHandler : MonoBehaviour
	{
		private Action<Texture2D> mOnSuccess;
		private Action mOnCancel;

		private static ImportPhotoHandler mHandler;

		public static void GetPhoto(Action<Texture2D> onSuccess, Action onFail)
		{
			if (mHandler == null)
				mHandler = new GameObject("ImportPhotoHandler").AddComponent<ImportPhotoHandler>();

			mHandler.Import(onSuccess, onFail);
		}

		public void Import(Action<Texture2D> onSuccess, Action onFail)
		{
			mOnSuccess = onSuccess;
			mOnCancel = onFail;

			if (Application.isEditor)
				UsePhoto(Resources.Load<Texture2D>("test_photo"), "test_photo");
			else
				PickImage();
		}

		private void PickImage()
		{
			NativeGallery.Permission permission = NativeGallery.GetImageFromGallery(OnPickImage);

			if (permission == NativeGallery.Permission.Denied)
			{
				if (mOnCancel != null)
					mOnCancel();

				ClearEvents();
			}
		}

		private void ClearEvents()
		{
			mOnSuccess = null;
			mOnCancel = null;
		}

		private void OnPickImage(string path)
		{
			Texture2D texture = NativeGallery.LoadImageAtPath(path, 512, false, false);
			UsePhoto(texture, path);
		}

		private void UsePhoto(Texture2D photo, string fileName)
		{
			if (photo != null)
			{
				string[] splitName = fileName.Split('/');
				photo.name = splitName[splitName.Length - 1];
			}

			if (mOnSuccess != null)
				mOnSuccess(photo);

			ClearEvents();
		}

		private void Cancel()
		{
			if (mOnCancel != null)
				mOnCancel();

			ClearEvents();
		}
	}
}
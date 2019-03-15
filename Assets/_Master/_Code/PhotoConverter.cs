using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ius
{
	// This is a rather hacky solution used due to lack of better alternatives at the time of implementation
	[RequireComponent(typeof(Camera))]
	public class PhotoConverter : MonoBehaviour
	{
		[SerializeField] private Camera mRenderCamera;
		[SerializeField] private RawImage mRawFeedImage;
		[SerializeField] private RectTransform mPhotoArea;

		private AspectRatioFitter mAspectRatioFitter;

		private static PhotoConverter Instance;

		void Awake()
		{
			Instance = this;
			mAspectRatioFitter = mRawFeedImage.GetComponent<AspectRatioFitter>();
		}

		public static Texture2D ConvertToSquare(Texture originalPhoto, int size = 256)
		{
			if (size > 256)
			{
				size = 256;
				// Need to adjust camera size dynamically to allow any size
				Debug.LogWarning("256 is max size for PhotoConverter in this implementation");
			}

			return Instance.PerformSquareConversion(originalPhoto, size);
		}

		private Texture2D PerformSquareConversion(Texture originalPhoto, int size)
		{
			mRawFeedImage.texture = originalPhoto;
			mPhotoArea.sizeDelta = new Vector2(size, size);
			mAspectRatioFitter.aspectRatio = (float)originalPhoto.width / originalPhoto.height;

			RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
			mRenderCamera.targetTexture = renderTexture;
			mRenderCamera.Render();

			#region Calculate screenspace coordinates of the camera feed
			Vector3[] worldCorners = new Vector3[4];
			mPhotoArea.GetWorldCorners(worldCorners);

			// Translate world corners to screen points
			for (int i = 0; i < worldCorners.Length; i++)
			{
				worldCorners[i] = mRenderCamera.WorldToScreenPoint(worldCorners[i]);
			}

			// Calculate screenshot rectangle
			float photoX = worldCorners[0].x;
			float photoY = worldCorners[0].y;
			float photoW = mPhotoArea.sizeDelta.x; //worldCorners[2].x - photoX;
			float photoH = mPhotoArea.sizeDelta.y; //worldCorners[2].y - photoY;
			#endregion

			Rect rect = new Rect(photoX, photoY, photoW, photoH);
			Texture2D newPhoto = new Texture2D((int)photoW, (int)photoH, TextureFormat.RGB24, false);
			newPhoto.name = "S_" + originalPhoto.name;

			// Take screenshot of the calculate area
			RenderTexture previousRenderTexture = RenderTexture.active;
			RenderTexture.active = renderTexture;

			newPhoto.ReadPixels(rect, 0, 0, false);
			newPhoto.Apply();

			mRawFeedImage.texture = null;
			RenderTexture.active = previousRenderTexture;

			return newPhoto;
		}
	}
}
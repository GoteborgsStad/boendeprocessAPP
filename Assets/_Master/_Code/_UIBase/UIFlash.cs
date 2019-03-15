using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ius
{
	[RequireComponent(typeof(Image))]
	public sealed class UIFlash : MonoBehaviour
	{
		private Image image;
		private bool isFlashing;

		private static bool playStartFlash = true;
		private static Color startFlashColor = Color.white;

		private static UIFlash Instance;

		void Awake()
		{
			if (Instance != null)
			{
				Debug.LogWarning("Multiple UIFlash instances in the scene " + Instance.name + " and " + name);
				return;
			}

			Instance = this;
			image = GetComponent<Image>();
			image.enabled = false;

			if (playStartFlash)
				StartCoroutine(StartFlashRoutine());
		}

		private IEnumerator StartFlashRoutine()
		{
			playStartFlash = false;

			isFlashing = true;
			image.enabled = true;

			yield return StartCoroutine(FadeOut(startFlashColor, 4f));

			image.enabled = false;
			isFlashing = false;
		}

		/// <summary> If called, will fade in once the scene starts. </summary>
		public static void SetStartFlash(Color color)
		{
			playStartFlash = true;
			startFlashColor = color;
		}

		/// <summary>
		/// Performs a screen color flash.
		/// </summary>
		/// <param name="color">The color to flash on the screen.</param>
		/// <param name="time">Time in seconds for the flash to perform.</param>
		/// <param name="onOpaque">Called when the screen is completly flashed in the requested color; time * 0.5 seconds.</param>
		/// <param name="onDone">Called when the flash is completed; time seconds.</param>
		public static void Flash(Color color, float time, System.Action onOpaque = null, System.Action onDone = null)
		{
			if (!Instance.isFlashing)
				Instance.StartCoroutine(Instance.FlashRoutine(color, time, onOpaque, onDone));
			/*else
				Debug.LogWarning("Called Flash during an ongoing flash, check the logic");*/
		}

		private IEnumerator FlashRoutine(Color color, float time, System.Action onOpaque, System.Action onDone)
		{
			isFlashing = true;
			image.enabled = true;
			float speed = 2f / Mathf.Max(time, 0.01f);

			yield return StartCoroutine(FadeIn(color, speed));

			if (onOpaque != null)
				onOpaque();

			yield return StartCoroutine(FadeOut(color, speed));

			image.enabled = false;
			isFlashing = false;

			if (onDone != null)
				onDone();
		}

		private IEnumerator FadeIn(Color color, float speed)
		{
			float timer = 0;

			while (timer < 1f)
			{
				timer += Time.deltaTime * speed;
				color.a = timer;
				image.color = color;
				yield return null;
			}
		}

		private IEnumerator FadeOut(Color color, float speed)
		{
			float timer = 1f;

			while (timer > 0f)
			{
				timer -= Time.deltaTime * speed;
				color.a = timer;
				image.color = color;
				yield return null;
			}
		}
	}
}
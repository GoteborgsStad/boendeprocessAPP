using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class SlideActivated : MonoBehaviour 
	{
		[SerializeField] private bool useFade;
		[SerializeField] private float fadeTime = 0.25f;
		[SerializeField] private float fadeValue = 0.5f;

		private UnityEngine.UI.Image toFade;

		void Awake()
		{
			UISlideElement slideElement = GetComponentInParent<UISlideElement>();

			if (slideElement == null)
			{
				Debug.LogWarning(name + " SlideActivated couldn't find a UISlideElement");
				gameObject.SetActive(false);
				return;
			}

			slideElement.onStartSlideIn += OnSlideIn; // (() => gameObject.SetActive(true));
			slideElement.onIsOut += OnSlideOut; // (() => gameObject.SetActive(false));

			toFade = GetComponent<UnityEngine.UI.Image>();

			gameObject.SetActive(false);
		}

		private void OnSlideIn()
		{
			gameObject.SetActive(true);

			if (useFade && toFade != null)
				StartCoroutine(FadeInRoutine());
		}

		private void OnSlideOut()
		{
			if (useFade && toFade != null && gameObject.activeSelf)
				StartCoroutine(FadeOutRoutine());
			else
				gameObject.SetActive(false);
		}

		private IEnumerator FadeInRoutine()
		{
			float timer = 0f;
			float speed = 1f / Mathf.Max(fadeTime, 0.01f);
			Color color = toFade.color;

			while (timer < 1f)
			{
				timer += Time.deltaTime * speed;
				color.a = Mathf.Clamp01(timer) * fadeValue;
				toFade.color = color;
				yield return null;
			}
		}

		private IEnumerator FadeOutRoutine()
		{
			float timer = 1f;
			float speed = 1f / Mathf.Max(fadeTime, 0.01f);
			Color color = toFade.color;

			while (timer > 0f)
			{
				timer -= Time.deltaTime * speed;
				color.a = Mathf.Clamp01(timer) * fadeValue;
				toFade.color = color;
				yield return null;
			}

			gameObject.SetActive(false);
		}
	}
}
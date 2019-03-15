using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	[RequireComponent(typeof(Graphic))]
	public class UIBlinkingElement : MonoBehaviour 
	{
		[SerializeField] private float blinkTime = 0.3f;
		[SerializeField] private Color blinkColor = Color.cyan;
		[SerializeField] private bool blinkOnStart;

		private Graphic blinkElement;
		private Color baseColor;
		private float timer;
		private float speed;

		public bool IsBlinking { get; private set; }

		void Awake()
		{
			blinkElement = GetComponent<Graphic>();
			baseColor = blinkElement.color;
			speed = 1f / Mathf.Max(blinkTime, 0.01f);

			if (blinkOnStart)
				Begin();
			else
				enabled = false;
		}

		public void Begin()
		{
			IsBlinking = true;
			enabled = true;
		}

		public void End()
		{
			IsBlinking = false;
			speed = -Mathf.Abs(speed);
		}

		void Update()
		{
			timer += Time.deltaTime * speed;

			blinkElement.color = Color.Lerp(baseColor, blinkColor, timer);

			if (timer < 0f && speed < 0f && !IsBlinking)
			{
				enabled = false;
			}
			else if ((timer > 1f && speed > 0) || (timer < 0f && speed < 0))
			{
				speed = -speed;
				timer = Mathf.Clamp01(timer);
			}
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	[RequireComponent(typeof(Graphic))]
	public class UIScalingElement : MonoBehaviour 
	{
		[SerializeField] private float scaleTime = 0.3f;
		[SerializeField] private float scaleMultiplier = 1.5f;
		[SerializeField] private bool activeOnStart;
		[SerializeField] private bool loop;

		private Transform myTransform;
		private Vector3 scaleFrom, scaleTo;
		private float timer;
		private float speed;

		public bool IsScaling { get; private set; }

		void Awake()
		{
			myTransform = transform;
			scaleFrom = myTransform.localScale;
			scaleTo = scaleFrom * scaleMultiplier;
			speed = 1f / Mathf.Max(scaleTime, 0.01f);

			if (activeOnStart)
				Begin();
			else
				enabled = false;
		}

		public void Begin()
		{
			IsScaling = true;
			enabled = true;
		}

		public void End()
		{
			IsScaling = false;
			speed = -Mathf.Abs(speed);
		}

		void Update()
		{
			timer += Time.deltaTime * speed;
			
			myTransform.localScale = Vector3.Lerp(scaleFrom, scaleTo, Mathfx.Hermite(0f, 1f, timer));

			if (timer < 0f && speed < 0f && !IsScaling)
			{
				if (loop)
				{
					speed = -speed;
					timer = Mathf.Clamp01(timer);
				}
				else
				{
					enabled = false;
				}
			}
			else if ((timer > 1f && speed > 0) || (timer < 0f && speed < 0))
			{
				speed = -speed;
				timer = Mathf.Clamp01(timer);
			}
		}
	}
}
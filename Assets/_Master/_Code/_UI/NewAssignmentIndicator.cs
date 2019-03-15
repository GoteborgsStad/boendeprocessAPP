using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	[RequireComponent(typeof(Image))]
	public class NewAssignmentIndicator : MonoBehaviour
	{
		[SerializeField] private Image mImage;
		
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

		void OnEnable()
		{
			mImage.enabled = false;
			GlobalStatus.OnStatusUpdate += UpdateState;
		}

		void OnDisable()
		{
			GlobalStatus.OnStatusUpdate -= UpdateState;
		}

		private void UpdateState()
		{
			mImage.enabled = GlobalStatus.HasNewAssignment;
		}
	}
}
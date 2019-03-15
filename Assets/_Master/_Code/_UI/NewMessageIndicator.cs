using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	[RequireComponent(typeof(Image))]
	public class NewMessageIndicator : MonoBehaviour
	{
		[SerializeField] private Image mImage;

		private DataUser mContact;

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
			if (mContact == null)
				mImage.enabled = false;

			GlobalStatus.OnStatusUpdate += UpdateState;
		}

		void OnDisable()
		{
			GlobalStatus.OnStatusUpdate -= UpdateState;
		}

		public void Initialize(DataUser user)
		{
			mContact = user;
			UpdateState();
		}

		private void UpdateState()
		{
			if (mContact == null)
				mImage.enabled = GlobalStatus.HasNewMessage;
			else
				mImage.enabled = GlobalStatus.HasMessages(mContact.ID);
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	public class SubmitAssignmentPage : MonoBehaviour 
	{
		private enum SubmissionInput
		{
			Photo = 1,
			Write = 2,
			Check = 3,
			All = 4
		}

		[Header("Title")]
		[SerializeField] private Image mTitleIcon;
		[SerializeField] private Text mTitle;
		[SerializeField] private Text mCategory;

		[Header("Submission - Root")]
		[SerializeField] private GameObject mCheck;
		[SerializeField] private GameObject mPhoto;
		[SerializeField] private GameObject mWrite;
		[SerializeField] private Button mSubmitButton;

		[Header("Submission - Common")]
		[SerializeField] private Color mNotDoneColor;
		[SerializeField] private Color mDoneColor;

		[Header("Check")]
		[SerializeField] private Image mCheckIcon;
		[SerializeField] private Image mCheckMark;
		
		[Header("Photo")]
		[SerializeField] private Image mPhotoIcon;
		
		[Header("Write")]
		[SerializeField] private Image mWriteIcon;
		[SerializeField] private InputField mWriteField;
		
		private DataAssignment mAssignment;

		private Texture2D mSelectedPhoto;

		public void SetAssignment(DataAssignment assignment)
		{
			mAssignment = assignment;

			Clear();

			mTitle.text = mAssignment.Name;
			mCategory.text = mAssignment.Category.Name;

			for (int i = 0; i < mAssignment.Forms.Length; i++)
			{
				SubmissionInput formType = (SubmissionInput)mAssignment.Forms[i].ID;

				switch (formType)
				{
					case SubmissionInput.Photo: mPhoto.SetActive(true); break;
					case SubmissionInput.Write: mWrite.SetActive(true); break;
					case SubmissionInput.Check: mCheck.SetActive(true); break;

					case SubmissionInput.All:
						mPhoto.SetActive(true);
						mWrite.SetActive(true);
						mCheck.SetActive(true);
						break;
				}
			}
		}

		private void Clear()
		{
			mCheck.SetActive(false);
			mCheckIcon.color = mNotDoneColor;
			mCheckMark.enabled = false;

			mPhoto.SetActive(false);
			mSelectedPhoto = null;
			mPhotoIcon.color = mNotDoneColor;

			mWrite.SetActive(false);
			mWriteIcon.color = mNotDoneColor;
			mWriteField.text = string.Empty;

			mSubmitButton.interactable = false;
		}

		public void ButtonCheck()
		{
			mCheckMark.enabled = !mCheckMark.enabled;

			UpdateSubmitButton();
		}

		public void ButtonPhoto()
		{
			ImportPhotoHandler.GetPhoto(OnGetPhoto, OnGetPhotoFail);
		}

		public void WriteFieldUpdate()
		{
			UpdateSubmitButton();
		}

		public void ButtonSubmit()
		{
			string text = null;
			Texture2D photo = null;

			if (mWrite.activeSelf)
				text = mWriteField.text;
			if (mPhoto.activeSelf)
				photo = mSelectedPhoto;

			Backend.SubmitAssignment(mAssignment.ID, text, photo);

			Clear();
			UIManager.Open(UILocation.Assignment);
		}

		private void OnGetPhoto(Texture2D photo)
		{
			mSelectedPhoto = photo;
			UpdateSubmitButton();
		}

		private void OnGetPhotoFail()
		{
			mSelectedPhoto = null;
			UpdateSubmitButton();
		}

		private void UpdateSubmitButton()
		{
			mSubmitButton.interactable = EvaluateSubmitInputs();
		}

		private bool EvaluateSubmitInputs()
		{
			bool canSubmit = true;

			if (mCheck.activeSelf) // mInputType == SubmissionInput.Check || mInputType == SubmissionInput.All)
			{
				mCheckIcon.color = mCheckMark.enabled ? mDoneColor : mNotDoneColor;

				if (!mCheckMark.enabled)
					canSubmit = false;
			}

			if (mWrite.activeSelf) // mInputType == SubmissionInput.Write || mInputType == SubmissionInput.All)
			{
				bool hasText = mWriteField.text.Length > 3;
				mWriteIcon.color = hasText ? mDoneColor : mNotDoneColor;

				if (!hasText)
					canSubmit = false;
			}

			if (mPhoto.activeSelf) // mInputType == SubmissionInput.Photo || mInputType == SubmissionInput.All)
			{
				bool hasPhoto = mSelectedPhoto != null;
				mPhotoIcon.color = hasPhoto ? mDoneColor : mNotDoneColor;

				if (!hasPhoto)
					canSubmit = false;
			}

			return canSubmit;
		}
	}
}
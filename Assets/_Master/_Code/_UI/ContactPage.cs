using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class ContactPage : MonoBehaviour 
	{
		[SerializeField] private ContactButton[] mMainContactButtons;
		[SerializeField] private GameObject[] mMainPlaceholders;
		[SerializeField] private Transform mRowTemplate;
		[SerializeField] private ContactButton mButtonTemplate;
		[SerializeField] private Transform mPlaceholderTemplate;

		private GameObject mGameObject;
		private ScreenChat mChatScreen;

		private Transform[] mRows;
		private List<Transform> mButtonPlaceholders = new List<Transform>();

		private const int BUTTONS_PER_ROW = 3;
						
		public void Initialize(ScreenChat chat)
		{
			mGameObject = gameObject;
			mChatScreen = chat;
			mRowTemplate.gameObject.SetActive(false);
		}

		public void Open()
		{
			mGameObject.SetActive(true);
			Refresh();
		}

		public void Close()
		{
			mGameObject.SetActive(false);
		}

		public void Refresh()
		{
			if (DataManager.Me.Data == null)
			{
				SetMainContacts(new DataUser[0]);
				SetOtherContacts(new DataUser[0]);
				return;
			}
			
			DataMe me = DataManager.Me.Data[0];

			DataUser[] mainContacts = RelationsToContacts(me.Relationships);
			SetMainContacts(mainContacts);
			SetOtherContacts(me.Contacts);
		}

		private DataUser[] RelationsToContacts(DataRelationship[] relations)
		{
			DataUser[] result = new DataUser[relations.Length];

			for (int i = 0; i < result.Length; i++)
			{
				result[i] = relations[i].Contact;
			}

			return result;
		}

		private void SetMainContacts(DataUser[] contacts)
		{
			if (contacts.Length > 3)
				Debug.LogWarning("More than 3 main contacts, not all will be visible");

			for (int i = 0; i < mMainContactButtons.Length; i++)
			{
				if (i < contacts.Length)
				{
					mMainContactButtons[i].Initialize(mChatScreen, contacts[i]);
					mMainPlaceholders[i].SetActive(false);
				}
				else
				{
					mMainContactButtons[i].Hide();
					mMainPlaceholders[i].SetActive(true);
				}
			}
		}

		private void SetOtherContacts(DataUser[] others)
		{
			// Clear rows
			if (mRows != null)
			{
				for (int i = 0; i < mRows.Length; i++)
				{
                    if (mRows[i] != null)
					    Destroy(mRows[i].gameObject);
				}
			}

			mRows = new Transform[Mathf.CeilToInt((float)others.Length / BUTTONS_PER_ROW)];
			int row = -1;
			int entryCount = 3;

			// Create rows
			for (int i = 0; i < others.Length; i++)
			{
				DataUser contact = others[i];

				// Fill rows with contacts
				if (entryCount > 2)
				{
					// Row full, create new row and start filling it instead
					entryCount = 0;
					row++;

					Transform newRow = Instantiate(mRowTemplate, mRowTemplate.parent);
					mRows[row] = newRow;
					newRow.gameObject.SetActive(true);

					for (int j = newRow.childCount - 1; j >= 0; j--)
					{
						Destroy(newRow.GetChild(j).gameObject);
					}
				}
				else
				{
					entryCount++;
				}

				Instantiate(mButtonTemplate, mRows[row]).Initialize(mChatScreen, contact);
			}
			
			SetOtherPlaceHolders(mRows.Length * 3 - others.Length);
		}

		private void SetOtherPlaceHolders(int toCreate)
		{
			for (int i = 0; i < mButtonPlaceholders.Count; i++)
			{
				Destroy(mButtonPlaceholders[i].gameObject);
			}

			mButtonPlaceholders.Clear();

			if (mRows.Length == 0)
				return;

			Transform lastRow = mRows.Last();

			for (int i = 0; i < toCreate; i++)
			{
				Instantiate(mPlaceholderTemplate, lastRow);
			}
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Globalization;

namespace ius
{
	public class ScreenMonthlyCheck : ScreenBase 
	{
		[SerializeField] [TextTag] private string mHeaderTag;
		[SerializeField] private PageNavigation mNavigation;

		[SerializeField] [TextTag] private string mLatestCheckTag;
		[SerializeField] [TextTag] private string mRemainingGoalsTag;
		[SerializeField] [TextTag] private string mCompletedTag;
		[SerializeField] [TextTag] private string mGoalStatusTag;

		[SerializeField] private Text mCounterText;
		[SerializeField] private Text mCounterInfoText;

		[SerializeField] private RectTransform mCurrentView;
		[SerializeField] private RectTransform mPreviousView;

		[Header("Prefabs")]
		[SerializeField] private MonthlyHeader mHeaderCurrentPrefab;
		[SerializeField] private MonthlyHeader mHeaderPreviousPrefab;
		[SerializeField] private MonthlyCheck mCheckPrefab;
		[SerializeField] private MonthlyGoal mGoalPrefab;

		private List<GameObject> mUIContent = new List<GameObject>();

		private RectTransform mActiveView;
		private CultureInfo mLanguageInfo;

		private bool mFocusIsOngoing;
		private MonthlyGoal mFocusGoal;
		private bool mEnteredWithFocus;

		private static DataGoal mToFocus;

		public static void Open(DataGoal toFocus)
		{
			mToFocus = toFocus;
			UIManager.Open(UILocation.MonthlyCheck);
		}

		void OnEnable()
		{
			mLanguageInfo = CultureInfo.CreateSpecificCulture("sv-SE");

			mCurrentView.SetToTop();
			mPreviousView.SetToTop();

			DataManager.Evaluation.OnDataUpdate += RefreshContent;
			DataManager.Plan.OnDataUpdate += RefreshContent;

			DataManager.Evaluation.FetchData();
			DataManager.Plan.FetchData();
		}

		void OnDisable()
		{
			DataManager.Evaluation.OnDataUpdate -= RefreshContent;
			DataManager.Plan.OnDataUpdate -= RefreshContent;

			Clear();
		}

		public override void RefreshUI()
		{
			mEnteredWithFocus = mToFocus != null;

			if (!mEnteredWithFocus)
				UIHeader.Show(mHeaderTag);
			else
				UIHeader.Show(mHeaderTag, () => UIManager.Open(UILocation.Progress));

			RefreshContent();

			if (mEnteredWithFocus)
			{
				mNavigation.SetPage(mFocusIsOngoing ? 0 : 1);

				if (mFocusIsOngoing)
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate(mCurrentView);

					Vector2 position = mCurrentView.anchoredPosition;
					position.y = -mFocusGoal.GetComponent<RectTransform>().anchoredPosition.y;
					mCurrentView.anchoredPosition = position;
				}
				else
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate(mPreviousView);

					Vector2 position = mPreviousView.anchoredPosition;
					position.y = -mFocusGoal.GetComponent<RectTransform>().anchoredPosition.y;
					mPreviousView.anchoredPosition = position;

					//Debug.Log("Move to wanted position. " + position.y);
				}

				mFocusGoal.Flash();
				mFocusGoal = null;
				mToFocus = null;
			}
		}

		private void RefreshContent()
		{
			Clear();

			if (DataManager.Evaluation.Data == null)
				return;

			RefreshCounter();
			RefreshCurrent();
			RefreshPrevious();
		}

		private void Clear()
		{
			for (int i = 0; i < mUIContent.Count; i++)
			{
				Destroy(mUIContent[i]);
			}

			mUIContent.Clear();

			mCounterText.text = string.Empty;
			mCounterInfoText.text = string.Empty;
		}

		private void RefreshCounter()
		{
			if (DataManager.Plan.Data != null)
			{
				int totalGoals = DataManager.Plan.OngoingGoals.Length + DataManager.Plan.CompletedGoals.Length;
				mCounterText.text = DataManager.Plan.CompletedGoals.Length + "/" + totalGoals;
			}
			else
			{
				mCounterText.text = "?/?";
			}
			
			if (DataManager.Me.Data != null)
			{
				string startDate = DataManager.Me.Data[0].CreatedAt.Value.ToString("yyyy-MM-dd", mLanguageInfo);
				mCounterInfoText.text = TextManager.Get(mGoalStatusTag) + " <b>" + startDate + "</b>";
			}
			else
			{
				mCounterInfoText.text = TextManager.Get(mGoalStatusTag);
			}
		}

		private void RefreshCurrent()
		{
			if (DataManager.Evaluation.Data == null || DataManager.Plan.Data == null)
				return;

			SetActiveView(mCurrentView);
			
			Add(mHeaderCurrentPrefab).SetTitle(TextManager.Get(mLatestCheckTag));

			if (DataManager.Evaluation.Done.Length > 0)
				Add(mCheckPrefab).SetData(DataManager.Evaluation.Done[0], mLanguageInfo);
			
			int remainingGoalCount = DataManager.Plan.OngoingGoals.Length;

			if (remainingGoalCount > 0)
			{
				Add(mHeaderCurrentPrefab).SetTitle(TextManager.Get(mRemainingGoalsTag) + " (" + remainingGoalCount + ")");

				for (int i = 0; i < DataManager.Plan.OngoingGoals.Length; i++)
				{
					DataGoal goal = DataManager.Plan.OngoingGoals[i];
					MonthlyGoal created = Add(mGoalPrefab);
					created.SetData(goal);

					if (mToFocus != null && goal.ID == mToFocus.ID)
					{
						mFocusGoal = created;
						mFocusIsOngoing = true;
					}
				}
			}
		}

		private void RefreshPrevious()
		{
			if (DataManager.Evaluation.Data == null || DataManager.Plan.Data == null)
				return;

			SetActiveView(mPreviousView);
			
			// Create an array of all represented months in our data
			DateTime[] months = CreateCompletedMonthList();

			for (int i = 0; i < months.Length; i++)
			{
				DateTime currentMonth = months[i];

				// Add month header
				string formatedMonth = currentMonth.ToString("MMMM yyyy", mLanguageInfo).ToUpper();
				string monthBarTitle = TextManager.Get(mCompletedTag) + " " + formatedMonth;
				Add(mHeaderPreviousPrefab).SetTitle(monthBarTitle);

				// Find the evaluation for this month and add it
				for (int j = 0; j < DataManager.Evaluation.Done.Length; j++)
				{
					DataEvaluation evaluation = DataManager.Evaluation.Done[j];
					DateTime date = evaluation.CreatedAt.Value;

					if (date.Year == currentMonth.Year && date.Month == currentMonth.Month)
					{
						Add(mCheckPrefab).SetData(evaluation, mLanguageInfo);
					}
				}

				// Find the goals for this month and add them
				for (int j = 0; j < DataManager.Plan.CompletedGoals.Length; j++)
				{
					DataGoal goal = DataManager.Plan.CompletedGoals[j];
					DateTime date = goal.FinishedAt.Value;

					if (date.Year == currentMonth.Year && date.Month == currentMonth.Month)
					{
						MonthlyGoal created = Add(mGoalPrefab);
						created.SetData(goal);

						if (mToFocus != null && goal.ID == mToFocus.ID)
						{
							mFocusGoal = created;
							mFocusIsOngoing = false;
						}
					}
				}
			}
		}

		private DateTime[] CreateCompletedMonthList()
		{
			List<DateTime> months = new List<DateTime>();

			for (int i = 0; i < DataManager.Evaluation.Done.Length; i++)
			{
				DateTime date = DataManager.Evaluation.Done[i].CreatedAt.Value;
				DateTime month = new DateTime(date.Year, date.Month, 1);

				if (!months.Contains(month))
				{
					months.Add(month);
				}
			}

			for (int i = 0; i < DataManager.Plan.CompletedGoals.Length; i++)
			{
				DateTime date = DataManager.Plan.CompletedGoals[i].FinishedAt.Value;
				DateTime month = new DateTime(date.Year, date.Month, 1);

				if (!months.Contains(month))
				{
					months.Add(month);
				}
			}

			months.Sort();
			months.Reverse();

			return months.ToArray();
		}

		private void SetActiveView(RectTransform view)
		{
			mActiveView = view;
		}

		private T Add<T>(T prefab) where T : MonoBehaviour
		{
			T created = Instantiate(prefab, mActiveView);
			mUIContent.Add(created.gameObject);
			return created;
		}
	}
}
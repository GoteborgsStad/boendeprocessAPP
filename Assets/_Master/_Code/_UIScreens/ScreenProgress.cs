using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	public class ScreenProgress : ScreenBase 
	{
		[Header("Flower count")]
		[SerializeField] private UISlideElement mFlowerArea;
		[SerializeField] private Text mFlowerCountText;

		[Header("Indicators")]
		[SerializeField] private UISlideElement mIndicatorArea;
		[SerializeField] private Image mIndicatorLiving;
		[SerializeField] private Image mIndicatorParticipation;
		[SerializeField] private Image mIndicatorAssignment;
		[SerializeField] private Sprite[] mIndicationSprites;

		void OnEnable()
		{
			DataManager.Evaluation.OnDataUpdate += RefreshHeader;
		}

		void OnDisable()
		{
			DataManager.Evaluation.OnDataUpdate -= RefreshHeader;
		}

		public override void RefreshUI()
		{
			UIHeader.Hide();
			PlantManager.SetVisible(true);

			RefreshHeader();
		}

		private void RefreshHeader()
		{
			if (DataManager.Evaluation.Data == null || DataManager.Plan.Data == null)
			{
				mFlowerArea.SetPositionOut();
				mIndicatorArea.SetPositionOut();
				return;
			}

			// Set flower count
			int flowerTotal = DataManager.Plan.OngoingGoals.Length + DataManager.Plan.CompletedGoals.Length;
			int flowerDone = DataManager.Plan.CompletedGoals.Length;

			if (flowerTotal > 0)
			{
				mFlowerArea.BeginMoveIn();
				mFlowerCountText.text = flowerDone + "/" + flowerTotal;
			}

			// Set indicators
			if (DataManager.Evaluation.Done.Length > 0)
			{
				DataEvaluation evaluation = DataManager.Evaluation.Done.Last();

				mIndicatorLiving.sprite = mIndicationSprites[Mathf.Min(evaluation.Answers[0].Rating, mIndicationSprites.Length - 1)];
				mIndicatorParticipation.sprite = mIndicationSprites[Mathf.Min(evaluation.Answers[1].Rating, mIndicationSprites.Length - 1)];
				mIndicatorAssignment.sprite = mIndicationSprites[Mathf.Min(evaluation.Answers[2].Rating, mIndicationSprites.Length - 1)];

				mIndicatorArea.BeginMoveIn();
			}
			else
			{
				// No evaluation done yet, hide indicators
				mIndicatorArea.SetPositionOut();
			}
		}

		public override void OnScreenExited()
		{
			PlantManager.SetVisible(false);
		}
	}
}
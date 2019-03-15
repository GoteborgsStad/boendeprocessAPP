using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Globalization;

namespace ius
{
	public class MonthlyCheck : MonoBehaviour 
	{
		[Header("UI References")]
		[SerializeField] private Text mTitle;
		[SerializeField] private MonthlyCheckTopic[] mTopics;

		[Header("Resources")]
		[SerializeField] [TextTag] private string mCheckTitleTag;
		[SerializeField] private Sprite[] mRatingSprites;

		public void SetData(DataEvaluation data, CultureInfo languageInfo)
		{
			string formatedMonth = data.CreatedAt.Value.ToString("MMMM yyyy", languageInfo).ToUpper();
			mTitle.text = TextManager.Get(mCheckTitleTag) + " " + formatedMonth;
			
			for (int i = 0; i < mTopics.Length; i++)
			{
				if (i < data.Answers.Length)
				{
					DataEvaluationAnswer answer = data.Answers[i];
                    //Sprite ratingSprite = mRatingSprites[Mathf.Min(answer.Rating, mRatingSprites.Length - 1)];

                    int rating = answer.Rating;
                    if (rating < 1)
                        rating = 1;
                    else if (rating > 6)
                        rating = 6;

                    Sprite ratingSprite = mRatingSprites[rating - 1];
					string text = "<b>" + answer.Category.Name + "</b>\n" + answer.Body;
					mTopics[i].SetData(ratingSprite, text);
				}
				else
				{
					mTopics[i].gameObject.SetActive(false);
				}
			}
		}
	}
}
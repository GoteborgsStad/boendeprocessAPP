using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace ius
{
	public class AssignmentContentFormat 
	{
		public static string Create(DataAssignment data, bool clampLength)
		{
			if (data.IsSubmitted)
				return CreateFormatedSubmitString(data, clampLength);
			else
				return CreateFormatedTimeString(data, clampLength);
		}

		private static string CreateFormatedSubmitString(DataAssignment data, bool clampLength)
		{
			string content = string.Empty;

			if (data.IsWaiting)
				content += "<b><color=#F18700FF>Inlämnad - inväntar bekräftelse</color></b>\n";
			else
				content += "<b>Avklarat <color=#52AA50FF>" + GetDateString(data.FinishedAt.Value) + "</color></b>\n";

			content += GetDescriptionString(data, clampLength);

			return content;
		}

		private static string CreateFormatedTimeString(DataAssignment data, bool clampLength)
		{
			string content = "<b>";

			// End date
			content += "Datum: <color=#01C0D1FF>";
			content += GetDateString(data.EndAt.Value);
			content += "</color> ";

			// Remaining timespan
			content += GetRemainingDaysString(data);
			
			content += "</b>\n";

			// Active timespan
			if (data.EndAt.HasValue && data.StartAt.HasValue)
			{
				TimeSpan timeSpan = data.EndAt.Value - data.StartAt.Value;

				if (timeSpan.TotalMinutes > 10 && timeSpan.TotalHours < 20)
				{
					// Show time span
					content += "<b>Klockan: <color=#01C0D1FF>"
						+ data.StartAt.Value.ToString("HH:mm") + "</color> till <color=#01C0D1FF>"
						+ data.EndAt.Value.ToString("HH:mm") + "</color></b>\n";
				}
			}

			content += GetDescriptionString(data, clampLength);

			return content;
		}

		private static string GetDescriptionString(DataAssignment data, bool clampLength)
		{
			string result = string.Empty;
			
			if (!string.IsNullOrEmpty(data.Description))
			{
				bool isLong = clampLength && data.Description.Length > 95;

				if (isLong)
				{
					result += data.Description.Substring(0, 95);
					result += "<color=#01C0D1FF><b> ...mer</b></color>";
				}
				else
				{
					result += data.Description;
				}
			}

			return result;
		}

		private static string GetRemainingDaysString(DataAssignment data)
		{
			string result = string.Empty;

			// Remaining timespan
			int daysToEnd = DateHelper.DaysToEnd(data.EndAt.Value);

			string color = data.StatusObject.Color;
			string days = Mathf.Abs(daysToEnd).ToString();

			if (daysToEnd < -1)
				result = TextManager.Get("Time_Late");
			else if (daysToEnd < 0)
				result = TextManager.Get("Time_Yesterday");
			else if (daysToEnd < 1)
				result = TextManager.Get("Time_Today");
			else if (daysToEnd < 2)
				result = TextManager.Get("Time_Tomorrow");
			else
				result = TextManager.Get("Time_Future");

			result = result.Replace("<COLOR>", color).Replace("<DAYS>", days);
			
			return result;
		}

		private static string GetDateString(DateTime date)
		{
			string result = string.Empty;

			int endDay = date.Day;
			result += endDay.ToString();

			if (endDay == 1 || endDay == 2 || endDay == 21 || endDay == 22 || endDay == 31)
				result += ":a ";
			else
				result += ":e ";

			result += date.ToString("MMMM", CultureInfo.CreateSpecificCulture("sv-SE"));

			return result;
		}
	}
}
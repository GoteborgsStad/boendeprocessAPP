using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ius
{
	public static class DateHelper 
	{
		public static int DaysToEnd(DateTime date)
		{
			TimeSpan timeToEnd = date.Date - DateTime.Now.Date;
			return Mathf.RoundToInt((float)timeToEnd.TotalDays);
		}
	}
}
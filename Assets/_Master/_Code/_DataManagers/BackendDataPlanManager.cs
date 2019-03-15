using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ius
{
	public class BackendDataPlanManager : BackendDataManager<DataPlan>
	{
		public DataGoal[] OngoingGoals { get; private set; }
		public DataGoal[] CompletedGoals { get; private set; }

		public BackendDataPlanManager(Action backendFetch, ref Action<string> backendSuccess, ref Action<WebCall> backendFail)
			: base(backendFetch, false, ref backendSuccess, ref backendFail)
		{

		}

		protected override void HandleData()
		{
			List<DataGoal> ongoing = new List<DataGoal>();
			List<DataGoal> completed = new List<DataGoal>();

			for (int i = 0; i < Data[0].Goals.Length; i++)
			{
				DataGoal goal = Data[0].Goals[i];

				if (goal.IsDone)
					completed.Add(goal);
				else
					ongoing.Add(goal);
			}

			OngoingGoals = ongoing.ToArray();
			CompletedGoals = completed.ToArray();
		}
	}
}
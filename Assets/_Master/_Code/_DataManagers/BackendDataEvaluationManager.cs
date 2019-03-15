using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ius
{
	public class BackendDataEvaluationManager : BackendDataManager<DataEvaluation>
	{
		public DataEvaluation[] Done { get; private set; }
		public DataEvaluation[] Waiting { get; private set; }

		public BackendDataEvaluationManager(Action backendFetch, ref Action<string> backendSuccess, ref Action<WebCall> backendFail)
			: base(backendFetch, true, ref backendSuccess, ref backendFail)
		{

		}

		protected override void HandleData()
		{
			List<DataEvaluation> done = new List<DataEvaluation>();
			List<DataEvaluation> waiting = new List<DataEvaluation>();

			for (int i = 0; i < Data.Length; i++)
			{
				if (Data[i].Answers.Length > 0)
					done.Add(Data[i]);
				else
					waiting.Add(Data[i]);
			}

			Done = done.ToArray();
			Waiting = waiting.ToArray();
		}
	}
}
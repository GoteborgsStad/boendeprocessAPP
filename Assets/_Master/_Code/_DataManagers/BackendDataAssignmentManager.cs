using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ius
{
	public class BackendDataAssignmentManager : BackendDataManager<DataAssignment>
	{
		private Dictionary<int, DataAssignment> mAssignmentByID;

		public BackendDataAssignmentManager(Action backendFetch, ref Action<string> backendSuccess, ref Action<WebCall> backendFail)
			: base(backendFetch, true, ref backendSuccess, ref backendFail)
		{

		}

		protected override void HandleData()
		{
			mAssignmentByID = new Dictionary<int, DataAssignment>(Data.Length);

			for (int i = 0; i < Data.Length; i++)
			{
				DataAssignment assignment = Data[i];
				mAssignmentByID[assignment.ID] = assignment;
			}
		}

		public DataAssignment GetAssignment(int id)
		{
			if (!mAssignmentByID.ContainsKey(id))
			{
				Debug.LogError("Could not find Assignment with ID: " + id);
				return null;
			}

			return mAssignmentByID[id];
		}
	}
}
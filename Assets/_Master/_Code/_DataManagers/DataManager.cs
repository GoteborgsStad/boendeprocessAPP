using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ius
{
	public static class DataManager
	{
		public static BackendDataManager<DataMe> Me { get; private set; }
		public static BackendDataManager<DataGlobalStatus> Status { get; private set; }
		public static BackendDataManager<DataNameObject> FAQ { get; private set; }
		public static BackendDataPlanManager Plan { get; private set; }
		public static BackendDataEvaluationManager Evaluation { get; private set; }
		public static BackendDataAssignmentManager Assignment { get; private set; }

		public static bool AllHasData
		{
			get
			{
				return Me.Data != null
					&& Status.Data != null
					&& FAQ.Data != null
					&& Plan.Data != null
					&& Evaluation.Data != null
					&& Assignment.Data != null;
			}
		}

		public static Action<WebCall> OnInitialFetchFail;

		public static string DataDebugState
		{
			get
			{
				return "Me: " + (Me.Data != null)
					+ ", Status: " + (Status.Data != null)
					+ ", FAQ: " + (FAQ.Data != null)
					+ ", Plan: " + (Plan.Data != null)
					+ ", Evaluation: " + (Evaluation.Data != null)
					+ ", Assignment: " + (Assignment.Data != null);
			}
		}

		static DataManager()
		{
			Backend.OnLogOut += CreateManagers;

			CreateManagers();
		}

		private static void CreateManagers()
		{
			Me = new BackendDataManager<DataMe>(Backend.GetMe, false, ref Backend.OnGetMeSuccess, ref Backend.OnGetMeFail);
			Status = new BackendDataManager<DataGlobalStatus>(Backend.GetStatus, true, ref Backend.OnGetStatusSuccess, ref Backend.OnGetStatusFail);
			FAQ = new BackendDataManager<DataNameObject>(Backend.GetFAQs, true, ref Backend.OnGetFAQsSuccess, ref Backend.OnGetFAQsFail);
			Plan = new BackendDataPlanManager(Backend.GetPlan, ref Backend.OnGetPlanSuccess, ref Backend.OnGetPlanFail);
			Evaluation = new BackendDataEvaluationManager(Backend.GetEvaluations, ref Backend.OnGetEvaluationsSuccess, ref Backend.OnGetEvaluationsFail);
			Assignment = new BackendDataAssignmentManager(Backend.GetAssignments, ref Backend.OnGetAssignmentsSuccess, ref Backend.OnGetAssignmentsFail);

			Me.OnDataFetchFail += OnDataFetchFail;
			Status.OnDataFetchFail += OnDataFetchFail;
			FAQ.OnDataFetchFail += OnDataFetchFail;
			Plan.OnDataFetchFail += OnDataFetchFail;
			Evaluation.OnDataFetchFail += OnDataFetchFail;
			Assignment.OnDataFetchFail += OnDataFetchFail;
		}

		public static void FetchInitialData()
		{
			if (Me.Data == null) Me.FetchData(true);
			if (FAQ.Data == null) FAQ.FetchData(true);
			if (Plan.Data == null) Plan.FetchData(true);
			if (Evaluation.Data == null) Evaluation.FetchData(true);
			
			if (Status.Data == null)
			{
				GlobalStatus.OnStatusUpdate -= FetchAssignmentAfterStatus;
				GlobalStatus.OnStatusUpdate += FetchAssignmentAfterStatus;
				GlobalStatus.ForceUpdate();
			}
			else if (Assignment.Data == null)
			{
				GlobalStatus.OnStatusUpdate -= FetchAssignmentAfterStatus;
				Assignment.FetchData(true);
			}
		}

		private static void FetchAssignmentAfterStatus()
		{
			GlobalStatus.OnStatusUpdate -= FetchAssignmentAfterStatus;
			Assignment.FetchData(true);
		}

		private static void OnDataFetchFail(WebCall webCall)
		{
			if (!AllHasData && OnInitialFetchFail != null)
			{
				OnInitialFetchFail(webCall);
			}
		}
	}
}
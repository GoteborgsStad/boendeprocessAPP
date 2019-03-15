using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Debug = UnityEngine.Debug;

namespace ius
{
	public class BackendDataManager<T1> where T1 : IDataSettable, new()
	{
		private Action mFetchMethod;
		private bool mDataIsArray;

		private float mNextFetchTime = -1;
		private string mPreviousJson;

		public Action OnDataUpdate;
		public Action<WebCall> OnDataFetchFail;

		public T1[] Data { get; private set; }

		private const float MAX_FETCH_TIME = 10f;

		public BackendDataManager(Action backendFetch, bool dataIsArray, ref Action<string> backendSuccess, ref Action<WebCall> backendFail)
		{
			// Backend method to fetch data
			mFetchMethod = backendFetch;
			mDataIsArray = dataIsArray;

			// Hook into messages from Backend
			backendSuccess += OnFetchSuccess;
			backendFail += OnFetchFail;
		}

		public void FetchData(bool force = false)
		{
			if (Time.time > mNextFetchTime || force)
			{
				mNextFetchTime = Time.time + MAX_FETCH_TIME;
				mFetchMethod();
			}
		}
		
		private void OnFetchSuccess(string json)
		{
			if (json != mPreviousJson)
			{
				mPreviousJson = json;

				if (mDataIsArray)
				{
					List<object> jsonData = MiniJSON.Json.Deserialize<List<object>>(json);
					Data = DataBaseObject.ArrayFromData<T1>(jsonData);
				}
				else
				{
					Data = new T1[1];
					Data[0] = new T1();

					Dictionary<string, object> jsonData = MiniJSON.Json.Deserialize<Dictionary<string, object>>(json);
					Data[0].SetData(jsonData);
				}

				HandleData();

				if (OnDataUpdate != null)
					OnDataUpdate();
			}
		}

		private void OnFetchFail(WebCall webCall)
		{
			if (OnDataFetchFail != null)
				OnDataFetchFail(webCall);
		}

		protected virtual void HandleData() { }
	}
}
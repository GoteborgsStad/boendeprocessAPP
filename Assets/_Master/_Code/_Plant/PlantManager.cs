using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ius
{
	public class PlantManager : MonoBehaviour 
	{
		[Header("Camera")]
		[SerializeField] private Transform mCameraAnchor;
		[SerializeField] private Camera mPlantCamera;
		[SerializeField] private float mVerticalSpeed = 3.5f;
		[SerializeField] private float mHorizontalSpeed = 180f;

		[Header("Plant Construction")]
		[SerializeField] private Transform mPlantStartPoint;
		[SerializeField] private StemSegment[] mStemPrefabs;
		[SerializeField] private Transform mStemTopPrefab;
		[SerializeField] private PlantClickable mBudPrefab;
		[SerializeField] private PlantClickable mWitheredLeafPrefab;
		[SerializeField] private PlantClickable[] mLeafPrefabs;
		[SerializeField] private PlantClickable[] mFlowerPrefabs;

		private bool mIsVisible;
		private bool mIsCameraInitialized;

		private List<PlantClickable> mParts = new List<PlantClickable>();

		private System.Random mRandom;
		private Transform mPlantRoot;
		private float mLocalPlantY;
		private StemSegment mCurrentStem;
		private int mPreviousStemPrefab;

		private bool mHasInput;
		private Vector2 mPreviousInput;
		private Vector2 mStartInput;
		private float mStartInputTime;
		private Vector2 mCameraVelocity;
		private Vector2 mCameraDecelleration;

		private float CameraBot { get; set; }
		private float CameraTop { get; set; }

		private static PlantManager Instance;

		void Awake()
		{
			Instance = this;

			SetVisible(false);
		}

		void OnEnable()
		{
			DataManager.Me.OnDataUpdate += CreatePlant;
			DataManager.Assignment.OnDataUpdate += CreatePlant;
		}

		void OnDisable()
		{
			DataManager.Me.OnDataUpdate -= CreatePlant;
			DataManager.Assignment.OnDataUpdate -= CreatePlant;
		}

		public static void SetVisible(bool isVisible)
		{
			Instance.SetState(isVisible);
		}

		private void SetState(bool isVisible)
		{
			mIsVisible = isVisible;
			mPlantCamera.enabled = mIsVisible;

			if (mIsVisible)
				CreatePlant();
			else
				Clear();
		}

		private void CreatePlant()
		{
			Clear();

			if (!mIsVisible || DataManager.Me.Data == null || DataManager.Assignment.Data == null || DataManager.Plan.Data == null)
				return;

			// Setup plant parent transform
			mPlantRoot = new GameObject("PlantRoot").transform;
			mPlantRoot.SetParent(mPlantStartPoint.parent);
			mPlantRoot.localPosition = mPlantStartPoint.localPosition;
			
			// Initialize random with the user's id
			mRandom = new System.Random(DataManager.Me.Data[0].ID);
			mPreviousStemPrefab = 0;
			mLocalPlantY = 0;
			
			// Fetch the data to build the plant from
			// Loop through all data points, call AddPoint for each
			// AddPoint function adds additional Stems when the current one is filled
			CreateStem(); // Create an empty stem at the bot of the plant
			CreateStem();

			FillPlant();

			// Add stem top
			Instantiate(mStemTopPrefab, mPlantRoot).localPosition = new Vector3(0, mLocalPlantY, 0);
			
			// Set highest camera position based on plant height
			CameraBot = 2f;
			CameraTop = Mathf.Max(CameraBot, mLocalPlantY + 1f);

			if (!mIsCameraInitialized)
			{
				mIsCameraInitialized = true;

				// Place camera at start point
				mCameraAnchor.localPosition = new Vector3(0, CameraBot, 0); // <TODO> Should probably start at specific height depending on completed assignments?
				mCameraAnchor.localEulerAngles = Vector3.zero;
			}

			MoveCamera(Vector2.zero, false);
		}

		private void FillPlant()
		{
			mParts.Clear();

			// Create a list with all assignments and goals
			List<DataGoal> entryData = new List<DataGoal>(DataManager.Assignment.Data);

			// Only show goals which are completed or which have an end date less than a week away
			TimeSpan showGoalRange = TimeSpan.FromDays(7);

			for (int i = 0; i < DataManager.Plan.Data[0].Goals.Length; i++)
			{
				DataGoal goal = DataManager.Plan.Data[0].Goals[i];

				if (goal.IsDone || goal.EndAt.Value - DateTime.Now < showGoalRange)
				{
					entryData.Add(goal);
				}
			}

			//entryData.AddRange(DataManager.Plan.Data[0].Goals);

			// Sort based on creation date for assignments and projected end dates for goals
			entryData.Sort((a, b) =>
				{
					DateTime aDate = (a is DataAssignment) ? a.CreatedAt.Value : a.EndAt.Value;
					DateTime bDate = (b is DataAssignment) ? b.CreatedAt.Value : b.EndAt.Value;
					return aDate.CompareTo(bDate);
				});

			// Create a plant component for each entry 
			for (int i = 0; i < entryData.Count; i++)
			{
				if (!mCurrentStem.HasOpenPoints)
					CreateStem();

				Transform chosenPoint = mCurrentStem.GetPoint(mRandom.Next());

				PlantClickable prefab;
				int random = mRandom.Next();

				if (entryData[i] is DataAssignment)
				{
					// Assignment
					if (entryData[i].IsPastDeadline)
						prefab = mWitheredLeafPrefab;
					else if (entryData[i].IsDone)
						prefab = mLeafPrefabs[random % mLeafPrefabs.Length];
					else
						prefab = mBudPrefab;
				}
				else 
				{
					// Goal
					prefab = mFlowerPrefabs[Mathf.Clamp(entryData[i].Category.ID - 1, 0, mFlowerPrefabs.Length)];
				}

				PlantClickable clickable = Instantiate(prefab, chosenPoint);
				Transform createdTransform = clickable.transform;
				createdTransform.localPosition = Vector3.zero;
				createdTransform.localRotation = Quaternion.Euler(0, 90f, 0);

				clickable.Initialize(entryData[i]);
				//created.localScale = Vector3.one;

				mParts.Add(clickable);
			}
		}

		private void CreateStem()
		{
			int roll = mRandom.Next(mStemPrefabs.Length - 1);

			if (roll >= mPreviousStemPrefab)
				roll++;

			mPreviousStemPrefab = roll;

			mCurrentStem = Instantiate(mStemPrefabs[roll], mPlantRoot);
			mCurrentStem.transform.localPosition = new Vector3(0, mLocalPlantY, 0);
			mCurrentStem.Initialize(mRandom);
			mLocalPlantY += mCurrentStem.Height;
		}
		
		private void Clear()
		{
			if (mPlantRoot != null)
				Destroy(mPlantRoot.gameObject);
		}

		void Update()
		{
			if (!mIsVisible)
				return;

			if (mPlantRoot != null)
				UpdateCameraControl();

			if (!UIManager.IsTransition)
				UpdateClick();

			mHasInput = HasInput();
		}

		private void UpdateCameraControl()
		{
			bool hasInput = HasInput();

			if (!mHasInput && hasInput)
			{
				// Start input, set start drag point
				mPreviousInput = GetInput();
			}

			if (mHasInput && hasInput)
			{
				// Drag
				Vector2 input = GetInput();
				MoveCamera(input - mPreviousInput, false);
				mPreviousInput = input;
			}

			if (!mHasInput && !hasInput)
			{
				// Inertia
				mCameraVelocity = Vector2.SmoothDamp(mCameraVelocity, Vector2.zero, ref mCameraDecelleration, 0.2f, 100f, Time.deltaTime);
				MoveCamera(mCameraVelocity * Time.deltaTime, true);
			}
		}

		private void UpdateClick()
		{
			bool hasInput = HasInput();

			if (hasInput && !mHasInput)
			{
				// Start input
				mStartInput = GetInput();
				mStartInputTime = Time.time;
			}

			if (!hasInput && mHasInput)
			{
				// Release input, check for click
				float timeSinceStart = Time.time - mStartInputTime;

				if (timeSinceStart < 0.5f && Vector2.Distance(mStartInput, GetInput()) < 0.1f)
				{
					PerformClick();
				}
			}
		}

		private void PerformClick()
		{
			Ray ray = mPlantCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit rayHit;

			if (Physics.Raycast(ray, out rayHit))
			{
				PlantClickable hitPart = rayHit.collider.GetComponent<PlantClickable>();

				if (hitPart != null)
				{
					HandleGoalClick(hitPart.Goal);
				}
			}
		}

		private void HandleGoalClick(DataGoal goal)
		{
			if (goal is DataAssignment)
			{
				DataAssignment assignment = goal as DataAssignment;

				if (assignment.IsSubmitted)
					ScreenInspectAssignmentDone.Open(assignment, true);
				else
					ScreenInspectAssignment.Open(assignment, true);
			}
			else
			{
				ScreenMonthlyCheck.Open(goal);
			}
		}

		private void MoveCamera(Vector2 move, bool isInertia)
		{
			if (!isInertia)
			{
				mCameraVelocity = mCameraVelocity * 0.66f;
				mCameraVelocity.x += (move.x / Time.deltaTime) * 0.34f;
				mCameraVelocity.y += (move.y / Time.deltaTime) * 0.34f;
			}

			Vector3 position = mCameraAnchor.position;
			position.y -= move.y * mVerticalSpeed;
			position.y = Mathf.Clamp(position.y, CameraBot, CameraTop);
			mCameraAnchor.position = position;

			mCameraAnchor.localEulerAngles += new Vector3(0, move.x * mHorizontalSpeed, 0);

			UpdateOutline();
		}

		private void UpdateOutline()
		{
			float y = mCameraAnchor.position.y;

			for (int i = 0; i < mParts.Count; i++)
			{
				mParts[i].UpdateOutlineState(y);
			}
		}

		private bool HasInput()
		{
			bool hasInput = Input.GetMouseButton(0);

			if (hasInput)
			{
				float yPos = Input.mousePosition.y;

				if (yPos < Screen.height * 0.1f)
				{
					// The touch input is in the bot 10% of the screen which is the navigation bar
					hasInput = false;
				}
			}

			return hasInput;
		}

		/// <summary> Returns a position in -1 to 1 range. </summary>
		private Vector2 GetInput()
		{
			return PixelsToView(Input.mousePosition);
		}

		private Vector2 PixelsToView(Vector2 screenPosition)
		{
			screenPosition.x /= Screen.width;
			screenPosition.y /= Screen.height;

			screenPosition *= 2f;
			screenPosition -= Vector2.one;
			
			return screenPosition;
		}
	}
}
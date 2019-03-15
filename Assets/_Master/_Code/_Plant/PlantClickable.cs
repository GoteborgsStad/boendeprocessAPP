using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ius
{
	[RequireComponent(typeof(Collider), typeof(MeshFilter))]
	public class PlantClickable : MonoBehaviour
	{
		[Header("Outline")]
		[SerializeField] private Renderer mOutline;
		[SerializeField] private MeshFilter mOutlineMeshFilter;

		[Header("State")]
		[SerializeField] private MeshFilter mMeshFilter;
		[SerializeField] private Mesh mNotDoneMesh;
		[SerializeField] private Mesh mDoneMesh;
		[SerializeField] private GameObject[] mDoneParticles;

		private Material[] mOutlineMaterials;
		private float mYPosition;

		private Color mOutlineColor;
		private bool mShouldShowOutline;
		private float mOutlineValue;
		private bool mShouldPulse;
		private float mPulseOffset;
		
		public DataGoal Goal { get; private set; }

		private const float OUTLINE_SPEED = 5f;
		private const float OUTLINE_WIDTH = 0.03f;
		private const float OUTLINE_PULSE_SPEED = 2f;
		private const string KEY_OUTLINE = "_Outline";

		void Reset()
		{
			if (mMeshFilter == null)
				mMeshFilter = GetComponent<MeshFilter>();
		}

		public void Initialize(DataGoal goal)
		{
			Goal = goal;

			if (mNotDoneMesh != null && mDoneMesh != null)
			{
				mMeshFilter.sharedMesh = Goal.IsDone ? mDoneMesh : mNotDoneMesh;
				mOutlineMeshFilter.sharedMesh = mMeshFilter.sharedMesh;
			}

			SetParticles(Goal.IsDone);

			mOutlineMaterials = mOutline.materials;
			mOutlineValue = 0;
			UpdateColor(0);
			mOutline.enabled = false;
			mYPosition = transform.position.y;
			mPulseOffset = UnityEngine.Random.value;
		}

		public void UpdateOutlineState(float cameraY)
		{
			mShouldShowOutline = true;
			mShouldPulse = false;
			
			bool isCentered = Mathf.Abs(mYPosition - cameraY) < 0.5f;
			bool isAssignment = Goal is DataAssignment;

			if (!Goal.IsDone && Goal.IsPastDeadline && (isCentered || isAssignment))
			{
				// Late
				mOutlineColor = ColorPalette.Negative;
				mShouldPulse = isAssignment;
			}
			else if (!Goal.IsDone && Goal.NeedAttention && (isCentered || isAssignment))
			{
				// Should soon be completed
				mOutlineColor = ColorPalette.Attention;
				mShouldPulse = isAssignment;
			}
			else if (!Goal.IsDone && isCentered)
			{
				// Component is at screen center
				mOutlineColor = ColorPalette.Neutral; // Goal.IsDone ?  : ColorPalette.Highlight;
			}
			else
			{
				mShouldShowOutline = false;
			}			
		}

		void Update()
		{
			if (mShouldShowOutline && mOutlineValue < 1f)
			{
				mOutlineValue += Time.deltaTime * OUTLINE_SPEED;
				UpdateColor(mOutlineValue);

				if (!mOutline.enabled)
					mOutline.enabled = true;
			}
			else if (!mShouldShowOutline && mOutlineValue > 0f)
			{
				mOutlineValue -= Time.deltaTime * OUTLINE_SPEED;
				UpdateColor(mOutlineValue);

				if (mOutlineValue <= 0f && mOutline.enabled)
					mOutline.enabled = false;
			}

			if (mShouldPulse)
			{
				float pulseValue = Mathf.Abs(Mathf.Sin(Time.time * OUTLINE_PULSE_SPEED + mPulseOffset));
				SetOutlineWidth(OUTLINE_WIDTH + pulseValue * OUTLINE_WIDTH);
				UpdateColor(mOutlineValue);
			}
		}

		private void UpdateColor(float alpha)
		{
			Color color = mOutlineColor;
			color.a = alpha * mOutlineColor.a;

			/*if (mShouldPulse)
				color.a *= (0.3f + 0.7f * Mathf.Abs(Mathf.Sin(Time.time * OUTLINE_PULSE_SPEED + mPulseOffset)));*/

			SetOutlineColor(color);
		}

		private void SetOutlineColor(Color color)
		{
			for (int i = 0; i < mOutlineMaterials.Length; i++)
			{
				mOutlineMaterials[i].color = color;
			}
		}

		private void SetOutlineWidth(float width)
		{
			for (int i = 0; i < mOutlineMaterials.Length; i++)
			{
				mOutlineMaterials[i].SetFloat(KEY_OUTLINE, width);
			}
		}

		private void SetParticles(bool isOn)
		{
			for (int i = 0; i < mDoneParticles.Length; i++)
			{
				mDoneParticles[i].SetActive(isOn);
			}
		}
	}
}
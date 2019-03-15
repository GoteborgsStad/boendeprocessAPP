using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class StemSegment : MonoBehaviour 
	{
		[SerializeField] private float mHeight = 1f;
		[SerializeField] private int mUsedPoints = 10;
		[SerializeField] private Transform[] mAttachPoints;

		private List<Transform> mOpenPoints;

		public float Height { get { return mHeight; } }
		public bool HasOpenPoints { get { return mOpenPoints.Count > 0; } }

		public void Initialize(System.Random random)
		{
			// Create a list with all attach points
			mOpenPoints = new List<Transform>(mAttachPoints);
			
			// Remove points until the number of wanted points is reached
			while (mOpenPoints.Count > mUsedPoints)
			{
				mOpenPoints.RemoveAt(random.Next(mOpenPoints.Count));
			}
		}

		public Transform GetPoint(int random)
		{
			if (!HasOpenPoints)
			{
				Debug.LogError("Tried to use GetPoint on a StemSegment with no open points");
				return null;
			}

			// Select a random open point and remove it from the list
			int selectedIndex = random % mOpenPoints.Count;
			Transform selected = mOpenPoints[selectedIndex];
			mOpenPoints.RemoveAt(selectedIndex);
			return selected;
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.cyan;

			for (int i = 0; i < mAttachPoints.Length; i++)
			{
				Gizmos.DrawLine(mAttachPoints[i].position, mAttachPoints[i].position + mAttachPoints[i].forward * 0.25f);
			}
		}
	}
}
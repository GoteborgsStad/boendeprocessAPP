using UnityEngine;
using System.Collections;

namespace ius
{
	[RequireComponent(typeof(RectTransform))]
	public class UIScreen : MonoBehaviour
	{
		[SerializeField] private UILocation location;
		[SerializeField] private bool gatherElementsOnClose;

		private GameObject myGameObject;
		private UISlideElement[] elements;

		private ScreenBase screenObject;

		public UILocation Location { get { return location; } }

		void Reset()
		{
			RectTransform rectTransform = GetComponent<RectTransform>();
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.sizeDelta = Vector2.zero;

			OnValidate();
		}

		void OnValidate()
		{
			if (location != UILocation.None)
			{
				string wantedName = "Screen_ " + location;

				if (name != wantedName)
					name = wantedName;
			}
			else
			{
				name = "!---> Screen: NO LOCATION";
			}
		}

		void Start()
		{
			if (myGameObject == null)
				Initialize();
		}

		private void Initialize()
		{
			myGameObject = gameObject;
			elements = GetComponentsInChildren<UISlideElement>();
			screenObject = GetComponent<ScreenBase>();

			CloseInstant();
		}
		
		/// <summary> Do not call this function, instead call UIManager.CloseInstant() </summary>
		public void CloseInstant()
		{
			myGameObject.SetActive(false);
		}

		/// <summary> Do not call this function, instead call UIManager.Open() </summary>
		public Coroutine Open()
		{
			if (myGameObject == null)
				Initialize();

			myGameObject.SetActive(true);

			if (screenObject != null)
				screenObject.RefreshUI();

			return StartCoroutine(ChangeRoutine(true));
		}

		/// <summary> Do not call this function, instead call UIManager.Open() </summary>
		public Coroutine Close()
		{
			if (gatherElementsOnClose)
				elements = GetComponentsInChildren<UISlideElement>();
			return StartCoroutine(ChangeRoutine(false));
		}

		private IEnumerator ChangeRoutine(bool moveIn)
		{
			bool needChange = true;

			// Prepare all childed elements for movement
			for (int i = 0; i < elements.Length; i++)
			{
				if (elements[i] == null)
					continue;

				if (!moveIn)
				{
					elements[i].StartMoveOut();
				}
				else if (!elements[i].IgnoreAutomaticEntry)
				{
					elements[i].StartMoveIn();
				}
			}

			// Move all childed elements until they are in the wanted position
			while (needChange)
			{
				needChange = false;

				for (int i = 0; i < elements.Length; i++)
				{
					if (elements[i] == null)
						continue;

					if (!elements[i].Move(true, !moveIn))
						needChange = true;
				}

				yield return null;
			}

			if (!moveIn)
			{
				if (screenObject != null)
					screenObject.OnScreenExited();
				myGameObject.SetActive(false);
			}
			else if (screenObject != null)
			{
				screenObject.OnScreenEntered();
			}
		}
	}
}
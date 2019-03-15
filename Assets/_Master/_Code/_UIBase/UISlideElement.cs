using UnityEngine;
using System.Collections;

namespace ius
{
	public enum MoveType
	{
		Lerp,
		Boing,
		Ease
	}

	public sealed class UISlideElement : MonoBehaviour 
	{
		[SerializeField] private float extraOutMarginal = 5;
		[SerializeField] private bool ignoreAutomaticEntry = false;
		[SerializeField] private bool ignoreAutomaticExit = false;

		[Header("Move in")]
		[SerializeField] private RectTransform.Edge enterEdge;
		[SerializeField] private MoveType inMovement = MoveType.Lerp;
		[SerializeField] private float inTime = 0.25f;
		[SerializeField] private float inDelay = 0f;
		[SerializeField] private bool randomInDelay;

		[Header("Move out")]
		[SerializeField] private RectTransform.Edge exitEdge;
		[SerializeField] private MoveType outMovement = MoveType.Lerp;
		[SerializeField] private float outTime = 0.25f;
		[SerializeField] private float outDelay = 0f;
		[SerializeField] private bool randomOutDelay;
		
		private RectTransform rectTransform;
		private Vector2 basePosition;
		private Vector2[] outPositions;

		private Vector2 currentOutPosition;

		private bool movingIn;
		private bool isDone;
		private float moveTimer;

		public bool IgnoreAutomaticEntry { get { return ignoreAutomaticEntry; } }
		public bool IgnoreAutomaticExit { get { return ignoreAutomaticEntry; } }
		public bool IsIn { get { return movingIn; } }

		public event System.Action onStartSlideIn;
		public event System.Action onStartSlideOut;
		public event System.Action onIsIn;
		public event System.Action onIsOut;

		private event System.Action onNextDone;

		void Start()
		{
			if (rectTransform != null)
				return;
			
			CalculateOutPositions();
		}

		public void CalculateOutPositions()
		{
			bool isFirstStart = rectTransform == null;

			if (isFirstStart)
			{
				rectTransform = GetComponent<RectTransform>();
				basePosition = rectTransform.anchoredPosition;
			}

			rectTransform.anchoredPosition = basePosition;

			outPositions = new Vector2[System.Enum.GetNames(typeof(RectTransform.Edge)).Length];

			// Calculate all the edge out positions
			for (int i = 0; i < outPositions.Length; i++)
			{
				outPositions[i] = CalculateOutPosition((RectTransform.Edge)i);
			}

			if (isFirstStart)
			{
				SetPositionOut();
				enabled = false;
			}
		}

		private Vector2 CalculateOutPosition(RectTransform.Edge edge)
		{
			Vector2 startPosition = rectTransform.anchoredPosition;

			#region Calculate off-screen position
			RectTransform parent = rectTransform.parent.GetComponent<RectTransform>();

			float edgePosition = 0;
			float edgeDistance = 0;
			float parentDist = 0;
			float relativeAnchorPosition = 0;

			switch (edge)
			{
				case RectTransform.Edge.Left:
					edgePosition = rectTransform.anchoredPosition.x + rectTransform.rect.xMax;
					relativeAnchorPosition = Mathf.Lerp(rectTransform.anchorMin.x, rectTransform.anchorMax.x, rectTransform.pivot.x);
					parentDist = parent.rect.width * relativeAnchorPosition;
					edgeDistance = edgePosition + extraOutMarginal + parentDist;
					rectTransform.anchoredPosition -= new Vector2(edgeDistance, 0);
					break;

				case RectTransform.Edge.Right:
					edgePosition = rectTransform.anchoredPosition.x + rectTransform.rect.xMin;
					relativeAnchorPosition = 1f - Mathf.Lerp(rectTransform.anchorMin.x, rectTransform.anchorMax.x, rectTransform.pivot.x);
					parentDist = parent.rect.width * relativeAnchorPosition;
					edgeDistance = edgePosition - extraOutMarginal - parentDist;
					rectTransform.anchoredPosition -= new Vector2(edgeDistance, 0);
					break;

				case RectTransform.Edge.Bottom:
					edgePosition = rectTransform.anchoredPosition.y + rectTransform.rect.yMax;
					relativeAnchorPosition = Mathf.Lerp(rectTransform.anchorMin.y, rectTransform.anchorMax.y, rectTransform.pivot.y);
					parentDist = parent.rect.height * relativeAnchorPosition;
					edgeDistance = edgePosition + extraOutMarginal + parentDist;
					rectTransform.anchoredPosition -= new Vector2(0, edgeDistance);
					break;

				case RectTransform.Edge.Top:
					edgePosition = rectTransform.anchoredPosition.y + rectTransform.rect.yMin;
					relativeAnchorPosition = 1f - Mathf.Lerp(rectTransform.anchorMin.y, rectTransform.anchorMax.y, rectTransform.pivot.y);
					parentDist = parent.rect.height * relativeAnchorPosition;
					edgeDistance = edgePosition - extraOutMarginal - parentDist;
					rectTransform.anchoredPosition -= new Vector2(0, edgeDistance);
					break;
			}
			
			#endregion

			Vector2 result = rectTransform.anchoredPosition;
			rectTransform.anchoredPosition = startPosition;
			return result;
		}

		void Update()
		{
			if (Move(false, false))
				enabled = false;
		}

		/// <summary> Never call this method, it's used automatically for everything that needs it. </summary>
		public bool Move(bool isScreenCall, bool isCloseCall)
		{
			if (isDone 
			|| (isScreenCall && ignoreAutomaticEntry && !isCloseCall)
			|| (isScreenCall && ignoreAutomaticExit && isCloseCall))
				return true;
			
			if (movingIn)
				moveTimer += Time.deltaTime;
			else
				moveTimer -= Time.deltaTime;

			float normalizedTime = Mathf.Clamp01(Mathf.Clamp01(moveTimer) / (movingIn ? inTime : outTime));
			rectTransform.anchoredPosition = Vector2.LerpUnclamped(currentOutPosition, basePosition, Evaluate(normalizedTime, movingIn));

			if ((movingIn && (Mathf.Approximately(normalizedTime, 1f) || normalizedTime > 1f))
			|| (!movingIn && (Mathf.Approximately(normalizedTime, 0f) || normalizedTime < 0)))
			{
				isDone = true;

				if (IsIn)
				{
					if (onIsIn != null)
						onIsIn();
				}
				else
				{
					if (onIsOut != null)
						onIsOut();
				}

				if (onNextDone != null)
				{
					onNextDone();
					onNextDone = null;
				}
			}

			return isDone;
		}

		/// <summary> Never call this method, instead call BeginMoveIn. </summary>
		public bool StartMoveIn(bool runUpdate = false)
		{
			if (rectTransform == null)
				Start();

			if (IsIn)
				return false;
			
			moveTimer = randomInDelay ? Random.Range(0, -inDelay) : -inDelay;
			currentOutPosition = outPositions[(int)enterEdge];

			movingIn = true;
			isDone = false;
			enabled = runUpdate;

			if (onStartSlideIn != null)
				onStartSlideIn();

			return true;
		}

		/// <summary> Never call this method, instead call BeginMoveOut. </summary>
		public bool StartMoveOut(bool runUpdate = false)
		{
			if (!IsIn)
				return false;
			
			moveTimer = outTime + (randomOutDelay ? Random.Range(0, outDelay) : outDelay);
			currentOutPosition = outPositions[(int)exitEdge];

			movingIn = false;
			isDone = false;
			enabled = runUpdate;

			if (onStartSlideOut != null)
				onStartSlideOut();

			return true;
		}
		
		public void SetEnterEdge(RectTransform.Edge edge)
		{
			enterEdge = edge;
		}

		public void SetExitEdge(RectTransform.Edge edge)
		{
			exitEdge = edge;
		}

		public void SetInDelay(float delay)
		{
			inDelay = delay;
		}

		public void SetOutDelay(float delay)
		{
			outDelay = delay;
		}

		public void OverrideCurrentEdge(RectTransform.Edge edge)
		{
			currentOutPosition = outPositions[(int)edge];
		}

		/// <summary> The element instantly leaves the screen. </summary>
		public void SetPositionOut()
		{
			moveTimer = 0;
			movingIn = false;
			isDone = true;
			enabled = false;

			if (rectTransform == null)
				CalculateOutPositions();

			currentOutPosition = outPositions[(int)exitEdge];
			rectTransform.anchoredPosition = currentOutPosition;
		}

		/// <summary> The element instantly enters the screen. </summary>
		public void SetPositionIn()
		{
			moveTimer = inTime;
			movingIn = true;
			isDone = true;
			enabled = false;

			if (rectTransform == null)
				CalculateOutPositions();

			rectTransform.anchoredPosition = basePosition;
		}

		/// <summary> Make the element move in on the screen. </summary>
		public void BeginMoveIn()
		{
			StartMoveIn(true);
		}

		/// <summary> Make the element move in on the screen. </summary>
		public bool BeginMoveIn(System.Action onDone)
		{
			onNextDone += onDone;
			return StartMoveIn(true);
		}

		/// <summary> Make the element move out from the screen. </summary>
		public void BeginMoveOut()
		{
			StartMoveOut(true);
		}

		/// <summary> Make the element move out from the screen. </summary>
		public bool BeginMoveOut(System.Action onDone)
		{
			onNextDone += onDone;
			return StartMoveOut(true);
		}

		private float Evaluate(float timer, bool moveIn)
		{
			// Timer goes from 1 to 0 when moving out, therefor invert input and output to avoid messed up math
			timer = moveIn ? timer : (1f - timer);
			float result = Evaluate(timer, moveIn ? inMovement : outMovement, moveIn);			
			return moveIn ? result : (1f - result);
		}

		private float Evaluate(float timer, MoveType type, bool moveIn)
		{
			switch (type)
			{
				default: return timer;
				case MoveType.Lerp: return timer;
				case MoveType.Boing: return Mathfx.Berp(0f, 1f, timer);
				case MoveType.Ease: return moveIn ? Mathfx.Sinerp(0f, 1f, timer) : Mathfx.Coserp(0f, 1f, timer);
			}
		}
	}
}
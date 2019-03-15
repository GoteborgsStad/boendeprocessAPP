using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public class ScreenBase : MonoBehaviour
	{
		/// <summary> Called when the screen is about to be opened. Fill UI data here. </summary>
		public virtual void RefreshUI() { }

		/// <summary> Called when the screen enter animation is done. </summary>
		public virtual void OnScreenEntered() { }

		/// <summary> Called when the screen exit animation is done, right before screen is disabled. </summary>
		public virtual void OnScreenExited() { }
	}
}
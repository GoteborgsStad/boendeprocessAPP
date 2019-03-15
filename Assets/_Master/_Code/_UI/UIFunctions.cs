using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ius
{
	public static class UIFunctions 
	{
		public static void UnregisterGraphic(params Graphic[] toUnregister)
		{
			for (int i = 0; i < toUnregister.Length; i++)
			{
				GraphicRegistry.UnregisterGraphicForCanvas(toUnregister[i].canvas, toUnregister[i]);
			}
		}
	}
}
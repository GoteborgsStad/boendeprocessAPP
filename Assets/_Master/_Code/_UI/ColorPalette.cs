using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public static class ColorPalette 
	{
		public static Color Attention { get; private set; }
		public static Color Highlight { get; private set; }
		public static Color Negative { get; private set; }
		public static Color Neutral { get; private set; }
		public static Color Flash { get; private set; }

		static ColorPalette()
		{
			Attention = From255(241, 135, 0);
			Highlight = From255(249, 211, 123, 150);
			Negative = From255(120, 10, 60);
			Neutral = From255(1, 230, 209, 150);
			Flash = From255(208, 240, 250);
		}

		private static Color From255(int r, int g, int b, int a = 255)
		{
			return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
		}
	}
}
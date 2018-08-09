using SkiaSharp;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Skia
{
	public static class SkiaExtensions
	{
		public static SKColor ToSKColor(this Color color)
		{
			return color.ToSKColor(Color.Black);
		}

			public static SKColor ToSKColor(this Color color, Color defaultColor)
		{
			if (color.IsDefault) color = defaultColor;
			return new SKColor((byte)(byte.MaxValue * color.R), 
				(byte)(byte.MaxValue * color.G), 
				(byte)(byte.MaxValue * color.B),
				(byte)(byte.MaxValue * color.A));
		}

		public static SKRect ToSKRect(this Rectangle rectangle)
		{
			return new SKRect((float)rectangle.Left, (float)rectangle.Top, (float)rectangle.Right, (float)rectangle.Bottom);
		}
	}
}

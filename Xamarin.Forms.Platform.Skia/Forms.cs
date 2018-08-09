using SkiaSharp;
using System;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Skia
{
	public static class Forms
	{
		public static IPlatform Platform = new Platform();

		static string currentDrawRequest;
		static Element currentElement;
		public static void Draw (Element element, Rectangle region, SKSurface surface, Action redraw)
		{
			if (!string.IsNullOrWhiteSpace(currentDrawRequest) && currentElement != element)
			{
				ImageCache.ClearCache(currentDrawRequest);
			}
			if (currentElement != element)
			{
				currentDrawRequest = Guid.NewGuid().ToString();
				currentElement = element;
			}

			var canvas = surface.Canvas;

			canvas.Clear(SKColors.White);

			element.Platform = Platform;
			foreach (var e in element.Descendants())
				if (e is VisualElement v)
					v.IsPlatformEnabled = true;
			if (element is VisualElement ve)
			{
				ve.IsPlatformEnabled = true;
				ve.Layout(region);
			}

			//Stack<Element> drawStack = new Stack<Element>();
			//drawStack.Push(element);

			DrawElement(element, canvas, currentDrawRequest, redraw);

			//while (drawStack.Count > 0)
			//{
			//	var current = drawStack.Pop();

			//	foreach (var child in current.LogicalChildren)
			//	{
			//		drawStack.Push(child);
			//	}

				
			//}
		}

		public static void GetTextLayout(string text, TextDrawingData data, bool measure, out List<LineInfo> lines)
		{
			var maxWidth = data.Rect.Width;

			var lineHeight = (float)(data.FontSize * 1.25f * data.LineHeight);

			var paint = new SKPaint
			{
				Color = data.Color.ToSKColor(Color.Black),
				IsAntialias = true,
				TextSize = (float)data.FontSize
			};

			lines = new List<LineInfo>();

			var remaining = text;
			float y = paint.TextSize + (float)data.Rect.Top;
			float x = (float)data.Rect.Left;
			while (!string.IsNullOrEmpty(remaining))
			{
				paint.BreakText(remaining, (float)maxWidth, out var measuredWidth, out var measuredText);

				if (measuredText.Length == 0)
					break;

				if (data.Wrapping == LineBreakMode.NoWrap)
				{
					lines.Add(new LineInfo(measuredText, measuredWidth, lineHeight, new SKPoint(x, y)));
					break;
				}
				else if (data.Wrapping == LineBreakMode.WordWrap)
				{
					if (measuredText.Length != remaining.Length)
					{
						for (int i = measuredText.Length - 1; i >= 0; i--)
						{
							if (char.IsWhiteSpace(measuredText[i]))
							{
								measuredText = measuredText.Substring(0, i + 1);
								break;
							}
						}
					}

					lines.Add(new LineInfo(measuredText, paint.MeasureText(measuredText), lineHeight, new SKPoint(x, y)));
				}
				else if (data.Wrapping == LineBreakMode.HeadTruncation)
				{
					throw new NotImplementedException();
				}
				else if (data.Wrapping == LineBreakMode.TailTruncation)
				{
					throw new NotImplementedException();
				}
				else if (data.Wrapping == LineBreakMode.MiddleTruncation)
				{
					throw new NotImplementedException();
				}

				remaining = remaining.Substring(measuredText.Length);

				y += lineHeight;
			}

			if (lines.Count > 0 && !measure && (data.VAlign != TextAlignment.Start || data.HAlign != TextAlignment.Start))
			{
				float vOffset = 0;
				switch (data.VAlign)
				{
					case TextAlignment.Center:
						vOffset = (float)(data.Rect.Height - (lines.Count * lineHeight)) / 2f;
						break;

					case TextAlignment.End:
						vOffset = (float)(data.Rect.Height - (lines.Count * lineHeight));
						break;
				}

				foreach (var line in lines)
				{
					float hOffset = 0;
					switch (data.HAlign)
					{
						case TextAlignment.Center:
							hOffset = (float)((data.Rect.Width - line.Width) / 2);
							break;

						case TextAlignment.End:
							hOffset = (float)(data.Rect.Width - line.Width);
							break;
					}

					line.Origin = new SKPoint(line.Origin.X + hOffset, line.Origin.Y + vOffset);
				}
			}
		}

		public static void Init()
		{
			Device.PlatformServices = new SkiaPlatformServices();
			Device.Info = new SkiaDeviceInfo();
		}

		private static async void DrawImage(Image image, SKCanvas canvas, string drawRequest, Action redraw)
		{
			SKBitmap bitmap = null;
			if (image.Source is UriImageSource uri)
			{
				var url = uri.Uri.AbsoluteUri;
				bitmap = ImageCache.TryGetValue(url);
				if (bitmap == null)
				{
					var success = await ImageCache.LoadImage(url, drawRequest);
					if (success && drawRequest == currentDrawRequest)
					{
						image.InvalidateMeasureNonVirtual(InvalidationTrigger.Undefined);
						redraw?.Invoke();
					}

					return;
				}
			} else if(image.Source is FileImageSource fileSource)
			{
				bitmap = ImageCache.FromFile(fileSource.File).bitmap;
			}
			if (bitmap != null)
			{
				var bounds = image.Bounds;
				canvas.DrawBitmap(bitmap, bounds.ToSKRect());
			}
		}

		private static void DrawButton(Button button, SKCanvas canvas)
		{
			//-----------------------------------------------------------------------------
			// Draw Group shape group
			// Shadow color for RoundRectangleStyleFill
			var RoundRectangleStyleFillShadowColor = new SKColor(0, 0, 0, 20);

			// Build shadow for RoundRectangleStyleFill
			var RoundRectangleStyleFillShadow = SKImageFilter.CreateDropShadow(0, 0, 4, 4, RoundRectangleStyleFillShadowColor, SKDropShadowImageFilterShadowMode.DrawShadowAndForeground, null, null);

			// Fill color for Round Rectangle Style
			var RoundRectangleStyleFillColor = button.BackgroundColor.ToSKColor(Color.Transparent);

			// New Round Rectangle Style fill paint
			var RoundRectangleStyleFillPaint = new SKPaint()
			{
				Style = SKPaintStyle.Fill,
				Color = RoundRectangleStyleFillColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true,
				ImageFilter = RoundRectangleStyleFillShadow
			};

			// Frame color for Round Rectangle Style
			var RoundRectangleStyleFrameColor = button.BorderColor.ToSKColor();

			// New Round Rectangle Style frame paint
			var RoundRectangleStyleFramePaint = new SKPaint()
			{
				Style = SKPaintStyle.Stroke,
				Color = RoundRectangleStyleFrameColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true,
				StrokeWidth = (float)button.BorderWidth,
			};

			float rounding = (float)button.CornerRadius;
			if (rounding < 0)
				rounding = 0;

			// Draw Round Rectangle shape
			var bounds = button.Bounds.Inflate(-4, -4);

			canvas.DrawRoundRect(bounds.ToSKRect(), rounding, rounding, RoundRectangleStyleFillPaint);
			canvas.DrawRoundRect(bounds.ToSKRect(), rounding, rounding, RoundRectangleStyleFramePaint);

			DrawText(button.Text, canvas, new TextDrawingData(button));
		}

		private static SKPath RoundedRect (Rectangle bounds, CornerRadius corners)
		{
			var b = bounds.ToSKRect();
			var topLeft = (float)corners.TopLeft;
			var topRight = (float)corners.TopRight;
			var bottomLeft = (float)corners.BottomLeft;
			var bottomRight = (float)corners.BottomRight;

			var result = new SKPath();
			result.MoveTo(b.Left + topLeft, b.Top);
			result.LineTo(b.Right - topRight, b.Top);
			result.ArcTo(b.Right, b.Top, b.Right, b.Top + topRight, topRight);
			result.ArcTo(b.Right, b.Bottom, b.Right - bottomRight, b.Bottom, bottomRight);
			result.ArcTo(b.Left, b.Bottom, b.Left, b.Bottom - bottomLeft, bottomLeft);
			result.ArcTo(b.Left, b.Top, b.Left + topLeft, b.Top, topLeft);

			return result;
		}

		private static void DrawBoxView(BoxView box, SKCanvas canvas)
		{
			var corner = box.CornerRadius;
			var paint = new SKPaint();
			paint.IsAntialias = true;
			var color = box.Color.IsDefault ? box.BackgroundColor : box.Color;
			paint.Color = color.ToSKColor(Color.Transparent);

			canvas.DrawPath(RoundedRect(box.Bounds, box.CornerRadius), paint);
		}

		private static void DrawContentPage(ContentPage page, SKCanvas canvas)
		{
			DrawVisualElement(page, canvas);
		}

		private static void DrawElement(Element element, SKCanvas canvas, string drawRequest, Action redraw)
		{
			if (element is ContentPage page)
			{
				DrawContentPage(page, canvas);
			}
			else if (element is Label label)
			{
				DrawLabel(label, canvas);
			}
			else if (element is Button button)
			{
				DrawButton(button, canvas);
			}
			else if (element is BoxView box)
			{
				DrawBoxView(box, canvas);
			}
			else if (element is Image image)
			{
				DrawImage(image, canvas, drawRequest, redraw);
			}
			else if (element is VisualElement ve)
			{
				DrawVisualElement(ve, canvas);
			}

			canvas.Save();

			if (element is VisualElement v)
				canvas.Translate((float)v.Bounds.X, (float)v.Bounds.Y);

			foreach (var child in element.LogicalChildren)
			{
				DrawElement(child, canvas, drawRequest, redraw);
			}

			canvas.Restore();
		}

		private static void DrawLabel(Label label, SKCanvas canvas)
		{
			DrawVisualElement(label, canvas);

			DrawText(label.Text, canvas, new TextDrawingData(label));
		}

		private static void DrawText(string text, SKCanvas canvas, TextDrawingData data)
		{
			canvas.Save();

			var paint = new SKPaint
			{
				Color = data.Color.ToSKColor(Color.Black),
				IsAntialias = true,
				TextSize = (float)data.FontSize
			};

			canvas.ClipRect(data.Rect.ToSKRect());

			GetTextLayout(text, data, false, out var lines);

			foreach (var line in lines)
			{
				if (!string.IsNullOrWhiteSpace(line.Text))
				{
					canvas.DrawText(line.Text, line.Origin, paint);
				}
			}

			canvas.Restore();
		}

		private static void DrawVisualElement(VisualElement ve, SKCanvas canvas)
		{
			var paint = new SKPaint();
			paint.Color = ve.BackgroundColor.ToSKColor(Color.Transparent);
			canvas.DrawRect(ve.Bounds.ToSKRect(), paint);
		}

		public class LineInfo
		{
			public LineInfo(string text, float width, float height, SKPoint origin)
			{
				Text = text;
				Width = width;
				Origin = origin;
				Height = height;
			}

			public float Height { get; set; }
			public SKPoint Origin { get; set; }
			public string Text { get; set; }
			public float Width { get; set; }
		}
	}

	public class TextDrawingData
	{
		private double _lineHeight = 1.0;

		public TextDrawingData()
		{
		}

		public TextDrawingData(Label label)
		{
			Color = label.TextColor;
			Rect = label.Bounds;
			FontSize = label.FontSize;
			Wrapping = label.LineBreakMode;
			HAlign = label.HorizontalTextAlignment;
			VAlign = label.VerticalTextAlignment;
			Attributes = label.FontAttributes;
			LineHeight = label.LineHeight;
		}

		public TextDrawingData(Button button)
		{
			Color = button.TextColor;
			Rect = button.Bounds;
			FontSize = button.FontSize;
			Wrapping = LineBreakMode.NoWrap;
			HAlign = TextAlignment.Center;
			VAlign = TextAlignment.Center;
			Attributes = button.FontAttributes;
		}

		public FontAttributes Attributes { get; set; }
		public Color Color { get; set; }
		public string FontFamily { get; set; }
		public double FontSize { get; set; }
		public TextAlignment HAlign { get; set; }

		public double LineHeight
		{
			get { return _lineHeight; }
			set
			{
				if (value < 0)
					value = 1;
				_lineHeight = value;
			}
		}

		public Rectangle Rect { get; set; }
		public TextAlignment VAlign { get; set; }
		public LineBreakMode Wrapping { get; set; }
	}
}
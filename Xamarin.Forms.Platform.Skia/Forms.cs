using SkiaSharp;
using System;
using System.Collections.Generic;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Platform.Skia
{
	public static class Forms
	{
		public static IPlatform Platform = new Platform();

		public static byte[] DrawToPng(Element element, Rectangle region, Action redraw)
		{
			var bitmap = new SKBitmap((int)region.Width, (int)region.Height);
			var canvas = new SKCanvas(bitmap);
			Draw(element, region, canvas, redraw);
			canvas.Save();
			using (var image = SKImage.FromBitmap(bitmap))
			using (var data = image.Encode(SKEncodedImageFormat.Png, 80))
			{
				return data.ToArray();
			}
		}

		static string currentDrawRequest;
		static Element currentElement;

		public static void Draw(Element element, Rectangle region, SKSurface surface, Action redraw)
		{
			Draw(element, region, surface.Canvas, redraw);
		}
		public static void Draw (Element element, Rectangle region, SKCanvas canvas, Action redraw)
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

			DrawElement(element, canvas, currentDrawRequest, redraw);
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

			string fontFamily = "System";
			if (!string.IsNullOrEmpty(data.FontFamily))
			{
				fontFamily = data.FontFamily;
			}

			paint.Typeface = SKTypeface.FromFamilyName(fontFamily, 
				data.Attributes.HasFlag(FontAttributes.Bold) ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal, 
				SKFontStyleWidth.Normal, 
				data.Attributes.HasFlag(FontAttributes.Italic) ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);

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
				canvas.Save();
				var rect = bounds.ToSKRect(false);
				canvas.ClipRect(rect);
				if(image.Aspect != Aspect.Fill) {
					var hRatio = bounds.Height/ bitmap.Height;
					var wRatio = bounds.Width / bitmap.Width;
					var newRatio = image.Aspect == Aspect.AspectFit ? Math.Min(hRatio, wRatio) : Math.Max(hRatio, wRatio);
					bounds = new Rectangle(bounds.X, bounds.Y, bitmap.Width * newRatio, bitmap.Height * newRatio);
					rect = bounds.ToSKRect(false);
				}
				canvas.DrawBitmap(bitmap, rect);
				canvas.Restore();
			}
		}

		private static void DrawListView(ListView listView, SKCanvas canvas, string drawRequest, Action redraw)
		{
			DrawVisualElement(listView, canvas);
			var template = listView.ItemTemplate;

			if (template == null)
				return;

			if (template is DataTemplateSelector)
				return;

			var cell = (Cell)template.CreateContent();

			if (cell is ViewCell vc)
			{
				var view = vc.View;
				var rowHeight = listView.RowHeight <= 0 ? 48d : listView.RowHeight;

				view.Platform = Platform;
				view.Parent = listView;
				foreach (var e in view.Descendants())
					if (e is VisualElement v)
						v.IsPlatformEnabled = true;

				if (listView.HasUnevenRows)
				{
					var request = view.Measure(listView.Width, double.PositiveInfinity);
					rowHeight = vc.Height > 0 ? vc.Height : request.Request.Height;
				}

				if (view is VisualElement ve)
				{
					ve.IsPlatformEnabled = true;
					ve.Layout(new Rectangle(0, 0, listView.Width, rowHeight));
				}

				canvas.Save();

				canvas.Translate((float)listView.Bounds.X, (float)listView.Bounds.Y);
				DrawElement(view, canvas, drawRequest, redraw);

				canvas.Restore();
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
			var bounds = button.Bounds.ToSKRect(false);
			bounds.Inflate(-2f, -2f);

			canvas.DrawRoundRect(bounds, rounding, rounding, RoundRectangleStyleFillPaint);
			canvas.DrawRoundRect(bounds, rounding, rounding, RoundRectangleStyleFramePaint);

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

			canvas.DrawPath(RoundedRect(new Rectangle(0, 0, box.Width, box.Height), box.CornerRadius), paint);
		}

		private static void DrawContentPage(ContentPage page, SKCanvas canvas)
		{
			DrawVisualElement(page, canvas);
		}

		private static void DrawEntryBackground(VisualElement element, SKCanvas canvas)
		{
			var path = RoundedRect(new Rectangle(0, 0, element.Width, element.Height).Inflate(-1, -1), new CornerRadius(3));
			var paint = new SKPaint
			{
				IsAntialias = true,
				Color = element.BackgroundColor.ToSKColor(Color.White),
			};

			canvas.DrawPath(path, paint);

			paint = new SKPaint
			{
				IsStroke = true,
				IsAntialias = true,
				Color = SKColors.Black,
				StrokeWidth = 2,
			};

			canvas.DrawPath(path, paint);
		}

		private static void DrawEditor(Editor editor, SKCanvas canvas)
		{
			DrawEntryBackground(editor, canvas);

			var data = new TextDrawingData(editor);
			data.Rect = data.Rect.Inflate(-8, -8);

			DrawText(editor.Text, canvas, data);
		}

		private static void DrawElement(Element element, SKCanvas canvas, string drawRequest, Action redraw)
		{
			if (element is VisualElement ve)
			{
				using (new CanvasTransForm(ve, canvas))
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
					else if (element is ActivityIndicator aind)
					{
						DrawActivityInidicator(aind, canvas);
					}
					else if (element is Switch theSwitch)
					{
						DrawSwitch(theSwitch, canvas);
					}
					else if (element is Stepper stepper)
					{
						DrawStepper(stepper, canvas);
					}
					else if (element is Entry entry)
					{
						DrawEntry(entry, canvas);
					}
					else if (element is DatePicker datePicker)
					{
						DrawDatePicker(datePicker, canvas);
					}
					else if (element is TimePicker timePicker)
					{
						DrawTimePicker(timePicker, canvas);
					}
					else if (element is Picker picker)
					{
						DrawPicker(picker, canvas);
					}
					else if (element is Editor editor)
					{
						DrawEditor(editor, canvas);
					}
					else if(element is Slider slider)
					{
						DrawSlider(slider, canvas);
					}
					else if (element is ProgressBar progressBar)
					{
						DrawProgressBar(progressBar, (float)progressBar.Progress, canvas);
					}
					else if (element is ListView listView)
					{
						DrawListView(listView, canvas, drawRequest, redraw);
					}
					else if(element is Layout)
					{
						DrawVisualElement(ve, canvas);
					}
					//This always goes last!
					else
					{
						DrawUnknown(ve, canvas);
					}
				}
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
		private static void DrawSwitch(Switch theSwitch, SKCanvas canvas)
		{
			bool switchState = theSwitch.IsToggled;
			// Fill color for Rectangle Style
			var RectangleStyleFillColor = switchState ? new SKColor(0, 150, 255, 255) : new SKColor(121, 121, 121, 255);

			// New Rectangle Style fill paint
			var RectangleStyleFillPaint = new SKPaint()
			{
				Style = SKPaintStyle.Fill,
				Color = RectangleStyleFillColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true
			};

			// Frame color for Rectangle Style
			var RectangleStyleFrameColor = new SKColor(0, 0, 0, 255);

			// New Rectangle Style frame paint
			var RectangleStyleFramePaint = new SKPaint()
			{
				Style = SKPaintStyle.Stroke,
				Color = RectangleStyleFrameColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true,
				StrokeWidth = 1f,
				StrokeMiter = 4f,
				StrokeJoin = SKStrokeJoin.Miter,
				StrokeCap = SKStrokeCap.Butt
			};

			// Draw Rectangle shape
			canvas.DrawRect(new SKRect(1f, 0.6445313f, 81f, 27.64453f), RectangleStyleFillPaint);
			canvas.DrawRect(new SKRect(1f, 0.6445313f, 81f, 27.64453f), RectangleStyleFramePaint);

			// Fill color for Rectangle Style
			var RectangleStyle1FillColor = new SKColor(254, 255, 255, 255);

			// New Rectangle Style fill paint
			var RectangleStyle1FillPaint = new SKPaint()
			{
				Style = SKPaintStyle.Fill,
				Color = RectangleStyle1FillColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true
			};

			// Frame color for Rectangle Style
			var RectangleStyle1FrameColor = new SKColor(0, 0, 0, 255);

			// New Rectangle Style frame paint
			var RectangleStyle1FramePaint = new SKPaint()
			{
				Style = SKPaintStyle.Stroke,
				Color = RectangleStyle1FrameColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true,
				StrokeWidth = 1f,
				StrokeMiter = 4f,
				StrokeJoin = SKStrokeJoin.Miter,
				StrokeCap = SKStrokeCap.Butt
			};

			var thumbRect = switchState ? new SKRect(53.35547f, 0.9570313f, 80.35547f, 27.95703f) : new SKRect(1.113281f, 0.9570313f, 28.11328f, 27.95703f);
			// Draw Rectangle shape
			canvas.DrawRect(thumbRect, RectangleStyle1FillPaint);
			canvas.DrawRect(thumbRect, RectangleStyle1FramePaint);

			// Fill color for Text Style
			var TextStyleFillColor = new SKColor(254, 255, 255, 255);

			// New Text Style fill paint
			var TextStyleFillPaint = new SKPaint()
			{
				Style = SKPaintStyle.Fill,
				Color = TextStyleFillColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true,
				Typeface = SKTypeface.FromFamilyName("Helvetica", SKTypefaceStyle.Normal),
				TextSize = 14f,
				TextAlign = SKTextAlign.Center,
				IsVerticalText = false,
				TextScaleX = 1f,
				TextSkewX = 0f
			};

			if(switchState)
				canvas.DrawText("On", 28.19141f, 20.24121f, TextStyleFillPaint);
			else
				canvas.DrawText("Off", 54.19141f, 20.24121f, TextStyleFillPaint);
		}
		private static void DrawSlider(Slider slider, SKCanvas canvas)
		{
			var progress = slider.Value * (slider.Maximum - slider.Minimum);

			DrawProgressBar(slider,(float)progress, canvas);


			var bounds = slider.Bounds.Inflate(-1, -1);

			const float thumbSquare = 18;
			var top = (float)(bounds.Height - thumbSquare) / 2f;
			var left = (float)((bounds.Width - thumbSquare) * progress);
			//var left =  (float)(bounds.Width *  progress)  - (thumbSquare / 2f);

			var fullRect = new SKRect(left, top, left + thumbSquare, top + thumbSquare);


			// Fill color for Thumb Style
			var ThumbStyleFillColor = new SKColor(254, 255, 255, 255);

			// New Thumb Style fill paint
			var ThumbStyleFillPaint = new SKPaint()
			{
				Style = SKPaintStyle.Fill,
				Color = ThumbStyleFillColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true
			};

			// Frame color for Thumb Style
			var ThumbStyleFrameColor = new SKColor(0, 0, 0, 255);

			// New Thumb Style frame paint
			var ThumbStyleFramePaint = new SKPaint()
			{
				Style = SKPaintStyle.Stroke,
				Color = ThumbStyleFrameColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true,
				StrokeWidth = 1f,
				StrokeMiter = 4f,
				StrokeJoin = SKStrokeJoin.Miter,
				StrokeCap = SKStrokeCap.Butt
			};

			// Draw Thumb shape
			canvas.DrawRect(fullRect, ThumbStyleFillPaint);
			canvas.DrawRect(fullRect, ThumbStyleFramePaint);
		}

		private static void DrawProgressBar(VisualElement element, float progress, SKCanvas canvas)
		{
			DrawVisualElement(element, canvas);
			var bounds = element.Bounds.Inflate(-1,-1);
			const float progressHeight = 6;
			var top = ((bounds.Height - progressHeight) / 2f);
			var bottom = top + progressHeight;
			var fullRect = new SKRect((float)bounds.Left, (float)top, (float)bounds.Width, (float)bottom);


			// Fill color for ProgressRectangle Style
			var ProgressRectangleStyleFillColor = new SKColor(230, 230, 230, 255);

			// New ProgressRectangle Style fill paint
			var ProgressRectangleStyleFillPaint = new SKPaint()
			{
				Style = SKPaintStyle.Fill,
				Color = ProgressRectangleStyleFillColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true
			};

			// Frame color for ProgressRectangle Style
			var ProgressRectangleStyleFrameColor = new SKColor(0, 0, 0, 255);

			// New ProgressRectangle Style frame paint
			var ProgressRectangleStyleFramePaint = new SKPaint()
			{
				Style = SKPaintStyle.Stroke,
				Color = ProgressRectangleStyleFrameColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true,
				StrokeWidth = 1f,
				StrokeMiter = 4f,
				StrokeJoin = SKStrokeJoin.Miter,
				StrokeCap = SKStrokeCap.Butt
			};

			// Draw ProgressRectangle shape
			canvas.DrawRect(fullRect, ProgressRectangleStyleFillPaint);
			canvas.DrawRect(fullRect, ProgressRectangleStyleFramePaint);

			// Fill color for ProgressFill Style
			var ProgressFillStyleFillColor = new SKColor(0, 150, 255, 255);

			// New ProgressFill Style fill paint
			var ProgressFillStyleFillPaint = new SKPaint()
			{
				Style = SKPaintStyle.Fill,
				Color = ProgressFillStyleFillColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true
			};

			var progressRect =  new SKRect(fullRect.Left,fullRect.Top, fullRect.Left + (fullRect.Width * progress), fullRect.Bottom);
			// Draw ProgressFill shape
			canvas.DrawRect(progressRect, ProgressFillStyleFillPaint);
			canvas.DrawRect(progressRect, ProgressRectangleStyleFramePaint);

		}

		private static void DrawEntry(Entry entry, SKCanvas canvas)
		{
			DrawEntryBackground(entry, canvas);
			var data = new TextDrawingData(entry);
			data.Rect = data.Rect.Inflate(-15, 0);

			string text = null;
			if (!string.IsNullOrEmpty(entry.Text))
			{
				if (entry.IsPassword)
				{
					for (int i = 0; i < entry.Text.Length; i++)
					{
						text += "•";
					}
				}
				else
				{
					text = entry.Text;
				}
			}
			else
				text = entry.Placeholder;

			DrawText(text, canvas, data);
		}

		private static void DrawDatePicker(DatePicker entry, SKCanvas canvas)
		{
			DrawEntryBackground(entry, canvas);
			var data = new TextDrawingData(entry);
			data.Rect = data.Rect.Inflate(-15, 0);
			string text = entry.Date.ToString(entry.Format);
			DrawText(text, canvas, data);
		}

		private static void DrawTimePicker(TimePicker entry, SKCanvas canvas)
		{
			DrawEntryBackground(entry, canvas);
			var data = new TextDrawingData(entry);
			data.Rect = data.Rect.Inflate(-15, 0);
			string text = entry.Time.ToString(entry.Format);
			DrawText(text, canvas, data);
		}

		private static void DrawPicker(Picker entry, SKCanvas canvas)
		{
			DrawEntryBackground(entry, canvas);
			var data = new TextDrawingData(entry);
			data.Rect = data.Rect.Inflate(-15, 0);
			string text = entry.SelectedItem?.ToString();
			DrawText(text, canvas, data);
		}

		private static void DrawActivityInidicator(ActivityIndicator indicator, SKCanvas canvas)
		{
			DrawVisualElement(indicator, canvas);

			var paint = new SKPaint
			{
				Color = indicator.Color.ToSKColor(Color.Black),
				IsStroke = true,
				IsAntialias = true,
				StrokeWidth = 4f
			};

			float size = (float)Math.Min(indicator.Width, indicator.Height);
			size -= 20;


			var x = (float)((indicator.Width - size) / 2);
			var y = (float)((indicator.Height - size) / 2);
			var path = new SKPath();
			path.ArcTo(
				new SKRect(
					x, 
					y, 
					x + size, 
					y + size), 0, 270, true);

			canvas.DrawPath(path, paint);
		}

		private static void DrawStepper(Stepper stepper, SKCanvas canvas)
		{
			var paint = new SKPaint
			{
				Color = stepper.BackgroundColor.ToSKColor(Color.Transparent),
				IsAntialias = true,
				IsStroke = false,
				StrokeWidth = 1f
			};

			canvas.DrawRect(0.5f, 0.5f, 80, 27, paint);

			paint.IsStroke = true;
			paint.Color = SKColors.Black;

			canvas.DrawRect(0.5f, 0.5f, 80, 27, paint);
			canvas.DrawLine(40.5f, 0.5f, 40.5f, 27.5f, paint);

			paint.StrokeWidth = 2f;

			canvas.DrawLine(15f, 14f, 25f, 14f, paint);
			canvas.DrawLine(60f, 9f, 60f, 19f, paint);
			canvas.DrawLine(55f, 14f, 65f, 14f, paint);
		}

		private static void DrawUnknown(VisualElement element, SKCanvas canvas)
		{
			// Fill color for Rectangle Style
			var RectangleStyleFillColor = new SKColor(225, 250, 250, 255);

			// New Rectangle Style fill paint
			var RectangleStyleFillPaint = new SKPaint()
			{
				Style = SKPaintStyle.Fill,
				Color = RectangleStyleFillColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true
			};

			// Frame color for Rectangle Style
			var RectangleStyleFrameColor = new SKColor(0, 0, 0, 255);

			// New Rectangle Style frame paint
			var RectangleStyleFramePaint = new SKPaint()
			{
				Style = SKPaintStyle.Stroke,
				Color = RectangleStyleFrameColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true,
				StrokeWidth = 1f,
				StrokeMiter = 4f,
				StrokeJoin = SKStrokeJoin.Miter,
				StrokeCap = SKStrokeCap.Butt
			};

			var rect = element.Bounds.ToSKRect(false);

			// Draw Rectangle shape
			canvas.DrawRect(rect, RectangleStyleFillPaint);
			canvas.DrawRect(rect, RectangleStyleFramePaint);

			//Draw background for control
			DrawVisualElement(element, canvas);

			// Fill color for Line Style
			var LineStyleFillColor = new SKColor(230, 230, 230, 255);

			// New Line Style fill paint
			var LineStyleFillPaint = new SKPaint()
			{
				Style = SKPaintStyle.Fill,
				Color = LineStyleFillColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true
			};

			// Frame color for Line Style
			var LineStyleFrameColor = new SKColor(255, 38, 0, 255);

			// New Line Style frame paint
			var LineStyleFramePaint = new SKPaint()
			{
				Style = SKPaintStyle.Stroke,
				Color = LineStyleFrameColor,
				BlendMode = SKBlendMode.SrcOver,
				IsAntialias = true,
				StrokeWidth = 1f,
				StrokeMiter = 4f,
				StrokeJoin = SKStrokeJoin.Miter,
				StrokeCap = SKStrokeCap.Butt
			};

			// Draw Line shape
			canvas.DrawLine(new SKPoint(rect.Right, rect.Bottom), new SKPoint(rect.Left, rect.Top), LineStyleFramePaint);

			// Draw Line shape
			canvas.DrawLine(new SKPoint(rect.Left, rect.Bottom), new SKPoint(rect.Right, rect.Top),LineStyleFramePaint);

			string displayName = element.GetType().Name;
			if(element is UnknownView unknown)
			{
				displayName = $"{unknown.NamespaceUri} : {unknown.ClassName}";
			}
			DrawText(displayName, canvas, new TextDrawingData
			{
				Color = Color.Black,
				HAlign = TextAlignment.Center,
				VAlign = TextAlignment.Center,
				Wrapping = LineBreakMode.WordWrap,
				FontSize = 12,
				Rect = new Rectangle(Point.Zero, element.Bounds.Size),
			});
		}

		private static void DrawLabel(Label label, SKCanvas canvas)
		{
			DrawVisualElement(label, canvas);

			DrawText(label.Text, canvas, new TextDrawingData(label));
		}

		private static void DrawText(string text, SKCanvas canvas, TextDrawingData data)
		{
			canvas.Save();

			// or:
			var emojiChar = 0x1F680;

			// ask the font manager for a font with that character
			var fontManager = SKFontManager.Default;
			var emojiTypeface = fontManager.MatchCharacter(emojiChar);

			var paint = new SKPaint
			{
				Color = data.Color.ToSKColor(Color.Black),
				IsAntialias = true,
				TextSize = (float)data.FontSize,
				Typeface = emojiTypeface
			};

			canvas.ClipRect(data.Rect.ToSKRect());

			GetTextLayout(text, data, false, out var lines);

			string fontFamily = "System";
			if (!string.IsNullOrEmpty(data.FontFamily))
			{
				fontFamily = data.FontFamily;
			}

			paint.Typeface = SKTypeface.FromFamilyName(fontFamily,
				data.Attributes.HasFlag(FontAttributes.Bold) ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
				SKFontStyleWidth.Normal,
				data.Attributes.HasFlag(FontAttributes.Italic) ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);

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
			canvas.DrawRect(ve.Bounds.ToSKRect(false), paint);
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
	public class CanvasTransForm : IDisposable
	{
		private readonly SKCanvas canvas;
		public CanvasTransForm(VisualElement element, SKCanvas canvas)
		{
			this.canvas = canvas;
			canvas.Save();
			canvas.Translate((float)element.Bounds.X, (float)element.Bounds.Y);

		}
		public void Dispose()
		{
			canvas.Restore();
		}
	}
	public class TextDrawingData
	{
		private double _lineHeight = 1.0;

		public TextDrawingData()
		{
		}

		public TextDrawingData(Entry entry)
		{
			if (!string.IsNullOrEmpty(entry.Text))
				Color = entry.TextColor.IsDefault ? Color.Black : entry.TextColor;
			else
				Color = entry.PlaceholderColor.IsDefault ? new Color(0.7) : entry.PlaceholderColor;
			Rect = new Rectangle(0, 0, entry.Width, entry.Height);
			FontSize = entry.FontSize;
			FontFamily = entry.FontFamily;
			Wrapping = LineBreakMode.NoWrap;
			HAlign = TextAlignment.Start;
			VAlign = TextAlignment.Center;
			Attributes = entry.FontAttributes;
		}

		public TextDrawingData(DatePicker picker)
		{
			Color = picker.TextColor.IsDefault ? Color.Black : picker.TextColor;
			Rect = new Rectangle(0, 0, picker.Width, picker.Height);
			FontSize = picker.FontSize;
			FontFamily = picker.FontFamily;
			Wrapping = LineBreakMode.NoWrap;
			HAlign = TextAlignment.Start;
			VAlign = TextAlignment.Center;
			Attributes = picker.FontAttributes;
		}

		public TextDrawingData(TimePicker picker)
		{
			Color = picker.TextColor.IsDefault ? Color.Black : picker.TextColor;
			Rect = new Rectangle(0, 0, picker.Width, picker.Height);
			FontSize = picker.FontSize;
			FontFamily = picker.FontFamily;
			Wrapping = LineBreakMode.NoWrap;
			HAlign = TextAlignment.Start;
			VAlign = TextAlignment.Center;
			Attributes = picker.FontAttributes;
		}

		public TextDrawingData(Picker picker)
		{
			Color = picker.TextColor.IsDefault ? Color.Black : picker.TextColor;
			Rect = new Rectangle(0, 0, picker.Width, picker.Height);
			FontSize = picker.FontSize;
			FontFamily = picker.FontFamily;
			Wrapping = LineBreakMode.NoWrap;
			HAlign = TextAlignment.Start;
			VAlign = TextAlignment.Center;
			Attributes = picker.FontAttributes;
		}

		public TextDrawingData(Editor editor)
		{
			Color = editor.TextColor;
			Rect = new Rectangle(0, 0, editor.Width, editor.Height);
			FontSize = editor.FontSize;
			FontFamily = editor.FontFamily;
			Wrapping = LineBreakMode.WordWrap;
			HAlign = TextAlignment.Start;
			VAlign = TextAlignment.Start;
			Attributes = editor.FontAttributes;
		}

		public TextDrawingData(Label label)
		{
			Color = label.TextColor;
			Rect = new Rectangle(0, 0, label.Width, label.Height);
			FontSize = label.FontSize;
			FontFamily = label.FontFamily;
			Wrapping = label.LineBreakMode;
			HAlign = label.HorizontalTextAlignment;
			VAlign = label.VerticalTextAlignment;
			Attributes = label.FontAttributes;
			LineHeight = label.LineHeight;
		}

		public TextDrawingData(Button button)
		{
			Color = button.TextColor;
			Rect = new Rectangle(0, 0, button.Width, button.Height);
			FontSize = button.FontSize;
			FontFamily = button.FontFamily;
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
using AppKit;
using SkiaSharp;
using SkiaSharp.Views.Mac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Skia;
using Xamarin.Forms.Previewer;

namespace PreviewerMac.Previewers
{
	public class SkiaPreviewer : SKCanvasView, IPreviewer
	{
		public event EventHandler Redraw;

		public SkiaPreviewer()
		{
		}

		Element currentElement;
		Rectangle rect;
		public async Task Draw(Element element, int width, int height)
		{
			currentElement = element;
			rect = new Rectangle(0, 0, width, height);
			this.SetNeedsDisplayInRect(Bounds);
		}
		public override void DrawInSurface(SKSurface surface, SKImageInfo info)
		{
			try
			{
				if (currentElement == null)
					return;
				//TODO: Draw this puppy
				var canvas = surface.Canvas;

				// get the screen density for scaling
				var scale = (float)NSScreen.MainScreen.BackingScaleFactor;
				var scaledSize = new SKSize((info.Width / scale),(info.Height / scale));

				// handle the device screen density
				canvas.Scale(scale);

				Xamarin.Forms.Platform.Skia.Forms.Draw(currentElement, rect, surface, () => this.BeginInvokeOnMainThread(()=>this.SetNeedsDisplayInRect(Bounds)));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				//Report exception
			}
		}

	}
}

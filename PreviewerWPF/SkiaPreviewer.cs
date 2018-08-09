using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Skia;
using Xamarin.Forms.Previewer;

namespace PreviewerWPF
{
	class SkiaPreviewer : SKElement, IPreviewer
	{
		public event EventHandler Redraw;

		Element currentElement;
		public async Task Draw(Element element, int width, int height)
		{
			currentElement = element;
			rect = new Rectangle(0, 0, width, height);
			InvalidateVisual();
		}
		Rectangle rect;
		protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			try
			{
				if (currentElement == null)
					return;
				//TODO: Draw this puppy
				var canvas = e.Surface.Canvas;

				// get the screen density for scaling
				var scale = (float)PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
				var scaledSize = new SKSize(e.Info.Width / scale, e.Info.Height / scale);

				// handle the device screen density
				canvas.Scale(scale);

				Forms.Draw(currentElement, rect, e.Surface, ()=> InvalidateVisual());
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
				//Report exception
			}
		}
	}
}

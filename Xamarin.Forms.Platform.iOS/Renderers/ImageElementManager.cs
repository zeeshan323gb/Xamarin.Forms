using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public static class ImageElementManager
	{
		internal static async Task<UIImage> GetImage(this ImageSource source)
		{
			IImageSourceHandler handler = null;
			UIImage uiimage = null;
			if (source != null && (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				try
				{
					uiimage = await handler.LoadImageAsync(source, scale: (float)UIScreen.MainScreen.Scale).ConfigureAwait(false);
				}
				catch (OperationCanceledException)
				{
					uiimage = null;
				}
			}

			return uiimage;
		}

		internal static async Task SetImage(IImageVisualElementRenderer renderer, IImageController imageElement, Image oldElement = null)
		{
			var Element = renderer.Element;
			var Control = renderer.GetImage();

			if (renderer.IsDisposed || Element == null || Control == null)
			{
				return;
			}

			var source = imageElement.Source;

			if (oldElement != null)
			{
				var oldSource = oldElement.Source;
				if (Equals(oldSource, source))
					return;

				if (oldSource is FileImageSource && source is FileImageSource && ((FileImageSource)oldSource).File == ((FileImageSource)source).File)
					return;

				renderer.SetImage(null);
			}

			try
			{
				imageElement.SetIsLoading(true);
				UIImage uiimage = await source.GetImage();
				if (renderer.IsDisposed)
					return;

				renderer.SetImage(uiimage);
			}
			finally
			{
				imageElement.SetIsLoading(false);
			}

			imageElement.NativeSizeChanged();
		}

		internal static async Task SetImage(IImageController imageElement, CellTableViewCell target)
		{
			var source = imageElement.Source;
			target.ImageView.Image = null;
			imageElement.SetIsLoading(true);
			UIImage uiimage = await source.GetImage().ConfigureAwait(false);
			Device.BeginInvokeOnMainThread(() =>
			{
				if (target.Cell != null)
				{
					target.ImageView.Image = uiimage;
					target.SetNeedsLayout();
				}
				else
					uiimage?.Dispose();

				imageElement.SetIsLoading(false);
			});
		}
	}
}
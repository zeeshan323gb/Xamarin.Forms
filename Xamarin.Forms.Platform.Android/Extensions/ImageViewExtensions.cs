using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Java.IO;
using AImageView = Android.Widget.ImageView;

namespace Xamarin.Forms.Platform.Android
{
	internal static class ImageViewExtensions
	{
		public static async Task UpdateBitmap(this AImageView imageView, Image newImage, Image previousImage = null)
		{
			if (Device.IsInvokeRequired)
				throw new InvalidOperationException("Image Bitmap must not be updated from background thread");

			if (previousImage != null && Equals(previousImage.Source, newImage.Source))
				return;

			((IImageController)newImage).SetIsLoading(true);

			var formsImageView = imageView as IImageRendererController;
			formsImageView?.SkipInvalidate();

			imageView.SetImageResource(global::Android.Resource.Color.Transparent);

			ImageSource source = newImage.Source;
			Bitmap bitmap = null;
			IImageSourceHandler handler;

			if (source != null && (handler = Internals.Registrar.Registered.GetHandler<IImageSourceHandler>(source.GetType())) != null)
			{
				try
				{
					// TODO hartez Some of the test cases don't throw an exception, they just return null
					// This is because the BitmapFactory methods return null if they can't decode.

					bitmap = await handler.LoadImageAsync(source, imageView.Context);
				}
				catch (TaskCanceledException)
				{
				}

				// TODO verify that when this is down here, you don't get double exceptions
				if (bitmap == null)
				{
					// Could not decode the bitmap, throw an exception to indicate 
					// the data wasn't decodable
					throw new InvalidDataException($"Could not load Bitmap from source {source}");
				}
			}

			if (newImage == null || !Equals(newImage.Source, source))
			{
				bitmap?.Dispose();
				return;
			}

			if (bitmap == null && source is FileImageSource)
				imageView.SetImageResource(ResourceManager.GetDrawableByName(((FileImageSource)source).File));
			else
				imageView.SetImageBitmap(bitmap);

			bitmap?.Dispose();

			((IImageController)newImage).SetIsLoading(false);
			((IVisualElementController)newImage).NativeSizeChanged();
		}
	}
}

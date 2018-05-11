using System;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using Xamarin.Forms.Internals;
using AImageView = Android.Widget.ImageView;

namespace Xamarin.Forms.Platform.Android
{
	internal static class ImageViewExtensions
	{
		// TODO hartez 2017/04/07 09:33:03 Review this again, not sure it's handling the transition from previousImage to 'null' newImage correctly
		public static async Task UpdateBitmap(this AImageView imageView, ImageSource newImageSource, ImageSource previousImageSource = null)
		{
			if (Equals(newImageSource, previousImageSource))
				return;

			(imageView as IImageRendererController)?.SkipInvalidate();
			imageView.SetImageResource(global::Android.Resource.Color.Transparent);

			Bitmap bitmap = null;
			Drawable drawable = null;

			IImageSourceHandler handler = null;

			if (newImageSource != null && (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(newImageSource)) != null)
			{
				if (handler is FileImageSourceHandler)
					drawable = imageView.Context.GetDrawable((FileImageSource)newImageSource);

				if (drawable == null)
				{
					try
					{
						bitmap = await handler.LoadImageAsync(newImageSource, imageView.Context);
					}
					catch (TaskCanceledException)
					{

					}
				}
			}

			if (!imageView.IsDisposed())
			{
				if (bitmap == null && drawable != null)
					imageView.SetImageDrawable(drawable);
				else
					imageView.SetImageBitmap(bitmap);
			}

			bitmap?.Dispose();
		}

		public static async Task UpdateBitmap(this AImageView imageView, IImageController newView, IImageController previousView = null)
		{
			ImageSource newImageSource = newView?.Source;
			ImageSource previousImageSource = previousView?.Source;

			if (imageView == null || imageView.IsDisposed())
				return;

			if (Device.IsInvokeRequired)
				throw new InvalidOperationException("Image Bitmap must not be updated from background thread");

			if (previousView != null && Equals(previousImageSource, newImageSource))
				return;

			try
			{
				newView?.SetIsLoading(true);
				await UpdateBitmap(imageView, newView.Source, previousView?.Source);
			}
			finally
			{
				newView?.SetIsLoading(false);
			}

			(newView as IVisualElementController)?.NativeSizeChanged();
		}
	}
}

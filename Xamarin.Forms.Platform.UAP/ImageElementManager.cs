using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.UWP
{
	public class ImageElementManager
	{
		static internal void RefreshImage(IImageController element)
		{
			element?.InvalidateMeasure(InvalidationTrigger.RendererReady);
		}


		public static async Task<Windows.UI.Xaml.Media.ImageSource> GetImage(IImageController controller)
		{
			Windows.UI.Xaml.Media.ImageSource imagesource = null;
			try
			{
				controller.SetIsLoading(true);
				ImageSource source = controller.Source;
				IImageSourceHandler handler;
				if (source != null && (handler = Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
				{
					try
					{
						imagesource = await handler.LoadImageAsync(source);
					}
					catch (OperationCanceledException)
					{
						imagesource = null;
					}
				}
			}
			finally
			{
				controller.SetIsLoading(false);
			}


			return imagesource;
		}

		public static async Task UpdateSource(IImageVisualElementRenderer renderer)
		{
			var Element = renderer.Element;
			var controller = Element as IImageController;

			if (renderer.IsDisposed || Element == null || renderer.GetNativeElement() == null)
			{
				return;
			}

			Windows.UI.Xaml.Media.ImageSource imagesource = await GetImage(controller);
			renderer.SetImage(imagesource);
			RefreshImage(controller);
		}
	}
}

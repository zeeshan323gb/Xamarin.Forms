using System;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IShellPageRendererTracker : IDisposable
	{
		bool IsRootPage { get; set; }
		IVisualElementRenderer Renderer { get; set; }
	}
}
using System;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellFlyoutContentRenderer : IDisposable
	{
		event EventHandler<ElementSelectedEventArgs> ElementSelected;

		AView AndroidView { get; }
	}
}
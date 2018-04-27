using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellFlyoutContentRenderer
	{
		AView AndroidView { get; }
	}
}
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellItemRenderer
	{
		AView AndroidView { get; }
	}
}
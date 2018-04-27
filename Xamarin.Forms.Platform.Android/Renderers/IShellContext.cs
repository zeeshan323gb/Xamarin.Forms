using Android.Content;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellContext
	{
		Shell Shell { get; }

		Context AndroidContext { get; }

		IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer();
	}
}
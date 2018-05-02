using Android.Content;
using Android.Support.V4.Widget;

namespace Xamarin.Forms.Platform.Android
{

	public interface IShellContext
	{
		Shell Shell { get; }

		Context AndroidContext { get; }

		DrawerLayout CurrentDrawerLayout { get; }

		IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer();

		IShellItemRenderer CreateShellItemRenderer();
	}
}
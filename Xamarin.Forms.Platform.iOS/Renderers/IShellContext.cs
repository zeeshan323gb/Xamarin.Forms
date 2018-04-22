using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IShellContext
	{
		IShellItemRenderer CurrentShellItemRenderer { get; }

		IShellTabItemRenderer CreateShellTabItemRenderer(ShellTabItem tabItem);

		IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer();

		UIViewController ViewController { get; }

		Shell Shell { get; }
	}
}
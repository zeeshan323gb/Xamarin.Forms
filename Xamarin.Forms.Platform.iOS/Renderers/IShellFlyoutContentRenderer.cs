using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{

	public interface IShellFlyoutContentRenderer
	{
		event EventHandler<ElementSelectedEventArgs> ElementSelected;
		UIViewController ViewController { get; }
	}
}
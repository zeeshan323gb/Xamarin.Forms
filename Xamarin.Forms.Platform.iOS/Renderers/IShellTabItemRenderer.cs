using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IShellTabItemRenderer : IDisposable
	{
		ShellTabItem ShellTabItem { get; set; }

		Page Page { get; }

		UIViewController ViewController { get; }

		void SetTintColors(UIColor foreground, UIColor background);

		void ResetTintColors();
	}
}
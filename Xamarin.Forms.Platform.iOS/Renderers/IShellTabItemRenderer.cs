using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IShellTabItemRenderer : IDisposable
	{
		Page Page { get; }
		ShellTabItem ShellTabItem { get; set; }
		UIViewController ViewController { get; }

		void ResetTintColors();

		void SetTintColors(UIColor foreground, UIColor background);
	}
}
using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IShellTabItemRenderer : IDisposable
	{
		bool IsInMoreTab { get; set; }
		Page Page { get; }
		ShellTabItem ShellTabItem { get; set; }
		UIViewController ViewController { get; }
	}
}
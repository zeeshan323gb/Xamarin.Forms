using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IShellContentRenderer : IDisposable
	{
		bool IsInMoreTab { get; set; }
		Page Page { get; }
		ShellContent ShellContent { get; set; }
		UIViewController ViewController { get; }
	}
}
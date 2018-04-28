using System;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellItemRenderer : IDisposable
	{
		AView AndroidView { get; }

		ShellItem ShellItem { get; set; }
	}
}
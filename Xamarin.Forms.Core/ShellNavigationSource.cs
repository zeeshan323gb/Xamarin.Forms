using System;

namespace Xamarin.Forms
{
	[Flags]
	public enum ShellNavigationSource
	{
		None = 0,
		ShellItemChanged = 1 << 0,
		ShellTabItemChanged = 1 << 1,
		PushEvent = 1 << 2,
		PopEvent = 1 << 3,
		PopToRootEvent = 1 << 4,
	}
}
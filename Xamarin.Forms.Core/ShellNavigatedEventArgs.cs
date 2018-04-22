using System;

namespace Xamarin.Forms
{
	public class ShellNavigatedEventArgs : EventArgs
	{
		public ShellNavigationState Previous { get; }

		public ShellNavigationState Current { get; }

		public ShellNavigationSource Source { get; }
	}
}
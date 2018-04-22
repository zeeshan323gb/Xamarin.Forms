using System;

namespace Xamarin.Forms
{
	public class ShellNavigatingEventArgs : EventArgs
	{
		public ShellNavigationState Current { get; }

		public ShellNavigationState Target { get; }

		public ShellNavigationSource Source { get; }

		public bool CanCancel { get; }

		public bool Cancel() { return false; }
	}
}
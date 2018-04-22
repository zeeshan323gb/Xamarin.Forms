using System;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IShellFlyoutRenderer : IDisposable
	{
		void CloseFlyout();

		void AttachFlyout(IShellContext context);

		void PerformLayout();
	}
}
using System;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IShellFlyoutRenderer : IDisposable
	{
		void AttachFlyout(IShellContext context);

		void CloseFlyout();

		void PerformLayout();
	}
}
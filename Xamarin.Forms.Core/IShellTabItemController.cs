using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public interface IShellTabItemController : IElementController
	{
		event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

		Page GetOrCreateContent();

		void SendPopped();
	}
}
using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public interface IShellTabItemController : IElementController
	{
		Page GetOrCreateContent();

		event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

		void SendPopped();
	}
}
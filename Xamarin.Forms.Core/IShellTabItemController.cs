using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public interface IShellTabItemController : IElementController
	{
		Page RootPageProjection { get; set; }

		Page CurrentPage { get; }

		event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

		Page GetOrCreateContent();

		void SendPopped();
	}
}
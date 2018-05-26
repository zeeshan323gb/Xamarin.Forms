using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public interface IShellContentController : IElementController
	{
		Page RootPage { get; }

		Page CurrentPage { get; }

		event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

		Page GetOrCreateContent();

		void RecyclePage(Page page);

		void SendPopped();
	}
}
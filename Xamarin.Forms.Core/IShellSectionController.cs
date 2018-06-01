using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public interface IShellSectionController : IElementController
	{
		event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

		Page PresentedPage { get; }

		void AddContentInsetObserver(IShellContentInsetObserver observer);

		Task GoToPart(List<string> parts, Dictionary<string, string> queryData);

		bool RemoveContentInsetObserver(IShellContentInsetObserver observer);

		void SendInsetChanged(Thickness inset, double tabThickness);

		void SendPopped();

		void UpdateChecked();
	}
}
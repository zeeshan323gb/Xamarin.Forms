using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public interface IShellSectionController : IElementController
	{
		void UpdateChecked();

		Page PresentedPage { get; }

		event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

		void SendPopped();

		Task GoToPart(List<string> parts, Dictionary<string, string> queryData);
	}
}
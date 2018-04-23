using System;
using System.Collections.Generic;

namespace Xamarin.Forms
{
	public interface IShellController : IPageController
	{
		event EventHandler HeaderChanged;

		View FlyoutHeader { get; }

		void UpdateCurrentState(ShellNavigationSource source);

		bool ProposeNavigation(ShellNavigationSource source, ShellItem item, ShellTabItem tab, IList<Page> stack, bool canCancel);

		ShellNavigationState GetNavigationState(ShellItem item, ShellTabItem tab);
	}
}
using System;
using System.Collections.Generic;

namespace Xamarin.Forms
{
	public interface IAppearanceObserver
	{
		void OnAppearanceChanged(ShellAppearance appearance);
	}

	public interface IShellController : IPageController
	{
		event EventHandler HeaderChanged;

		event EventHandler StructureChanged;

		View FlyoutHeader { get; }

		void UpdateCurrentState(ShellNavigationSource source);

		bool ProposeNavigation(ShellNavigationSource source, ShellItem item, ShellTabItem tab, IList<Page> stack, bool canCancel);

		ShellNavigationState GetNavigationState(ShellItem item, ShellTabItem tab);

		void AddAppearanceObserver(IAppearanceObserver observer, Element pivot);

		bool RemoveAppearanceObserver(IAppearanceObserver observer);
	}
}
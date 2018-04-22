using System;

namespace Xamarin.Forms
{
	public interface IShellController : IPageController
	{
		event EventHandler HeaderChanged;

		View FlyoutHeader { get; }
	}
}
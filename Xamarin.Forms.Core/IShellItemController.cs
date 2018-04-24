using System;

namespace Xamarin.Forms
{
	public interface IShellItemController : IElementController
	{
		event EventHandler CurrentShellAppearanceChanged;

		ShellAppearance CurrentShellAppearance { get; set; }

		void CurrentItemNavigationChanged();
	}
}
using System;

namespace Xamarin.Forms
{
	public interface IShellItemController : IElementController
	{
		event EventHandler CurrentShellAppearanceChanged;

		event EventHandler StructureChanged;

		ShellAppearance CurrentShellAppearance { get; set; }

		void CurrentItemNavigationChanged();
	}
}
using System;

namespace Xamarin.Forms
{
	public interface IShellItemController : IElementController
	{
		void UpdateChecked();

		event EventHandler StructureChanged;
	}
}
using System;

namespace Xamarin.Forms
{
	public interface IShellItemController : IElementController
	{
		event EventHandler StructureChanged;
	}
}
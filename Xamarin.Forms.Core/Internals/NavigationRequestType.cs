using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public enum NavigationRequestType
	{
		Unknown,
		Push,
		Pop,
		PopToRoot,
		Insert,
		Remove,
	}
}
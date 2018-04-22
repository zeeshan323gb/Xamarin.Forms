using System.Threading.Tasks;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IShellItemTransition
	{
		Task Transition(IShellItemRenderer oldController, IShellItemRenderer newController);
	}
}
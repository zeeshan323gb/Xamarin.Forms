using System.Windows.Input;

namespace Xamarin.Forms
{
	public class BackButtonBehavior : BindableObject
	{
		public ImageSource IconOverride { get; set; }
		public string TextOverride { get; set; }
		public ICommand Command { get; set; }
		public object CommandParameter { get; set; }
	}
}
using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public sealed partial class FlipViewControl : UserControl
	{
		public FlipViewControl()
		{
			this.InitializeComponent();
		}
	}
}

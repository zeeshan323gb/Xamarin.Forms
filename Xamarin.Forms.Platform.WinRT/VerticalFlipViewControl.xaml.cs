using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public sealed partial class VerticalFlipViewControl : UserControl
	{
		public VerticalFlipViewControl()
		{
			this.InitializeComponent();
		}
	}
}

using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
	internal interface IToolBarForegroundBinder : IToolbarProvider
	{
		void BindForegroundColor(AppBar appBar);
		void BindForegroundColor(AppBarButton button);
	}
}
using Android.Content;
using Android.Support.V4.Widget;
using Android.Views;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellFlyoutRenderer : DrawerLayout, IShellFlyoutRenderer, DrawerLayout.IDrawerListener
	{
		#region IShellFlyoutRenderer

		AView IShellFlyoutRenderer.AndroidView => this;

		void IShellFlyoutRenderer.AttachFlyout(IShellContext context, AView content)
		{
			AttachFlyout(context, content);
		}

		void IShellFlyoutRenderer.CloseFlyout()
		{
			
		}

		#endregion IShellFlyoutRenderer

		#region IDrawerListener
		void IDrawerListener.OnDrawerClosed(AView drawerView)
		{
		}

		void IDrawerListener.OnDrawerOpened(AView drawerView)
		{
		}

		void IDrawerListener.OnDrawerSlide(AView drawerView, float slideOffset)
		{
		}

		void IDrawerListener.OnDrawerStateChanged(int newState)
		{
		}
		#endregion IDrawerListener

		private readonly IShellContext _shellContext;

		private AView _content;
		private IShellFlyoutContentRenderer _flyoutContent;

		public ShellFlyoutRenderer(IShellContext shellContext, Context context) : base (context)
		{
			_shellContext = shellContext;

			SetBackgroundColor(Color.Red.ToAndroid());
		
		}

		protected virtual void AttachFlyout(IShellContext context, AView content)
		{
			_content = content;

			_flyoutContent = context.CreateShellFlyoutContentRenderer();
			_flyoutContent.AndroidView.LayoutParameters =
				new LayoutParams(LP.MatchParent, LP.MatchParent) { Gravity = (int)GravityFlags.Start };

			content.SetBackgroundColor(Color.Gray.ToAndroid());

			AddView(content);
			AddView(_flyoutContent.AndroidView);

			AddDrawerListener(this);
		}
	}
}
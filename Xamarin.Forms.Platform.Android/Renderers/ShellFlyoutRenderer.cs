using Android.Content;
using Android.Support.V4.Widget;
using Android.Views;
using AView = Android.Views.View;

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

		private IShellFlyoutContentRenderer _flyoutContent;

		public ShellFlyoutRenderer(IShellContext shellContext, Context context) : base (context)
		{
			_shellContext = shellContext;

			SetBackgroundColor(Color.Red.ToAndroid());
		
		}

		protected virtual void AttachFlyout(IShellContext context, AView content)
		{
			_flyoutContent = context.CreateShellFlyoutContentRenderer();
			_flyoutContent.AndroidView.LayoutParameters =
				new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent) { Gravity = (int)GravityFlags.Start };

			content.SetBackgroundColor(Color.Gray.ToAndroid());

			AddView(content, new ViewGroup.LayoutParams (ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent));
			AddView(_flyoutContent.AndroidView);

			AddDrawerListener(this);

			OpenDrawer(_flyoutContent.AndroidView);
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);
		}
	}
}
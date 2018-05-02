using Android.Content;
using Android.Support.V4.Widget;
using Android.Views;
using System.ComponentModel;
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
			CloseDrawers();
		}

		#endregion IShellFlyoutRenderer

		#region IDrawerListener
		void IDrawerListener.OnDrawerClosed(AView drawerView)
		{
			Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, false);
		}

		void IDrawerListener.OnDrawerOpened(AView drawerView)
		{
			Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, true);
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

			Shell.PropertyChanged += OnShellPropertyChanged;
		}

		private Shell Shell => _shellContext.Shell;

		protected virtual void OnShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.FlyoutIsPresentedProperty.PropertyName)
			{
				var presented = Shell.FlyoutIsPresented;
				if (presented)
					OpenDrawer(_flyoutContent.AndroidView, true);
				else
					CloseDrawers();
			}
		}

		protected virtual void AttachFlyout(IShellContext context, AView content)
		{
			_content = content;

			_flyoutContent = context.CreateShellFlyoutContentRenderer();
			_flyoutContent.AndroidView.LayoutParameters =
				new LayoutParams(LP.MatchParent, LP.MatchParent) { Gravity = (int)GravityFlags.Start };

			AddView(content);
			AddView(_flyoutContent.AndroidView);

			AddDrawerListener(this);
		}
	}
}
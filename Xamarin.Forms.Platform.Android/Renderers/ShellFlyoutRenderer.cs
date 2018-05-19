using Android.Content;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Views;
using System;
using System.ComponentModel;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellFlyoutRenderer : DrawerLayout, IShellFlyoutRenderer, DrawerLayout.IDrawerListener, IFlyoutBehaviorObserver
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

		#region IFlyoutBehaviorObserver

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			switch (behavior)
			{
				case FlyoutBehavior.Disabled:
					CloseDrawers();
					SetDrawerLockMode(LockModeLockedClosed);
					break;
				case FlyoutBehavior.Flyout:
					SetDrawerLockMode(LockModeUnlocked);
					break;
				case FlyoutBehavior.Locked:
					SetDrawerLockMode(LockModeLockedOpen);
					break;
			}
		}

		#endregion IFlyoutBehaviorObserver

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

			// Depending on what you read the right edge of the drawer should be 56dp
			// from the right edge of the screen. Fine. Well except that doesn't account
			// for landscape devices, in which case its still, according to design
			// documents from google 56dp, except google doesn't do that with their own apps.
			// So we are just going to go ahead and do what google does here even though
			// this isn't what DrawerLayout does by default.

			var metrics = Context.Resources.DisplayMetrics;
			var width = Math.Min(metrics.WidthPixels, metrics.HeightPixels);

			var tv = new TypedValue();
			var actionBarHeight = (int)Context.ToPixels(56);
			if (Context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ActionBarSize, tv, true))
			{
				actionBarHeight = TypedValue.ComplexToDimensionPixelSize(tv.Data, metrics);
			}
			width -= actionBarHeight;

			_flyoutContent.AndroidView.LayoutParameters =
				new LayoutParams(width, LP.MatchParent) { Gravity = (int)GravityFlags.Start };

			AddView(content);
			AddView(_flyoutContent.AndroidView);

			AddDrawerListener(this);

			((IShellController)context.Shell).AddFlyoutBehaviorObserver(this);
		}
	}
}
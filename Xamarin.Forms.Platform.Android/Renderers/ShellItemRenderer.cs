using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Xamarin.Forms.Platform.Android.AppCompat;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Fragment = Android.Support.V4.App.Fragment;
using ActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;

namespace Xamarin.Forms.Platform.Android
{

	public class ShellItemRenderer : Fragment, IShellItemRenderer, ViewPager.IOnPageChangeListener, AView.IOnClickListener
	{
		#region IShellItemRenderer

		Fragment IShellItemRenderer.Fragment => this;

		ShellItem IShellItemRenderer.ShellItem
		{
			get { return _shellItem; }
			set
			{
				_shellItem = value;

				_shellItem.PropertyChanged += OnShellItemPropertyChanged;
			}
		}

		#endregion IShellItemRenderer

		#region IOnPageChangeListener

		void ViewPager.IOnPageChangeListener.OnPageScrollStateChanged(int state)
		{
		}

		void ViewPager.IOnPageChangeListener.OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
		}

		void ViewPager.IOnPageChangeListener.OnPageSelected(int position)
		{
		}

		#endregion IOnPageChangeListener

		#region IOnClickListener

		void AView.IOnClickListener.OnClick(AView v)
		{
		}

		#endregion IOnClickListener

		private Toolbar _toolbar;
		private TabLayout _tablayout;
		private AView _rootView = null;
		private ShellItem _shellItem;
		private ViewPager _viewPager;
		private IShellContext _shellContext;
		private Context _androidContext;
		private ActionBarDrawerToggle _drawerToggle;

		public ShellItemRenderer(IShellContext shellContext, Context androidContext)
		{
			_shellContext = shellContext;
			_androidContext = androidContext;
		}

		protected ShellItemRenderer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var shellItem = ((IShellItemRenderer)this).ShellItem;
			if (shellItem == null)
				return null;

			var inflator = LayoutInflater.From(_androidContext);
			var root = inflator.Inflate(Resource.Layout.RootLayout, null).JavaCast<CoordinatorLayout>();

			_toolbar = root.FindViewById<Toolbar>(Resource.Id.main_toolbar);
			var scrollview = root.FindViewById<NestedScrollView>(Resource.Id.main_scrollview);
			_tablayout = root.FindViewById<TabLayout>(Resource.Id.main_tablayout);

			_viewPager = new FormsViewPager(_androidContext)
			{
				LayoutParameters = new LP(LP.MatchParent, LP.MatchParent),
			};

			_viewPager.AddOnPageChangeListener(this);
			_viewPager.Id = Platform.GenerateViewId();

			var adapter = new ShellFragmentPagerAdapter(shellItem, ChildFragmentManager);

			_viewPager.Adapter = adapter;
			_viewPager.OverScrollMode = OverScrollMode.Never;

			_tablayout.SetupWithViewPager(_viewPager);

			// this is the wrong title, this should be the CurrentItem.CurrentPage title
			_toolbar.Title = shellItem.Title;

			var currentIndex = _shellItem.Items.IndexOf(_shellItem.CurrentItem);
			_viewPager.CurrentItem = currentIndex;

			scrollview.FillViewport = true;
			scrollview.AddView(_viewPager);

			SetupDrawerButton();

			return _rootView = root;
		}

		private void SetupDrawerButton ()
		{
			_drawerToggle = new ActionBarDrawerToggle((Activity)_androidContext, 
				_shellContext.CurrentDrawerLayout, _toolbar, 
				global::Android.Resource.String.Ok, 
				global::Android.Resource.String.Ok)
			{
				ToolbarNavigationClickListener = this
			};

			_drawerToggle.DrawerIndicatorEnabled = true;
			_shellContext.CurrentDrawerLayout.AddDrawerListener(_drawerToggle);

			_drawerToggle.SyncState();
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();

			if (_shellItem != null)
			{
				_viewPager.RemoveOnPageChangeListener(this);
				_shellItem.PropertyChanged -= OnShellItemPropertyChanged;

				_rootView.Dispose();
			}

			if (_drawerToggle != null)
			{
				_drawerToggle.Dispose();
				_drawerToggle = null;
			}

			_toolbar = null;
			_tablayout = null;
			_rootView = null;
			_shellItem = null;
			_viewPager = null;
			_shellContext = null;
			_androidContext = null;
		}

		protected virtual void OnShellItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_rootView == null)
				return;

			if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
			{
				var newIndex = _shellItem.Items.IndexOf(_shellItem.CurrentItem);

				if (newIndex >= 0)
				{
					_viewPager.CurrentItem = newIndex;
				}
			}
		}
	}
}
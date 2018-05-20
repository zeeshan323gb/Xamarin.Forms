using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms.Platform.Android.AppCompat;
using AView = Android.Views.View;
using Fragment = Android.Support.V4.App.Fragment;
using LP = Android.Views.ViewGroup.LayoutParams;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellTopTabFragment : Fragment, ViewPager.IOnPageChangeListener, AView.IOnClickListener, IShellObservableFragment, IAppearanceObserver
	{
		#region IOnPageChangeListener

		void ViewPager.IOnPageChangeListener.OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
		}

		void ViewPager.IOnPageChangeListener.OnPageScrollStateChanged(int state)
		{
		}

		void ViewPager.IOnPageChangeListener.OnPageSelected(int position)
		{
			// TODO : Find a way to make this cancellable
			var shellitem = ShellItem;
			var tab = shellitem.Items[position];
			var stack = tab.Stack.ToList();
			ShellController.ProposeNavigation(ShellNavigationSource.ShellTabItemChanged, shellitem, tab, stack, false);

			ShellItem.SetValueFromRenderer(ShellItem.CurrentItemProperty, tab);

			_toolbarTracker.Page = ((IShellTabItemController)ShellItem.CurrentItem).CurrentPage;
		}

		#endregion IOnPageChangeListener

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
				ResetAppearance();
			else
				SetAppearance(appearance);
		}

		#endregion IAppearanceObserver

		#region IOnClickListener

		void AView.IOnClickListener.OnClick(AView v)
		{
		}

		#endregion IOnClickListener

		private readonly IShellContext _shellContext;
		private AView _rootView;
		private TabLayout _tablayout;
		private IShellTabLayoutAppearanceTracker _tabLayoutAppearanceTracker;
		private Toolbar _toolbar;
		private IShellToolbarAppearanceTracker _toolbarAppearanceTracker;
		private IShellToolbarTracker _toolbarTracker;
		private ViewPager _viewPager;

		public ShellTopTabFragment(IShellContext shellContext)
		{
			_shellContext = shellContext;
		}

		protected ShellTopTabFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public event EventHandler AnimationFinished;

		Fragment IShellObservableFragment.Fragment => this;
		public ShellItem ShellItem { get; set; }

		private IShellController ShellController => _shellContext.Shell;

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var shellItem = ShellItem;
			if (shellItem == null)
				return null;

			var root = inflater.Inflate(Resource.Layout.RootLayout, null).JavaCast<CoordinatorLayout>();

			_toolbar = root.FindViewById<Toolbar>(Resource.Id.main_toolbar);
			var scrollview = root.FindViewById<NestedScrollView>(Resource.Id.main_scrollview);
			_tablayout = root.FindViewById<TabLayout>(Resource.Id.main_tablayout);

			_viewPager = new FormsViewPager(Context)
			{
				LayoutParameters = new LP(LP.MatchParent, LP.MatchParent),
			};

			_viewPager.AddOnPageChangeListener(this);
			_viewPager.Id = Platform.GenerateViewId();

			_viewPager.Adapter = new ShellFragmentPagerAdapter(shellItem, ChildFragmentManager);
			_viewPager.OverScrollMode = OverScrollMode.Never;

			_tablayout.SetupWithViewPager(_viewPager);

			var currentPage = ((IShellTabItemController)shellItem.CurrentItem).GetOrCreateContent();
			var currentIndex = ShellItem.Items.IndexOf(ShellItem.CurrentItem);

			_toolbarTracker = _shellContext.CreateTrackerForToolbar(_toolbar);
			_toolbarTracker.Page = currentPage;

			_viewPager.CurrentItem = currentIndex;
			scrollview.AddView(_viewPager);

			if (shellItem.Items.Count == 1)
			{
				_tablayout.Visibility = ViewStates.Gone;
			}

			_tabLayoutAppearanceTracker = _shellContext.CreateTabLayoutAppearanceTracker(ShellItem);
			_toolbarAppearanceTracker = _shellContext.CreateToolbarAppearanceTracker();

			HookEvents();

			return _rootView = root;
		}

		// Use OnDestroy instead of OnDestroyView because OnDestroyView will be
		// called before the animation completes. This causes tons of tiny issues.
		public override void OnDestroy()
		{
			base.OnDestroy();

			if (_rootView != null)
			{
				UnhookEvents();
				_toolbarAppearanceTracker.Dispose();
				_tabLayoutAppearanceTracker.Dispose();
				_viewPager.RemoveOnPageChangeListener(this);
				_rootView.Dispose();
				_toolbarTracker.Dispose();
			}

			_toolbarAppearanceTracker = null;
			_tabLayoutAppearanceTracker = null;
			_toolbarTracker = null;
			_toolbar = null;
			_tablayout = null;
			_rootView = null;
			_viewPager = null;
		}

		protected virtual void OnAnimationFinished(EventArgs e)
		{
			AnimationFinished?.Invoke(this, e);
		}

		protected virtual void OnItemsCollectionChagned(object sender, NotifyCollectionChangedEventArgs e) =>
			_tablayout.Visibility = (ShellItem.Items.Count > 1) ? ViewStates.Visible : ViewStates.Gone;

		protected virtual void OnShellItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_rootView == null)
				return;

			if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
			{
				var newIndex = ShellItem.Items.IndexOf(ShellItem.CurrentItem);

				if (newIndex >= 0)
				{
					_viewPager.CurrentItem = newIndex;
				}
			}
		}

		protected virtual void ResetAppearance()
		{
			_toolbarAppearanceTracker.ResetAppearance(_toolbar, _toolbarTracker);
			_tabLayoutAppearanceTracker.ResetAppearance(_tablayout);
		}

		protected virtual void SetAppearance(ShellAppearance appearance)
		{
			_toolbarAppearanceTracker.SetAppearance(_toolbar, _toolbarTracker, appearance);
			_tabLayoutAppearanceTracker.SetAppearance(_tablayout, appearance);
		}

		private void HookEvents()
		{
			((INotifyCollectionChanged)ShellItem.Items).CollectionChanged += OnItemsCollectionChagned;
			((IShellController)_shellContext.Shell).AddAppearanceObserver(this, ShellItem);
			ShellItem.PropertyChanged += OnShellItemPropertyChanged;
		}

		private void UnhookEvents()
		{
			((INotifyCollectionChanged)ShellItem.Items).CollectionChanged -= OnItemsCollectionChagned;
			((IShellController)_shellContext.Shell).RemoveAppearanceObserver(this);
			ShellItem.PropertyChanged -= OnShellItemPropertyChanged;
		}
	}
}
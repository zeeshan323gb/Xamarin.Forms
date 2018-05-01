using System;
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

namespace Xamarin.Forms.Platform.Android
{

	public class ShellItemRenderer : Fragment, IShellItemRenderer, ViewPager.IOnPageChangeListener
	{
		#region IShellItemRenderer

		Fragment IShellItemRenderer.Fragment => this;

		ShellItem IShellItemRenderer.ShellItem { get; set; }

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

		private Toolbar _toolbar;
		private TabLayout _tablayout;
		private AView _rootView = null;
		private readonly IShellContext _shellContext;
		private readonly Context _androidContext;

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

			var viewPager = new FormsViewPager(_androidContext)
			{
				LayoutParameters = new LP(LP.MatchParent, LP.MatchParent),
			};

			viewPager.AddOnPageChangeListener(this);
			viewPager.Id = Platform.GenerateViewId();

			var adapter = new ShellFragmentPagerAdapter(shellItem, ChildFragmentManager);

			viewPager.Adapter = adapter;
			viewPager.OverScrollMode = OverScrollMode.Never;

			_tablayout.SetupWithViewPager(viewPager);

			_toolbar.Title = shellItem.Title;

			scrollview.FillViewport = true;
			scrollview.AddView(viewPager);

			return _rootView = root;
		}

		public override void OnDestroyView()
		{
			// FIXME
			base.OnDestroyView();
		}
	}
}
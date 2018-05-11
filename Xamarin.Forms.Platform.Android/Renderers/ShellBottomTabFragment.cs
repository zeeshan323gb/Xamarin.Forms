using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using AView = Android.Views.View;
using IMenu = Android.Views.IMenu;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellBottomTabFragment : Fragment, IShellItemRenderer, BottomNavigationView.IOnNavigationItemSelectedListener
	{
		#region IShellItemRenderer

		Fragment IShellItemRenderer.Fragment => this;

		ShellItem IShellItemRenderer.ShellItem
		{
			get => _shellItem;
			set => _shellItem = value;
		}

		#endregion IShellItemRenderer

		#region IOnNavigationItemSelectedListener

		bool BottomNavigationView.IOnNavigationItemSelectedListener.OnNavigationItemSelected(IMenuItem item)
		{
			return OnItemSelected(item);
		}

		#endregion IOnNavigationItemSelectedListener

		protected const int MoreTabId = 99;

		private readonly IShellContext _shellContext;

		private BottomNavigationView _bottomView;

		private Fragment _currentFragment;

		private ShellItem _shellItem;

		public ShellBottomTabFragment(IShellContext shellContext)
		{
			_shellContext = shellContext;
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var outerLayout = inflater.Inflate(Resource.Layout.BottomTabLayout, null);
			var _bottomView = outerLayout.FindViewById<BottomNavigationView>(Resource.Id.bottomtab_tabbar);
			var navigationArea = outerLayout.FindViewById<FrameLayout>(Resource.Id.bottomtab_navarea);

			_bottomView.SetBackgroundColor(Color.White.ToAndroid());
			_bottomView.SetOnNavigationItemSelectedListener(this);

			SetupMenu(_bottomView.Menu, _bottomView.MaxItemCount, _shellItem);

			SwapFragments(null, CreateFragmentForTab(_shellItem.CurrentItem).Fragment, navigationArea);

			return outerLayout;
		}

		// Use OnDestory become OnDestroyView may fire before events are completed.
		public override void OnDestroy()
		{
			if (_bottomView != null)
			{
				_bottomView.SetOnNavigationItemSelectedListener(null);
				_bottomView.Dispose();
				_bottomView = null;
			}
			base.OnDestroy();
		}

		protected virtual IShellObservableFragment CreateFragmentForTab(ShellTabItem tab)
		{
			// this needs to support setting the ShellTabItem also
			return new ShellContentFragment(_shellContext, tab);
		}

		protected virtual bool OnItemSelected(IMenuItem item)
		{
			var id = item.ItemId;
			if (id == MoreTabId)
			{
				// handle the more tab
			}
			else
			{
				var shellTabItem = _shellItem.Items[id];
			}

			return true;
		}

		protected virtual void SetupMenu(IMenu menu, int maxBottomItems, ShellItem shellItem)
		{
			bool showMore = _shellItem.Items.Count > maxBottomItems;

			int end = showMore ? maxBottomItems - 1 : _shellItem.Items.Count;

			for (int i = 0; i < end; i++)
			{
				var menuItem = menu.Add(0, i, 0, new Java.Lang.String(shellItem.Items[i].Title));
				SetMenuItemIcon(menuItem, shellItem.Items[i].Icon);
			}

			if (showMore)
			{
				var menuItem = menu.Add(0, MoreTabId, 0, new Java.Lang.String("More"));
				SetMenuItemIcon(menuItem, "grid.png");
			}
		}

		protected virtual void SwapFragments(Fragment oldFragment, Fragment newFragment, AView target)
		{
			var trans = ChildFragmentManager.BeginTransaction();

			trans.Replace(target.Id, newFragment);

			_currentFragment = newFragment;

			trans.CommitNowAllowingStateLoss();
		}

		private async void SetMenuItemIcon(IMenuItem menuItem, ImageSource source)
		{
			if (source == null)
				return;
			var drawable = await Context.GetFormsDrawable(source);
			menuItem.SetIcon(drawable);
		}
	}
}
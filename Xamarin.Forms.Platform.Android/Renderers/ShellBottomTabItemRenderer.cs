using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using AView = Android.Views.View;
using IMenu = Android.Views.IMenu;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellBottomTabItemRenderer : ShellItemRendererBase, BottomNavigationView.IOnNavigationItemSelectedListener
	{
		#region IOnNavigationItemSelectedListener

		bool BottomNavigationView.IOnNavigationItemSelectedListener.OnNavigationItemSelected(IMenuItem item)
		{
			return OnItemSelected(item);
		}

		#endregion IOnNavigationItemSelectedListener

		protected const int MoreTabId = 99;
		private readonly IShellContext _shellContext;
		private BottomNavigationView _bottomView;
		private FrameLayout _navigationArea;

		public ShellBottomTabItemRenderer(IShellContext shellContext) : base(shellContext)
		{
			_shellContext = shellContext;
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var outerLayout = inflater.Inflate(Resource.Layout.BottomTabLayout, null);
			_bottomView = outerLayout.FindViewById<BottomNavigationView>(Resource.Id.bottomtab_tabbar);
			_navigationArea = outerLayout.FindViewById<FrameLayout>(Resource.Id.bottomtab_navarea);
			_navigationArea.SetBackgroundColor(Color.Black.ToAndroid());

			_bottomView.SetBackgroundColor(Color.White.ToAndroid());
			_bottomView.SetOnNavigationItemSelectedListener(this);

			HookEvents(ShellItem);
			SetupMenu(_bottomView.Menu, _bottomView.MaxItemCount, ShellItem);

			return outerLayout;
		}

		// Use OnDestory become OnDestroyView may fire before events are completed.
		public override void OnDestroy()
		{
			UnhookEvents(ShellItem);
			if (_bottomView != null)
			{
				_bottomView.SetOnNavigationItemSelectedListener(null);
				_bottomView.Dispose();
				_bottomView = null;
			}

			base.OnDestroy();
		}

		protected override IShellObservableFragment CreateFragmentForPage(Page page)
		{
			return new ShellContentFragment(_shellContext, page);
		}

		protected override ViewGroup GetNavigationTarget() => _navigationArea;

		protected override IShellObservableFragment GetOrCreateFragmentForTab(ShellTabItem tab)
		{
			return new ShellContentFragment(_shellContext, tab);
		}

		protected override void OnCurrentTabItemChanged()
		{
			base.OnCurrentTabItemChanged();

			var index = ShellItem.Items.IndexOf(CurrentTabItem);
			if (_bottomView.Menu.Size() > index)
			{
				var menuItem = _bottomView.Menu.GetItem(index);
				menuItem.SetChecked(true);
			}
		}

		protected virtual bool OnItemSelected(IMenuItem item)
		{
			if (item.IsChecked)
				return true;

			var id = item.ItemId;
			if (id == MoreTabId)
			{
				// handle the more tab
				for (int i = 4; i < ShellItem.Items.Count; i++)
				{
				}
			}
			else
			{
				var shellTabItem = ShellItem.Items[id];
				ShellItem.SetValueFromRenderer(ShellItem.CurrentItemProperty, shellTabItem);
			}

			return true;
		}

		protected virtual void SetupMenu(IMenu menu, int maxBottomItems, ShellItem shellItem)
		{
			bool showMore = ShellItem.Items.Count > maxBottomItems;

			int end = showMore ? maxBottomItems - 1 : ShellItem.Items.Count;

			for (int i = 0; i < end; i++)
			{
				var item = shellItem.Items[i];
				var menuItem = menu.Add(0, i, 0, new Java.Lang.String(item.Title));
				SetMenuItemIcon(menuItem, item.Icon);
				if (item == CurrentTabItem)
				{
					menuItem.SetChecked(true);
				}
			}

			if (showMore)
			{
				var menuItem = menu.Add(0, MoreTabId, 0, new Java.Lang.String("More"));
				SetMenuItemIcon(menuItem, "grid.png");
			}

			_bottomView.SetShiftMode(false, false);
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
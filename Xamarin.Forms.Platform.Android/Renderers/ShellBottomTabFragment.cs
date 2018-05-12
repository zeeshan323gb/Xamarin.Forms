using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using LP = Android.Views.ViewGroup.LayoutParams;
using AView = Android.Views.View;
using IMenu = Android.Views.IMenu;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xamarin.Forms.Internals;
using System.Linq;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellBottomTabFragment : ShellItemRendererBase, BottomNavigationView.IOnNavigationItemSelectedListener
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

		private IShellObservableFragment _currentFragment;

		public ShellBottomTabFragment(IShellContext shellContext) : base(shellContext)
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

		protected override void OnCurrentTabItemChanged()
		{
			EnsureNavigationState(ShellNavigationSource.ShellTabItemChanged, CurrentTabItem, null, false);

			var index = ShellItem.Items.IndexOf(CurrentTabItem);
			if (_bottomView.Menu.Size() > index)
			{
				var menuItem = _bottomView.Menu.GetItem(index);
				menuItem.SetChecked(true);
			}
		}

		protected override void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
			EnsureNavigationState((ShellNavigationSource)e.RequestType, sender as ShellTabItem, e.Page, e.Animated);
		}

		private void RemoveFragment (Fragment fragment)
		{
			var t = ChildFragmentManager.BeginTransaction();
			t.Remove(fragment);
			t.CommitAllowingStateLoss();
		}

		private void RemoveAllPushedPages (ShellTabItem tab, bool keepCurrent)
		{
			if (tab.Stack.Count <= 1 || (keepCurrent && tab.Stack.Count == 2))
				return;

			var t = ChildFragmentManager.BeginTransaction();

			foreach (var kvp in _fragmentMap.ToList())
			{
				if (kvp.Key.Parent != tab)
					continue;

				_fragmentMap.Remove(kvp.Key);

				if (keepCurrent && kvp.Value.Fragment == _currentFragment)
					continue;

				t.Remove(kvp.Value.Fragment);
			}

			t.CommitAllowingStateLoss();
		}

		private void RemoveAllButCurrent ()
		{
			var trans = ChildFragmentManager.BeginTransaction();
			foreach (var kvp in _fragmentMap)
			{
				var f = kvp.Value.Fragment;
				if (kvp.Value.Fragment == _currentFragment || !f.IsAdded)
					continue;
				trans.Remove(f);
			};
			trans.CommitAllowingStateLoss();
		}

		private readonly Dictionary<Element, IShellObservableFragment> _fragmentMap = new Dictionary<Element, IShellObservableFragment>();

		private Task<bool> EnsureNavigationState(ShellNavigationSource navSource, ShellTabItem item, Page page, bool animated)
		{
			TaskCompletionSource<bool> result = new TaskCompletionSource<bool>();

			var stack = CurrentTabItem.Stack;
			bool isForCurrentTab = item == CurrentTabItem;

			if (!_fragmentMap.ContainsKey(CurrentTabItem))
			{
				_fragmentMap[CurrentTabItem] = CreateFragmentForTab(CurrentTabItem);
			}

			switch (navSource)
			{
				case ShellNavigationSource.Push:
				case ShellNavigationSource.Insert:
					_fragmentMap[page] = CreateFragmentForPage(page);
					if (!isForCurrentTab)
					{
						return Task.FromResult(true);
					}
					break;
				case ShellNavigationSource.Pop:
					if (_fragmentMap.TryGetValue(page, out var frag))
					{
						if (frag.Fragment.IsAdded && !isForCurrentTab)
							RemoveFragment(frag.Fragment);
						_fragmentMap.Remove(page);
					}
					if (!isForCurrentTab)
						return Task.FromResult(true);
					break;
				case ShellNavigationSource.Remove:
					if (_fragmentMap.TryGetValue(page, out var removeFragment))
					{
						if(removeFragment.Fragment.IsAdded)
							RemoveFragment(removeFragment.Fragment);
						_fragmentMap.Remove(page);
					}
					return Task.FromResult(true);
				case ShellNavigationSource.PopToRoot:
					RemoveAllPushedPages(item, isForCurrentTab);
					if (!isForCurrentTab)
						return Task.FromResult(true);
					break;
				case ShellNavigationSource.ShellTabItemChanged:
					RemoveAllButCurrent();
					break;
				default:
					throw new InvalidOperationException("Unexpected navigation type");
			}

			IShellObservableFragment target = null;
			if (stack.Count == 1 || navSource == ShellNavigationSource.PopToRoot)
				target = _fragmentMap[CurrentTabItem];
			else
				target = _fragmentMap[stack[stack.Count - 1]];

			var t = ChildFragmentManager.BeginTransaction();

			if (animated)
				SetupAnimation(navSource, t, page);

			IShellObservableFragment trackFragment = null;
			switch (navSource)
			{
				case ShellNavigationSource.Push:
					trackFragment = target;

					if (_currentFragment != null)
						t.Hide(_currentFragment.Fragment);

					if (!target.Fragment.IsAdded)
						t.Add(_navigationArea.Id, target.Fragment);
					t.Show(target.Fragment);
					break;
				case ShellNavigationSource.Pop:
				case ShellNavigationSource.PopToRoot:
				case ShellNavigationSource.ShellTabItemChanged:
					trackFragment = _currentFragment;

					if (_currentFragment != null)
						t.Remove(_currentFragment.Fragment);

					if (!target.Fragment.IsAdded)
						t.Add(_navigationArea.Id, target.Fragment);
					t.Show(target.Fragment);
					break;
			}


			if (animated && trackFragment != null)
			{
				void callback(object s, EventArgs e)
				{
					trackFragment.AnimationFinished -= callback;
					result.TrySetResult(true);
				}
				trackFragment.AnimationFinished += callback;
			}
			else
			{
				result.TrySetResult(true);
			}

			t.CommitAllowingStateLoss();

			_currentFragment = target;

			return result.Task;
		}

		private void SetupAnimation(ShellNavigationSource navSource, FragmentTransaction t, Page page)
		{
			switch (navSource)
			{
				case ShellNavigationSource.Push:
					t.SetCustomAnimations(Resource.Animation.EnterFromRight, Resource.Animation.ExitToLeft);
					break;
				case ShellNavigationSource.Pop:
				case ShellNavigationSource.PopToRoot:
					t.SetCustomAnimations(Resource.Animation.EnterFromLeft, Resource.Animation.ExitToRight);
					break;
				case ShellNavigationSource.ShellTabItemChanged:
					break;
			}
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

		protected virtual IShellObservableFragment CreateFragmentForPage(Page page)
		{
			return new ShellContentFragment(_shellContext, page);
		}

		protected virtual IShellObservableFragment CreateFragmentForTab(ShellTabItem tab)
		{
			return new ShellContentFragment(_shellContext, tab);
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
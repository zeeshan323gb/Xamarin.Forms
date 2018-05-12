using Android.Support.V4.App;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	public abstract class ShellItemRendererBase : Fragment, IShellItemRenderer
	{
		#region IShellItemRenderer

		Fragment IShellItemRenderer.Fragment => this;

		ShellItem IShellItemRenderer.ShellItem
		{
			get { return ShellItem; }
			set { ShellItem = value; }
		}

		#endregion IShellItemRenderer

		private readonly Dictionary<Element, IShellObservableFragment> _fragmentMap = new Dictionary<Element, IShellObservableFragment>();
		private IShellObservableFragment _currentFragment;
		private ShellTabItem _currentTabItem;

		public ShellItemRendererBase(IShellContext shellContext)
		{
			ShellContext = shellContext;
		}

		protected ShellTabItem CurrentTabItem
		{
			get { return _currentTabItem; }
			set
			{
				_currentTabItem = value;
				if (value != null)
				{
					OnCurrentTabItemChanged();
				}
			}
		}

		protected IShellContext ShellContext { get; }

		protected ShellItem ShellItem { get; private set; }

		protected abstract IShellObservableFragment CreateFragmentForPage(Page page);

		protected abstract ViewGroup GetNavigationTarget();

		protected abstract IShellObservableFragment GetOrCreateFragmentForTab(ShellTabItem tab);

		protected virtual Task<bool> HandleFragmentUpdate(ShellNavigationSource navSource, ShellTabItem item, Page page, bool animated)
		{
			TaskCompletionSource<bool> result = new TaskCompletionSource<bool>();

			var stack = CurrentTabItem.Stack;
			bool isForCurrentTab = item == CurrentTabItem;

			if (!_fragmentMap.ContainsKey(CurrentTabItem))
			{
				_fragmentMap[CurrentTabItem] = GetOrCreateFragmentForTab(CurrentTabItem);
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
						if (removeFragment.Fragment.IsAdded)
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
					// We need to handle this after we know what the target is
					// because we might accidentally remove an already added target.
					// Then there would be two transactions in a row, one removing and one adding
					// the same fragement and things get really screwy when you do that.
					break;

				default:
					throw new InvalidOperationException("Unexpected navigation type");
			}

			IShellObservableFragment target = null;
			if (stack.Count == 1 || navSource == ShellNavigationSource.PopToRoot)
				target = _fragmentMap[CurrentTabItem];
			else
				target = _fragmentMap[stack[stack.Count - 1]];

			// Down here because of comment above
			if (navSource == ShellNavigationSource.ShellTabItemChanged)
				RemoveAllButCurrent(target.Fragment);

			if (target == _currentFragment)
				return Task.FromResult(true);

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
						t.Add(GetNavigationTarget().Id, target.Fragment);
					t.Show(target.Fragment);
					break;

				case ShellNavigationSource.Pop:
				case ShellNavigationSource.PopToRoot:
				case ShellNavigationSource.ShellTabItemChanged:
					trackFragment = _currentFragment;

					if (_currentFragment != null)
						t.Remove(_currentFragment.Fragment);

					if (!target.Fragment.IsAdded)
						t.Add(GetNavigationTarget().Id, target.Fragment);
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

		protected virtual void HookEvents(ShellItem shellItem)
		{
			shellItem.PropertyChanged += OnShellItemPropertyChanged;
			((INotifyCollectionChanged)shellItem.Items).CollectionChanged += OnShellTabItemsChanged;
			CurrentTabItem = shellItem.CurrentItem;

			foreach (var shellTabItem in shellItem.Items)
			{
				HookTabEvents(shellTabItem);
			}
		}

		protected virtual void HookTabEvents(ShellTabItem shellTabItem)
		{
			((IShellTabItemController)shellTabItem).NavigationRequested += OnNavigationRequested;
		}

		protected virtual void OnCurrentTabItemChanged()
		{
			HandleFragmentUpdate(ShellNavigationSource.ShellTabItemChanged, CurrentTabItem, null, false);
		}

		protected virtual void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
			e.Task = HandleFragmentUpdate((ShellNavigationSource)e.RequestType, (ShellTabItem)sender, e.Page, e.Animated);
		}

		protected virtual void OnShellItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
				CurrentTabItem = ShellItem.CurrentItem;
		}

		protected virtual void OnShellTabItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			foreach (ShellTabItem tab in e.OldItems)
				UnhookTabEvents(tab);

			foreach (ShellTabItem tab in e.NewItems)
				HookTabEvents(tab);
		}

		protected virtual void SetupAnimation(ShellNavigationSource navSource, FragmentTransaction t, Page page)
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

		protected virtual void UnhookEvents(ShellItem shellItem)
		{
			foreach (var shellTabItem in shellItem.Items)
			{
				UnhookTabEvents(shellTabItem);
			}

			ShellItem.PropertyChanged -= OnShellItemPropertyChanged;
			CurrentTabItem = null;
		}

		protected virtual void UnhookTabEvents(ShellTabItem shellTabItem)
		{
			((IShellTabItemController)shellTabItem).NavigationRequested -= OnNavigationRequested;
		}

		private void RemoveAllButCurrent(Fragment skip)
		{
			var trans = ChildFragmentManager.BeginTransaction();
			foreach (var kvp in _fragmentMap)
			{
				var f = kvp.Value.Fragment;
				if (kvp.Value == _currentFragment || kvp.Value.Fragment == skip || !f.IsAdded)
					continue;
				trans.Remove(f);
			};
			trans.CommitAllowingStateLoss();
		}

		private void RemoveAllPushedPages(ShellTabItem tab, bool keepCurrent)
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

		private void RemoveFragment(Fragment fragment)
		{
			var t = ChildFragmentManager.BeginTransaction();
			t.Remove(fragment);
			t.CommitAllowingStateLoss();
		}
	}
}
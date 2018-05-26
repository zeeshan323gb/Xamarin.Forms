using Android.Support.V4.App;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
		private ShellContent _currentShellContent;
		private Page _displayedPage;

		protected ShellItemRendererBase(IShellContext shellContext)
		{
			ShellContext = shellContext;
		}

		protected ShellContent ShellContent
		{
			get => _currentShellContent;
			set
			{
				_currentShellContent = value;
				if (value != null)
				{
					OnCurrentContentChanged();
				}
			}
		}

		protected Page DisplayedPage
		{
			get => _displayedPage;
			set
			{
				if (_displayedPage == value)
					return;

				Page oldPage = _displayedPage;
				_displayedPage = value;
				OnDisplayedPageChanged(_displayedPage, oldPage);
			}
		}

		protected IShellContext ShellContext { get; }

		protected ShellItem ShellItem { get; private set; }

		protected virtual IShellObservableFragment CreateFragmentForPage(Page page)
		{
			return ShellContext.CreateFragmentForPage(page);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				DisplayedPage = null;
			}
		}

		protected abstract ViewGroup GetNavigationTarget();

		protected virtual IShellObservableFragment GetOrCreateFragmentForTab(ShellContent shellContent)
		{
			return new ShellContentFragment(ShellContext, shellContent);
		}

		protected virtual Task<bool> HandleFragmentUpdate(ShellNavigationSource navSource, ShellContent item, Page page, bool animated)
		{
			TaskCompletionSource<bool> result = new TaskCompletionSource<bool>();

			var stack = ShellContent.Stack;
			bool isForCurrentTab = item == ShellContent;

			if (!_fragmentMap.ContainsKey(ShellContent))
			{
				_fragmentMap[ShellContent] = GetOrCreateFragmentForTab(ShellContent);
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

				case ShellNavigationSource.ShellContentChanged:
					// We need to handle this after we know what the target is
					// because we might accidentally remove an already added target.
					// Then there would be two transactions in a row, one removing and one adding
					// the same fragement and things get really screwy when you do that.
					break;

				default:
					throw new InvalidOperationException("Unexpected navigation type");
			}

			Page targetPage = null;
			IShellObservableFragment target = null;
			if (stack.Count == 1 || navSource == ShellNavigationSource.PopToRoot)
			{
				target = _fragmentMap[ShellContent];
				targetPage = ((IShellContentController)ShellContent).RootPage;
				if (targetPage == null)
				{
					// The ShellContent hasn't had to realize the page yet, we need
					// to force it here.
					targetPage = ((IShellContentController)ShellContent).GetOrCreateContent();
				}
			}
			else
			{
				targetPage = stack[stack.Count - 1];
				target = _fragmentMap[targetPage];
			}

			// Down here because of comment above
			if (navSource == ShellNavigationSource.ShellContentChanged)
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
				case ShellNavigationSource.ShellContentChanged:
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
				GetNavigationTarget().SetBackgroundColor(Color.Black.ToAndroid());
				void callback(object s, EventArgs e)
				{
					trackFragment.AnimationFinished -= callback;
					result.TrySetResult(true);
					GetNavigationTarget().SetBackground(null);
				}
				trackFragment.AnimationFinished += callback;
			}
			else
			{
				result.TrySetResult(true);
			}

			t.CommitAllowingStateLoss();

			_currentFragment = target;

			DisplayedPage = targetPage;

			return result.Task;
		}

		protected virtual void HookEvents(ShellItem shellItem)
		{
			shellItem.PropertyChanged += OnShellItemPropertyChanged;
			((INotifyCollectionChanged)shellItem.Items).CollectionChanged += OnShellItemsChanged;
			ShellContent = shellItem.CurrentItem;

			foreach (var shellContent in shellItem.Items)
			{
				HookTabEvents(shellContent);
			}
		}

		protected virtual void HookTabEvents(ShellContent shellContent)
		{
			((IShellContentController)shellContent).NavigationRequested += OnNavigationRequested;
			shellContent.PropertyChanged += OnShellContentPropertyChanged;
		}

		protected virtual void OnCurrentContentChanged()
		{
			HandleFragmentUpdate(ShellNavigationSource.ShellContentChanged, ShellContent, null, false);
		}

		protected virtual void OnDisplayedPageChanged(Page newPage, Page oldPage)
		{

		}

		protected virtual void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
			e.Task = HandleFragmentUpdate((ShellNavigationSource)e.RequestType, (ShellContent)sender, e.Page, e.Animated);
		}

		protected virtual void OnShellItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
				ShellContent = ShellItem.CurrentItem;
		}

		protected virtual void OnShellItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (ShellContent content in e.OldItems)
					UnhookTabEvents(content);
			}

			if (e.NewItems != null)
			{
				foreach (ShellContent content in e.NewItems)
					HookTabEvents(content);
			}
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

				case ShellNavigationSource.ShellContentChanged:
					break;
			}
		}

		protected virtual void UnhookEvents(ShellItem shellItem)
		{
			foreach (var shellContent in shellItem.Items)
			{
				UnhookTabEvents(shellContent);
			}

			ShellItem.PropertyChanged -= OnShellItemPropertyChanged;
			ShellContent = null;
		}

		protected virtual void UnhookTabEvents(ShellContent shellContent)
		{
			((IShellContentController)shellContent).NavigationRequested -= OnNavigationRequested;
			shellContent.PropertyChanged -= OnShellContentPropertyChanged;
		}

		protected virtual void OnShellContentPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
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

		private void RemoveAllPushedPages(ShellContent content, bool keepCurrent)
		{
			if (content.Stack.Count <= 1 || (keepCurrent && content.Stack.Count == 2))
				return;

			var t = ChildFragmentManager.BeginTransaction();

			foreach (var kvp in _fragmentMap.ToList())
			{
				if (kvp.Key.Parent != content)
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
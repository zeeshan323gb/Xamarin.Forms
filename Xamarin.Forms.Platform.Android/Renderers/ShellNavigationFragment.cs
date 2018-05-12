using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellNavigationFragment : ShellItemRendererBase
	{
		private Fragment _currentFragment;
		private readonly Dictionary<Page, IShellObservableFragment> _fragmentMap = new Dictionary<Page, IShellObservableFragment>();
		private ShellTabItem _lastEnsuredItem;
		private FrameLayout _navigationTarget;
		private Fragment _rootFragment;

		public ShellNavigationFragment(IShellContext shellContext) : base(shellContext)
		{
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			_navigationTarget = new FrameLayout(Context)
			{
				LayoutParameters = new LP(LP.MatchParent, LP.MatchParent),
				Id = Platform.GenerateViewId()
			};
			_navigationTarget.SetBackgroundColor(global::Android.Graphics.Color.Black);

			HookEvents(ShellItem);

			return _navigationTarget;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			UnhookEvents(ShellItem);

			_navigationTarget?.Dispose();

			_navigationTarget = null;
			CurrentTabItem = null;
		}

		protected virtual IShellObservableFragment CreateFragmentForPage(Page page)
		{
			return new ShellContentFragment(ShellContext, page);
		}

		protected override void OnCurrentTabItemChanged()
		{
			EnsureNavigationState(NavigationRequestType.Push, null, false);
		}

		protected override void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
			if (sender == CurrentTabItem)
				e.Task = EnsureNavigationState(e.RequestType, e.Page, e.Animated);
		}

		protected virtual void SetupAnimation(NavigationRequestType navType, FragmentTransaction transaction, Page operativePage)
		{
			if (navType == NavigationRequestType.Push)
				transaction.SetCustomAnimations(Resource.Animation.EnterFromRight, Resource.Animation.ExitToLeft);
			else
				transaction.SetCustomAnimations(Resource.Animation.EnterFromLeft, Resource.Animation.ExitToRight);
		}

		private Task<bool> EnsureNavigationState(NavigationRequestType navType, Page page, bool animated)
		{
			TaskCompletionSource<bool> result = new TaskCompletionSource<bool>();

			var stack = CurrentTabItem.Stack;

			if (_rootFragment == null)
				_rootFragment = new ShellItemRenderer(ShellContext) { ShellItem = ShellItem };

			if (_currentFragment == _rootFragment && stack.Count == 1)
			{
				result.TrySetResult(true);
				return result.Task;
			}

			if (_lastEnsuredItem != CurrentTabItem)
			{
				// we switched tabs, we need to clear out the old fragments
				var clearTrans = ChildFragmentManager.BeginTransaction();

				foreach (var kvp in _fragmentMap)
				{
					var frag = kvp.Value.Fragment;
					// only clear out hidden fragments as the currently visible one will be wiped away in the nav transaction
					if (frag.IsAdded && _currentFragment != frag)
						clearTrans.Remove(kvp.Value.Fragment);
				}

				clearTrans.CommitAllowingStateLoss();

				_fragmentMap.Clear();

				foreach (var p in stack)
				{
					if (p == null)
						continue;

					_fragmentMap[p] = CreateFragmentForPage(p);
				}

				_lastEnsuredItem = CurrentTabItem;
			}
			else
			{
				switch (navType)
				{
					case NavigationRequestType.Push:
					case NavigationRequestType.Insert:
						if (page != null)
							_fragmentMap[page] = CreateFragmentForPage(page);
						break;
						// this is borked, removed pages never get removed from the view
					case NavigationRequestType.Pop:
					case NavigationRequestType.Remove:
						_fragmentMap.Remove(page);
						break;

					case NavigationRequestType.PopToRoot:
						_fragmentMap.Clear();
						break;
				}
			}

			Fragment targetFragment = null;

			if (stack.Count == 1 || navType == NavigationRequestType.PopToRoot)
			{
				targetFragment = _rootFragment;
			}
			else
			{
				var lastPage = stack[stack.Count - 1];
				targetFragment = _fragmentMap[lastPage].Fragment;
			}

			if (targetFragment == _currentFragment)
			{
				result.TrySetResult(true);
				return result.Task;
			}

			var t = ChildFragmentManager.BeginTransaction();

			if (animated)
				SetupAnimation(navType, t, page);

			Fragment animationTrackFragment = null;
			switch (navType)
			{
				case NavigationRequestType.Push:
					animationTrackFragment = targetFragment;

					if (_currentFragment != null)
						t.Hide(_currentFragment);

					t.Add(_navigationTarget.Id, targetFragment);
					t.Show(targetFragment);

					break;

				case NavigationRequestType.Pop:
				case NavigationRequestType.PopToRoot:
					animationTrackFragment = _currentFragment;

					if (_currentFragment != null)
						t.Remove(_currentFragment);

					if (!targetFragment.IsAdded)
						t.Add(_navigationTarget.Id, targetFragment);
					t.Show(targetFragment);

					break;
			}

			if (animated && animationTrackFragment is IShellObservableFragment observableFragment)
			{
				void callback(object s, EventArgs e)
				{
					observableFragment.AnimationFinished -= callback;
					result.TrySetResult(true);
				}
				observableFragment.AnimationFinished += callback;
			}
			else
			{
				result.TrySetResult(true);
			}

			t.CommitAllowingStateLoss();

			_currentFragment = targetFragment;

			return result.Task;
		}
	}
}
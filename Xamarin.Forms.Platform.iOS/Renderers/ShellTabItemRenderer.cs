using Foundation;
using ObjCRuntime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellContentRenderer : UINavigationController, IShellContentRenderer, IAppearanceObserver
	{
		#region IShellContentRenderer

		public bool IsInMoreTab { get; set; }

		public Page Page { get; private set; }

		public ShellContent ShellContent
		{
			get { return _shellContent; }
			set
			{
				if (_shellContent == value)
					return;
				_shellContent = value;
				LoadPages();
				OnShellContentSet();
				_shellContent.PropertyChanged += HandlePropertyChanged;
				((IShellContentController)_shellContent).NavigationRequested += OnNavigationRequested;
			}
		}

		public UIViewController ViewController => this;

		#endregion IShellContentRenderer

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
				_appearanceTracker.ResetAppearance(this);
			else
				_appearanceTracker.SetAppearance(this, appearance);
		}

		#endregion IAppearanceObserver

		private readonly IShellContext _context;

		private readonly Dictionary<Page, IShellPageRendererTracker> _trackers =
			new Dictionary<Page, IShellPageRendererTracker>();
		private IShellNavBarAppearanceTracker _appearanceTracker;
		private Dictionary<UIViewController, TaskCompletionSource<bool>> _completionTasks =
							new Dictionary<UIViewController, TaskCompletionSource<bool>>();

		private bool _disposed;
		private bool _ignorePop;
		private TaskCompletionSource<bool> _popCompletionTask;
		private IVisualElementRenderer _renderer;
		private ShellContent _shellContent;

		public ShellContentRenderer(IShellContext context)
		{
			Delegate = new NavDelegate(this);
			_context = context;
		}

		public override UIViewController PopViewController(bool animated)
		{
			if (!_ignorePop)
			{
				_popCompletionTask = new TaskCompletionSource<bool>();
				SendPoppedOnCompletion(_popCompletionTask.Task);
			}

			return base.PopViewController(animated);
		}

		[Export("navigationBar:shouldPopItem:")]
		public bool ShouldPopItem(UINavigationBar navigationBar, UINavigationItem item)
		{
			// this means the pop is already done, nothing we can do
			if (ViewControllers.Length < NavigationBar.Items.Length)
				return true;

			bool allowPop = ShouldPop();

			if (allowPop)
			{
				// Do not remove, wonky behavior on some versions of iOS if you dont dispatch
				CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() => PopViewController(true));
			}
			else
			{
				for (int i = 0; i < NavigationBar.Subviews.Length; i++)
				{
					var child = NavigationBar.Subviews[i];
					if (child.Alpha != 1)
						UIView.Animate(.2f, () => child.Alpha = 1);
				}
			}

			return false;
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_appearanceTracker.UpdateLayout(this);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			InteractivePopGestureRecognizer.Delegate = new GestureDelegate(this, ShouldPop);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				_disposed = true;
				((IShellContentController)_shellContent).RecyclePage(Page);
				_appearanceTracker.Dispose();
				_shellContent.PropertyChanged -= HandlePropertyChanged;
				((IShellContentController)_shellContent).NavigationRequested -= OnNavigationRequested;
				((IShellController)_context.Shell).RemoveAppearanceObserver(this);
				DisposePage(Page);
			}

			// must be set null prior to _shellContent to ensure weak ref page gets cleared
			Page = null;
			_shellContent = null;
			_appearanceTracker = null;
			_renderer = null;
		}

		protected virtual void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BaseShellItem.TitleProperty.PropertyName)
				UpdateTabBarItem();
			else if (e.PropertyName == BaseShellItem.IconProperty.PropertyName)
				UpdateTabBarItem();
		}

		protected virtual void LoadPages()
		{
			var content = ((IShellContentController)ShellContent).GetOrCreateContent();
			Page = content;

			if (!Shell.GetTabBarVisible(Page))
				Log.Warning("Shell", "Root page of a ShellContent will never hide the TabBar");

			_renderer = Platform.CreateRenderer(content);
			Platform.SetRenderer(content, _renderer);

			var tracker = _context.CreatePageRendererTracker();
			tracker.IsRootPage = !IsInMoreTab; // default tracker requires this be set first
			tracker.Renderer = _renderer;

			_trackers[Page] = tracker;

			PushViewController(_renderer.ViewController, false);

			var stack = ShellContent.Stack;
			for (int i = 1; i < stack.Count; i++)
			{
				PushPage(stack[i], false);
			}
		}

		protected virtual void OnInsertRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;
			var before = e.BeforePage;

			var beforeRenderer = Platform.GetRenderer(before);

			var renderer = Platform.CreateRenderer(page);
			Platform.SetRenderer(page, renderer);

			var tracker = _context.CreatePageRendererTracker();
			tracker.Renderer = renderer;

			_trackers[page] = tracker;

			ViewControllers.Insert(ViewControllers.IndexOf(beforeRenderer.ViewController), renderer.ViewController);
		}

		protected virtual void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
			switch (e.RequestType)
			{
				case NavigationRequestType.Push:
					OnPushRequested(e);
					break;

				case NavigationRequestType.Pop:
					OnPopRequested(e);
					break;

				case NavigationRequestType.PopToRoot:
					OnPopToRootRequested(e);
					break;

				case NavigationRequestType.Insert:
					OnInsertRequested(e);
					break;

				case NavigationRequestType.Remove:
					OnRemoveRequested(e);
					break;
			}
		}

		protected virtual async void OnPopRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;
			var animated = e.Animated;

			_popCompletionTask = new TaskCompletionSource<bool>();
			e.Task = _popCompletionTask.Task;

			_ignorePop = true;
			PopViewController(animated);
			_ignorePop = false;

			await _popCompletionTask.Task;

			DisposePage(page);
		}

		protected virtual async void OnPopToRootRequested(NavigationRequestedEventArgs e)
		{
			var animated = e.Animated;

			var task = new TaskCompletionSource<bool>();
			var pages = _shellContent.Stack.ToList();
			_completionTasks[_renderer.ViewController] = task;
			e.Task = task.Task;

			PopToRootViewController(animated);

			await e.Task;

			for (int i = pages.Count - 1; i >= 1; i--)
			{
				var page = pages[i];
				DisposePage(page);
			}
		}

		protected virtual void OnPushRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;
			var animated = e.Animated;

			var taskSource = new TaskCompletionSource<bool>();
			PushPage(page, animated, taskSource);

			e.Task = taskSource.Task;
		}

		protected virtual void OnRemoveRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;

			var renderer = Platform.GetRenderer(page);

			if (renderer != null)
			{
				if (renderer.ViewController == TopViewController)
				{
					e.Animated = false;
					OnPopRequested(e);
				}
				ViewControllers = ViewControllers.Remove(renderer.ViewController);
				DisposePage(page);
			}
		}

		protected virtual void OnShellContentSet()
		{
			_appearanceTracker = _context.CreateNavBarAppearanceTracker();
			UpdateTabBarItem();
			((IShellController)_context.Shell).AddAppearanceObserver(this, ShellContent);
		}

		protected virtual async void UpdateTabBarItem()
		{
			Title = ShellContent.Title;
			var imageSource = ShellContent.Icon;
			UIImage icon = null;
			if (imageSource != null)
			{
				var source = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(imageSource);
				icon = await source.LoadImageAsync(imageSource);
			}
			TabBarItem = new UITabBarItem(ShellContent.Title, icon, null);
		}

		private void DisposePage(Page page)
		{
			if (_trackers.TryGetValue(page, out var tracker))
			{
				tracker.Dispose();
				_trackers.Remove(page);
			}

			var renderer = Platform.GetRenderer(page);
			if (renderer != null)
			{
				renderer.Dispose();
				page.ClearValue(Platform.RendererProperty);
			}
		}

		private Page PageForViewController(UIViewController viewController)
		{
			if (_renderer.ViewController == viewController)
				return Page;

			foreach (var child in ShellContent.Stack)
			{
				if (child == null)
					continue;
				var renderer = Platform.GetRenderer(child);
				if (viewController == renderer.ViewController)
					return child;
			}

			return null;
		}

		private void PushPage(Page page, bool animated, TaskCompletionSource<bool> completionSource = null)
		{
			var renderer = Platform.CreateRenderer(page);
			Platform.SetRenderer(page, renderer);

			bool tabBarVisible = Shell.GetTabBarVisible(page);
			renderer.ViewController.HidesBottomBarWhenPushed = !tabBarVisible;

			var tracker = _context.CreatePageRendererTracker();
			tracker.Renderer = renderer;

			_trackers[page] = tracker;

			if (completionSource != null)
				_completionTasks[renderer.ViewController] = completionSource;

			PushViewController(renderer.ViewController, animated);
		}

		private async void SendPoppedOnCompletion(Task popTask)
		{
			if (popTask == null)
			{
				throw new ArgumentNullException(nameof(popTask));
			}

			await popTask;

			var poppedPage = _shellContent.Stack[_shellContent.Stack.Count - 1];
			((IShellContentController)_shellContent).SendPopped();
			DisposePage(poppedPage);
		}

		private bool ShouldPop()
		{
			var shellItem = _context.Shell.CurrentItem;
			var content = shellItem?.CurrentItem;
			var stack = content?.Stack.ToList();

			stack.RemoveAt(stack.Count - 1);

			return ((IShellController)_context.Shell).ProposeNavigation(ShellNavigationSource.Pop, shellItem, content, stack, true);
		}

		private class GestureDelegate : UIGestureRecognizerDelegate
		{
			private readonly UINavigationController _parent;
			private readonly Func<bool> _shouldPop;

			public GestureDelegate(UINavigationController parent, Func<bool> shouldPop)
			{
				_parent = parent;
				_shouldPop = shouldPop;
			}

			public override bool ShouldBegin(UIGestureRecognizer recognizer)
			{
				if (_parent.ViewControllers.Length == 1)
					return false;
				return _shouldPop();
			}
		}

		private class NavDelegate : UINavigationControllerDelegate
		{
			private readonly ShellContentRenderer _self;

			public NavDelegate(ShellContentRenderer renderer)
			{
				_self = renderer;
			}

			public override void DidShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
			{
				var tasks = _self._completionTasks;
				var popTask = _self._popCompletionTask;

				if (tasks.TryGetValue(viewController, out var source))
				{
					source.TrySetResult(true);
					tasks.Remove(viewController);
				}
				else if (popTask != null)
				{
					popTask.TrySetResult(true);
				}
			}

			public override void WillShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
			{
				var page = _self.PageForViewController(viewController);
				bool navBarVisible = Shell.GetNavBarVisible(page);
				navigationController.SetNavigationBarHidden(!navBarVisible, true);
			}
		}
	}
}
using CoreGraphics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellItemRenderer : UITabBarController, IShellItemRenderer, IAppearanceObserver
	{
		#region IShellItemRenderer

		public ShellItem ShellItem
		{
			get => _shellItem;
			set
			{
				if (_shellItem == value)
					return;
				_shellItem = value;
				OnShellItemSet(_shellItem);
				CreateTabRenderers();
			}
		}

		UIViewController IShellItemRenderer.ViewController => this;

		#endregion IShellItemRenderer

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			UpdateShellAppearance(appearance);
		}

		#endregion IAppearanceObserver

		private readonly IShellContext _context;
		private readonly Dictionary<UIViewController, IShellSectionRenderer> _sectionRenderers = new Dictionary<UIViewController, IShellSectionRenderer>();
		private IShellTabBarAppearanceTracker _appearanceTracker;
		private bool _disposed;
		private ShellItem _shellItem;

		public ShellItemRenderer(IShellContext context)
		{
			_context = context;
		}

		public override UIViewController SelectedViewController
		{
			get { return base.SelectedViewController; }
			set
			{
				base.SelectedViewController = value;

				var renderer = RendererForViewController(value);
				if (renderer != null)
				{
					ShellItem.SetValueFromRenderer(ShellItem.CurrentItemProperty, renderer.ShellSection);
					CurrentRenderer = renderer;
				}
			}
		}

		private IShellSectionRenderer CurrentRenderer { get; set; }

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_appearanceTracker?.UpdateLayout(this);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			ShouldSelectViewController = (tabController, viewController) =>
			{
				bool accept = true;
				var r = RendererForViewController(viewController);
				if (r != null)
				{
					var controller = (IShellController)_context.Shell;
					// This is the part where we get down on one knee and ask the deveoper
					// to navigate us. If they dont cancel our proposal we will be engaged
					// to navigate together.
					accept = controller.ProposeNavigation(ShellNavigationSource.ShellSectionChanged,
						ShellItem,
						r.ShellSection,
						r.ShellSection?.CurrentItem,
						r.ShellSection.Stack.ToList(),
						true
					);
				}

				return accept;
			};
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				_disposed = true;
				foreach (var kvp in _sectionRenderers.ToList())
				{
					var renderer = kvp.Value;
					RemoveRenderer(renderer);
					renderer.Dispose();
				}
				_sectionRenderers.Clear();
				CurrentRenderer = null;
				ShellItem.PropertyChanged -= OnElementPropertyChanged;
				((IShellController)_context.Shell).RemoveAppearanceObserver(this);
				((INotifyCollectionChanged)ShellItem.Items).CollectionChanged -= OnItemsCollectionChanged;
				_shellItem = null;
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
			{
				GoTo(ShellItem.CurrentItem);
			}
		}

		protected virtual void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (ShellSection shellSection in e.OldItems)
				{
					var renderer = RendererForShellContent(shellSection);
					if (renderer != null)
					{
						ViewControllers = ViewControllers.Remove(renderer.ViewController);
						RemoveRenderer(renderer);
					}
				}
			}

			if (e.NewItems != null && e.NewItems.Count > 0)
			{
				var count = ShellItem.Items.Count;
				UIViewController[] viewControllers = new UIViewController[count];

				int maxTabs = 5; // fetch this a better way
				bool willUseMore = count > maxTabs;

				int i = 0;
				bool goTo = false; // its possible we are in a transitionary state and should not nav
				var current = ShellItem.CurrentItem;
				for (int j = 0; j < ShellItem.Items.Count; j++)
				{
					var shellContent = ShellItem.Items[j];
					var renderer = RendererForShellContent(shellContent) ?? _context.CreateShellSectionRenderer(shellContent);

					if (willUseMore && j >= maxTabs - 1)
						renderer.IsInMoreTab = true;
					else
						renderer.IsInMoreTab = false;

					renderer.ShellSection = shellContent;

					AddRenderer(renderer);
					viewControllers[i++] = renderer.ViewController;
					if (shellContent == current)
						goTo = true;
				}

				ViewControllers = viewControllers;

				if (goTo)
					GoTo(ShellItem.CurrentItem);
			}

			SetTabBarHidden(ViewControllers.Length == 1);
		}

		protected virtual void OnShellItemSet(ShellItem item)
		{
			_appearanceTracker = _context.CreateTabBarAppearanceTracker();
			item.PropertyChanged += OnElementPropertyChanged;
			((IShellController)_context.Shell).AddAppearanceObserver(this, item);
			((INotifyCollectionChanged)item.Items).CollectionChanged += OnItemsCollectionChanged;
		}

		protected virtual void OnShellContentPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BaseShellItem.IsEnabledProperty.PropertyName)
			{
				var shellSection = (ShellSection)sender;
				var renderer = RendererForShellContent(shellSection);
				var index = ViewControllers.ToList().IndexOf(renderer.ViewController);
				TabBar.Items[index].Enabled = shellSection.IsEnabled;
			}
		}

		protected virtual void UpdateShellAppearance(ShellAppearance appearance)
		{
			if (appearance == null)
			{
				_appearanceTracker.ResetAppearance(this);
				return;
			}
			_appearanceTracker.SetAppearance(this, appearance);
		}

		private void AddRenderer(IShellSectionRenderer renderer)
		{
			if (_sectionRenderers.ContainsKey(renderer.ViewController))
				return;
			_sectionRenderers[renderer.ViewController] = renderer;
			renderer.ShellSection.PropertyChanged += OnShellContentPropertyChanged;
		}

		private void CreateTabRenderers()
		{
			var count = ShellItem.Items.Count;
			int maxTabs = 5; // fetch this a better way
			bool willUseMore = count > maxTabs;

			UIViewController[] viewControllers = new UIViewController[count];
			int i = 0;
			foreach (var shellContent in ShellItem.Items)
			{
				var renderer = _context.CreateShellSectionRenderer(shellContent);

				renderer.IsInMoreTab = willUseMore && i >= maxTabs - 1;

				renderer.ShellSection = shellContent;
				AddRenderer(renderer);
				viewControllers[i++] = renderer.ViewController;
			}
			ViewControllers = viewControllers;

			// No sense showing a bar that has a single icon
			if (ViewControllers.Length == 1)
				SetTabBarHidden(true);

			// Make sure we are at the right item
			GoTo(ShellItem.CurrentItem);

			// now that they are applied we can set the enabled state of the TabBar items
			for (i = 0; i < ViewControllers.Length; i++)
			{
				var renderer = RendererForViewController(ViewControllers[i]);
				if (!renderer.ShellSection.IsEnabled)
				{
					TabBar.Items[i].Enabled = false;
				}
			}
		}

		private void GoTo(ShellSection shellSection)
		{
			if (shellSection == null)
				return;
			var renderer = RendererForShellContent(shellSection);
			if (renderer?.ViewController != SelectedViewController)
				SelectedViewController = renderer.ViewController;
			CurrentRenderer = renderer;
		}

		private void RemoveRenderer(IShellSectionRenderer renderer)
		{
			if (_sectionRenderers.Remove(renderer.ViewController))
			{
				renderer.ShellSection.PropertyChanged -= OnShellContentPropertyChanged;
			}
		}

		private IShellSectionRenderer RendererForShellContent(ShellSection shellSection)
		{
			// Not Efficient!
			foreach (var item in _sectionRenderers)
			{
				if (item.Value.ShellSection == shellSection)
					return item.Value;
			}
			return null;
		}

		private IShellSectionRenderer RendererForViewController(UIViewController viewController)
		{
			// Efficient!
			if (_sectionRenderers.TryGetValue(viewController, out var value))
				return value;
			return null;
		}

		private void SetTabBarHidden(bool hidden)
		{
			TabBar.Hidden = hidden;

			if (CurrentRenderer == null)
				return;

			// now we must do the uikit jiggly dance to make sure the safe area updates. Failure
			// to perform the jiggle may result in the page not insetting properly when unhiding
			// the TabBar

			// a devious 1 pixel inset vertically
			CurrentRenderer.ViewController.View.Frame = View.Bounds.Inset(0, 1);

			// and quick as a whip we return it back to what it was with its insets being all proper
			CurrentRenderer.ViewController.View.Frame = View.Bounds;
		}
	}
}
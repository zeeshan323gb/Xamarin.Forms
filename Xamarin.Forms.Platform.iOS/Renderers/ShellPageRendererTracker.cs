using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellPageRendererTracker : IShellPageRendererTracker, IFlyoutBehaviorObserver
	{
		#region IShellPageRendererTracker

		public bool IsRootPage { get; set; }

		public IVisualElementRenderer Renderer
		{
			get
			{
				_rendererRef.TryGetTarget(out var target);
				return target;
			}
			set
			{
				_rendererRef = new WeakReference<IVisualElementRenderer>(value);
				Page = value.Element as Page;
				OnRendererSet();
			}
		}

		#endregion IShellPageRendererTracker

		private readonly IShellContext _context;
		private bool _disposed;
		private WeakReference<IVisualElementRenderer> _rendererRef;
		private IShellSearchResultsRenderer _resultsRenderer;
		private UISearchController _searchController;
		private SearchHandler _searchHandler;
		private FlyoutBehavior _flyoutBehavior;

		public ShellPageRendererTracker(IShellContext context)
		{
			_context = context;
		}

		private BackButtonBehavior BackButtonBehavior { get; set; }
		private UINavigationItem NavigationItem { get; set; }
		private Page Page { get; set; }

		public async void OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			_flyoutBehavior = behavior;
			await UpdateToolbarItems().ConfigureAwait(false);
		}

		protected virtual async void OnBackButtonBehaviorPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BackButtonBehavior.CommandParameterProperty.PropertyName)
				return;
			await UpdateToolbarItems().ConfigureAwait(false);
		}

		protected virtual void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.BackButtonBehaviorProperty.PropertyName)
			{
				SetBackButtonBehavior(Shell.GetBackButtonBehavior(Page));
			}
			else if (e.PropertyName == Shell.SearchHandlerProperty.PropertyName)
			{
				SearchHandler = Shell.GetSearchHandler(Page);
			}
			else if (e.PropertyName == Shell.TitleViewProperty.PropertyName)
			{
				UpdateTitleView();
			}
		}

		protected virtual void OnRendererSet()
		{
			NavigationItem = Renderer.ViewController.NavigationItem;
			Page.PropertyChanged += OnPagePropertyChanged;
			((INotifyCollectionChanged)Page.ToolbarItems).CollectionChanged += OnToolbarItemsChanged;
			SetBackButtonBehavior(Shell.GetBackButtonBehavior(Page));
			SearchHandler = Shell.GetSearchHandler(Page);
			UpdateTitleView();
			((IShellController)_context.Shell).AddFlyoutBehaviorObserver(this);
		}

		protected virtual void UpdateTitleView()
		{
			var titleView = Shell.GetTitleView(Page);

			if (titleView == null)
			{
				var view = NavigationItem.TitleView;
				NavigationItem.TitleView = null;
				view?.Dispose();
			}
			else
			{
				var view = new TitleViewContainer(titleView);

				if (Forms.IsiOS11OrNewer)
				{
					view.TranslatesAutoresizingMaskIntoConstraints = false;
				}
				else
				{
					view.TranslatesAutoresizingMaskIntoConstraints = true;
					view.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
				}

				NavigationItem.TitleView = view;
			}
		}

		protected virtual async Task UpdateToolbarItems()
		{
			if (NavigationItem.RightBarButtonItems != null)
			{
				for (var i = 0; i < NavigationItem.RightBarButtonItems.Length; i++)
					NavigationItem.RightBarButtonItems[i].Dispose();
			}

			List<UIBarButtonItem> items = new List<UIBarButtonItem>();
			foreach (var item in Page.ToolbarItems)
			{
				items.Add(item.ToUIBarButtonItem(false, true));
			}

			items.Reverse();
			NavigationItem.SetRightBarButtonItems(items.ToArray(), false);

			if (BackButtonBehavior != null)
			{
				var behavior = BackButtonBehavior;
				var command = behavior.Command;
				var commandParameter = behavior.CommandParameter;
				var image = behavior.IconOverride;
				var enabled = behavior.IsEnabled;
				if (image == null)
				{
					var text = BackButtonBehavior.TextOverride;
					NavigationItem.LeftBarButtonItem =
						new UIBarButtonItem(text, UIBarButtonItemStyle.Plain, (s, e) => command?.Execute(commandParameter)) { Enabled = enabled };
				}
				else
				{
					var source = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(image);
					var icon = await source.LoadImageAsync(image);
					NavigationItem.LeftBarButtonItem =
						new UIBarButtonItem(icon, UIBarButtonItemStyle.Plain, (s, e) => command?.Execute(commandParameter)) { Enabled = enabled };
				}
			}
			else if (IsRootPage && _flyoutBehavior == FlyoutBehavior.Flyout)
			{
				ImageSource image = "3bar.png";
				var source = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(image);
				var icon = await source.LoadImageAsync(image);
				NavigationItem.LeftBarButtonItem = new UIBarButtonItem(icon, UIBarButtonItemStyle.Plain, OnMenuButtonPressed);
			}
			else
			{
				NavigationItem.LeftBarButtonItem = null;
			}
		}

		private void OnMenuButtonPressed(object sender, EventArgs e)
		{
			_context.Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, true);
		}

		private async void OnToolbarItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			await UpdateToolbarItems().ConfigureAwait(false);
		}

		private async void SetBackButtonBehavior(BackButtonBehavior value)
		{
			if (BackButtonBehavior == value)
				return;

			if (BackButtonBehavior != null)
			{
				BackButtonBehavior.PropertyChanged -= OnBackButtonBehaviorPropertyChanged;
			}

			BackButtonBehavior = value;

			if (BackButtonBehavior != null)
			{
				BackButtonBehavior.PropertyChanged += OnBackButtonBehaviorPropertyChanged;
			}
			await UpdateToolbarItems().ConfigureAwait(false);
		}

		public class TitleViewContainer : UIContainerView
		{
			public TitleViewContainer(View view) : base(view)
			{
			}

			public override CGRect Frame
			{
				get => base.Frame;
				set
				{
					if (!Forms.IsiOS11OrNewer && Superview != null)
					{
						value.Y = Superview.Bounds.Y;
						value.Height = Superview.Bounds.Height;
					}

					base.Frame = value;
				}
			}

			public override CGSize IntrinsicContentSize => UILayoutFittingExpandedSize;

			public override CGSize SizeThatFits(CGSize size)
			{
				return size;
			}
		}

		#region SearchHandler

		private SearchHandler SearchHandler
		{
			get { return _searchHandler; }
			set
			{
				if (_searchHandler == value)
					return;

				if (_searchHandler != null)
				{
					if (_resultsRenderer != null)
					{
						_resultsRenderer.ItemSelected -= OnSearchItemSelected;
						_resultsRenderer.Dispose();
						_resultsRenderer = null;
					}
					_searchHandler.PropertyChanged -= OnSearchHandlerPropertyChanged;
					DettachSearchController();
				}

				_searchHandler = value;

				if (_searchHandler != null)
				{
					_searchHandler.PropertyChanged += OnSearchHandlerPropertyChanged;
					AttachSearchController();
				}
			}
		}

		protected virtual void OnSearchHandlerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == SearchHandler.ClearPlaceholderEnabledProperty.PropertyName)
				_searchController.SearchBar.ShowsBookmarkButton = _searchHandler.ClearPlaceholderEnabled;
			else if (e.PropertyName == SearchHandler.SearchBoxVisibilityProperty.PropertyName)
				UpdateSearchVisibility(_searchController);
			else if (e.PropertyName == SearchHandler.IsSearchEnabledProperty.PropertyName)
				UpdateSearchIsEnabled(_searchController);
		}

		protected virtual void UpdateSearchIsEnabled(UISearchController searchController)
		{
			searchController.SearchBar.UserInteractionEnabled = SearchHandler.IsSearchEnabled;
		}

		protected virtual void UpdateSearchVisibility(UISearchController searchController)
		{
			var visibility = SearchHandler.SearchBoxVisibility;
			if (visibility == SearchBoxVisiblity.Hidden)
			{
				if (searchController != null)
				{
					if (Forms.IsiOS11OrNewer)
						NavigationItem.SearchController = null;
					else
						NavigationItem.TitleView = null;
				}
			}
			else if (visibility == SearchBoxVisiblity.Collapsable || visibility == SearchBoxVisiblity.Expanded)
			{
				if (Forms.IsiOS11OrNewer)
				{
					NavigationItem.SearchController = _searchController;
					NavigationItem.HidesSearchBarWhenScrolling = visibility == SearchBoxVisiblity.Collapsable;
				}
				else
				{
					NavigationItem.TitleView = _searchController.SearchBar;
				}
			}
		}

		private void AttachSearchController()
		{
			if (SearchHandler.ShowsResults)
			{
				_resultsRenderer = _context.CreateShellSearchResultsRenderer();
				_resultsRenderer.ItemSelected += OnSearchItemSelected;
				_resultsRenderer.SearchHandler = _searchHandler;
				Renderer.ViewController.DefinesPresentationContext = true;
			}

			_searchController = new UISearchController(_resultsRenderer?.ViewController);
			var visibility = SearchHandler.SearchBoxVisibility;
			if (visibility != SearchBoxVisiblity.Hidden)
			{
				if (Forms.IsiOS11OrNewer)
					NavigationItem.SearchController = _searchController;
				else
					NavigationItem.TitleView = _searchController.SearchBar;
			}

			var searchBar = _searchController.SearchBar;

			_searchController.SetSearchResultsUpdater(sc =>
			{
				SearchHandler.SetValueCore(SearchHandler.QueryProperty, sc.SearchBar.Text);
			});

			searchBar.BookmarkButtonClicked += BookmarkButtonClicked;

			searchBar.Placeholder = SearchHandler.Placeholder;
			UpdateSearchIsEnabled(_searchController);
			searchBar.SearchButtonClicked += SearchButtonClicked;
			if (Forms.IsiOS11OrNewer)
				NavigationItem.HidesSearchBarWhenScrolling = visibility == SearchBoxVisiblity.Collapsable;

			var icon = SearchHandler.QueryIcon;
			if (icon != null)
			{
				SetSearchBarIcon(searchBar, icon, UISearchBarIcon.Search);
			}

			icon = SearchHandler.ClearIcon;
			if (icon != null)
			{
				SetSearchBarIcon(searchBar, icon, UISearchBarIcon.Clear);
			}

			icon = SearchHandler.ClearPlaceholderIcon;
			if (icon != null)
			{
				SetSearchBarIcon(searchBar, icon, UISearchBarIcon.Bookmark);
			}

			searchBar.ShowsBookmarkButton = SearchHandler.ClearPlaceholderEnabled;
		}

		private void BookmarkButtonClicked(object sender, EventArgs e)
		{
			((ISearchHandlerController)SearchHandler).ClearPlaceholderClicked();
		}

		private void DettachSearchController()
		{
			if (Forms.IsiOS11OrNewer)
			{
				NavigationItem.SearchController = null;
			}
			else
			{
				NavigationItem.TitleView = null;
			}

			_searchController.SetSearchResultsUpdater(null);
			_searchController.Dispose();
			_searchController = null;
		}

		private void OnSearchItemSelected(object sender, object e)
		{
			_searchController.Active = false;
			((ISearchHandlerController)SearchHandler).ItemSelected(e);
		}

		private void SearchButtonClicked(object sender, EventArgs e)
		{
			_searchController.Active = false;
			((ISearchHandlerController)SearchHandler).QueryConfirmed();
		}

		private async void SetSearchBarIcon(UISearchBar searchBar, ImageSource source, UISearchBarIcon icon)
		{
			var image = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source);
			var result = await image.LoadImageAsync(source);
			searchBar.SetImageforSearchBarIcon(result, icon, UIControlState.Normal);
		}

		#endregion SearchHandler

		#region IDisposable Support

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Page.PropertyChanged -= OnPagePropertyChanged;
					((INotifyCollectionChanged)Page.ToolbarItems).CollectionChanged -= OnToolbarItemsChanged;
					((IShellController)_context.Shell).RemoveFlyoutBehaviorObserver(this);
				}

				SearchHandler = null;
				Page = null;
				SetBackButtonBehavior(null);
				_rendererRef = null;
				NavigationItem = null;
				_disposed = true;
			}
		}

		#endregion IDisposable Support
	}
}
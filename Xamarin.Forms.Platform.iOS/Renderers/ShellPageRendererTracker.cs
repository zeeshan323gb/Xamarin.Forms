using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellPageRendererTracker : IShellPageRendererTracker
	{
		private readonly IShellContext _context;
		private BackButtonBehavior _backButtonBehavior;
		private bool _disposed = false;
		private WeakReference<IVisualElementRenderer> _rendererRef;
		private UISearchController _searchController;
		private SearchHandler _searchHandler;

		public ShellPageRendererTracker(IShellContext context)
		{
			_context = context;
		}

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

		private BackButtonBehavior BackButtonBehavior => _backButtonBehavior;
		private UINavigationItem NavigationItem { get; set; }
		private Page Page { get; set; }

		private UIBarButtonItem[] ToolbarItems
		{
			get => Renderer.ViewController.ToolbarItems;
			set => Renderer.ViewController.ToolbarItems = value;
		}

		protected virtual async void OnBackButtonBehaviorPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BackButtonBehavior.CommandParameterProperty.PropertyName)
				return;
			await UpdateToolbarItems();
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
		}

		protected virtual async void OnRendererSet()
		{
			NavigationItem = Renderer.ViewController.NavigationItem;
			Page.PropertyChanged += OnPagePropertyChanged;
			((INotifyCollectionChanged)Page.ToolbarItems).CollectionChanged += OnToolbarItemsChanged;
			SetBackButtonBehavior(Shell.GetBackButtonBehavior(Page));
			SearchHandler = Shell.GetSearchHandler(Page);
			await UpdateToolbarItems();
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
				items.Add(item.ToUIBarButtonItem());
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
			else if (IsRootPage)
			{
				ImageSource image = "3bar.png";
				var source = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(image);
				var icon = await source.LoadImageAsync(image);
				NavigationItem.LeftBarButtonItem = new UIBarButtonItem(icon, UIBarButtonItemStyle.Plain, OnMenuButtonPressed);
			}
		}

		private void OnMenuButtonPressed(object sender, EventArgs e)
		{
			_context.Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, true);
		}

		private async void OnToolbarItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			await UpdateToolbarItems();
		}

		private async void SetBackButtonBehavior(BackButtonBehavior value)
		{
			if (_backButtonBehavior == value)
				return;

			if (_backButtonBehavior != null)
			{
				_backButtonBehavior.PropertyChanged -= OnBackButtonBehaviorPropertyChanged;
			}

			_backButtonBehavior = value;

			if (_backButtonBehavior != null)
			{
				_backButtonBehavior.PropertyChanged += OnBackButtonBehaviorPropertyChanged;
				await UpdateToolbarItems();
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
			{
				_searchController.SearchBar.ShowsBookmarkButton = _searchHandler.ClearPlaceholderEnabled;
			}
		}

		private void AttachSearchController()
		{
			_searchController = new UISearchController((UIViewController)null);
			var visibility = SearchHandler.SearchBoxVisibility;
			if (SearchHandler.SearchBoxVisibility != SearchBoxVisiblity.Hidden)
			{
				if (Forms.IsiOS11OrNewer)
				{
					NavigationItem.SearchController = _searchController;
				}
				else
				{
					NavigationItem.TitleView = _searchController.SearchBar;
				}
			}

			var searchBar = _searchController.SearchBar;

			_searchController.SetSearchResultsUpdater(sc =>
			{
				SearchHandler.SetValueCore(SearchHandler.QueryProperty, sc.SearchBar.Text);
			});

			searchBar.BookmarkButtonClicked += BookmarkButtonClicked;

			searchBar.Placeholder = SearchHandler.Placeholder;
			searchBar.UserInteractionEnabled = SearchHandler.IsSearchEnabled;
			searchBar.SearchButtonClicked += SearchButtonClicked;
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
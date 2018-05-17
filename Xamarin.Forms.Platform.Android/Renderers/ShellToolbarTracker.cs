using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Support.V4.Widget;
using Android.Support.V7.Graphics.Drawable;
using Android.Views;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using ActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;
using AView = Android.Views.View;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using LP = Android.Views.ViewGroup.LayoutParams;
using R = Android.Resource;
using Android.Graphics;
using Android.Support.V7.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellToolbarTracker : Java.Lang.Object, AView.IOnClickListener, IShellToolbarTracker
	{
		private bool _canNavigateBack;
		private bool _disposed;
		private DrawerLayout _drawerLayout;
		private ActionBarDrawerToggle _drawerToggle;
		private Page _page;
		private IShellContext _shellContext;
		private Toolbar _toolbar;
		private IShellSearchView _searchView;
		private Color _tintColor = Color.Default;

		public ShellToolbarTracker(IShellContext shellContext, Toolbar toolbar, DrawerLayout drawerLayout)
		{
			_shellContext = shellContext ?? throw new ArgumentNullException(nameof(shellContext));
			_toolbar = toolbar ?? throw new ArgumentNullException(nameof(toolbar));
			_drawerLayout = drawerLayout ?? throw new ArgumentNullException(nameof(drawerLayout));
		}

		public bool CanNavigateBack
		{
			get { return _canNavigateBack; }
			set
			{
				if (_canNavigateBack == value)
					return;
				_canNavigateBack = value;
				UpdateLeftBarButtonItem();
			}
		}

		public Page Page
		{
			get { return _page; }
			set
			{
				if (_page == value)
					return;
				var oldPage = _page;
				_page = value;
				OnPageChanged(oldPage, _page);
			}
		}

		public Color TintColor
		{
			get { return _tintColor; }
			set
			{
				_tintColor = value;
				if (Page != null)
				{
					UpdateToolbarItems();
					UpdateLeftBarButtonItem();
				}
			}
		}

		void AView.IOnClickListener.OnClick(AView v)
		{
			var backButtonHandler = Shell.GetBackButtonBehavior(Page);
			if (backButtonHandler?.Command != null)
				backButtonHandler.Command.Execute(backButtonHandler.CommandParameter);
			else
				OnNavigateBack();
		}

		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_drawerToggle.Dispose();
					if (_searchView != null)
					{
						_searchView.View.RemoveFromParent();
						_searchView.View.ViewAttachedToWindow -= OnSearchViewAttachedToWindow;
						_searchView.SearchConfirmed -= OnSearchConfirmed;
						_searchView.Dispose();
					}

				}

				SearchHandler = null;
				_shellContext = null;
				_drawerToggle = null;
				_searchView = null;
				Page = null;
				_toolbar = null;
				_drawerLayout = null;
				_disposed = true;
			}
		}

		protected virtual void OnNavigateBack()
		{
			Page.Navigation.PopAsync();
		}

		protected virtual void OnPageChanged(Page oldPage, Page newPage)
		{
			if (oldPage != null)
			{
				oldPage.PropertyChanged -= OnPagePropertyChanged;
				((INotifyCollectionChanged)oldPage.ToolbarItems).CollectionChanged -= OnPageToolbarItemsChanged;
			}

			if (newPage != null)
			{
				newPage.PropertyChanged += OnPagePropertyChanged;
				((INotifyCollectionChanged)newPage.ToolbarItems).CollectionChanged += OnPageToolbarItemsChanged;

				UpdatePageTitle(_toolbar, newPage);
				UpdateLeftBarButtonItem();
				UpdateToolbarItems();
				UpdateNavBarVisible(_toolbar, newPage);
			}
		}

		protected virtual void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdatePageTitle(_toolbar, Page);
			else if (e.PropertyName == Shell.SearchHandlerProperty.PropertyName)
				UpdateToolbarItems();
			else if (e.PropertyName == Shell.NavBarVisibleProperty.PropertyName)
				UpdateNavBarVisible(_toolbar, Page);
		}

		protected virtual void OnPageToolbarItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateToolbarItems(_toolbar, Page);
		}

		protected virtual async void UpdateLeftBarButtonItem(Context context, Toolbar toolbar, DrawerLayout drawerLayout, Page page)
		{
			var backButtonHandler = Shell.GetBackButtonBehavior(page);

			if (backButtonHandler != null)
			{
				var icon = await context.GetFormsDrawable(backButtonHandler.IconOverride);
				icon = icon.GetConstantState().NewDrawable().Mutate();
				icon.SetColorFilter(TintColor.ToAndroid(Color.White), PorterDuff.Mode.SrcAtop);
				toolbar.SetNavigationOnClickListener(this);
				toolbar.NavigationIcon = icon;
			}
			else
			{
				var activity = (FormsAppCompatActivity)context;
				if (_drawerToggle == null)
				{
					_drawerToggle = new ActionBarDrawerToggle((Activity)context, drawerLayout, toolbar,
						R.String.Ok, R.String.Ok)
					{
						ToolbarNavigationClickListener = this,
					};
					drawerLayout.AddDrawerListener(_drawerToggle);
				}

				if (CanNavigateBack)
				{
					_drawerToggle.DrawerIndicatorEnabled = false;
					var icon = new DrawerArrowDrawable(activity.SupportActionBar.ThemedContext);
					icon.SetColorFilter(TintColor.ToAndroid(Color.White), PorterDuff.Mode.SrcAtop);
					icon.Progress = 1;
					toolbar.NavigationIcon = icon;
				}
				else
				{
					toolbar.NavigationIcon = null;
					_drawerToggle.DrawerArrowDrawable.SetColorFilter(TintColor.ToAndroid(Color.White), PorterDuff.Mode.SrcAtop);
					_drawerToggle.DrawerIndicatorEnabled = true;
				}
				_drawerToggle.SyncState();
			}
		}

		protected virtual void UpdateMenuItemIcon(Context context, IMenuItem menuItem, ToolbarItem toolBarItem)
		{
			FileImageSource icon = toolBarItem.Icon;
			if (!string.IsNullOrEmpty(icon))
			{
				Drawable iconDrawable = context.GetFormsDrawable(icon).GetConstantState().NewDrawable().Mutate();
				iconDrawable.SetColorFilter(TintColor.ToAndroid(Color.White), PorterDuff.Mode.SrcAtop);
				if (iconDrawable != null)
				{
					menuItem.SetIcon(iconDrawable);
					iconDrawable.Dispose();
				}
			}
		}

		protected virtual void UpdateNavBarVisible(Toolbar toolbar, Page page)
		{
			var navBarVisible = Shell.GetNavBarVisible(page);
			toolbar.Visibility = navBarVisible ? ViewStates.Visible : ViewStates.Gone;
		}

		protected virtual void UpdatePageTitle(Toolbar toolbar, Page page)
		{
			_toolbar.Title = page.Title;
		}

		protected virtual IShellSearchView GetSearchView(Context context)
		{
			return new ShellSearchView(context, _shellContext);
		}

		private SearchHandler _searchHandler;
		protected SearchHandler SearchHandler
		{
			get => _searchHandler;
			set
			{
				if (value == _searchHandler)
					return;

				var oldValue = _searchHandler;
				_searchHandler = value;
				OnSearchHandlerChanged(oldValue, _searchHandler);
			}
		}

		protected virtual void OnSearchHandlerChanged(SearchHandler oldValue, SearchHandler newValue)
		{
			if (oldValue != null)
			{
				oldValue.PropertyChanged -= OnSearchHandlerPropertyChanged;
			}

			if (newValue != null)
			{
				newValue.PropertyChanged += OnSearchHandlerPropertyChanged;
			}
		}

		protected virtual void OnSearchHandlerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == SearchHandler.SearchBoxVisibilityProperty.PropertyName)
			{
				UpdateToolbarItems(_toolbar, Page);
			}
		}

		protected virtual void UpdateToolbarItems(Toolbar toolbar, Page page)
		{
			var menu = toolbar.Menu;
			menu.Clear();

			foreach (var item in page.ToolbarItems)
			{
				var menuitem = menu.Add(item.Text);
				UpdateMenuItemIcon(_shellContext.AndroidContext, menuitem, item);
				menuitem.SetEnabled(item.IsEnabled);
				menuitem.SetShowAsAction(ShowAsAction.Always);
				menuitem.SetOnMenuItemClickListener(new GenericMenuClickListener(item.Activate));
			}

			SearchHandler = Shell.GetSearchHandler(page);
			if (SearchHandler != null && SearchHandler.SearchBoxVisibility != SearchBoxVisiblity.Hidden)
			{
				var context = _shellContext.AndroidContext;
				if (_searchView == null)
				{
					_searchView = GetSearchView(context);
					_searchView.SearchHandler = SearchHandler;

					_searchView.LoadView();
					_searchView.View.ViewAttachedToWindow += OnSearchViewAttachedToWindow;

					_searchView.View.LayoutParameters = new LP(LP.MatchParent, LP.MatchParent);
					_searchView.SearchConfirmed += OnSearchConfirmed;
				}

				if (SearchHandler.SearchBoxVisibility == SearchBoxVisiblity.Collapsable)
				{
					var item = menu.Add(new Java.Lang.String(SearchHandler.Placeholder));
					item.SetIcon(Resource.Drawable.abc_ic_search_api_material);
					item.Icon.SetColorFilter(TintColor.ToAndroid(Color.White), PorterDuff.Mode.SrcAtop);
					item.SetShowAsAction(ShowAsAction.IfRoom | ShowAsAction.CollapseActionView);

					if (_searchView.View.Parent != null)
						_searchView.View.RemoveFromParent();

					_searchView.ShowKeyboardOnAttached = true;
					item.SetActionView(_searchView.View);
				}
				else if (SearchHandler.SearchBoxVisibility == SearchBoxVisiblity.Expanded)
				{
					_searchView.ShowKeyboardOnAttached = false;
					if (_searchView.View.Parent != _toolbar)
						_toolbar.AddView(_searchView.View);
				}
			}
			else
			{
				if (_searchView != null)
				{
					_searchView.View.RemoveFromParent();
					_searchView.View.ViewAttachedToWindow -= OnSearchViewAttachedToWindow;
					_searchView.SearchConfirmed -= OnSearchConfirmed;
					_searchView.Dispose();
					_searchView = null;
				}
			}
		}

		private void OnSearchViewAttachedToWindow(object sender, AView.ViewAttachedToWindowEventArgs e)
		{
			// We only need to do this tint hack when using collapsed search handlers
			if (SearchHandler.SearchBoxVisibility != SearchBoxVisiblity.Collapsable)
				return;

			for (int i = 0; i < _toolbar.ChildCount; i++)
			{
				var child = _toolbar.GetChildAt(i);
				if (child is AppCompatImageButton button)
				{
					// we want the newly added button which will need layout
					if (child.IsLayoutRequested)
					{
						button.SetColorFilter(TintColor.ToAndroid(Color.White), PorterDuff.Mode.SrcAtop);
					}
				}
			}
		}

		protected virtual void OnSearchConfirmed(object sender, EventArgs e)
		{
			_toolbar.CollapseActionView();
		}

		private void UpdateLeftBarButtonItem()
		{
			UpdateLeftBarButtonItem(_shellContext.AndroidContext, _toolbar, _drawerLayout, Page);
		}

		private void UpdateToolbarItems()
		{
			UpdateToolbarItems(_toolbar, Page);
		}
	}
}
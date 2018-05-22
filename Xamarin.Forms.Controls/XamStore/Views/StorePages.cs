using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Controls.XamStore
{
    public class BasePage : ContentPage
	{
		private Button MakeButton (string title, Action callback)
		{
			var result = new Button();
			result.Text = title;
			result.Clicked += (s, e) => callback();
			return result;
		}

		public BasePage(string title, Color tint)
		{
			Title = title;

			var grid = new Grid()
			{
				Padding = 20,
				ColumnDefinitions =
				{
					new ColumnDefinition {Width = GridLength.Star},
					new ColumnDefinition {Width = GridLength.Star},
					new ColumnDefinition {Width = GridLength.Star},
				}
			};

			grid.Children.Add(new Label
			{
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalTextAlignment = TextAlignment.Center,
				Text = "Welcome to the " + GetType().Name
			}, 0, 3, 0, 1);

			grid.Children.Add(MakeButton("Push",
					() => Navigation.PushAsync((Page)Activator.CreateInstance(GetType()))),
				0, 1);

			grid.Children.Add(MakeButton("Pop",
					() => Navigation.PopAsync()),
				1, 1);

			grid.Children.Add(MakeButton("Pop To Root",
					() => Navigation.PopToRootAsync()),
				2, 1);

			grid.Children.Add(MakeButton("Insert",
					() => Navigation.InsertPageBefore((Page)Activator.CreateInstance(GetType()), this)),
				0, 2);

			grid.Children.Add(MakeButton("Remove",
					() => Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2])),
				1, 2);

			grid.Children.Add(MakeButton("Add Search",
					() => AddSearchHandler("Added Search", SearchBoxVisiblity.Expanded)),
				2, 2);

			grid.Children.Add(MakeButton("Add Toolbar",
					() => ToolbarItems.Add(new ToolbarItem("Test", "calculator.png", () => { }))),
				0, 3);

			grid.Children.Add(MakeButton("Remove Toolbar",
					() => ToolbarItems.RemoveAt(0)),
				1, 3);

			grid.Children.Add(MakeButton("Remove Search",
					RemoveSearchHandler),
				2, 3);

			grid.Children.Add(MakeButton("Add Tab",
					AddTabItem),
				0, 4);

			grid.Children.Add(MakeButton("Remove Tab",
					RemoveTabItem),
				1, 4);

			grid.Children.Add(MakeButton("Hide Tabs",
					() => Shell.SetTabBarVisible(this, false)),
				2, 4);

			grid.Children.Add(MakeButton("Show Tabs",
					() => Shell.SetTabBarVisible(this, true)),
				0, 5);

			grid.Children.Add(MakeButton("Hide Nav",
					() => Shell.SetNavBarVisible(this, false)),
				1, 5);

			grid.Children.Add(MakeButton("Show Nav",
					() => Shell.SetNavBarVisible(this, true)),
				2, 5);

			grid.Children.Add(MakeButton("Hide Search",
					() => Shell.GetSearchHandler(this).SearchBoxVisibility = SearchBoxVisiblity.Hidden),
				0, 6);

			grid.Children.Add(MakeButton("Collapse Search",
					() => Shell.GetSearchHandler(this).SearchBoxVisibility = SearchBoxVisiblity.Collapsable),
				1, 6);

			grid.Children.Add(MakeButton("Show Search",
					() => Shell.GetSearchHandler(this).SearchBoxVisibility = SearchBoxVisiblity.Expanded),
				2, 6);

			grid.Children.Add(MakeButton("Set Back",
					() => Shell.SetBackButtonBehavior(this, new BackButtonBehavior()
					{
						IconOverride = "calculator.png"
					})),
				0, 7);

			grid.Children.Add(MakeButton("Clear Back",
					() => Shell.SetBackButtonBehavior(this, null)),
				1, 7);

			grid.Children.Add(MakeButton("Disable Tab",
					() => ((ShellTabItem)Parent).IsEnabled = false),
				2, 7);

			grid.Children.Add(MakeButton("Enable Tab",
					() => ((ShellTabItem)Parent).IsEnabled = true),
				0, 8);

			grid.Children.Add(MakeButton("Enable Search",
					() => Shell.GetSearchHandler(this).IsSearchEnabled = true),
				1, 8);

			grid.Children.Add(MakeButton("Disable Search",
					() => Shell.GetSearchHandler(this).IsSearchEnabled = false),
				2, 8);

			grid.Children.Add(MakeButton("Set Title",
					() => Title = "New Title"),
				0, 9);

			grid.Children.Add(MakeButton("Set Tab Title",
					() => ((ShellTabItem)Parent).Title = "New Title"),
				1, 9);

			grid.Children.Add(MakeButton("Set GroupTitle",
					() => ((ShellItem)Parent.Parent).Title = "New Title"),
				2, 9);

			grid.Children.Add(MakeButton("New Tab Icon",
					() => ((ShellTabItem)Parent).Icon = "calculator.png"),
				0, 10);

			grid.Children.Add(MakeButton("Flyout Disabled",
					() => Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled)),
				1, 10);

			grid.Children.Add(MakeButton("Flyout Collapse",
					() => Shell.SetFlyoutBehavior(this, FlyoutBehavior.Flyout)),
				2, 10);

			grid.Children.Add(MakeButton("Flyout Locked",
					() => Shell.SetFlyoutBehavior(this, FlyoutBehavior.Locked)),
				0, 11);

			grid.Children.Add(MakeButton("Add TitleView",
					() => Shell.SetTitleView(this, new Label {
						BackgroundColor = Color.Purple,
						Margin = new Thickness(5, 10),
						Text = "TITLE VIEW"
					})),
				1, 11);

			grid.Children.Add(MakeButton("Null TitleView",
					() => Shell.SetTitleView(this, null)),
				2, 11);

			Content = new ScrollView { Content = grid };
		}

		private void RemoveTabItem()
		{
			var shellitem = (ShellItem)Parent.Parent;
			shellitem.Items.Remove(shellitem.Items[shellitem.Items.Count - 1]);
		}

		private void AddTabItem()
		{
			var shellitem = (ShellItem)Parent.Parent;
			shellitem.Items.Add(new ShellTabItem
			{
				Route = "newitem",
				Title = "New Item",
				Icon = "calculator.png",
				Content = new UpdatesPage()
			});
		}

		protected void AddSearchHandler(string placeholder, SearchBoxVisiblity visibility)
		{
			var searchHandler = new SearchHandler();

			searchHandler.ClearIconName = "Clear";
			searchHandler.ClearIconHelpText = "Clears the search field text";

			searchHandler.ClearPlaceholderName = "Voice Search";
			searchHandler.ClearPlaceholderHelpText = "Start voice search";

			searchHandler.QueryIconName = "Search";
			searchHandler.QueryIconHelpText = "Press to search app";

			searchHandler.Placeholder = placeholder;
			searchHandler.SearchBoxVisibility = visibility;

			searchHandler.ClearPlaceholderEnabled = true;
			searchHandler.ClearPlaceholderIcon = "mic.png";

			Shell.SetSearchHandler(this, searchHandler);
		}

		protected void RemoveSearchHandler()
		{
			ClearValue(Shell.SearchHandlerProperty);
		}
	}

	public class UpdatesPage : BasePage
	{
		public UpdatesPage() : base("Available Updates", Color.Default)
		{
			AddSearchHandler("Search Updates", SearchBoxVisiblity.Collapsable);
		}
	}

	public class InstalledPage : BasePage
	{
		public InstalledPage() : base("Installed Items", Color.Default)
		{
			AddSearchHandler("Search Installed", SearchBoxVisiblity.Collapsable);
		}
	}

	public class LibraryPage : BasePage
	{
		public LibraryPage() : base("My Library", Color.Default)
		{
			AddSearchHandler("Search Apps", SearchBoxVisiblity.Collapsable);
		}
	}

	public class NotificationsPage : BasePage
	{
		public NotificationsPage() : base("Notifications", Color.Default) { }
	}

	public class SubscriptionsPage : BasePage
	{
		public SubscriptionsPage() : base("My Subscriptions", Color.Default) { }
	}

	public class HomePage : BasePage
	{
		public HomePage() : base("Store Home", Color.Default)
		{
			AddSearchHandler("Search Apps", SearchBoxVisiblity.Expanded);
		}
	}

	public class GamesPage : BasePage
	{
		public GamesPage() : base("Games", Color.Default)
		{
			AddSearchHandler("Search Games", SearchBoxVisiblity.Expanded);
		}
	}

	public class MoviesPage : BasePage
	{
		public MoviesPage() : base("Hot Movies", Color.Default)
		{
			AddSearchHandler("Search Movies", SearchBoxVisiblity.Expanded);
		}
	}

	public class BooksPage : BasePage
	{
		public BooksPage() : base("Bookstore", Color.Default)
		{
			AddSearchHandler("Search Books", SearchBoxVisiblity.Expanded);
		}
	}

	public class MusicPage : BasePage
	{
		public MusicPage() : base("Music", Color.Default)
		{
			AddSearchHandler("Search Music", SearchBoxVisiblity.Expanded);
		}
	}

	public class NewsPage : BasePage
	{
		public NewsPage() : base("Newspapers", Color.Default)
		{
			AddSearchHandler("Search Papers", SearchBoxVisiblity.Expanded);
		}
	}

	public class AccountsPage : BasePage
	{
		public AccountsPage() : base("Account Items", Color.Default) { }
	}

	public class WishlistPage : BasePage
	{
		public WishlistPage() : base("My Wishlist", Color.Default) { }
	}
	public class SettingsPage : BasePage
	{
		public SettingsPage() : base("Settings", Color.Default) { }
	}

}

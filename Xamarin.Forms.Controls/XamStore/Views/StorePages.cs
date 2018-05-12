using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Controls.XamStore
{
    public class BasePage : ContentPage
	{
		public BasePage(string title, Color tint)
		{
			Title = title;

			var stack = new StackLayout();
			stack.Padding = 40;

			stack.Children.Add(new Label
			{
				Text = "Welcome to the " + title + " page!",
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalTextAlignment = TextAlignment.Center
			});

			var pushButton = new Button
			{
				Text = "Push Page",
				HorizontalOptions = LayoutOptions.Center
			};
			pushButton.Clicked += (s, e) =>
			{
				Navigation.PushAsync((Page)Activator.CreateInstance(GetType()));
			};
			stack.Children.Add(pushButton);

			var popButton = new Button
			{
				Text = "Pop Page",
				HorizontalOptions = LayoutOptions.Center
			};
			popButton.Clicked += (s, e) =>
			{
				Navigation.PopAsync();
			};
			stack.Children.Add(popButton);

			var popToRootButton = new Button
			{
				Text = "Pop To Root",
				HorizontalOptions = LayoutOptions.Center
			};
			popToRootButton.Clicked += (s, e) =>
			{
				Navigation.PopToRootAsync();
			};
			stack.Children.Add(popToRootButton);

			Content = stack;
		}
	}

	public class InstalledPage : BasePage
	{
		public InstalledPage() : base("Installed Items", Color.Default) {}
	}

	public class LibraryPage : BasePage
	{
		public LibraryPage() : base("My Library", Color.Default) { }
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
		public HomePage() : base("Store Home", Color.Default) { }
	}

	public class GamesPage : BasePage
	{
		public GamesPage() : base("Games", Color.Default) { }
	}

	public class MoviesPage : BasePage
	{
		public MoviesPage() : base("Hot Movies", Color.Default) { }
	}

	public class BooksPage : BasePage
	{
		public BooksPage() : base("Bookstore", Color.Default) { }
	}

	public class MusicPage : BasePage
	{
		public MusicPage() : base("Music", Color.Default) { }
	}

	public class NewsPage : BasePage
	{
		public NewsPage() : base("Newspapers", Color.Default) { }
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

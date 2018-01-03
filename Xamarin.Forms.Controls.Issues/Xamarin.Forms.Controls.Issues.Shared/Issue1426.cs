using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 99991426, "SetHasNavigationBar screen height wrong", PlatformAffected.iOS)]
	public class Issue1426 : TestTabbedPage
	{
		protected override void Init()
		{
			Children.Add(new NavigationPage(new HomePage()) { Title = "Home" });
			Children.Add(new NavigationPage(new ContentPage { Title = "Fixes", Content = new Label() { Text = "coffee.png" } }) { Title = "Fixes" });
			CurrentPage = Children.Where(x => x.Title == "Home").FirstOrDefault();

		}

		class HomePage : ContentPage
		{
			public HomePage()
			{
				var grd = new Grid { BackgroundColor = Color.Brown };
				grd.RowDefinitions.Add(new RowDefinition());
				grd.RowDefinitions.Add(new RowDefinition());
				grd.RowDefinitions.Add(new RowDefinition());

				var boxView = new BoxView { BackgroundColor = Color.Blue };
				grd.Children.Add(boxView, 0, 0);
				var btn = new Button()
				{
					BackgroundColor = Color.Pink,
					Text = "NextButtonID",
					AutomationId ="NextButtonID",
					Command = new Command(async () =>
					{
						var btnPop = new Button { Text = "PopButtonId",AutomationId ="PopButtonId", Command = new Command(async () => await Navigation.PopAsync()) };
						var page = new ContentPage
						{
							Title = "Detail",
							Content = btnPop,
							BackgroundColor = Color.Yellow
						};
						NavigationPage.SetHasNavigationBar(page, false); //This breaks layout when you pop!

						await Navigation.PushAsync(page);
					})
				};

				grd.Children.Add(btn, 0, 1);
				var image = new Image() { Source = "coffee.png",AutomationId ="CoffeeImageId", BackgroundColor = Color.Yellow };
				image.VerticalOptions = LayoutOptions.End;
				grd.Children.Add(image, 0, 2);
				Content = grd;

			}
		}

#if UITEST
		[Test]
		public void Github1426Test ()
		{
			RunningApp.Screenshot ("You can see the coffe mug");
			RunningApp.WaitForElement (q => q.Marked ("CoffeeImageId"));
			RunningApp.WaitForElement (q => q.Marked ("NextButtonID"));
			RunningApp.Tap (q => q.Marked ("NextButtonID"));
			RunningApp.WaitForElement (q => q.Marked ("PopButtonId"));
			RunningApp.Tap (q => q.Marked ("PopButtonId"));
			RunningApp.WaitForElement (q => q.Marked ("CoffeeImageId"));
			RunningApp.Screenshot ("Coffe mug Image is still there on the bottom");
		}
#endif
	}
}
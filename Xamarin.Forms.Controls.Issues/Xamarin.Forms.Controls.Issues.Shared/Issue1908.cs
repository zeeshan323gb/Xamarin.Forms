using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Threading.Tasks;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1908, "Image reuse", PlatformAffected.Android)]
	public class Issue1908 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Button()
						{
							Command = new Command(() =>
							{
								PushAsync(new StackLayoutPage());
							}),
							Text = "Stack Layout Images"
						},
						new Button()
						{
							Command = new Command(() =>
							{
								PushAsync(new CellViewPage());
							}),
							Text = "Cell View Images"
						}
					}
				}
			});
		}

		[Preserve(AllMembers = true)]
		public class StackLayoutPage : ContentPage
		{
			public StackLayoutPage()
			{
				StackLayout listView = new StackLayout();

				for (int i = 0; i < 1000; i++)
				{
					listView.Children.Add(new Image() { Source = "oasis.jpg", ClassId = $"OASIS{i}", AutomationId = $"OASIS{i}" });
				}

				Content = new ScrollView() { Content = listView };

			}

		}

		[Preserve(AllMembers = true)]
		public class CellViewPage : ContentPage
		{
			public CellViewPage()
			{
				var list = new List<object>();
				for (var i = 0; i < 1000; i++)
					list.Add(new object());

				var listView = new ListView
				{
					AutomationId = "ListViewScrollMe",
					ItemsSource = list,
					ItemTemplate = new DataTemplate(() => new ImageCell { ImageSource = "oasis" })
				};
				Content = listView;

				listView.ItemAppearing += async (_, args) =>
				{
					await Task.Delay(1000);
					if(args.Item == list[0])
					{
						listView.ScrollTo(list[list.Count - 1], ScrollToPosition.End, true);
					}
					else if(args.Item == list[list.Count - 1])
					{
						await Task.Delay(1000);
						Content = new Label() { Text = "Success" };
					}
				};
			}
		}


#if UITEST && __ANDROID__
		[Test]
		public void Issue1908Test()
		{
			//"Cell View Images"
			RunningApp.WaitForElement(q => q.Marked("Cell View Images"));
			RunningApp.Tap(q => q.Marked("Cell View Images"));
			RunningApp.WaitForElement(q => q.Marked("Success"));
			RunningApp.Back();

			RunningApp.WaitForElement(q => q.Marked("Stack Layout Images"));
			RunningApp.Tap(q => q.Marked("Stack Layout Images"));
			RunningApp.WaitForElement(q => q.Marked("OASIS1"));
			RunningApp.Back();
			RunningApp.WaitForElement(q => q.Marked("Stack Layout Images"));
		}
#endif

	}
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3763, "BindingContext on TitleView does not update after being initially set")]
	public class Issue3763 : TestNavigationPage
	{
		const string _success = "Success";

		[Preserve(AllMembers = true)]
		public class Model
		{
			public string Title { get; set; } = "Binding Working";
			public string ImageSource { get; } = "coffee.png";
		}

		protected override void Init()
		{
			var image1 = new Image();

			image1.SetBinding(Image.SourceProperty, "ImageSource");
			var page = new ContentPage()
			{
				Content = new Label()
				{
					Text = $"If you see a coffee cup and the word {_success} then this test has passed"
				}
			};

			var title = new Label() { Text = "Failed" };
			title.SetBinding(Label.TextProperty, "Title");

			var layout = new StackLayout()
			{
				Orientation = StackOrientation.Horizontal,
				Children =
					{
						title,
						image1
					}
			};

			page.SetValue(TitleViewProperty, layout);
			PushAsync(page);
			Device.BeginInvokeOnMainThread(() =>
			{
				this.BindingContext = new Model();
				Device.BeginInvokeOnMainThread(() =>
				{
					page.BindingContext = new Model() { Title = _success };
					this.BindingContext = new Model() { Title = "Failed" };
				});
			});
		}

#if UITEST
		[Test]
		public void TitleViewBindingContextUpdates()
		{
			RunningApp.WaitForElement(_success);
		}
#endif
	}
}

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
	[Issue(IssueTracker.Github, 3717, "[Android] ScrollView causes IsClippedToBounds to fail on Android",
		PlatformAffected.Android)]
	[Preserve(AllMembers = true)]
	public class Issue3717 : TestContentPage
	{
		RelativeLayout rl = new RelativeLayout { IsClippedToBounds = true, Padding = new Thickness(0, 0, 0, 0), BackgroundColor = Color.White };
		BoxView bigframeborder = new BoxView() { BackgroundColor = Color.Green };
		RelativeLayout rl2 = new RelativeLayout { IsClippedToBounds = true, Padding = new Thickness(0, 0, 0, 0), BackgroundColor = Color.White, InputTransparent = false };
		Image bigimage = new Image() { BackgroundColor = Color.White, Source = "coffee.png" };
		ScrollView error_sv = new ScrollView { IsVisible = false, IsEnabled = false, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand, BackgroundColor = Color.WhiteSmoke };

		protected override void Init()
		{
			rl.Children.Add(bigframeborder,
				Xamarin.Forms.Constraint.Constant(10),
				Xamarin.Forms.Constraint.Constant(10),
				Xamarin.Forms.Constraint.RelativeToParent((parent) => { return parent.Width - 20; }),
				Xamarin.Forms.Constraint.RelativeToParent((parent) => { return parent.Height - 20; }));
			rl.Children.Add(rl2,
				Xamarin.Forms.Constraint.Constant(15),
				Xamarin.Forms.Constraint.Constant(15),
				Xamarin.Forms.Constraint.RelativeToParent((parent) => { return parent.Width - 30; }),
				Xamarin.Forms.Constraint.RelativeToParent((parent) => { return parent.Height - 30; }));
			rl2.Children.Add(bigimage,
				Xamarin.Forms.Constraint.Constant(0),
				Xamarin.Forms.Constraint.Constant(0),
				Xamarin.Forms.Constraint.RelativeToParent((parent) => { return parent.Width; }),
				Xamarin.Forms.Constraint.RelativeToParent((parent) => { return parent.Height; }));
			rl.Children.Add(error_sv,
				Xamarin.Forms.Constraint.Constant(10),
				Xamarin.Forms.Constraint.Constant(10),
				Xamarin.Forms.Constraint.RelativeToParent((parent) => { return parent.Width - 20; }),
				Xamarin.Forms.Constraint.RelativeToParent((parent) => { return parent.Height - 20; }));

			bigimage.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(o => OnBigImageTapped()) });
			Content = rl;
		}
		public void OnBigImageTapped()
		{
			if (bigimage.Scale > 1)
				bigimage.ScaleTo(1);
			else
				bigimage.ScaleTo(5);
		}
	}
}

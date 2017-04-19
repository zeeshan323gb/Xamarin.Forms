using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Core.UnitTests;
using Xamarin.Forms.FlexLayoutTests;

namespace Xamarin.Forms.FlexLayoutTests
{
	public class FlexLayoutMarginTests : FlexLayoutBaseTestFixture
	{
		[Test]
		public void TestMarginLeft()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.Platform = platform;

			var view1 = new View { Platform = platform, IsPlatformEnabled = true };
			view1.Margin = new Thickness(10, 0, 0, 0);
			view1.WidthRequest = 10;
			layout.Children.Add(view1);

			var measure = layout.Measure(100, 100);
			layout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(100f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(10f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(10f, view1.Width);
			Assert.AreEqual(100f, view1.Height);
		}

		[Test]
		public void TestMarginTop()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.FlexDirection = Flex.FlexDirection.Column;
			layout.Platform = platform;

			var view1 = new View { Platform = platform, IsPlatformEnabled = true };
			view1.Margin = new Thickness(0, 10, 0, 0);
			view1.HeightRequest = 10;
			layout.Children.Add(view1);

			var measure = layout.Measure(100, 100);
			layout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(100f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view1.X);
			Assert.AreEqual(10f, view1.Y);
			Assert.AreEqual(100f, view1.Width);
			Assert.AreEqual(10f, view1.Height);
		}

		[Test]
		public void TestMarginRight()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.JustifyContent = Flex.Justify.FlexEnd;
			layout.Platform = platform;

			var view1 = new View { Platform = platform, IsPlatformEnabled = true };
			view1.Margin = new Thickness(0, 0, 10, 0);
			view1.WidthRequest = 10;
			layout.Children.Add(view1);

			var measure = layout.Measure(100, 100);
			layout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(100f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(80f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(10f, view1.Width);
			Assert.AreEqual(100f, view1.Height);
		}

		[Test]
		public void TestMarginBottom()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.JustifyContent = Flex.Justify.FlexEnd;
			layout.FlexDirection = Flex.FlexDirection.Column;
			layout.Platform = platform;

			var view1 = new View { Platform = platform, IsPlatformEnabled = true };
			view1.Margin = new Thickness(0, 0, 0, 10);
			view1.HeightRequest = 10;
			layout.Children.Add(view1);

			var measure = layout.Measure(100, 100);
			layout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(100f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view1.X);
			Assert.AreEqual(80f, view1.Y);
			Assert.AreEqual(100f, view1.Width);
			Assert.AreEqual(10f, view1.Height);
		}

		[Test]
		public void TestMarginAndFlexRow()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.Platform = platform;

			var view1 = new View { Platform = platform, IsPlatformEnabled = true };
			view1.Margin = new Thickness(10, 0, 10, 0);
			FlexLayout.SetGrow(view1, 1);
			layout.Children.Add(view1);

			var measure = layout.Measure(100, 100);
			layout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(100f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(10f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(80f, view1.Width);
			Assert.AreEqual(100f, view1.Height);
		}

		[Test]
		public void TestMarginAndFlexColumn()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.FlexDirection = Flex.FlexDirection.Column;
			layout.Platform = platform;

			var view1 = new View { Platform = platform, IsPlatformEnabled = true };
			view1.Margin = new Thickness(0, 10, 0, 10);
			FlexLayout.SetGrow(view1, 1);
			layout.Children.Add(view1);

			var measure = layout.Measure(100, 100);
			layout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(100f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view1.X);
			Assert.AreEqual(10f, view1.Y);
			Assert.AreEqual(100f, view1.Width);
			Assert.AreEqual(80f, view1.Height);
		}

		[Test]
		public void TestMarginAndStretchRow()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.Platform = platform;

			var view1 = new View { Platform = platform, IsPlatformEnabled = true };
			view1.Margin = new Thickness(0, 10, 0, 10);
			FlexLayout.SetGrow(view1, 1);
			layout.Children.Add(view1);

			var measure = layout.Measure(100, 100);
			layout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(100f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view1.X);
			Assert.AreEqual(10f, view1.Y);
			Assert.AreEqual(100f, view1.Width);
			Assert.AreEqual(80f, view1.Height);
		}

		[Test]
		public void TestMarginAndStretchColumn()
		{

			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.FlexDirection = Flex.FlexDirection.Column;
			layout.Platform = platform;

			var view1 = new View { Platform = platform, IsPlatformEnabled = true };
			view1.Margin = new Thickness(10, 0, 10, 0);
			FlexLayout.SetGrow(view1, 1);
			layout.Children.Add(view1);

			var measure = layout.Measure(100, 100);
			layout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(100f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(10f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(80f, view1.Width);
			Assert.AreEqual(100f, view1.Height);
		}

		[Test]
		public void TestMarginWithSiblingRow()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.Platform = platform;

			var view1 = new View { Platform = platform, IsPlatformEnabled = true };
			view1.Margin = new Thickness(0, 0, 10, 0);
			FlexLayout.SetGrow(view1, 1);
			layout.Children.Add(view1);

			var view2 = new View { Platform = platform, IsPlatformEnabled = true };
			FlexLayout.SetGrow(view2, 1);
			layout.Children.Add(view2);

			var measure = layout.Measure(100, 100);
			layout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(100f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(45f, view1.Width);
			Assert.AreEqual(100f, view1.Height);

			Assert.AreEqual(55f, view2.X);
			Assert.AreEqual(0f, view2.Y);
			Assert.AreEqual(45f, view2.Width);
			Assert.AreEqual(100f, view2.Height);
		}

	}
}

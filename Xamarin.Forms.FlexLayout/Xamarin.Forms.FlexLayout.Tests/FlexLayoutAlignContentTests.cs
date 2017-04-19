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
	[TestFixture]
	public class FlexLayoutAlignContentTests : FlexLayoutBaseTestFixture
	{
		[Test]
		public void TestAlignContentFlexStart()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.Platform = platform;
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.Wrap = Flex.Wrap.Wrap;

			layout.WidthRequest = 130;
			layout.HeightRequest = 100;

			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			view0.HeightRequest = 10;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			view1.HeightRequest = 10;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			view2.HeightRequest = 10;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.WidthRequest = 50;
			view3.HeightRequest = 10;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			view4.HeightRequest = 10;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 130, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(130f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(10f, view0.Height);

			Assert.AreEqual(50f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(10f, view1.Height);

			Assert.AreEqual(0f, view2.X);
			Assert.AreEqual(10f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(10f, view2.Height);

			Assert.AreEqual(50f, view3.X);
			Assert.AreEqual(10f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(10f, view3.Height);

			Assert.AreEqual(0f, view4.X);
			Assert.AreEqual(20f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(10f, view4.Height);
		}

		[Test]
		public void TestAlignContentFlexStartWithoutHeightOnChildren()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.Platform = platform;
			layout.Wrap = Flex.Wrap.Wrap;

			layout.WidthRequest = 100;
			layout.HeightRequest = 100;

			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			view1.HeightRequest = 10;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.HeightRequest = 50;
			view3.WidthRequest = 10;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(100f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(0f, view0.Height);

			Assert.AreEqual(0f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(10f, view1.Height);

			Assert.AreEqual(0f, view2.X);
			Assert.AreEqual(10f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(0f, view2.Height);

			Assert.AreEqual(0f, view3.X);
			Assert.AreEqual(10f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(10f, view3.Height);

			Assert.AreEqual(0f, view4.X);
			Assert.AreEqual(20f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(0f, view4.Height);

		}

		[Test]
		public void TestAlignContentFlexStartWithFlex()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.FlexDirection = Flex.FlexDirection.Column;
			layout.Platform = platform;
			layout.Wrap = Flex.Wrap.Wrap;
			layout.WidthRequest = 100;
			layout.HeightRequest = 120;

			var view0 = new View { IsPlatformEnabled = true };
			FlexLayout.SetGrow(view0, 1);
			FlexLayout.SetBasis(view0, 0);
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			FlexLayout.SetGrow(view1, 1);
			FlexLayout.SetBasis(view1, 0);
			view1.WidthRequest = 50;
			view1.HeightRequest = 10;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			FlexLayout.SetGrow(view3, 1);
			FlexLayout.SetShrink(view3, 1);
			FlexLayout.SetBasis(view3, 0);
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 100, 120));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(100f, layout.Width);
			Assert.AreEqual(120f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(40f, view0.Height);

			Assert.AreEqual(0f, view1.X);
			Assert.AreEqual(40f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(40f, view1.Height);

			Assert.AreEqual(0f, view2.X);
			Assert.AreEqual(80f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(0f, view2.Height);

			Assert.AreEqual(0f, view3.X);
			Assert.AreEqual(10f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(10f, view3.Height);

			Assert.AreEqual(0f, view4.X);
			Assert.AreEqual(120f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(0f, view4.Height);

		}

		[Test]
		public void TestAlignContentFlexEnd()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.Platform = platform;
			layout.FlexDirection = Flex.FlexDirection.Column;
			layout.AlignContent = Flex.Align.FlexEnd;
			layout.Wrap = Flex.Wrap.Wrap;
			layout.WidthRequest = 100;
			layout.HeightRequest = 100;

			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			view0.HeightRequest = 10;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			view1.HeightRequest = 10;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			view2.HeightRequest = 10;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.WidthRequest = 50;
			view3.HeightRequest = 10;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			view4.HeightRequest = 10;
			layout.Children.Add(view4);

			var measure = layout.Measure(100, 100);
			layout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(100f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(10f, view0.Height);

			Assert.AreEqual(0f, view1.X);
			Assert.AreEqual(10f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(10f, view1.Height);

			Assert.AreEqual(0f, view2.X);
			Assert.AreEqual(20f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(10f, view2.Height);

			Assert.AreEqual(0f, view3.X);
			Assert.AreEqual(30f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(10f, view3.Height);

			Assert.AreEqual(0f, view4.X);
			Assert.AreEqual(40f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(10f, view4.Height);
		}

		[Test]
		public void TestAlignContentStretch()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.Platform = platform;
			layout.FlexDirection = Flex.FlexDirection.Column;
			layout.AlignContent = Flex.Align.Stretch;
			layout.Wrap = Flex.Wrap.Wrap;
			layout.WidthRequest = 150;
			layout.HeightRequest = 100;

			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(150f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(0f, view0.Height);

			Assert.AreEqual(0f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(0f, view1.Height);

			Assert.AreEqual(0f, view2.X);
			Assert.AreEqual(0f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(0f, view2.Height);

			Assert.AreEqual(0f, view3.X);
			Assert.AreEqual(0f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(0f, view3.Height);

			Assert.AreEqual(0f, view4.X);
			Assert.AreEqual(0f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(0f, view4.Height);
		}

		[Test]
		public void TestAlignContentSpacebetween()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.Platform = platform;
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.AlignContent = Flex.Align.SpaceBetween;
			layout.Wrap = Flex.Wrap.Wrap;
			layout.WidthRequest = 130;
			layout.HeightRequest = 100;

			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			view0.HeightRequest = 10;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			view1.HeightRequest = 10;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			view2.HeightRequest = 10;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.WidthRequest = 50;
			view3.HeightRequest = 10;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			view4.HeightRequest = 10;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 130, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(130f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(10f, view0.Height);

			Assert.AreEqual(50f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(10f, view1.Height);

			Assert.AreEqual(0f, view2.X);
			Assert.AreEqual(45f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(10f, view2.Height);

			Assert.AreEqual(50f, view3.X);
			Assert.AreEqual(45f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(10f, view3.Height);

			Assert.AreEqual(0f, view4.X);
			Assert.AreEqual(90f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(10f, view4.Height);

		}

		[Test]
		public void TestAlignContentSpacearound()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.Platform = platform;
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.AlignContent = Flex.Align.SpaceAround;
			layout.Wrap = Flex.Wrap.Wrap;
			layout.WidthRequest = 140;
			layout.HeightRequest = 120;

			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			view0.HeightRequest = 10;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			view1.HeightRequest = 10;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			view2.HeightRequest = 10;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.WidthRequest = 50;
			view3.HeightRequest = 10;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			view4.HeightRequest = 10;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 140, 120));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(140f, layout.Width);
			Assert.AreEqual(120f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(15f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(10f, view0.Height);

			Assert.AreEqual(50f, view1.X);
			Assert.AreEqual(15f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(10f, view1.Height);

			Assert.AreEqual(0f, view2.X);
			Assert.AreEqual(55f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(10f, view2.Height);

			Assert.AreEqual(50f, view3.X);
			Assert.AreEqual(55f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(10f, view3.Height);

			Assert.AreEqual(0f, view4.X);
			Assert.AreEqual(95f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(10f, view4.Height);
		}

		[Test]
		public void TestAlignContentStretchRow()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.Platform = platform;
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.AlignContent = Flex.Align.Stretch;
			layout.Wrap = Flex.Wrap.Wrap;
			layout.WidthRequest = 150;
			layout.HeightRequest = 100;

			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(150f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(50f, view0.Height);

			Assert.AreEqual(50f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(50f, view1.Height);

			Assert.AreEqual(100f, view2.X);
			Assert.AreEqual(0f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(50f, view2.Height);

			Assert.AreEqual(0f, view3.X);
			Assert.AreEqual(50f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(50f, view3.Height);

			Assert.AreEqual(50f, view4.X);
			Assert.AreEqual(55f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(50f, view4.Height);
		}

		[Test]
		public void TestAlignContentStretchRowWithChildren()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.Platform = platform;
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.AlignContent = Flex.Align.Stretch;
			layout.Wrap = Flex.Wrap.Wrap;
			layout.WidthRequest = 150;
			layout.HeightRequest = 100;

			var view0 = new FlexLayout { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view0_child0 = new View { IsPlatformEnabled = true };
			FlexLayout.SetGrow(view0_child0, 1);
			FlexLayout.SetShrink(view0_child0, 1);
			FlexLayout.SetBasis(view0_child0, 0);
			view0.Children.Add(view0_child0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(150f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(50f, view0.Height);

			Assert.AreEqual(0f, view0_child0.X);
			Assert.AreEqual(0f, view0_child0.Y);
			Assert.AreEqual(50f, view0_child0.Width);
			Assert.AreEqual(50f, view0_child0.Height);

			Assert.AreEqual(50f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(50f, view1.Height);

			Assert.AreEqual(100f, view2.X);
			Assert.AreEqual(0f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(50f, view2.Height);

			Assert.AreEqual(0f, view3.X);
			Assert.AreEqual(50f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(50f, view3.Height);

			Assert.AreEqual(50f, view4.X);
			Assert.AreEqual(50f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(50f, view4.Height);
		}

		[Test]
		public void TestAlignContentStretchRowWithFlex()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.Platform = platform;
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.AlignContent = Flex.Align.Stretch;
			layout.Wrap = Flex.Wrap.Wrap;
			layout.WidthRequest = 150;
			layout.HeightRequest = 100;

			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			FlexLayout.SetGrow(view1, 1);
			FlexLayout.SetShrink(view1, 1);
			FlexLayout.SetBasis(view1, 0);
			view1.WidthRequest = 50;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			FlexLayout.SetGrow(view3, 1);
			FlexLayout.SetShrink(view3, 1);
			FlexLayout.SetBasis(view3, 0);
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(150f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(100f, view0.Height);

			Assert.AreEqual(50f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(0f, view1.Width);
			Assert.AreEqual(100f, view1.Height);

			Assert.AreEqual(50f, view2.X);
			Assert.AreEqual(0f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(100f, view2.Height);

			Assert.AreEqual(100f, view3.X);
			Assert.AreEqual(0f, view3.Y);
			Assert.AreEqual(0f, view3.Width);
			Assert.AreEqual(100f, view3.Height);

			Assert.AreEqual(100f, view4.X);
			Assert.AreEqual(0f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(100f, view4.Height);
		}

		[Test]
		public void TestAlignContentStretchRowWithFlexNoShrink()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.Platform = platform;
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.AlignContent = Flex.Align.Stretch;
			layout.Wrap = Flex.Wrap.Wrap;
			layout.WidthRequest = 150;
			layout.HeightRequest = 100;

			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			FlexLayout.SetGrow(view1, 1);
			FlexLayout.SetShrink(view1, 1);
			FlexLayout.SetBasis(view1, 0);
			view1.WidthRequest = 50;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			FlexLayout.SetGrow(view3, 1);
			FlexLayout.SetBasis(view3, 0);
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(150f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(100f, view0.Height);

			Assert.AreEqual(50f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(0f, view1.Width);
			Assert.AreEqual(100f, view1.Height);

			Assert.AreEqual(50f, view2.X);
			Assert.AreEqual(0f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(100f, view2.Height);

			Assert.AreEqual(100f, view3.X);
			Assert.AreEqual(0f, view3.Y);
			Assert.AreEqual(0f, view3.Width);
			Assert.AreEqual(100f, view3.Height);

			Assert.AreEqual(100f, view4.X);
			Assert.AreEqual(0f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(100f, view4.Height);
		}

		[Test]
		public void TestAlignContentStretchRowWithMargin()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.Platform = platform;
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.AlignContent = Flex.Align.Stretch;
			layout.Wrap = Flex.Wrap.Wrap;
			layout.WidthRequest = 150;
			layout.HeightRequest = 100;

			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.Margin = new Thickness(10);
			view1.WidthRequest = 50;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.Margin = new Thickness(10);
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(150f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(40f, view0.Height);

			Assert.AreEqual(60f, view1.X);
			Assert.AreEqual(10f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(20f, view1.Height);

			Assert.AreEqual(0f, view2.X);
			Assert.AreEqual(40f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(40f, view2.Height);

			Assert.AreEqual(60f, view3.X);
			Assert.AreEqual(50f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(20f, view3.Height);

			Assert.AreEqual(0f, view4.X);
			Assert.AreEqual(80f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(20f, view4.Height);
		}


		[Test]
		public void TestAlignContentStretchRowWithSingleRow()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.Platform = platform;
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.AlignContent = Flex.Align.Stretch;
			layout.Wrap = Flex.Wrap.Wrap;
			layout.WidthRequest = 150;
			layout.HeightRequest = 100;

			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			layout.Children.Add(view1);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(150f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(100f, view0.Height);

			Assert.AreEqual(50f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(100f, view1.Height);
		}

		[Test]
		public void TestAlignContentStretchRowWithFixedHeight()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout();
			layout.Platform = platform;
			layout.FlexDirection = Flex.FlexDirection.Row;
			layout.AlignContent = Flex.Align.Stretch;
			layout.Wrap = Flex.Wrap.Wrap;
			layout.WidthRequest = 150;
			layout.HeightRequest = 100;

			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			view1.HeightRequest = 60;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(150f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(80f, view0.Height);

			Assert.AreEqual(50f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(60f, view1.Height);

			Assert.AreEqual(100f, view2.X);
			Assert.AreEqual(0f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(80f, view2.Height);

			Assert.AreEqual(0f, view3.X);
			Assert.AreEqual(80f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(20f, view3.Height);

			Assert.AreEqual(50f, view4.X);
			Assert.AreEqual(80f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(20f, view4.Height);
		}
	}
}

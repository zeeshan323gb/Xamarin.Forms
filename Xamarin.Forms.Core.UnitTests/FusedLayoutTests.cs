using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class FusedLayoutTests : BaseTestFixture
	{
		[Test]
		public void AlignCenterTest()
		{
			FusedLayout fusedLayout = new FusedLayout()
			{
				Platform = new UnitPlatform(),
				IsPlatformEnabled = true
			};

			View view1 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view1",
				HeightRequest = 20,
				WidthRequest = 20
			};


			View view2 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view2",
				HeightRequest = 12,
				WidthRequest = 12
			};

			var fuse1 = new Fusion(fusedLayout).Center;
			fusedLayout.Children.Add(view1, FuseProperty.Center, fuse1.Add(new Point(3, -3)));


			FusedLayout.AddFusion(view2, FuseProperty.Center, new Fusion(view1).Center);
			fusedLayout.Children.Add(view2);

			fusedLayout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(20, view1.Height);
			Assert.AreEqual(20, view1.Width);
			Assert.AreEqual(43, view1.X);
			Assert.AreEqual(37, view1.Y);

			Assert.AreEqual(12, view2.Height);
			Assert.AreEqual(12, view2.Width);
			Assert.AreEqual(47, view2.X);
			Assert.AreEqual(41, view2.Y);
		}

		[Test]
		public void VerifySolves()
		{
			FusedLayout fusedLayout = new FusedLayout()
			{
				Platform = new UnitPlatform(),
				IsPlatformEnabled = true
			};

			View view1 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view1"
			};

			Fusion fusion = new Fusion(fusedLayout);
			FusedLayout.AddFusion(view1, FuseProperty.X, fusion);
			FusedLayout.AddFusion(view1, FuseProperty.Y, fusion);
			FusedLayout.AddFusion(view1, FuseProperty.Height, fusion);
			FusedLayout.AddFusion(view1, FuseProperty.Width, fusion);

			fusedLayout.Children.Add(view1);
			fusedLayout.Layout(new Rectangle(0, 0, 50, 100));

			var solveView = FusedLayout.GetSolveView(view1);

			Assert.AreEqual(0, solveView.X);
			Assert.AreEqual(0, solveView.Y);
			Assert.AreEqual(50, solveView.Width);
			Assert.AreEqual(100, solveView.Height);
			Assert.AreEqual(0, solveView.Left);
			Assert.AreEqual(100, solveView.Top);
			Assert.AreEqual(50, solveView.Right);
			Assert.AreEqual(0, solveView.Bottom);
			Assert.AreEqual(new Point(25, 50), solveView.Center);
			Assert.AreEqual(25, solveView.CenterX);
			Assert.AreEqual(50, solveView.CenterY);
			Assert.AreEqual(new Size(50, 100), solveView.Size);

			//	None = 0,
			//X,
			//Y,
			//Width,
			//Height,
			//Left,
			//Top,
			//Right,
			//Bottom,
			//Center,
			//CenterX,
			//CenterY,
			//Size
		}


		[Test]
		public void BasicFuseTest()
		{
			FusedLayout fusedLayout = new FusedLayout()
			{
				Platform = new UnitPlatform(),
				IsPlatformEnabled = true
			};

			View view1 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view1"
			};

			var fuse1 = new Fusion(fusedLayout).X.Add(20);

			View view2 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view2"
			};

			var fuse2 = new Fusion(view1).X.Add(13);


			// need to fix so unconstrained are set to a default measure value
			FusedLayout.AddFusion(view2, FuseProperty.Height, 10);
			FusedLayout.AddFusion(view2, FuseProperty.Width, 10);
			FusedLayout.AddFusion(view2, FuseProperty.Y, 0);

			FusedLayout.AddFusion(view1, FuseProperty.Height, 10);
			FusedLayout.AddFusion(view1, FuseProperty.Width, 10);
			FusedLayout.AddFusion(view1, FuseProperty.Y, 0);

			// view 2 add
			FusedLayout.AddFusion(view2, FuseProperty.X, fuse2);
			fusedLayout.Children.Add(view2);


			// view 1 add
			fusedLayout.Children.Add(view1, FuseProperty.X, fuse1);


			fusedLayout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(20, view1.X);
			Assert.AreEqual(33, view2.X);
		}


		/*
		 * View1.Right.Fixed => 200px (x + width)
			View2.Right => View1.Right 
			View2.Width => View1.Right
			View1.Width => View2.Width */
		[Test]
		public void RightSolve()
		{
			FusedLayout fusedLayout = new FusedLayout()
			{
				Platform = new UnitPlatform(),
				IsPlatformEnabled = true
			};

			View view1 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view1"
			};

			View view2 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view2"
			};

			FusedLayout.AddFusion(view1, FuseProperty.Height, 20);
			FusedLayout.AddFusion(view1, FuseProperty.Y, 0);
			FusedLayout.AddFusion(view2, FuseProperty.Height, 15);
			FusedLayout.AddFusion(view2, FuseProperty.Y, 0);


			var view1Right = new Fusion(view1).Right;
			FusedLayout.AddFusion(view1, FuseProperty.Right, 200);
			FusedLayout.AddFusion(view2, FuseProperty.Right, view1Right);
			FusedLayout.AddFusion(view2, FuseProperty.Width, view1Right);
			FusedLayout.AddFusion(view1, FuseProperty.Width, new Fusion(view2).Width);

			fusedLayout.Children.Add(view2);
			fusedLayout.Children.Add(view1);

			fusedLayout.Layout(new Rectangle(0, 0, 400, 400));


			Assert.AreEqual(200, view1.X + view1.Width);
			Assert.AreEqual(200, view2.X + view2.Width);
			Assert.AreEqual(200, view2.Width);
			Assert.AreEqual(view2.Width, view1.Width);



			//new FusedLayout(view2);
		}




		[Test]
		public void RelatedWidth()
		{
			FusedLayout fusedLayout = new FusedLayout()
			{
				Platform = new UnitPlatform(),
				IsPlatformEnabled = true
			};

			View view1 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view1",
				HeightRequest = 20
			};

			View view2 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view2",
				HeightRequest = 15,
				WidthRequest = 43
			};


			FusedLayout.AddFusion(view1, FuseProperty.X, 0);
			FusedLayout.AddFusion(view1, FuseProperty.Y, 0);

			FusedLayout.AddFusion(view2, FuseProperty.X, 0);
			FusedLayout.AddFusion(view2, FuseProperty.Y, 0);

			FusedLayout.AddFusion(view1, FuseProperty.Height, 20);
			FusedLayout.AddFusion(view1, FuseProperty.Width, new Fusion(view2).Width);


			FusedLayout.AddFusion(view2, FuseProperty.Height, 15);
			FusedLayout.AddFusion(view2, FuseProperty.Width, 43);


			fusedLayout.Children.Add(view1);
			fusedLayout.Children.Add(view2);

			fusedLayout.Layout(new Rectangle(0, 0, 100, 100));


			Assert.AreEqual(43, view2.Width);
			Assert.AreEqual(view2.Width, view1.Width);



			//new FusedLayout(view2);


		}


		[Test]
		public void CenterTestAgainstRightAlignedElement()
		{
			FusedLayout fusedLayout = new FusedLayout()
			{
				Platform = new UnitPlatform(),
				IsPlatformEnabled = true
			};

			View view1 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view1",
				HeightRequest = 20,
				WidthRequest = 20
			};

			View view2 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view2",
				HeightRequest = 6,
				WidthRequest = 6
			};

			// place 20x20 view in bottom right of fused layout
			FusedLayout.AddFusion(view1, FuseProperty.Right, new Fusion(fusedLayout).Right);
			FusedLayout.AddFusion(view1, FuseProperty.Top, new Fusion(fusedLayout).Top);
			FusedLayout.AddFusion(view2, FuseProperty.Center, new Fusion(view1).Center);

			fusedLayout.Children.Add(view1);
			fusedLayout.Children.Add(view2);
			fusedLayout.Layout(new Rectangle(0, 0, 100, 100));


			Assert.AreEqual(80, view1.X);
			Assert.AreEqual(80, view1.Y);


			Assert.AreEqual(87, view2.X);
			Assert.AreEqual(87, view2.Y);
		}
	}
}

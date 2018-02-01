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
		public void SizeFusionTests()
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
				HeightRequest = 15,
				WidthRequest = 25
			};


			View view2 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view2"
			};
 
			FusedLayout.AddFusion(view2, FuseProperty.Left, new Fusion(view1).Size.Height);
			FusedLayout.AddFusion(view2, FuseProperty.Top, new Fusion(view1).Size.Width);


			fusedLayout.Children.Add(view1);
			fusedLayout.Children.Add(view2);

			fusedLayout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(15, view1.Height);
			Assert.AreEqual(25, view1.Width);
			
			
			Assert.AreEqual(view1.Height, view2.X);
			Assert.AreEqual(view1.Width, view2.Y);
				
			
			
		}


		[Test]
		public void IncompatibleFusions()
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


			var fuse1 = new Fusion(fusedLayout).Center;
			fusedLayout.Children.Add(view1, FuseProperty.Left, fuse1);
			fusedLayout.Children.Add(view1, FuseProperty.Left, fuse1);



			fusedLayout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(20, view1.Height);
			Assert.AreEqual(20, view1.Width);
			Assert.AreEqual(50, view1.X);
			Assert.AreEqual(50, view1.Y);
		}

		[Test]
		public void StackingViewsTest()
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
				HeightRequest = 20,
				WidthRequest = 20
			};

			View view3 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view3",
				HeightRequest = 20,
				WidthRequest = 20
			};


			var view1fusion = new Fusion(view1).Height;
			var view2fusion = new Fusion(view2).Height.Add(3);

			FusedLayout.AddFusion(view1, FuseProperty.Top, 0);
			FusedLayout.AddFusion(view2, FuseProperty.Top, view1fusion);
			FusedLayout.AddFusion(view3, FuseProperty.Top, view1fusion.Add(view2fusion));

			fusedLayout.Children.Add(view1);
			fusedLayout.Children.Add(view2);
			fusedLayout.Children.Add(view3);

			fusedLayout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(0, view1.Y);
			Assert.AreEqual(20, view2.Y);
			Assert.AreEqual(43, view3.Y);

			view2.IsVisible = false;
			fusedLayout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(0, view1.Y);
			Assert.AreEqual(20, view3.Y);
		}


		[Test]
		public void AlignLeftToCenterOfLayoutTest()
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


			var fuse1 = new Fusion(fusedLayout).Center;
			fusedLayout.Children.Add(view1, FuseProperty.Left, new Fusion(fusedLayout).CenterX);
			fusedLayout.Children.Add(view1, FuseProperty.Top, new Fusion(fusedLayout).CenterY);



			fusedLayout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(20, view1.Height);
			Assert.AreEqual(20, view1.Width);
			Assert.AreEqual(50, view1.X);
			Assert.AreEqual(50, view1.Y);
		}


		[Test]
		public void ConditionalTest()
		{
			FusedLayout fusedLayout = new FusedLayout()
			{
				Platform = new UnitPlatform(),
				IsPlatformEnabled = true
			};

			View container = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "container",
				HeightRequest = 40
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
				HeightRequest = 20,
				WidthRequest = 20
			};


			View view3 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view2",
				HeightRequest = 20,
				WidthRequest = 20
			};

			FusedLayout.AddFusion(view1, FuseProperty.Top, 0);
			FusedLayout.AddFusion(view1, FuseProperty.Left, 0);

			FusedLayout.AddFusion(view2, FuseProperty.Top, 0);
			FusedLayout.AddFusion(view2, FuseProperty.Left, new Fusion(view1).Right);


			FusedLayout.AddFusion(container, FuseProperty.Width, new Fusion(fusedLayout).Width);




			var targetWrapperSet = new ConditionalTargetWrapperSet(view3);

			ConditionalTargetWrapper landscape = 
				targetWrapperSet.AddConditionalSet(() => (FusedLayout.GetSolveView(container).Width >= 60));

			landscape.AddFusion(FuseProperty.Top, new Fusion(fusedLayout).Top);
			landscape.AddFusion(FuseProperty.Left, new Fusion(view2).Right);


			ConditionalTargetWrapper portrait =
				targetWrapperSet.AddConditionalSet(() => !(FusedLayout.GetSolveView(container).Width >= 60));

			portrait.AddFusion(FuseProperty.Top, new Fusion(view1).Bottom);
			portrait.AddFusion(FuseProperty.Left, new Fusion(fusedLayout).Left);


			FusedLayout.AddFusion(targetWrapperSet);

			fusedLayout.Children.Add(container);
			fusedLayout.Children.Add(view3);
			fusedLayout.Children.Add(view1);
			fusedLayout.Children.Add(view2);

			fusedLayout.Layout(new Rectangle(0, 0, 100, 50));

			Assert.AreEqual(0, view3.Y);
			Assert.AreEqual(40, view3.X);

			fusedLayout.Layout(new Rectangle(0, 0, 50, 100));


			Assert.AreEqual(20, view3.Y);
			Assert.AreEqual(0, view3.X);

		}

		public class ConditionalFusion
		{
			TargetWrapper Wrapper;
		}


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
			FusedLayout.AddFusion(view1, FuseProperty.X, fusion.X);
			FusedLayout.AddFusion(view1, FuseProperty.Y, fusion.Y);
			FusedLayout.AddFusion(view1, FuseProperty.Height, fusion.Height);
			FusedLayout.AddFusion(view1, FuseProperty.Width, fusion.Width);

			fusedLayout.Children.Add(view1);
			fusedLayout.Layout(new Rectangle(0, 0, 50, 100));

			var solveView = FusedLayout.GetSolveView(view1);

			Assert.AreEqual(0, solveView.X);
			Assert.AreEqual(0, solveView.Y);
			Assert.AreEqual(50, solveView.Width);
			Assert.AreEqual(100, solveView.Height);
			Assert.AreEqual(0, solveView.Left);
			Assert.AreEqual(0, solveView.Top);
			Assert.AreEqual(50, solveView.Right);
			Assert.AreEqual(100, solveView.Bottom);
			Assert.AreEqual(new Point(25, 50), solveView.Center);
			Assert.AreEqual(25, solveView.CenterX);
			Assert.AreEqual(50, solveView.CenterY);
			Assert.AreEqual(new Size(50, 100), solveView.Size);
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

			View view3 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view3",
				HeightRequest = 16,
				WidthRequest = 16
			};

			View view4 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view4",
				HeightRequest = 6,
				WidthRequest = 4
			};

			// place 20x20 view in top right of fused layout
			FusedLayout.AddFusion(view1, FuseProperty.Right, new Fusion(fusedLayout).Right);
			FusedLayout.AddFusion(view1, FuseProperty.Bottom, new Fusion(fusedLayout).Bottom);
			FusedLayout.AddFusion(view2, FuseProperty.Center, new Fusion(view1).Center);

			FusedLayout.AddFusion(view3, FuseProperty.Right, new Fusion(view2).Left);
			FusedLayout.AddFusion(view3, FuseProperty.Bottom, new Fusion(fusedLayout).Bottom);


			FusedLayout.AddFusion(view4, FuseProperty.Bottom, new Fusion(fusedLayout).Bottom);

			FusedLayout.AddFusion(
				view4, 
				FuseProperty.Right, 
				new Fusion(view3).Left.Add(new Fusion(view2).Width));


			fusedLayout.Children.Add(view1);
			fusedLayout.Children.Add(view2);
			fusedLayout.Children.Add(view3);
			fusedLayout.Children.Add(view4);

			fusedLayout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(80, view1.X);
			Assert.AreEqual(80, view1.Y);

			Assert.AreEqual(87, view2.X);
			Assert.AreEqual(87, view2.Y);
			Assert.AreEqual(6, view2.Width);

			Assert.AreEqual(71, view3.X); 


			Assert.AreEqual(73, view4.X);

		}

		[Test]
		public void OverFuse_Redundant_X_Properties()
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
			};

			FusedLayout.AddFusion(view1, FuseProperty.X, 10);
			FusedLayout.AddFusion(view1, FuseProperty.X, 20);

			fusedLayout.Children.Add(view1);

			Assert.Throws<ArgumentException>(() => fusedLayout.Layout(new Rectangle(0, 0, 100, 100)));

		}

		[Test]
		public void OverFuse_Redundant_Y_Properties()
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
			};

			FusedLayout.AddFusion(view1, FuseProperty.Y, 10);
			FusedLayout.AddFusion(view1, FuseProperty.Y, 20);

			fusedLayout.Children.Add(view1);

			Assert.Throws<ArgumentException>(() => fusedLayout.Layout(new Rectangle(0, 0, 100, 100)));

		}


		[Test]
		public void OverFuse_X_Left()
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
			};
 
			FusedLayout.AddFusion(view1, FuseProperty.X, 10);
			FusedLayout.AddFusion(view1, FuseProperty.Left, 20);

			fusedLayout.Children.Add(view1);
			
			Assert.Throws<ArgumentException>(() => fusedLayout.Layout(new Rectangle(0, 0, 100, 100)));

		}
		
		[Test]
		public void OverFuseX()
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
			};
 
			FusedLayout.AddFusion(view1, FuseProperty.X, 10);
			FusedLayout.AddFusion(view1, FuseProperty.CenterX, 20);
			FusedLayout.AddFusion(view1, FuseProperty.Right, 20);

			fusedLayout.Children.Add(view1);
			
			Assert.Throws<ArgumentException>(() => fusedLayout.Layout(new Rectangle(0, 0, 100, 100)));

		}
		
		
		
		
		[Test]
		public void OverFuse_Y_Top()
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
			};
 
			FusedLayout.AddFusion(view1, FuseProperty.Y, 10);
			FusedLayout.AddFusion(view1, FuseProperty.Top, 20);

			fusedLayout.Children.Add(view1);
			
			Assert.Throws<ArgumentException>(() => fusedLayout.Layout(new Rectangle(0, 0, 100, 100)));

		}
		
		[Test]
		public void OverFuseY()
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
			};
 
			FusedLayout.AddFusion(view1, FuseProperty.Y, 10);
			FusedLayout.AddFusion(view1, FuseProperty.CenterY, 20);
			FusedLayout.AddFusion(view1, FuseProperty.Bottom, 20);

			fusedLayout.Children.Add(view1);
			
			Assert.Throws<ArgumentException>(() => fusedLayout.Layout(new Rectangle(0, 0, 100, 100)));

		}
	}
}

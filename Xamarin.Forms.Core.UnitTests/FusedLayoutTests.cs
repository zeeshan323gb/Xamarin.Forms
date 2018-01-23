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

			var fuse1 = new Fusion(fusedLayout).Center.Add(new Point(3,-3));

			fusedLayout.Children.Add(view1, FuseProperty.Center, fuse1);
			fusedLayout.Layout(new Rectangle(0, 0, 100, 100));


			Assert.AreEqual(20, view1.Height);
			Assert.AreEqual(20, view1.Width);
			Assert.AreEqual(43, view1.X);
			Assert.AreEqual(37, view1.Y);
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

			// view 2 add
			FusedLayout.AddFusion(view2, FuseProperty.X, fuse2);
			fusedLayout.Children.Add(view2);


			// view 1 add
			fusedLayout.Children.Add(view1, FuseProperty.X, fuse1);


			fusedLayout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(20, view1.X);
			Assert.AreEqual(33, view2.X);
		}

	}
}

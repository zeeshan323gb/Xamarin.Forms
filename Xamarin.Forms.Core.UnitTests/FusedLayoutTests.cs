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
		public void AddingFusesTest()
		{
			FusedLayout fusedLayout = new FusedLayout();
			Button basicButton1 = new Button();
			var fuse1 = new Fuse();
			fusedLayout.Children.Add(basicButton1, fuse1);

			Button basicButton2 = new Button();
			var fuses = FusedLayout.GetFuses(basicButton2);

			var fuse2 = new Fuse() { SourceElement = new Button() };
			fusedLayout.Children.Add(basicButton2, fuse2);


			Assert.AreEqual(fuse1, FusedLayout.GetFuses(basicButton1).Single());
			Assert.AreEqual(fuse2, FusedLayout.GetFuses(basicButton2).Single());
			

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

			var fuse1 = new Fuse()
			{
				Operator = FuseOperator.None,
				SourceElement = fusedLayout,
				SourceProperty = FuseProperty.Center,
				TargetProperty = FuseProperty.Center
			};

			fusedLayout.Children.Add(view1, fuse1);
			fusedLayout.Layout(new Rectangle(0, 0, 100, 100));


			Assert.AreEqual(20, view1.Height);
			Assert.AreEqual(20, view1.Width);
			Assert.AreEqual(40, view1.X);
			Assert.AreEqual(40, view1.Y);
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

			var fuse1 = new Fuse()
			{
				 Operator = FuseOperator.Plus,				 
				 Constant = 20,
				 SourceElement = fusedLayout,
				 SourceProperty = FuseProperty.X,
				 TargetProperty = FuseProperty.X
			};

			View view2 = new View()
			{
				IsPlatformEnabled = true,
				AutomationId = "view2"
			};

			var fuse2 = new Fuse()
			{
				Operator = FuseOperator.Plus,
				Constant = 13,
				SourceElement = view1,
				TargetProperty = FuseProperty.X,
				SourceProperty = FuseProperty.X
			};


			FusedLayout.AddFusion(view2, fuse2);

			// add the dependent fuse first
			fusedLayout.Children.Add(view1, fuse1);
			fusedLayout.Children.Add(view2);

			fusedLayout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(20, view1.X);
			Assert.AreEqual(33, view2.X);
		}

	}
}

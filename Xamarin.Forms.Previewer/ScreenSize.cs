using System;
namespace Xamarin.Forms.Previewer
{
	public class ScreenSize
	{
		public string Description { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		public static readonly ScreenSize[] Sizes = {
			new ScreenSize{
				Description="iPhone X",
				Width=375,
				Height = 812
			},
			new ScreenSize{
				Description="iPhone 6/7/8 Plus",
				Width=414,
				Height = 736
			},
			new ScreenSize{
				Description="iPhone 6/7/8",
				Width=375,
				Height = 667
			},
			new ScreenSize {
				Description = "iPhone 5",
				Width = 320,
				Height = 568
			},
			new ScreenSize{
				Description = "iPhone 4",
				Width = 320,
				Height = 480,
			},
		};
	}
}

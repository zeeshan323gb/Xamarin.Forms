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
				Description="375x812 (iPhone X)",
				Width=375,
				Height = 812
			},
			new ScreenSize{
				Description="412x732 (Pixel 2)",
				Width=412,
				Height = 732
			},
			new ScreenSize{
				Description="414x736 (iPhone 6/7/8 Plus)",
				Width=414,
				Height = 736
			},
			new ScreenSize{
				Description="375x667 (iPhone 6/7/8)",
				Width=375,
				Height = 667
			},
			new ScreenSize {
				Description = "320x568 (iPhone 5)",
				Width = 320,
				Height = 568
			},
			new ScreenSize{
				Description = "320x480 (iPhone 4)",
				Width = 320,
				Height = 480,
			},
		};
	}
}

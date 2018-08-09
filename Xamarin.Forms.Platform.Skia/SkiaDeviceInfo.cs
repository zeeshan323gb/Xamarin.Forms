using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Skia
{
	public class SkiaDeviceInfo : DeviceInfo
	{
		public override Size PixelScreenSize => new Size(1000, 1000);

		public override Size ScaledScreenSize => new Size(1000, 1000);

		public override double ScalingFactor => 1;
	}
}

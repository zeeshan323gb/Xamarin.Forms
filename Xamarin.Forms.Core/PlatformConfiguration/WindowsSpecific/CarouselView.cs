
namespace Xamarin.Forms.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Forms.CarouselView;

	public static class CarouselView
	{
		#region Arrows

		public static readonly BindableProperty ArrowsProperty = BindableProperty.Create("Arrows", typeof(bool), typeof(CarouselView), false);

		public static bool GetArrows(BindableObject element)
		{
			return (bool)element.GetValue(ArrowsProperty);
		}

		public static void SetArrows(BindableObject element, bool Arrows)
		{
			element.SetValue(ArrowsProperty, Arrows);
		}

		public static bool GetArrows(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (bool)config.Element.GetValue(ArrowsProperty);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> SetArrows(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			config.Element.SetValue(ArrowsProperty, value);
			return config;
		}

		#endregion
	}
}
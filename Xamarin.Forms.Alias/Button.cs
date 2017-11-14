namespace Xamarin.Forms.Alias
{
	public class Button : Xamarin.Forms.Button
	{
		public static readonly BindableProperty BorderBrushProperty = BorderColorProperty;

		public Color BorderBrush
		{
			get { return BorderColor; }
			set { BorderColor = value; }
		}


	}
}
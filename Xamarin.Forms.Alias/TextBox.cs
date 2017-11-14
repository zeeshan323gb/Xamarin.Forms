namespace Xamarin.Forms.Alias
{
	public class TextBox : Entry
	{
		public static readonly BindableProperty ForegroundProperty = TextColorProperty;

		public Color Foreground
		{
			get { return TextColor; }
			set { TextColor = value; }
		}

		public static readonly BindableProperty PlaceholderForegroundProperty = PlaceholderColorProperty;

		public Color PlaceholderForeground
		{
			get { return PlaceholderColor; }
			set { PlaceholderColor = value; }
		}

		public static readonly BindableProperty PlaceholderTextProperty = PlaceholderProperty;

		public string PlaceholderText
		{
			get { return Placeholder; }
			set { Placeholder = value; }
		}
	}
}
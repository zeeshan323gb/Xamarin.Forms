namespace Xamarin.Forms.Alias
{
	public class TextBlock : Label
	{
		public static readonly BindableProperty ForegroundProperty = TextColorProperty;

		public Color Foreground
		{
			get { return TextColor; }
			set { TextColor = value; }
		}

		public static readonly BindableProperty TextWrappingProperty = BindableProperty.Create(nameof(TextWrapping), typeof(TextWrapping), typeof(TextBlock), TextWrapping.NoWrap);

		public TextWrapping TextWrapping
		{
			get { return LineBreakMode.ToTextWrapping(); }
			set { LineBreakMode = value.ToLineBreakMode(); }
		}
	}
}
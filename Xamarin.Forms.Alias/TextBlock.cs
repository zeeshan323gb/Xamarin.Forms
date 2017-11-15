using System;

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

		public static readonly BindableProperty TextWrappingProperty = BindableProperty.Create(nameof(TextWrapping), typeof(TextWrapping), typeof(TextBlock), TextWrapping.NoWrap, propertyChanged: TextWrappingChanged);

		static void TextWrappingChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((TextBlock)bindable).LineBreakMode = ((TextWrapping)newValue).ToLineBreakMode();
		}

		public TextWrapping TextWrapping
		{
			get { return (TextWrapping)GetValue(TextWrappingProperty); }
			set { SetValue(TextWrappingProperty, value); }
		}
	}
}
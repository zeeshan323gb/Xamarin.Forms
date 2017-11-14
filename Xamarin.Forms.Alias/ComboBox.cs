using System;

namespace Xamarin.Forms.Alias
{
	public class ComboBox : Picker
	{
		public ComboBox()
		{
			SelectedIndexChanged += (sender, args) => SelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		public static readonly BindableProperty ForegroundProperty = TextColorProperty;

		public Color Foreground
		{
			get { return TextColor; }
			set { TextColor = value; }
		}

		public static readonly BindableProperty HeaderProperty = TitleProperty;

		public string Header
		{
			get { return Title; }
			set { Title = value; }
		}

		public event EventHandler SelectionChanged;
	}
}
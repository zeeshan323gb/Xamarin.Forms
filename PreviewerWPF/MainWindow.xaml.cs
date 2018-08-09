using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Skia;
using Xamarin.Forms.Previewer;

using Rectangle = Xamarin.Forms.Rectangle;

namespace PreviewerWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			Forms.Init();

			InitializeComponent();
			sizeComboBox.SelectedIndex = 0;
			sampleComboBox.SelectedIndex = 0;
			XamlEntry.Text = XamlParser.XamlSimpleString;
			Previewer.Redraw += Previewer_Redraw;
		}

		private async void Previewer_Redraw(object sender, EventArgs e)
		{
			Render();
		}

		private async void XamlEntry_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			Render();
		}

		async void Render()
		{

			var result = XamlParser.ParseXaml(XamlEntry.Text);
			xamlError.Text = result.error?.ToString();
			var size = ScreenSize.Sizes[sizeComboBox.SelectedIndex];
			if(result.element != null)
				await Previewer.Draw(result.element, size.Width, size.Height);
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Render();
		}

		private void sampleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var sample = XamlParser.Samples[Math.Max(sampleComboBox.SelectedIndex,0)];
			XamlEntry.Text = sample.Xaml;
		}
	}
}

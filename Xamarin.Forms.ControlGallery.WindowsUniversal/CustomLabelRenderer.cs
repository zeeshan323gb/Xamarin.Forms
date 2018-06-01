using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Triage.UWP;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Label), typeof(CustomLabelRenderer))]
namespace Triage.UWP
{
	public class CustomLabelRenderer : LabelRenderer
	{

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			base.OnElementChanged(e);
			UpdateTextDecorations();
		}

		void UpdateTextDecorations()
		{
			Control.TextDecorations |= Windows.UI.Text.TextDecorations.Underline;
			Control.TextDecorations |= Windows.UI.Text.TextDecorations.Strikethrough;

			if (!String.IsNullOrWhiteSpace(Control.Text))
				Control.Text = Control.Text; //TextDecorations are not updated in the UI until the text changes
		}
	}
}

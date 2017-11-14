using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xamarin.Forms.Alias
{
	[ContentPropertyAttribute("Child")]
	public class Border : Frame
	{
		public static readonly BindableProperty BorderBrushProperty = OutlineColorProperty;
		public Color BorderBrush
		{
			get { return OutlineColor; }
			set { OutlineColor = value; }
		}
		
		public View Child
		{
			get { return Content; }
			set { Content = value; }
		}
	}
}

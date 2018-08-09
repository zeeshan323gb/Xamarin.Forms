using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xamarin.Forms.Previewer
{
	public interface IPreviewer
	{
		event EventHandler Redraw;
		Task Draw(Element element, int width, int height);
	}
}

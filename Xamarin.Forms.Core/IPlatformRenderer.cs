using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms
{
	// IPR page
	// IVER custom layout
	// IVER button

	public sealed class VisualElementAttribute : Attribute { }

	public class ElementChangedEventArgs : EventArgs
	{
		public ElementChangedEventArgs(Element oldElement, Element newElement)
		{
			OldElement = oldElement;
			NewElement = newElement;
		}

		public Element NewElement { get; private set; }

		public Element OldElement { get; private set; }
	}

	public interface IPlatformRenderer : IDisposable
	{
		VisualElement Element { get; }

		void SetElement(VisualElement element);

		SizeRequest Measure(double widthConstraint, double heightConstraint);

		object Control { get; }

		event EventHandler<ElementChangedEventArgs> ElementChanged;
	}

	public interface IPlatformRenderer<out T> : IPlatformRenderer
	{
		new T Control { get; }
	}
}

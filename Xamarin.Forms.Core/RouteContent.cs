using System;

namespace Xamarin.Forms
{
	public class RouteContent
	{
		Type _type;
		Func<Element> _factory;
		Element _element;

		public RouteContent(string route, Element element)
		{
			Route = route ?? throw new ArgumentNullException(nameof(route));
			_element = element ?? throw new ArgumentNullException(nameof(element));
		}

		public RouteContent(string route, Func<Element> factory)
		{
			Route = route ?? throw new ArgumentNullException(nameof(route));
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		public RouteContent (string route, Type type)
		{
			Route = route ?? throw new ArgumentNullException(nameof(route));
			_type = type ?? throw new ArgumentNullException(nameof(type));
		}

		public bool IsForElement (Element element)
		{
			if (element == null)
				throw new ArgumentNullException(nameof(element));

			return _element == element;
		}

		public string Route { get; private set; }

		public Element GetOrCreateContent()
		{
			if (_type != null)
				return (Element)Activator.CreateInstance(_type);
			if (_factory != null)
				return _factory();
			return _element;
		}
	}
}
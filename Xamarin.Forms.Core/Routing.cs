using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xamarin.Forms
{
	public static class Routing
	{
		public static readonly BindableProperty RouteProperty =
			BindableProperty.CreateAttached("Route", typeof(string), typeof(Routing), null);

		private static Dictionary<string, RouteFactory> _routes = new Dictionary<string, RouteFactory>();

		public static Element GetOrCreateContent(string route, string queryString)
		{
			Element result = null;

			if (_routes.TryGetValue(route, out var content))
				result = content.GetOrCreate();

			if (result == null)
			{
				// okay maybe its a type, we'll try that just to be nice to the user
				var type = Type.GetType(route);
				if (type != null)
					result = Activator.CreateInstance(type) as Element;
			}

			if (result != null)
				SetRoute(result, route);

			return result;
		}

		public static string GetRoute(Element obj)
		{
			return (string)obj.GetValue(RouteProperty);
		}

		public static string GetRouteStringForElement(Element element)
		{
			string route = GetRoute(element);

			if (route != null)
				return route;

			return element.GetType().FullName;
		}

		public static void RegisterRoute(string route, RouteFactory factory)
		{
			if (!ValidateRoute(route))
				throw new ArgumentException("Route must contain only lowercase letters");

			_routes[route] = factory;
		}

		public static void RegisterRoute(string route, Type type)
		{
			if (!ValidateRoute(route))
				throw new ArgumentException("Route must contain only lowercase letters");

			_routes[route] = new TypeRouteFactory(type);
		}

		public static void SetRoute(Element obj, string value)
		{
			obj.SetValue(RouteProperty, value);
		}

		private static bool ValidateRoute(string route)
		{
			// Honestly this could probably be expanded to allow any URI allowable character
			// I just dont want to figure out what that validation looks like.
			// It does however need to be lowercase since uri case sensitivity is a bit touchy
			Regex r = new Regex("^[a-z]*$");
			return r.IsMatch(route);
		}

		private class TypeRouteFactory : RouteFactory
		{
			private readonly Type _type;

			public TypeRouteFactory(Type type)
			{
				_type = type;
			}

			public override Element GetOrCreate()
			{
				return (Element)Activator.CreateInstance(_type);
			}
		}
	}
}
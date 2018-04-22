using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xamarin.Forms
{
	public static class Router
	{
		static Dictionary<string, RouteContent> _routes = new Dictionary<string, RouteContent>();

		public static readonly BindableProperty RouteProperty = 
			BindableProperty.CreateAttached("Route", typeof(string), typeof(Router), null,
				propertyChanged: OnRouteChanged);

		private static void OnRouteChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (!(bindable is Element))
				throw new ArgumentException("Routes must be to Elements");

			var element = (Element)bindable;
			var oldRoute = (string)oldValue;
			var newRoute = (string)newValue;

			if (oldRoute != null && _routes[oldRoute].IsForElement(element))
			{
				_routes.Remove(oldRoute);
			}
			_routes[newRoute] = new RouteContent(newRoute, element);
		}

		public static string GetRoute(Element obj)
		{
			return (string)obj.GetValue(RouteProperty);
		}

		public static void SetRoute(Element obj, string value)
		{
			obj.SetValue(RouteProperty, value);
		}

		public static void RegisterRoute(string route, Func<Element> factory)
		{
			if (!ValidateRoute(route))
				throw new ArgumentException("Route must contain only letters");

			_routes[route] = new RouteContent(route, factory);
		}

		public static void ReigsterRoute(string route, Type type)
		{
			if (!ValidateRoute(route))
				throw new ArgumentException("Route must contain only letters");

			_routes[route] = new RouteContent(route, type);
		}

		public static void RegisterRoute(string route, Element element)
		{
			if (!ValidateRoute(route))
				throw new ArgumentException("Route must contain only letters");

			_routes[route] = new RouteContent(route, element);
		}

		static bool ValidateRoute (string route)
		{
			// Honestly this could probably be expanded to allow any URI allowable character
			// I just dont want to figure out what that validation looks like.
			Regex r = new Regex("^[a-zA-Z]*$");
			return r.IsMatch(route);
		}

		public static RouteContent GetContent (string route)
		{
			if (_routes.TryGetValue(route, out var value))
				return value;
			return null;
		}
	}
}
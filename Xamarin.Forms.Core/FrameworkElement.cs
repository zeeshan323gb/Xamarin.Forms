using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public class FrameworkElement : Element, INavigationProxy
	{
		static readonly BindablePropertyKey NavigationPropertyKey = BindableProperty.CreateReadOnly("Navigation", typeof(INavigation), typeof(VisualElement), default(INavigation));
		public static readonly BindableProperty NavigationProperty = NavigationPropertyKey.BindableProperty;

		internal FrameworkElement()
		{
			Navigation = new NavigationProxy();
		}

		public INavigation Navigation
		{
			get { return (INavigation)GetValue(NavigationProperty); }
			internal set { SetValue(NavigationPropertyKey, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public NavigationProxy NavigationProxy
		{
			get { return Navigation as NavigationProxy; }
		}

		protected override void OnParentSet()
		{
			base.OnParentSet();

			Element parent = Parent;
			INavigationProxy navProxy = null;
			while (parent != null)
			{
				if (parent is INavigationProxy proxy)
				{
					navProxy = proxy;
					break;
				}
				parent = parent.RealParent;
			}

			if (navProxy != null)
			{
				NavigationProxy.Inner = navProxy.NavigationProxy;
			}
			else
			{
				NavigationProxy.Inner = null;
			}
		}
	}
}
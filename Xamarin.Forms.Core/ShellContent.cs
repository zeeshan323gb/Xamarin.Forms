using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[ContentProperty("Content")]
	public class ShellContent : BaseShellItem, IShellContentController
	{
		#region IShellContentController

		Page IShellContentController.GetOrCreateContent()
		{
			var template = ContentTemplate;
			var content = Content;

			Page result = null;
			if (template == null)
			{
				if (content is Page page)
					result = page;
			}
			else
			{
				result = _contentCache ?? (Page)template.CreateContent(content, this);
				_contentCache = result;
			}

			if (result != null && result.Parent != this)
				OnChildAdded(result);

			return result;
		}

		void IShellContentController.RecyclePage(Page page)
		{
			if (_contentCache == page)
			{
				OnChildRemoved(page);
				_contentCache = null;
			}
		}

		Page IShellContentController.Page => _contentCache;

		#endregion IShellContentController

		public static readonly BindableProperty ContentProperty =
			BindableProperty.Create(nameof(Content), typeof(object), typeof(ShellContent), null, BindingMode.OneTime, propertyChanged: OnContentChanged);

		public static readonly BindableProperty ContentTemplateProperty =
			BindableProperty.Create(nameof(ContentTemplate), typeof(DataTemplate), typeof(ShellContent), null, BindingMode.OneTime);

		private Page _contentCache;
		private IList<Element> _logicalChildren = new List<Element>();
		private ReadOnlyCollection<Element> _logicalChildrenReadOnly;

		public object Content
		{
			get { return GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		public DataTemplate ContentTemplate
		{
			get { return (DataTemplate)GetValue(ContentTemplateProperty); }
			set { SetValue(ContentTemplateProperty, value); }
		}

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal => _logicalChildrenReadOnly ?? (_logicalChildrenReadOnly = new ReadOnlyCollection<Element>(_logicalChildren));

		public static implicit operator ShellContent(TemplatedPage page)
		{
			var shellContent = new ShellContent();

			var pageRoute = Routing.GetRoute(page);

			shellContent.Route = Routing.GenerateImplicitRoute(pageRoute);

			shellContent.Content = page;
			shellContent.SetBinding(TitleProperty, new Binding("Title", BindingMode.OneWay));
			shellContent.SetBinding(IconProperty, new Binding("Icon", BindingMode.OneWay));

			return shellContent;
		}

		private static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shellContent = (ShellContent)bindable;
			// This check is wrong but will work for testing
			if (shellContent.ContentTemplate == null)
			{
				// deparent old item
				if (oldValue is Page oldElement)
					shellContent.OnChildRemoved(oldElement);

				// make sure LogicalChildren collection stays consisten
				shellContent._logicalChildren.Clear();
				if (newValue is Page newElement)
				{
					shellContent._logicalChildren.Add((Element)newValue);
					// parent new item
					shellContent.OnChildAdded(newElement);
				}
			}

			if (shellContent.Parent?.Parent is ShellItem shellItem)
			{
				shellItem?.SendStructureChanged();
			}
		}
	}
}
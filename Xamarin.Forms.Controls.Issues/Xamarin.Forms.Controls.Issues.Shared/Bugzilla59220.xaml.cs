using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
#if APP
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 59220, "Dot Binding Syntax", PlatformAffected.All)]
	public partial class Bugzilla59220 : ContentPage, IValueConverter
	{
		public Bugzilla59220()
		{
			InitializeComponent();
			BindingContext = new {
				Source = new List<ListItem> {
					new ListItem {Name="Foo"},
					new ListItem {Name="Bar"},
					new ListItem {Name="Foo"},
					new ListItem {Name="Bar"},
					new ListItem {Name="Foo"},
					new ListItem {Name="Bar"},
					new ListItem {Name="Foo"},
					new ListItem {Name="Bar"},
					new ListItem {Name="Foo"},
					new ListItem {Name="Bar"},
					new ListItem {Name="Foo"},
					new ListItem {Name="Bar"},
				}
			};
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (value as ListItem)?.Name;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public class ListItem
		{
			public string Name { get; set; }
		}
	}
#endif
}

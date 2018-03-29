using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Collections.Generic;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	public class GroupHeaderTemplateSelector : DataTemplateSelector
	{
	[	Preserve(AllMembers = true)]
		public class MyLabel : Label
		{
			public MyLabel()
			{
				SetBinding(TextProperty, new Binding("Name"));
			}
		}

		public GroupHeaderTemplateSelector()
		{
			_dt = new DataTemplate(typeof(MyLabel));
		}

		DataTemplate _dt;

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			return _dt;
		}
	}

	public class Group : List<object>
	{
		public Group(string name)
		{
			Name = name;
		}

		public string Name { get; set; }

		public override string ToString() => Name;
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1939, "ArgumentOutOfRangeException on clearing a group on a grouped ListView on Android", PlatformAffected.Android)]
	public class GitHub1939 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			Content = new ListView(ListViewCachingStrategy.RetainElement)
			{
				ItemsSource = new[] {
					new Group("a") { "0", "1", "2" },
					new Group("b") { "3", "4", "5" }
				},
				IsGroupingEnabled = true,
				GroupHeaderTemplate = new GroupHeaderTemplateSelector()
			};
		}

#if UITEST
		[Test]
		public void Issue1Test ()
		{
			RunningApp.Screenshot ("I am at Issue 1");
			RunningApp.WaitForElement (q => q.Marked ("IssuePageLabel"));
			RunningApp.Screenshot ("I see the Label");
		}
#endif
	}
}
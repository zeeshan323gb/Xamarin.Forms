using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3273, "Drag and drop reordering not firing CollectionChanged", PlatformAffected.UWP)]
	public class Issue3273 : TestContentPage
	{
		public class SortableListView : ListView
		{

		}

		protected override void Init()
		{
			var statusLabel = new Label();
			var actionLabel = new Label();
			var Items = new ObservableCollection<string>
			{
				"apple",
				"orange",
				"pear",
				"trash"
			};

			BindingContext = Items;

			var listView = new SortableListView();
			var listView2 = new ListView();


			// There's a bug in the PR where if you leave this null it throws NRE's
			listView2.ItemsSource = new List<string>();

			int collectionChangedFired = 0;
			Items.CollectionChanged += (_, e) =>
			{
				collectionChangedFired++;
				statusLabel.Text = $"Collection Fired Count: {collectionChangedFired}";
				actionLabel.Text = $"<{DateTime.Now.ToLongTimeString()}> {e.Action} action fired.";
				listView2.ItemsSource = Items.ToList();
			};

			listView.SetBinding(ListView.ItemsSourceProperty, ".");

			Content = new StackLayout
			{
				Children = {
					new Label()
					{
						Text = "Moving things around on the top list should resort the bottom list"
					},
					statusLabel,
					actionLabel,
					new Button {
						Text = "Move items",
						Command = new Command(() =>
						{
							actionLabel.Text = string.Empty;
							statusLabel.Text = "Failed";
							Items.Move(0, 1); })
					},
					listView,
					listView2
				}
			};
		}

#if UITEST
		[Test]
		public void Issue3273Test()
		{
			RunningApp.WaitForElement("Move items");
			RunningApp.Tap("Move items");
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}

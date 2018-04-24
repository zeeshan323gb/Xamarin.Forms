using Foundation;
using System;
using UIKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellTableViewSource : UITableViewSource
	{
		private readonly IShellContext _context;
		private readonly Action<Element> _onElementSelected;
		private string _cellID = "ShellCell";

		public ShellTableViewSource(IShellContext context, Action<Element> onElementSelected)
		{
			_context = context;
			_onElementSelected = onElementSelected;
		}

		public event EventHandler<UIScrollView> ScrolledEvent;

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UIContainerCell cell = (UIContainerCell)tableView.DequeueReusableCell(_cellID);

			var shellItem = _context.Shell.Items[indexPath.Section];
			var tabItem = shellItem.Items[indexPath.Row];

			Element context = shellItem.GroupBehavior == ShellItemGroupBehavior.ShowTabs ? (Element)tabItem : (Element)shellItem;

			if (cell == null)
			{
				var view = (View)_context.Shell.ItemTemplate.CreateContent(context, _context.Shell);
				view.Parent = _context.Shell;
				view.BindingContext = context;
				cell = new UIContainerCell(_cellID, view);
			}
			else
			{
				cell.View.BindingContext = tabItem;
			}

			return cell;
		}

		public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
		{
			cell.BackgroundColor = UIColor.Clear;
		}

		public override nint NumberOfSections(UITableView tableView)
		{
			return _context.Shell.Items.Count;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			int section = indexPath.Section;
			int row = indexPath.Row;

			var shellItem = _context.Shell.Items[section];

			if (row == -1)
			{
				_onElementSelected(shellItem);
			}
			else
			{
				var shellTabItem = shellItem.Items[row];
				_onElementSelected(shellTabItem);
			}
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			if (section >= _context.Shell.Items.Count)
				return 0;
			var shellItem = _context.Shell.Items[(int)section];
			if (shellItem.GroupBehavior == ShellItemGroupBehavior.HideTabs)
				return 1;
			return shellItem.Items.Count;
		}

		public override void Scrolled(UIScrollView scrollView)
		{
			ScrolledEvent?.Invoke(this, scrollView);
		}
	}
}
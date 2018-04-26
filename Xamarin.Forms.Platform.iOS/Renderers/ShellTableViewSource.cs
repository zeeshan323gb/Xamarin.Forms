using Foundation;
using System;
using System.Collections.Generic;
using UIKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellTableViewSource : UITableViewSource
	{
		private readonly IShellContext _context;
		private readonly Action<Element> _onElementSelected;
		private List<List<Element>> _groups;
		private bool _hasMenuItems;

		public ShellTableViewSource(IShellContext context, Action<Element> onElementSelected)
		{
			_context = context;
			_onElementSelected = onElementSelected;
			SetVisualGroups(_context.Shell);
		}

		public event EventHandler<UIScrollView> ScrolledEvent;

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			int section = indexPath.Section;
			int row = indexPath.Row;

			var template = _context.Shell.ItemTemplate;
			if (section == _groups.Count - 1 && _hasMenuItems)
			{
				template = _context.Shell.MenuItemTemplate;
			}
			var context = _groups[indexPath.Section][indexPath.Row];

			var cellId = template.SelectDataTemplate(context, _context.Shell).GetType().FullName;

			var cell = (UIContainerCell)tableView.DequeueReusableCell(cellId);

			if (cell == null)
			{
				var view = (View)template.CreateContent(context, _context.Shell);
				view.Parent = _context.Shell;
				view.BindingContext = context;
				cell = new UIContainerCell(cellId, view);
			}
			else
			{
				cell.View.BindingContext = context;
			}

			return cell;
		}

		public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
		{
			cell.BackgroundColor = UIColor.Clear;
		}

		public override nint NumberOfSections(UITableView tableView)
		{
			return _groups.Count;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			int section = indexPath.Section;
			int row = indexPath.Row;

			var element = _groups[section][row];
			_onElementSelected(element);
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return _groups[(int)section].Count;
		}

		public override nfloat GetHeightForFooter(UITableView tableView, nint section)
		{
			if (section < _groups.Count - 1)
				return 1;
			return 0;
		}

		public override UIView GetViewForFooter(UITableView tableView, nint section)
		{
			return new SeparatorView();
		}

		public override void Scrolled(UIScrollView scrollView)
		{
			ScrolledEvent?.Invoke(this, scrollView);
		}

		private void SetVisualGroups(Shell shell)
		{
			_groups = new List<List<Element>>();
			ShellItemGroupBehavior previous = ShellItemGroupBehavior.HideTabs;
			List<Element> section = null;
			foreach (var shellItem in shell.Items)
			{
				var groupBehavior = shellItem.GroupBehavior;
				if (section == null ||
					groupBehavior == ShellItemGroupBehavior.ShowTabs ||
					previous == ShellItemGroupBehavior.ShowTabs)
				{
					section = new List<Element>();
					_groups.Add(section);
					
					if (groupBehavior == ShellItemGroupBehavior.ShowTabs)
					{
						section.AddRange(shellItem.Items);
					}
					else
					{
						section.Add(shellItem);
					}
				}

				previous = groupBehavior;
			}

			if (shell.MenuItems.Count > 0)
			{
				section = new List<Element>();
				_groups.Add(section);
				section.AddRange(shell.MenuItems);
				_hasMenuItems = true;
			}
			else
			{
				_hasMenuItems = false;
			}
		}

		private class SeparatorView : UIView
		{
			UIView _line;
			public SeparatorView()
			{
				_line = new UIView
				{
					BackgroundColor = UIColor.Black,
					TranslatesAutoresizingMaskIntoConstraints = true,
					Alpha = 0.2f
				};

				Add(_line);
			}

			public override void LayoutSubviews()
			{
				_line.Frame = new CoreGraphics.CGRect(15, 0, Frame.Width - 30, 1);
				base.LayoutSubviews();
			}
		}
	}
}
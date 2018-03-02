using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Foundation;
using UIKit;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

using Specifics = Xamarin.Forms.PlatformConfiguration.iOSSpecific.ListView;

namespace Xamarin.Forms.Platform.iOS
{
	internal class UnevenListViewDataSource : ListViewDataSource
	{
		IVisualElementRenderer _prototype;
		bool _disposed;
		Dictionary<object, Cell> _prototypicalCellByTypeOrDataTemplate = new Dictionary<object, Cell>();

		public UnevenListViewDataSource(ListView list, ListViewRenderer uiTableViewController) : base(list, uiTableViewController)
		{
		}

		public UnevenListViewDataSource(ListViewDataSource source) : base(source)
		{
		}

		internal nfloat GetEstimatedRowHeight(UITableView table)
		{
			if (List.RowHeight != -1)
			{
				// Not even sure we need this case; A list with HasUnevenRows and a RowHeight doesn't make a ton of sense
				// Anyway, no need for an estimate, because the heights we'll use are known
				return 0;
			}

			var templatedItems = TemplatedItemsView.TemplatedItems;

			if (templatedItems.Count == 0)
			{
				// No cells to provide an estimate, use the default row height constant
				return DefaultRowHeight;
			}

			// We're going to base our estimate off of the first cell
			var isGroupingEnabled = List.IsGroupingEnabled;

			if (isGroupingEnabled)
				templatedItems = templatedItems.GetGroup(0);

			object item = null;
			if (templatedItems == null || templatedItems.ListProxy.TryGetValue(0, out item) == false)
				return DefaultRowHeight;

			var firstCell = templatedItems.ActivateContent(0, item);

			// Let's skip this optimization for grouped lists. It will likely cause more trouble than it's worth.
			if (firstCell?.Height > 0 && !isGroupingEnabled)
			{
				// Seems like we've got cells which already specify their height; since the heights are known,
				// we don't need to use estimatedRowHeight at all; zero will disable it and use the known heights.
				// However, not setting the EstimatedRowHeight will drastically degrade performance with large lists.
				// In this case, we will cache the specified cell heights asynchronously, which will be returned one time on
				// table load by EstimatedHeight. 

				return 0;
			}

			return CalculateHeightForCell(table, firstCell);
		}

		internal override void InvalidatePrototypicalCellCache()
		{
			_prototypicalCellByTypeOrDataTemplate.Clear();
		}

		internal Cell GetPrototypicalCell(NSIndexPath indexPath)
		{
			var itemTypeOrDataTemplate = default(object);

			var cachingStrategy = List.CachingStrategy;
			if (cachingStrategy == ListViewCachingStrategy.RecycleElement)
				itemTypeOrDataTemplate = GetDataTemplateForPath(indexPath);

			else if (cachingStrategy == ListViewCachingStrategy.RecycleElementAndDataTemplate)
				itemTypeOrDataTemplate = GetItemTypeForPath(indexPath);

			else // ListViewCachingStrategy.RetainElement
				return GetCellForPath(indexPath);


			Cell protoCell;
			if (!_prototypicalCellByTypeOrDataTemplate.TryGetValue(itemTypeOrDataTemplate, out protoCell))
			{
				// cache prototypical cell by item type; Items of the same Type share
				// the same DataTemplate (this is enforced by RecycleElementAndDataTemplate)
				protoCell = GetCellForPath(indexPath);
				_prototypicalCellByTypeOrDataTemplate[itemTypeOrDataTemplate] = protoCell;
			}

			var templatedItems = GetTemplatedItemsListForPath(indexPath);
			return templatedItems.UpdateContent(protoCell, indexPath.Row);
		}

		public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
		{
			// iOS may ask for a row we have just deleted and hence cannot rebind in order to measure height.
			if (!IsValidIndexPath(indexPath))
				return DefaultRowHeight;

			var cell = GetPrototypicalCell(indexPath);

			if (List.RowHeight == -1 && cell.Height == -1 && cell is ViewCell)
				return UITableView.AutomaticDimension;

			var renderHeight = cell.RenderHeight;
			return renderHeight > 0 ? (nfloat)renderHeight : DefaultRowHeight;
		}

		internal nfloat CalculateHeightForCell(UITableView tableView, Cell cell)
		{
			if (cell is ViewCell viewCell && viewCell.View != null)
			{
				var target = viewCell.View;
				if (_prototype == null)
					_prototype = Platform.CreateRenderer(target);
				else
					_prototype.SetElement(target);

				Platform.SetRenderer(target, _prototype);

				var req = target.Measure(tableView.Frame.Width, double.PositiveInfinity, MeasureFlags.IncludeMargins);

				target.ClearValue(Platform.RendererProperty);
				foreach (Element descendant in target.Descendants())
				{
					IVisualElementRenderer renderer = Platform.GetRenderer(descendant as VisualElement);

					// Clear renderer from descendent; this will not happen in Dispose as normal because we need to
					// unhook the Element from the renderer before disposing it.
					descendant.ClearValue(Platform.RendererProperty);
					renderer?.Dispose();
					renderer = null;
				}

				// Let the EstimatedHeight method know to use this value.
				// Much more efficient than checking the value each time.
				//_useEstimatedRowHeight = true;
				var height = (nfloat)req.Request.Height;
				return height > 1 ? height : DefaultRowHeight;
			}

			var renderHeight = cell.RenderHeight;
			return renderHeight > 0 ? (nfloat)renderHeight : DefaultRowHeight;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (_prototype != null)
				{
					_prototype.Dispose();
					_prototype = null;
				}
			}

			base.Dispose(disposing);
		}
	}

	internal class ListViewDataSource : UITableViewSource
	{
		internal const int DefaultRowHeight = 44;
		const int DefaultItemTemplateId = 1;
		static int s_dataTemplateIncrementer = 2; // lets start at not 0 because
		readonly nfloat _defaultSectionHeight;
		Dictionary<DataTemplate, int> _templateToId = new Dictionary<DataTemplate, int>();
		UITableView _uiTableView;
		ListViewRenderer _uiTableViewController;
		protected ListView List;
		protected ITemplatedItemsView<Cell> TemplatedItemsView => List;
		bool _isDragging;
		bool _selectionFromNative;
		bool _disposed;
		public UITableViewRowAnimation ReloadSectionsAnimation { get; set; } = UITableViewRowAnimation.Automatic;

		public ListViewDataSource(ListViewDataSource source)
		{
			_uiTableViewController = source._uiTableViewController;
			List = source.List;
			_uiTableView = source._uiTableView;
			_defaultSectionHeight = source._defaultSectionHeight;
			_selectionFromNative = source._selectionFromNative;

			Counts = new Dictionary<int, int>();
		}

		public ListViewDataSource(ListView list, ListViewRenderer uiTableViewController)
		{
			_uiTableViewController = uiTableViewController;
			_uiTableView = uiTableViewController.TableView;
			_defaultSectionHeight = DefaultRowHeight;
			List = list;
			List.ItemSelected += OnItemSelected;
			UpdateShortNameListener();

			Counts = new Dictionary<int, int>();
		}

		public Dictionary<int, int> Counts { get; set; }

		UIColor DefaultBackgroundColor
		{
			get { return UIColor.Clear; }
		}

		internal virtual void InvalidatePrototypicalCellCache()
		{
		}

		public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
		{
			_isDragging = false;
			_uiTableViewController.UpdateShowHideRefresh(false);
		}

		public override void DraggingStarted(UIScrollView scrollView)
		{
			_isDragging = true;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			Cell cell;
			UITableViewCell nativeCell;

			var reference = Guid.NewGuid().ToString();
			Performance.Start(reference);

			var cachingStrategy = List.CachingStrategy;
			if (cachingStrategy == ListViewCachingStrategy.RetainElement)
			{
				cell = GetCellForPath(indexPath);
				nativeCell = CellTableViewCell.GetNativeCell(tableView, cell);
			}
			else if ((cachingStrategy & ListViewCachingStrategy.RecycleElement) != 0)
			{
				var id = TemplateIdForPath(indexPath);
				nativeCell = tableView.DequeueReusableCell(ContextActionsCell.Key + id);
				if (nativeCell == null)
				{
					cell = GetCellForPath(indexPath);

					nativeCell = CellTableViewCell.GetNativeCell(tableView, cell, true, id.ToString());
				}
				else
				{
					var templatedList = TemplatedItemsView.TemplatedItems.GetGroup(indexPath.Section);

					cell = (Cell)((INativeElementView)nativeCell).Element;
					cell.SendDisappearing();

					templatedList.UpdateContent(cell, indexPath.Row);
					cell.SendAppearing();
				}
			}
			else
				throw new NotSupportedException();

			if (List.IsSet(Specifics.SeparatorStyleProperty))
			{
				if (List.OnThisPlatform().GetSeparatorStyle() == SeparatorStyle.FullWidth)
				{
					nativeCell.SeparatorInset = UIEdgeInsets.Zero;
					nativeCell.LayoutMargins = UIEdgeInsets.Zero;
					nativeCell.PreservesSuperviewLayoutMargins = false;
				}
			}
			var bgColor = tableView.IndexPathForSelectedRow != null && tableView.IndexPathForSelectedRow.Equals(indexPath) ? UIColor.Clear : DefaultBackgroundColor;
			SetCellBackgroundColor(nativeCell, bgColor);
			PreserveActivityIndicatorState(cell);
			Performance.Stop(reference);
			return nativeCell;
		}

		public override nfloat GetHeightForHeader(UITableView tableView, nint section)
		{
			if (List.IsGroupingEnabled)
			{
				var cell = TemplatedItemsView.TemplatedItems[(int)section];
				nfloat height = (float)cell.RenderHeight;
				if (height == -1)
					height = _defaultSectionHeight;

				return height;
			}

			return 0;
		}

		public override UIView GetViewForHeader(UITableView tableView, nint section)
		{
			UIView view = null;

			if (!List.IsGroupingEnabled)
				return view;

			var cell = TemplatedItemsView.TemplatedItems[(int)section];
			if (cell.HasContextActions)
				throw new NotSupportedException("Header cells do not support context actions");

			var renderer = (CellRenderer)Internals.Registrar.Registered.GetHandlerForObject<IRegisterable>(cell);

			view = new HeaderWrapperView();
			view.AddSubview(renderer.GetCell(cell, null, tableView));

			return view;
		}

		public override void HeaderViewDisplayingEnded(UITableView tableView, UIView headerView, nint section)
		{
			if (!List.IsGroupingEnabled)
				return;

			var cell = TemplatedItemsView.TemplatedItems[(int)section];
			cell.SendDisappearing();
		}

		public override nint NumberOfSections(UITableView tableView)
		{
			if (List.IsGroupingEnabled)
				return TemplatedItemsView.TemplatedItems.Count;

			return 1;
		}

		public void OnItemSelected(object sender, SelectedItemChangedEventArgs eventArg)
		{
			if (_selectionFromNative)
			{
				_selectionFromNative = false;
				return;
			}

			if (List == null)
				return;

			var location = TemplatedItemsView.TemplatedItems.GetGroupAndIndexOfItem(eventArg.SelectedItem);
			if (location.Item1 == -1 || location.Item2 == -1)
			{
				var selectedIndexPath = _uiTableView.IndexPathForSelectedRow;

				var animate = true;

				if (selectedIndexPath != null)
				{
					if (_uiTableView.CellAt(selectedIndexPath) is ContextActionsCell cell)
					{
						cell.PrepareForDeselect();
						if (cell.IsOpen)
							animate = false;
					}
				}

				if (selectedIndexPath != null)
					_uiTableView.DeselectRow(selectedIndexPath, animate);
				return;
			}

			_uiTableView.SelectRow(NSIndexPath.FromRowSection(location.Item2, location.Item1), true, UITableViewScrollPosition.Middle);
		}

		public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.CellAt(indexPath);
			if (cell == null)
				return;

			SetCellBackgroundColor(cell, DefaultBackgroundColor);
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.CellAt(indexPath);

			if (cell == null)
				return;

			Cell formsCell = null;
			if ((List.CachingStrategy & ListViewCachingStrategy.RecycleElement) != 0)
				formsCell = (Cell)((INativeElementView)cell).Element;

			SetCellBackgroundColor(cell, UIColor.Clear);

			_selectionFromNative = true;

			tableView.EndEditing(true);
			List.NotifyRowTapped(indexPath.Section, indexPath.Row, formsCell);
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			int countOverride;
			if (Counts.TryGetValue((int)section, out countOverride))
			{
				Counts.Remove((int)section);
				return countOverride;
			}

			var templatedItems = TemplatedItemsView.TemplatedItems;
			if (List.IsGroupingEnabled)
			{
				var group = (IList)((IList)templatedItems)[(int)section];
				return group.Count;
			}

			return templatedItems.Count;
		}

		public override void Scrolled(UIScrollView scrollView)
		{
			if (_isDragging && scrollView.ContentOffset.Y < 0)
				_uiTableViewController.UpdateShowHideRefresh(true);
		}

		public override string[] SectionIndexTitles(UITableView tableView)
		{
			var templatedItems = TemplatedItemsView.TemplatedItems;
			if (templatedItems.ShortNames == null)
				return null;

			return templatedItems.ShortNames.ToArray();
		}

		public void Cleanup()
		{
			_selectionFromNative = false;
			_isDragging = false;
		}

		public void UpdateGrouping()
		{
			UpdateShortNameListener();
			_uiTableView.ReloadData();
		}

		protected bool IsValidIndexPath(NSIndexPath indexPath)
		{
			var templatedItems = TemplatedItemsView.TemplatedItems;
			if (List.IsGroupingEnabled)
			{
				var section = indexPath.Section;
				if (section < 0 || section >= templatedItems.Count)
					return false;

				templatedItems = (ITemplatedItemsList<Cell>)((IList)templatedItems)[indexPath.Section];
			}

			return templatedItems.ListProxy.TryGetValue(indexPath.Row, out var _);
		}

		protected ITemplatedItemsList<Cell> GetTemplatedItemsListForPath(NSIndexPath indexPath)
		{
			var templatedItems = TemplatedItemsView.TemplatedItems;
			if (List.IsGroupingEnabled)
				templatedItems = (ITemplatedItemsList<Cell>)((IList)templatedItems)[indexPath.Section];

			return templatedItems;
		}

		protected DataTemplate GetDataTemplateForPath(NSIndexPath indexPath)
		{
			var templatedList = GetTemplatedItemsListForPath(indexPath);
			var item = templatedList.ListProxy[indexPath.Row];
			return templatedList.SelectDataTemplate(item);
		}

		protected Type GetItemTypeForPath(NSIndexPath indexPath)
		{
			var templatedList = GetTemplatedItemsListForPath(indexPath);
			var item = templatedList.ListProxy[indexPath.Row];
			return item.GetType();
		}

		protected Cell GetCellForPath(NSIndexPath indexPath)
		{
			var templatedItems = GetTemplatedItemsListForPath(indexPath);
			var cell = templatedItems[indexPath.Row];
			return cell;
		}

		void OnShortNamesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_uiTableView.ReloadSectionIndexTitles();
		}

		static void SetCellBackgroundColor(UITableViewCell cell, UIColor color)
		{
			var contextCell = cell as ContextActionsCell;
			cell.BackgroundColor = color;
			if (contextCell != null)
				contextCell.ContentCell.BackgroundColor = color;
		}

		int TemplateIdForPath(NSIndexPath indexPath)
		{
			var itemTemplate = List.ItemTemplate;
			var selector = itemTemplate as DataTemplateSelector;
			if (selector == null)
				return DefaultItemTemplateId;

			var templatedList = GetTemplatedItemsListForPath(indexPath);
			var item = templatedList.ListProxy[indexPath.Row];

			itemTemplate = selector.SelectTemplate(item, List);
			int key;
			if (!_templateToId.TryGetValue(itemTemplate, out key))
			{
				s_dataTemplateIncrementer++;
				key = s_dataTemplateIncrementer;
				_templateToId[itemTemplate] = key;
			}
			return key;
		}

		void UpdateShortNameListener()
		{
			var templatedList = TemplatedItemsView.TemplatedItems;
			if (List.IsGroupingEnabled)
			{
				if (templatedList.ShortNames != null)
					((INotifyCollectionChanged)templatedList.ShortNames).CollectionChanged += OnShortNamesCollectionChanged;
			}
			else
			{
				if (templatedList.ShortNames != null)
					((INotifyCollectionChanged)templatedList.ShortNames).CollectionChanged -= OnShortNamesCollectionChanged;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (List != null)
				{
					List.ItemSelected -= OnItemSelected;
					List = null;
				}

				_templateToId = null;
				_uiTableView = null;
				_uiTableViewController = null;
			}

			_disposed = true;

			base.Dispose(disposing);
		}

		void PreserveActivityIndicatorState(Element element)
		{
			if (element == null)
				return;

			if (element is ActivityIndicator activityIndicator)
			{
				var renderer = Platform.GetRenderer(activityIndicator) as ActivityIndicatorRenderer;
				renderer?.PreserveState();
			}
			else
			{
				foreach (Element childElement in (element as IElementController).LogicalChildren)
					PreserveActivityIndicatorState(childElement);
			}
		}
	}
}

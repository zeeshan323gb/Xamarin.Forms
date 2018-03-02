using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using Xamarin.Forms.Internals;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Specifics = Xamarin.Forms.PlatformConfiguration.iOSSpecific.ListView;

namespace Xamarin.Forms.Platform.iOS
{
	public class ListViewRenderer : UITableViewController, IVisualElementRenderer, IEffectControlProvider
	{
		EventTracker _events;
		VisualElementPackager _packager;
		VisualElementTracker _tracker;

		const int DefaultRowHeight = 44;
		ListViewDataSource _dataSource;
		bool _estimatedRowHeight;
		IVisualElementRenderer _headerRenderer;
		IVisualElementRenderer _footerRenderer;

		KeyboardInsetTracker _insetTracker;
		RectangleF _previousFrame;
		ScrollToRequestedEventArgs _requestedScroll;
		UIRefreshControl _refresh;
		ListView ListView => Element as ListView;
		ITemplatedItemsView<Cell> TemplatedItemsView => ListView;
		bool _disposed;
		bool _refreshAdded;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		protected UITableViewRowAnimation InsertRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;
		protected UITableViewRowAnimation DeleteRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;
		protected UITableViewRowAnimation ReloadRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;
		protected UITableViewRowAnimation ReloadSectionsAnimation
		{
			get { return _dataSource.ReloadSectionsAnimation; }
			set { _dataSource.ReloadSectionsAnimation = value; }
		}

		RectangleF Bounds => NativeView.Superview.Bounds;
		UIView Superview => NativeView?.Superview;
		RectangleF Frame => NativeView.Superview.Frame;

		public VisualElement Element { get; private set; }

		public UIView NativeView => Control;

		public UITableView Control => TableView;

		public UIViewController ViewController => this;

		public virtual SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint) => Control.GetSizeRequest(widthConstraint, heightConstraint, DefaultRowHeight, DefaultRowHeight);

		public void SetElement(VisualElement element)
		{
			_requestedScroll = null;

			var oldElement = Element;
			if (oldElement != null)
			{
				var oldListview = oldElement as ListView;
				var headerView = (VisualElement)oldListview.HeaderElement;
				if (headerView != null)
					headerView.MeasureInvalidated -= OnHeaderMeasureInvalidated;

				var footerView = (VisualElement)oldListview.FooterElement;
				if (footerView != null)
					footerView.MeasureInvalidated -= OnFooterMeasureInvalidated;

				oldListview.ScrollToRequested -= OnScrollToRequested;
				var templatedItems = ((ITemplatedItemsView<Cell>)oldListview).TemplatedItems;

				templatedItems.CollectionChanged -= OnCollectionChanged;
				templatedItems.GroupedCollectionChanged -= OnGroupedCollectionChanged;
			}

			Element = element;
			Element.PropertyChanged += OnElementPropertyChanged;

			_tracker = new VisualElementTracker(this);
			_packager = new VisualElementPackager(this);
			_packager.Load();

			_events = new EventTracker(this);
			_events.LoadEvents(Control);


			_insetTracker = new KeyboardInsetTracker(TableView, () => Control.Window, insets => Control.ContentInset = Control.ScrollIndicatorInsets = insets, point =>
			{
				var offset = Control.ContentOffset;
				offset.Y += point.Y;
				Control.SetContentOffset(offset, true);
			});

			if (Forms.IsiOS9OrNewer)
				TableView.CellLayoutMarginsFollowReadableWidth = false;

			_refresh = new UIRefreshControl();
			_refresh.ValueChanged += OnRefreshingChanged;

			Load(ListView);

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

			if (Element != null && !string.IsNullOrEmpty(Element.AutomationId))
				Control.AccessibilityIdentifier = Element.AutomationId;

			element?.SendViewInitialized(Control);
		}

		void Load(ListView listView)
		{
			listView.ScrollToRequested += OnScrollToRequested;
			var templatedItems = ((ITemplatedItemsView<Cell>)listView).TemplatedItems;

			templatedItems.CollectionChanged += OnCollectionChanged;
			templatedItems.GroupedCollectionChanged += OnGroupedCollectionChanged;

			UpdateRowHeight();

			Control.Source = _dataSource = listView.HasUnevenRows ? new UnevenListViewDataSource(listView, this) : new ListViewDataSource(listView, this);

			UpdateEstimatedRowHeight();
			UpdateHeader();
			UpdateFooter();
			UpdatePullToRefreshEnabled();
			UpdateIsRefreshing();
			UpdateSeparatorColor();
			UpdateSeparatorVisibility();

			var selected = listView.SelectedItem;
			if (selected != null)
				_dataSource.OnItemSelected(null, new SelectedItemChangedEventArgs(selected));
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e) => ElementChanged?.Invoke(this, e);

		public void SetElementSize(Size size)
		{
			Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);

			View.LayoutSubviews();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			double height = Bounds.Width;
			double width = Bounds.Height;
			if (_headerRenderer != null)
			{
				var e = _headerRenderer.Element;
				var request = e.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);

				// Time for another story with Jason. Gather round children because the following Math.Ceiling will look like it's completely useless.
				// You will remove it and test and find everything is fiiiiiine, but it is not fine, no it is far from fine. See iOS, or at least iOS 8
				// has an issue where-by if the TableHeaderView happens to NOT be an integer height, it will add padding to the space between the content
				// of the UITableView and the TableHeaderView to the tune of the difference between Math.Ceiling (height) - height. Now this seems fine
				// and when you test it will be, EXCEPT that it does this every time you toggle the visibility of the UITableView causing the spacing to 
				// grow a little each time, which you weren't testing at all were you? So there you have it, the stupid reason we integer align here.
				//
				// The same technically applies to the footer, though that could hardly matter less. We just do it for fun.
				Layout.LayoutChildIntoBoundingRegion(e, new Rectangle(0, 0, width, Math.Ceiling(request.Request.Height)));

				Device.BeginInvokeOnMainThread(() =>
				{
					if (_headerRenderer != null)
						Control.TableHeaderView = _headerRenderer.NativeView;
				});
			}

			if (_footerRenderer != null)
			{
				var e = _footerRenderer.Element;
				var request = e.Measure(width, height, MeasureFlags.IncludeMargins);
				Layout.LayoutChildIntoBoundingRegion(e, new Rectangle(0, 0, width, Math.Ceiling(request.Request.Height)));

				Device.BeginInvokeOnMainThread(() =>
				{
					if (_footerRenderer != null)
						Control.TableFooterView = _footerRenderer.NativeView;
				});
			}

			if (_requestedScroll != null && Superview != null)
			{
				var request = _requestedScroll;
				_requestedScroll = null;
				OnScrollToRequested(this, request);
			}

			Control.Frame = new RectangleF(0, 0, (nfloat)width, (nfloat)height);

			if (_previousFrame != Control.Frame)
			{
				_previousFrame = Control.Frame;
				_insetTracker?.UpdateInsets();
			}
		}

		void DisposeSubviews(UIView view)
		{
			var ver = view as IVisualElementRenderer;

			if (ver == null)
			{
				// VisualElementRenderers should implement their own dispose methods that will appropriately dispose and remove their child views.
				// Attempting to do this work twice could cause a SIGSEGV (only observed in iOS8), so don't do this work here.
				// Non-renderer views, such as separator lines, etc., can be removed here.
				foreach (UIView subView in view.Subviews)
					DisposeSubviews(subView);

				view.RemoveFromSuperview();
			}

			view.Dispose();
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (_insetTracker != null)
				{
					_insetTracker.Dispose();
					_insetTracker = null;
				}

				foreach (UIView subview in View.Subviews)
					DisposeSubviews(subview);

				if (Element != null)
				{
					var templatedItems = TemplatedItemsView.TemplatedItems;
					templatedItems.CollectionChanged -= OnCollectionChanged;
					templatedItems.GroupedCollectionChanged -= OnGroupedCollectionChanged;
				}

				if (_dataSource != null)
				{
					_dataSource.Dispose();
					_dataSource = null;
				}

				if (_headerRenderer != null)
				{
					var platform = _headerRenderer.Element?.Platform as Platform;
					platform?.DisposeModelAndChildrenRenderers(_headerRenderer.Element);
					_headerRenderer = null;
				}
				if (_footerRenderer != null)
				{
					var platform = _footerRenderer.Element?.Platform as Platform;
					platform?.DisposeModelAndChildrenRenderers(_footerRenderer.Element);
					_footerRenderer = null;
				}

				if (ListView?.HeaderElement is VisualElement headerView)
					headerView.MeasureInvalidated -= OnHeaderMeasureInvalidated;
				Control?.TableHeaderView?.Dispose();

				if (ListView?.FooterElement is VisualElement footerView)
					footerView.MeasureInvalidated -= OnFooterMeasureInvalidated;
				Control?.TableFooterView?.Dispose();

				if (_refresh != null)
				{
					_refresh.ValueChanged -= OnRefreshingChanged;
					_refresh.EndRefreshing();
					_refresh.Dispose();
					_refresh = null;
				}
			}

			_disposed = true;

			base.Dispose(disposing);
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Xamarin.Forms.ListView.RowHeightProperty.PropertyName)
				UpdateRowHeight();
			else if (e.PropertyName == Xamarin.Forms.ListView.IsGroupingEnabledProperty.PropertyName)
				_dataSource.UpdateGrouping();
			else if (e.PropertyName == Xamarin.Forms.ListView.HasUnevenRowsProperty.PropertyName)
			{
				_estimatedRowHeight = false;
				Control.Source = _dataSource = ListView.HasUnevenRows ? new UnevenListViewDataSource(_dataSource) : new ListViewDataSource(_dataSource);
			}
			else if (e.PropertyName == Xamarin.Forms.ListView.IsPullToRefreshEnabledProperty.PropertyName)
				UpdatePullToRefreshEnabled();
			else if (e.PropertyName == Xamarin.Forms.ListView.IsRefreshingProperty.PropertyName)
				UpdateIsRefreshing();
			else if (e.PropertyName == Xamarin.Forms.ListView.SeparatorColorProperty.PropertyName)
				UpdateSeparatorColor();
			else if (e.PropertyName == Xamarin.Forms.ListView.SeparatorVisibilityProperty.PropertyName)
				UpdateSeparatorVisibility();
			else if (e.PropertyName == "HeaderElement")
				UpdateHeader();
			else if (e.PropertyName == "FooterElement")
				UpdateFooter();
			else if (e.PropertyName == "RefreshAllowed")
				UpdatePullToRefreshEnabled();
		}

		NSIndexPath[] GetPaths(int section, int index, int count)
		{
			var paths = new NSIndexPath[count];
			for (var i = 0; i < paths.Length; i++)
				paths[i] = NSIndexPath.FromRowSection(index + i, section);

			return paths;
		}

		UITableViewScrollPosition GetScrollPosition(ScrollToPosition position)
		{
			switch (position)
			{
				case ScrollToPosition.Center:
					return UITableViewScrollPosition.Middle;
				case ScrollToPosition.End:
					return UITableViewScrollPosition.Bottom;
				case ScrollToPosition.Start:
					return UITableViewScrollPosition.Top;
				case ScrollToPosition.MakeVisible:
				default:
					return UITableViewScrollPosition.None;
			}
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateItems(e, 0, true);
		}

		void OnFooterMeasureInvalidated(object sender, EventArgs eventArgs)
		{
			double width = NativeView.Bounds.Width;
			if (width == 0)
				return;

			var footerView = (VisualElement)sender;
			var request = footerView.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
			Layout.LayoutChildIntoBoundingRegion(footerView, new Rectangle(0, 0, width, request.Request.Height));

			Control.TableFooterView = _footerRenderer.NativeView;
		}

		void OnGroupedCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var til = (TemplatedItemsList<ItemsView<Cell>, Cell>)sender;

			var templatedItems = TemplatedItemsView.TemplatedItems;
			var groupIndex = templatedItems.IndexOf(til.HeaderContent);
			UpdateItems(e, groupIndex, false);
		}

		void OnHeaderMeasureInvalidated(object sender, EventArgs eventArgs)
		{
			double width = NativeView.Bounds.Width;
			if (width == 0)
				return;

			var headerView = (VisualElement)sender;
			var request = headerView.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
			Layout.LayoutChildIntoBoundingRegion(headerView, new Rectangle(0, 0, width, request.Request.Height));

			Control.TableHeaderView = _headerRenderer.NativeView;
		}

		async void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			if (Superview == null)
			{
				_requestedScroll = e;
				return;
			}

			var position = GetScrollPosition(e.Position);
			var scrollArgs = (ITemplatedItemsListScrollToRequestedEventArgs)e;

			var templatedItems = TemplatedItemsView.TemplatedItems;
			if (ListView.IsGroupingEnabled)
			{
				var result = templatedItems.GetGroupAndIndexOfItem(scrollArgs.Group, scrollArgs.Item);
				if (result.Item1 != -1 && result.Item2 != -1)
					Control.ScrollToRow(NSIndexPath.FromRowSection(result.Item2, result.Item1), position, e.ShouldAnimate);
			}
			else
			{
				var index = templatedItems.GetGlobalIndexOfItem(scrollArgs.Item);
				if (index != -1)
				{
					Control.Layer.RemoveAllAnimations();
					//iOS11 hack
					if (Forms.IsiOS11OrNewer)
					{
						await Task.Delay(1);
					}
					Control.ScrollToRow(NSIndexPath.FromRowSection(index, 0), position, e.ShouldAnimate);
				}
			}
		}

		void UpdateEstimatedRowHeight()
		{
			if (_estimatedRowHeight)
				return;

			// if even rows OR uneven rows but user specified a row height anyway...
			if (!ListView.HasUnevenRows || ListView.RowHeight != -1)
			{
				Control.EstimatedRowHeight = 0;
				_estimatedRowHeight = true;
				return;
			}

			var source = _dataSource as UnevenListViewDataSource;

			// We want to make sure we reset the cached defined row heights whenever this is called.
			// Failing to do this will regress Bugzilla 43313 
			// (strange animation when adding rows with uneven heights)
			//source?.CacheDefinedRowHeights();

			if (source == null)
			{
				// We need to set a default estimated row height, 
				// because re-setting it later(when we have items on the TIL)
				// will cause the UITableView to reload, and throw an Exception
				Control.EstimatedRowHeight = DefaultRowHeight;
				return;
			}

			Control.EstimatedRowHeight = source.GetEstimatedRowHeight(Control);
			_estimatedRowHeight = true;
			return;
		}

		void UpdateFooter()
		{
			var footer = ListView.FooterElement;
			var footerView = (View)footer;

			if (footerView != null)
			{
				if (_footerRenderer != null)
				{
					_footerRenderer.Element.MeasureInvalidated -= OnFooterMeasureInvalidated;
					var reflectableType = _footerRenderer as System.Reflection.IReflectableType;
					var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : _footerRenderer.GetType();
					if (footer != null && rendererType == Internals.Registrar.Registered.GetHandlerTypeForObject(footer))
					{
						_footerRenderer.SetElement(footerView);
						return;
					}
					Control.TableFooterView = null;
					if (_footerRenderer.Element.Platform is Platform platform)
						platform.DisposeModelAndChildrenRenderers(_footerRenderer.Element);
					_footerRenderer.Dispose();
					_footerRenderer = null;
				}

				_footerRenderer = Platform.CreateRenderer(footerView);
				Platform.SetRenderer(footerView, _footerRenderer);

				double width = Bounds.Width;
				var request = footerView.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
				Layout.LayoutChildIntoBoundingRegion(footerView, new Rectangle(0, 0, width, request.Request.Height));

				Control.TableFooterView = _footerRenderer.NativeView;
				footerView.MeasureInvalidated += OnFooterMeasureInvalidated;
			}
			else if (_footerRenderer != null)
			{
				Control.TableFooterView = null;
				_footerRenderer.Element.MeasureInvalidated -= OnFooterMeasureInvalidated;

				if (_footerRenderer.Element.Platform is Platform platform)
					platform.DisposeModelAndChildrenRenderers(_footerRenderer.Element);
				_footerRenderer.Dispose();
				_footerRenderer = null;
			}
		}

		void UpdateHeader()
		{
			var header = ListView.HeaderElement;
			var headerView = (View)header;

			if (headerView != null)
			{
				if (_headerRenderer != null)
				{
					_headerRenderer.Element.MeasureInvalidated -= OnHeaderMeasureInvalidated;
					var reflectableType = _headerRenderer as System.Reflection.IReflectableType;
					var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : _headerRenderer.GetType();
					if (header != null && rendererType == Internals.Registrar.Registered.GetHandlerTypeForObject(header))
					{
						_headerRenderer.SetElement(headerView);
						return;
					}
					Control.TableHeaderView = null;
					if (_headerRenderer.Element.Platform is Platform platform)
						platform.DisposeModelAndChildrenRenderers(_headerRenderer.Element);
					_headerRenderer = null;
				}

				_headerRenderer = Platform.CreateRenderer(headerView);
				// This will force measure to invalidate, which we haven't hooked up to yet because we are smarter!
				Platform.SetRenderer(headerView, _headerRenderer);

				double width = Bounds.Width;
				var request = headerView.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
				Layout.LayoutChildIntoBoundingRegion(headerView, new Rectangle(0, 0, width, request.Request.Height));

				Control.TableHeaderView = _headerRenderer.NativeView;
				headerView.MeasureInvalidated += OnHeaderMeasureInvalidated;
			}
			else if (_headerRenderer != null)
			{
				Control.TableHeaderView = null;
				_headerRenderer.Element.MeasureInvalidated -= OnHeaderMeasureInvalidated;

				if (_headerRenderer.Element.Platform is Platform platform)
					platform.DisposeModelAndChildrenRenderers(_headerRenderer.Element);
				_headerRenderer.Dispose();
				_headerRenderer = null;
			}
		}

		void UpdateIsRefreshing()
		{
			var refreshing = ListView.IsRefreshing;
			UpdateIsRefreshing(refreshing);
		}

		void UpdateItems(NotifyCollectionChangedEventArgs e, int section, bool resetWhenGrouped)
		{
			if (e is NotifyCollectionChangedEventArgsEx exArgs)
				_dataSource.Counts[section] = exArgs.Count;

			var groupReset = resetWhenGrouped && ListView.IsGroupingEnabled;

			if (!groupReset)
			{
				var lastIndex = Control.NumberOfRowsInSection(section);
				if (e.NewStartingIndex > lastIndex || e.OldStartingIndex > lastIndex)
					throw new ArgumentException(
						$"Index '{Math.Max(e.NewStartingIndex, e.OldStartingIndex)}' is greater than the number of rows '{lastIndex}'.");
			}

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:

					UpdateEstimatedRowHeight();
					if (e.NewStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;

					Control.BeginUpdates();
					Control.InsertRows(GetPaths(section, e.NewStartingIndex, e.NewItems.Count), InsertRowsAnimation);
					Control.EndUpdates();

					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;
					Control.BeginUpdates();
					Control.DeleteRows(GetPaths(section, e.OldStartingIndex, e.OldItems.Count), DeleteRowsAnimation);

					Control.EndUpdates();

					if (_estimatedRowHeight && TemplatedItemsView.TemplatedItems.Count == 0)
						_estimatedRowHeight = false;

					break;

				case NotifyCollectionChangedAction.Move:
					if (e.OldStartingIndex == -1 || e.NewStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;
					Control.BeginUpdates();
					for (var i = 0; i < e.OldItems.Count; i++)
					{
						var oldi = e.OldStartingIndex;
						var newi = e.NewStartingIndex;

						if (e.NewStartingIndex < e.OldStartingIndex)
						{
							oldi += i;
							newi += i;
						}

						Control.MoveRow(NSIndexPath.FromRowSection(oldi, section), NSIndexPath.FromRowSection(newi, section));
					}
					Control.EndUpdates();

					if (_estimatedRowHeight && e.OldStartingIndex == 0)
						_estimatedRowHeight = false;

					break;

				case NotifyCollectionChangedAction.Replace:
					if (e.OldStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;
					Control.BeginUpdates();
					Control.ReloadRows(GetPaths(section, e.OldStartingIndex, e.OldItems.Count), ReloadRowsAnimation);
					Control.EndUpdates();

					if (_estimatedRowHeight && e.OldStartingIndex == 0)
						_estimatedRowHeight = false;

					break;

				case NotifyCollectionChangedAction.Reset:
					_estimatedRowHeight = false;
					Control.ReloadData();
					return;
			}
		}

		void UpdatePullToRefreshEnabled()
		{
			var isPullToRequestEnabled = ListView.IsPullToRefreshEnabled && ListView.RefreshAllowed;
			UpdatePullToRefreshEnabled(isPullToRequestEnabled);
		}

		void UpdateRowHeight()
		{
			var rowHeight = ListView.RowHeight;

			if (ListView.HasUnevenRows && rowHeight == -1)
				Control.RowHeight = UITableView.AutomaticDimension;
			else
				Control.RowHeight = rowHeight <= 0 ? DefaultRowHeight : rowHeight;
		}

		void UpdateSeparatorColor()
		{
			var color = ListView.SeparatorColor;
			// ...and Steve said to the unbelievers the separator shall be gray, and gray it was. The unbelievers looked on, and saw that it was good, and 
			// they went forth and documented the default color. The holy scripture still reflects this default.
			// Defined here: https://developer.apple.com/library/ios/documentation/UIKit/Reference/UITableView_Class/#//apple_ref/occ/instp/UITableView/separatorColor
			Control.SeparatorColor = color.ToUIColor(UIColor.Gray);
		}

		void UpdateSeparatorVisibility()
		{
			var visibility = ListView.SeparatorVisibility;
			switch (visibility)
			{
				case SeparatorVisibility.Default:
					Control.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
					break;
				case SeparatorVisibility.None:
					Control.SeparatorStyle = UITableViewCellSeparatorStyle.None;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void RegisterEffect(Effect effect)
		{
			VisualElementRenderer<VisualElement>.RegisterEffect(effect, NativeView);
		}

		public void UpdateIsRefreshing(bool refreshing)
		{
			if (refreshing)
			{
				if (!_refreshAdded)
				{
					RefreshControl = _refresh;
					_refreshAdded = true;
				}

				if (!RefreshControl.Refreshing)
				{
					RefreshControl.BeginRefreshing();

					//hack: when we don't have cells in our UITableView the spinner fails to appear
					CheckContentSize();

					TableView.ScrollRectToVisible(new RectangleF(0, 0, _refresh.Bounds.Width, _refresh.Bounds.Height), true);
				}
			}
			else
			{
				RefreshControl.EndRefreshing();

				if (!ListView.IsPullToRefreshEnabled)
					RemoveRefresh();
			}
		}

		public void UpdatePullToRefreshEnabled(bool pullToRefreshEnabled)
		{
			if (pullToRefreshEnabled)
			{
				if (!_refreshAdded)
				{
					_refreshAdded = true;
					RefreshControl = _refresh;
				}
			}
			// https://bugzilla.xamarin.com/show_bug.cgi?id=52962
			// just because pullToRefresh is being disabled does not mean we should kill an in progress refresh. 
			// Consider the case where:
			//   1. User pulls to refresh
			//   2. App RefreshCommand fires (at this point _refresh.Refreshing is true)
			//   3. RefreshCommand disables itself via a call to ChangeCanExecute which returns false
			//			(maybe the command it's attached to a button the app wants disabled)
			//   4. OnCommandCanExecuteChanged handler sets RefreshAllowed to false because the RefreshCommand is disabled
			//   5. We end up here; A refresh is in progress while being asked to disable pullToRefresh
		}

		public void UpdateShowHideRefresh(bool shouldHide)
		{
			if (ListView.IsPullToRefreshEnabled)
				return;

			if (shouldHide)
				RemoveRefresh();
			else
				UpdateIsRefreshing(ListView.IsRefreshing);
		}

		void CheckContentSize()
		{
			//adding a default height of at least 1 pixel tricks iOS to show the spinner
			var contentSize = TableView.ContentSize;
			if (contentSize.Height == 0)
				TableView.ContentSize = new SizeF(contentSize.Width, 1);
		}

		void OnRefreshingChanged(object sender, EventArgs eventArgs)
		{
			if (RefreshControl.Refreshing)
				ListView.SendRefreshing();
		}

		void RemoveRefresh()
		{
			if (!_refreshAdded)
				return;

			if (RefreshControl.Refreshing)
				RefreshControl.EndRefreshing();

			RefreshControl = null;
			_refreshAdded = false;
		}
	}

	internal class HeaderWrapperView : UIView
	{
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			foreach (var item in Subviews)
				item.Frame = Bounds;
		}
	}
}

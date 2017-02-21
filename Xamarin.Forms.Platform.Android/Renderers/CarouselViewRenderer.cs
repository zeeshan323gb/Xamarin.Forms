using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Java.Lang;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

using AColor = Android.Graphics.Color;
using ARelativeLayout = Android.Widget.RelativeLayout;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class CarouselViewRenderer : ViewRenderer<CarouselView, AView>
	{
		bool _disposed;
		CirclePageIndicator _indicator;
		bool _isRemoving;
		// To avoid triggering Position changed
		bool _isSwiping;
		AView _nativeView;
		ViewPager _viewPager;

		interface PageIndicator : ViewPager.IOnPageChangeListener
		{
			/* Adapted from https://github.com/JakeWharton/ViewPagerIndicator
			 * Copyright 2012 Jake Wharton
			 * Copyright 2011 Patrik Åkerfeldt
			 * Copyright 2011 Francisco Figueiredo Jr.
			 * 
			 * Licensed under the Apache License, Version 2.0 (the "License");
			 * you may not use this file except in compliance with the License.
			 * You may obtain a copy of the License at
			 * 
			 *    http://www.apache.org/licenses/LICENSE-2.0
			 * 
			 * Unless required by applicable law or agreed to in writing, software
			 * distributed under the License is distributed on an "AS IS" BASIS,
			 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
			 * See the License for the specific language governing permissions and
			 * limitations under the License.
			 */

			void NotifyDataSetChanged();

			void SetCurrentItem(int item);

			void SetOnPageChangeListener(ViewPager.IOnPageChangeListener listener);

			void SetViewPager(ViewPager view);

			void SetViewPager(ViewPager view, int initialPosition);
		}

		public async void InsertItem(object item, int position)
		{
			if (Element != null && _viewPager != null && Element.ItemsSource != null)
			{
				if (position > Element.ItemsSource.Count)
					throw new CarouselViewException("Page cannot be inserted at a position bigger than ItemsSource.Count");

				if (position == -1)
					Element.ItemsSource.Add(item);
				else
					Element.ItemsSource.Insert(position, item);

				_viewPager.Adapter.NotifyDataSetChanged();

				await Task.Delay(100);
			}
		}

		public async void RemoveItem(int position)
		{
			if (Element != null && _viewPager != null && Element.ItemsSource != null && Element.ItemsSource?.Count > 0)
			{
				_isRemoving = true;

				if (position > Element.ItemsSource.Count - 1)
					throw new CarouselViewException("Page cannot be removed at a position bigger than ItemsSource.Count - 1");

				if (Element.ItemsSource?.Count == 1)
				{
					Element.ItemsSource.RemoveAt(position);
					_viewPager.Adapter = new PageAdapter(Element, _viewPager);
					_viewPager.SetCurrentItem(Element.Position, false);

					_indicator.SetViewPager(_viewPager);
				}
				else
				{
					if (position == Element.Position)
					{
						var newPos = position - 1;
						if (newPos == -1)
							newPos = 0;

						_isSwiping = true;

						if (position == 0)
						{
							_viewPager.SetCurrentItem(1, Element.AnimateTransition);

							await Task.Delay(100);

							Element.ItemsSource.RemoveAt(position);

							_viewPager.Adapter.NotifyDataSetChanged();
							_viewPager.SetCurrentItem(0, false);

							Element.Position = 0;
						}
						else
						{
							_viewPager.SetCurrentItem(newPos, Element.AnimateTransition);

							await Task.Delay(100);

							Element.ItemsSource.RemoveAt(position);
							if (position == 1)
								_viewPager.Adapter = new PageAdapter(Element, _viewPager);
							else
								_viewPager.Adapter.NotifyDataSetChanged();
							Element.Position = newPos;
						}

						_isSwiping = false;
					}
					else
					{
						Element.ItemsSource.RemoveAt(position);

						if (position == 1)
							_viewPager.Adapter = new PageAdapter(Element, _viewPager);
						else
							_viewPager.Adapter.NotifyDataSetChanged();
					}

					Element.PositionSelected?.Invoke(Element, EventArgs.Empty);
				}

				_isRemoving = false;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				if (_viewPager != null)
				{
					_viewPager.PageSelected -= ViewPager_PageSelected;
					_viewPager.PageScrollStateChanged -= ViewPager_PageScrollStateChanged;

					if (_viewPager.Adapter != null)
						_viewPager.Adapter.Dispose();
					_viewPager.Dispose();
					_viewPager = null;
				}

				_disposed = true;
			}

			try
			{
				base.Dispose(disposing);
			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex.Message);
				return;
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				// Unsubscribe from event handlers and cleanup any resources

				if (_viewPager != null)
				{
					_viewPager.PageSelected -= ViewPager_PageSelected;
					_viewPager.PageScrollStateChanged -= ViewPager_PageScrollStateChanged;
				}

				if (Element != null)
				{
					Element.RemoveAction = null;
					Element.InsertAction = null;
				}
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					// Instantiate the native control and assign it to the Control property with
					// the SetNativeControl method

					var metrics = Resources.DisplayMetrics;
					var interPageSpacing = e.NewElement.OnThisPlatform().GetInterPageSpacing() * metrics.Density;

					var isHorizontal = Element.Orientation == CarouselViewOrientation.Horizontal;
					int indicatorPaddingLeft = 20;
					var indicatorHeight = ViewGroup.LayoutParams.WrapContent;
					var indicatorWidth = ViewGroup.LayoutParams.MatchParent;
					var indicatorAlign = global::Android.Widget.LayoutRules.AlignParentBottom;

					if (isHorizontal)
						_viewPager = new ViewPager(Context);
					else
					{
						_viewPager = new VerticalViewPager(Context);
						indicatorPaddingLeft = 15;
						indicatorHeight = ViewGroup.LayoutParams.MatchParent;
						indicatorWidth = ViewGroup.LayoutParams.WrapContent;
						indicatorAlign = global::Android.Widget.LayoutRules.AlignParentRight;
					}

					_viewPager.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
					_viewPager.PageMargin = (int)interPageSpacing;
					_viewPager.SetBackgroundColor(e.NewElement.OnThisPlatform().GetInterPageSpacingColor().ToAndroid());

					_indicator = new CirclePageIndicator(Context);
					_indicator.SetViewPager(_viewPager);
					_indicator.SetPageColor(e.NewElement.PageIndicatorTintColor.ToAndroid());
					_indicator.SetFillColor(e.NewElement.CurrentPageIndicatorTintColor.ToAndroid());
					_indicator.SetStyle(e.NewElement.IndicatorsShape); // Rounded or Squared
					_indicator.Visibility = e.NewElement.ShowIndicators ? ViewStates.Visible : ViewStates.Gone;
					_indicator.SetPadding(indicatorPaddingLeft, 15, 15, 15);

					if (!isHorizontal)
						_indicator.SetOrientation(CarouselViewOrientation.Vertical);

					var newLayoutParams = new ARelativeLayout.LayoutParams(indicatorWidth, indicatorHeight);
					newLayoutParams.AddRule(indicatorAlign);
					_indicator.LayoutParameters = newLayoutParams;

					var relativeLayout = new ARelativeLayout(Context) { LayoutParameters = new ARelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent) };
					relativeLayout.AddView(_viewPager);
					relativeLayout.AddView(_indicator);

					ConfigPosition();

					_nativeView = relativeLayout;
					SetNativeControl(_nativeView);
				}
				// Configure the control and subscribe to event handlers

				_viewPager.PageSelected += ViewPager_PageSelected;
				_viewPager.PageScrollStateChanged += ViewPager_PageScrollStateChanged;

				e.NewElement.RemoveAction = new Action<int>(RemoveItem);
				e.NewElement.InsertAction = new Action<object, int>(InsertItem);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == CarouselView.WidthProperty.PropertyName)
			{
			}
			else if (e.PropertyName == CarouselView.HeightProperty.PropertyName)
			{
				_viewPager.Adapter = new PageAdapter(Element, _viewPager);
				_viewPager.SetCurrentItem(Element.Position, false);
			}
			else if (e.PropertyName == CarouselView.ShowIndicatorsProperty.PropertyName)
			{
				_indicator.Visibility = Element.ShowIndicators ? ViewStates.Visible : ViewStates.Gone;
			}
			else if (e.PropertyName == CarouselView.ItemsSourceProperty.PropertyName)
			{
				if (Element != null && _viewPager != null)
				{
					ConfigPosition();

					_viewPager.Adapter = new PageAdapter(Element, _viewPager);
					_viewPager.SetCurrentItem(Element.Position, false);

					_indicator.SetViewPager(_viewPager);

					Element.PositionSelected?.Invoke(Element, EventArgs.Empty);
				}
			}
			else if (e.PropertyName == CarouselView.PositionProperty.PropertyName)
			{
				if (Element.Position != -1 && !_isSwiping)
					SetCurrentItem(Element.Position);
			}
		}

		static ViewGroup AddNativeView(View view, Rectangle size)
		{
			if (Platform.GetRenderer(view) == null)
				Platform.SetRenderer(view, Platform.CreateRenderer(view));

			var vRenderer = Platform.GetRenderer(view);

			var viewGroup = vRenderer.ViewGroup;
			vRenderer.Tracker.UpdateLayout();
			var layoutParams = new LayoutParams((int)size.Width, (int)size.Height);
			viewGroup.LayoutParameters = layoutParams;
			view.Layout(size);
			viewGroup.Layout(0, 0, (int)view.WidthRequest, (int)view.HeightRequest);
			return viewGroup;
		}

		void ConfigPosition()
		{
			if (Element == null)
				return;

			_isSwiping = true;
			if (Element.ItemsSource != null)
			{
				if (Element.Position > Element.ItemsSource.Count - 1)
					Element.Position = Element.ItemsSource.Count - 1;

				if (Element.Position == -1)
					Element.Position = 0;
			}
			else
			{
				Element.Position = 0;
			}
			_isSwiping = false;

			if (_indicator != null)
				_indicator.SetSnapPage(Element.Position);
		}

		void SetCurrentItem(int position)
		{
			if (Element != null && _viewPager != null && Element.ItemsSource != null && Element.ItemsSource?.Count > 0)
			{
				if (position > Element.ItemsSource.Count - 1)
					throw new CarouselViewException("Current page index cannot be bigger than ItemsSource.Count - 1");

				_viewPager.SetCurrentItem(Element.Position, Element.AnimateTransition);

				if (!Element.AnimateTransition)
					Element.PositionSelected?.Invoke(Element, EventArgs.Empty);
			}
		}

		void ViewPager_PageScrollStateChanged(object sender, ViewPager.PageScrollStateChangedEventArgs e)
		{
			if (e.State == 0)
			{
				if (!_isRemoving)
					Element.PositionSelected?.Invoke(Element, EventArgs.Empty);
			}
		}

		void ViewPager_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
		{
			_isSwiping = true;
			Element.Position = e.Position;
			_isSwiping = false;
		}

		class CirclePageIndicator : AView, PageIndicator
		{
			/* Adapted from https://github.com/JakeWharton/ViewPagerIndicator
			 * Copyright 2012 Jake Wharton
			 * Copyright 2011 Patrik Åkerfeldt
			 * Copyright 2011 Francisco Figueiredo Jr.
			 * 
			 * Licensed under the Apache License, Version 2.0 (the "License");
			 * you may not use this file except in compliance with the License.
			 * You may obtain a copy of the License at
			 * 
			 *    http://www.apache.org/licenses/LICENSE-2.0
			 * 
			 * Unless required by applicable law or agreed to in writing, software
			 * distributed under the License is distributed on an "AS IS" BASIS,
			 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
			 * See the License for the specific language governing permissions and
			 * limitations under the License.
			 */

			const int HORIZONTAL = (int)CarouselViewOrientation.Horizontal;
			const int VERTICAL = (int)CarouselViewOrientation.Vertical;
			bool _Centered;
			int _CurrentOffset;
			int _CurrentPage;
			ViewPager.IOnPageChangeListener _Listener;
			int _Orientation;
			int _PageSize;
			Paint _PaintFill;
			Paint _PaintPageFill;
			Paint _PaintStroke;
			float _Radius;
			int _ScrollState;
			bool _Snap;
			int _SnapPage;
			ViewPager _ViewPager;
			CarouselViewIndicatorsShape indicatorsStyle = CarouselViewIndicatorsShape.Circle;

			public CirclePageIndicator(Context context) : base(context, null)
			{
				ApplyStyle();
			}

			public CirclePageIndicator(Context context, IAttributeSet attrs) : base(context, attrs)
			{
				ApplyStyle();
			}

			public CirclePageIndicator(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
			{
				ApplyStyle();
			}

			public void NotifyDataSetChanged()
			{
				Invalidate();
			}

			public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
			{
				_CurrentPage = position;
				_CurrentOffset = positionOffsetPixels;
				UpdatePageSize();
				Invalidate();

				if (_Listener != null)
				{
					_Listener.OnPageScrolled(position, positionOffset, positionOffsetPixels);
				}
			}

			public void OnPageScrollStateChanged(int state)
			{
				_ScrollState = state;

				if (_Listener != null)
				{
					_Listener.OnPageScrollStateChanged(state);
				}
			}

			public void OnPageSelected(int position)
			{
				if (_Snap || _ScrollState == ViewPager.ScrollStateIdle)
				{
					_CurrentPage = position;
					SetSnapPage(position);
					Invalidate();
				}

				if (_Listener != null)
				{
					_Listener.OnPageSelected(position);
				}
			}

			public void SetCurrentItem(int item)
			{
				if (_ViewPager == null)
				{
					throw new IllegalStateException("ViewPager has not been bound.");
				}
				_ViewPager.CurrentItem = item;
				_CurrentPage = item;
				Invalidate();
			}

			public void SetFillColor(AColor fillColor)
			{
				_PaintFill.Color = fillColor;
				Invalidate();
			}

			public void SetOnPageChangeListener(ViewPager.IOnPageChangeListener listener)
			{
				_Listener = listener;
			}

			public void SetOrientation(CarouselViewOrientation orientation)
			{
				_Orientation = (int)orientation;
			}

			public void SetPageColor(AColor pageColor)
			{
				_PaintPageFill.Color = pageColor;
				Invalidate();
			}

			public void SetStyle(CarouselViewIndicatorsShape style)
			{
				indicatorsStyle = style;
				Invalidate();
			}

			public void SetViewPager(ViewPager view)
			{
				_ViewPager = view;
				_ViewPager.AddOnPageChangeListener(this);
				UpdatePageSize();
				Invalidate();
			}

			public void SetViewPager(ViewPager view, int initialPosition)
			{
				SetViewPager(view);
				SetCurrentItem(initialPosition);
			}

			internal void SetSnapPage(int snapPage)
			{
				_SnapPage = snapPage;
			}

			protected override void OnDraw(Canvas canvas)
			{
				base.OnDraw(canvas);

				if (_ViewPager == null)
				{
					return;
				}
				int count = _ViewPager.Adapter.Count;
				if (count == 0)
				{
					return;
				}

				if (_CurrentPage >= count)
				{
					SetCurrentItem(count - 1);
					return;
				}

				int longSize;
				int longPaddingBefore;
				int longPaddingAfter;
				int shortPaddingBefore;
				if (_Orientation == HORIZONTAL)
				{
					longSize = Width;
					longPaddingBefore = PaddingLeft;
					longPaddingAfter = PaddingRight;
					shortPaddingBefore = PaddingTop;
				}
				else
				{
					longSize = Height;
					longPaddingBefore = PaddingTop;
					longPaddingAfter = PaddingBottom;
					shortPaddingBefore = PaddingLeft;
				}

				float threeRadius = _Radius * 4; // dots separation
				float shortOffset = shortPaddingBefore + _Radius;
				float longOffset = longPaddingBefore + _Radius;
				if (_Centered)
				{
					longOffset += ((longSize - longPaddingBefore - longPaddingAfter) / 2.0f) - ((count * threeRadius) / 2.0f);
				}

				float dX;
				float dY;

				float pageFillRadius = _Radius;
				if (_PaintStroke.StrokeWidth > 0)
				{
					pageFillRadius -= _PaintStroke.StrokeWidth / 2.0f;
				}

				//Draw stroked circles
				for (int iLoop = 0; iLoop < count; iLoop++)
				{
					float drawLong = longOffset + (iLoop * threeRadius);
					if (_Orientation == HORIZONTAL)
					{
						dX = drawLong;
						dY = shortOffset;
					}
					else
					{
						dX = shortOffset;
						dY = drawLong;
					}

					// Only paint fill if not completely transparent
					if (_PaintPageFill.Alpha > 0)
					{
						switch (indicatorsStyle)
						{
							case CarouselViewIndicatorsShape.Square:
								canvas.DrawRect(dX, dY, dX + (pageFillRadius * 2), dY + (pageFillRadius * 2), _PaintPageFill);
								break;
							default:
								canvas.DrawCircle(dX, dY, pageFillRadius, _PaintPageFill);
								break;
						}
					}

					// Only paint stroke if a stroke width was non-zero
					/*if (pageFillRadius != mRadius)
					{
						switch (indicatorsStyle)
						{
							case CarouselViewIndicatorsShape.Square:
								canvas.DrawRect(dX, dY, dX + (this.mRadius * 2), dY + (this.mRadius * 2), mPaintStroke);
								break;
							default:
								canvas.DrawCircle(dX, dY, mRadius, mPaintStroke);
								break;
						}
					}*/
				}

				//Draw the filled circle according to the current scroll
				float cx = (_Snap ? _SnapPage : _CurrentPage) * threeRadius;
				if (!_Snap && (_PageSize != 0))
				{
					cx += (_CurrentOffset * 1.0f / _PageSize) * threeRadius;
				}
				if (_Orientation == HORIZONTAL)
				{
					dX = longOffset + cx;
					dY = shortOffset;
				}
				else
				{
					dX = shortOffset;
					dY = longOffset + cx;
				}

				switch (indicatorsStyle)
				{
					case CarouselViewIndicatorsShape.Square:
						canvas.DrawRect(dX, dY, dX + (_Radius * 2), dY + (_Radius * 2), _PaintFill);
						break;
					default:
						canvas.DrawCircle(dX, dY, _Radius, _PaintFill);
						break;
				}
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				if (_Orientation == HORIZONTAL)
				{
					SetMeasuredDimension(MeasureLong(widthMeasureSpec), MeasureShort(heightMeasureSpec));
				}
				else
				{
					SetMeasuredDimension(MeasureShort(widthMeasureSpec), MeasureLong(heightMeasureSpec));
				}
			}

			protected override void OnRestoreInstanceState(IParcelable state)
			{
				try
				{
					var bundle = state as Bundle;
					if (bundle != null)
					{
						var superState = (IParcelable)bundle.GetParcelable("base");
						if (superState != null)
							base.OnRestoreInstanceState(superState);
						_CurrentPage = bundle.GetInt("mCurrentPage", 0);
						SetSnapPage(bundle.GetInt("mCurrentPage", 0));
					}
				}
				catch
				{
					base.OnRestoreInstanceState(state);
					// Ignore, this needs to support IParcelable...
				}

				RequestLayout();
			}

			protected override IParcelable OnSaveInstanceState()
			{
				var superState = base.OnSaveInstanceState();
				var state = new Bundle();
				state.PutParcelable("base", superState);
				state.PutInt("mCurrentPage", _CurrentPage);

				return state;
			}

			void ApplyStyle()
			{
				AColor defaultPageColor = AColor.ParseColor("#c0c0c0");
				AColor defaultFillColor = AColor.ParseColor("#808080");
				const int defaultOrientation = (int)CarouselViewOrientation.Horizontal;
				AColor defaultStrokeColor = AColor.ParseColor("#FFDDDDDD"); ;
				const float defaultStrokeWidth = 0;
				const float defaultRadius = 3;
				bool defaultCentered = true;
				bool defaultSnap = true;

				_Centered = defaultCentered;
				SetOrientation(defaultOrientation);
				_PaintPageFill = new Paint(PaintFlags.AntiAlias);
				_PaintPageFill.SetStyle(Paint.Style.Fill);
				_PaintPageFill.Color = defaultPageColor;
				_PaintStroke = new Paint(PaintFlags.AntiAlias);
				_PaintStroke.SetStyle(Paint.Style.Stroke);
				_PaintFill = new Paint(PaintFlags.AntiAlias);
				_PaintFill.SetStyle(Paint.Style.Fill);
				_Snap = defaultSnap;

				_Radius = defaultRadius;
				_PaintFill.Color = defaultFillColor;
				_PaintStroke.Color = defaultStrokeColor;
				_PaintStroke.StrokeWidth = defaultStrokeWidth;
			}

			int MeasureLong(int measureSpec)
			{
				int result = 0;
				var specMode = MeasureSpec.GetMode(measureSpec);
				var specSize = MeasureSpec.GetSize(measureSpec);

				if ((specMode == MeasureSpecMode.Exactly) || (_ViewPager == null) || (_ViewPager.Adapter == null))
				{
					//We were told how big to be
					result = specSize;
				}
				else
				{
					//Calculate the width according the views count
					int count = _ViewPager.Adapter.Count;
					result = (int)(PaddingLeft + PaddingRight
							+ (count * 2 * _Radius) + (count - 1) * _Radius + 1);
					//Respect AT_MOST value if that was what is called for by measureSpec
					if (specMode == MeasureSpecMode.AtMost)
					{
						result = Java.Lang.Math.Min(result, specSize);
					}
				}
				return result;
			}

			int MeasureShort(int measureSpec)
			{
				int result = 0;
				var specMode = MeasureSpec.GetMode(measureSpec);
				var specSize = MeasureSpec.GetSize(measureSpec);

				if (specMode == MeasureSpecMode.Exactly)
				{
					//We were told how big to be
					result = specSize;
				}
				else
				{
					//Measure the height
					result = (int)(2 * _Radius + PaddingTop + PaddingBottom + 1);
					//Respect AT_MOST value if that was what is called for by measureSpec
					if (specMode == MeasureSpecMode.AtMost)
					{
						result = Java.Lang.Math.Min(result, specSize);
					}
				}
				return result;
			}

			void UpdatePageSize()
			{
				if (_ViewPager != null)
				{
					_PageSize = (_Orientation == HORIZONTAL) ? _ViewPager.Width : _ViewPager.Height;
				}
			}
		}

		class DefaultTransformer : Java.Lang.Object, ViewPager.IPageTransformer
		{
			/* Adapted from https://github.com/kaelaela/VerticalViewPager
			* Copyright (C) 2015 Kaelaela
			*
			* Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
			* in compliance with the License. You may obtain a copy of the License at
			*
			* http://www.apache.org/licenses/LICENSE-2.0
			*
			* Unless required by applicable law or agreed to in writing, software distributed under the License
			* is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
			* or implied. See the License for the specific language governing permissions and limitations under
			* the License.
			*/
			public void TransformPage(AView page, float position)
			{
				page.TranslationX = page.Width * -position;
				float yPosition = (float)(position * page.Height);
				page.TranslationY = yPosition;
			}
		}

		class PageAdapter : PagerAdapter
		{
			const string TAG_VIEWS = "TAG_VIEWS";
			CarouselView _Element;
			ViewPager _ViewPager;
			SparseArray<Parcelable> _ViewStates = new SparseArray<Parcelable>();

			public PageAdapter(CarouselView element, ViewPager viewpager)
			{
				_Element = element;
				_ViewPager = viewpager;
			}

			public override int Count
			{
				get
				{
					return _Element.ItemsSource?.Count ?? 0;
				}
			}

			public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object objectValue)
			{
				var pager = (ViewPager)container;
				var view = (ViewGroup)objectValue;
				pager.RemoveView(view);
			}

			public override int GetItemPosition(Java.Lang.Object objectValue)
			{
				var tag = int.Parse(((AView)objectValue).Tag.ToString());
				if (tag == _Element.Position)
					return tag;
				return PagerAdapter.PositionNone;
			}

			public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
			{
				View formsView = null;

				object bindingContext = null;

				if (_Element.ItemsSource != null && _Element.ItemsSource?.Count > 0)
					bindingContext = _Element.ItemsSource.Cast<object>().ElementAt(position);

				var dt = bindingContext as DataTemplate;

				if (dt != null)
				{
					formsView = (View)dt.CreateContent();
				}
				else
				{
					var selector = _Element.ItemTemplate as DataTemplateSelector;
					if (selector != null)
						formsView = (View)selector.SelectTemplate(bindingContext, _Element).CreateContent();
					else
						formsView = (View)_Element.ItemTemplate.CreateContent();

					formsView.BindingContext = bindingContext;
				}

				formsView.Parent = _Element;

				var nativeConverted = AddNativeView(formsView, new Rectangle(0, 0, _Element.Width, _Element.Height));
				nativeConverted.Tag = position;

				var pager = (ViewPager)container;

				nativeConverted.RestoreHierarchyState(_ViewStates);

				pager.AddView(nativeConverted);

				return nativeConverted;
			}

			public override bool IsViewFromObject(AView view, Java.Lang.Object objectValue)
			{
				return view == objectValue;
			}

			public override void RestoreState(IParcelable state, ClassLoader loader)
			{
				var bundle = (Bundle)state;
				bundle.SetClassLoader(loader);
				_ViewStates = (SparseArray<Parcelable>)bundle.GetSparseParcelableArray(TAG_VIEWS);
			}

			public override IParcelable SaveState()
			{
				var count = _ViewPager.ChildCount;
				for (int i = 0; i < count; i++)
				{
					var c = _ViewPager.GetChildAt(i);
					if (c.SaveFromParentEnabled)
					{
						c.SaveHierarchyState(_ViewStates);
					}
				}
				var bundle = new Bundle();
				bundle.PutSparseParcelableArray(TAG_VIEWS, _ViewStates);
				return bundle;
			}
		}

		class VerticalViewPager : ViewPager
		{
			/* Adapted from https://github.com/kaelaela/VerticalViewPager
			* Copyright (C) 2015 Kaelaela
			*
			* Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
			* in compliance with the License. You may obtain a copy of the License at
			*
			* http://www.apache.org/licenses/LICENSE-2.0
			*
			* Unless required by applicable law or agreed to in writing, software distributed under the License
			* is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
			* or implied. See the License for the specific language governing permissions and limitations under
			* the License.
			*/

			public VerticalViewPager(Context context) : base(context, null)
			{
			}

			public VerticalViewPager(Context context, IAttributeSet attrs) : base(context, attrs)
			{
				SetPageTransformer(false, new DefaultTransformer());
				// get rid of the overscroll drawing that happens on the left and right
				OverScrollMode = OverScrollMode.Never;
			}

			public override bool OnInterceptTouchEvent(MotionEvent ev)
			{
				bool intercept = base.OnInterceptTouchEvent(SwapTouchEvent(ev));
				//If not intercept, touch event should not be swapped.
				SwapTouchEvent(ev);
				return intercept;
			}

			public override bool OnTouchEvent(MotionEvent e)
			{
				return base.OnTouchEvent(SwapTouchEvent(e));
			}

			MotionEvent SwapTouchEvent(MotionEvent ev)
			{
				float width = Width;
				float height = Height;

				float swappedX = (ev.GetY() / height) * width;
				float swappedY = (ev.GetX() / width) * height;

				ev.SetLocation(swappedX, swappedY);

				return ev;
			}
		}
	}
}
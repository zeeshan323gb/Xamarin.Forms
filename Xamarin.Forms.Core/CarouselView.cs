using System;
using System.Collections;

namespace Xamarin.Forms
{
	public enum CarouselViewIndicatorsShape
	{
		Circle,
		Square
	}

	public enum CarouselViewOrientation
	{
		Horizontal,
		Vertical
	}

	public class CarouselView : View
	{
		public static readonly BindableProperty AnimateTransitionProperty = BindableProperty.Create(nameof(AnimateTransition), typeof(bool), typeof(CarouselView), true);

		// UWP only
		public static readonly BindableProperty ArrowsProperty = BindableProperty.Create(nameof(Arrows), typeof(bool), typeof(CarouselView), false);

		// iOS only
		public static readonly BindableProperty BouncesProperty = BindableProperty.Create(nameof(Bounces), typeof(bool), typeof(CarouselView), true);

		public static readonly BindableProperty CurrentPageIndicatorTintColorProperty = BindableProperty.Create(nameof(CurrentPageIndicatorTintColor), typeof(Color), typeof(CarouselView), Color.Gray);

		public static readonly BindableProperty IndicatorsShapeProperty = BindableProperty.Create(nameof(IndicatorsShape), typeof(CarouselViewIndicatorsShape), typeof(CarouselView), CarouselViewIndicatorsShape.Circle);

		// Android and iOS only
		public static readonly BindableProperty InterPageSpacingColorProperty = BindableProperty.Create(nameof(InterPageSpacingColor), typeof(Color), typeof(CarouselView), Color.White);

		// Android and iOS only
		public static readonly BindableProperty InterPageSpacingProperty = BindableProperty.Create(nameof(InterPageSpacing), typeof(int), typeof(CarouselView), 0);

		public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(CarouselView), null);

		public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(CarouselView), null);

		public static readonly BindableProperty OrientationProperty = BindableProperty.Create(nameof(Orientation), typeof(CarouselViewOrientation), typeof(CarouselView), CarouselViewOrientation.Horizontal);

		public static readonly BindableProperty PageIndicatorTintColorProperty = BindableProperty.Create(nameof(PageIndicatorTintColor), typeof(Color), typeof(CarouselView), Color.Silver);

		public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(int), typeof(CarouselView), 0, BindingMode.TwoWay);

		// Page Indicators properties
		public static readonly BindableProperty ShowIndicatorsProperty = BindableProperty.Create(nameof(ShowIndicators), typeof(bool), typeof(CarouselView), false);

		public Action<object, int> InsertAction;

		public EventHandler PositionSelected;

		public Action<int> RemoveAction;

		public bool AnimateTransition
		{
			get { return (bool)GetValue(AnimateTransitionProperty); }
			set { SetValue(AnimateTransitionProperty, value); }
		}

		public bool Arrows
		{
			get { return (bool)GetValue(ArrowsProperty); }
			set { SetValue(ArrowsProperty, value); }
		}

		public bool Bounces
		{
			get { return (bool)GetValue(BouncesProperty); }
			set { SetValue(BouncesProperty, value); }
		}

		public Color CurrentPageIndicatorTintColor
		{
			get { return (Color)GetValue(CurrentPageIndicatorTintColorProperty); }
			set { SetValue(CurrentPageIndicatorTintColorProperty, value); }
		}

		public CarouselViewIndicatorsShape IndicatorsShape
		{
			get { return (CarouselViewIndicatorsShape)GetValue(IndicatorsShapeProperty); }
			set { SetValue(IndicatorsShapeProperty, value); }
		}

		public int InterPageSpacing
		{
			get { return (int)GetValue(InterPageSpacingProperty); }
			set { SetValue(InterPageSpacingProperty, value); }
		}

		public Color InterPageSpacingColor
		{
			get { return (Color)GetValue(InterPageSpacingColorProperty); }
			set { SetValue(InterPageSpacingColorProperty, value); }
		}

		public IList ItemsSource
		{
			get { return (IList)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		public CarouselViewOrientation Orientation
		{
			get { return (CarouselViewOrientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		public Color PageIndicatorTintColor
		{
			get { return (Color)GetValue(PageIndicatorTintColorProperty); }
			set { SetValue(PageIndicatorTintColorProperty, value); }
		}

		public int Position
		{
			get { return (int)GetValue(PositionProperty); }
			set { SetValue(PositionProperty, value); }
		}

		public bool ShowIndicators
		{
			get { return (bool)GetValue(ShowIndicatorsProperty); }
			set { SetValue(ShowIndicatorsProperty, value); }
		}

		public void InsertPage(object item, int position = -1)
		{
			InsertAction?.Invoke(item, position);
		}

		public void RemovePage(int position)
		{
			RemoveAction?.Invoke(position);
		}
	}

	public class CarouselViewException : Exception
	{
		public CarouselViewException()
		{
		}

		public CarouselViewException(string message)
			: base(message)
		{
		}

		public CarouselViewException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms.Platform;

/*
The MIT License(MIT)

Copyright(c) 2017 Alexander Reyes (alexrainman1975@gmail.com)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
IN THE SOFTWARE.
 */

namespace Xamarin.Forms
{
	[RenderWith(typeof(_CarouselViewRenderer))]
	public class CarouselView : ItemsView<View> //, IElementConfiguration<CarouselView>
	{
		public static readonly BindableProperty OrientationProperty = BindableProperty.Create(nameof(Orientation), typeof(CarouselViewOrientation), typeof(CarouselView), CarouselViewOrientation.Horizontal);

        public CarouselViewOrientation Orientation
        {
            get { return (CarouselViewOrientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        // Android and iOS only
        public static readonly BindableProperty InterPageSpacingProperty = BindableProperty.Create(nameof(InterPageSpacing), typeof(int), typeof(CarouselView), 0);

        public int InterPageSpacing
        {
            get { return (int)GetValue(InterPageSpacingProperty); }
            set { SetValue(InterPageSpacingProperty, value); }
        }

        public static readonly BindableProperty IsSwipeEnabledProperty = BindableProperty.Create(nameof(IsSwipeEnabled), typeof(bool), typeof(CarouselView), true);

        public bool IsSwipeEnabled
        {
            get { return (bool)GetValue(IsSwipeEnabledProperty); }
            set { SetValue(IsSwipeEnabledProperty, value); }
        }

        public static readonly BindableProperty IndicatorsTintColorProperty = BindableProperty.Create(nameof(IndicatorsTintColor), typeof(Color), typeof(CarouselView), Color.Silver);

        public Color IndicatorsTintColor
        {
            get { return (Color)GetValue(IndicatorsTintColorProperty); }
            set { SetValue(IndicatorsTintColorProperty, value); }
        }

        public static readonly BindableProperty CurrentPageIndicatorTintColorProperty = BindableProperty.Create(nameof(CurrentPageIndicatorTintColor), typeof(Color), typeof(CarouselView), Color.Gray);

        public Color CurrentPageIndicatorTintColor
        {
            get { return (Color)GetValue(CurrentPageIndicatorTintColorProperty); }
            set { SetValue(CurrentPageIndicatorTintColorProperty, value); }
        }

        public static readonly BindableProperty IndicatorsShapeProperty = BindableProperty.Create(nameof(IndicatorsShape), typeof(IndicatorsShape), typeof(CarouselView), IndicatorsShape.Circle);

        public IndicatorsShape IndicatorsShape
        {
            get { return (IndicatorsShape)GetValue(IndicatorsShapeProperty); }
            set { SetValue(IndicatorsShapeProperty, value); }
        }

        public static readonly BindableProperty ShowIndicatorsProperty = BindableProperty.Create(nameof(ShowIndicators), typeof(bool), typeof(CarouselView), false);

        public bool ShowIndicators
        {
            get { return (bool)GetValue(ShowIndicatorsProperty); }
            set { SetValue(ShowIndicatorsProperty, value); }
        }
     
        public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(int), typeof(CarouselView), 0, BindingMode.TwoWay);

        public int Position
        {
            get { return (int)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly BindableProperty AnimateTransitionProperty = BindableProperty.Create(nameof(AnimateTransition), typeof(bool), typeof(CarouselView), true);

        public bool AnimateTransition
        {
            get { return (bool)GetValue(AnimateTransitionProperty); }
            set { SetValue(AnimateTransitionProperty, value); }
        }

        public static readonly BindableProperty ShowArrowsProperty = BindableProperty.Create(nameof(ShowArrows), typeof(bool), typeof(CarouselView), false);

        public bool ShowArrows
        {
            get { return (bool)GetValue(ShowArrowsProperty); }
            set { SetValue(ShowArrowsProperty, value); }
        }

        public static readonly BindableProperty ArrowsBackgroundColorProperty = BindableProperty.Create(nameof(ArrowsBackgroundColor), typeof(Color), typeof(CarouselView), Color.Black);

        public Color ArrowsBackgroundColor
        {
            get { return (Color)GetValue(ArrowsBackgroundColorProperty); }
            set { SetValue(ArrowsBackgroundColorProperty, value); }
        }

        public static readonly BindableProperty ArrowsTintColorProperty = BindableProperty.Create(nameof(ArrowsTintColor), typeof(Color), typeof(CarouselView), Color.White);

        public Color ArrowsTintColor
        {
            get { return (Color)GetValue(ArrowsTintColorProperty); }
            set { SetValue(ArrowsTintColorProperty, value); }
        }

        // Not working on UWP
        public static readonly BindableProperty ArrowsTransparencyProperty = BindableProperty.Create(nameof(ArrowsTransparency), typeof(float), typeof(CarouselView), 0.5f);

        public float ArrowsTransparency
        {
            get { return (float)GetValue(ArrowsTransparencyProperty); }
            set { SetValue(ArrowsTransparencyProperty, value); }
        }

        public static readonly BindableProperty PositionSelectedCommandProperty = BindableProperty.Create(nameof(PositionSelectedCommand), typeof(Command), typeof(CarouselView), null, BindingMode.Default, (bindable, value) =>
        {
            return true;
        });

		public Command PositionSelectedCommand
        {
            get { return (Command)GetValue(PositionSelectedCommandProperty); }
            set { SetValue(PositionSelectedCommandProperty, value); }
        }

        public event EventHandler<SelectedPositionChangedEventArgs> PositionSelected;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendPositionSelected()
        {
            PositionSelected?.Invoke(this, new SelectedPositionChangedEventArgs(Position));
        }

        public event EventHandler<ScrolledDirectionEventArgs> Scrolled;

		public event EventHandler<SelectedItemChangedEventArgs> ItemSelected;

		readonly Lazy<PlatformConfigurationRegistry<CarouselView>> _platformConfigurationRegistry;

		public CarouselView()
		{
			VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<CarouselView>>();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
        public void SendScrolled(double percent, ScrollDirection direction)
        {
            Scrolled?.Invoke(this, new ScrolledDirectionEventArgs { NewValue = percent, Direction = direction });
        }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public int GetCount()
		{
			return ItemsSource.GetCount();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public IList<object> GetList()
		{
			return ItemsSource.GetList();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public object GetItem(int i)
		{
			return ItemsSource.GetItem(i);
		}

		protected override View CreateDefault(object item)
		{
			return new Label { Text = item?.ToString() ?? "" };
		}

		//public IPlatformElementConfiguration<T, CarouselView> On<T>() where T : IConfigPlatform
		//{
		//	return _platformConfigurationRegistry.Value.On<T>();
		//}
	}
}

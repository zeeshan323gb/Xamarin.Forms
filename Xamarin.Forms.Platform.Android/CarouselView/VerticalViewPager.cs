using System;
using System.Linq;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Support.V4.View;

/*
The MIT License(MIT)

Copyright(c) 2017 Alexander Reyes(alexrainman1975 @gmail.com)

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

namespace Xamarin.Forms.Platform.Android
{

	public class BaseVerticalViewPager : ViewPager
	{
		bool _isSwipingEnabled = true;

		public BaseVerticalViewPager(Context context) : base(context, null)
		{
		}

		public BaseVerticalViewPager(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Init();
		}

		public override bool CanScrollHorizontally(int direction)
		{
			return false;
		}

		public override bool CanScrollVertically(int direction)
		{
			return base.CanScrollHorizontally(direction);
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			if (_isSwipingEnabled)
			{
				var toIntercept = base.OnInterceptTouchEvent(flipXY(ev));
				// Return MotionEvent to normal
				flipXY(ev);
				return toIntercept;
			}

			return false;
		}

		public override bool OnTouchEvent(MotionEvent ev)
		{
			if (_isSwipingEnabled)
			{
				var toHandle = base.OnTouchEvent(flipXY(ev));
				//Return MotionEvent to normal
				flipXY(ev);
				return toHandle;
			}

			return false;
		}

		public void SetPagingEnabled(bool enabled)
		{
			_isSwipingEnabled = enabled;
		}

		public void Init()
		{
			// Make page transit vertical
			SetPageTransformer(true, new VerticalPageTransformer());
			// Get rid of the overscroll drawing that happens on the left and right (the ripple)
			OverScrollMode = OverScrollMode.Never;
		}

		MotionEvent flipXY(MotionEvent ev)
		{
			var width = Width;
			var height = Height;
			var x = (ev.GetY() / height) * width;
			var y = (ev.GetX() / width) * height;
			ev.SetLocation(x, y);
			return ev;
		}

		class VerticalPageTransformer : Java.Lang.Object, IPageTransformer
		{
			public void TransformPage(global::Android.Views.View view, float position)
			{
				var pageWidth = view.Width;
				var pageHeight = view.Height;

				if (position < -1)
				{
					// This page is way off-screen to the left.
					view.Alpha = 0;
				}
				else if (position <= 1)
				{
					view.Alpha = 1;
					// Counteract the default slide transition
					view.TranslationX = pageWidth * -position;
					// set Y position to swipe in from top
					float yPosition = position * pageHeight;
					view.TranslationY = yPosition;
				}
				else
				{
					// This page is way off-screen to the right.
					view.Alpha = 0;
				}
			}
		}
	}


    public class VerticalViewPager : BaseVerticalViewPager, IViewPager
    {
        bool isSwipeEnabled = true;
        CarouselView Element;

        // Fix for #171 System.MissingMethodException: No constructor found
        //public VerticalViewPager(IntPtr intPtr, JniHandleOwnership jni) : base(intPtr, jni)
        //{
        //}

        public VerticalViewPager(Context context) : base(context, null)
        {
        }

        public VerticalViewPager(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            base.Init();
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            if (ev.Action == MotionEventActions.Up)
            {
                if (Element?.GestureRecognizers.GetCount() > 0)
                {
                    var gesture = Element.GestureRecognizers.First() as Xamarin.Forms.TapGestureRecognizer;
                    if (gesture != null)
                        gesture.Command?.Execute(gesture.CommandParameter);
                }
            }

            if (this.isSwipeEnabled)
            {
                return base.OnInterceptTouchEvent(ev);
            }

            return false;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (this.isSwipeEnabled)
            {
                return base.OnTouchEvent(e);
            }

            return false;
        }

        public void SetElement(CarouselView element)
        {
            this.Element = element;
        }
    }
}

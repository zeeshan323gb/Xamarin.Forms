using System;
using Android.Content;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class CustomViewPager : ViewPager
	{
		private bool isSwipingEnabled = true;

		public CustomViewPager(Context context) : base(context, null)
		{
		}

		public CustomViewPager(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (this.isSwipingEnabled)
			{
				return base.OnTouchEvent(e);
		    }

            return false;
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			if (this.isSwipingEnabled)
			{
				return base.OnInterceptTouchEvent(ev);
		    }

            return false;
		}

		public void SetPagingEnabled(bool enabled)
		{
			this.isSwipingEnabled = enabled;
		}
	}
}

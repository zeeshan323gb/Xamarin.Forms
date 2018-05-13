using Android.Animation;
using Android.Graphics;
using Android.Graphics.Drawables;
using System;
using AColor = Android.Graphics.Color;

namespace Xamarin.Forms.Platform.Android
{
	public class ColorChangeRevealDrawable : AnimationDrawable
	{
		private readonly Point _center;
		private readonly AColor _endColor;
		private readonly AColor _startColor;
		private float progress;

		public ColorChangeRevealDrawable(AColor startColor, AColor endColor, Point center) : base()
		{
			_startColor = startColor;
			_endColor = endColor;

			ValueAnimator animator = ValueAnimator.OfFloat(0, 1);
			animator.SetInterpolator(new global::Android.Views.Animations.DecelerateInterpolator());
			animator.SetDuration(500);
			animator.Update += OnUpdate;
			animator.Start();
			_center = center;
		}

		public override void Draw(Canvas canvas)
		{
			var bounds = Bounds;
			canvas.DrawColor(_startColor);
			float centerX = (float)_center.X;
			float centerY = (float)_center.Y;

			float width = bounds.Width();
			float distanceFromCenter = (float)Math.Abs(width / 2 - _center.X);
			float radius = (width / 2 + distanceFromCenter) * 1.1f;

			var paint = new Paint();
			paint.Color = _endColor;
			canvas.DrawCircle(centerX, centerY, radius * progress, paint);
		}

		private void OnUpdate(object sender, ValueAnimator.AnimatorUpdateEventArgs e)
		{
			progress = (float)e.Animation.AnimatedValue;
			InvalidateSelf();
		}
	}
}
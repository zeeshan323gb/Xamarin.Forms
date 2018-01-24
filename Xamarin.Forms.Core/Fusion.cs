using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Core;

namespace Xamarin.Forms
{
	/*
	 * var centerFuse = new Fusion (mainLayout).Center.Add(20, new Fusion (label2).Height);
	 * new Fusion (mainLayout).Center.Add(20, new Fusion (label2).Height).Center
	 * new Fusion (mainLayout).Center.Center
	 * 
	 * 
var sizeFuse = new Fusion (otherLabel).Size.Add (20, 20);
FusedLayout.AddFusion (view2, FuseProperty.Center, centerFuse);
FusedLayout.AddFusion (view2, FuseProperty.Size, sizeFuse);
var sizeFuse = new Fusion (view2).Measure().Add (20, 20);(

		// view2 is the targetview    mainLayout is the source view
		FusedLayout.AddFusion(view2, FuseProperty.Center, new Fuse (mainLayout).Center.Add (20, 0));
 */


	public abstract class PointFusionBase : FusionBase
	{ 

		protected PointFusionBase(FusionBase  parent) : base(parent)
		{
			
		}

		public PointFusion Add(Point point)
		{
			return new PointFusion(point, this);
		}
	}

	

	public class RightFusion : FusionBase
	{
		public RightFusion(FusionBase parent) : base(parent)
		{
			SourceElement = parent.SourceElement;
			SourceProperty = FuseProperty.Right;
		}

		public override double Calculate(FuseProperty propertyXYHW)
		{
			switch (propertyXYHW)
			{
				case FuseProperty.X:
					var parentX = ParentFusion.Calculate(FuseProperty.X);
					if (double.IsNaN(parentX)) { return double.NaN; }

					var parentWidth = ParentFusion.Calculate(FuseProperty.Width);
					if (double.IsNaN(parentWidth)) { return double.NaN; }

					return (parentX + parentWidth);
				default:
					return double.NaN;
			}
		}

		public override object GetPropertySolve()
		{
			var sourceSolveView = FusedLayout.GetSolveView(SourceElement);
			return sourceSolveView.Right;
		}
	}



	public class PointFusion : PointFusionBase
	{
		public PointFusion(Point addMe, FusionBase parent) : base(parent)
		{
			SourceElement = parent.SourceElement;
			SourceProperty = parent.SourceProperty;
			Addition = addMe;
		}

		public Point Addition
		{
			get;
		}

		public override object GetPropertySolve()
		{
			var parentSolve = (Point)ParentFusion.GetPropertySolve();

			if (parentSolve == FusedLayout.SolveView.NullPoint)
				return parentSolve;

			return new Point(parentSolve.X + Addition.X, parentSolve.Y + Addition.Y);
		}

		public override double Calculate(FuseProperty propertyXYHW)
		{
			var calculatedSize = ParentFusion.Calculate(propertyXYHW);
			if (double.IsNaN(calculatedSize)) { return double.NaN; }
			switch (propertyXYHW)
			{
				case FuseProperty.X:
					calculatedSize += Addition.X;
					break;
				case FuseProperty.Y:
					calculatedSize += Addition.Y;
					break;
				default:
					throw new ArgumentException($"{propertyXYHW}");
			}

			return calculatedSize;

		}
	}

	public class CenterFusion : PointFusionBase
	{
		public CenterFusion(FusionBase parent) : base(parent)
		{
			SourceElement = parent.SourceElement;
		} 

		public override object GetPropertySolve()
		{
			return FusedLayout.GetSolveView(SourceElement).Center;
		}

		public override double Calculate(FuseProperty propertyXYHW)
		{
			switch (propertyXYHW)
			{
				case FuseProperty.X:
					var parentX = ParentFusion.Calculate(FuseProperty.X);
					if (double.IsNaN(parentX)) { return double.NaN; }

					var parentWidth = ParentFusion.Calculate(FuseProperty.Width);
					if (double.IsNaN(parentWidth)) { return double.NaN; }

					return ((parentX + parentWidth) / 2);


				case FuseProperty.Y:
					var parentY = ParentFusion.Calculate(FuseProperty.Y);
					if (double.IsNaN(parentY)) { return double.NaN; }

					var parentHeight = ParentFusion.Calculate(FuseProperty.Height);
					if (double.IsNaN(parentHeight)) { return double.NaN; }

					return ((parentY + parentHeight) / 2);

				default:
					return double.NaN;
			}
		}
	}
	 

	public abstract class FusionBase : BindableObject
	{

		protected FusionBase(FusionBase parent)
		{
			this.ParentFusion = parent;
		}
		 
		public FusionBase ParentFusion
		{
			get;
		}

		public VisualElement SourceElement { get; set; } // BP 

		public FuseProperty SourceProperty { get; set; } // BP 

		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetView"></param>
		/// <param name="propertyXYHW">Needs a better name. This basically indicates this property is only going to be XYHW		
		/// </param>
		/// <returns></returns>
		public abstract double Calculate(FuseProperty propertyXYHW);


		public abstract object GetPropertySolve();
	}


	/// <summary>
	/// Transforms the Source result to the target property
	/// </summary>
	public class TargetWrapper
	{
		public TargetWrapper(FusionBase fusion, FuseProperty targetProperty, View targetView)
		{
			Fuse = fusion;
			TargetProperty = targetProperty;
			TargetView = targetView;
		}


		string getErrorString(FuseProperty targetViewPropertyXYWH)
		{
			return $"{targetViewPropertyXYWH} - {TargetProperty}";
		}

		public bool Influences(FuseProperty targetViewPropertyXYWH)
		{
			switch (targetViewPropertyXYWH)
			{
				case FuseProperty.X:
					switch (TargetProperty)
					{
						case FuseProperty.X:
						case FuseProperty.Center:
						case FuseProperty.Right:
							return true;
						case FuseProperty.Y:
						case FuseProperty.Width:
							return false;
						default:
							throw new ArgumentException(getErrorString(targetViewPropertyXYWH));
					}
				case FuseProperty.Y:
					switch (TargetProperty)
					{
						case FuseProperty.Y:
						case FuseProperty.Center:
							return true;
						case FuseProperty.X:
						case FuseProperty.Right:
						case FuseProperty.Width:
							return false;
						default:
							throw new ArgumentException(getErrorString(targetViewPropertyXYWH));

					}
				case FuseProperty.Height:
					switch (TargetProperty)
					{
						case FuseProperty.Height:
							return true;
						case FuseProperty.Width:
						case FuseProperty.X:
						case FuseProperty.Right:
						case FuseProperty.Center:
							return false;
						default:
							throw new ArgumentException(getErrorString(targetViewPropertyXYWH));

					}
				case FuseProperty.Width:
					switch (TargetProperty)
					{
						case FuseProperty.Width:
						case FuseProperty.Right:
							return true;
						case FuseProperty.Height:
						case FuseProperty.X:
						case FuseProperty.Center:
							return false;
						default:
							throw new ArgumentException(getErrorString(targetViewPropertyXYWH));

					}
				default:					
					return targetViewPropertyXYWH == TargetProperty;
			}
		}

		internal double Calculate(FuseProperty targetViewPropertyXYWHCalculating)
		{	
			double sourceResult = Fuse.Calculate(targetViewPropertyXYWHCalculating);
			if (double.IsNaN(sourceResult)) { return double.NaN; }
			var targetSolve = FusedLayout.GetSolveView(TargetView);

			switch (TargetProperty)
			{
				case FuseProperty.X:
					return sourceResult;
				case FuseProperty.Center:
					switch(targetViewPropertyXYWHCalculating)
					{
						case FuseProperty.X:
							if (double.IsNaN(targetSolve.Width)) { return double.NaN; }
							return sourceResult - targetSolve.Width / 2;

						case FuseProperty.Y:
							if (double.IsNaN(targetSolve.Height)) { return double.NaN; }
							return sourceResult - targetSolve.Height / 2;

						default:
							throw new ArgumentException(getErrorString(targetViewPropertyXYWHCalculating));

					}
				case FuseProperty.Width:
					switch (targetViewPropertyXYWHCalculating)
					{
						case FuseProperty.Width:
							return sourceResult;
						default:
							throw new ArgumentException(getErrorString(targetViewPropertyXYWHCalculating));

					}
				default:
					if(targetViewPropertyXYWHCalculating == TargetProperty)
					{
						return sourceResult;
					}

					return double.NaN;

			}

			throw new ArgumentException(getErrorString(targetViewPropertyXYWHCalculating));
		}

		public FuseProperty TargetProperty { get; set; } // BP 
		public FusionBase Fuse { get; set; }
		public View TargetView { get; set; } // BP 
	}


	public class ScalarOperationFusion : FusionBase
	{
		private readonly FuseOperator _operation;
		private readonly double _value;

		public ScalarOperationFusion(FusionBase parent, FuseOperator operation, double value) : base(parent)
		{
			SourceProperty = parent.SourceProperty;
			SourceElement = parent.SourceElement;
			_operation = operation;
			_value = value;
		}

		public override double Calculate(FuseProperty propertyXYHW)
		{
			var returnValue = ParentFusion.Calculate(propertyXYHW);
			if (double.IsNaN(returnValue) ) { return double.NaN; }

			switch(_operation)
			{
				case FuseOperator.Add:
					returnValue += _value;
					break;

				case FuseOperator.Subtract:
					returnValue -= _value;
					break;
				case FuseOperator.None:
					break;
				default:
					throw new ArgumentException($"{propertyXYHW}");

			}

			return returnValue;
		}

		public override object GetPropertySolve()
		{
			return Calculate(SourceProperty);
		}
	}

	public class ScalarValueFusion : FusionBase
	{
		public ScalarValueFusion(
			FuseProperty sourceProperty,
			double value) : base(null)
		{
			SourceProperty = sourceProperty;
			Value = value;
		}

		public double Value { get; }

		public ScalarOperationFusion Add(double value)
		{
			return new ScalarOperationFusion(this, FuseOperator.Add, value);
		}


		public override double Calculate(FuseProperty propertyXYHW)
		{
			if (propertyXYHW != SourceProperty)
				return double.NaN;

			return Value;
		}

		public override object GetPropertySolve()
		{
			return Calculate(SourceProperty);
		}
	}

	public class ScalarPropertyFusion : FusionBase
	{
		public ScalarPropertyFusion(
			FusionBase parent, 
			FuseProperty sourceProperty) : base(parent)
		{			
			SourceProperty = sourceProperty;
			SourceElement = parent.SourceElement;
		}

		public ScalarOperationFusion Add(double value)
		{
			return new ScalarOperationFusion(this, FuseOperator.Add, value);
		}


		public override double Calculate(FuseProperty propertyXYHW)
		{
			if (propertyXYHW != SourceProperty)
				return Double.NaN;

			var dependentSourceSolve = FusedLayout.GetSolveView(SourceElement);
			switch (SourceProperty)
			{
				case FuseProperty.X:
					{
						return dependentSourceSolve.X;
					}
				case FuseProperty.Y:
					{
						return dependentSourceSolve.Y;
					}
				case FuseProperty.Height:
					{
						return dependentSourceSolve.Height;
					}
				case FuseProperty.Width:
					{
						return dependentSourceSolve.Width;
					}
				case FuseProperty.Right:
					{
						return dependentSourceSolve.Right;
					}
			}

			throw new ArgumentException($"Invalid SourceProperty: {SourceProperty}");
		}

		public override object GetPropertySolve()
		{
			return Calculate(SourceProperty);
		}
	}



	public class Fusion : FusionBase
	{
		public Fusion(VisualElement sourceElement) : base(null)
		{
			SourceElement = sourceElement;
		}

		public CenterFusion Center  => new CenterFusion(this);
		public RightFusion Right => new RightFusion(this);

		public ScalarPropertyFusion X => new ScalarPropertyFusion(this, FuseProperty.X);
		public ScalarPropertyFusion Y => new ScalarPropertyFusion(this, FuseProperty.Y);
		public ScalarPropertyFusion Height => new ScalarPropertyFusion(this, FuseProperty.Height);
		public ScalarPropertyFusion Width => new ScalarPropertyFusion(this, FuseProperty.Width);

		public override double Calculate(FuseProperty propertyXYHW)
		{
			var dependentSourceSolve = FusedLayout.GetSolveView(SourceElement);
			switch (propertyXYHW)
			{
				case FuseProperty.X:
					{
						return dependentSourceSolve.X;
					}
				case FuseProperty.Y:
					{
						return dependentSourceSolve.Y;
					}
				case FuseProperty.Height:
					{
						return dependentSourceSolve.Height;
					}
				case FuseProperty.Width:
					{
						return dependentSourceSolve.Width;
					}
			}

			throw new ArgumentException($"Invalid SourceProperty: {SourceProperty}");
		}

		public override object GetPropertySolve()
		{
			throw new NotImplementedException();
		}
	}

}

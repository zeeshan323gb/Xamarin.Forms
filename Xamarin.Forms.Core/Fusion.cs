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

		public override object GetPropertySolve(FuseProperty targetPropertySolving)
		{
			var parentSolve = (Point)ParentFusion.GetPropertySolve(targetPropertySolving);

			if (parentSolve == FusedLayout.SolveView.NullPoint)
				return parentSolve;

			return new Point(parentSolve.X + Addition.X, parentSolve.Y + Addition.Y);
		}
	}

	public class CenterFusion : PointFusionBase
	{
		public CenterFusion(FusionBase parent) : base(parent)
		{
			SourceElement = parent.SourceElement;
		} 

		public override object GetPropertySolve(FuseProperty targetPropertySolving)
		{
			return FusedLayout.GetSolveView(SourceElement).Center;
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

		public abstract object GetPropertySolve(FuseProperty targetPropertySolving);
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


		public override object GetPropertySolve(FuseProperty targetPropertySolving)
		{
			var returnValue = (double)ParentFusion.GetPropertySolve(targetPropertySolving);
			if (double.IsNaN(returnValue)) { return double.NaN; }

			switch (_operation)
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
					throw new ArgumentException($"{targetPropertySolving}");

			}

			return returnValue;
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

		public override object GetPropertySolve(FuseProperty targetPropertySolving)
		{
			return Value;
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


		public override object GetPropertySolve(FuseProperty targetPropertySolving)
		{

			return Fusion.GetViewProperty(SourceElement, SourceProperty);
		}
	}



	public class Fusion : FusionBase
	{
		public Fusion(VisualElement sourceElement) : base(null)
		{
			SourceElement = sourceElement;
		}

		public CenterFusion Center  => new CenterFusion(this);
		public ScalarPropertyFusion Right => new ScalarPropertyFusion(this, FuseProperty.Right);
		public ScalarPropertyFusion Top => new ScalarPropertyFusion(this, FuseProperty.Top);

		public ScalarPropertyFusion X => new ScalarPropertyFusion(this, FuseProperty.X);
		public ScalarPropertyFusion Y => new ScalarPropertyFusion(this, FuseProperty.Y);
		public ScalarPropertyFusion Height => new ScalarPropertyFusion(this, FuseProperty.Height);
		public ScalarPropertyFusion Width => new ScalarPropertyFusion(this, FuseProperty.Width);

		public static object GetViewProperty(VisualElement sourceElement, FuseProperty property)
		{
			var dependentSourceSolve = FusedLayout.GetSolveView(sourceElement);
			switch (property)
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
				case FuseProperty.Bottom:
					{
						return dependentSourceSolve.Bottom;
					}
				case FuseProperty.CenterX:
					{
						return dependentSourceSolve.CenterX;
					}
				case FuseProperty.CenterY:
					{
						return dependentSourceSolve.CenterY;
					}
				case FuseProperty.Left:
					{
						return dependentSourceSolve.Left;
					}
				case FuseProperty.Top:
					{
						return dependentSourceSolve.Top;
					}
				case FuseProperty.Center:
					{
						return dependentSourceSolve.Center;
					}
				case FuseProperty.Size:
					{
						return dependentSourceSolve.Size;
					}
			}

			throw new ArgumentException($"Invalid SourceProperty: {property}");
		}

		public override object GetPropertySolve(FuseProperty targetPropertySolving)
		{
			return GetViewProperty(SourceElement, targetPropertySolving);
		}
	}

}
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

		public PointFusionBase Add(Point point)
		{
			return new PointOperationFusion(this, FuseOperator.Add, point);
		}

		public PointFusionBase Add(PointFusionBase fusionPoint)
		{
			return new PointOperationFusion(this, FuseOperator.Add, fusionPoint);
		}

		public PointFusionBase Add(ScalarFusionBase scalar)
		{
			return new PointOperationFusion(this, FuseOperator.Add, scalar);
		}
	}


	public class PointOperationFusion : PointFusionBase
	{
		private FuseOperator Operation { get; }
		private Point PointValue { get; }
		private PointFusionBase FusionPointValue { get; }
		private ScalarFusionBase FusionScalarValue { get; }


		private PointOperationFusion(FusionBase parent, FuseOperator operation) 
			: base(parent)
		{
			SourceElement = parent.SourceElement;
			SourceProperty = parent.SourceProperty;
			Operation = operation;
		}


		public PointOperationFusion(FusionBase parent, FuseOperator operation, Point value) 
			: this(parent, operation)
		{
			PointValue = value;
		}


		public PointOperationFusion(FusionBase parent, FuseOperator operation, PointFusionBase value)
			: this(parent, operation)
		{
			FusionPointValue = value;
		}

		public PointOperationFusion(FusionBase parent, FuseOperator operation, ScalarFusionBase value)
			: this(parent, operation)
		{
			FusionScalarValue = value;
		}

		public override object GetPropertySolve(FuseProperty targetPropertySolving)
		{
			var parentSolve = (Point)ParentFusion.GetPropertySolve(SourceProperty);

			if (parentSolve == FusedLayout.SolveView.NullPoint || Operation == FuseOperator.None)
				return parentSolve;

			Point value;
			if(FusionPointValue != null)
			{
				value = (Point)FusionPointValue.GetPropertySolve(SourceProperty);
			}
			else if(FusionScalarValue != null)
			{
				var scalarResult = (double)FusionScalarValue.GetPropertySolve(SourceProperty);
				if(!double.IsNaN(scalarResult))
				{
					value = new Point(scalarResult, scalarResult);
				}
				else
				{
					return FusedLayout.SolveView.NullPoint;
				}
			}
			else
			{
				value = PointValue;
			}

			if(value == FusedLayout.SolveView.NullPoint)
			{
				return FusedLayout.SolveView.NullPoint;
			}

			switch(Operation)
			{
				case FuseOperator.Add:
					return new Point(parentSolve.X + value.X, parentSolve.Y + value.Y);
				case FuseOperator.Subtract:
					return new Point(parentSolve.X - value.X, parentSolve.Y - value.Y);
			}

			throw new ArgumentException($"{targetPropertySolving}");
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetPropertySolving">this property is a little silly right now and only matters if an ask goes all the way up the hierarchy
		/// to the flow layout itself. Need to surface this better.
		/// Also should this be surface using generics?
		/// </param>
		/// <returns></returns>
		public abstract object GetPropertySolve(FuseProperty targetPropertySolving);

	}

	public abstract class ScalarFusionBase : FusionBase
	{
		protected ScalarFusionBase(FusionBase parent) : base(parent)
		{
		}

		public ScalarFusionBase Add(ScalarFusionBase childFusion)
		{
			return new ScalarOperationFusion(this, FuseOperator.Add, childFusion);
		}
	}

	public class ScalarOperationFusion : ScalarFusionBase
	{
		private readonly FuseOperator _operation;
		private readonly double _scalarValue;
		private readonly ScalarFusionBase _fusionValue;


		protected ScalarOperationFusion(FusionBase parent, FuseOperator operation) : base(parent)
		{
			SourceProperty = parent.SourceProperty;
			SourceElement = parent.SourceElement;
			_operation = operation;
		}

		public ScalarOperationFusion(FusionBase parent, FuseOperator operation, double scalarValue) : 
			this(parent, operation)
		{
			_scalarValue = scalarValue;
		}
		public ScalarOperationFusion(FusionBase parent, FuseOperator operation, ScalarFusionBase fusionValue) :
			this(parent, operation)
		{
			_fusionValue = fusionValue;
		}


		public override object GetPropertySolve(FuseProperty targetPropertySolving)
		{
			var returnValue = (double)ParentFusion.GetPropertySolve(SourceProperty);
			if (double.IsNaN(returnValue)) { return double.NaN; }
			if (_operation == FuseOperator.None) { return returnValue; }

			double value;
			if(_fusionValue != null)
			{
				value = (double)_fusionValue.GetPropertySolve(SourceProperty);
			}
			else
			{
				value = _scalarValue;
			}

			if (double.IsNaN(value)) { return double.NaN; }


			switch (_operation)
			{
				case FuseOperator.Add:
					returnValue += value;
					break;
				case FuseOperator.Subtract:
					returnValue -= value;
					break;
				default:
					throw new ArgumentException($"{SourceProperty}");
			}

			return returnValue;
		}
	}

	public class ScalarPropertyFusion : ScalarFusionBase
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

	public class ScalarValueFusion : ScalarFusionBase
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





	public class Fusion : FusionBase
	{
		public Fusion(VisualElement sourceElement) : base(null)
		{
			SourceElement = sourceElement;
		}

		public CenterFusion Center  => new CenterFusion(this);
		public ScalarPropertyFusion Right => new ScalarPropertyFusion(this, FuseProperty.Right);
		public ScalarPropertyFusion Bottom => new ScalarPropertyFusion(this, FuseProperty.Bottom);
		public ScalarPropertyFusion Left => new ScalarPropertyFusion(this, FuseProperty.Left);
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

}
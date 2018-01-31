using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Core;

namespace Xamarin.Forms
{

	public static class FusionExtensions
	{
		public static PointFusionBase Add(this PointFusionBase This, Point point)
		{
			return This.Offset(point, FuseOperator.Add);
		}
		public static PointFusionBase Add(this PointFusionBase This, PointFusionBase fusionPoint)
		{
			return This.Offset(fusionPoint, FuseOperator.Add);
		}
		public static PointFusionBase Add(this PointFusionBase This, ScalarFusionBase scalar)
		{
			return This.Offset(scalar, FuseOperator.Add);
		}


		public static ScalarFusionBase Add(this ScalarFusionBase This, ScalarFusionBase childFusion)
		{
			return This.Offset(childFusion, FuseOperator.Add);
		}



		public static SizeFusionBase Add(this SizeFusionBase This, Size size)
		{
			return This.Offset(size, FuseOperator.Add);
		}

		public static SizeFusionBase Add(this SizeFusionBase This, SizeFusionBase fusionSize)
		{
			return This.Offset(fusionSize, FuseOperator.Add);
		}

		public static SizeFusionBase Add(this SizeFusionBase This, ScalarFusionBase scalar)
		{
			return This.Offset(scalar, FuseOperator.Add);
		}

		public static ScalarOperationFusion Add(this ScalarPropertyFusion This,  double value)
		{
			return This.Offset(value, FuseOperator.Add);
		}



	}
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
	public abstract class PointFusionBase : FusionBase<Point>
	{ 
		protected PointFusionBase(FusionBase  parent) : base(parent)
		{
			
		}

		public PointFusionBase Offset(Point point, FuseOperator operation)
		{
			return new PointOperationFusion(this, operation, point);
		}

		public PointFusionBase Offset(PointFusionBase fusionPoint, FuseOperator operation)
		{
			return new PointOperationFusion(this, operation, fusionPoint);
		}

		public PointFusionBase Offset(ScalarFusionBase scalar, FuseOperator operation)
		{
			return new PointOperationFusion(this, operation, scalar);
		}


		double GetCenterX()
		{
			var result = GetPropertySolve();
			if (result == FusedLayout.SolveView.NullPoint)
			{
				return double.NaN;
			}

			return result.X;
		}

		double GetCenterY()
		{
			var result = GetPropertySolve();
			if (result == FusedLayout.SolveView.NullPoint)
			{
				return double.NaN;
			}

			return result.Y;
		}

		public ScalarFusionBase CenterX
		{
			get
			{
				return new ScalarFunctionFusion(this, GetCenterX);
			}
		}
		public ScalarFusionBase CenterY
		{
			get
			{
				return new ScalarFunctionFusion(this, GetCenterY);
			}
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

		public override Point GetPropertySolve()
		{
			if(!SourceElementVisible)
			{
				return Point.Zero;
			}

			var parentSolve = ParentFusion.GetPropertySolve();
			if (parentSolve == FusedLayout.SolveView.NullPoint || Operation == FuseOperator.None)
				return parentSolve;

			Point value;
			if(FusionPointValue != null)
			{
				value = FusionPointValue.GetPropertySolve();
			}
			else if(FusionScalarValue != null)
			{
				var scalarResult = FusionScalarValue.GetPropertySolve();
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

			throw new ArgumentException($"{SourceProperty}");
		}
	}

	

	public class CenterFusion : PointFusionBase
	{
		public CenterFusion(FusionBase parent) : base(parent)
		{
			SourceElement = parent.SourceElement;
		} 

		public override Point GetPropertySolve()
		{
			if (!SourceElementVisible) { return Point.Zero; }

			return FusedLayout.GetSolveView(SourceElement).Center;
		}
	}


	public class SizeOperationFusion : SizeFusionBase
	{
		private FuseOperator Operation { get; }
		private Size SizeValue { get; }
		private SizeFusionBase FusionSizeValue { get; }
		private ScalarFusionBase FusionScalarValue { get; }


		private SizeOperationFusion(FusionBase parent, FuseOperator operation)
			: base(parent)
		{
			SourceElement = parent.SourceElement;
			SourceProperty = parent.SourceProperty;
			Operation = operation;
		}


		public SizeOperationFusion(FusionBase parent, FuseOperator operation, Size value)
			: this(parent, operation)
		{
			SizeValue = value;
		}


		public SizeOperationFusion(FusionBase parent, FuseOperator operation, SizeFusionBase value)
			: this(parent, operation)
		{
			FusionSizeValue = value;
		}

		public SizeOperationFusion(FusionBase parent, FuseOperator operation, ScalarFusionBase value)
			: this(parent, operation)
		{
			FusionScalarValue = value;
		}

		public override Size GetPropertySolve()
		{
			if (!SourceElementVisible) { return Size.Zero; }

			var parentSolve = (Size)ParentFusion.GetPropertySolve();

			if (parentSolve == FusedLayout.SolveView.NullSize || Operation == FuseOperator.None)
				return parentSolve;

			Size value;
			if (FusionSizeValue != null)
			{
				value = (Size)FusionSizeValue.GetPropertySolve();
			}
			else if (FusionScalarValue != null)
			{
				var scalarResult = (double)FusionScalarValue.GetPropertySolve();
				if (!double.IsNaN(scalarResult))
				{
					value = new Size(scalarResult, scalarResult);
				}
				else
				{
					return FusedLayout.SolveView.NullSize;
				}
			}
			else
			{
				value = SizeValue;
			}

			if (value == FusedLayout.SolveView.NullSize)
			{
				return FusedLayout.SolveView.NullSize;
			}

			switch (Operation)
			{
				case FuseOperator.Add:
					return new Size(parentSolve.Width + value.Width, parentSolve.Height + value.Height);
				case FuseOperator.Subtract:
					return new Size(parentSolve.Width - value.Width, parentSolve.Height - value.Height);
			}

			throw new ArgumentException($"{SourceProperty}");
		}
	}


	public abstract class SizeFusionBase : FusionBase<Size>
	{
		protected SizeFusionBase(FusionBase parent) : base(parent)
		{

		}

		public SizeFusionBase Offset(Size size, FuseOperator operation)
		{
			return new SizeOperationFusion(this, operation, size);
		}

		public SizeFusionBase Offset(SizeFusionBase fusionSize, FuseOperator operation)
		{
			return new SizeOperationFusion(this, operation, fusionSize);
		}

		public SizeFusionBase Offset(ScalarFusionBase scalar, FuseOperator operation)
		{
			return new SizeOperationFusion(this, operation, scalar);
		}
		double GetHeight()
		{
			var result = GetPropertySolve();
			if (result == FusedLayout.SolveView.NullSize)
			{
				return double.NaN;
			}

			return result.Height;
		}

		double GetWidth()
		{
			var result = GetPropertySolve();
			if (result == FusedLayout.SolveView.NullSize)
			{
				return double.NaN;
			}

			return result.Width;
		}

		public ScalarFusionBase Height
		{
			get
			{
				return new ScalarFunctionFusion(this, GetHeight);
			}
		}
		public ScalarFusionBase Width
		{
			get
			{
				return new ScalarFunctionFusion(this, GetWidth);
			}
		}

	}



	public class SizeFusion : SizeFusionBase
	{
		public SizeFusion(FusionBase parent) : base(parent)
		{
			SourceElement = parent.SourceElement;
		}

		public override Size GetPropertySolve()
		{
			if (!SourceElementVisible) { return Size.Zero; }
			return FusedLayout.GetSolveView(SourceElement).Size;
		}
	} 

	public abstract class FusionBase : BindableObject
	{
		protected FusionBase(FusionBase parent)
		{
			this.ParentFusion = parent;
			SourceElement = parent?.SourceElement;
			SourceProperty = parent?.SourceProperty ?? FuseProperty.None;
		}
		 
		public FusionBase ParentFusion
		{
			get;
		}

		public VisualElement SourceElement { get; set; } // BP 

		public FuseProperty SourceProperty { get; set; } // BP 


		protected bool SourceElementVisible => SourceElement == null || SourceElement.IsVisible;
	}

	public abstract class FusionBase<T> : FusionBase, IFusion<T>
	{
		protected FusionBase(FusionBase parent) : base(parent)
		{
		}

		public new FusionBase<T> ParentFusion
		{
			get
			{
				return (FusionBase<T>)base.ParentFusion;
			}
		}

		public abstract T GetPropertySolve();



		
	}

	public interface IFusion<T>
	{
		T GetPropertySolve();
	}


	public abstract class ScalarFusionBase : FusionBase<double>
	{
		protected ScalarFusionBase(FusionBase parent) : base(parent)
		{
		}

		public ScalarFusionBase Offset(ScalarFusionBase childFusion, FuseOperator operation)
		{
			return new ScalarOperationFusion(this, operation, childFusion);
		}
	}


	public class ScalarFunctionFusion : ScalarFusionBase
	{
		Func<double> Value { get; }
		public ScalarFunctionFusion(FusionBase parent, Func<double> value) : base(parent)
		{ 
			Value = value;
		}

		public override double GetPropertySolve()
		{
			if (!SourceElementVisible)
				return 0;
			return Value();
		}
	}

	public class ScalarOperationFusion : ScalarFusionBase
	{
		private readonly FuseOperator _operation;
		private readonly double _scalarValue;
		private readonly ScalarFusionBase _fusionValue;

		public ScalarOperationFusion(
			FuseProperty sourceProperty,
			double value) : base(null)
		{
			SourceProperty = sourceProperty;
			_operation = FuseOperator.None;
			_scalarValue = value;
		}

		protected ScalarOperationFusion(FusionBase parent, FuseOperator operation) : base(parent)
		{
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


		public override double GetPropertySolve()
		{
			if (!SourceElementVisible)
				return 0;

			var returnValue = double.NaN;

			if(ParentFusion != null)
			{
				returnValue = ParentFusion.GetPropertySolve();
			}
			else
			{
				returnValue = _scalarValue;
			}
			
			if (double.IsNaN(returnValue)) { return double.NaN; }
			if (_operation == FuseOperator.None) { return returnValue; }

			double value;
			if(_fusionValue != null)
			{
				value = _fusionValue.GetPropertySolve();
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
		}

		public ScalarOperationFusion Offset(double value, FuseOperator operation)
		{
			return new ScalarOperationFusion(this, operation, value);
		}


		public override double GetPropertySolve()
		{
			if (!SourceElementVisible)
				return 0;

			return (double)Fusion.GetViewProperty(SourceElement, SourceProperty);
		}
	}


	internal class FusionRootView : FusionBase
	{
		internal FusionRootView(VisualElement sourceElement) : base(null)
		{
			SourceElement = sourceElement;
		}
	}

	public class Fusion : BindableObject
	{
		FusionRootView _rootView;
		public Fusion(VisualElement sourceElement)  
		{
			_rootView = new FusionRootView(sourceElement);
		}

		public CenterFusion Center  => new CenterFusion(_rootView);
		public ScalarPropertyFusion CenterX => new ScalarPropertyFusion(_rootView, FuseProperty.CenterX);
		public ScalarPropertyFusion CenterY => new ScalarPropertyFusion(_rootView, FuseProperty.CenterY);

		public SizeFusion Size => new SizeFusion(_rootView);
		public ScalarPropertyFusion Right => new ScalarPropertyFusion(_rootView, FuseProperty.Right);
		public ScalarPropertyFusion Bottom => new ScalarPropertyFusion(_rootView, FuseProperty.Bottom);
		public ScalarPropertyFusion Left => new ScalarPropertyFusion(_rootView, FuseProperty.Left);
		public ScalarPropertyFusion Top => new ScalarPropertyFusion(_rootView, FuseProperty.Top);

		public ScalarPropertyFusion X => new ScalarPropertyFusion(_rootView, FuseProperty.X);
		public ScalarPropertyFusion Y => new ScalarPropertyFusion(_rootView, FuseProperty.Y);
		public ScalarPropertyFusion Height => new ScalarPropertyFusion(_rootView, FuseProperty.Height);
		public ScalarPropertyFusion Width => new ScalarPropertyFusion(_rootView, FuseProperty.Width);

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

	}

	public class ConditionalTargetWrapper : TargetWrapper
	{
		List<(Func<bool>, FusionBase[])> _fusions;
		public ConditionalTargetWrapper(FusionBase fusion, FuseProperty targetProperty, View targetView) 
			: base(fusion, targetProperty, targetView)
		{
			_fusions = new List<(Func<bool>, FusionBase[])>();
		}

		public override void SetProperty<T>(Action<T> propertyToSet)
		{
			for (int i = 0; i < _fusions.Count; i++)
			{
				if (_fusions[i].Item1())
				{
					
					break;
				}
			}
		}

		public void AddSet(Func<bool> condition, params FusionBase[] fusions)
		{
			_fusions.Add((condition, fusions));
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

		public virtual void SetProperty<T>(Action<T> propertyToSet)
		{			
			propertyToSet(((IFusion<T>)Fuse).GetPropertySolve());
		}
	}

}
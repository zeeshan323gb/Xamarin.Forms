using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Xamarin.Forms.Core
{
	public class FusedLayout : Layout<View>, IElementConfiguration<FusedLayout>
	{
		public static readonly BindableProperty FusesProperty =
			BindableProperty.Create("FusesProperty", typeof(FuseCollection), typeof(FusedLayout), null);



		// Node List? or better to break out to SolveNodeX, SolveNodeYProperty etc...
		public static readonly BindableProperty SolveViewProperty =
			BindableProperty.Create("SolveViewProperty", typeof(SolveView), typeof(FusedLayout), null);


		readonly Lazy<PlatformConfigurationRegistry<FusedLayout>> _platformConfigurationRegistry;
		readonly FusedElementCollection _children;


		public FusedLayout()
		{
			_children = new FusedElementCollection(InternalChildren, this);
			_children.Parent = this;

			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<FusedLayout>>(() =>
				new PlatformConfigurationRegistry<FusedLayout>(this));
		}

		public new IFusedLayoutList<View> Children
		{
			get { return _children; }
		}

		public static FuseCollection GetFuses(BindableObject bindable)
		{
			return (FuseCollection)bindable.GetValue(FusesProperty);
		}

		public static void SetFuses(BindableObject bindable, FuseCollection value)
		{
			bindable.SetValue(FusesProperty, value);
		}

		internal static SolveView GetSolveView(BindableObject bindable)
		{
			return (SolveView)bindable.GetValue(SolveViewProperty);
		}

		internal static void SetSolveView(BindableObject bindable, SolveView value)
		{
			bindable.SetValue(SolveViewProperty, value);
		}


		public static void AddFusion(View targetView, FuseBase fuse)
		{
			var currentFuses = GetFuses(targetView) ?? new FuseCollection();
			currentFuses.Add(fuse);
			SetFuses(targetView, currentFuses);
		}


		public static void RemoveFusion(View targetView, FuseBase fuse)
		{
			var currentFuses = GetFuses(targetView);
			currentFuses.Remove(fuse);
			SetFuses(targetView, currentFuses);
		}


		public IPlatformElementConfiguration<T, FusedLayout> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}



		protected override void LayoutChildren(double x, double y, double width, double height)
		{

			// this setup could just be cached after the first run through
			// then subsequent run throughs would just reset the nodes
			// plus on a second run through the order would be more known
			SolveNodeList unSolvedNodeList = new SolveNodeList();
			List<SolveView> unSolvedViews = new List<SolveView>();
			foreach (var view in _children)
			{
				var fusesToSolveView = GetFuses(view);
				SolveNodeList viewNodes = new SolveNodeList();
				SolveView solveView = new SolveView(view);
				unSolvedNodeList.AddRange(solveView.Nodes);
				SetSolveView(view, solveView);
				unSolvedViews.Add(solveView);
			}

			// in theory after the first run through this contains the order
			SolveNodeList solved = new SolveNodeList();

			var layoutSolveView = new SolveView(this);
			SetSolveView(this, layoutSolveView);

			bool iSolvedSomething = true;
			while (unSolvedNodeList.Count > 0 && iSolvedSomething)
			{
				iSolvedSomething = false;
				// solve is a single view being solved over
				foreach (var unsolved in unSolvedNodeList.ToArray())
				{
					if (unsolved.TrySolve())
					{
						unSolvedNodeList.Remove(unsolved);
						solved.Add(unsolved);
						iSolvedSomething = true;
					}
				}
			}

			if (unSolvedNodeList.Count == 0)
			{
				foreach (var apply in unSolvedViews)
				{
					apply.Apply();
				}
			}
		}
		public interface IFusedLayoutList<T> : IList<T> where T : View
		{
			void Add(T view, FuseBase fuse);
		}

		class FusedElementCollection : ElementCollection<View>, IFusedLayoutList<View>
		{
			public FusedElementCollection(ObservableCollection<Element> inner, FusedLayout parent) : base(inner)
			{
				Parent = parent;
			}
			internal FusedLayout Parent { get; set; }

			public void Add(View view, FuseBase fuse)
			{
				AddFusion(view, fuse);
				base.Add(view);
			}
		}



		/// <summary>
		/// Quick naive dependency tree
		/// </summary>
		/// <param name="viewProperty">x,y,height,width</param>
		/// <param name="specialProperty"></param>
		/// <returns></returns>
		internal static bool DoesViewPropertyDependOnProperty(FuseProperty viewProperty, FuseProperty specialProperty)
		{
			switch (viewProperty)
			{
				case FuseProperty.X:
					switch (specialProperty)
					{
						case FuseProperty.X:
						case FuseProperty.Center:
							return true;
						case FuseProperty.Y:
							return false;
						default:
							throw new ArgumentException($"{viewProperty}-{specialProperty}");

					}
				case FuseProperty.Y:
					switch (specialProperty)
					{
						case FuseProperty.Y:
						case FuseProperty.Center:
							return true;
						case FuseProperty.X:
							return false;
						default:
							throw new ArgumentException($"{viewProperty}-{specialProperty}");

					}
				case FuseProperty.Height:
					switch (specialProperty)
					{
						case FuseProperty.Height:
							return true;
						case FuseProperty.Width:
						case FuseProperty.X:
						case FuseProperty.Center:
							return false;
						default:
							throw new ArgumentException($"{viewProperty}-{specialProperty}");

					}
				case FuseProperty.Width:
					switch (specialProperty)
					{
						case FuseProperty.Height:
							return true;
						case FuseProperty.Width:
						case FuseProperty.X:
						case FuseProperty.Center:
							return false;
						default:
							throw new ArgumentException($"{viewProperty}-{specialProperty}");

					}
				default:
					throw new ArgumentException($"{viewProperty}-{specialProperty}");
			}
		}

		public class SolveNode
		{
			public SolveNode(
				View targetView,
				FuseProperty fuseProperty,
				FuseCollection viewFuses)
			{
				TargetView = targetView;
				Property = fuseProperty;
				Fuses = new FuseCollection();
				if (viewFuses != null)
				{
					foreach (var fuseToSolveView in viewFuses)
					{
						if (DoesViewPropertyDependOnProperty(fuseProperty, fuseToSolveView.SourceProperty))
						{
							Fuses.Add(fuseToSolveView);
						}
					}
				}
			}

			public View TargetView { get; set; }

			public bool IsSolved => Value.HasValue;

			// Fuses relevant to this Property
			public FuseCollection Fuses { get; }
			public FuseProperty Property { get; }

			public double? Value { get; set; }


			public void SetValue(double value)
			{
				Value = value;
			}

			public bool TrySolve()
			{
				if(IsSolved)
				{
					return true;
				}

				double returnValue = 0;
				foreach (var fuse in Fuses)
				{
					var result = fuse.Calculate(TargetView, Property);
					if (!result.HasValue)
					{
						return false;
					}
					returnValue += result.Value;
				}

				// naive averaged bais of values
				SetValue(returnValue / (Fuses.Count));
				return true;
			}
		}

		public class SolveView
		{
			public View TargetElement { get; set; }
			public FuseCollection Fuses { get; private set; }
			public SolveNodeList Nodes { get; private set; }

			public SolveView(View target)
			{
				TargetElement = target;
				Fuses = GetFuses(target);

				X = new SolveNode(target, FuseProperty.X, Fuses);
				Y = new SolveNode(target, FuseProperty.Y, Fuses);
				Height = new SolveNode(target, FuseProperty.Height, Fuses);
				Width = new SolveNode(target, FuseProperty.Width, Fuses);

				// just setup defaults for the solve if no fuse is specified
				if(Height.Fuses.Count == 0)
				{
					if(TargetElement.HeightRequest == -1)
						Height.SetValue(TargetElement.Height);
					else
						Height.SetValue(TargetElement.HeightRequest);
				}

				if(Width.Fuses.Count == 0)
				{
					if (TargetElement.WidthRequest == -1)
						Width.SetValue(TargetElement.Width);
					else
						Width.SetValue(TargetElement.WidthRequest);
				}

				if (X.Fuses.Count == 0)
				{
					X.SetValue(TargetElement.X);
				}

				if (Y.Fuses.Count == 0)
				{
					Y.SetValue(TargetElement.Y);
				}

				Nodes = new SolveNodeList();
				Nodes.Add(X);
				Nodes.Add(Y);
				Nodes.Add(Height);
				Nodes.Add(Width);
			}

			public void Apply()
			{
				TargetElement.Layout(
					new Rectangle(
						X.Value.Value,
						Y.Value.Value,
						Height.Value.Value,
						Width.Value.Value
						)
					);
			}

			public SolveNode X { get; set; }
			public SolveNode Y { get; set; }
			public SolveNode Height { get; set; }
			public SolveNode Width { get; set; }


			public bool IsSolved =>
				X.IsSolved &&
				Y.IsSolved &&
				Height.IsSolved &&
				Width.IsSolved;
		}


		public class SolveNodeList : List<SolveNode>
		{
			public List<SolveNode> GetForProperty(FuseProperty fuseProperty)
			{
				List<SolveNode> properties = new List<SolveNode>();

				foreach (var item in this)
				{
					if (item.Property == fuseProperty)
					{
						properties.Add(item);
					}
				}

				return properties;
			}
		}
	}





	public class FuseCollection : List<FuseBase> // DefinitionCollection<FuseBase> or other base
	{

	}

	public abstract class FuseBase : BindableObject
	{
		public VisualElement SourceElement { get; set; } // BP 

		public FuseProperty SourceProperty { get; set; } // BP 

		public abstract double? Calculate(VisualElement targetView, FuseProperty property);
	}

	public class Fuse : FuseBase
	{
		public FuseProperty TargetProperty { get; set; } // BP 

		public FuseOperator Operator { get; set; } // BP 

		public double Constant { get; set; } // BP 


		public override double? Calculate(VisualElement targetView, FuseProperty property)
		{
			var dependentSourceSolve = FusedLayout.GetSolveView(SourceElement);
			var mySourceSolve = FusedLayout.GetSolveView(targetView);

			double? returnValue = null;

			switch (SourceProperty)
			{
				case FuseProperty.X:
					{
						returnValue = dependentSourceSolve.X.Value;
						break;
					}
				case FuseProperty.Center:
					{
						switch (property)
						{
							case FuseProperty.X:
								if (dependentSourceSolve.X.IsSolved && 
									dependentSourceSolve.Width.IsSolved && 
									mySourceSolve.Width.IsSolved)
								{
									returnValue =
										((dependentSourceSolve.X.Value + dependentSourceSolve.Width.Value) / 2) -
										(mySourceSolve.Width.Value / 2);
								}
								break;
							case FuseProperty.Y:
								if (dependentSourceSolve.Y.IsSolved && 
									dependentSourceSolve.Height.IsSolved &&
									mySourceSolve.Height.IsSolved)
								{

									returnValue =
										((dependentSourceSolve.Y.Value + dependentSourceSolve.Height.Value) / 2) -
										(mySourceSolve.Height.Value / 2);
								}
								break;
						}
						break;
					}
				default:
					throw new ArgumentException($"Invalid SourceProperty: {SourceProperty}");
			}

			if (returnValue == null )
			{
				return null;
			}
			 
			if (returnValue.HasValue)
			{
				switch (Operator)
				{
					case FuseOperator.Minus:
						returnValue -= Constant;
						break;
					case FuseOperator.Plus:
						returnValue += Constant;
						break;
					case FuseOperator.None:
						break;
					default:
						throw new ArgumentException($"Invalid SourceProperty: {SourceProperty}");

				}
			}

			return returnValue;
		}
	}

	public enum FuseOperator
	{
		None = 0,
		Plus = 1,
		Minus = 2
	}

	public enum FuseProperty
	{
		None = 0,
		X,
		Y,
		Width,
		Height,
		Left,
		Top,
		Right,
		Bottom,
		Center,
		CenterX,
		Size
	}
}
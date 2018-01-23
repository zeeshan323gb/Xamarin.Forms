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


		public static void AddFusion(View targetView, FuseProperty targetProperty, FusionBase fuse)
		{
			var currentFuses = GetFuses(targetView) ?? new FuseCollection();
			currentFuses.Add(new TargetWrapper(fuse, targetProperty, targetView));
			SetFuses(targetView, currentFuses);
		}


		public static void RemoveFusion(View targetView, FuseProperty targetProperty, FusionBase removeFuse)
		{
			var currentFuses = GetFuses(targetView);

			foreach(var fuse in currentFuses.ToArray())
			{
				if(fuse.TargetProperty == targetProperty && fuse.Fuse == removeFuse)
				{
					currentFuses.Remove(fuse);
				}
			}

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
				SolveView solveView = new SolveView(view, null);
				unSolvedNodeList.AddRange(solveView.Nodes);
				SetSolveView(view, solveView);
				unSolvedViews.Add(solveView);
			}

			// in theory after the first run through this contains the order
			SolveNodeList solved = new SolveNodeList();

			var layoutSolveView = new SolveView(this, new Size(width, height));
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
			void Add(T view, FuseProperty targetProperty, FusionBase fuse);
		}

		class FusedElementCollection : ElementCollection<View>, IFusedLayoutList<View>
		{
			public FusedElementCollection(ObservableCollection<Element> inner, FusedLayout parent) : base(inner)
			{
				Parent = parent;
			}
			internal FusedLayout Parent { get; set; }

			public void Add(View view, FuseProperty targetProperty, FusionBase fuse)
			{
				AddFusion(view, targetProperty, fuse);
				base.Add(view);
			}
		}


		 
		public class SolveNode
		{
			public SolveNode(
				View targetView,
				FuseProperty fusePropertyXYHW,
				FuseCollection viewFuses)
			{
				TargetView = targetView;
				PropertyXYHW = fusePropertyXYHW;
				Fuses = new FuseCollection();
				if (viewFuses != null)
				{
					foreach (var fuseToSolveView in viewFuses)
					{
						if(fuseToSolveView.Influences(fusePropertyXYHW)) 
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
			public FuseProperty PropertyXYHW { get; }

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
					var result = fuse.Calculate(PropertyXYHW);
					
					if (!result.HasValue)
					{
						return false;
					}
					returnValue += result.Value;
				}

				// naive averaged bias of values
				SetValue(returnValue / (Fuses.Count));
				return true;
			}
		}

		public class SolveView
		{
			public View TargetElement { get; set; }
			public FuseCollection Fuses { get; private set; }
			public SolveNodeList Nodes { get; private set; }

			public SolveView(View target, Size? measureRequest)
			{
				TargetElement = target;
				Fuses = GetFuses(target);

				X = new SolveNode(target, FuseProperty.X, Fuses);
				Y = new SolveNode(target, FuseProperty.Y, Fuses);
				Height = new SolveNode(target, FuseProperty.Height, Fuses);
				Width = new SolveNode(target, FuseProperty.Width, Fuses);

				// just measure here? or create a Measure Fusion to put in here
				if(Height.Fuses.Count == 0)
				{
					measureRequest = measureRequest ?? TargetElement.Measure(Double.PositiveInfinity, Double.PositiveInfinity).Request;
					Height.SetValue(measureRequest.Value.Height);
				}
				
				if (Width.Fuses.Count == 0)
				{
					measureRequest = measureRequest ?? TargetElement.Measure(Double.PositiveInfinity, Double.PositiveInfinity).Request;
					Width.SetValue(measureRequest.Value.Width);
				}

				if (X.Fuses.Count == 0)
				{
					X.SetValue(0);
				}

				if (Y.Fuses.Count == 0)
				{
					Y.SetValue(0);
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
					if (item.PropertyXYHW == fuseProperty)
					{
						properties.Add(item);
					}
				}

				return properties;
			}
		}
	}





	public class FuseCollection : List<TargetWrapper> 
	{

	}


	public enum FuseOperator
	{
		None = 0,
		Add = 1,
		Subtract = 2
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
using System;
using Android.Graphics;
using Android.Text;
using Android.Views;
using Android.Widget;
using Android.Views.InputMethods;
using AView = Android.Views.View;
using Java.Lang;
using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	internal struct FastVisualElementRenderer<T> :
		IVisualElementRenderer,
		IEffectControlProvider
	{
		readonly EffectControlProvider _effectControlProvider;

		VisualElement _element;
		AView _view;
		VisualElementTracker _tracker;
		bool _disposed;
		int? _defaultLabelFor;

		internal FastVisualElementRenderer(AView view) {
			_effectControlProvider = new EffectControlProvider(view);

			_view = view;
			_defaultLabelFor = null;
			_tracker = null;
			_disposed = false;
			_element = default(VisualElement);

			ElementChanged = null;
			ElementPropertyChanged = null;
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e) { }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public AView View 
			=> _view;
		public ViewGroup ViewGroup 
			=> null;
		public VisualElementTracker Tracker
			=> _tracker;
		public VisualElement Element
			=> _element;

		public void SetTracker(VisualElementTracker tracker)
			=> _tracker = tracker;
		public void SetElement(VisualElement newElement)
		{
			if (!(newElement is T))
				throw new ArgumentException(
					$"Element is not of type '{typeof(T)}'.", nameof(newElement));

			var oldElement = _element;
			if (oldElement != null)
				oldElement.PropertyChanged -= OnElementPropertyChanged;

			_element = newElement;

			// inherit BackgroundColor
			var oldColor = oldElement?.BackgroundColor ?? Color.Default;
			var newColor = _element.BackgroundColor;
			if (newColor != oldColor)
				_view.SetBackgroundColor(newColor.ToAndroid());

			// raise ElementChanged
			ElementChanged?.Invoke(this, 
				new VisualElementChangedEventArgs(oldElement, _element));

			ElevationHelper.SetElevation(_view, _element);

			_element.SendViewInitialized(_view);

			EffectUtilities.RegisterEffectControlProvider(
				_effectControlProvider, oldElement, _element);
		}

		public SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			if (_disposed)
				return new SizeRequest();

			_view.Measure(widthConstraint, heightConstraint);

			return new SizeRequest(
				request: new Size(
					width: _view.MeasuredWidth,
					height: _view.MeasuredHeight
				), 
				minimum: new Size()
			);
		}

		public void SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = _view.LabelFor;

			_view.LabelFor = id ?? (int)_defaultLabelFor;
		}

		public void UpdateLayout() 
			=> _tracker?.UpdateLayout();

		public void RegisterEffect(Effect effect)
			=> _effectControlProvider.RegisterEffect(effect);

		public void Dispose() 
			=> _disposed = true;
	}

	internal sealed class EntryRenderer : FormsEditText,
		TextView.IOnEditorActionListener,
		ITextWatcher,
		IVisualElementRenderer,
		IEffectControlProvider
	{
		FastVisualElementRenderer<EntryRenderer> _visualElementRenderer;

		public EntryRenderer() : base(Forms.Context)
		{
			_visualElementRenderer = new FastVisualElementRenderer<EntryRenderer>(this);
		}

		Entry Entry => (Entry)_visualElementRenderer.Element;
		IElementController ElementController => Entry;
		IEntryController EntryController => Entry;

		bool IOnEditorActionListener.OnEditorAction(TextView textView, ImeAction action, KeyEvent keyEvent)
		{
			var done = action == ImeAction.Done;
			var enter = action == ImeAction.ImeNull && keyEvent.KeyCode == Keycode.Enter;

			if (done || enter)
			{
				ClearFocus();
				textView.HideKeyboard();
				EntryController.SendCompleted();
			}

			return true;
		}

		// ITextWatcher
		void ITextWatcher.AfterTextChanged(IEditable value) { /* empty */ }
		void ITextWatcher.BeforeTextChanged(ICharSequence value, int start, int count, int after) { /* empty */ }
		void ITextWatcher.OnTextChanged(ICharSequence value, int start, int before, int count)
		{
			var stringValue = value.ToString();

			if (string.IsNullOrEmpty(Entry.Text) && string.IsNullOrEmpty(stringValue))
				return;

			ElementController.SetValueFromRenderer(Entry.TextProperty, stringValue);
		}

		// IVisualElementRenderer (boilerplate)
		public event EventHandler<VisualElementChangedEventArgs> ElementChanged
		{
			add { _visualElementRenderer.ElementChanged += value; }
			remove { _visualElementRenderer.ElementChanged -= value; }
		}
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged
		{
			add { _visualElementRenderer.ElementPropertyChanged += value; }
			remove { _visualElementRenderer.ElementPropertyChanged += value; }
		}
		void IVisualElementRenderer.SetElement(VisualElement element)
			=> _visualElementRenderer.SetElement(element);
		VisualElement IVisualElementRenderer.Element 
			=> _visualElementRenderer.Element;
		VisualElementTracker IVisualElementRenderer.Tracker
			=> _visualElementRenderer.Tracker;
		AView IVisualElementRenderer.View 
			=> _visualElementRenderer.View;
		ViewGroup IVisualElementRenderer.ViewGroup 
			=> _visualElementRenderer.ViewGroup;
		void IVisualElementRenderer.SetLabelFor(int? id) 
			=> _visualElementRenderer.SetLabelFor(id);
		void IVisualElementRenderer.UpdateLayout() 
			=> _visualElementRenderer.UpdateLayout();
		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
			=> _visualElementRenderer.GetDesiredSize(widthConstraint, heightConstraint);

		// IEffectControlProvider (boilerplate)
		void IEffectControlProvider.RegisterEffect(Effect effect)
			=> _visualElementRenderer.RegisterEffect(effect);

	}
}

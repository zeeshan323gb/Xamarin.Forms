using System;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Views;
using AView = Android.Views.View;
using Android.Text.Method;
using Android.App;
using Android.OS;
using Java.Lang;
using Android.Support.V4.Graphics.Drawable;
using Android.Graphics.Drawables;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	public class EditorRenderer : FormsEditText, IVisualElementRenderer, ITextWatcher, IEffectControlProvider, AView.IOnFocusChangeListener
	{
		int? _defaultLabelFor;
		ColorStateList _defaultColors;
		Drawable _defaultDrawable;
		bool _disposed;
		Editor _element;
		readonly EffectControlProvider _effectControlProvider;
		VisualElementTracker _visualElementTracker;
		VisualElementRenderer _visualElementRenderer;
		SoftInput _startingInputMode;

		public EditorRenderer() : base(Forms.Context)
		{
			//_visualElementRenderer = new VisualElementRenderer(this);
			_defaultDrawable = Background;
			_effectControlProvider = new EffectControlProvider(this);
			OnFocusChangeListener = this;
		}

		protected Editor Element
		{
			get { return _element; }
			set
			{
				if (_element == value)
					return;

				Editor oldElement = _element;
				_element = value;

				OnElementChanged(new ElementChangedEventArgs<Editor>(oldElement, _element));

				_element?.SendViewInitialized(this);
			}
		}

		VisualElement IVisualElementRenderer.Element => Element;

		public VisualElementTracker Tracker => _visualElementTracker;

		public ViewGroup ViewGroup => null;

		public AView View => this;

		IEditorController ElementController => Element;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			if (_disposed)
			{
				return new SizeRequest();
			}

			Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), new Size());
		}

		void ITextWatcher.AfterTextChanged(IEditable s)
		{
		}

		void ITextWatcher.BeforeTextChanged(ICharSequence s, int start, int count, int after)
		{
		}

		void ITextWatcher.OnTextChanged(ICharSequence s, int start, int before, int count)
		{
			if (Element == null || string.IsNullOrEmpty(Element.Text) && s.Length() == 0)
				return;

			if (Element.Text != s.ToString())
				((IElementController)Element).SetValueFromRenderer(Editor.TextProperty, s.ToString());
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			var editor = element as Editor;
			if (editor == null)
				throw new ArgumentException("Element must be of type Editor");

			Element = editor;
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = LabelFor;

			LabelFor = (int)(id ?? _defaultLabelFor);
		}

		void IVisualElementRenderer.UpdateLayout()
		{
			VisualElementTracker tracker = _visualElementTracker;
			tracker?.UpdateLayout();
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				OnKeyboardBackPressed -= OnKeyboardBackButtonPressed;

				if (_visualElementTracker != null)
				{
					_visualElementTracker.Dispose();
					_visualElementTracker = null;
				}

				if (_visualElementTracker != null)
				{
					_visualElementRenderer.Dispose();
					_visualElementRenderer = null;
				}

				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;
				}
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			if (e.OldElement != null)
			{
			}

			if (e.NewElement != null)
			{
				this.EnsureId();

				if (_visualElementTracker == null)
				{
					_visualElementTracker = new VisualElementTracker(this);
				}

				e.NewElement.PropertyChanged += OnElementPropertyChanged;
			}
			
			OnKeyboardBackPressed += OnKeyboardBackButtonPressed;
			SetSingleLine(false);
			Gravity = GravityFlags.Top;
			SetHorizontallyScrolling(false);

			UpdateText();
			UpdateInputType();
			UpdateTextColor();
			UpdateFont();

			//Background.SetColorFilter(global::Android.Graphics.Color.MediumPurple, PorterDuff.Mode.SrcIn);
			//SetBackgroundColor(Element.BackgroundColor.ToAndroid());

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Editor.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == InputView.KeyboardProperty.PropertyName)
				UpdateInputType();
			else if (e.PropertyName == Editor.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Editor.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();

			ElementPropertyChanged?.Invoke(this, e);
		}

		void UpdateBackgroundColor()
		{
			if (!Element.BackgroundColor.IsDefault)
			{
				SetBackgroundColor(Element.BackgroundColor.ToAndroid());
			}
			else
			{
				Background = _defaultDrawable;
			}
			//DrawableCompat.SetTintMode(Background, PorterDuff.Mode.SrcAtop);
			//DrawableCompat.SetTint(Background, Color.MediumPurple.ToAndroid());
		}

		protected virtual NumberKeyListener GetDigitsKeyListener(InputTypes inputTypes)
		{
			// Override this in a custom renderer to use a different NumberKeyListener 
			// or to filter out input types you don't want to allow 
			// (e.g., inputTypes &= ~InputTypes.NumberFlagSigned to disallow the sign)
			return LocalizedDigitsKeyListener.Create(inputTypes);
		}

		void UpdateFont()
		{
			Typeface = Element.ToTypeface();
			SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

		void UpdateInputType()
		{
			Editor model = Element;
			var keyboard = model.Keyboard;

			InputType = keyboard.ToInputType() | InputTypes.TextFlagMultiLine;

			if (keyboard == Keyboard.Numeric)
			{
				KeyListener = GetDigitsKeyListener(InputType);
			}
		}

		void UpdateText()
		{
			string newText = Element.Text ?? "";

			if (Text == newText)
				return;

			Text = newText;
			SetSelection(newText.Length);
		}

		void UpdateTextColor()
		{
			if (Element.TextColor.IsDefault)
			{
				if (_defaultColors == null)
				{
					// This control has always had the default colors; nothing to update
					return;
				}

				// This control is being set back to the default colors
				SetTextColor(_defaultColors);
			}
			else
			{
				if (_defaultColors == null)
				{
					// Keep track of the default colors so we can return to them later
					// and so we can preserve the default disabled color
					_defaultColors = TextColors;
				}

				SetTextColor(Element.TextColor.ToAndroidPreserveDisabled(_defaultColors));
			}
		}

		void OnKeyboardBackButtonPressed(object sender, EventArgs eventArgs)
		{
			ElementController?.SendCompleted();
			ClearFocus();
		}

		void IOnFocusChangeListener.OnFocusChange(AView v, bool hasFocus)
		{
			var isInViewCell = false;
			Element parent = Element.RealParent;
			while (!(parent is Page) && parent != null)
			{
				if (parent is Cell)
				{
					isInViewCell = true;
					break;
				}
				parent = parent.RealParent;
			}

			if (isInViewCell)
			{
				Window window = ((Activity)Context).Window;
				if (hasFocus)
				{
					_startingInputMode = window.Attributes.SoftInputMode;
					window.SetSoftInputMode(SoftInput.AdjustPan);
				}
				else
					window.SetSoftInputMode(_startingInputMode);
			}
			OnNativeFocusChanged(hasFocus);

			if (hasFocus)
				v.ShowKeyboard();
			else
				v.HideKeyboard();

			((IElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, hasFocus);
		}

		internal void OnNativeFocusChanged(bool hasFocus)
		{
			if (Element.IsFocused && !hasFocus) // Editor has requested an unfocus, fire completed event
				ElementController.SendCompleted();
		}

		public void RegisterEffect(Effect effect)
		{
			_effectControlProvider.RegisterEffect(effect);
		}
	}
}
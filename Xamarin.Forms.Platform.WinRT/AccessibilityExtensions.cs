using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;

#if WINDOWS_UWP
namespace Xamarin.Forms.Platform.UWP
#else
namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public static class AccessibilityExtensions
	{
		public static string SetAutomationPropertiesName(this FrameworkElement Control, Element Element, string _defaultAutomationPropertiesName = null)
		{
			if (Element == null)
				return _defaultAutomationPropertiesName;

			if (_defaultAutomationPropertiesName == null)
				_defaultAutomationPropertiesName = (string)Control.GetValue(AutomationProperties.NameProperty);

			var elemValue = (string)Element.GetValue(Accessibility.NameProperty);

			if (!string.IsNullOrWhiteSpace(elemValue))
				Control.SetValue(AutomationProperties.NameProperty, elemValue);
			else
				Control.SetValue(AutomationProperties.NameProperty, _defaultAutomationPropertiesName);

			return _defaultAutomationPropertiesName;
		}

		public static AccessibilityView? SetAutomationPropertiesAccessibilityView(this FrameworkElement Control, Element Element, AccessibilityView? _defaultAutomationPropertiesAccessibilityView = null)
		{
			if (Element == null)
				return _defaultAutomationPropertiesAccessibilityView;

			if (!_defaultAutomationPropertiesAccessibilityView.HasValue)
				_defaultAutomationPropertiesAccessibilityView = (AccessibilityView)Control.GetValue(AutomationProperties.AccessibilityViewProperty);

			var newValue = _defaultAutomationPropertiesAccessibilityView;

			var elemValue = (bool?)Element.GetValue(Accessibility.IsInAccessibleTreeProperty);

			if (elemValue == true)
				newValue = AccessibilityView.Content;

			else if (elemValue == false)
				newValue = AccessibilityView.Raw;

			Control.SetValue(AutomationProperties.AccessibilityViewProperty, newValue);

			return _defaultAutomationPropertiesAccessibilityView;
		}

		public static string SetAutomationPropertiesHelpText(this FrameworkElement Control, Element Element, string _defaultAutomationPropertiesHelpText = null)
		{
			if (Element == null)
				return _defaultAutomationPropertiesHelpText;

			if (_defaultAutomationPropertiesHelpText == null)
				_defaultAutomationPropertiesHelpText = (string)Control.GetValue(AutomationProperties.HelpTextProperty);

			var elemValue = (string)Element.GetValue(Accessibility.HintProperty);

			if (!string.IsNullOrWhiteSpace(elemValue))
				Control.SetValue(AutomationProperties.HelpTextProperty, elemValue);
			else
				Control.SetValue(AutomationProperties.HelpTextProperty, _defaultAutomationPropertiesHelpText);

			return _defaultAutomationPropertiesHelpText;
		}

		public static UIElement SetAutomationPropertiesLabeledBy(this FrameworkElement Control, Element Element, UIElement _defaultAutomationPropertiesLabeledBy = null)
		{
			if (Element == null)
				return _defaultAutomationPropertiesLabeledBy;

			if (_defaultAutomationPropertiesLabeledBy == null)
				_defaultAutomationPropertiesLabeledBy = (UIElement)Control.GetValue(AutomationProperties.LabeledByProperty);

			var elemValue = (VisualElement)Element.GetValue(Accessibility.LabeledByProperty);

			var renderer = elemValue?.GetOrCreateRenderer();

			var nativeElement = renderer?.GetNativeElement();

			if (nativeElement != null)
				Control.SetValue(AutomationProperties.LabeledByProperty, nativeElement);
			else
				Control.SetValue(AutomationProperties.LabeledByProperty, _defaultAutomationPropertiesLabeledBy);

			return _defaultAutomationPropertiesLabeledBy;
		}

		// TODO: This is not having any effect on anything I've tested yet. See if we need it 
		// after we test the MDP and NP w/ back button explicitly enabled.
		public static void SetBackButtonTitle(this PageControl Control, Element Element)
		{
			if (Element == null)
				return;

			var elemValue = ConcatenateNameAndHint(Element);

			Control.BackButtonTitle = elemValue;
		}

		static string ConcatenateNameAndHint(Element Element)
		{
			string separator;

			var name = (string)Element.GetValue(Accessibility.NameProperty);
			var hint = (string)Element.GetValue(Accessibility.HintProperty);

			if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(hint))
				separator = "";
			else
				separator = ". ";

			return string.Join(separator, name, hint);
		}
	}
}

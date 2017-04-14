using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class ViewRenderer<TElement, TNativeElement> : VisualElementRenderer<TElement, TNativeElement> where TElement : View where TNativeElement : FrameworkElement
	{
		string _defaultAutomationPropertiesName;
		AccessibilityView? _defaultAutomationPropertiesAccessibilityView;
		string _defaultAutomationPropertiesHelpText;
		UIElement _defaultAutomationPropertiesLabeledBy;

		protected override void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				UpdateBackgroundColor();
			}
		}

		protected override void SetAutomationId(string id)
		{
			if (Control == null)
			{
				base.SetAutomationId(id);
			}
			else
			{
				this.SetAutomationPropertiesAutomationId($"{id}_Container");
				Control.SetAutomationPropertiesAutomationId(id);
			}
		}
		protected override void SetAutomationPropertiesName()
		{
			if (Control == null)
			{
				base.SetAutomationPropertiesName();
				return;
			}

			_defaultAutomationPropertiesName = Control.SetAutomationPropertiesName(Element, _defaultAutomationPropertiesName);
		}

		protected override void SetAutomationPropertiesAccessibilityView()
		{
			if (Control == null)
			{
				base.SetAutomationPropertiesAccessibilityView();
				return;
			}

			_defaultAutomationPropertiesAccessibilityView = Control.SetAutomationPropertiesAccessibilityView(Element, _defaultAutomationPropertiesAccessibilityView);
		}

		protected override void SetAutomationPropertiesHelpText()
		{
			if (Control == null)
			{
				base.SetAutomationPropertiesHelpText();
				return;
			}

			_defaultAutomationPropertiesHelpText = Control.SetAutomationPropertiesHelpText(Element, _defaultAutomationPropertiesHelpText);
		}

		protected override void SetAutomationPropertiesLabeledBy()
		{
			if (Control == null)
			{
				base.SetAutomationPropertiesLabeledBy();
				return;
			}

			_defaultAutomationPropertiesLabeledBy = Control.SetAutomationPropertiesLabeledBy(Element, _defaultAutomationPropertiesLabeledBy);
		}
	}
}
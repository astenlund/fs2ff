using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace fs2ff.Behaviors
{
    public class MoveFocusOnEnterBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject != null)
                AssociatedObject.KeyDown += AssociatedObject_KeyDown;
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
                AssociatedObject.KeyDown -= AssociatedObject_KeyDown;

            base.OnDetaching();
        }

        private static void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is UIElement element && e.Key == Key.Return)
                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}

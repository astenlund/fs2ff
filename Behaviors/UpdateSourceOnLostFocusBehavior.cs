using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

namespace fs2ff.Behaviors
{
    public class UpdateSourceOnLostFocusBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject != null)
                AssociatedObject.LostFocus += AssociatedObject_LostFocus;
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
                AssociatedObject.LostFocus -= AssociatedObject_LostFocus;

            base.OnDetaching();
        }

        private static void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
                BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty)?.UpdateSource();
        }
    }
}

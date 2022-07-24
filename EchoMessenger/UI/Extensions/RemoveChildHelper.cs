using System;
using System.Windows;
using System.Windows.Controls;

namespace EchoMessenger.UI.Extensions
{
    public static class RemoveChildHelper
    {
        public static void RemoveChild(this DependencyObject parent, UIElement child)
        {
            if (parent is Panel panel)
                panel.Children.Remove(child);
            else if (parent is Decorator decorator)
                if (decorator.Child == child)
                    decorator.Child = null;

            var contentPresenter = parent as ContentPresenter;
            if (contentPresenter != null)
            {
                if (contentPresenter.Content == child)
                {
                    contentPresenter.Content = null;
                }
                return;
            }

            var contentControl = parent as ContentControl;
            if (contentControl != null)
            {
                if (contentControl.Content == child)
                {
                    contentControl.Content = null;
                }
                return;
            }
        }
    }
}
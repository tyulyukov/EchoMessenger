using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EchoMessenger.UI.Controls.Typing
{
    public class TypingIndicator : Control
    {
        public static readonly DependencyProperty DiameterProperty =
            DependencyProperty.Register("Diameter", typeof(double), typeof(TypingIndicator),
                new PropertyMetadata(5d));

        public double Diameter
        {
            get { return (double)GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(TypingIndicator),
                new PropertyMetadata(Brushes.White));

        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.Register("Spacing", typeof(Thickness), typeof(TypingIndicator),
                new PropertyMetadata(new Thickness(2d)));

        public Thickness Spacing
        {
            get { return (Thickness)GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        static TypingIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TypingIndicator), new FrameworkPropertyMetadata(typeof(TypingIndicator)));
        }
    }
}

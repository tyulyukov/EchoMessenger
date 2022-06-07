using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace EchoMessenger.Helpers
{
    public static class Extensions
    {
        public static void SetPercent(this ProgressBar progressBar, double percentage, TimeSpan duration)
        {
            DoubleAnimation animation = new DoubleAnimation(percentage, duration);
            progressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
        }

        public static void ChangeVisibilityWithOpacity(this UIElement element, bool visible, TimeSpan duration)
        {
            double opacity;

            if (visible)
                opacity = 0.9;
            else
                opacity = 0;

            DoubleAnimation animation = new DoubleAnimation(opacity, duration);
            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        public static void ChangeVisibility(this UIElement element, bool visible, TimeSpan duration)
        {
            var animation = new DoubleAnimation
            {
                From = visible ? 0 : 1,
                To = visible ? 1 : 0,
                Duration = new Duration(duration)
            };

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Begin();
        }

        public static void ShowSmoothly(this UIElement element, TimeSpan duration)
        {
            var from = 0.3;

            Storyboard storyboard = new Storyboard();

            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            element.RenderTransformOrigin = new Point(0.5, 0.5);
            element.RenderTransform = scale;

            DoubleAnimation growAnimationX = new DoubleAnimation();
            growAnimationX.Duration = duration;
            growAnimationX.From = from;
            growAnimationX.To = 1;
            storyboard.Children.Add(growAnimationX);

            Storyboard.SetTargetProperty(growAnimationX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(growAnimationX, element);

            DoubleAnimation growAnimationY = new DoubleAnimation();
            growAnimationY.Duration = duration;
            growAnimationY.From = from;
            growAnimationY.To = 1;
            storyboard.Children.Add(growAnimationY);

            Storyboard.SetTargetProperty(growAnimationY, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(growAnimationY, element);

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.Duration = duration;
            opacityAnimation.From = 0;
            opacityAnimation.To = 1;

            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTarget(opacityAnimation, element);

            storyboard.Begin();
        }
    }
}

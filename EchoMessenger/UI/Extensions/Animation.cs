using EchoMessenger.UI.Controls.Loading;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using XamlFlair;

namespace EchoMessenger.UI.Extensions
{
    public static class Animation
    {
        public static readonly AnimationSettings FadeInAndSlideFromLeft = (AnimationSettings)Application.Current.FindResource("FadeInAndSlideFromLeft");
        public static readonly AnimationSettings FadeInAndSlideFromBottom = (AnimationSettings)Application.Current.FindResource("FadeInAndSlideFromBottom");
        public static readonly Style SelectableBorderStyle = (Style)Application.Current.FindResource("SelectableBorder");
        public static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(150);

        public static void SetPercent(this ProgressBar progressBar, double percentage, TimeSpan duration)
        {
            DoubleAnimation animation = new DoubleAnimation(percentage, duration);
            progressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
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

        public static void SetSlideFromLeftOnLoad(this UIElement element)
        {
            Animations.SetPrimary(element, FadeInAndSlideFromLeft);
        }

        public static void SetSlideFromBottomOnLoad(this UIElement element)
        {
            Animations.SetPrimary(element, FadeInAndSlideFromBottom);
        }

        public static void ChangeOpacity(this UIElement element, bool visible, TimeSpan? duration = null)
        {
            double opacity = visible ? 0.9 : 0;
            TimeSpan durationAnimation = (TimeSpan)(duration == null ? Duration : duration);

            DoubleAnimation animation = new DoubleAnimation(opacity, durationAnimation);
            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        public static void ChangeVisibility(this UIElement element, bool visible, TimeSpan? duration = null)
        {
            double toOpacity = visible ? 1 : 0;
            double fromOpacity = visible ? 0 : 1;
            TimeSpan durationAnimation = (TimeSpan)(duration == null ? Duration : duration);

            DoubleAnimation animation = new DoubleAnimation(fromOpacity, toOpacity, durationAnimation);
            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        public static void StartLoading(this LoadingSpinner spinner)
        {
            Storyboard storyboard = new Storyboard();
            storyboard.RepeatBehavior = RepeatBehavior.Forever;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 0;
            opacityAnimation.To = 1;
            opacityAnimation.SpeedRatio = 0.35;
            opacityAnimation.AutoReverse = true;

            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(LoadingSpinner.StrokeGapProperty));
            Storyboard.SetTarget(opacityAnimation, spinner);

            storyboard.Begin();
        }

        public static Border ConvertToSelectable(this UIElement element)
        {
            Border selectableBorder = new Border();
            selectableBorder.Style = SelectableBorderStyle;
            selectableBorder.Child = element;
            return selectableBorder;
        }
    }
}

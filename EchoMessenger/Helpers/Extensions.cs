using System;
using System.Windows;
using System.Windows.Controls;
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
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EchoMessenger.UI.Controls.Checks
{
    public class CheckMark : StackPanel
    {
        public CheckMark(bool isFirst)
        {
            Orientation = Orientation.Horizontal;

            if (isFirst)
            {
                var firstBorder = new Border();
                firstBorder.Background = new SolidColorBrush(Colors.White);
                firstBorder.CornerRadius = new CornerRadius(1);
                firstBorder.RenderTransformOrigin = new Point(0.5, 0.5);
                firstBorder.Width = 1.5;
                firstBorder.Height = 6;
                firstBorder.VerticalAlignment = VerticalAlignment.Bottom;
                firstBorder.Margin = new Thickness(1, 0, 1, 0);

                var firstTransformGroup = new TransformGroup();
                firstTransformGroup.Children.Add(new RotateTransform()
                {
                    Angle = -45
                });

                firstBorder.RenderTransform = firstTransformGroup;

                Children.Add(firstBorder);
            }

            var secondBorder = new Border();
            secondBorder.Background = new SolidColorBrush(Colors.White);
            secondBorder.CornerRadius = new CornerRadius(1);
            secondBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            secondBorder.Width = 1.5;
            secondBorder.Height = 10;
            secondBorder.VerticalAlignment = VerticalAlignment.Bottom;
            secondBorder.Margin = new Thickness(2, 0, 0, 0);

            var secondTransformGroup = new TransformGroup();
            secondTransformGroup.Children.Add(new RotateTransform()
            {
                Angle = 45
            });

            secondBorder.RenderTransform = secondTransformGroup;

            Children.Add(secondBorder);
        }
    }
}

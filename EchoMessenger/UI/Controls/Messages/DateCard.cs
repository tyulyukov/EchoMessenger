using EchoMessenger.Helpers.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EchoMessenger.UI.Controls.Messages
{
    public class DateCard : Grid
    {
        private static SolidColorBrush separationBrush = new SolidColorBrush(Colors.Gray);

        public DateCard(DateTime date) : base()
        {
            ColumnDefinitions.Add(new ColumnDefinition());
            ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            ColumnDefinitions.Add(new ColumnDefinition());

            var startLine = new Border();
            startLine.Height = 1;
            startLine.Background = separationBrush.Clone();
            startLine.HorizontalAlignment = HorizontalAlignment.Stretch;
            startLine.VerticalAlignment = VerticalAlignment.Center;

            Grid.SetColumn(startLine, 0);
            Children.Add(startLine);

            var endLine = new Border();
            endLine.Height = startLine.Height;
            endLine.Background = startLine.Background.Clone();
            endLine.HorizontalAlignment = startLine.HorizontalAlignment;
            endLine.VerticalAlignment = startLine.VerticalAlignment;

            Grid.SetColumn(endLine, 2);
            Children.Add(endLine);

            Label label = new Label();
            label.Foreground = separationBrush.Clone();
            label.Margin = new Thickness(3);

            if (date.IsToday())
                label.Content = "Today";
            else
                label.Content = date.ToLongDateString();

            Grid.SetColumn(label, 1);
            Children.Add(label);
        }
    }
}

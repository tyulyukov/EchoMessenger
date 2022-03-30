using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EchoMessenger.Helpers
{
    public static class UIMessageFactory
    {
        public static Border CreateOwnMessage(String text, DateTime dateTime)
        {
            var border = CreateDefaultBorder(text, dateTime);
            border.HorizontalAlignment = HorizontalAlignment.Right;
            border.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#283EFF");
            
            return border;
        }

        public static Border CreateForeignMessage(String text, DateTime dateTime)
        {
            var border = CreateDefaultBorder(text, dateTime);
            border.HorizontalAlignment = HorizontalAlignment.Left;
            border.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF1C1D26");

            return border;
        }

        private static Border CreateDefaultBorder(String text, DateTime time)
        {
            var border = new Border();
            border.MinHeight = 50;
            border.BorderBrush = new SolidColorBrush(Colors.White);
            border.BorderThickness = new Thickness(1);
            border.Margin = new Thickness(10);
            border.CornerRadius = new CornerRadius(30);

            var grid = new Grid();

            var messageColumn = new ColumnDefinition();
            messageColumn.MinWidth = 50;
            messageColumn.MaxWidth = 500;

            var timeColumn = new ColumnDefinition();

            grid.ColumnDefinitions.Add(messageColumn);
            grid.ColumnDefinitions.Add(timeColumn);

            var textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            textBlock.FontSize = 14;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.Margin = new Thickness(10);

            var timeTextBlock = new TextBlock();
            timeTextBlock.Text = time.ToString("HH:mm");
            timeTextBlock.Foreground = new SolidColorBrush(Colors.LightGray);
            timeTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
            timeTextBlock.Margin = new Thickness(0, 0, 10, 15);

            Grid.SetColumn(textBlock, 0);
            grid.Children.Add(textBlock);

            Grid.SetColumn(timeTextBlock, 1);
            grid.Children.Add(timeTextBlock);

            border.Child = grid;

            return border;
        }
    }
}
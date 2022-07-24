using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EchoMessenger.UI
{
    public class TextBlockWithIcon : Border
    {
        public static readonly ImageBrush DeleteImage = Application.Current.FindResource("DeleteImage") as ImageBrush;
        public static readonly ImageBrush ReplyImage = Application.Current.FindResource("ReplyImage") as ImageBrush;
        public static readonly ImageBrush EditImage = Application.Current.FindResource("EditImage") as ImageBrush;
        public static readonly ImageBrush CopyImage = Application.Current.FindResource("CopyImage") as ImageBrush;
        public static readonly ImageBrush HistoryImage = Application.Current.FindResource("HistoryImage") as ImageBrush;

        public TextBlockWithIcon(ImageBrush imageBrush, String text) : base()
        {
            Padding = new Thickness(5);

            var grid = new Grid();

            var avatarColumn = new ColumnDefinition();
            avatarColumn.Width = new GridLength(50);

            var usernameColumn = new ColumnDefinition();

            grid.ColumnDefinitions.Add(avatarColumn);
            grid.ColumnDefinitions.Add(usernameColumn);

            var iconBorder = new Border();
            iconBorder.Height = 17.5;
            iconBorder.Width = 17.5;
            iconBorder.Background = imageBrush;
            iconBorder.Margin = new Thickness(5);

            var textBlock = new TextBlock();
            textBlock.Width = 75;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            textBlock.FontSize = 14;
            textBlock.FontFamily = new FontFamily("Segoe UI");
            textBlock.Text = text;

            Grid.SetColumn(iconBorder, 0);
            grid.Children.Add(iconBorder);

            Grid.SetColumn(textBlock, 1);
            grid.Children.Add(textBlock);

            Child = grid;
        }
    }
}

using EchoMessenger.Core;
using EchoMessenger.Helpers.Api;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EchoMessenger.UI.Controls.Cards.Search
{
    public class SearchResultUserCard : Border
    {
        public SearchResultUserCard(String avatarUrl, String username) : base()
        {
            BorderBrush = Application.Current.FindResource("SeparationBrush") as SolidColorBrush;
            BorderThickness = new Thickness(0, 1, 0, 0);
            // Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF1C1D26");
            Padding = new Thickness(10, 0, 10, 0);

            var grid = new Grid();

            var avatarColumn = new ColumnDefinition();
            avatarColumn.Width = new GridLength(60);

            var usernameColumn = new ColumnDefinition();

            grid.ColumnDefinitions.Add(avatarColumn);
            grid.ColumnDefinitions.Add(usernameColumn);

            var avatarBorder = new Border();
            avatarBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
            avatarBorder.BorderThickness = new Thickness(1);
            avatarBorder.CornerRadius = new CornerRadius(100);
            avatarBorder.Width = avatarBorder.Height = 35;
            avatarBorder.Margin = new Thickness(5);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(avatarUrl, UriKind.Absolute);
            bitmap.EndInit();
            avatarBorder.Background = new ImageBrush() { ImageSource = bitmap, Stretch = Stretch.UniformToFill };

            var usernameTextBlock = new TextBlock();
            usernameTextBlock.MinWidth = 400;
            usernameTextBlock.VerticalAlignment = VerticalAlignment.Center;
            usernameTextBlock.Foreground = new SolidColorBrush(Colors.White);
            usernameTextBlock.Margin = new Thickness(0);
            usernameTextBlock.FontSize = 14;
            usernameTextBlock.FontFamily = new FontFamily("Segoe UI");
            usernameTextBlock.Text = username;

            Grid.SetColumn(avatarBorder, 0);
            grid.Children.Add(avatarBorder);

            Grid.SetColumn(usernameTextBlock, 1);
            grid.Children.Add(usernameTextBlock);

            Child = grid;
        }

        public void SetFirst()
        {
            BorderThickness = new Thickness(BorderThickness.Left, 0, BorderThickness.Right, BorderThickness.Bottom);
        }

        public void SetLast()
        {
            BorderThickness = new Thickness(BorderThickness.Left, BorderThickness.Top, BorderThickness.Right, 1);
        }
    }
}

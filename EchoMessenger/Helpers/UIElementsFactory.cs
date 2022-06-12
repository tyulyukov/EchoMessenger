using EchoMessenger.Helpers.Server;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LoadingSpinnerControl;

namespace EchoMessenger.Helpers
{
    public static class UIElementsFactory
    {

        public static Border CreateUsersCard(String avatarUrl, String username)
        {
            var border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.White);
            border.BorderThickness = new Thickness(0, 1, 0, 0);
            border.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF1C1D26");
            border.Padding = new Thickness(10, 0, 10, 0);

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
            bitmap.UriSource = new Uri(Database.HostUrl(avatarUrl), UriKind.Absolute);
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

            border.Child = grid;

            return border;
        }

        public static Border CreateUserIcon(String avatarUrl, bool isOnline)
        {
            var border = new Border();
            border.Width = 40;
            border.Height = 40;
            border.Margin = new Thickness(0, 0, 0, 10);
            border.CornerRadius = new CornerRadius(100);
            border.BorderBrush = new SolidColorBrush(Colors.Gray);
            border.BorderThickness = new Thickness(1);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(Database.HostUrl(avatarUrl), UriKind.Absolute);
            bitmap.EndInit();
            border.Background = new ImageBrush() { ImageSource = bitmap, Stretch = Stretch.UniformToFill };

            var grid = new Grid();

            var onlineStatusIcon = new OnlineStatusIcon(isOnline);
            grid.Children.Add(onlineStatusIcon);

            var notificationsBadge = new NotificationBadge("0");
            notificationsBadge.Visibility = Visibility.Collapsed;
            grid.Children.Add(notificationsBadge);

            border.Child = grid;

            return border;
        }

        public static Line CreateSelectionLine()
        {
            Line line = new Line();
            line.X1 = 0;
            line.Y1 = 0;
            line.X2 = 0;
            line.Y2 = 30;
            line.Margin = new Thickness(-11, 5, 0, 5);
            line.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#ff6088"); 
            line.StrokeThickness = 4;

            return line;
        }

        public static Border CreateDateCard(DateTime date)
        {
            Border border = new Border();
            border.HorizontalAlignment = HorizontalAlignment.Center;
            border.CornerRadius = new CornerRadius(10);
            border.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF242430");
            border.Margin = new Thickness(0, 10, 0, 0);
            border.Effect = new DropShadowEffect();

            Label label = new Label();
            label.Foreground = new SolidColorBrush(Colors.White);
            label.Margin = new Thickness(2);

            if (date.ToLongDateString() == DateTime.Today.ToLongDateString())
                label.Content = "Today";
            else
                label.Content = date.ToLongDateString();

            border.Child = label;

            return border;
        }
    }

    public class MessageBorder : Border
    {
        public TextBlock MessageTextBlock;
        public TextBlock TimeTextBlock;
        public LoadingSpinner LoadingSpinner;

        public MessageBorder(String text, bool isOwn)
        {
            MinHeight = 40;
            BorderBrush = new SolidColorBrush(Colors.White);
            BorderThickness = new Thickness(1);
            Margin = new Thickness(0, 3, 0, 0);
            CornerRadius = new CornerRadius(20);

            if (isOwn)
            {
                HorizontalAlignment = HorizontalAlignment.Right;
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#283EFF");
            }
            else
            {
                HorizontalAlignment = HorizontalAlignment.Left;
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF1C1D26");
            }

            var grid = new Grid();

            var messageColumn = new ColumnDefinition();
            messageColumn.MinWidth = 30;
            messageColumn.MaxWidth = 500;

            var timeColumn = new ColumnDefinition();

            grid.ColumnDefinitions.Add(messageColumn);
            grid.ColumnDefinitions.Add(timeColumn);

            MessageTextBlock = new TextBlock();
            MessageTextBlock.Text = text;
            MessageTextBlock.TextWrapping = TextWrapping.Wrap;
            MessageTextBlock.VerticalAlignment = VerticalAlignment.Center;
            MessageTextBlock.Foreground = new SolidColorBrush(Colors.White);
            MessageTextBlock.FontSize = 14;
            MessageTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            MessageTextBlock.Margin = new Thickness(7.5);

            Grid.SetColumn(MessageTextBlock, 0);
            grid.Children.Add(MessageTextBlock);

            TimeTextBlock = new TextBlock();
            TimeTextBlock.Visibility = Visibility.Collapsed;
            TimeTextBlock.Foreground = new SolidColorBrush(Colors.LightGray);
            TimeTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
            TimeTextBlock.Margin = new Thickness(0, 0, 10, 5);

            Grid.SetColumn(TimeTextBlock, 1);
            grid.Children.Add(TimeTextBlock);

            LoadingSpinner = new LoadingSpinner();
            LoadingSpinner.Diameter = 10;
            LoadingSpinner.StrokeGap = 0.75;
            LoadingSpinner.Cap = PenLineCap.Round;
            LoadingSpinner.IsLoading = true;
            LoadingSpinner.VerticalAlignment = VerticalAlignment.Bottom;
            LoadingSpinner.Margin = new Thickness(0, 0, 10, 5);
            LoadingSpinner.Color = new SolidColorBrush(Colors.White);
            LoadingSpinner.StartLoading();

            Grid.SetColumn(LoadingSpinner, 1);
            grid.Children.Add(LoadingSpinner);

            Child = grid;
        }

        public void SetLoaded(DateTime sentAt)
        {
            LoadingSpinner.IsLoading = false;

            TimeTextBlock.Text = sentAt.ToString("HH:mm");
            TimeTextBlock.Visibility = Visibility.Visible;
        }

        public void SetFailedLoading()
        {
            LoadingSpinner.IsLoading = false;

            Background = new SolidColorBrush(Colors.Red);
        }
    }

    public class OnlineStatusIcon : Border
    {
        public OnlineStatusIcon(bool isOnline) : base()
        {
            Width = Height = 10;
            Background = isOnline ? (SolidColorBrush)new BrushConverter().ConvertFrom("#ff6088") : new SolidColorBrush(Colors.Gray);
            HorizontalAlignment = HorizontalAlignment.Right;
            VerticalAlignment = VerticalAlignment.Bottom;
            CornerRadius = new CornerRadius(100);
            BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF131522");
            BorderThickness = new Thickness(2);
        }
    }

    public class NotificationBadge : Border
    {
        public NotificationBadge(String count) : base()
        {
            MinWidth = 21;
            Background = new SolidColorBrush(Colors.Red);
            HorizontalAlignment = HorizontalAlignment.Right;
            VerticalAlignment = VerticalAlignment.Top;
            CornerRadius = new CornerRadius(10);
            BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF131522");
            BorderThickness = new Thickness(3);
            Margin = new Thickness(0, -5, -10, 0);

            var textBlock = new TextBlock();
            textBlock.Text = count;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            textBlock.FontSize = 10;
            textBlock.FontFamily = new FontFamily("Segoe UI Semibold");
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.Margin = new Thickness(3, 1, 3, 1);

            Child = textBlock;
        }
    }
}
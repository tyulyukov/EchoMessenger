﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EchoMessenger.Helpers
{
    public static class UIElementsFactory
    {
        public static Border CreateOwnMessage(String text, DateTime dateTime)
        {
            var border = CreateDefaultMessageBorder(text, dateTime);
            border.HorizontalAlignment = HorizontalAlignment.Right;
            border.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#283EFF");
            
            return border;
        }

        public static Border CreateForeignMessage(String text, DateTime dateTime)
        {
            var border = CreateDefaultMessageBorder(text, dateTime);
            border.HorizontalAlignment = HorizontalAlignment.Left;
            border.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF1C1D26");

            return border;
        }

        private static Border CreateDefaultMessageBorder(String text, DateTime time)
        {
            var border = new Border();
            border.MinHeight = 50;
            border.BorderBrush = new SolidColorBrush(Colors.White);
            border.BorderThickness = new Thickness(1);
            border.Margin = new Thickness(0, 10, 0, 0);
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

        public static Border CreateUsersCard(String avatarUrl, String username)
        {
            var border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.White);
            border.BorderThickness = new Thickness(1);
            border.Margin = new Thickness(10, 10, 10, 0);
            border.HorizontalAlignment = HorizontalAlignment.Left;
            border.CornerRadius = new CornerRadius(30);
            border.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF1C1D26");

            var grid = new Grid();

            var avatarColumn = new ColumnDefinition();
            avatarColumn.Width = new GridLength(75);

            var usernameColumn = new ColumnDefinition();

            grid.ColumnDefinitions.Add(avatarColumn);
            grid.ColumnDefinitions.Add(usernameColumn);

            var avatarBorder = new Border();
            avatarBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
            avatarBorder.BorderThickness = new Thickness(1);
            avatarBorder.CornerRadius = new CornerRadius(100);
            avatarBorder.Width = avatarBorder.Height = 50;
            avatarBorder.Margin = new Thickness(10);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(avatarUrl, UriKind.Absolute);
            bitmap.EndInit();
            avatarBorder.Background = new ImageBrush() { ImageSource = bitmap, Stretch = Stretch.UniformToFill };

            var usernameTextBlock = new TextBlock();
            usernameTextBlock.MinWidth = 400;
            usernameTextBlock.VerticalAlignment = VerticalAlignment.Center;
            usernameTextBlock.Foreground = new SolidColorBrush(Colors.White);
            usernameTextBlock.Margin = new Thickness(10);
            usernameTextBlock.FontSize = 14d;
            usernameTextBlock.FontFamily = new FontFamily("Segoe UI Semibold");
            usernameTextBlock.Text = username;

            Grid.SetColumn(avatarBorder, 0);
            grid.Children.Add(avatarBorder);

            Grid.SetColumn(usernameTextBlock, 1);
            grid.Children.Add(usernameTextBlock);

            border.Child = grid;

            return border;
        }

        public static Border CreateUserIcon(String avatarUrl)
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
            bitmap.UriSource = new Uri(avatarUrl, UriKind.Absolute);
            bitmap.EndInit();
            border.Background = new ImageBrush() { ImageSource = bitmap, Stretch = Stretch.UniformToFill };

            return border;
        }

        public static Line CreateSelectionLine()
        {
            Line line = new Line();
            line.X1 = 0;
            line.Y1 = 0;
            line.X2 = 0;
            line.Y2 = 30;
            line.Margin = new Thickness(-9, 5, 0, 5);
            line.Stroke = new SolidColorBrush(Colors.White);
            line.StrokeThickness = 3;

            return line;
        }

        public static Border CreateDateCard(DateTime date)
        {
            Border border = new Border();
            border.Width = 200;
            border.Height = 25;
            border.CornerRadius = new CornerRadius(15);
            border.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF242430");
            border.Margin = new Thickness(0, 10, 0, 0);
            border.Effect = new DropShadowEffect();

            Label label = new Label();
            label.Foreground = new SolidColorBrush(Colors.White);
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.Content = date.ToLongDateString();

            border.Child = label;

            return border;
        }
    }
}
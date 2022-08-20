using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace EchoMessenger.UI.Controls.Cards.Chats
{
    public class UserIcon : Border
    {
        public OnlineStatusIcon OnlineStatusIcon { get; private set; }

        private Duration selectionDuration;

        private Color baseBackground;
        private Color baseSelectionBackground;

        public UserIcon(String avatarUrl, bool isOnline, Duration selectionDuration, Color baseBackground, Color baseSelectionBackground) : base()
        {
            this.baseBackground = baseBackground;
            this.baseSelectionBackground = baseSelectionBackground;
            this.selectionDuration = selectionDuration;

            Width = Height = 45;
            CornerRadius = new CornerRadius(100);
            BorderBrush = new SolidColorBrush(Colors.Gray);
            BorderThickness = new Thickness(1);

            UpdateAvatar(avatarUrl);

            var grid = new Grid();

            this.OnlineStatusIcon = new OnlineStatusIcon(selectionDuration, isOnline);
            OnlineStatusIcon.HorizontalAlignment = HorizontalAlignment.Right;
            OnlineStatusIcon.VerticalAlignment = VerticalAlignment.Bottom;
            grid.Children.Add(OnlineStatusIcon);

            Child = grid;
        }

        public void UpdateAvatar(String avatarUrl)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(avatarUrl, UriKind.Absolute);
            bitmap.EndInit();
            Background = new ImageBrush() { ImageSource = bitmap, Stretch = Stretch.UniformToFill };
        }

        public void Select()
        {
            OnlineStatusIcon.Select();

            var animation = new ColorAnimation();
            animation.To = baseSelectionBackground;
            animation.Duration = selectionDuration;
            OnlineStatusIcon.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);

            var borderAnimation = new ColorAnimation();
            borderAnimation.To = Colors.White;
            borderAnimation.Duration = selectionDuration;
            BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, borderAnimation);
        }

        public void Deselect()
        {
            OnlineStatusIcon.Deselect();

            BorderBrush = new SolidColorBrush(Colors.Gray);
            OnlineStatusIcon.BorderBrush = new SolidColorBrush(baseBackground);
        }
    }
}

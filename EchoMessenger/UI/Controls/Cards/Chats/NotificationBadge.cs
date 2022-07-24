using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace EchoMessenger.UI.Controls.Cards.Chats
{
    public class NotificationBadge : Border
    {
        public int NotificationsCount { get; private set; }

        private TextBlock NotificationTextBlock;
        private Duration selectionDuration;

        private SolidColorBrush background;
        private SolidColorBrush foreground;

        private Color baseSelectionBackground;

        public NotificationBadge(Color baseSelectionBackground, Duration selectionDuration) : base()
        {
            background = new SolidColorBrush(Colors.Red);
            foreground = new SolidColorBrush(Colors.White);

            this.selectionDuration = selectionDuration;
            this.baseSelectionBackground = baseSelectionBackground;

            Background = background.Clone();
            CornerRadius = new CornerRadius(8.5);
            // BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF131522");
            // BorderThickness = new Thickness(3);

            NotificationTextBlock = new TextBlock();
            NotificationTextBlock.VerticalAlignment = VerticalAlignment.Center;
            NotificationTextBlock.Foreground = foreground.Clone();
            NotificationTextBlock.FontSize = 10;
            NotificationTextBlock.FontFamily = new FontFamily("Segoe UI Semibold");
            NotificationTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            NotificationTextBlock.Margin = new Thickness(-4, 2, -4, 2);

            Child = NotificationTextBlock;
        }

        public void Select()
        {
            var animation = new ColorAnimation();
            animation.To = Colors.White;
            animation.Duration = selectionDuration;
            Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);

            var textAnimation = new ColorAnimation();
            textAnimation.To = baseSelectionBackground;
            textAnimation.Duration = selectionDuration;
            NotificationTextBlock.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, textAnimation);
        }

        public void Deselect()
        {
            Background = background.Clone();
            NotificationTextBlock.Foreground = foreground.Clone();
        }

        public void SetNotifications(int count)
        {
            if (count < 0)
                return;

            NotificationsCount = count;
            NotificationTextBlock.Text = GetRepresentationString(NotificationsCount);
            Visibility = NotificationsCount != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public void AddNotification()
        {
            NotificationsCount++;

            NotificationTextBlock.Text = GetRepresentationString(NotificationsCount);
            Visibility = Visibility.Visible;
        }

        public void RemoveNotification()
        {
            if (NotificationsCount == 0)
                return;

            NotificationsCount--;
            NotificationTextBlock.Text = GetRepresentationString(NotificationsCount);
            Visibility = NotificationsCount != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private String GetRepresentationString(int count) => count >= 1000 ? "999+" : count.ToString();
    }
}

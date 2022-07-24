using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace EchoMessenger.UI.Controls.Cards.Chats
{
    public class OnlineStatusIcon : Border
    {
        public SolidColorBrush OfflineBrush { get; private set; }
        public SolidColorBrush OnlineBrush { get; private set; }

        private SolidColorBrush offlineBrush;
        private SolidColorBrush selectedOfflineBrush;
        private SolidColorBrush onlineBrush;
        private SolidColorBrush selectedOnlineBrush;

        private bool _isOnline = false;

        private Duration selectionDuration;

        private SolidColorBrush activeBrush = Application.Current.FindResource("ActiveBrush") as SolidColorBrush;
        private SolidColorBrush mainBrush = Application.Current.FindResource("MainBrush") as SolidColorBrush;

        public OnlineStatusIcon(Duration selectionDuration, bool isOnline = false) : base()
        {
            this.selectionDuration = selectionDuration;

            OfflineBrush = offlineBrush = new SolidColorBrush(Colors.Gray);
            OnlineBrush = onlineBrush = activeBrush.Clone();
            selectedOfflineBrush = new SolidColorBrush(Colors.White);
            selectedOnlineBrush = new SolidColorBrush(Colors.DeepPink);

            Background = OfflineBrush.Clone();
            Width = Height = 10;
            CornerRadius = new CornerRadius(100);
            BorderBrush = mainBrush.Clone();
            BorderThickness = new Thickness(2);

            SetStatus(isOnline);
        }

        public void SetOnline()
        {
            _isOnline = true;

            var animation = new ColorAnimation();
            animation.To = OnlineBrush.Color;
            animation.Duration = selectionDuration;
            Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        public void SetOffline()
        {
            _isOnline = false;

            var animation = new ColorAnimation();
            animation.To = OfflineBrush.Color;
            animation.Duration = selectionDuration;
            Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        public void Select()
        {
            OfflineBrush = selectedOfflineBrush;
            OnlineBrush = selectedOnlineBrush;

            SetStatus(_isOnline);
        }

        public void Deselect()
        {
            OfflineBrush = offlineBrush;
            OnlineBrush = onlineBrush;

            SetStatus(_isOnline);
        }

        private void SetStatus(bool isOnline)
        {
            if (isOnline)
                SetOnline();
            else
                SetOffline();
        }
    }
}

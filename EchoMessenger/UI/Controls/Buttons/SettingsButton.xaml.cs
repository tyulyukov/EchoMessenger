using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace EchoMessenger.UI.Controls.Buttons
{
    /// <summary>
    /// Interaction logic for SettingsButton.xaml
    /// </summary>
    public partial class SettingsButton : UserControl
    {
        private Duration selectionDuration = new Duration(TimeSpan.FromMilliseconds(200));
        private bool selected = false;
        private Color selectionColor;

        private SolidColorBrush mainBrush = Application.Current.Resources["MainBrush"] as SolidColorBrush;
        private SolidColorBrush activeBrush = Application.Current.Resources["ActiveBrush"] as SolidColorBrush;
        private SolidColorBrush secondaryActiveBrush = Application.Current.Resources["SecondaryActiveBrush"] as SolidColorBrush;

        public SettingsButton(String settingName, ImageBrush settingIcon)
        {
            if (mainBrush is null || activeBrush is null || secondaryActiveBrush is null)
                throw new ArgumentNullException();

            InitializeComponent();

            selectionColor = secondaryActiveBrush.Clone().Color;
            MainButton.Background = mainBrush.Clone();
            
            MainButton.MouseEnter += MainButton_MouseEnter;
            MainButton.MouseLeave += MainButton_MouseLeave;

            SettingIconBorder.Background = settingIcon;
            SettingTextBlock.Text = settingName;

            Deselect();
        }

        public void Select()
        {
            if (selected)
                return;

            MainButton.MouseEnter -= MainButton_MouseEnter;
            MainButton.MouseLeave -= MainButton_MouseLeave;

            var animation = new ColorAnimation();
            animation.To = selectionColor;
            animation.Duration = selectionDuration;
            MainButton.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);

            selected = true;
        }

        public void Deselect()
        {
            if (!selected)
                return;

            MainButton.Background = mainBrush.Clone();

            MainButton.MouseEnter += MainButton_MouseEnter;
            MainButton.MouseLeave += MainButton_MouseLeave;

            selected = false;
        }

        private void MainButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var animation = new ColorAnimation();
            animation.To = mainBrush.Clone().Color;
            animation.Duration = selectionDuration;
            MainButton.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        private void MainButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var animation = new ColorAnimation();
            animation.To = Color.FromRgb(33, 34, 49);
            animation.Duration = selectionDuration;
            MainButton.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }
    }
}

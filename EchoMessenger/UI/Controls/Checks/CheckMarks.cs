using EchoMessenger.UI.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace EchoMessenger.UI.Controls.Checks
{
    public class CheckMarks : StackPanel
    {
        public CheckMark FirstCheck { get; private set; }
        public CheckMark SecondCheck { get; private set; }

        public CheckMarks()
        {
            Orientation = Orientation.Horizontal;
            Width = 15;
            Height = 10;

            FirstCheck = new CheckMark(true);
            SecondCheck = new CheckMark(false);
            SecondCheck.Visibility = Visibility.Collapsed;

            Children.Add(FirstCheck);
            Children.Add(SecondCheck);
        }

        public void SetHaveSeen()
        {
            SecondCheck.Visibility = Visibility.Visible;
            SecondCheck.ChangeVisibility(true);
        }

        public void SetHaveNotSeen()
        {
            SecondCheck.Visibility = Visibility.Collapsed;
            SecondCheck.ChangeVisibility(false);
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EchoMessenger.UI
{
    public class SelectionLine : Border
    {
        public SelectionLine() : base()
        {
            Height = 30;
            Width = 2;
            Margin = new Thickness(-11, 5, 0, 5);
            HorizontalAlignment = HorizontalAlignment.Left;
            Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ff6088");
        }
    }
}

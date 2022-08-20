using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EchoMessenger
{
    /// <summary>
    /// Interaction logic for PreloaderWindow.xaml
    /// </summary>
    public partial class PreloaderWindow : Window
    {
        private TimeSpan waitingDuration = TimeSpan.FromSeconds(7.5);
        private bool isRetrying = false;

        public PreloaderWindow()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        public TimeSpan NoConnection()
        {
            DoubleWaitingDuration();

            AlertTextBlock.Text = $"No connection. Retrying in {waitingDuration.TotalSeconds} sec";

            return waitingDuration;
        }

        public TimeSpan ServerError()
        {
            DoubleWaitingDuration();

            AlertTextBlock.Text = $"Server error. Retrying in {waitingDuration.TotalSeconds} sec";

            return waitingDuration;
        }

        public void Retrying()
        {
            isRetrying = true;

            Dispatcher.Invoke(async () =>
            {
                while (isRetrying)
                {
                    AlertTextBlock.Text = "Retrying";

                    for (int i = 0; i <= 3; i++)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));

                        if (isRetrying)
                            AlertTextBlock.Text += ".";
                        else break;
                    }
                }
            });
        }

        private void DoubleWaitingDuration()
        {
            isRetrying = false;

            if (waitingDuration.TotalSeconds < 60)
                waitingDuration = waitingDuration.Multiply(2);
        }
    }
}

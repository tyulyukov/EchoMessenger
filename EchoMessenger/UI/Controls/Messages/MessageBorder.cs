using EchoMessenger.UI.Controls.Checks;
using EchoMessenger.UI.Extensions;
using LoadingSpinnerControl;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace EchoMessenger.UI.Controls.Messages
{
    public class MessageBorder : Border
    {
        public static Action<Models.Message>? OnEditButtonClick;
        public static Action<Models.Message>? OnDeleteButtonClick;
        public static Action<Models.Message>? OnReplyButtonClick;
        public static Action<Models.Message>? OnHistoryButtonClick;

        public static Popup? OpenedPopup { get; private set; }

        public bool IsOwn { get; private set; }
        public Models.Message? Message { get; private set; }

        public bool IsTopRounded { get; private set; } = false;
        public bool IsBottomRounded { get; private set; } = false;

        private StackPanel ReplyPanel;
        private TextBox MessageTextBox;
        private TextBlock TimeTextBlock;
        private LoadingSpinner LoadingSpinner;
        private CheckMarks? CheckMarks;

        private Label? ReplyTextLabel;
        private Label? ReplyUsernameLabel;

        private Grid messageGrid;

        private const double cornerRoundingIn = 5;
        private const double cornerRoundingOut = 17;

        private static SolidColorBrush mainBrush = Application.Current.Resources["MainBrush"] as SolidColorBrush;
        private static SolidColorBrush activeBrush = Application.Current.Resources["ActiveBrush"] as SolidColorBrush;
        private static SolidColorBrush secondaryActiveBrush = Application.Current.Resources["SecondaryActiveBrush"] as SolidColorBrush;

        public MessageBorder(String text, bool isOwn)
        {
            this.IsOwn = isOwn;

            MinHeight = 30;
            BorderBrush = new SolidColorBrush(Colors.White);
            BorderThickness = new Thickness(1);
            Margin = new Thickness(0, 3, 0, 0);
            CornerRadius = new CornerRadius(cornerRoundingOut);
            Padding = new Thickness(7);

            if (isOwn)
            {
                HorizontalAlignment = HorizontalAlignment.Right;
                Background = secondaryActiveBrush.Clone();
            }
            else
            {
                HorizontalAlignment = HorizontalAlignment.Left;
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF1C1D26");
            }

            messageGrid = new Grid();

            var replyRow = new RowDefinition();
            replyRow.Height = GridLength.Auto;
            replyRow.MaxHeight = 100;

            var messageRow = new RowDefinition();
            messageRow.Height = GridLength.Auto;

            messageGrid.RowDefinitions.Add(replyRow);
            messageGrid.RowDefinitions.Add(messageRow);

            var grid = new Grid();

            var messageColumn = new ColumnDefinition();
            messageColumn.MaxWidth = 500;

            var timeColumn = new ColumnDefinition();
            timeColumn.Width = GridLength.Auto;

            var checkMarksColumn = new ColumnDefinition();
            checkMarksColumn.Width = GridLength.Auto;

            grid.ColumnDefinitions.Add(messageColumn);
            grid.ColumnDefinitions.Add(timeColumn);
            grid.ColumnDefinitions.Add(checkMarksColumn);

            ReplyPanel = new StackPanel();
            ReplyPanel.Orientation = Orientation.Horizontal;
            ReplyPanel.HorizontalAlignment = HorizontalAlignment.Left;
            ReplyPanel.Cursor = Cursors.Hand;
            ReplyPanel.MaxWidth = 450;
            ReplyPanel.Margin = new Thickness(-5, -5, 0, -2);

            Grid.SetRow(ReplyPanel, 0);
            messageGrid.Children.Add(ReplyPanel);

            MessageTextBox = new TextBox();
            MessageTextBox.Text = text;
            MessageTextBox.TextWrapping = TextWrapping.Wrap;
            MessageTextBox.Foreground = new SolidColorBrush(Colors.White);
            MessageTextBox.FontSize = 14;
            MessageTextBox.VerticalAlignment = VerticalAlignment.Center;
            MessageTextBox.HorizontalAlignment = HorizontalAlignment.Left;
            MessageTextBox.Margin = new Thickness(0, 0, 5, 0);
            MessageTextBox.Background = new SolidColorBrush(Colors.Transparent);
            MessageTextBox.BorderThickness = new Thickness(0);
            MessageTextBox.IsReadOnly = true;

            Grid.SetColumn(MessageTextBox, 0);
            grid.Children.Add(MessageTextBox);

            TimeTextBlock = new TextBlock();
            TimeTextBlock.Visibility = Visibility.Collapsed;
            TimeTextBlock.Foreground = new SolidColorBrush(Colors.LightGray);
            TimeTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
            //TimeTextBlock.Margin = new Thickness(0, 0, 4, 0);

            Grid.SetColumn(TimeTextBlock, 1);
            grid.Children.Add(TimeTextBlock);

            LoadingSpinner = new LoadingSpinner();
            LoadingSpinner.Diameter = 10;
            LoadingSpinner.StrokeGap = 0.75;
            LoadingSpinner.Cap = PenLineCap.Round;
            LoadingSpinner.IsLoading = true;
            LoadingSpinner.VerticalAlignment = VerticalAlignment.Bottom;
            LoadingSpinner.Margin = new Thickness(0, 0, 4, 2);
            LoadingSpinner.Color = new SolidColorBrush(Colors.White);
            LoadingSpinner.StartLoading();

            Grid.SetColumn(LoadingSpinner, 1);
            grid.Children.Add(LoadingSpinner);

            if (isOwn)
            {
                TimeTextBlock.Margin = new Thickness(0, 0, 4, 0);
                CheckMarks = new CheckMarks();
                CheckMarks.Visibility = Visibility.Collapsed;
                CheckMarks.VerticalAlignment = VerticalAlignment.Bottom;
                CheckMarks.Margin = new Thickness(0, 0, 0, 3);

                Grid.SetColumn(CheckMarks, 2);
                grid.Children.Add(CheckMarks);
            }

            Grid.SetRow(grid, 1);
            messageGrid.Children.Add(grid);

            Child = messageGrid;
        }

        public void SetReplyingMessage(String text, String username, Action MouseClick)
        {
            var border = new Border();
            border.Width = 2;
            border.Margin = new Thickness(9, 10, 3, 9);
            border.Background = activeBrush.Clone();

            var stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;

            ReplyUsernameLabel = new Label();
            ReplyUsernameLabel.HorizontalAlignment = HorizontalAlignment.Left;
            ReplyUsernameLabel.HorizontalContentAlignment = HorizontalAlignment.Left;
            ReplyUsernameLabel.Foreground = activeBrush.Clone();
            ReplyUsernameLabel.FontSize = 14;
            ReplyUsernameLabel.Content = username;

            ReplyTextLabel = new Label();
            ReplyTextLabel.HorizontalContentAlignment = HorizontalAlignment.Left;
            ReplyTextLabel.Foreground = new SolidColorBrush(Colors.White);
            ReplyTextLabel.FontSize = 14;
            ReplyTextLabel.Margin = new Thickness(0, -10, 0, 0);
            ReplyTextLabel.Content = text;
            ReplyTextLabel.MaxHeight = 100;

            var gradientStops = new GradientStopCollection();

            var gradientStart = new GradientStop(Colors.Black, 0.5);
            var gradientEnd = new GradientStop(Colors.Transparent, 1);

            gradientStops.Add(gradientStart);
            gradientStops.Add(gradientEnd);

            stackPanel.Children.Add(ReplyUsernameLabel);
            stackPanel.Children.Add(ReplyTextLabel);

            ReplyTextLabel.OpacityMask = new LinearGradientBrush(gradientStops, new Point(0.5, 0), new Point(0.5, 1));

            ReplyPanel.MouseLeftButtonUp += (s, e) => MouseClick.Invoke();
            ReplyPanel.Children.Add(border);
            ReplyPanel.Children.Add(stackPanel);
        }

        public void DeleteReplyingMessage(System.Threading.SynchronizationContext uiSync)
        {
            TimeSpan deletingTime = TimeSpan.FromMilliseconds(250);

            uiSync.Send((s) =>
            {
                ReplyPanel.ChangeVisibility(false, deletingTime);

                System.Threading.Tasks.Task.Delay(deletingTime).ContinueWith(t =>
                {
                    uiSync.Send((s) =>
                    {
                        ReplyPanel.Visibility = Visibility.Collapsed;
                    }, null);
                });
            }, null);
        }

        public void SetLoaded(Models.Message message)
        {
            LoadingSpinner.IsLoading = false;

            Message = message;
            TimeTextBlock.Text = message.sentAtLocal.ToString("HH:mm");
            TimeTextBlock.Visibility = Visibility.Visible;

            if (CheckMarks != null)
            {
                CheckMarks.Visibility = Visibility.Visible;

                if (message.haveSeen)
                    CheckMarks.SetHaveSeen();
            }

            var messagePopup = new Popup();
            messagePopup.AllowsTransparency = true;
            messagePopup.IsOpen = false;
            messagePopup.Placement = PlacementMode.Mouse;
            messagePopup.PlacementTarget = this;
            messagePopup.StaysOpen = false;
            messagePopup.PopupAnimation = PopupAnimation.Scroll;

            var popupBorder = new Border();
            popupBorder.CornerRadius = new CornerRadius(10);
            popupBorder.Padding = new Thickness(0, 5, 0, 5);

            var buttonsStack = new StackPanel();
            buttonsStack.Background = mainBrush.Clone();
            buttonsStack.Orientation = Orientation.Vertical;

            var replyButton = new TextBlockWithIcon(TextBlockWithIcon.ReplyImage, "Reply");
            if (OnReplyButtonClick != null)
            {
                replyButton.MouseLeftButtonUp += (s, e) => OnReplyButtonClick.Invoke(Message);
                MouseDown += (s, e) =>
                {
                    if (e.ClickCount == 2)
                        OnReplyButtonClick.Invoke(Message); 
                };
            }

            buttonsStack.Children.Add(replyButton.ConvertToSelectable());

            if (IsOwn)
            {
                var editButton = new TextBlockWithIcon(TextBlockWithIcon.EditImage, "Edit");
                if (OnEditButtonClick != null)
                    editButton.MouseLeftButtonUp += (s, e) => OnEditButtonClick.Invoke(Message);

                buttonsStack.Children.Add(editButton.ConvertToSelectable());

                var deleteButton = new TextBlockWithIcon(TextBlockWithIcon.DeleteImage, "Delete");
                if (OnDeleteButtonClick != null)
                    deleteButton.MouseLeftButtonUp += (s, e) => OnDeleteButtonClick.Invoke(Message);

                buttonsStack.Children.Add(deleteButton.ConvertToSelectable());
            }

            var historyButton = new TextBlockWithIcon(TextBlockWithIcon.HistoryImage, "History");
            if (OnHistoryButtonClick != null)
                historyButton.MouseLeftButtonUp += (s, e) => OnHistoryButtonClick.Invoke(Message);

            buttonsStack.Children.Add(historyButton.ConvertToSelectable());

            var copyButton = new TextBlockWithIcon(TextBlockWithIcon.CopyImage, "Copy Text");
            copyButton.MouseLeftButtonUp += (s, e) =>
            {
                Clipboard.SetText(MessageTextBox.Text);
                messagePopup.IsOpen = false;
            };

            buttonsStack.Children.Add(copyButton.ConvertToSelectable());

            popupBorder.Child = buttonsStack;
            messagePopup.Child = popupBorder;
            messageGrid.Children.Add(messagePopup);

            MouseRightButtonUp += (s, e) => { messagePopup.IsOpen = !messagePopup.IsOpen; };

            messagePopup.Opened += (s, e) => OpenedPopup = messagePopup;
        }

        public void SetFailedLoading()
        {
            LoadingSpinner.IsLoading = false;

            Background = new SolidColorBrush(Colors.Red);
        }

        public void SetEdited(Models.Message? message = null)
        {
            if (!TimeTextBlock.Text.StartsWith("edited"))
                TimeTextBlock.Text = "edited " + TimeTextBlock.Text;

            if (message != null)
            {
                Message = message;
                MessageTextBox.Text = message.content;
            }
        }

        public void SetReplyEdited(String content)
        {
            if (Message != null)
                Message.repliedOn.content = content;

            if (ReplyTextLabel != null)
                ReplyTextLabel.Content = content;
        }

        public void ReplyUserUpdate(Models.UserInfo user)
        {
            if (Message != null)
                Message.repliedOn.sender = user;

            if (ReplyUsernameLabel != null)
                ReplyUsernameLabel.Content = user.username;
        }

        public void SetHaveSeen()
        {
            CheckMarks?.SetHaveSeen();
        }

        public void RoundInTop()
        {
            if (IsOwn)
                CornerRadius = new CornerRadius(CornerRadius.TopLeft,
                                                cornerRoundingIn,
                                                CornerRadius.BottomRight,
                                                CornerRadius.BottomLeft);
            else
                CornerRadius = new CornerRadius(cornerRoundingIn,
                                                CornerRadius.TopRight,
                                                CornerRadius.BottomRight,
                                                CornerRadius.BottomLeft);

            IsTopRounded = true;
        }

        public void RoundInBottom()
        {
            if (IsOwn)
                CornerRadius = new CornerRadius(CornerRadius.TopLeft,
                                                CornerRadius.TopRight,
                                                cornerRoundingIn,
                                                CornerRadius.BottomLeft);
            else
                CornerRadius = new CornerRadius(CornerRadius.TopLeft,
                                                CornerRadius.TopRight,
                                                CornerRadius.BottomRight,
                                                cornerRoundingIn);

            IsBottomRounded = true;
        }

        public void RoundOutTop()
        {
            if (IsOwn)
                CornerRadius = new CornerRadius(CornerRadius.TopLeft,
                                                cornerRoundingOut,
                                                CornerRadius.BottomRight,
                                                CornerRadius.BottomLeft);
            else
                CornerRadius = new CornerRadius(cornerRoundingOut,
                                                CornerRadius.TopRight,
                                                CornerRadius.BottomRight,
                                                CornerRadius.BottomLeft);

            IsTopRounded = false;
        }

        public void RoundOutBottom()
        {
            if (IsOwn)
                CornerRadius = new CornerRadius(CornerRadius.TopLeft,
                                                CornerRadius.TopRight,
                                                cornerRoundingOut,
                                                CornerRadius.BottomLeft);
            else
                CornerRadius = new CornerRadius(CornerRadius.TopLeft,
                                                CornerRadius.TopRight,
                                                CornerRadius.BottomRight,
                                                cornerRoundingOut);

            IsBottomRounded = false;

        }
    }
}

using EchoMessenger.Helpers.Server;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using LoadingSpinnerControl;
using System.Windows.Controls.Primitives;

namespace EchoMessenger.Helpers.UI
{
    public class SearchResultUserCard : Border
    {
        public SearchResultUserCard(String avatarUrl, String username) : base()
        {
            BorderBrush = new SolidColorBrush(Colors.White);
            BorderThickness = new Thickness(0, 1, 0, 0);
            Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF1C1D26");
            Padding = new Thickness(10, 0, 10, 0);

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

            Child = grid;
        }

        public void SetFirst()
        {
            BorderThickness = new Thickness(BorderThickness.Left, 0, BorderThickness.Right, BorderThickness.Bottom);
        }

        public void SetLast()
        {
            BorderThickness = new Thickness(BorderThickness.Left, BorderThickness.Top, BorderThickness.Right, 1);
        }
    }

    public class TextBlockWithIcon : Border
    {
        public static readonly ImageBrush DeleteImage = Application.Current.FindResource("DeleteImage") as ImageBrush;
        public static readonly ImageBrush ReplyImage = Application.Current.FindResource("ReplyImage") as ImageBrush;
        public static readonly ImageBrush EditImage = Application.Current.FindResource("EditImage") as ImageBrush;

        public TextBlockWithIcon(ImageBrush imageBrush, String text) : base()
        {
            Padding = new Thickness(5);

            var grid = new Grid();

            var avatarColumn = new ColumnDefinition();
            avatarColumn.Width = new GridLength(50);

            var usernameColumn = new ColumnDefinition();

            grid.ColumnDefinitions.Add(avatarColumn);
            grid.ColumnDefinitions.Add(usernameColumn);

            var iconBorder = new Border();
            iconBorder.Height = 17.5;
            iconBorder.Width = 17.5;
            iconBorder.Background = imageBrush;
            iconBorder.Margin = new Thickness(5);

            var textBlock = new TextBlock();
            textBlock.Width = 75;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            textBlock.FontSize = 14;
            textBlock.FontFamily = new FontFamily("Segoe UI");
            textBlock.Text = text;

            Grid.SetColumn(iconBorder, 0);
            grid.Children.Add(iconBorder);

            Grid.SetColumn(textBlock, 1);
            grid.Children.Add(textBlock);

            Child = grid;
        }
    }

    public class DateCard : Border
    {
        public DateCard(DateTime date) : base()
        {
            HorizontalAlignment = HorizontalAlignment.Center;
            CornerRadius = new CornerRadius(10);
            Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF242430");
            Margin = new Thickness(0, 10, 0, 0);
            Effect = new DropShadowEffect();

            Label label = new Label();
            label.Foreground = new SolidColorBrush(Colors.White);
            label.Margin = new Thickness(2);

            if (date.Date == DateTime.Today)
                label.Content = "Today";
            else
                label.Content = date.ToLongDateString();

            Child = label;
        }
    }

    public class MessageBorder : Border
    {
        public static Action<Entities.Message>? OnEditButtonClick;
        public static Action<Entities.Message>? OnDeleteButtonClick;
        public static Action<Entities.Message>? OnReplyButtonClick;

        public static Popup? OpenedPopup;

        public Entities.Message? Message;

        public TextBlock MessageTextBlock;
        public TextBlock TimeTextBlock;
        public LoadingSpinner LoadingSpinner;
        public CheckMarks? CheckMarks;

        private Grid grid;
        private bool isOwn;

        public MessageBorder(String text, bool isOwn)
        {
            this.isOwn = isOwn;

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

            grid = new Grid();

            var messageColumn = new ColumnDefinition();
            messageColumn.MinWidth = 30;
            messageColumn.MaxWidth = 500;

            var timeColumn = new ColumnDefinition();

            var checkMarksColumn = new ColumnDefinition();

            grid.ColumnDefinitions.Add(messageColumn);
            grid.ColumnDefinitions.Add(timeColumn);
            grid.ColumnDefinitions.Add(checkMarksColumn);

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

            if (isOwn)
            {
                TimeTextBlock.Margin = new Thickness(0, 0, 5, 5);
                CheckMarks = new CheckMarks();
                CheckMarks.Visibility = Visibility.Collapsed;
                CheckMarks.VerticalAlignment = VerticalAlignment.Bottom;
                CheckMarks.Margin = new Thickness(0, 0, 7, 7);

                Grid.SetColumn(CheckMarks, 2);
                grid.Children.Add(CheckMarks);
            }
            
            Child = grid;
        }

        public void SetLoaded(Entities.Message message)
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

            var buttonsStack = new StackPanel();
            buttonsStack.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF1C1D26");
            buttonsStack.Orientation = Orientation.Vertical;

            var replyButton = new TextBlockWithIcon(TextBlockWithIcon.ReplyImage, "Reply");
            if (OnReplyButtonClick != null)
                replyButton.MouseLeftButtonUp += (s, e) => OnReplyButtonClick.Invoke(Message);

            buttonsStack.Children.Add(replyButton.ConvertToSelectable());

            if (isOwn)
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

            popupBorder.Child = buttonsStack;
            messagePopup.Child = popupBorder;
            grid.Children.Add(messagePopup);

            MouseRightButtonUp += (s, e) => { messagePopup.IsOpen = true; };

            messagePopup.Opened += (s, e) => OpenedPopup = messagePopup;
        }

        public void SetFailedLoading()
        {
            LoadingSpinner.IsLoading = false;

            Background = new SolidColorBrush(Colors.Red);
        }
    }

    public class CheckMarks : StackPanel
    {
        public CheckMark FirstCheck;
        public CheckMark SecondCheck;

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
    }

    public class CheckMark : StackPanel
    {
        public CheckMark(bool isFirst)
        {
            Orientation = Orientation.Horizontal;

            if (isFirst)
            {
                var firstBorder = new Border();
                firstBorder.Background = new SolidColorBrush(Colors.White);
                firstBorder.CornerRadius = new CornerRadius(1);
                firstBorder.RenderTransformOrigin = new Point(0.5, 0.5);
                firstBorder.Width = 1.5;
                firstBorder.Height = 6;
                firstBorder.VerticalAlignment = VerticalAlignment.Bottom;
                firstBorder.Margin = new Thickness(1, 0, 1, 0);

                var firstTransformGroup = new TransformGroup();
                firstTransformGroup.Children.Add(new RotateTransform()
                {
                    Angle = -45
                });

                firstBorder.RenderTransform = firstTransformGroup;

                Children.Add(firstBorder);
            }

            var secondBorder = new Border();
            secondBorder.Background = new SolidColorBrush(Colors.White);
            secondBorder.CornerRadius = new CornerRadius(1);
            secondBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            secondBorder.Width = 1.5;
            secondBorder.Height = 10;
            secondBorder.VerticalAlignment = VerticalAlignment.Bottom;
            secondBorder.Margin = new Thickness(2, 0, 0, 0);

            var secondTransformGroup = new TransformGroup();
            secondTransformGroup.Children.Add(new RotateTransform()
            {
                Angle = 45
            });

            secondBorder.RenderTransform = secondTransformGroup;

            Children.Add(secondBorder);
        }
    }

    public class UserIcon : Border
    {
        public OnlineStatusIcon OnlineStatusIcon { get; set; }
        public NotificationBadge NotificationBadge { get; set; }

        public UserIcon(String avatarUrl, bool isOnline) : base()
        {
            Width = 40;
            Height = 40;
            Margin = new Thickness(0, 0, 0, 10);
            CornerRadius = new CornerRadius(100);
            BorderBrush = new SolidColorBrush(Colors.Gray);
            BorderThickness = new Thickness(1);

            UpdateAvatar(avatarUrl);

            var grid = new Grid();

            this.OnlineStatusIcon = new OnlineStatusIcon(isOnline);
            grid.Children.Add(OnlineStatusIcon);

            this.NotificationBadge = new NotificationBadge();
            NotificationBadge.Visibility = Visibility.Collapsed;
            grid.Children.Add(NotificationBadge);

            Child = grid;
        }

        public void UpdateAvatar(String avatarUrl)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(Database.HostUrl(avatarUrl), UriKind.Absolute);
            bitmap.EndInit();
            Background = new ImageBrush() { ImageSource = bitmap, Stretch = Stretch.UniformToFill };
        }
    }

    public class OnlineStatusIcon : Border
    {
        public OnlineStatusIcon(bool isOnline) : base()
        {
            Width = Height = 10;
            HorizontalAlignment = HorizontalAlignment.Right;
            VerticalAlignment = VerticalAlignment.Bottom;
            CornerRadius = new CornerRadius(100);
            BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF131522");
            BorderThickness = new Thickness(2);

            if (isOnline)
                SetOnline();
            else
                SetOffline();
        }

        public void SetOnline()
        {
            var brush = (SolidColorBrush)new BrushConverter().ConvertFrom("#ff6088");
            Background = brush;
        }

        public void SetOffline()
        {
            var brush = new SolidColorBrush(Colors.Gray);
            Background = brush;
        }
    }

    public class NotificationBadge : Border
    {
        public readonly TextBlock NotificationTextBlock;
        public int NotificationsCount { get; private set; }

        public NotificationBadge() : base()
        {
            MinWidth = 21;
            Background = new SolidColorBrush(Colors.Red);
            HorizontalAlignment = HorizontalAlignment.Right;
            VerticalAlignment = VerticalAlignment.Top;
            CornerRadius = new CornerRadius(10);
            BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF131522");
            BorderThickness = new Thickness(3);
            Margin = new Thickness(0, -5, -10, 0);

            NotificationTextBlock = new TextBlock();
            NotificationTextBlock.VerticalAlignment = VerticalAlignment.Center;
            NotificationTextBlock.Foreground = new SolidColorBrush(Colors.White);
            NotificationTextBlock.FontSize = 10;
            NotificationTextBlock.FontFamily = new FontFamily("Segoe UI Semibold");
            NotificationTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            NotificationTextBlock.Margin = new Thickness(3, 1, 3, 1);

            Child = NotificationTextBlock;
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

        private String GetRepresentationString(int count) => count >= 100 ? "99+" : count.ToString();
    }

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

    public static class RemoveChildHelper
    {
        public static void RemoveChild(this DependencyObject parent, UIElement child)
        {
            if (parent is Panel panel)
                panel.Children.Remove(child);
            else if (parent is Decorator decorator)
                if (decorator.Child == child)
                    decorator.Child = null;

            var contentPresenter = parent as ContentPresenter;
            if (contentPresenter != null)
            {
                if (contentPresenter.Content == child)
                {
                    contentPresenter.Content = null;
                }
                return;
            }

            var contentControl = parent as ContentControl;
            if (contentControl != null)
            {
                if (contentControl.Content == child)
                {
                    contentControl.Content = null;
                }
                return;
            }
        }
    }
}
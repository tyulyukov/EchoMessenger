using EchoMessenger.Core;
using EchoMessenger.Helpers;
using EchoMessenger.Helpers.Extensions;
using EchoMessenger.Models;
using EchoMessenger.UI.Controls.Checks;
using EchoMessenger.UI.Controls.Typing;
using EchoMessenger.UI.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace EchoMessenger.UI.Controls.Cards.Chats
{
    public class UserCard : Border
    {
        public UserIcon AvatarIcon { get; private set; }
        public NotificationBadge NotificationBadge { get; private set; }

        private UserInfo user;
        private Message? lastMessage;

        private Grid cardGrid;
        private TextBlock usernameTextBlock;
        private TextBlock lastActivityTextBlock;
        private TextBlock editedPlaceholderTextBlock;
        private TextBlock timeTextBlock;
        private CheckMarks checkMarks;

        private Border typingIndicatorPlaceholder;
        private TypingIndicator typingIndicator;
        private TextBlock targetUserTypingTextBlock;

        private Duration selectionDuration = new Duration(TimeSpan.FromMilliseconds(200));
        private bool selected = false;
        private Color selectionColor;

        private bool isTyping = false;
        private TypeAssistant userTyping;

        private SolidColorBrush mainBrush = Core.Resources.Find<SolidColorBrush>("MainBrush");
        private SolidColorBrush activeBrush = Core.Resources.Find<SolidColorBrush>("ActiveBrush"); 
        private SolidColorBrush secondaryActiveBrush = Core.Resources.Find<SolidColorBrush>("SecondaryActiveBrush");

        public UserCard(UserInfo user, Chat chat, Message? lastMessage, bool isOnline, int unreadMessagesCount)
        {
            selectionColor = secondaryActiveBrush.Clone().Color;

            CornerRadius = new CornerRadius(10);
            Padding = new Thickness(10);
            Background = mainBrush.Clone();

            this.user = user;

            userTyping = new TypeAssistant(3100);
            userTyping.Idled += UserTyping_Idled;

            MouseEnter += UserCard_MouseEnter;
            MouseLeave += UserCard_MouseLeave;

            Loaded += UserCard_Loaded;
            Unloaded += UserCard_Unloaded;

            cardGrid = new Grid();
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition());
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            AvatarIcon = new UserIcon(user.avatarUrl, isOnline, selectionDuration, mainBrush.Clone().Color, selectionColor);

            Grid.SetColumn(AvatarIcon, 0);

            cardGrid.Children.Add(AvatarIcon);

            var mainStackPanel = new StackPanel();
            mainStackPanel.VerticalAlignment = VerticalAlignment.Top;
            mainStackPanel.HorizontalAlignment = HorizontalAlignment.Left;
            mainStackPanel.Orientation = Orientation.Vertical;
            mainStackPanel.Margin = new Thickness(10, 1, 0, 0);

            usernameTextBlock = new TextBlock();
            usernameTextBlock.Text = user.username;
            usernameTextBlock.Foreground = new SolidColorBrush(Colors.White);
            usernameTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
            //usernameTextBlock.FontFamily = new FontFamily("Segoe UI Semibold");
            usernameTextBlock.FontSize = 13.5;

            mainStackPanel.Children.Add(usernameTextBlock);

            var activityGrid = new Grid();
            activityGrid.Margin = new Thickness(0, 5, 0, 0);
            activityGrid.ColumnDefinitions.Add(new ColumnDefinition());
            activityGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            lastActivityTextBlock = new TextBlock();
            lastActivityTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
            lastActivityTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
            lastActivityTextBlock.MaxHeight = 20;
            lastActivityTextBlock.MinWidth = 0;
            lastActivityTextBlock.FontSize = 13;

            Grid.SetColumn(lastActivityTextBlock, 0);
            activityGrid.Children.Add(lastActivityTextBlock);

            editedPlaceholderTextBlock = new TextBlock();
            editedPlaceholderTextBlock.Text = "edited";
            editedPlaceholderTextBlock.Visibility = Visibility.Collapsed;
            editedPlaceholderTextBlock.FontSize = 12d;
            editedPlaceholderTextBlock.Foreground = new SolidColorBrush(Colors.Gray);
            editedPlaceholderTextBlock.Margin = new Thickness(3, 3, 0, 0);
            editedPlaceholderTextBlock.HorizontalAlignment = HorizontalAlignment.Left;

            Grid.SetColumn(editedPlaceholderTextBlock, 1);
            activityGrid.Children.Add(editedPlaceholderTextBlock);

            mainStackPanel.Children.Add(activityGrid);

            var typingStackPanel = new StackPanel();
            typingStackPanel.Orientation = Orientation.Horizontal;
            typingStackPanel.VerticalAlignment = VerticalAlignment.Top;
            typingStackPanel.HorizontalAlignment = HorizontalAlignment.Left;
            typingStackPanel.Margin = new Thickness(7.5, -2, 0, 0);

            typingIndicatorPlaceholder = new Border();

            typingStackPanel.Children.Add(typingIndicatorPlaceholder);
            
            targetUserTypingTextBlock = new TextBlock();
            targetUserTypingTextBlock.Foreground = activeBrush.Clone();
            targetUserTypingTextBlock.FontSize = 12d;
            targetUserTypingTextBlock.Text = "typing";
            targetUserTypingTextBlock.Visibility = Visibility.Collapsed;
            targetUserTypingTextBlock.VerticalAlignment = VerticalAlignment.Center;
            targetUserTypingTextBlock.HorizontalAlignment = HorizontalAlignment.Left;

            typingStackPanel.Children.Add(targetUserTypingTextBlock);

            mainStackPanel.Children.Add(typingStackPanel);

            var metaInfoStackPanel = new StackPanel();
            metaInfoStackPanel.VerticalAlignment = VerticalAlignment.Top;
            metaInfoStackPanel.HorizontalAlignment = HorizontalAlignment.Right;
            metaInfoStackPanel.Orientation = Orientation.Vertical;
            metaInfoStackPanel.Margin = new Thickness(10, 1, 0, 0);

            timeTextBlock = new TextBlock();
            timeTextBlock.Foreground = new SolidColorBrush(Colors.LightGray);
            timeTextBlock.FontSize = 11.5;

            var metaInfoGrid = new Grid();

            checkMarks = new CheckMarks();
            checkMarks.HorizontalAlignment = HorizontalAlignment.Right;
            checkMarks.Visibility = Visibility.Collapsed;

            NotificationBadge = new NotificationBadge(selectionColor, selectionDuration);
            NotificationBadge.Visibility = Visibility.Collapsed;
            NotificationBadge.Margin = new Thickness(0, 4, 0, 0);

            metaInfoGrid.Children.Add(checkMarks); 
            metaInfoGrid.Children.Add(NotificationBadge);

            if (lastMessage != null)
            {
                UpdateLastMessage(lastMessage);

                if (unreadMessagesCount != 0)
                {
                    NotificationBadge.Visibility = Visibility.Visible;
                    NotificationBadge.SetNotifications(chat.unreadMessagesCount);
                }
            }
            else
            {
                lastActivityTextBlock.Foreground = new SolidColorBrush(Colors.Gray);
                lastActivityTextBlock.Text = "Start the conversation";
            }

            metaInfoStackPanel.Children.Add(timeTextBlock);
            metaInfoStackPanel.Children.Add(metaInfoGrid);

            Grid.SetColumn(mainStackPanel, 1);
            Grid.SetColumn(metaInfoStackPanel, 2);

            cardGrid.Children.Add(mainStackPanel);
            cardGrid.Children.Add(metaInfoStackPanel);

            Child = cardGrid;
        }

        private void UserTyping_Idled(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() => 
            { 
                SetUserTyping(false);
            });
        }

        public void Select()
        {
            if (selected)
                return;

            MouseLeave -= UserCard_MouseLeave;
            MouseEnter -= UserCard_MouseEnter;

            NotificationBadge.Select();
            AvatarIcon.Select();

            var animation = new ColorAnimation();
            animation.To = selectionColor;
            animation.Duration = selectionDuration;
            Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);

            var editedPlaceholderAnimation = new ColorAnimation();
            editedPlaceholderAnimation.To = Colors.LightGray;
            editedPlaceholderAnimation.Duration = selectionDuration;
            editedPlaceholderTextBlock.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, editedPlaceholderAnimation);

            selected = true;
        }

        public void Deselect()
        {
            if (!selected)
                return;

            Background = mainBrush.Clone();
            editedPlaceholderTextBlock.Foreground = new SolidColorBrush(Colors.Gray);

            NotificationBadge.Deselect();
            AvatarIcon.Deselect();

            MouseLeave += UserCard_MouseLeave;
            MouseEnter += UserCard_MouseEnter;

            selected = false;
        }
        
        public void UpdateTargetUser(Models.UserInfo user)
        {
            AvatarIcon.UpdateAvatar(user.avatarUrl);
            usernameTextBlock.Text = user.username;
        }

        public void UpdateLastMessage(Models.Message lastMessage)
        {
            Dispatcher.Invoke(() =>
            {
                this.lastMessage = lastMessage;

                checkMarks.Visibility = Visibility.Collapsed;

                lastActivityTextBlock.Foreground = new SolidColorBrush(Colors.White);
                lastActivityTextBlock.Text = lastMessage.content;

                editedPlaceholderTextBlock.Visibility = lastMessage.edits.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

                if (lastMessage.sentAtLocal.IsToday())
                    timeTextBlock.Text = lastMessage.sentAtLocal.ToString("HH:mm");
                else if (lastMessage.sentAt.IsThisYear())
                    timeTextBlock.Text = lastMessage.sentAtLocal.ToString("dd.MM");
                else
                    timeTextBlock.Text = lastMessage.sentAtLocal.ToString("MM.yyyy");

                if (lastMessage.sender != user)
                {
                    checkMarks.Visibility = Visibility.Visible;

                    if (lastMessage.haveSeen)
                        checkMarks.SetHaveSeen();
                    else
                        checkMarks.SetHaveNotSeen();
                }
            });
        }

        public void SetHaveSeen(String messageId)
        {
            Dispatcher.Invoke(() =>
            {
                if (lastMessage?._id == messageId)
                {
                    lastMessage.haveSeen = true;
                    checkMarks.SetHaveSeen();
                }
            });
        }

        public void EditMessage(Models.Message message)
        {
            if (lastMessage == message)
            {
                message.haveSeen = lastMessage.haveSeen;
                UpdateLastMessage(message);
            }
        }

        public void UserTyping()
        {
            SetUserTyping(true);

            userTyping.TextChanged();
        }

        private void SetUserTyping(bool isTyping)
        {
            this.isTyping = isTyping;

            if (typingIndicator != null)
                typingIndicator.Visibility = isTyping ? Visibility.Visible : Visibility.Collapsed;
            targetUserTypingTextBlock.Visibility = isTyping ? Visibility.Visible : Visibility.Collapsed;

            lastActivityTextBlock.Visibility = !isTyping ? Visibility.Visible : Visibility.Collapsed;
            timeTextBlock.Visibility = !isTyping ? Visibility.Visible : Visibility.Collapsed;
            checkMarks.Visibility = !isTyping && lastMessage != null && lastMessage.sender != user ? Visibility.Visible : Visibility.Collapsed;
            NotificationBadge.Visibility = !isTyping && NotificationBadge.NotificationsCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            editedPlaceholderTextBlock.Visibility = !isTyping && lastMessage != null && lastMessage.edits.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UserCard_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var animation = new ColorAnimation();
            animation.To = mainBrush.Clone().Color;
            animation.Duration = selectionDuration;
            Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        private void UserCard_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var animation = new ColorAnimation();
            animation.To = Color.FromRgb(33, 34, 49);
            animation.Duration = selectionDuration;
            Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        private void UserCard_Loaded(object sender, RoutedEventArgs e)
        {
            if (typingIndicator != null)
                typingIndicatorPlaceholder.RemoveChild(typingIndicator);

            typingIndicator = new TypingIndicator()
            {
                Diameter = 4d,
                Spacing = new Thickness(1.2d),
                Margin = new Thickness(-7.5, 3.5, 5, 0),
                Color = activeBrush.Clone(),
                Visibility = isTyping ? Visibility.Visible : Visibility.Collapsed,
            };
            typingIndicatorPlaceholder.Child = typingIndicator;
        }

        private void UserCard_Unloaded(object sender, RoutedEventArgs e)
        {
            typingIndicatorPlaceholder.RemoveChild(typingIndicator);
        }
    }
}

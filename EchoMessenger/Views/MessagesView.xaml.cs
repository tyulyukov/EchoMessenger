using EchoMessenger.Entities;
using EchoMessenger.Helpers;
using EchoMessenger.Helpers.Server;
using EchoMessenger.Helpers.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EchoMessenger
{
    public partial class MessagesView : UserControl
    {
        public const int LoadingMessagesCount = 15;

        private MessengerWindow owner;
        private SynchronizationContext uiSync;
        private TypeAssistant userTyping;

        private bool isLoading = false;
        private bool isTyping = false;

        private Dictionary<String, MessageBorder> messages;
        private bool isAllMessagesLoaded = false;

        private TypingIndicatorControl.TypingIndicator TypingIndicator;

        private Chat currentChat;
        private UserInfo currentUser;
        private UserInfo targetUser;

        private double prevHeight = 0;
        private bool isLoadingMessages = false;
        private DateTime? lastMessageSentAt = null;
        private DateTime? firstMessageSentAt = null;
        private DateTime? lastTyping = null;

        public MessagesView(MessengerWindow owner, UserInfo user, Chat chat, bool isOnline)
        {
            InitializeComponent();
            uiSync = SynchronizationContext.Current;
            userTyping = new TypeAssistant(3100);

            this.owner = owner;
            currentUser = user;
            currentChat = chat;

            targetUser = currentChat.sender._id == currentUser._id ? currentChat.receiver : currentChat.sender;

            messages = new Dictionary<String, MessageBorder>();

            SetOnlineStatus(isOnline);

            var placeholder = MessageTextBox.Text;
            bool isTextChanged = false;

            TextChangedEventHandler textChanged = async (s, e) =>
            {
                isTextChanged = !String.IsNullOrEmpty(MessageTextBox.Text);

                if (lastTyping == null || lastTyping?.AddMilliseconds(3000) < DateTime.Now)
                {
                    lastTyping = DateTime.Now;
                    await Messages.SendTyping(targetUser._id);
                }
            };

            MessageTextBox.GotFocus += (s, e) =>
            {
                if (MessageTextBox.Text == placeholder && !isTextChanged)
                {
                    SendMessageButton.IsEnabled = true;
                    MessageTextBox.Text = String.Empty;
                }

                MessageTextBox.TextChanged += textChanged;
            };

            MessageTextBox.LostFocus += (s, e) =>
            {
                MessageTextBox.TextChanged -= textChanged;

                if (String.IsNullOrEmpty(MessageTextBox.Text))
                {
                    SendMessageButton.IsEnabled = false;
                    MessageTextBox.Text = placeholder;
                }
            };

            MessageTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && Keyboard.IsKeyDown(Key.LeftShift))
                {
                    var index = MessageTextBox.CaretIndex;
                    MessageTextBox.Text = MessageTextBox.Text.Insert(index, "\n");
                    MessageTextBox.CaretIndex = index + 1;
                }
                else if (e.Key == Key.Enter)
                {
                    SendMessageHandle();
                }
            };

            userTyping.Idled += (s, e) =>
            {
                SetUserTyping(false);
            };

            TargetUserName.Content = targetUser.username;
        }

        public void Open()
        {
            uiSync.Post((s) =>
            {
                TargetUserName.ChangeVisibility(true, TimeSpan.FromMilliseconds(150));
                TargetUserOnlineStatus.ChangeVisibility(true, TimeSpan.FromMilliseconds(150));
                TargetUserOnlineStatusIcon.ChangeVisibility(true, TimeSpan.FromMilliseconds(150));
            }, null);
        }

        public void UpdateUser(UserInfo user)
        {
            currentUser = user;
        }

        public void ShowLoading(bool visible)
        {
            LoadingBorder.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            isLoading = visible;
        }

        public void SetOnlineStatus(bool isOnline, DateTime? lastTimeOnline = null)
        {
            uiSync.Post((s) =>
            {
                if (isOnline)
                {
                    TargetUserOnlineStatus.Content = "online";
                }
                else
                {
                    var onlineDate = lastTimeOnline == null ? targetUser.lastOnlineAtLocal : lastTimeOnline;

                    if (onlineDate?.Date != DateTime.Today)
                        TargetUserOnlineStatus.Content = $"last online { onlineDate?.ToLongDateString() }";
                    else
                        TargetUserOnlineStatus.Content = $"last online at { onlineDate?.ToShortTimeString() }";
                }

                TargetUserOnlineStatus.Foreground = isOnline ? (SolidColorBrush)new BrushConverter().ConvertFrom("#ff6088") : new SolidColorBrush(Colors.Gray);
                TargetUserOnlineStatusIcon.Background = isOnline ? (SolidColorBrush)new BrushConverter().ConvertFrom("#ff6088") : new SolidColorBrush(Colors.Gray);
            }, null);
        }

        public void AddMessage(Message message)
        {
            if (messages.ContainsKey(message._id))
            {
                SuccessLoadingMessage(message);
                return;
            }

            uiSync.Send(s =>
            {
                var messageBorder = new MessageBorder(message.content, message.sender.username == currentUser.username);
                messageBorder.SetLoaded(message);

                messages.Add(message._id, messageBorder);

                if (firstMessageSentAt == null || firstMessageSentAt?.Date != DateTime.Today)
                    MessagesStackPanel.Children.Add(new DateCard(message.sentAtLocal));
                else if (firstMessageSentAt?.AddMinutes(10) < message.sentAtLocal)
                    messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top + 5, messageBorder.Margin.Right, messageBorder.Margin.Bottom);
                else if (firstMessageSentAt?.AddHours(1) < message.sentAtLocal)
                    messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top + 10, messageBorder.Margin.Right, messageBorder.Margin.Bottom);

                firstMessageSentAt = message.sentAtLocal;

                messageBorder.SetSlideFromBottomOnLoad();
                MessagesStackPanel.Children.Add(messageBorder);
            }, null);
        }

        public void SuccessLoadingMessage(Message message)
        {
            if (messages.TryGetValue(message._id, out var messageBorder))
            {
                uiSync.Send(s =>
                {
                    messageBorder.SetLoaded(message);
                }, null);
            }
        }

        public void UpdateTargetUser(UserInfo user)
        {
            uiSync.Send(s =>
            {
                targetUser = user;
                TargetUserName.Content = targetUser.username;
            }, null);
        }

        public void FailLoadingMessage(String messageId)
        {
            if (messages.TryGetValue(messageId, out var messageBorder))
            {
                uiSync.Send(s =>
                {
                    messageBorder.SetFailedLoading();
                }, null);
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

            uiSync.Send(s =>
            {
                TypingIndicator.Visibility = isTyping ? Visibility.Visible : Visibility.Collapsed;
                TargetUserTypingLabel.Visibility = isTyping ? Visibility.Visible : Visibility.Collapsed;

                TargetUserOnlineStatusIcon.Visibility = !isTyping ? Visibility.Visible : Visibility.Collapsed;
                TargetUserOnlineStatus.Visibility = !isTyping ? Visibility.Visible : Visibility.Collapsed;
            }, null);
        }

        private async void LoadOlderMessages()
        {
            try
            {
                ShowLoading(true);

                var response = await Database.LoadMessages(currentChat._id, messages.Count, LoadingMessagesCount);

                if (response == null || response.StatusCode == (HttpStatusCode)0 || response.StatusCode == (HttpStatusCode)500)
                {
                    return;
                }
                else if (response.StatusCode == (HttpStatusCode)200)
                {
                    if (String.IsNullOrEmpty(response.Content))
                        return;

                    var result = JArray.Parse(response.Content);

                    var messages = result?.ToObject<IEnumerable<Message>>();

                    isLoadingMessages = true;
                    prevHeight = MessagesScroll.ExtentHeight;
                    MessagesScroll.LayoutUpdated += MessagesScroll_LayoutUpdated;

                    if (messages == null || messages.Count() == 0)
                    {
                        if (lastMessageSentAt != null)
                            MessagesStackPanel.Children.Insert(0, new DateCard((DateTime)lastMessageSentAt));

                        isAllMessagesLoaded = true;

                        return;
                    }

                    foreach (var message in messages)
                    {
                        if (firstMessageSentAt == null)
                            firstMessageSentAt = message.sentAtLocal;

                        MessageBorder messageBorder = new MessageBorder(message.content, message.sender._id == currentUser._id);
                        messageBorder.SetLoaded(message);

                        this.messages.Add(message._id, messageBorder);

                        if ((lastMessageSentAt?.Year == message.sentAtLocal.Year && lastMessageSentAt?.DayOfYear > message.sentAtLocal.DayOfYear) || lastMessageSentAt?.Year > message.sentAtLocal.Year)
                        {
                            var dateCard = new DateCard((DateTime)lastMessageSentAt);
                            dateCard.SetSlideFromBottomOnLoad();
                            MessagesStackPanel.Children.Insert(0, dateCard);
                        }
                        else if (lastMessageSentAt > message.sentAtLocal.AddHours(1))
                        {
                            messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top, messageBorder.Margin.Right, messageBorder.Margin.Bottom + 10);
                        }
                        else if (lastMessageSentAt > message.sentAtLocal.AddMinutes(10))
                        {
                            messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top, messageBorder.Margin.Right, messageBorder.Margin.Bottom + 5);
                        }

                        messageBorder.SetSlideFromBottomOnLoad();
                        MessagesStackPanel.Children.Insert(0, messageBorder);
                        lastMessageSentAt = message.sentAtLocal;
                    }

                    if (messages.Count() < LoadingMessagesCount)
                    {
                        if (lastMessageSentAt != null)
                            MessagesStackPanel.Children.Insert(0, new DateCard((DateTime)lastMessageSentAt));

                        isAllMessagesLoaded = true;
                    }
                }
                else if (response.StatusCode == (HttpStatusCode)401)
                {
                    RegistryManager.ForgetJwt();
                    owner.Hide();
                    new LoginWindow().Show();
                    owner.Close();
                }
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void MessagesScroll_LayoutUpdated(object? sender, EventArgs e)
        {
            if (MessagesScroll.ExtentHeight == prevHeight)
                return;

            isLoadingMessages = false;
            MessagesScroll.ScrollToVerticalOffset(MessagesScroll.ExtentHeight - prevHeight);
            MessagesScroll.LayoutUpdated -= MessagesScroll_LayoutUpdated;
        }

        private void SendMessageButtonClick(object sender, RoutedEventArgs e)
        {
            SendMessageHandle();
        }

        private async void SendMessageHandle()
        {
            if (String.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            var sentAt = DateTime.Now;
            var messageId = Guid.NewGuid().ToString();
            var content = MessageTextBox.Text.Trim();
            MessageTextBox.Text = String.Empty;

            await Messages.SendMessage(messageId, currentChat._id, content);

            var messageBorder = new MessageBorder(content, true);
            messages.Add(messageId, messageBorder);

            if (firstMessageSentAt == null || (firstMessageSentAt?.Year == sentAt.Year && firstMessageSentAt?.DayOfYear < sentAt.DayOfYear) || firstMessageSentAt?.Year < sentAt.Year)
            {
                var dateCard = new DateCard(sentAt);
                dateCard.SetSlideFromBottomOnLoad();
                MessagesStackPanel.Children.Add(dateCard);
            }
            else if (firstMessageSentAt?.AddMinutes(10) < sentAt)
            {
                messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top + 5, messageBorder.Margin.Right, messageBorder.Margin.Bottom);
            }
            else if (firstMessageSentAt?.AddHours(1) < sentAt)
            {
                messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top + 10, messageBorder.Margin.Right, messageBorder.Margin.Bottom);
            }

            firstMessageSentAt = sentAt;

            messageBorder.SetSlideFromBottomOnLoad();
            MessagesStackPanel.Children.Add(messageBorder);
            MessagesScroll.ScrollToBottom();
        }

        private void ButtonGoBottom_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessagesScroll.ScrollToBottom();
        }

        private void MessagesScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            bool isVisible = MessagesScroll.VerticalOffset + 50 < MessagesScroll.ScrollableHeight;
            ButtonGoBottom.ChangeOpacity(isVisible, TimeSpan.FromMilliseconds(150));

            if (MessagesScroll.VerticalOffset == 0 && !isLoadingMessages && !isAllMessagesLoaded)
                LoadOlderMessages();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (TypingIndicator != null)
                TypingIndicatorPlaceholder.RemoveChild(TypingIndicator);

            TypingIndicator = new TypingIndicatorControl.TypingIndicator()
            {
                Diameter = 4d,
                Spacing = new Thickness(1.2d),
                Margin = new Thickness(0, 3.5, 0, 0),
                Color = (SolidColorBrush)new BrushConverter().ConvertFrom("#ff6088"),
                Visibility = isTyping ? Visibility.Visible : Visibility.Collapsed,
            };
            TypingIndicatorPlaceholder.Child = TypingIndicator;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            TypingIndicatorPlaceholder.RemoveChild(TypingIndicator);
        }
    }
}

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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EchoMessenger
{
    public partial class MessagesView : UserControl
    {
        public const int LoadingMessagesCount = 15;

        private readonly MessengerWindow owner;
        private readonly SynchronizationContext uiSync;
        private readonly TypeAssistant userTyping;

        private readonly Chat currentChat;
        private UserInfo currentUser;
        private UserInfo targetUser;

        private bool isLoading = false;
        private bool isTyping = false;

        private Dictionary<String, MessageBorder> messages;
        private Dictionary<String, MessageBorder> unreadMessages;
        private bool isAllMessagesLoaded = false;

        private TypingIndicatorControl.TypingIndicator TypingIndicator;

        private double prevHeight = 0;
        private bool isLoadingMessages = false;
        private DateTime? lastMessageSentAt = null;
        private DateTime? firstMessageSentAt = null;
        private DateTime? lastTyping = null;

        private Message? replyingOnMessage = null;
        private Message? editingMessage = null;

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
            unreadMessages = new Dictionary<String, MessageBorder>();

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

            MessageTextBox.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
                {
                    int index;

                    if (MessageTextBox.SelectionLength != 0)
                    {
                        index = MessageTextBox.SelectionStart;
                        MessageTextBox.Text = MessageTextBox.Text.Remove(MessageTextBox.SelectionStart, MessageTextBox.SelectionLength);
                    }
                    else
                    {
                        index = MessageTextBox.CaretIndex;
                    }

                    MessageTextBox.Text = MessageTextBox.Text.Insert(index, "\n");
                    MessageTextBox.CaretIndex = index + 1;
                    MessageTextBox.SelectionLength = 0;

                    e.Handled = true;
                }
                else if (e.Key == Key.Enter)
                {
                    EnterButtonDownHandle();
                    e.Handled = true;
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

            uiSync.Send((s) =>
            {
                foreach (var messageBorder in messages.Values)
                    if (messageBorder.Message != null && messageBorder.Message.repliedOn != null && messageBorder.Message.repliedOn.sender._id == user._id)
                        messageBorder.ReplyUserUpdate(user);
            }, null);
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
                var messageBorder = new MessageBorder(message.content, message.sender._id == currentUser._id);

                if (message.repliedOn != null)
                    messageBorder.SetReplyingMessage(message.repliedOn.content, message.repliedOn.sender.username, () =>
                    {
                        uiSync.Send(s =>
                        {
                            messageBorder.BringIntoView();
                        }, null);
                    });

                messageBorder.SetLoaded(message);

                messages.Add(message._id, messageBorder);

                if (message.sender._id != currentUser._id)
                    unreadMessages.Add(message._id, messageBorder);

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

                uiSync.Send((s) =>
                {
                    foreach (var messageBorder in messages.Values)
                        if (messageBorder.Message != null && messageBorder.Message.repliedOn != null && messageBorder.Message.repliedOn.sender._id == user._id)
                            messageBorder.ReplyUserUpdate(user);
                }, null);
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

        public void MessageRead(String messageId)
        {
            if (messages.TryGetValue(messageId, out var messageBorder))
                uiSync.Send(s =>
                {
                    messageBorder.CheckMarks?.SetHaveSeen();
                }, null);
        }

        public void MessageDeleted(String messageId)
        {
            if (messages.TryGetValue(messageId, out var messageBorder))
                uiSync.Send(s =>
                {
                    var deletionTime = TimeSpan.FromMilliseconds(250);

                    messageBorder.ChangeVisibility(false, deletionTime);
                    Task.Delay(deletionTime).ContinueWith((t) =>
                    {
                        uiSync.Send(s =>
                        {
                            MessagesStackPanel.Children.Remove(messageBorder);
                        }, null);
                    });

                    foreach (var message in messages.Values)
                        if (message.Message?.repliedOn != null && message.Message?.repliedOn._id == messageId)
                            message.DeleteReplyingMessage(uiSync);

                    var messageIndex = MessagesStackPanel.Children.IndexOf(messageBorder);

                    if (messageIndex == MessagesStackPanel.Children.Count - 1)
                    {
                        if (MessagesStackPanel.Children[messageIndex - 1] is MessageBorder previousMessageBorder)
                            firstMessageSentAt = previousMessageBorder.Message?.sentAt;
                        else if (messageIndex - 2 >= 0
                              && MessagesStackPanel.Children[messageIndex - 2] is MessageBorder lastMessageBorder)
                            firstMessageSentAt = lastMessageBorder.Message?.sentAt;
                        else
                            firstMessageSentAt = null;
                    }

                    if (MessagesStackPanel.Children[messageIndex - 1] is DateCard dateCard
                    && (MessagesStackPanel.Children.Count <= messageIndex + 1
                     || MessagesStackPanel.Children[messageIndex + 1] is not MessageBorder))
                    {
                        dateCard.ChangeVisibility(false, deletionTime);
                        Task.Delay(deletionTime).ContinueWith((t) =>
                        {
                            uiSync.Send(s =>
                            {
                                MessagesStackPanel.Children.Remove(dateCard);
                            }, null);
                        });
                    }
                }, null);
        }

        public void MessageEdited(Message message)
        {
            if (messages.TryGetValue(message._id, out var messageBorder))
                uiSync.Send(s =>
                {
                    messageBorder.SetEdited(message);

                    foreach (var messageBorder in messages.Values)
                        if (messageBorder.Message?.repliedOn != null && messageBorder.Message?.repliedOn._id == message._id)
                            messageBorder.SetReplyEdited(message.content);
                }, null);
        }

        public bool IsUnreadMessage(String messageId) => unreadMessages.ContainsKey(messageId);

        private void SetUserTyping(bool isTyping)
        {
            this.isTyping = isTyping;

            uiSync.Send(s =>
            {
                if (TypingIndicator != null)
                    TypingIndicator.Visibility = isTyping ? Visibility.Visible : Visibility.Collapsed;
                TargetUserTypingLabel.Visibility = isTyping ? Visibility.Visible : Visibility.Collapsed;

                TargetUserOnlineStatusIcon.Visibility = !isTyping ? Visibility.Visible : Visibility.Collapsed;
                TargetUserOnlineStatus.Visibility = !isTyping ? Visibility.Visible : Visibility.Collapsed;
            }, null);
        }

        private async void ReadMessage(String messageId)
        {
            owner.ReadMessage(targetUser._id);
            unreadMessages.Remove(messageId);
            await Messages.ReadMessage(messageId);
        }

        private async Task LoadOlderMessages()
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

                    RenderMessages(messages);

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

        private void RenderMessages(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                if (firstMessageSentAt == null)
                    firstMessageSentAt = message.sentAtLocal;

                message.chat = currentChat;

                if (message.repliedOn != null)
                    message.repliedOn.chat = currentChat;

                MessageBorder messageBorder = new MessageBorder(message.content, message.sender._id == currentUser._id);

                if (message.repliedOn != null)
                    messageBorder.SetReplyingMessage(message.repliedOn.content, message.repliedOn.sender.username, () =>
                    {
                        uiSync.Send(async s =>
                        {
                            while (true)
                            {
                                if (this.messages.TryGetValue(message.repliedOn._id, out var repliedMessageBorder))
                                {
                                    _ = Task.Delay(300).ContinueWith((t) =>
                                    {
                                        uiSync.Send(s =>
                                        {
                                            repliedMessageBorder.BringIntoView(new Rect(0, -50, repliedMessageBorder.ActualWidth, repliedMessageBorder.ActualHeight + 50));
                                        }, null);
                                    });

                                    return;
                                }

                                await LoadOlderMessages();
                            }
                        }, null);
                    });

                messageBorder.SetLoaded(message);

                if (message.edits != null && message.edits.Count > 0)
                {
                    messageBorder.SetEdited();
                }

                this.messages.Add(message._id, messageBorder);

                if (message.sender._id != currentUser._id && !message.haveSeen)
                    unreadMessages.Add(message._id, messageBorder);

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
        }

        private void MessagesScroll_LayoutUpdated(object? sender, EventArgs e)
        {
            if (MessagesScroll.ExtentHeight == prevHeight)
                return;

            isLoadingMessages = false;

            MessagesScroll.ScrollToVerticalOffset(MessagesScroll.ExtentHeight - prevHeight);
            MessagesScroll.LayoutUpdated -= MessagesScroll_LayoutUpdated;
        }

        private MessageBorder CreateOwnMessageBorder(String content, bool isOwn, DateTime sentAt)
        {
            var messageBorder = new MessageBorder(content, true);

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
            return messageBorder;
        }

        private void SendMessageButtonClick(object sender, RoutedEventArgs e)
        {
            if (replyingOnMessage == null)
                SendMessage();
            else
                ReplyOnMessage();
        }

        private void EnterButtonDownHandle()
        {
            if (replyingOnMessage != null)
                ReplyOnMessage();
            else if (editingMessage != null)
                EditMessage();
            else
                SendMessage();
        }

        private void SendMessage()
        {
            if (String.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            var sentAt = DateTime.Now;
            var messageId = Guid.NewGuid().ToString();
            var content = MessageTextBox.Text.Trim();
            
            _ = Task.Run(() => Messages.SendMessage(messageId, currentChat._id, content));

            MessageTextBox.Text = String.Empty;

            var messageBorder = CreateOwnMessageBorder(content, true, sentAt);

            messages.Add(messageId, messageBorder);

            MessagesStackPanel.Children.Add(messageBorder);
            MessagesScroll.ScrollToBottom();
        }

        private void ReplyOnMessage()
        {
            if (String.IsNullOrWhiteSpace(MessageTextBox.Text) || replyingOnMessage == null)
                return;

            var sentAt = DateTime.Now;
            var messageId = Guid.NewGuid().ToString();
            var content = MessageTextBox.Text.Trim();

            _ = Task.Run(() => Messages.ReplyMessage(messageId, currentChat._id, content, replyingOnMessage._id));

            MessageTextBox.Text = String.Empty;

            var messageBorder = CreateOwnMessageBorder(content, true, sentAt);

            var replyingOnMessageId = replyingOnMessage._id;
            messageBorder.SetReplyingMessage(replyingOnMessage.content, replyingOnMessage.sender.username, () =>
            {
                uiSync.Send(async s =>
                {
                    while (true)
                    {
                        if (this.messages.TryGetValue(replyingOnMessageId, out var repliedMessageBorder))
                        {
                            _ = Task.Delay(150).ContinueWith((t) =>
                            {
                                uiSync.Send(s =>
                                {
                                    repliedMessageBorder.BringIntoView(new Rect(0, -50, repliedMessageBorder.ActualWidth, repliedMessageBorder.ActualHeight + 50));
                                }, null);
                            });

                            return;
                        }

                        await LoadOlderMessages();
                    }
                }, null);
            });

            LeaveReplying();

            messages.Add(messageId, messageBorder);

            MessagesStackPanel.Children.Add(messageBorder);
            MessagesScroll.ScrollToBottom();
        }

        private void EditMessage()
        {
            if (String.IsNullOrWhiteSpace(MessageTextBox.Text) || editingMessage == null)
                return;

            var content = MessageTextBox.Text.Trim();

            _ = Task.Run(() => Messages.EditMessage(editingMessage._id, content));

            if (messages.TryGetValue(editingMessage._id, out var editedMessageBorder))
            {
                editingMessage.content = content;
                editedMessageBorder.SetEdited(editingMessage);

                foreach (var messageBorder in messages.Values)
                    if (messageBorder.Message?.repliedOn != null && messageBorder.Message?.repliedOn._id == editingMessage._id)
                        messageBorder.SetReplyEdited(editingMessage.content);

                editedMessageBorder.BringIntoView(new Rect(0, -50, editedMessageBorder.ActualWidth, editedMessageBorder.ActualHeight + 50));
            }

            MessageTextBox.Text = String.Empty;

            LeaveEditing();
        }

        private void ButtonGoBottom_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessagesScroll.ScrollToBottom();
        }

        private void MessagesScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (MessageBorder.OpenedPopup != null)
                MessageBorder.OpenedPopup.IsOpen = false;

            foreach (var message in unreadMessages)
            {
                Rect bounds = message.Value.TransformToAncestor(MessagesScroll).TransformBounds(new Rect(0.0, 0.0, message.Value.ActualWidth, message.Value.ActualHeight));
                Rect rect = new Rect(0.0, 0.0, MessagesScroll.ActualWidth, MessagesScroll.ActualHeight);
                if (rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight))
                {
                    ReadMessage(message.Key);
                }
            }
            
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

        private void CancelReplyButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LeaveReplying();
        }

        private void ReplyMessagePanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (replyingOnMessage != null)
                if (messages.TryGetValue(replyingOnMessage._id, out var messageBorder))
                    messageBorder.BringIntoView();
        }
        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (editingMessage != null)
                if (messages.TryGetValue(editingMessage._id, out var messageBorder))
                    messageBorder.BringIntoView();
        }

        private void CancelEditButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LeaveEditing();
        }

        private void ConfirmEditButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EditMessage();
        }

        public void EnterReplying(Message message)
        {
            uiSync.Send(s =>
            {
                replyingOnMessage = message;
                ReplyPanel.Visibility = Visibility.Visible;

                ReplyToNickname.Content = message.sender.username;
                ReplyToText.Content = message.content;

                MessageTextBox.Focus();
            }, null);
        }

        public void LeaveReplying()
        {
            uiSync.Send(s =>
            {
                replyingOnMessage = null;
                ReplyPanel.Visibility = Visibility.Collapsed;
            }, null);
            
        }

        public void EnterEditing(Message message)
        {
            uiSync.Send(s =>
            {
                editingMessage = message;
                EditPanel.Visibility = Visibility.Visible;

                EditText.Content = message.content;

                MessageTextBox.Text = message.content;
                MessageTextBox.Focus();
                SendMessageButton.Visibility = Visibility.Collapsed;
                ConfirmEditButton.Visibility = Visibility.Visible;
            }, null);
        }

        public void LeaveEditing()
        {
            uiSync.Send(s =>
            {
                editingMessage = null;
                EditPanel.Visibility = Visibility.Collapsed;

                MessageTextBox.Text = String.Empty;
                MessageTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                SendMessageButton.Visibility = Visibility.Visible;
                ConfirmEditButton.Visibility = Visibility.Collapsed;
            }, null);
        }
    }
}

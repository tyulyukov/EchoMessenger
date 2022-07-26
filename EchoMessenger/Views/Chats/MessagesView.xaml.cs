using EchoMessenger.Models;
using EchoMessenger.Helpers;
using EchoMessenger.Helpers.Extensions;
using EchoMessenger.Helpers.Server;
using EchoMessenger.UI.Controls.Messages;
using EchoMessenger.UI.Extensions;
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
using EchoMessenger.UI.Controls.Cards.Dialog;
using EchoMessenger.UI.Controls.Typing;

namespace EchoMessenger.Views.Chats
{
    public partial class MessagesView : UserControl
    {
        public const int LoadingMessagesCount = 15;
        public Message? LastSentMessage { get; private set; }

        public int UnreadMessagesCount => unreadMessages.Count;

        private readonly MessengerWindow owner;
        private readonly ChatsListView ownerList;
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

        private TypingIndicator TypingIndicator;

        private double prevHeight = 0;
        private bool isLoadingMessages = false;
        private DateTime? firstMessageSentAt = null;
        private DateTime? lastMessageSentAt = null;  // including loading messages
        private DateTime? lastTyping = null;

        private Message? replyingOnMessage = null;
        private Message? editingMessage = null;

        private DateTime? lastSendingAt = null;

        public MessagesView(MessengerWindow owner, ChatsListView ownerList, UserInfo user, Chat chat, bool isOnline)
        {
            InitializeComponent();
            uiSync = SynchronizationContext.Current;
            userTyping = new TypeAssistant(3100);

            this.owner = owner;
            this.ownerList = ownerList;
            currentUser = user;
            currentChat = chat;

            targetUser = currentChat.sender == currentUser ? currentChat.receiver : currentChat.sender;

            messages = new Dictionary<String, MessageBorder>();
            unreadMessages = new Dictionary<String, MessageBorder>();

            if (chat.messages != null && chat.messages.Count > 0)
                LastSentMessage = chat.messages.Last();

            SetOnlineStatus(isOnline);

            TextChangedEventHandler textChanged = async (s, e) =>
            {
                if (lastTyping == null || lastTyping?.AddMilliseconds(3000) < DateTime.Now)
                {
                    lastTyping = DateTime.Now;
                    await Messages.SendTyping(targetUser._id);
                }
            };

            MessageTextBox.GotFocus += (s, e) =>
            {
                MessageTextBox.TextChanged += textChanged;
            };

            MessageTextBox.LostFocus += (s, e) =>
            {
                MessageTextBox.TextChanged -= textChanged;
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
                    if (messageBorder.Message != null && messageBorder.Message.repliedOn != null && messageBorder.Message.repliedOn.sender == user)
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

                    if (onlineDate.Value.IsToday())
                        TargetUserOnlineStatus.Content = $"last online at { onlineDate?.ToShortTimeString() }";
                    else
                        TargetUserOnlineStatus.Content = $"last online { onlineDate?.ToLongDateString() }";
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
                var messageBorder = new MessageBorder(message.content, message.sender == currentUser);

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

                if (message.sender != currentUser)
                    unreadMessages.Add(message._id, messageBorder);

                if (lastMessageSentAt == null || !lastMessageSentAt.Value.IsToday())
                {
                    var dateCard = new DateCard(message.sentAtLocal);
                    dateCard.SetSlideFromBottomOnLoad();
                    MessagesStackPanel.Children.Add(dateCard);
                }
                else if (message.sentAtLocal.IsOlderOnTime(lastMessageSentAt.Value, TimeSpan.FromMinutes(10)))
                {
                    messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top + 5,
                                                         messageBorder.Margin.Right, messageBorder.Margin.Bottom);
                }
                else if (message.sentAtLocal.IsOlderOnTime(lastMessageSentAt.Value, TimeSpan.FromHours(1)))
                {
                    messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top + 10,
                                                         messageBorder.Margin.Right, messageBorder.Margin.Bottom);
                }
                else if (MessagesStackPanel.Children.Count > 0 
                      && MessagesStackPanel.Children[MessagesStackPanel.Children.Count - 1] is MessageBorder previousMessageBorder)
                {
                    if ((previousMessageBorder.IsOwn && message.sender == currentUser) || (!previousMessageBorder.IsOwn && message.sender == targetUser))
                    {
                        previousMessageBorder.RoundInBottom();
                        messageBorder.RoundInTop();
                    }
                }

                LastSentMessage = message;
                lastMessageSentAt = message.sentAtLocal;

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
                    if (MessagesStackPanel.Children.IndexOf(messageBorder) == MessagesStackPanel.Children.Count - 1)
                        LastSentMessage = message;

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
                    foreach (var element in MessagesStackPanel.Children)
                    {
                        if (element is MessageBorder messageBorder && messageBorder.Message != null && messageBorder.Message.repliedOn != null && messageBorder.Message.repliedOn.sender == user)
                            messageBorder.ReplyUserUpdate(user);
                        else if (element is DialogCard dialogCard)
                            dialogCard.UpdateTargetUser(user);
                    }
                        
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
                    messageBorder.SetHaveSeen();
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

                    if (!messageBorder.IsTopRounded
                     && messageIndex + 1 < MessagesStackPanel.Children.Count
                     && MessagesStackPanel.Children[messageIndex + 1] is MessageBorder nextMessageBorder)
                    {
                        nextMessageBorder.RoundOutTop();
                    }
                    else if (!messageBorder.IsBottomRounded
                          && messageIndex - 1 >= 0
                          && MessagesStackPanel.Children[messageIndex - 1] is MessageBorder previousMessageBorder)
                    {
                        previousMessageBorder.RoundOutBottom();
                    }

                    // CHANGE THE MARGIN TO SAVE THE SPACE BETWEEN LONG TIME AGO MESSAGES!

                    if (messageIndex == MessagesStackPanel.Children.Count - 1)
                    {
                        if (MessagesStackPanel.Children[messageIndex - 1] is MessageBorder previousMessageBorder)
                        {
                            LastSentMessage = previousMessageBorder.Message;
                            lastMessageSentAt = previousMessageBorder.Message?.sentAtLocal;
                        }
                        else if (messageIndex - 2 >= 0
                              && MessagesStackPanel.Children[messageIndex - 2] is MessageBorder lastMessageBorder)
                        {
                            LastSentMessage = lastMessageBorder.Message;
                            lastMessageSentAt = lastMessageBorder.Message?.sentAtLocal;
                        }
                        else
                        {
                            LastSentMessage = null;
                            lastMessageSentAt = null;
                        }
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
                        if (messageBorder.Message?.repliedOn != null && messageBorder.Message?.repliedOn == message)
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
            ownerList.ReadMessage(targetUser._id);
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
                        RenderStartOfDialog();

                        return;
                    }

                    RenderMessages(messages);

                    if (messages.Count() < LoadingMessagesCount)
                        RenderStartOfDialog();
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

        private void RenderStartOfDialog()
        {
            if (targetUser is null || currentChat is null)
                return;

            if (firstMessageSentAt is not null)
            {
                var lastDateCard = new DateCard(firstMessageSentAt.Value);
                lastDateCard.SetSlideFromBottomOnLoad();
                MessagesStackPanel.Children.Insert(0, lastDateCard);
            }

            var dialogCard = new DialogCard(targetUser);
            dialogCard.HorizontalAlignment = HorizontalAlignment.Left;
            dialogCard.Margin = new Thickness(5);
            dialogCard.SetSlideFromLeftOnLoad();
            MessagesStackPanel.Children.Insert(0, dialogCard);

            var chatCreatedAtDateCard = new DateCard(currentChat.createdAtLocal);
            chatCreatedAtDateCard.SetSlideFromBottomOnLoad();
            MessagesStackPanel.Children.Insert(0, chatCreatedAtDateCard);

            isAllMessagesLoaded = true;
        }

        private void RenderMessages(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                if (lastMessageSentAt == null)
                {
                    LastSentMessage = message;
                    lastMessageSentAt = message.sentAtLocal;
                }

                message.chat = currentChat;

                if (message.repliedOn != null)
                    message.repliedOn.chat = currentChat;

                MessageBorder messageBorder = new MessageBorder(message.content, message.sender == currentUser);
                
                messageBorder.SetLoaded(message);

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

                if (message.edits != null && message.edits.Count > 0)
                    messageBorder.SetEdited();

                this.messages.Add(message._id, messageBorder);

                if (message.sender != currentUser && !message.haveSeen)
                    unreadMessages.Add(message._id, messageBorder);

                if (firstMessageSentAt != null)
                {
                    if ((firstMessageSentAt?.Year == message.sentAtLocal.Year && firstMessageSentAt?.DayOfYear > message.sentAtLocal.DayOfYear) || firstMessageSentAt?.Year > message.sentAtLocal.Year)
                    {
                        var dateCard = new DateCard(firstMessageSentAt.Value);
                        dateCard.SetSlideFromBottomOnLoad();
                        MessagesStackPanel.Children.Insert(0, dateCard);
                    }
                    else if (firstMessageSentAt.Value.IsOlderOnTime(message.sentAtLocal, TimeSpan.FromHours(1)))
                    {
                        messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top, messageBorder.Margin.Right, messageBorder.Margin.Bottom + 10);
                    }
                    else if (firstMessageSentAt.Value.IsOlderOnTime(message.sentAtLocal, TimeSpan.FromMinutes(10)))
                    {
                        messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top, messageBorder.Margin.Right, messageBorder.Margin.Bottom + 5);
                    }
                    else if (MessagesStackPanel.Children.Count > 0
                          && MessagesStackPanel.Children[0] is MessageBorder nextMessageBorder)
                    {
                        if ((nextMessageBorder.IsOwn && message.sender == currentUser) || (!nextMessageBorder.IsOwn && message.sender == targetUser))
                        {
                            nextMessageBorder.RoundInTop();
                            messageBorder.RoundInBottom();
                        }
                    }
                }

                messageBorder.SetSlideFromBottomOnLoad();
                MessagesStackPanel.Children.Insert(0, messageBorder);
                firstMessageSentAt = message.sentAtLocal;
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

        private MessageBorder CreateOwnMessageBorder(String content, DateTime sentAt)
        {
            var messageBorder = new MessageBorder(content, true);

            if (lastMessageSentAt == null || (lastMessageSentAt.Value.Year == sentAt.Year && lastMessageSentAt.Value.DayOfYear < sentAt.DayOfYear) || lastMessageSentAt.Value.Year < sentAt.Year)
            {
                var dateCard = new DateCard(sentAt);
                dateCard.SetSlideFromBottomOnLoad();
                MessagesStackPanel.Children.Add(dateCard);
            }
            else if (lastMessageSentAt.Value.AddMinutes(10) < sentAt)
            {
                messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top + 5, messageBorder.Margin.Right, messageBorder.Margin.Bottom);
            }
            else if (lastMessageSentAt.Value.AddHours(1) < sentAt)
            {
                messageBorder.Margin = new Thickness(messageBorder.Margin.Left, messageBorder.Margin.Top + 10, messageBorder.Margin.Right, messageBorder.Margin.Bottom);
            }
            else if (MessagesStackPanel.Children.Count > 0
                     && MessagesStackPanel.Children[MessagesStackPanel.Children.Count - 1] is MessageBorder previousMessageBorder)
            {
                if (previousMessageBorder.IsOwn)
                {
                    previousMessageBorder.RoundInBottom();
                    messageBorder.RoundInTop();
                }
            }

            lastMessageSentAt = sentAt;

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

            if (!IsSendingDelayPassed())
                return;

            var sentAt = DateTime.Now;
            var messageId = Guid.NewGuid().ToString();
            var content = MessageTextBox.Text.Trim();
            
            _ = Task.Run(() => Messages.SendMessage(messageId, currentChat._id, content));

            MessageTextBox.Text = String.Empty;

            var messageBorder = CreateOwnMessageBorder(content, sentAt);

            messages.Add(messageId, messageBorder);

            MessagesStackPanel.Children.Add(messageBorder);
            MessagesScroll.ScrollToBottom();
        }

        private void ReplyOnMessage()
        {
            if (String.IsNullOrWhiteSpace(MessageTextBox.Text) || replyingOnMessage == null)
                return;

            if (!IsSendingDelayPassed())
                return;

            var sentAt = DateTime.Now;
            var messageId = Guid.NewGuid().ToString();
            var content = MessageTextBox.Text.Trim();

            _ = Task.Run(() => Messages.ReplyMessage(messageId, currentChat._id, content, replyingOnMessage._id));

            MessageTextBox.Text = String.Empty;

            var messageBorder = CreateOwnMessageBorder(content, sentAt);

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

            if (content != editingMessage.content)
            {
                _ = Task.Run(() => Messages.EditMessage(editingMessage._id, content));

                if (messages.TryGetValue(editingMessage._id, out var editedMessageBorder))
                {
                    editingMessage.edits.Add(new Edit() { content = editingMessage.content, date = editingMessage.editedAtLocal });
                    editingMessage.content = content;
                    editingMessage.editedAt = DateTime.Now;
                    editedMessageBorder.SetEdited(editingMessage);

                    foreach (var messageBorder in messages.Values)
                        if (messageBorder.Message?.repliedOn != null && messageBorder.Message?.repliedOn == editingMessage)
                            messageBorder.SetReplyEdited(editingMessage.content);

                    owner.EditMessage(editingMessage);

                    editedMessageBorder.BringIntoView(new Rect(0, -50, editedMessageBorder.ActualWidth, editedMessageBorder.ActualHeight + 50));
                }
            }    

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
                _ = LoadOlderMessages();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (TypingIndicator != null)
                TypingIndicatorPlaceholder.RemoveChild(TypingIndicator);

            TypingIndicator = new TypingIndicator()
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

        private bool IsSendingDelayPassed()
        {
            if (lastSendingAt != null && !DateTime.Now.IsOlderOnTime(lastSendingAt.Value, TimeSpan.FromMilliseconds(200)))
                return false;

            lastSendingAt = DateTime.Now;
            return true;
        }

        public void EnterReplying(Message message)
        {
            LeaveEditing();

            uiSync.Send(s =>
            {
                replyingOnMessage = message;
                ReplyPanel.Visibility = Visibility.Visible;

                ReplyToNickname.Text = message.sender.username;
                ReplyToText.Text = message.content;

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
            LeaveReplying();

            uiSync.Send(s =>
            {
                editingMessage = message;
                EditPanel.Visibility = Visibility.Visible;

                EditText.Text = message.content;

                MessageTextBox.Text = message.content;
                MessageTextBox.Focus();
                MessageTextBox.CaretIndex = MessageTextBox.Text.Length;

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

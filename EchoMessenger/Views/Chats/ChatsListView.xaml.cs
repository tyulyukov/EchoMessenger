using EchoMessenger.Models;
using EchoMessenger.Helpers;
using EchoMessenger.Helpers.Server;
using EchoMessenger.UI;
using EchoMessenger.UI.Controls.Cards.Chats;
using EchoMessenger.UI.Controls.Cards.Search;
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

namespace EchoMessenger.Views.Chats
{
    public partial class ChatsListView : UserControl
    {
        public readonly SelectChatView SelectChatView;
        public readonly Dictionary<String, MessagesView> MessagesViews;
        public readonly Dictionary<String, KeyValuePair<Chat, UserCard>> OpenedChats;
        public readonly List<String> OnlineChats;

        private bool chatsLoaded;
        private bool isSearching;

        private MessengerWindow owner;
        private SynchronizationContext uiSync;

        private UserInfo currentUser;
        private Chat? openedChat;

        private UserCard? selectedUserCard;
        private Border? selectedButton;
        private SelectionLine selectionLine;
        private TimeSpan selectionDuration = TimeSpan.FromMilliseconds(250);

        private UserCard? firstUserCard;

        private object locker = new object();
        private TypeAssistant typeAssistant;

        public ChatsListView(MessengerWindow owner, UserInfo user)
        {
            InitializeComponent();

            uiSync = SynchronizationContext.Current;
            this.owner = owner;

            currentUser = user;

            chatsLoaded = false;
            isSearching = false;

            OpenedChats = new Dictionary<string, KeyValuePair<Chat, UserCard>>();
            OnlineChats = new List<String>();
            MessagesViews = new Dictionary<string, MessagesView>();
            SelectChatView = new SelectChatView();

            selectedButton = null;
            selectionLine = new SelectionLine();

            typeAssistant = new TypeAssistant(350);
            typeAssistant.Idled += TypeAssistant_Idled;

            _ = LoadChats();
        }

        public void UpdateUser(UserInfo user)
        {
            if (user == null)
                return;

            currentUser = user;

            foreach (var messageView in MessagesViews)
                messageView.Value.UpdateUser(user);
        }

        public void ShowLoading(bool visible)
        {
            LoadingSpinner.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public async Task OpenChatByUserId(String userId)
        {
            Chat? chat = null;

            if (OpenedChats.ContainsKey(userId))
            {
                chat = OpenedChats[userId].Key;
            }
            else
            {
                try
                {
                    uiSync.Post((s) => { ShowLoading(true); }, null);
                    var chatResponse = await Database.CreateChat(userId);

                    if (chatResponse == null)
                        return;

                    if (chatResponse.StatusCode == (HttpStatusCode)201)
                    {
                        if (chatResponse.Content == null)
                            return;

                        var result = JObject.Parse(chatResponse.Content);

                        chat = result?.ToObject<Chat>();

                        if (chat == null)
                            return;

                        var targetUser = chat.sender == currentUser ? chat.receiver : chat.sender;
                        var icon = AddUserCard(targetUser, chat, chat.messages.FirstOrDefault(), OnlineChats.Contains(targetUser._id), true);

                        LoadChat(userId, chat, icon, OnlineChats.Contains(targetUser._id));
                    }
                    else if (chatResponse.StatusCode == (HttpStatusCode)401)
                    {
                        RegistryManager.ForgetJwt();
                        owner.Hide();
                        new LoginWindow().Show();
                        owner.Close();
                        return;
                    }
                }
                finally
                {
                    uiSync.Post((s) => { ShowLoading(false); }, null);
                }
            }

            if (chat == null)
                return;

            OpenChat(chat);
        }

        public void OpenChat(Chat chat)
        {
            openedChat = chat;

            if (owner.OpenTab(MessagesViews[openedChat._id]))
            {
                MessagesViews[openedChat._id].Open();

                var chatByUserId = OpenedChats.FirstOrDefault(o => o.Value.Key == openedChat);
                var card = chatByUserId.Value.Value;

                uiSync.Send((s) =>
                {
                    if (selectedUserCard != null)
                        selectedUserCard.Deselect();

                    card.Select();
                    selectedUserCard = card;
                }, null);
            }
        }

        public void ReadMessage(String userId)
        {
            uiSync.Post((s) =>
            {
                OpenedChats[userId].Value.NotificationBadge.RemoveNotification();
            }, null);
        }

        private async Task LoadChats()
        {
            SearchTextBox.IsEnabled = false;
            chatsLoaded = false;
            ButtonRefresh.Visibility = Visibility.Collapsed;
            ShowLoading(true);

            try
            {
                MessagesViews.Clear();
                OpenedChats.Clear();
                ChatsMenu.Children.Clear();
                var response = await Database.GetLastChats();

                if (response == null)
                {
                    throw new Exception();
                }
                else if (response.StatusCode == (HttpStatusCode)200)
                {
                    if (String.IsNullOrEmpty(response.Content))
                        throw new Exception();

                    var result = JArray.Parse(response.Content);

                    var chats = result?.ToObject<IEnumerable<Chat>>();

                    if (chats == null)
                        throw new Exception();

                    RenderChats(chats);
                    chatsLoaded = true;
                    SearchTextBox.IsEnabled = true;
                }
                else if (response.StatusCode == (HttpStatusCode)401)
                {
                    RegistryManager.ForgetJwt();
                    owner.Hide();
                    new LoginWindow().Show();
                    owner.Close();
                }
                else throw new Exception();
            }
            catch
            {
                ButtonRefresh.Visibility = Visibility.Visible;
            }
            finally
            {
                ShowLoading(false);
            }
        }

        public void RenderChats(IEnumerable<Chat> chats)
        {
            uiSync.Send((s) => {
                ChatsMenu.Children.Clear();
                OpenedChats.Clear();

                foreach (var chat in chats)
                {
                    Message? lastMessage = null;

                    if (MessagesViews.ContainsKey(chat._id))
                        lastMessage = MessagesViews[chat._id].LastSentMessage;
                    else if (chat.messages != null && chat.messages.Count > 0)
                        lastMessage = chat.messages.OrderBy(m => m.sentAtLocal).Last();

                    chat.messages = new List<Message>();

                    if (lastMessage != null)
                        chat.messages.Add(lastMessage);
                }
                
                chats = chats.OrderBy(c => c.GetLastSentAt());
                foreach (var chat in chats)
                {
                    var targetUser = chat.sender == currentUser ? chat.receiver : chat.sender;

                    bool selected = openedChat == chat;
                    var card = AddUserCard(targetUser, chat, chat.messages.FirstOrDefault(), OnlineChats.Contains(targetUser._id), selected);

                    LoadChat(targetUser._id, chat, card, OnlineChats.Contains(targetUser._id));

                    if (chat == chats.Last())
                        firstUserCard = card;
                }

                selectedUserCard?.BringIntoView();
            }, null);
        }

        public UserCard AddUserCard(UserInfo targetUser, Chat chat, Message? lastMessage, bool isOnline, bool select = false)
        {
            UserCard? card = null;

            uiSync.Send((s) => {
                int unreadMessagesCount = chat.unreadMessagesCount;

                if (MessagesViews.ContainsKey(chat._id))
                    unreadMessagesCount = MessagesViews[chat._id].UnreadMessagesCount;

                card = new UserCard(targetUser, chat, lastMessage, isOnline, unreadMessagesCount);

                if (select)
                {
                    if (selectedUserCard != null)
                        selectedUserCard.Deselect();

                    card.Select();
                    selectedUserCard = card;
                }

                card.MouseLeftButtonUp += (sender, e) =>
                {
                    _ = Task.Run(() => { OpenChat(chat); });
                };

                card.SetSlideFromLeftOnLoad();
                ChatsMenu.Children.Insert(0, card);
            }, null);

            return card;
        }

        public void LoadChat(String userId, Chat chat, UserCard icon, bool isOnline)
        {
            uiSync.Send((s) =>
            {
                if (!MessagesViews.ContainsKey(chat._id))
                    MessagesViews.Add(chat._id, new MessagesView(owner, this, currentUser, chat, isOnline));

                if (!OpenedChats.ContainsKey(userId))
                    OpenedChats.Add(userId, new KeyValuePair<Chat, UserCard>(chat, icon));
            }, null);
        }

        public void PushUserIcon(UserCard icon)
        {
            if (firstUserCard != null && firstUserCard == icon)
                return;

            uiSync.Post((s) => {
                icon.Parent.RemoveChild(icon);
                ChatsMenu.Children.Remove((UIElement)icon.Parent);
                ChatsMenu.Children.Insert(0, icon);
                firstUserCard = icon;
                icon.ChangeVisibility(true, selectionDuration);
            }, null);
        }

        public void SetOnlineStatus(String userId, bool isOnline)
        {
            if (!OnlineChats.Contains(userId) && isOnline)
                OnlineChats.Add(userId);
            else if (OnlineChats.Contains(userId) && !isOnline)
                OnlineChats.Remove(userId);

            if (OpenedChats.TryGetValue(userId, out var chat))
            {
                uiSync.Send((s) =>
                {
                    if (isOnline)
                        chat.Value.AvatarIcon.OnlineStatusIcon.SetOnline();
                    else
                        chat.Value.AvatarIcon.OnlineStatusIcon.SetOffline();

                    if (MessagesViews.TryGetValue(chat.Key._id, out var view))
                        view.SetOnlineStatus(isOnline, DateTime.Now);
                }, null);
            }
        }

        private void TypeAssistant_Idled(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(async () =>
            {
                var query = SearchTextBox.Text;

                if (String.IsNullOrWhiteSpace(query))
                {
                    ShowLoading(false);

                    if (isSearching)
                    {
                        foreach (Border control in ChatsMenu.Children)
                            control.ChangeVisibility(false);

                        await Task.Delay(150).ContinueWith((task) =>
                        {
                            uiSync?.Send(state =>
                            {
                                ChatsMenu.Children.Clear();

                                if (!chatsLoaded)
                                    _ = LoadChats();
                                else
                                    RenderChats(OpenedChats.Values.Select(c => c.Key).ToList());
                            }, null);
                        });

                        isSearching = false;
                    }
                    
                    return;
                }

                isSearching = true;
                ChatsMenu.Children.Clear();
                ShowLoading(true);

                try
                {
                    var response = await Database.SearchUsers(query.Trim().ToLower());

                    if (response == null)
                        return;

                    if (response.StatusCode == (HttpStatusCode)200)
                    {
                        if (response.Content == null)
                            return;

                        var result = JArray.Parse(response.Content);

                        var users = result?.ToObject<IEnumerable<UserInfo>>();

                        if (users == null)
                            return;

                        lock (locker)
                        {
                            foreach (var user in users)
                            {
                                var userCard = new SearchResultUserCard(user.avatarUrl, user.username);

                                userCard.MouseLeftButtonUp += (s, e) =>
                                {
                                    _ = Task.Run(async () =>
                                    {
                                        uiSync?.Send(state => 
                                        {
                                            SearchTextBox.Text = String.Empty;
                                        }, null);
                                        
                                        await OpenChatByUserId(user._id);
                                    });
                                };

                                if (user == users.First())
                                    userCard.SetFirst();

                                if (user == users.Last())
                                    userCard.SetLast();

                                userCard.SetSlideFromLeftOnLoad();
                                ChatsMenu.Children.Add(userCard);
                            }
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
            });
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            typeAssistant.TextChanged();
        }

        private void ChatClosedControlLoaded(object sender, RoutedEventArgs e)
        {
            if (openedChat != null)
            { 
                ChatOpenedControlLoaded(sender, e);
                Loaded += ChatOpenedControlLoaded;
                Loaded -= ChatClosedControlLoaded;
                return;
            }

            owner.OpenTab(SelectChatView);
        }

        private void ChatOpenedControlLoaded(object sender, RoutedEventArgs e)
        {
            if (openedChat != null)
                OpenChat(openedChat);
        }

        private void ButtonRefresh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _ = LoadChats();
        }
    }
}

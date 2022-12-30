using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace WallpaperCore.Services.Messenger;

public interface IMessengerService
{
    void Register<TRecipient, TMessage>(TRecipient recipient, MessageHandler<TRecipient, TMessage> action)
        where TRecipient : ObservableRecipient where TMessage : class;

    void Send<TMessage>(TMessage message) where TMessage : class;
}

public class MessengerService : ObservableRecipient, IMessengerService
{
    public void Register<TRecipient, TMessage>(TRecipient recipient, MessageHandler<TRecipient, TMessage> action)
        where TRecipient : ObservableRecipient where TMessage : class
    {
        Messenger.Register(recipient, action);
    }

    public void Send<TMessage>(TMessage message) where TMessage : class
    {
        Messenger.Send(message);
    }
}
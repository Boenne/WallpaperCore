using System;
using System.Threading.Tasks;
using WallpaperCore.Services.Messenger;
using WallpaperCore.Services.Messenger.Messages;
using WallpaperCore.Windows;

namespace WallpaperCore.Services;

public interface IConfirmationService
{
    void Prompt(string title, string text, Action confirmationAction);
}

public class ConfirmationService : IConfirmationService
{
    private readonly IMessengerService _messenger;

    public ConfirmationService(IMessengerService messenger)
    {
        _messenger = messenger;
    }

    public void Prompt(string title, string text, Action confirmationAction)
    {
        //Come up with a better version?
        Task.Run(async () =>
        {
            await Task.Delay(50);
            _messenger.Send(new UserPromptedMessage(title, text, confirmationAction));
        });
        new ConfirmationWindow().ShowDialog();
    }
}
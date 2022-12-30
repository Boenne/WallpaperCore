using System;
using System.Threading.Tasks;
using WallpaperCore.Windows;
using WallpaperCore.Wrappers.Messenger;

namespace WallpaperCore.Services;

public interface IConfirmationService
{
    void Prompt(string title, string text, Action confirmationAction);
}

public class ConfirmationService : IConfirmationService
{
    private readonly IMessengerWrapper _messenger;

    public ConfirmationService(IMessengerWrapper messenger)
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
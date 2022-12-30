using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WallpaperCore.Windows.Interfaces;
using WallpaperCore.Wrappers.Messenger;

namespace WallpaperCore.ViewModels;

public interface IConfirmationViewModel
{
    string Title { get; set; }
    string Text { get; set; }
    IRelayCommand ConfirmCommand { get; }
    IRelayCommand DeclineCommand { get; }
    IRelayCommand<IClosable> LoadedCommand { get; }
}

public class ConfirmationViewModel : ObservableRecipient, IConfirmationViewModel
{
    private readonly IMessengerWrapper _messenger;
    private Action _confirmationAction;
    private string _text;
    private string _title;
    private IClosable _window;

    public ConfirmationViewModel(IMessengerWrapper messenger)
    {
        _messenger = messenger;
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public IRelayCommand ConfirmCommand => new RelayCommand(Confirm);
    public IRelayCommand DeclineCommand => new RelayCommand(Decline);
    public IRelayCommand<IClosable> LoadedCommand => new RelayCommand<IClosable>(Loaded);

    public void Loaded(IClosable closable)
    {
        _window = closable;
        _messenger.Register<ConfirmationViewModel, UserPromptedMessage>(this, (vm, message) =>
        {
            Title = message.Title;
            Text = message.Text;
            _confirmationAction = message.ConfirmationAction;
        });
    }

    public void Confirm()
    {
        _confirmationAction();
        _window.Close();
    }

    public void Decline()
    {
        _window.Close();
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WallpaperCore.Properties;
using WallpaperCore.Services.Messenger;
using WallpaperCore.Services.Messenger.Messages;

namespace WallpaperCore.ViewModels;

public interface ISettingsViewModel
{
    public string BasePath { get; set; }
    public string TempFolderName { get; set; }
    public int Interval { get; set; }
    public IRelayCommand LoadedCommand { get; }
    public IRelayCommand SaveCommand { get; }
}

public class SettingsViewModel : ObservableObject, ISettingsViewModel
{
    private readonly IMessengerService _messenger;
    private string _basePath;
    private int _interval;
    private string _tempFolderName;

    public SettingsViewModel(IMessengerService messenger)
    {
        _messenger = messenger;
    }

    public string BasePath
    {
        get => _basePath;
        set => SetProperty(ref _basePath, value);
    }

    public string TempFolderName
    {
        get => _tempFolderName;
        set => SetProperty(ref _tempFolderName, value);
    }

    public int Interval
    {
        get => _interval;
        set => SetProperty(ref _interval, value);
    }

    public IRelayCommand LoadedCommand => new RelayCommand(Loaded);
    public IRelayCommand SaveCommand => new RelayCommand(Save);

    public void Loaded()
    {
        BasePath = Settings.Default.BasePath;
        TempFolderName = Settings.Default.TempFolderName;
        Interval = Settings.Default.Interval;
    }

    public void Save()
    {
        Settings.Default.BasePath = BasePath;
        Settings.Default.Interval = Interval;
        Settings.Default.TempFolderName = TempFolderName;
        Settings.Default.Save();
        _messenger.Send(new SettingsSavedMessage());
    }
}
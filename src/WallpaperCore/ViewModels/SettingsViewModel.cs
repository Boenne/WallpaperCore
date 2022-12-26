using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WallpaperCore.ViewModels;

public interface ISettingsViewModel
{
    public string BasePath { get; set; }
    public IRelayCommand LoadedCommand { get; }
    public IRelayCommand SaveCommand { get; }
}

public class SettingsViewModel : ObservableObject, ISettingsViewModel
{
    private string _basePath;

    public string BasePath
    {
        get => _basePath;
        set => SetProperty(ref _basePath, value);
    }

    public IRelayCommand LoadedCommand => new RelayCommand(Loaded);
    public IRelayCommand SaveCommand => new RelayCommand(Save);

    public void Loaded()
    {
        BasePath = Properties.Settings.Default.BasePath;
    }

    public void Save()
    {
        Properties.Settings.Default.BasePath = BasePath;
        Properties.Settings.Default.Save();
    }
}
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WallpaperCore.Services;
using WallpaperCore.Services.Messenger;
using WallpaperCore.Services.Messenger.Messages;

namespace WallpaperCore.ViewModels;

public interface IMainViewModel
{
    public IRelayCommand StartupCommand { get; }
    public IRelayCommand ClosedCommand { get; }
    public IRelayCommand SettingsCommand { get; }
    public IRelayCommand StartCommand { get; }
    public IRelayCommand PauseCommand { get; }
    public IRelayCommand PreviousCommand { get; }
    public IRelayCommand NextCommand { get; }
    public IRelayCommand RestartCommand { get; }
    public bool IncludeSubfolders { get; set; }
    public bool IsPaused { get; set; }
    public bool IsRunning { get; set; }
    public bool ShowSettings { get; set; }
    public double Progress { get; set; }
    public string RunningDirectoryPath { get; set; }
    public BitmapImage PreviewImage { get; set; }
}

public class MainViewModel : ObservableRecipient, IMainViewModel
{
    private readonly IBackgroundWorker _backgroundWorker;
    private readonly IDispatcherService _dispatcher;
    private readonly IMessengerService _messenger;
    private bool _includeSubfolders;
    private bool _isPaused;
    private bool _isRunning;
    private BitmapImage _previewImage;
    private double _progress;
    private string _runningDirectoryPath;
    private bool _showSettings;

    public MainViewModel(IDispatcherService dispatcher, IMessengerService messenger,
        IBackgroundWorker backgroundWorker)
    {
        _dispatcher = dispatcher;
        _messenger = messenger;
        _backgroundWorker = backgroundWorker;
    }

    public IRelayCommand ClosedCommand => new RelayCommand(Close);
    public IRelayCommand StartupCommand => new RelayCommand(StartUp);
    public IRelayCommand SettingsCommand => new RelayCommand(Settings);
    public IRelayCommand StartCommand => new RelayCommand(Start);
    public IRelayCommand PauseCommand => new RelayCommand(Pause);
    public IRelayCommand PreviousCommand => new RelayCommand(Previous);
    public IRelayCommand NextCommand => new RelayCommand(Next);
    public IRelayCommand RestartCommand => new RelayCommand(Start);

    public bool IncludeSubfolders
    {
        get => _includeSubfolders;
        set => SetProperty(ref _includeSubfolders, value);
    }

    public bool IsPaused
    {
        get => _isPaused;
        set => SetProperty(ref _isPaused, value);
    }

    public bool IsRunning
    {
        get => _isRunning;
        set => SetProperty(ref _isRunning, value);
    }

    public bool ShowSettings
    {
        get => _showSettings;
        set => SetProperty(ref _showSettings, value);
    }

    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    public string RunningDirectoryPath
    {
        get => _runningDirectoryPath;
        set => SetProperty(ref _runningDirectoryPath, value);
    }

    public BitmapImage PreviewImage
    {
        get => _previewImage;
        set => SetProperty(ref _previewImage, value);
    }

    public void Pause()
    {
        IsPaused = !IsPaused;
        _backgroundWorker.TogglePause();
    }

    public void Next()
    {
        _backgroundWorker.Next();
    }

    public void Previous()
    {
        _backgroundWorker.Previous();
    }

    public void StartUp()
    {
        _messenger.Register<MainViewModel, BookmarkSelectedMessage>(this, (vm, message) =>
        {
            RunningDirectoryPath = message.Path;
            Start();
        });
        _messenger.Register<MainViewModel, SettingsSavedMessage>(this, (vm, message) => { ShowSettings = false; });
        _backgroundWorker.Initialize(Properties.Settings.Default.TempFolderName, Properties.Settings.Default.Interval);
    }

    public void Close()
    {
        _backgroundWorker.Dispose();
    }

    public void Settings()
    {
        ShowSettings = true;
    }

    public void Start()
    {
        IsRunning = true;
        _backgroundWorker.Begin(RunningDirectoryPath, IncludeSubfolders, SetPreviewImage, IncrementProgress);
    }

    public void SetPreviewImage(BitmapImage bitmapImage)
    {
        try
        {
            _dispatcher.DoUIWork(() => PreviewImage = bitmapImage);
        }
        catch
        {
            //Sometimes there's a problem with the meta data of the image
            //so just fail silently
        }
    }

    public void IncrementProgress(double value)
    {
        _dispatcher.DoUIWork(() => Progress = value);
    }
}
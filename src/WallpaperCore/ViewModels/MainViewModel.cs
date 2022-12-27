using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WallpaperCore.Extensions;
using WallpaperCore.Services;
using WallpaperCore.Wrappers;
using WallpaperCore.Wrappers.Messenger;

namespace WallpaperCore.ViewModels;

public interface IMainViewModel
{
    public IRelayCommand StartupCommand { get; }
    public IRelayCommand ClosedCommand { get; }
    public IRelayCommand SettingsCommand { get; }
    public IRelayCommand StartCommand { get; }
    public IRelayCommand PauseCommand { get; }
    public IRelayCommand RestartCommand { get; }
    public bool IncludeSubfolders { get; set; }
    public bool IsPaused { get; set; }
    public bool IsRunning { get; set; }
    public bool ShowSettings { get; set; }
    public string RunningDirectoryPath { get; set; }
    public BitmapImage PreviewImage { get; set; }
}

public class MainViewModel : ObservableRecipient, IMainViewModel
{
    private const string TempFolderName = "wallpaper_temps";
    private readonly IDispatcherWrapper _dispatcherWrapper;
    private readonly IImageService _imageService;
    private readonly IMessengerWrapper _messenger;

    private readonly IWallpaperService _wallpaperService;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _imagePreviewTask, _backgroundTask;
    private List<FileInfo> _images;
    private bool _includeSubfolders;
    private bool _isPaused;
    private bool _isRunning;
    private DirectoryInfo? _mainDirectory, _tempDirectoryInfo;
    private BitmapImage _previewImage;
    private string _runningDirectoryPath;
    private bool _showSettings;

    public MainViewModel(IWallpaperService wallpaperService, IDispatcherWrapper dispatcherWrapper,
        IImageService imageService, IMessengerWrapper messenger)
    {
        _wallpaperService = wallpaperService;
        _dispatcherWrapper = dispatcherWrapper;
        _imageService = imageService;
        _messenger = messenger;
    }

    public IRelayCommand ClosedCommand => new RelayCommand(Close);
    public IRelayCommand StartupCommand => new RelayCommand(StartUp);
    public IRelayCommand SettingsCommand => new RelayCommand(Settings);
    public IRelayCommand StartCommand => new RelayCommand(Start);
    public IRelayCommand PauseCommand => new RelayCommand(Pause);
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
    }

    public void StartUp()
    {
        _messenger.Register<MainViewModel, BookmarkSelectedMessage>(this, (vm, message) =>
        {
            RunningDirectoryPath = message.Path;
            Start();
        });
    }

    public void Close()
    {
        CancelCurrentTask();
        DeleteTempFolder();
    }

    public void Settings()
    {
        ShowSettings = true;
    }

    public void Start()
    {
        IsRunning = true;
        CancelCurrentTask();
        SetDirectory();
        _cancellationTokenSource = new CancellationTokenSource();

        _backgroundTask = Task.Run(async () =>
        {
            if (!_mainDirectory!.Exists) return;
            _images = _mainDirectory.GetImageFiles();
            if (IncludeSubfolders)
                GetFilesInSubFolders(_mainDirectory, _images);

            if (!_images.Any()) return;

            _images.Shuffle();
            var files = _images.Select(x => x.FullName).ToList();
            var i = 1;
            var filePath = _imageService.GetImagePath(files[0], _tempDirectoryInfo!.FullName);

            while (true)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                    break;
                if (!IsPaused)
                {
                    if (i >= files.Count) i = 0;
                    _wallpaperService.Set(filePath);

                    filePath = _imageService.GetImagePath(files[i], _tempDirectoryInfo!.FullName);
                    SetPreviewImage(filePath);
                    i++;
                }

                await WaitSeconds(10);
            }
        }, _cancellationTokenSource.Token);
    }

    public void SetPreviewImage(string filePath)
    {
        try
        {
            _imagePreviewTask =
                _dispatcherWrapper.BeginInvoke(() => PreviewImage = _imageService.CreatePreviewImage(filePath));
        }
        catch
        {
            //Sometimes there's a problem with the meta data of the image
            //so just fail silently
        }
    }

    private async Task WaitSeconds(int seconds)
    {
        double secondsCount = 0;
        while (secondsCount < seconds)
        {
            if (_cancellationTokenSource!.IsCancellationRequested)
                break;
            secondsCount += 0.5;
            await Task.Delay(500);
        }
    }

    private void SetDirectory()
    {
        _mainDirectory = Directory.Exists(RunningDirectoryPath)
            ? new DirectoryInfo(RunningDirectoryPath)
            : new DirectoryInfo($"{Properties.Settings.Default.BasePath}/{(string?)RunningDirectoryPath}");

        if (FolderHasChanged()) DeleteTempFolder();

        _tempDirectoryInfo = new DirectoryInfo($"{_mainDirectory.FullName}/{TempFolderName}");

        if (_tempDirectoryInfo.Exists) return;
        _tempDirectoryInfo.Create();
    }

    public bool FolderHasChanged()
    {
        return _tempDirectoryInfo != null &&
               _tempDirectoryInfo.Parent?.Parent?.FullName != _mainDirectory?.Parent?.FullName;
    }

    private void CancelCurrentTask()
    {
        if (_cancellationTokenSource == null) return;

        _cancellationTokenSource.Cancel();
        try
        {
            _imagePreviewTask?.Wait();
            _backgroundTask?.Wait();
        }
        finally
        {
            _imageService.ClearBitmapStream();
        }
    }

    private void GetFilesInSubFolders(DirectoryInfo dir, List<FileInfo> result)
    {
        var subDirectories = dir.GetDirectories().Where(x => x.Name != TempFolderName).ToList();
        if (!subDirectories.Any()) return;
        foreach (var subDirectory in subDirectories)
        {
            result.AddRange(subDirectory.GetImageFiles());
            GetFilesInSubFolders(subDirectory, result);
        }
    }

    private void DeleteTempFolder()
    {
        if (_tempDirectoryInfo != null && Directory.Exists(_tempDirectoryInfo.FullName))
            Directory.Delete(_tempDirectoryInfo.FullName, true);
    }
}
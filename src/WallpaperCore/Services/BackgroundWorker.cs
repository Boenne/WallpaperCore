using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WallpaperCore.Extensions;
using WallpaperCore.Properties;

namespace WallpaperCore.Services;

public interface IBackgroundWorker
{
    void Begin(string runningDirectoryPath, bool includeSubfolders, Action<string> setPreviewImageAction,
        Action<double> incrementProgressAction);

    void TogglePause();
    void Next();
    void Previous();
    void Dispose();
}

public class BackgroundWorker : IBackgroundWorker, IDisposable
{
    private const string TempFolderName = "wallpaper_temps";
    private readonly IImageService _imageService;
    private readonly IWallpaperService _wallpaperService;
    private Task? _backgroundTask;
    private List<FileInfo> _images;
    private int _index;
    private bool _isPaused;
    private CancellationTokenSource? _mainCts, _timerCts;
    private DirectoryInfo? _mainDirectory, _tempDirectoryInfo;
    private string _nextWallpaperPath;

    public BackgroundWorker(IImageService imageService, IWallpaperService wallpaperService)
    {
        _imageService = imageService;
        _wallpaperService = wallpaperService;
    }

    public void Begin(string runningDirectoryPath, bool includeSubfolders, Action<string> setPreviewImageAction,
        Action<double> incrementProgressAction)
    {
        Cancel();
        SetDirectory(runningDirectoryPath);

        _mainCts = new CancellationTokenSource();
        _timerCts = new CancellationTokenSource();

        _backgroundTask = Task.Run(async () =>
        {
            if (!_mainDirectory!.Exists) return;
            _images = _mainDirectory.GetImageFiles();
            if (includeSubfolders)
                GetFilesInSubFolders(_mainDirectory, _images);

            if (!_images.Any()) return;

            _images.Shuffle();
            SetNextWallpaperPath();

            while (true)
            {
                if (_mainCts.Token.IsCancellationRequested)
                    break;
                if (!_isPaused)
                {
                    if (_index >= _images.Count) _index = 0;
                    _wallpaperService.Set(_nextWallpaperPath);

                    SetNextWallpaperPath();
                    setPreviewImageAction(_nextWallpaperPath);
                }

                await WaitSeconds(10, incrementProgressAction);
            }
        }, _mainCts.Token);
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
    }

    public void Next()
    {
        _timerCts?.Cancel();
    }

    public void Previous()
    {
        if (_images.Count < 3)
            _index = _index == 1 ? 0 : 1;
        else
            _index -= 3;
        if (_index < 0) _index = _images.Count + _index;
        SetNextWallpaperPath();
        _timerCts?.Cancel();
    }

    public void Dispose()
    {
        Cancel();
        DeleteTempFolder();
    }

    public void SetNextWallpaperPath()
    {
        _nextWallpaperPath = _imageService.GetImagePath(_images[_index].FullName, _tempDirectoryInfo!.FullName);
        _index++;
    }

    public void Cancel()
    {
        if (_mainCts == null || _timerCts == null) return;

        _mainCts.Cancel();
        _timerCts.Cancel();

        try
        {
            _backgroundTask?.Wait();
        }
        finally
        {
            _imageService.ClearBitmapStream();
        }
    }

    public async Task WaitSeconds(int seconds, Action<double> incrementProgressAction)
    {
        double secondsCount = 0;
        while (secondsCount < seconds)
        {
            if (_mainCts!.IsCancellationRequested || _timerCts!.IsCancellationRequested)
                break;
            secondsCount += 0.1;
            incrementProgressAction(100 / (double)seconds * secondsCount);
            await Task.Delay(100);
        }

        //If only timer is cancelled
        if (!_mainCts!.IsCancellationRequested && _timerCts!.IsCancellationRequested)
            _timerCts = new CancellationTokenSource();
    }

    public void GetFilesInSubFolders(DirectoryInfo dir, List<FileInfo> result)
    {
        var subDirectories = dir.GetDirectories().Where(x => x.Name != TempFolderName).ToList();
        if (!subDirectories.Any()) return;
        foreach (var subDirectory in subDirectories)
        {
            result.AddRange(subDirectory.GetImageFiles());
            GetFilesInSubFolders(subDirectory, result);
        }
    }

    public void SetDirectory(string runningDirectoryPath)
    {
        _mainDirectory = Directory.Exists(runningDirectoryPath)
            ? new DirectoryInfo(runningDirectoryPath)
            : new DirectoryInfo($"{Settings.Default.BasePath}/{runningDirectoryPath}");

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

    public void DeleteTempFolder()
    {
        if (_tempDirectoryInfo != null && Directory.Exists(_tempDirectoryInfo.FullName))
            Directory.Delete(_tempDirectoryInfo.FullName, true);
    }
}
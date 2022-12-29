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
    private bool _isPaused;
    private CancellationTokenSource? _mainCts, _timerCts;
    private DirectoryInfo? _mainDirectory, _tempDirectoryInfo;

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
            var images = _mainDirectory.GetImageFiles();
            if (includeSubfolders)
                GetFilesInSubFolders(_mainDirectory, images);

            if (!images.Any()) return;

            images.Shuffle();
            var files = images.Select(x => x.FullName).ToList();
            var i = 1;
            var filePath = _imageService.GetImagePath(files[0], _tempDirectoryInfo!.FullName);

            while (true)
            {
                if (_mainCts.Token.IsCancellationRequested)
                    break;
                if (!_isPaused)
                {
                    if (i >= files.Count) i = 0;
                    _wallpaperService.Set(filePath);

                    filePath = _imageService.GetImagePath(files[i], _tempDirectoryInfo!.FullName);
                    setPreviewImageAction(filePath);
                    i++;
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
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        Cancel();
        DeleteTempFolder();
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
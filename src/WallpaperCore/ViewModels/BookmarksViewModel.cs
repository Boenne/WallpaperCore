using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using WallpaperCore.Models;
using WallpaperCore.Properties;
using WallpaperCore.Wrappers;
using WallpaperCore.Wrappers.Messenger;

namespace WallpaperCore.ViewModels;

public interface IBookmarksViewModel
{
    public ObservableCollection<Bookmark> Bookmarks { get; set; }
    public string Title { get; set; }
    public string Path { get; set; }
    public IRelayCommand LoadedCommand { get; }
    public IRelayCommand AddCommand { get; }
    public IRelayCommand<Bookmark> RunCommand { get; }
    public IRelayCommand<Bookmark> DeleteCommand { get; }
}

public class BookmarksViewModel : ObservableObject, IBookmarksViewModel
{
    private readonly IMessageBoxWrapper _messageBoxWrapper;
    private readonly IMessengerWrapper _messenger;
    private ObservableCollection<Bookmark> _bookmarks = new();
    private string _path;
    private string _title;

    public BookmarksViewModel(IMessageBoxWrapper messageBoxWrapper, IMessengerWrapper messenger)
    {
        _messageBoxWrapper = messageBoxWrapper;
        _messenger = messenger;
    }

    public ObservableCollection<Bookmark> Bookmarks
    {
        get => _bookmarks;
        set => SetProperty(ref _bookmarks, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Path
    {
        get => _path;
        set => SetProperty(ref _path, value);
    }

    public IRelayCommand LoadedCommand => new RelayCommand(Loaded);
    public IRelayCommand AddCommand => new RelayCommand(Add);
    public IRelayCommand<Bookmark> RunCommand => new RelayCommand<Bookmark>(Run);
    public IRelayCommand<Bookmark> DeleteCommand => new RelayCommand<Bookmark>(Delete);

    public void Loaded()
    {
        var defaultBookmarks = Settings.Default.Bookmarks;
        if (string.IsNullOrWhiteSpace(defaultBookmarks)) return;
        var bookmarks = JsonConvert.DeserializeObject<List<Bookmark>>(defaultBookmarks);
        Bookmarks.Clear();
        bookmarks?.ForEach(_bookmarks.Add);
    }

    public void Add()
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Path)) return;
        if (Bookmarks.Any(x => x.Path == Path)) return;
        Bookmarks.Add(new Bookmark(Title, Path));
        Settings.Default.Bookmarks = JsonConvert.SerializeObject(Bookmarks, Formatting.None);
        Settings.Default.Save();
        Title = null;
        Path = null;
    }

    public void Run(Bookmark bookmark)
    {
        _messenger.Send(new BookmarkSelectedMessage(bookmark.Path));
    }

    public void Delete(Bookmark bookmark)
    {
        _messageBoxWrapper.Prompt("Delete bookmark", $"Are you sure you want to delete '{bookmark.Title}'",
            () => { RemoveBookmark(bookmark); });
    }

    public void RemoveBookmark(Bookmark bookmark)
    {
        Bookmarks.Remove(bookmark);
        Settings.Default.Bookmarks = JsonConvert.SerializeObject(Bookmarks, Formatting.None);
        Settings.Default.Save();
    }
}
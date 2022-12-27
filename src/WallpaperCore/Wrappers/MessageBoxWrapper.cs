using System;
using System.Windows;

namespace WallpaperCore.Wrappers;

public interface IMessageBoxWrapper
{
    void Prompt(string title, string text, Action action);
}

public class MessageBoxWrapper : IMessageBoxWrapper
{
    public void Prompt(string title, string text, Action action)
    {
        if (MessageBox.Show(text, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
            MessageBoxResult.Yes)
        {
            action();
        }
    }
}
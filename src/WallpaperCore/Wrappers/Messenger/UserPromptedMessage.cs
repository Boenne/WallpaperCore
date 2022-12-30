﻿using System;

namespace WallpaperCore.Wrappers.Messenger;

public class UserPromptedMessage
{
    public UserPromptedMessage(string title, string text, Action confirmationAction)
    {
        Title = title;
        Text = text;
        ConfirmationAction = confirmationAction;
    }

    public string Title { get; set; }
    public string Text { get; set; }
    public Action ConfirmationAction { get; set; }
}
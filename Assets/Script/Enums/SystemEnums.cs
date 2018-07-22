using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Enums
{
    public enum GameMode
    {
        Title,
        Loading,
        Ingame,
    }

    public enum InputMode
    {
        None = 0,
        Waiting = 1,
        OnAnimation = 2,
    }

    public enum MessageMode
    {
        None = 0,
        OnWaitTime = 1,
        OnWaitInput = 2,
    }

    /// <summary>
    /// ウィンドウの種類
    /// </summary>
    public enum WindowType
    {
        None = 0,
        MenuTop = 1,
        Item = 2,
        ItemCommand = 3,
    }
}

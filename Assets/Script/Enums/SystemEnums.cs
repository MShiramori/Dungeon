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
}

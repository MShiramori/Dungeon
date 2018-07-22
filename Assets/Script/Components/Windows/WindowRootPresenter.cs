using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Components
{
    public class WindowRootPresenter : MonoBehaviour
    {
        public ItemWindow ItemWindow;
        public ItemCommandWindow ItemCommandWindow;

        public Stack<Window> ActiveWindows = new Stack<Window>();

        private int inputWait = 0;

        public void Initialize()
        {

        }

        public void Update()
        {
            if (inputWait > 0) inputWait--;
        }

        public WindowType GetCurrentWindowType()
        {
            if (!ActiveWindows.Any())
                return WindowType.None;

            return ActiveWindows.Peek().Type;
        }

        public void OnMenuButtonClick()
        {
            if (inputWait > 0)
                return;

            var type = GetCurrentWindowType();
            switch (type)
            {
                case WindowType.None:
                    ActiveWindows.Push(ItemWindow);
                    ItemWindow.ShowWindow();
                    break;
                case WindowType.Item:
                    ItemWindow.HideWindow();
                    ActiveWindows.Pop();
                    break;
                case WindowType.ItemCommand:
                    ItemCommandWindow.HideWindow();
                    ActiveWindows.Pop();
                    break;
            }
            inputWait = 10;
        }
    }
}

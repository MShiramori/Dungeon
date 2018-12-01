using Assets.Script.Enums;
using Assets.Script.Model;
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

        public Stack<Window> ActiveWindows = new Stack<Window>();

        private int inputWait = 0;

        public void Initialize(Dungeon dungeon)
        {
            ItemWindow.Initialize(dungeon);
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

        /// <summary>
        /// 入力チェック
        /// </summary>
        public void CheckInput()
        {
            if (inputWait > 0)
                return;

            var type = GetCurrentWindowType();
            if (type == WindowType.None)
            {
                //メニューを開く
                if (Input.GetKey(KeyCode.X))
                {
                    ActiveWindows.Push(ItemWindow);
                    ItemWindow.ShowWindow();
                    inputWait = 10;
                }
            }
            else
            {
                //開いているウィンドウの入力チェック
                if (ActiveWindows.Peek().CheckInput())
                {
                    inputWait = 10;
                }
            }
        }
    }
}

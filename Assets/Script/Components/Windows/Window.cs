using Assets.Script.Enums;
using Assets.Script.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Components
{
    public class Window : MonoBehaviour
    {
        /// カーソル位置 ///
        public int CursorIndex { get; set; }
        /// ウィンドウの種類 ///
        public virtual WindowType Type { get { return WindowType.None; } }
        /// 表示優先度 ///
        public int Priority { get; set; }

        public WindowRootPresenter RootPresenter;

        public virtual void ShowWindow()
        {
            this.gameObject.SetActive(true);
        }

        public virtual void HideWindow()
        {
            this.gameObject.SetActive(false);
        }

        //入力チェック
        public virtual bool CheckInput()
        {
            //閉じる
            if (Input.GetKey(KeyCode.X))
            {
                CloseWindow();
                return true;
            }

            return false;
        }

        public void CloseWindow()
        {
            var window = RootPresenter.ActiveWindows.Pop();
            window.HideWindow();
        }

        public void CloseAllWindow()
        {
            foreach (var window in RootPresenter.ActiveWindows)
            {
                window.HideWindow();
            }
            RootPresenter.ActiveWindows.Clear();
        }
    }
}

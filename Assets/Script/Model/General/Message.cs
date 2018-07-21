using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;
using UnityEngine.UI;

namespace Assets.Script.Model
{
    /// <summary>
    /// ゲーム内メッセージ表示管理クラス
    /// </summary>
    public class Message
    {
        public MessageMode Mode { get; private set; }
        public Queue<string> Log { get; private set; }
        public Text UIText { get; private set; }

        private const int MAX_MESSAGE_COUNT = 10;

        public Message(Text uiText)
        {
            Mode = MessageMode.None;
            Log = new Queue<string>();
            UIText = uiText;
            UIText.text = "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        /// <param name="isWait">メッセージ確認するまで待機状態にするかどうか</param>
        /// <returns></returns>
        public IObservable<Unit> ShowMessage(string message, bool isWait)
        {
            if (Log.Count() >= MAX_MESSAGE_COUNT)
                Log.Dequeue();
            Log.Enqueue(message);

            UIText.text = string.Join("\n", Log.Reverse().ToArray());

            if (!isWait)
                return Observable.ReturnUnit();

            this.Mode = MessageMode.OnWaitTime;
            return Observable.TimerFrame(20)
                .Do(x =>
                {
                    this.Mode = MessageMode.OnWaitInput;
                })
                .AsUnitObservable();
        }

        public void OnInput()
        {
            if (this.Mode != MessageMode.OnWaitInput)
                return;

            this.Mode = MessageMode.None;
        }
    }
}

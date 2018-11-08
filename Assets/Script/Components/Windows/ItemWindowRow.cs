using Assets.Script.Enums;
using Assets.Script.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.Components
{
    public class ItemWindowRow : MonoBehaviour
    {
        public Text KeyText;
        public Image Icon;
        public Text NameText;
        public Button Button;

        public KeyCode Key { get; private set; }
        public Item Model { get; private set; }
        public Func<bool> Function { get; private set; }

        private CompositeDisposable disposable;

        public void Initialize(Func<bool> func)
        {
            disposable = new CompositeDisposable();
            this.Function = func;

            this.Button.OnClickAsObservable().Subscribe(_ =>
            {
                //項目をクリックしたとき
                Function();
            }).AddTo(disposable);
        }

        public void SetItem(Item item, int idx)
        {
            var master = Database.DataBase.KeyMaster[idx];
            this.Key = master.Item2;
            this.KeyText.text = string.Format("{0})", master.Item1);
            this.Model = item;
            this.Icon.sprite = item.GetSprite();
            this.NameText.text = item.Name + (item.IsEquiped ? "（装備中" : "");
        }

        private void OnDestroy()
        {
            disposable.Dispose();
        }
    }
}

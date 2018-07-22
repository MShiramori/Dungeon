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
        public Image Icon;
        public Text NameText;
        public Button Button;

        public Item Model { get; private set; }

        public void Initialize()
        {
            this.Button.OnClickAsObservable().Subscribe(_ =>
            {
                //アイテム選択時処理
                Debug.LogFormat("選択:{0}", Model.Name);
            });
        }

        public void SetItem(Item item)
        {
            this.Model = item;
            this.Icon.sprite = item.GetSprite();
            this.NameText.text = item.Name;
        }
    }
}

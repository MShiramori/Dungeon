using Assets.Script.Enums;
using Assets.Script.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.Components
{
    public class ItemCommandWindow : Window
    {
        public override WindowType Type { get { return WindowType.ItemCommand; } }

        public Image Icon;
        public Text NameText;
        public Transform CommandRoot;
        public CommandRow CommandRowPrefab;

        private Item _model;
        private List<CommandRow> Commands = new List<CommandRow>();

        public void UpdateInfo(Item model)
        {
            this._model = model;
            this.Icon.sprite = model.GetSprite();
            this.NameText.text = model.Name;

            var children = CommandRoot.GetComponentsInChildren<CommandRow>();
            foreach (var child in children)
            {
                Destroy(child.gameObject);
            }

            //TODO: アイテムの種類に応じたコマンドを追加する
            Commands.Clear();
            string[] itemcoms = new string[]{ "使う", "投げる", "置く" };//とりあえず適当に
            var idx = 0;
            foreach(var command in itemcoms)
            {
                var row = Instantiate(CommandRowPrefab);
                row.transform.SetParent(CommandRoot, false);
                row.GetComponent<RectTransform>().localPosition = new Vector2(0, idx * -40);
                row.SetCommand(command, idx);
                row.Initialize(() => { return false; });//仮なので何もしない
                Commands.Add(row);
                idx++;
            }

            var rect = this.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, 124 + Commands.Count() * 40);
        }
    }
}

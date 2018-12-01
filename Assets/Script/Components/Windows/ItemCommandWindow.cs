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
    public class ItemCommandWindow : Window
    {
        public override WindowType Type { get { return WindowType.ItemCommand; } }

        public Image Icon;
        public Text NameText;
        public Transform CommandRoot;
        public CommandRow CommandRowPrefab;

        private Item _model;
        private List<CommandRow> Commands = new List<CommandRow>();
        private Dungeon _dungeon;

        public void Initialize(Dungeon dungeon)
        {
            _dungeon = dungeon;
        }

        //入力チェック
        public override bool CheckInput()
        {
            //コマンドごとの処理を実行する
            foreach (var row in Commands)
            {
                if (Input.GetKey(row.Key))
                {
                    row.Function();
                    return true;
                }
            }

            return base.CheckInput();
        }

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

            //アイテムの種類に応じたコマンドを追加する
            Commands.Clear();
            var list = GetCommandList();
            var idx = 0;
            foreach(var command in list)
            {
                var row = Instantiate(CommandRowPrefab);
                row.transform.SetParent(CommandRoot, false);
                row.GetComponent<RectTransform>().localPosition = new Vector2(0, idx * -40);
                row.SetCommand(command.Item1, idx);
                row.Initialize(() => { return command.Item2(); });
                Commands.Add(row);
                idx++;
            }

            var rect = this.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, 124 + Commands.Count() * 40);
        }

        /// <summary>
        /// アイテムの種類に応じてコマンドを取得する
        /// </summary>
        /// <returns></returns>
        private List<Tuple<string,Func<bool>>> GetCommandList()
        {
            var list = new List<Tuple<string, Func<bool>>>();

            if(_model.Category == ItemCategory.Weapon 
                || _model.Category == ItemCategory.Armor
                || _model.Category == ItemCategory.Ring
                || _model.Category == ItemCategory.Arrow)
            {
                list.Add(Tuple.Create<string, Func<bool>>(_model.IsEquiped ? "外す" : "装備", () =>
                {
                    var result = _dungeon.Player.EquipItem(_model);
                    CloseAllWindow();
                    return result;
                }));
            }

            if (_model.Category == ItemCategory.Potion)
            {
                list.Add(Tuple.Create<string, Func<bool>>("飲む", () =>
                {
                    var result = _dungeon.Player.DrinkPotion(_model);
                    CloseAllWindow();
                    return result;
                }));
            }

            list.Add(Tuple.Create<string, Func<bool>>("投げる", () =>
            {
                var result = _dungeon.Player.ThrowItem(_model);
                CloseAllWindow();
                return result;
            }));

            list.Add(Tuple.Create<string, Func<bool>>("置く", () =>
            {
                var result = _dungeon.Player.PutItem(_model);
                CloseAllWindow();
                return result;
            }));

            return list;
        }
    }
}

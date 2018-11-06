using Assets.Script.Enums;
using Assets.Script.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Components
{
    public class ItemWindow : Window
    {
        public Transform ItemRoot;
        public ItemWindowRow ItemRowPrefab;
        public ItemCommandWindow ItemCommandWindow;

        public override WindowType Type { get { return WindowType.Item; } }

        public List<ItemWindowRow> Rows = new List<ItemWindowRow>();

        public override void ShowWindow()
        {
            UpdateList();
            base.ShowWindow();
        }

        //入力チェック
        public override bool CheckInput()
        {
            //リストに対応した詳細を開く
            foreach (var row in Rows)
            {
                if (Input.GetKey(row.Key))
                {
                    return row.Function();
                }
            }

            return base.CheckInput();
        }

        //入力に対応したアイテムのコマンドを開く
        public bool OpenCommandWindow(Item item)
        {
            ItemCommandWindow.UpdateInfo(item);
            ItemCommandWindow.ShowWindow();
            RootPresenter.ActiveWindows.Push(ItemCommandWindow);
            return true;
        }

        public void UpdateList()
        {
            var items = StaticData.PlayerParams.Items;

            var children = ItemRoot.GetComponentsInChildren<ItemWindowRow>();
            foreach(var child in children)
            {
                Destroy(child.gameObject);
            }
            Rows.Clear();
            var idx = 0;
            foreach(var item in items)
            {
                var row = Instantiate(ItemRowPrefab);
                row.transform.SetParent(ItemRoot, false);
                row.GetComponent<RectTransform>().localPosition = new Vector2(0, idx * -40);
                row.SetItem(item, idx);
                row.Initialize(() => OpenCommandWindow(row.Model));
                Rows.Add(row);
                idx++;
            }
        }
    }
}

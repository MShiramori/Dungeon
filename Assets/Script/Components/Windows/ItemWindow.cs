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

        public override WindowType Type { get { return WindowType.Item; } }

        public override void ShowWindow()
        {
            UpdateList();
            base.ShowWindow();
        }

        public void UpdateList()
        {
            var items = StaticData.PlayerParams.Items;

            var children = ItemRoot.GetComponentsInChildren<ItemWindowRow>();
            foreach(var child in children)
            {
                Destroy(child.gameObject);
            }
            var idx = 0;
            foreach(var item in items)
            {
                var row = Instantiate(ItemRowPrefab);
                row.transform.SetParent(ItemRoot, false);
                row.GetComponent<RectTransform>().localPosition = new Vector2(0, idx * -80);
                row.SetItem(item);
                row.Initialize();
                idx++;
            }
        }
    }
}

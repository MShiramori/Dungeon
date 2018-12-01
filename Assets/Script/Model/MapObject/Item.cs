using Assets.Script.Database;
using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Model
{
    public class Item : MapObject
    {
        public override string Name { get { return GetName(); } }
        public int MasterId { get; private set; }
        public ItemMaster Master { get; private set; }
        public ItemCategory Category { get { return Master.Category; } }
        public int Powor { get { return Master.Powor; } }
        public int CountValue { get; set; }//装備の強化値、杖の回数、矢の本数など
        //装備中判定
        public bool IsEquiped
        {
            get
            {
                var equips = dungeon.Player.Equips;
                return equips.ContainsKey(this.Category) && equips[this.Category] == this;
            }
        }

        public Item(Dungeon _dungeon, int id, int count) : base(_dungeon)
        {
            this.MasterId = id;
            this.Master = DataBase.ItemMasters[id];
            this.CountValue = count;
        }

        protected override void SetTexture()
        {
            sprites = Resources.LoadAll<Sprite>(string.Format("Textures/Objects/test/sword"));
            UpdateSpriteImage();
        }

        private string GetName()
        {
            switch (Category)
            {
                case ItemCategory.Weapon:
                case ItemCategory.Armor:
                    if (CountValue == 0) return Master.Name;
                    if (CountValue < 0) return Master.Name + CountValue.ToString();
                    return Master.Name + "+" + CountValue.ToString();
                case ItemCategory.Arrow:
                case ItemCategory.Rod:
                    return string.Format("{0}[{1}]", Master.Name, CountValue);
            }
            return Master.Name;
        }

        public Sprite GetSprite()
        {
            if (sprites == null || !sprites.Any()) return null;
            return sprites[0];
        }

        /// <summary>
        /// アイテムが床に落ちる処理
        /// </summary>
        /// <returns></returns>
        public bool DropFloor(Form target)
        {
            if (dungeon.Objects.Any(x => x.Position == target))
            {
                //TODO: 落ちた場所に既に何かあったとき、周囲に落とす処理
                Debug.LogFormat("既に何かあるので{0}を置けなかった", this.Name);
                return false;
            }

            //落ちる
            if (!dungeon.Objects.Contains(this))
            {
                //ダンジョンのオブジェクト一覧に追加
                dungeon.AddObject(this, target);
            }
            else
            {
                //既にあるなら移動
                this.Position = target;
            }

            if (this.Presenter == null)
            {
                //GameObjectがなければ生成
                InstantiateObject(dungeon.DungeonPrefabs.ObjectPrefab, dungeon.ObjectRoot);
            }
            Debug.LogFormat("{0}は地面に落ちた", this.Name);

            return true;
        }
    }
}

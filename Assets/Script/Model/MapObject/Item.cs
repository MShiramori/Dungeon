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

        public Item(Dungeon _dungeon, int id) : base(_dungeon)
        {
            this.MasterId = id;
            this.Master = DataBase.ItemMasters[id];
            this.CountValue = 0;//TODO: 回数指定
        }

        public bool PutItem()
        {
            //TODO: プレイヤー以外にもやらせる場合どうするか要検討
            return dungeon.Player.PutItem(this);
        }

        public bool EquipItem()
        {
            return dungeon.Player.EquipItem(this);
        }

        public bool DrinkPotion()
        {
            return dungeon.Player.DrinkPotion(this);
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
    }
}

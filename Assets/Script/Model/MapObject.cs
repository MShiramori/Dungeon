using Assets.Script.Components;
using Assets.Script.Database;
using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Model
{
    public abstract class MapObject
    {
        public int UniqueId { get; set; }
        public Form Position { get; set; }
        public abstract string Name { get; }

        public MapObjectPresenter Presenter { get; set; }

        protected Sprite[] sprites;
        protected Dungeon dungeon { get; set; }

        public MapObject(Dungeon _dungeon)
        {
            this.dungeon = _dungeon;
        }

        /// <summary>
        /// オブジェクトを生成する
        /// </summary>
        public virtual void InstantiateObject(GameObject prefab, Transform objectRoot)
        {
            Presenter = GameObject.Instantiate(prefab).GetComponent<MapObjectPresenter>();
            Presenter.transform.SetParent(objectRoot, false);
            SetTexture();
            ResetViewPotition();
        }

        /// <summary>
        /// オブジェクトを消去する
        /// </summary>
        public void RemoveObject()
        {
            this.Position = new Form(-1, -1);
            GameObject.Destroy(this.Presenter.gameObject);
        }

        /// <summary>
        /// 画像設定
        /// </summary>
        protected abstract void SetTexture();

        /// <summary>
        /// スプライト画像を更新する
        /// </summary>
        protected virtual void UpdateSpriteImage()
        {
            if (sprites == null || !sprites.Any()) return;
            Presenter.Sprite.sprite = sprites[0];
        }

        /// <summary>
        /// 表示位置を現在座標に合わせる
        /// </summary>
        public virtual void ResetViewPotition()
        {
            if (Presenter == null) return;

            Presenter.transform.localPosition = new Vector3(Position.x * 32, Position.y * -32, 0);
            this.Presenter.Sprite.sortingOrder = 100 + this.Position.y;
        }
    }

    public class Step : MapObject
    {
        public override string Name { get { return "階段"; } }
        public bool IsNext { get; set; }

        public Step(Dungeon _dungeon) : base(_dungeon) { }

        protected override void SetTexture()
        {
            sprites = Resources.LoadAll<Sprite>(string.Format("Textures/Objects/test/steps"));
            UpdateSpriteImage();
        }

        protected override void UpdateSpriteImage()
        {
            if (sprites == null || !sprites.Any()) return;
            Presenter.Sprite.sprite = sprites[IsNext ? 1 : 0];
        }
    }

    public class Item : MapObject
    {
        public override string Name { get { return GetName(); } }
        public int MasterId { get; private set; }
        public ItemMaster Master { get; private set; }
        public ItemCategory Category { get { return Master.Category; } }
        public int CountValue { get; set; }//装備の強化値、杖の回数、矢の本数など

        public Item(Dungeon _dungeon, int id) : base(_dungeon)
        {
            this.MasterId = id;
            this.Master = DataBase.ItemMasters[id];
            this.CountValue = 0;//TODO: 回数指定
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

    public class Trap : MapObject
    {
        public override string Name { get { return Type.ToString(); } }
        public TrapType Type { get; set; }
        public bool IsVisible { get; set; }

        public Trap(Dungeon _dungeon) : base(_dungeon) { }

        protected override void SetTexture()
        {
            sprites = Resources.LoadAll<Sprite>(string.Format("Textures/Objects/test/steps"));
            UpdateSpriteImage();
        }
    }
}

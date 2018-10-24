using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UniRx;

namespace Assets.Script.Model
{
    public class Player : Character
    {
        protected override CharacterParams Params { get { return StaticData.PlayerParams; } }
        public override CharacterType Type { get { return CharacterType.Player; } }
        public int Level { get { return StaticData.PlayerParams.Level; } set { StaticData.PlayerParams.Level = value; } }
        public int Stamina { get { return StaticData.PlayerParams.Stamina; } set { StaticData.PlayerParams.Stamina = value; } }
        public int MaxStamina { get { return StaticData.PlayerParams.MaxStamina; } set { StaticData.PlayerParams.MaxStamina = value; } }
        public Dictionary<ItemCategory, Item> Equips { get { return StaticData.PlayerParams.Equips; } }

        public const int MAX_ITEM_COUNT = 20;

        public Player(Dungeon _dungeon) : base(_dungeon)
        {
            if (StaticData.PlayerParams == null)
            {
                StaticData.PlayerParams = new PlayerParams();
                Name = "プレイヤー";
                Level = 1;
                HP = 30;
                MaxHP = 30;
                Speed = 8;
                Stamina = 100;
                MaxStamina = 100;
                Params.Str = 8;
                Params.Vit = 0;
                Params.Dex = 0;
                Params.Agi = 0;
                Equips.Add(ItemCategory.Weapon , null);
                Equips.Add(ItemCategory.Armor, null);
                Equips.Add(ItemCategory.Arrow, null);
                Equips.Add(ItemCategory.Ring, null);
            }
        }

        // コマンド入力を受けて行動する
        public int CommandExec()
        {
            return MAX_WAIT / Speed;
        }

        public override void ResetViewPotition()
        {
            base.ResetViewPotition();
            dungeon.MainCamera.transform.localPosition = new Vector3(this.Presenter.transform.localPosition.x, this.Presenter.transform.localPosition.y, 0);
        }

        public override void MovingAnimationEveryFrameAction()
        {
            dungeon.MainCamera.transform.localPosition = new Vector3(this.Presenter.transform.localPosition.x, this.Presenter.transform.localPosition.y, 0);
        }

        protected override bool PickUpCore(Item item)
        {
            if(item.Position == this.Position && this.Items.Count < MAX_ITEM_COUNT)
            {
                this.Items.Add(item);
                item.RemoveObject();
                Debug.LogFormat("{0}を拾った", item.Name);
                return true;
            }
            Debug.LogFormat("持ち物がいっぱいで{0}を拾えなかった", item.Name);
            return false;
        }

        /// <summary>
        /// オブジェクトを生成する
        /// </summary>
        public void SetCamera(Camera mainCamera)
        {
            dungeon.MainCamera = mainCamera;
        }

        /// <summary>
        /// 画像設定
        /// </summary>
        protected override void SetTexture()
        {
            sprites = Resources.LoadAll<Sprite>(string.Format("Textures/Character/test/player"));

            UpdateSpriteImage();
        }
    }
}

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
        public float Stamina { get { return StaticData.PlayerParams.Stamina; } set { StaticData.PlayerParams.Stamina = value; } }
        public int MaxStamina { get { return StaticData.PlayerParams.MaxStamina; } set { StaticData.PlayerParams.MaxStamina = value; } }
        public Dictionary<ItemCategory, Item> Equips { get { return StaticData.PlayerParams.Equips; } }
        public bool IsAction { get; set; }

        public const int MAX_ITEM_COUNT = 20;

        private float flacHP;//HP自動回復用端数

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

                flacHP = 0;
                IsAction = false;
            }
        }

        // コマンド入力を受けて行動する
        public int CommandExec()
        {
            return MAX_WAIT / Speed;
        }

        public override void AfterAction()
        {
            Stamina -= Speed / 80f;
            if (HP < MaxHP)
            {
                flacHP += MaxHP / 150f;
                if (flacHP >= 1)
                {
                    ReduceHP(-(int)flacHP);
                    flacHP -= (int)flacHP;
                }
            }
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
                //TODO:持ち物に入った時点でuniqueIdの管理とかおかしな感じになるのでオブジェクトと実体切り離してdungeonのリストから消去すべきではないかと愚考
                this.Items.Add(item);
                item.RemoveObject();
                Debug.LogFormat("{0}を拾った", item.Name);
                return true;
            }
            Debug.LogFormat("持ち物がいっぱいで{0}を拾えなかった", item.Name);
            return false;
        }

        public bool PutItem(Item item)
        {
            var result = PutItemCore(item);
            ReservedActions.Add(new CharacterItemPut(this, item, result));//アイテム置いた処理を登録
            this.IsAction |= result;
            return result;
        }

        public bool PutItemCore(Item item)
        {
            //持ってない
            if (!Items.Contains(item))
                return false;
            //何かあったら置けない
            if (dungeon.Objects.Any(x => x.Position == this.Position))
                return false;

            if (Equips[item.Category] == item)
            {
                Equips[item.Category] = null;
            }
            Items.Remove(item);

            item.Position = this.Position;
            item.InstantiateObject(dungeon.DungeonPrefabs.ObjectPrefab, dungeon.ObjectRoot);

            return true;
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

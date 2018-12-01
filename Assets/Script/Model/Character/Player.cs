using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UniRx;
using Assets.Script.Database;

namespace Assets.Script.Model
{
    public class Player : Character
    {
        protected override CharacterParams Params { get { return StaticData.PlayerParams; } }
        public override int WeaponAtk
        {
            get
            {
                var weaponAtk = 0;
                Item weapon = null;
                if (Equips.TryGetValue(ItemCategory.Weapon, out weapon))
                {
                    if (weapon != null) weaponAtk = weapon.Powor + weapon.CountValue;
                }
                return weaponAtk;
            }
        }
        public override int Def
        {
            get
            {
                var ArmorDef = 0;
                Item armor = null;
                if (Equips.TryGetValue(ItemCategory.Armor, out armor))
                {
                    if (armor != null) ArmorDef = armor.Powor + armor.CountValue;
                }
                return base.Def + ArmorDef;
            }
        }
        public override CharacterType Type { get { return CharacterType.Player; } }
        public int Level { get { return StaticData.PlayerParams.Level; } set { StaticData.PlayerParams.Level = value; } }
        public long Exp { get { return StaticData.PlayerParams.Exp; } set { StaticData.PlayerParams.Exp = value; } }
        // 満腹度
        public float Satiety { get { return StaticData.PlayerParams.Stamina; } set { StaticData.PlayerParams.Stamina = value; } }
        public int ViewableSatiety { get { return Mathf.CeilToInt(Satiety); } }
        public int MaxSatiety { get { return StaticData.PlayerParams.MaxStamina; } set { StaticData.PlayerParams.MaxStamina = value; } }
        public Dictionary<ItemCategory, Item> Equips { get { return StaticData.PlayerParams.Equips; } }
        public bool IsAction { get; set; }

        public const int MAX_ITEM_COUNT = 20;
        public const int DEFAULT_HP = 30;

        private float flacHP;//HP自動回復用端数

        public Player(Dungeon _dungeon) : base(_dungeon)
        {
            if (StaticData.PlayerParams == null)
            {
                StaticData.PlayerParams = new PlayerParams();
                Name = "プレイヤー";
                Level = 1;
                HP = DEFAULT_HP;
                MaxHP = DEFAULT_HP;
                Speed = 8;
                Satiety = 100;
                MaxSatiety = 100;
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

        /// <summary>
        /// 満腹度を減らす（マイナス指定なら回復する）
        /// </summary>
        public float ReduceSatiety(float point)
        {
            var beforSatiety = Satiety;
            Satiety = Mathf.Clamp(Satiety - point, 0, MaxSatiety);
            return beforSatiety - Satiety;
        }

        /// <summary>
        /// 経験値を加算する
        /// </summary>
        public bool AddExp(long point)
        {
            this.Exp += point;
            var afterLevel = Database.DataBase.PlayerLevelMasters.Where(x => x.Value <= this.Exp).Min(x => x.Key);
            if (afterLevel != this.Level)
            {
                UpdateLevel(afterLevel);
            }
            return true;
        }

        /// <summary>
        /// レベルを加算する
        /// </summary>
        public bool AddLevel(int point)
        {
            if(point > 0)//レベルアップ
            {
                var maxLevel = Database.DataBase.PlayerLevelMasters.Max(x => x.Key);
                if (this.Level >= maxLevel)
                {
                    //既に最大Lv
                    return false;
                }
                var afterLevel = Mathf.Min(maxLevel, this.Level + point);
                this.Exp = Database.DataBase.PlayerLevelMasters[afterLevel];
                UpdateLevel(afterLevel);
            }
            else//レベルダウン
            {
                if(this.Level == 1)
                {
                    //これ以上下がらない
                    return false;
                }
                var afterLevel = Mathf.Max(1, this.Level - point);
                this.Exp = Database.DataBase.PlayerLevelMasters[afterLevel + 1] - 1;
                UpdateLevel(afterLevel);
            }
            return true;
        }

        private void UpdateLevel(int level)
        {
            this.Level = level;
            this.MaxHP = DEFAULT_HP + (level - 1) * 8;
        }

        // コマンド入力を受けて行動する
        public int CommandExec()
        {
            return MAX_WAIT / Speed;
        }

        public override void AfterAction()
        {
            ReduceSatiety(Speed / 80f);
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

        protected override List<CharacterAction> KillCharacter(Character target)
        {
            var exp = 0;
            var enemy = target as Enemy;
            if (enemy != null)
            {
                exp = enemy.HasExp;
            }
            var beforLevel = this.Level;
            this.AddExp(exp);
            var subActions = new List<CharacterAction>();
            subActions.Add(new MessageAction(this, string.Format("{0}の経験値を獲得", exp), false));
            if (this.Level > beforLevel)
            {
                subActions.Add(new MessageAction(this, string.Format("レベル{0}へようこそ", this.Level), false));
            }
            return subActions;
        }

        protected override bool PickUpCore(Item item)
        {
            if(item.Position == this.Position && this.Items.Count < MAX_ITEM_COUNT)
            {
                //持ち物一覧に加える
                var newId = this.Items.Any() ? this.Items.Max(x => x.UniqueId) : 0;
                this.Items.Add(item);
                //オブジェクトを削除してダンジョンのリストから消去
                item.RemoveObject();
                dungeon.Objects.Remove(item);
                item.UniqueId = newId;//一応持ち物内ＩＤとして再採番
                Debug.LogFormat("{0}を拾った", item.Name);
                return true;
            }
            Debug.LogFormat("持ち物がいっぱいで{0}を拾えなかった", item.Name);
            return false;
        }

        protected override bool PutItemCore(Item item)
        {
            //持ってない
            if (!Items.Contains(item))
                return false;
            //何かあったら置けない
            if (dungeon.Objects.Any(x => x.Position == this.Position))
                return false;

            if (Equips.ContainsKey(item.Category) && Equips[item.Category] == item)
            {
                Equips[item.Category] = null;
            }
            Items.Remove(item);

            dungeon.AddObject(item, this.Position);
            item.InstantiateObject(dungeon.DungeonPrefabs.ObjectPrefab, dungeon.ObjectRoot);

            this.IsAction = true;
            return true;
        }

        public override bool ThrowItem(Item item)
        {
            //持ってない
            if (!Items.Contains(item))
                return false;

            var result = ThrowItemCore(item);
            if (result)
            {
                //アイテムを持ち物から消去or矢弾なら数を減らす
                if (item.Category == ItemCategory.Arrow)
                {
                    if (item.CountValue > 1)
                    {
                        item.CountValue--;
                    }
                    else
                    {
                        if (Equips.ContainsKey(item.Category) && Equips[item.Category] == item)
                        {
                            Equips[item.Category] = null;
                        }
                        Items.Remove(item);
                    }
                }
                else
                {
                    if (Equips.ContainsKey(item.Category) && Equips[item.Category] == item)
                    {
                        Equips[item.Category] = null;
                    }
                    Items.Remove(item);
                }
            }
            this.IsAction |= result;
            return result;
        }

        /// <summary>
        /// アイテムを装備する
        /// 装備中のアイテムが選択された場合は外す
        /// </summary>
        public bool EquipItem(Item item)
        {
            var result = EquipItemCore(item);
            this.IsAction |= result;
            return result;
        }

        private bool EquipItemCore(Item item)
        {
            //装備できないカテゴリのアイテム
            if (!Equips.ContainsKey(item.Category))
            {
                StaticData.Message.ShowMessage(string.Format("それは装備できない"), false);
                return false;
            }

            //装備中のアイテムなら外す
            if (Equips[item.Category] == item)
            {
                Equips[item.Category] = null;
                StaticData.Message.ShowMessage(string.Format("{0}を外した", item.Name), false);
                return true;
            }

            //装備する
            Equips[item.Category] = item;
            StaticData.Message.ShowMessage(string.Format("{0}を装備した", item.Name), false);
            return true;
        }

        /// <summary>
        /// ポーションを飲む
        /// TODO: 効果をdelegate化してマスタに突っ込む？
        /// </summary>
        public bool DrinkPotion(Item item)
        {
            if (item.Category != ItemCategory.Potion)
            {
                return false;
            }

            var action = new CharacterDrinkPotionAction(this, item);
            var effectType = (item.Master as PotionMaster).EffectType;
            switch (effectType)
            {
                case PotionEffectType.回復薬:
                    if (this.HP < this.MaxHP)
                    {
                        var point = this.ReduceHP(-Mathf.Max(25, (int)(this.MaxHP * 0.3)));
                        action.SubActions.Add(new MessageAction(this, string.Format("HPが{0}回復した", -point), false));
                    }
                    else
                    {
                        this.MaxHP += 1;
                        this.HP = this.MaxHP;
                        action.SubActions.Add(new MessageAction(this, string.Format("最大HPが{0}アップした", 1), false));
                    }
                    break;

                case PotionEffectType.強回復薬:
                    if (this.HP < this.MaxHP)
                    {
                        var point = this.ReduceHP(-Mathf.Max(100, (int)(this.MaxHP * 0.8)));
                        action.SubActions.Add(new MessageAction(this, string.Format("HPが{0}回復した", -point), false));
                    }
                    else
                    {
                        this.MaxHP += 2;
                        this.HP = this.MaxHP;
                        action.SubActions.Add(new MessageAction(this, string.Format("最大HPが{0}アップした", 2), false));
                    }
                    break;

                case PotionEffectType.Lvアップ:
                    if (this.AddLevel(1))
                    {
                        action.SubActions.Add(new MessageAction(this, string.Format("レベル{0}へようこそ", this.Level), false));
                    }
                    break;

                case PotionEffectType.Lvダウン:
                    if (this.AddLevel(-1))
                    {
                        action.SubActions.Add(new MessageAction(this, string.Format("体から力が抜けていく…"), true));
                        action.SubActions.Add(new MessageAction(this, string.Format("{0}はレベル{1}になった", this.Name, this.Level), false));
                    }
                    break;
            }

            //満腹度5回復
            this.ReduceSatiety(-5f);
            //消費
            this.Items.Remove(item);

            //行動を登録
            ReservedActions.Add(action);
            this.IsAction = true;

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
        /// １回の行動ループ終了時の後処理
        /// </summary>
        public override void TurnEndEvent()
        {
            this.IsAction = false;
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

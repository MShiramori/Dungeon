using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UniRx;
using Assets.Script.Components;

namespace Assets.Script.Model
{
    public abstract class Character
    {
        public int UniqueId { get; private set; }
        public Form Position { get; private set; }
        public Form Direction { get; private set; }
        protected abstract CharacterParams Params { get; }
        public string Name { get { return Params.Name; } set { Params.Name = value; } }
        public int HP { get { return Params.HP; } set { Params.HP = value; } }
        public int MaxHP { get { return Params.MaxHP; } set { Params.MaxHP = value; } }
        public int Speed { get { return Params.Speed; } set { Params.Speed = value; } }
        public List<Item> Items { get { return Params.Items; } }
        public int ActionWait { get; set; }

        protected const int MAX_WAIT = 120;
        private static int lastId = 0;
        protected Dungeon dungeon { get; set; }

        public abstract CharacterType Type { get; }
        private List<CharacterAction> ReservedActions;
        public CharacterPresenter Presenter { get; set; }

        private SerialDisposable spriteAnimationDisposable;
        protected Sprite[] sprites;
        private long spriteAnimationCurrentIndex;
        private static Dictionary<Form, int> directionSpriteIndexis;

        public Character(Dungeon _dungeon)
        {
            UniqueId = lastId++;
            ActionWait = MAX_WAIT;
            dungeon = _dungeon;
            ReservedActions = new List<CharacterAction>();
            spriteAnimationDisposable = new SerialDisposable();
        }

        /// <summary>
        /// オブジェクトを生成する
        /// </summary>
        public virtual void InstantiateObject(GameObject prefab, Transform characterRoot)
        {
            Presenter = GameObject.Instantiate(prefab).GetComponent<CharacterPresenter>();
            Presenter.transform.SetParent(characterRoot, false);
            spriteAnimationDisposable.AddTo(Presenter);
            SetTexture();
            StartSpriteAnimation();
            ResetViewPotition();
        }

        //　時間経過で行動ウェイトを減らし、ウェイトゼロになったら行動させる
        public bool Tick()
        {
            ActionWait -= Speed;
            if (ActionWait <= 0)
            {
                Action();
                ActionWait = MAX_WAIT;
                return true;
            }
            return false;
        }

        // 行動処理
        protected virtual void Action()
        {

        }

        protected bool CanMove(Form destination)
        {
            if (destination == this.Position)
                return false;
            if(destination.x < 0 || destination.y < 0 || destination.x > dungeon.MapSize.x || destination.y > dungeon.MapSize.y)
                return false;

            // 他キャラ衝突チェック
            if (dungeon.Characters.Where(x => x != this).Any(x => x.Position == destination))
                return false;

            // 地形チェック
            var cell = dungeon.MapData[destination.x, destination.y];
            if (cell.Terra == Enums.Terrain.Wall)
                return false;
            else
            {
                var direction = destination - this.Position;
                if (direction.AbsTotal > 1)//ナナメの場合は両脇の壁判定
                {
                    if (dungeon.MapData[this.Position.x + direction.x, this.Position.y].Terra == Enums.Terrain.Wall
                        || dungeon.MapData[this.Position.x, this.Position.y + direction.y].Terra == Enums.Terrain.Wall)
                        return false;
                }
            }

            return true;
        }

        public bool Move(Form destination)
        {
            SetDirection(destination - Position);
            if (CanMove(destination))
            {
                ReservedActions.Add(new CharacterMove(this, this.Position, destination));//移動情報を登録
                SetPosition(destination);
                return true;
            }
            return false;
        }

        public bool Attack()
        {
            //攻撃処理
            var action = new CharacterAttack(this, this.Direction);
            var target = dungeon.Characters.FirstOrDefault(x => x.Position == this.Position + this.Direction);
            if(target != null)
            {
                //命中判定
                var isHit = (int)UnityEngine.Random.Range(0, 100) >= Mathf.Clamp(target.Params.Agi - this.Params.Dex + 20, 0, 95);
                if (isHit)
                {
                    var damage = Mathf.Max(1, (int)((this.Params.Attack - target.Params.Defence / 2) * UnityEngine.Random.Range(0.95f, 1.05f)));
                    action.SubActions.Add(new CharacterDamaged(target, this, damage, true));
                }
                else
                {
                    action.SubActions.Add(new CharacterDamaged(target, this, 0, false));
                }
            }

            ReservedActions.Add(action);//攻撃情報を登録

            return true;
        }

        /// <summary>
        /// アイテムを拾う処理
        /// </summary>
        public bool PickUp(Item item)
        {
            var result = PickUpCore(item);
            ReservedActions.Add(new CharacterItemPickup(this, item, result));//アイテム拾得行動情報を登録
            return true;
        }

        /// <summary>
        /// 拾う処理の実態
        /// </summary>
        /// <returns>拾うのに成功したか</returns>
        protected virtual bool PickUpCore(Item item)
        {
            return false;
        }

        public void SetPosition(Form position)
        {
            this.Position = position;
        }

        public void SetDirection(Form direction)
        {
            this.Direction = direction.GetDirection();
            UpdateSpriteImage();
        }

        /// <summary>
        /// 表示位置を現在座標に合わせる
        /// </summary>
        public virtual void ResetViewPotition()
        {
            if (Presenter == null) return;

            Presenter.transform.localPosition = new Vector3(Position.x * 32 + 2, Position.y * -32, 0);
            this.Presenter.Sprite.sortingOrder = 500 + this.Position.y;
        }

        /// <summary>
        /// 移動アニメーション
        /// </summary>
        /// <returns></returns>
        public IObservable<Unit> MovingAnimation()
        {
            var actions = ReservedActions.Select(x => x as CharacterMove).Where(x => x != null).ToArray();
            if (!actions.Any())
                return Observable.ReturnUnit();
            var frame = DungeonConstants.MOVING_ANIMATION_FRAME / actions.Count();
            return actions.Select(x =>
            {
                return Observable.EveryUpdate()
                    .Take(frame)
                    .Do(i => this.Presenter.transform.localPosition = 
                        new Vector3(
                            (x.FromPosition.x + (x.DesPosition.x - x.FromPosition.x) * (float)(i + 1) / frame) * 32 + 2, 
                            (x.FromPosition.y + (x.DesPosition.y - x.FromPosition.y) * (float)(i + 1) / frame) * -32, 
                            0
                        ));
            })
            .Concat()
            .Do(_=> MovingAnimationEveryFrameAction())
            .Last()
            .AsUnitObservable();
        }

        /// <summary>
        /// 移動以外のアクションを順次実行
        /// </summary>
        public IObservable<Unit> ActionAnimations()
        {
            var actions = ReservedActions.Where(x => x as CharacterMove == null).ToArray();
            if (!actions.Any())
                return Observable.ReturnUnit();
            var actionList = new List<IObservable<Unit>>();
            foreach (var action in actions)
            {
                var attackAction = action as CharacterAttack;
                if (attackAction != null)
                {
                    actionList.Add(AttackAnimation(attackAction));
                    continue;
                }
                var pickupAction = action as CharacterItemPickup;
                if (pickupAction != null)
                {
                    actionList.Add(Observable.Defer(() =>
                    {
                        if (pickupAction.IsSuccess)
                            StaticData.Message.ShowMessage(string.Format("{0}を拾った", pickupAction.TargetItem.Name), false);
                        else
                            StaticData.Message.ShowMessage(string.Format("持ち物がいっぱいで{0}を拾えなかった", pickupAction.TargetItem.Name), false);
                        return Observable.ReturnUnit();
                    }));
                    continue;
                }
                actionList.Add(Observable.ReturnUnit());
            }
            return actionList.Concat();
        }

        /// <summary>
        /// 攻撃アニメーション
        /// </summary>
        private IObservable<Unit> AttackAnimation(CharacterAttack action)
        {
            var frame = 16;
            var attackAnimation = Observable.EveryUpdate()
                .Take(frame)
                .Do(i =>
                {
                    var dis = 0;
                    if (i < 2) dis = 3 * (int)i;
                    else if (i >= frame - 3) dis = 3 * (int)(frame - 1 - i);
                    else dis = 3 * 2;
                    var move = action.Direction * dis;
                    this.Presenter.transform.localPosition =
                        new Vector3(
                            this.Position.x * 32 + 2 + move.x,
                            this.Position.y * -32 - move.y,
                            0);
                })
                .Last()
                .AsUnitObservable();
            var damageAnimation = Observable.TimerFrame(4)
                .SelectMany(_ =>
                {
                    return action.SubActions
                        .Where(x => x as CharacterDamaged != null)
                        .Select(x => DamageAnimation(x as CharacterDamaged))
                        .Concat();
                });

            return Observable.WhenAll(attackAnimation, damageAnimation);
        }

        /// <summary>
        /// 被ダメージアニメーション
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IObservable<Unit> DamageAnimation(CharacterDamaged action)
        {
            if (!action.IsHit)
            {
                StaticData.Message.ShowMessage(string.Format("{0}の攻撃は外れた", action.Attacker.Name), false);
                return Observable.TimerFrame(8).AsUnitObservable();
            }

            var frame = 16;
            return Observable.EveryUpdate()
                .Take(frame)
                .Do(i =>
                {
                    //TODO: ダメージグラフィック
                    if(i == 8)
                    {
                        StaticData.Message.ShowMessage(string.Format("{0}は{1}に{2}のダメージを与えた", action.Attacker.Name, action.Character.Name, action.Point), false);
                    }
                })
                .Last()
                .AsUnitObservable();
        }

        public virtual void MovingAnimationEveryFrameAction()
        {

        }

        /// <summary>
        /// 予約したアクションを消去する
        /// </summary>
        public void ResetActions()
        {
            ReservedActions.Clear();
        }

        /// <summary>
        /// 画像設定
        /// </summary>
        protected abstract void SetTexture();

        /// <summary>
        /// 待機アニメーション実行
        /// </summary>
        protected void StartSpriteAnimation()
        {
            spriteAnimationDisposable.Disposable = Observable.IntervalFrame(10).Do(x =>
            {
                if (sprites == null || sprites.Length < 12) return;

                spriteAnimationCurrentIndex = x % 4;
                if (spriteAnimationCurrentIndex == 3) spriteAnimationCurrentIndex = 1;
                UpdateSpriteImage();
            })
            .Subscribe();
        }

        /// <summary>
        /// スプライト画像を更新する
        /// </summary>
        protected void UpdateSpriteImage()
        {
            if (sprites == null || sprites.Length < 12) return;
            Presenter.Sprite.sprite = sprites[spriteAnimationCurrentIndex + GetSpriteIndexByDirection()];
        }

        /// <summary>
        /// 向き情報から対応するスプライトの開始インデックスを取得する
        /// </summary>
        private int GetSpriteIndexByDirection()
        {
            if (directionSpriteIndexis == null)
            {
                directionSpriteIndexis = new Dictionary<Form, int>();
                directionSpriteIndexis.Add(new Form(0, 1), 0);
                directionSpriteIndexis.Add(new Form(-1, 1), 0);
                directionSpriteIndexis.Add(new Form(-1, 0), 3);
                directionSpriteIndexis.Add(new Form(-1, -1), 9);
                directionSpriteIndexis.Add(new Form(0, -1), 9);
                directionSpriteIndexis.Add(new Form(1, -1), 9);
                directionSpriteIndexis.Add(new Form(1, 0), 6);
                directionSpriteIndexis.Add(new Form(1, 1), 0);
            }

            var idx = 0;
            directionSpriteIndexis.TryGetValue(this.Direction, out idx);
            return idx;
        }

        public void Dispose()
        {
            spriteAnimationDisposable.Dispose();
        }
    }

    /// <summary>
    /// キャラクターの１アクション定義抽象型
    /// </summary>
    public abstract class CharacterAction
    {
        public Character Character { get; set; }

        public CharacterAction(Character self)
        {
            Character = self;
        }
    }

    /// <summary>
    /// 移動アクション
    /// </summary>
    public class CharacterMove : CharacterAction
    {
        //移動先
        public Form FromPosition;
        public Form DesPosition;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="des">移動先座標</param>
        public CharacterMove(Character self, Form from, Form des) : base(self)
        {
            FromPosition = from;
            DesPosition = des;
        }
    }

    /// <summary>
    /// 攻撃アクション
    /// </summary>
    public class CharacterAttack : CharacterAction
    {
        //向き
        public Form Direction;
        public List<CharacterAction> SubActions { get; private set; }

        public CharacterAttack(Character self, Form dir) : base(self)
        {
            Direction = dir;
            SubActions = new List<CharacterAction>();
        }
    }

    /// <summary>
    /// アイテムを拾うアクション
    /// </summary>
    public class CharacterItemPickup : CharacterAction
    {
        //対象のアイテム
        public Item TargetItem;
        public bool IsSuccess;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="item">対象のアイテム</param>
        /// <param name="isSuccess">拾えたかどうか</param>
        public CharacterItemPickup(Character self, Item item, bool isSuccess) : base(self)
        {
            TargetItem = item;
            IsSuccess = isSuccess;
        }
    }

    /// <summary>
    /// ダメージを受けたアクション
    /// </summary>
    public class CharacterDamaged : CharacterAction
    {
        public Character Attacker { get; private set; }
        public int Point { get; private set; }
        public bool IsHit { get; private set; }

        public CharacterDamaged(Character self, Character attacker, int point, bool isHit) : base(self)
        {
            this.Attacker = attacker;
            this.Point = point;
            this.IsHit = isHit;
        }
    }
}

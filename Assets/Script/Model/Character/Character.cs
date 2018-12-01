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
        public virtual int Atk { get { return Params.Attack; } }
        public virtual int WeaponAtk { get { return 0; } }
        public virtual int Def { get { return Params.Defence; } }
        public List<Item> Items { get { return Params.Items; } }
        public int ActionWait { get; set; }
        public bool IsDeath { get { return this.HP <= 0; } }
        public List<CharacterStatusEffect> StatusEffects { get; private set; }

        protected const int MAX_WAIT = 256;
        private static int lastId = 0;
        protected Dungeon dungeon { get; set; }

        public abstract CharacterType Type { get; }
        protected List<CharacterAction> ReservedActions;
        public CharacterPresenter Presenter { get; set; }

        private SerialDisposable spriteAnimationDisposable;
        protected Sprite[] sprites;
        private long spriteAnimationCurrentIndex;
        private static Dictionary<Form, int> directionSpriteIndexis;
        public static DirectionManager DirectionManager { get { return _directionManager == null ? _directionManager = new DirectionManager() : _directionManager; } }
        private static DirectionManager _directionManager;

        public Character(Dungeon _dungeon)
        {
            UniqueId = lastId++;
            ActionWait = MAX_WAIT;
            dungeon = _dungeon;
            ReservedActions = new List<CharacterAction>();
            spriteAnimationDisposable = new SerialDisposable();
            StatusEffects = new List<CharacterStatusEffect>();
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
            if (this.IsDeath) return false; //死んでる

            ActionWait -= Speed;
            if (ActionWait <= 0)
            {
                Action();
                ActionWait = ActionWait + MAX_WAIT;
                return true;
            }
            return false;
        }

        // 行動処理
        protected virtual void Action()
        {

        }

        // 行動後処理
        public virtual void AfterAction()
        {

        }

        /// <summary>
        /// HPを減らす（マイナス指定なら回復する）
        /// </summary>
        public int ReduceHP(int point)
        {
            var beforHP = HP;
            HP = Mathf.Clamp(HP - point, 0, MaxHP);
            return beforHP - HP;
        }

        protected bool CanMove(Form destination)
        {
            if (destination == this.Position)
                return false;
            if(destination.x < 0 || destination.y < 0 || destination.x > dungeon.MapSize.x || destination.y > dungeon.MapSize.y)
                return false;

            // 他キャラ衝突チェック
            if (dungeon.Characters.Where(x => x != this && !x.IsDeath).Any(x => x.Position == destination))
                return false;

            // 地形チェック
            if (CheckWallForAction(destination))
                return false;

            return true;
        }

        /// <summary>
        /// 移動・攻撃用壁判定
        /// </summary>
        protected bool CheckWallForAction(Form destination)
        {
            var cell = dungeon.MapData[destination.x, destination.y];
            if (cell.Terra == Enums.Terrain.Wall)
                return true;
            else
            {
                var direction = destination - this.Position;
                if (direction.AbsTotal > 1)//ナナメの場合は両脇の壁判定
                {
                    if (dungeon.MapData[this.Position.x + direction.x, this.Position.y].Terra == Enums.Terrain.Wall
                        || dungeon.MapData[this.Position.x, this.Position.y + direction.y].Terra == Enums.Terrain.Wall)
                        return true;
                }
            }
            return false;
        }

        public bool Move(Form destination)
        {
            SetDirection(destination - Position);
            if (CanMove(destination))
            {
                ReservedActions.Add(new CharacterMoveAction(this, this.Position, destination));//移動情報を登録
                SetPosition(destination);
                return true;
            }
            return false;
        }

        public bool Attack()
        {
            //攻撃処理
            var action = new CharacterAttackAction(this, this.Direction);
            var des = this.Position + this.Direction;
            var target = dungeon.Characters.FirstOrDefault(x => x.Position == des && !x.IsDeath);
            if(target != null && !target.IsDeath && !CheckWallForAction(des))
            {
                //命中判定
                var isHit = (int)UnityEngine.Random.Range(0, 100) >= Mathf.Clamp(target.Params.Agi - this.Params.Dex + 20, 0, 95);
                if (isHit)
                {
                    var damage = Mathf.Max(1, (int)((this.Atk + this.WeaponAtk - target.Def / 2) * UnityEngine.Random.Range(0.95f, 1.05f)));
                    target.ReduceHP(damage);
                    action.SubActions.Add(new CharacterDamagedAction(target, this, damage, true, target.IsDeath));
                    //相手死亡判定
                    if (target.IsDeath)
                    {
                        action.SubActions.AddRange(KillCharacter(target));
                    }
                }
                else
                {
                    action.SubActions.Add(new CharacterDamagedAction(target, this, 0, false, false));
                }
            }

            ReservedActions.Add(action);//攻撃情報を登録

            return true;
        }

        /// <summary>
        /// 攻撃で誰かを倒したときの処理
        /// </summary>
        protected virtual List<CharacterAction> KillCharacter(Character target)
        {
            return new List<CharacterAction>();
        }

        /// <summary>
        /// アイテムを拾う処理
        /// </summary>
        public bool PickUp(Item item)
        {
            var result = PickUpCore(item);
            ReservedActions.Add(new CharacterItemPickupAction(this, item, result));//アイテム拾得行動情報を登録
            return result;
        }

        /// <summary>
        /// 拾う処理の実体
        /// </summary>
        /// <returns>拾うのに成功したか</returns>
        protected virtual bool PickUpCore(Item item)
        {
            return false;
        }

        /// <summary>
        /// アイテムを置く処理
        /// </summary>
        public bool PutItem(Item item)
        {
            var result = PutItemCore(item);
            ReservedActions.Add(new CharacterItemPutAction(this, item, result));//アイテム置いた処理を登録
            return result;
        }

        protected virtual bool PutItemCore(Item item)
        {
            return false;
        }

        /// <summary>
        /// アイテムを投げる処理
        /// </summary>
        public virtual bool ThrowItem(Item item)
        {
            return ThrowItemCore(item);
        }

        public bool ThrowItemCore(Item item)
        {
            //矢弾の場合は１つずつ処理するので１個のオブジェクトを新たに生成
            if(item.Category == ItemCategory.Arrow && item.CountValue > 1)
            {
                item = new Item(dungeon, item.MasterId, 1);
            }

            var targetPosition = this.Position;
            var range = 10;//飛距離
            var subActions = new List<CharacterAction>();
            for (int i = 1; i <= range; i++)
            {
                targetPosition += this.Direction;
                //画面外
                if (targetPosition.x < 0 || targetPosition.x >= dungeon.MapSize.x || targetPosition.y < 0 || targetPosition.y >= dungeon.MapSize.y)
                {
                    Debug.LogFormat("{0}は彼方へと飛んでいった…", item.Name);
                    break;
                }
                //壁
                if (dungeon.MapData[targetPosition.x, targetPosition.y].Terra == Enums.Terrain.Wall)
                {
                    targetPosition -= this.Direction;
                    item.DropFloor(targetPosition);
                    break;
                }
                //他のキャラ
                var target = dungeon.Characters.Where(x => !x.IsDeath).FirstOrDefault(x => x.Position == targetPosition);
                if (target != null)
                {
                    //命中判定
                    var isHit = (int)UnityEngine.Random.Range(0, 100) >= Mathf.Clamp(target.Params.Agi - this.Params.Dex + 20, 0, 95);
                    if (isHit)
                    {
                        // 命中処理（targetのアイテムをぶつけられた処理呼び出し）
                        subActions.AddRange(target.HitItem(item, this));
                        Debug.LogFormat("{0}は{1}に命中した", item.Name, target.Name);
                        //相手死亡判定
                        if (target.IsDeath)
                        {
                            subActions.AddRange(KillCharacter(target));
                        }
                    }
                    else
                    {
                        item.DropFloor(targetPosition);
                    }
                    break;
                }
                //何もぶつからずに飛距離終わり
                if (i == range)
                {
                    item.DropFloor(targetPosition);
                }
            }
            var action = new CharacterThrowAction(this, item, targetPosition);
            action.SubActions.AddRange(subActions);
            this.ReservedActions.Add(action);

            return true;
        }

        /// <summary>
        /// アイテムをぶつけられたときの処理
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <param name="thrower">投げた人</param>
        public List<CharacterAction> HitItem(Item item, Character thrower)
        {
            var actions = new List<CharacterAction>();

            //アイテム種類ごとの効果
            if (item.Category == ItemCategory.Weapon
                || item.Category == ItemCategory.Arrow)
            {
                var damage = Mathf.Max(1, (int)((thrower.Atk + item.Powor - this.Def / 2) * UnityEngine.Random.Range(0.95f, 1.05f)));
                this.ReduceHP(damage);
                actions.Add(new CharacterDamagedAction(this, thrower, damage, true, this.IsDeath));
            }
            else
            {
                //デフォルトは固定ダメージ
                var damage = 1;
                this.ReduceHP(damage);
                actions.Add(new CharacterDamagedAction(this, thrower, damage, true, this.IsDeath));
            }

            return actions;
        }

        public virtual void SetPosition(Form position)
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

            Presenter.transform.localPosition = new Vector3(Position.x * DungeonConstants.MAPTIP_PIXCEL_SIZE + 2, Position.y * -DungeonConstants.MAPTIP_PIXCEL_SIZE, 0);
            this.Presenter.Sprite.sortingOrder = 500 + this.Position.y;
        }

        /// <summary>
        /// 移動アニメーション
        /// </summary>
        /// <returns></returns>
        public IObservable<Unit> MovingAnimation()
        {
            var actions = ReservedActions.Select(x => x as CharacterMoveAction).Where(x => x != null).ToArray();
            if (!actions.Any())
                return Observable.ReturnUnit();
            var frame = DungeonConstants.MOVING_ANIMATION_FRAME / actions.Count() - Mathf.Clamp(actions.Count() - 1, 0, 1);//※２回行動の奴がいると何故かずれるので1f調整
            return actions.Select(x =>
            {
                return Observable.EveryUpdate()
                    .Take(frame)
                    .Do(i => this.Presenter.transform.localPosition = 
                        new Vector3(
                            (x.FromPosition.x + (x.DesPosition.x - x.FromPosition.x) * (float)(i + 1) / frame) * DungeonConstants.MAPTIP_PIXCEL_SIZE + 2, 
                            (x.FromPosition.y + (x.DesPosition.y - x.FromPosition.y) * (float)(i + 1) / frame) * -DungeonConstants.MAPTIP_PIXCEL_SIZE, 
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
        public IObservable<Unit> AllReservedActionAnimations()
        {
            var actions = ReservedActions.Where(x => x as CharacterMoveAction == null).ToArray();
            return ActionAnimations(actions);
        }

        private IObservable<Unit> ActionAnimations(CharacterAction[] actions)
        {
            if (!actions.Any())
                return Observable.ReturnUnit();
            var actionList = new List<IObservable<Unit>>();
            foreach (var action in actions)
            {
                //攻撃
                var attackAction = action as CharacterAttackAction;
                if (attackAction != null)
                {
                    actionList.Add(AttackAnimation(attackAction));
                    continue;
                }
                //拾う
                var pickupAction = action as CharacterItemPickupAction;
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
                //置く
                var putAction = action as CharacterItemPutAction;
                if (putAction != null)
                {
                    actionList.Add(Observable.Defer(() =>
                    {
                        if (putAction.IsSuccess)
                            StaticData.Message.ShowMessage(string.Format("{0}を置いた", putAction.TargetItem.Name), false);
                        else
                            StaticData.Message.ShowMessage(string.Format("ここには置けない"), false);
                        return Observable.ReturnUnit();
                    }));
                    continue;
                }
                //投げる
                var throwAction = action as CharacterThrowAction;
                if (throwAction != null)
                {
                    actionList.Add(ThrowAnimation(throwAction));
                    continue;
                }
                //飲む
                var drinkAction = action as CharacterDrinkPotionAction;
                if(drinkAction != null)
                {
                    actionList.Add(Observable.Defer(() =>
                    {
                        StaticData.Message.ShowMessage(string.Format("{0}を飲んだ", drinkAction.TargetItem.Name), false);
                        return Observable.ReturnUnit();
                    })
                    .SelectMany(ActionAnimations(drinkAction.SubActions.ToArray()))
                    .Last());
                    continue;
                }
                //メッセージ
                var messageAction = action as MessageAction;
                if (messageAction != null)
                {
                    actionList.Add(Observable.Defer(() =>
                    {
                        StaticData.Message.ShowMessage(messageAction.Message, messageAction.IsWait);
                        return Observable.ReturnUnit();
                    }));
                    continue;
                }
                //該当なし
                actionList.Add(Observable.ReturnUnit());
            }
            return actionList.Concat().Last();
        }

        /// <summary>
        /// 攻撃アニメーション
        /// </summary>
        private IObservable<Unit> AttackAnimation(CharacterAttackAction action)
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
                            this.Position.x * DungeonConstants.MAPTIP_PIXCEL_SIZE + 2 + move.x,
                            this.Position.y * -DungeonConstants.MAPTIP_PIXCEL_SIZE - move.y,
                            0);
                })
                .Last()
                .AsUnitObservable();
            var damageAnimation = Observable.TimerFrame(4)
                .SelectMany(_ =>
                {
                    return action.SubActions
                        .Where(x => x as CharacterDamagedAction != null)
                        .Select(x => DamageAnimation(x as CharacterDamagedAction))
                        .Concat();
                });
            var otherAnimation = ActionAnimations(action.SubActions.Where(x => x as CharacterDamagedAction == null).ToArray());

            return Observable.WhenAll(attackAnimation, damageAnimation)
                    .SelectMany(otherAnimation).Last();
        }

        /// <summary>
        /// アイテムを投げるアクション
        /// </summary>
        public IObservable<Unit> ThrowAnimation(CharacterThrowAction action)
        {
            var dis = action.TargetPosition - action.Character.Position;
            var basePosition = this.Presenter.transform.localPosition;
            var noGameObject = action.TargetItem.Presenter == null;
            var frame = Mathf.Max(Mathf.Abs(dis.x), Mathf.Abs(dis.y)) * 3 + 2;
            var throwAction = Observable.Defer(() =>
                {
                    if (noGameObject)
                    {
                        //オブジェクトがない場合は表示用に生成
                        action.TargetItem.InstantiateObject(dungeon.DungeonPrefabs.ObjectPrefab, dungeon.ObjectRoot);
                    }
                    action.TargetItem.Presenter.transform.localPosition = basePosition;
                    return Observable.EveryUpdate();
                })
                .Take(frame)
                .Do(i =>
                {
                    // アイテムが飛んでくアニメーション
                    action.TargetItem.Presenter.transform.localPosition = basePosition + new Vector3(dis.x * DungeonConstants.MAPTIP_PIXCEL_SIZE * i / frame, -dis.y * DungeonConstants.MAPTIP_PIXCEL_SIZE * i / frame);
                })
                .Last()
                .Do(_ =>
                {
                    if (noGameObject)
                    {
                        //表示用に作ったやつの後片付け
                        GameObject.Destroy(action.TargetItem.Presenter.gameObject);
                    }
                    else
                    {
                        //オブジェクトがあるのは床落ちなので表示位置リセットして終了
                        action.TargetItem.ResetViewPotition();
                    }
                })
                .AsUnitObservable();
            var damageAnimation = action.SubActions
                        .Where(x => x as CharacterDamagedAction != null)
                        .Select(x => DamageAnimation(x as CharacterDamagedAction))
                        .Concat();
            var otherAnimation = ActionAnimations(action.SubActions.Where(x => x as CharacterDamagedAction == null).ToArray());

            return throwAction.Concat(damageAnimation).Last()
                    .SelectMany(otherAnimation).Last();
        }

        /// <summary>
        /// 被ダメージアニメーション
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IObservable<Unit> DamageAnimation(CharacterDamagedAction action)
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
                        if (action.Character as Player != null)
                        {
                            dungeon.StatusUpdateEventTrigger.OnNext(Unit.Default);
                            StaticData.Message.ShowMessage(string.Format("{0}は{1}から{2}のダメージを受けた", action.Character.Name, action.Attacker.Name, action.Point), false);
                        }
                        else
                            StaticData.Message.ShowMessage(string.Format("{0}は{1}に{2}のダメージを与えた", action.Attacker.Name, action.Character.Name, action.Point), false);
                    }
                })
                .Last()
                .Do(x =>
                {
                    //死亡
                    if (action.IsDeath)
                    {
                        action.Character.spriteAnimationDisposable.Dispose();
                        GameObject.Destroy(action.Character.Presenter.gameObject);//TODO: リサイクルするなら初期化してプールに移動
                        if (action.Character as Player != null)
                        {
                            StaticData.Message.ShowMessage(string.Format("{0}は力尽きた…", action.Character.Name), false);
                        }
                        else
                        {
                            StaticData.Message.ShowMessage(string.Format("{0}をやっつけた", action.Character.Name), false);
                        }
                    }
                })
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
        /// １回の行動ループ終了時の後処理
        /// </summary>
        public virtual void TurnEndEvent()
        {
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

        /// <summary>
        /// 現在いる部屋を取得する
        /// </summary>
        public Room GetCurrentRoom()
        {
            return dungeon.Rooms.Where(x => x.OnRoom(this.Position)).FirstOrDefault();
        }

        /// <summary>
        /// 現在いる位置が外周を含めた範囲にある部屋を取得する
        /// </summary>
        public Room GetCurrentRoomAround()
        {
            return dungeon.Rooms.Where(x => x.OnRoomAround(this.Position)).FirstOrDefault();
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
    public class CharacterMoveAction : CharacterAction
    {
        //移動先
        public Form FromPosition;
        public Form DesPosition;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="des">移動先座標</param>
        public CharacterMoveAction(Character self, Form from, Form des) : base(self)
        {
            FromPosition = from;
            DesPosition = des;
        }
    }

    /// <summary>
    /// 攻撃アクション
    /// </summary>
    public class CharacterAttackAction : CharacterAction
    {
        //向き
        public Form Direction;
        public List<CharacterAction> SubActions { get; private set; }

        public CharacterAttackAction(Character self, Form dir) : base(self)
        {
            Direction = dir;
            SubActions = new List<CharacterAction>();
        }
    }

    /// <summary>
    /// アイテムを拾うアクション
    /// </summary>
    public class CharacterItemPickupAction : CharacterAction
    {
        //対象のアイテム
        public Item TargetItem;
        public bool IsSuccess;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="item">対象のアイテム</param>
        /// <param name="isSuccess">拾えたかどうか</param>
        public CharacterItemPickupAction(Character self, Item item, bool isSuccess) : base(self)
        {
            TargetItem = item;
            IsSuccess = isSuccess;
        }
    }

    /// <summary>
    /// アイテムを置くアクション
    /// </summary>
    public class CharacterItemPutAction : CharacterAction
    {
        //対象のアイテム
        public Item TargetItem;
        public bool IsSuccess;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="item">対象のアイテム</param>
        /// <param name="isSuccess">置けたかどうか</param>
        public CharacterItemPutAction(Character self, Item item, bool isSuccess) : base(self)
        {
            TargetItem = item;
            IsSuccess = isSuccess;
        }
    }

    /// <summary>
    /// アイテムを投げるアクション
    /// </summary>
    public class CharacterThrowAction : CharacterAction
    {
        public Item TargetItem;
        public Form TargetPosition; 
        public List<CharacterAction> SubActions { get; private set; }

        public CharacterThrowAction(Character self, Item item, Form targetPosition) : base(self)
        {
            TargetItem = item;
            TargetPosition = targetPosition;
            SubActions = new List<CharacterAction>();
        }
    }

    /// <summary>
    /// ポーションを使うアクション
    /// </summary>
    public class CharacterDrinkPotionAction : CharacterAction
    {
        public Item TargetItem;
        public List<CharacterAction> SubActions { get; private set; }
        public CharacterDrinkPotionAction(Character self, Item item) : base(self)
        {
            TargetItem = item;
            SubActions = new List<CharacterAction>();
        }
    }

    /// <summary>
    /// メッセージを表示するだけのアクション
    /// </summary>
    public class MessageAction : CharacterAction
    {
        public string Message;
        public bool IsWait;
        public MessageAction(Character self, string message, bool isWait) : base(self)
        {
            Message = message;
            IsWait = isWait;
        }
    }

    /// <summary>
    /// ダメージを受けたアクション
    /// </summary>
    public class CharacterDamagedAction : CharacterAction
    {
        public Character Attacker { get; private set; }
        public int Point { get; private set; }
        public bool IsHit { get; private set; }
        public bool IsDeath { get; private set; }

        public CharacterDamagedAction(Character self, Character attacker, int point, bool isHit, bool isDeath) : base(self)
        {
            this.Attacker = attacker;
            this.Point = point;
            this.IsHit = isHit;
            this.IsDeath = isDeath;
        }
    }

    /// <summary>
    /// ステータス変化クラス
    /// </summary>
    public class CharacterStatusEffect
    {
        public StatusEffectType Type { get; set; }
        public int Value { get; set; }
    }

    public class DirectionManager
    {
        public Dictionary<int, Form> Directions;

        public DirectionManager()
        {
            Directions = new Dictionary<int, Form>();
            Directions.Add(0, new Form(1, 0));
            Directions.Add(1, new Form(1, 1));
            Directions.Add(2, new Form(0, 1));
            Directions.Add(3, new Form(-1, 1));
            Directions.Add(4, new Form(-1, 0));
            Directions.Add(5, new Form(-1, -1));
            Directions.Add(6, new Form(0, -1));
            Directions.Add(7, new Form(1, -1));
        }

        public int GetId(Form direction)
        {
            return Directions.First(x => x.Value == direction).Key;
        }

        /// <summary>
        /// 受け取った方向を基点としてその周囲の方向を近い方から順に一覧として取得する
        /// </summary>
        public List<Form> GetDirectionsForSearch(Form direction, bool isClockwise)
        {
            var baseId = GetId(direction);
            var list = new List<Form>();
            var coef = isClockwise ? 1 : -1;

            list.Add(direction);
            for (int i = 1; i < 4; i++)
            {
                list.Add(Directions[(baseId + i * coef + 8) % 8]);
                list.Add(Directions[(baseId - i * coef + 8) % 8]);
            }
            list.Add(Directions[(baseId + 4) % 8]);//真後

            return list;
        }
    }
}

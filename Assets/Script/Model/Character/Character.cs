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
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int Speed { get; set; }
        public int ActionWait { get; set; }

        protected const int MAX_WAIT = 120;
        private static int lastId = 0;
        protected Dungeon dungeon { get; set; }

        public abstract CharacterType Type { get; }
        private List<CharacterAction> ReservedActions;
        public CharacterObject Object { get; set; }

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
            Object = GameObject.Instantiate(prefab).GetComponent<CharacterObject>();
            Object.transform.SetParent(characterRoot, false);
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
                ReservedActions.Add(new CharacterMove(this.Position, destination));//移動情報を登録
                SetPosition(destination);
                return true;
            }
            return false;
        }

        public bool Attack()
        {
            ReservedActions.Add(new CharacterAttack(this.Direction));//攻撃情報を登録
            //TODO: 攻撃処理

            return true;
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
            if (Object == null) return;

            Object.transform.localPosition = new Vector3(Position.x * 32 + 2, Position.y * -32, 0);
            this.Object.Sprite.sortingOrder = 500 + this.Position.y;
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
                    .Do(i => this.Object.transform.localPosition = 
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
                if (action as CharacterAttack != null)
                    actionList.Add(AttackAnimation(action as CharacterAttack));
            }
            return actionList.Concat();
        }

        /// <summary>
        /// 攻撃アニメーション
        /// </summary>
        private IObservable<Unit> AttackAnimation(CharacterAttack action)
        {
            var frame = 16;
            return Observable.EveryUpdate()
                .Take(frame)
                .Do(i =>
                {
                    var dis = 0;
                    if (i < 2) dis = 3 * (int)i;
                    else if (i >= frame - 3) dis = 3 * (int)(frame - 1 - i);
                    else dis = 3 * 2;
                    var move = action.Direction * dis;
                    this.Object.transform.localPosition =
                        new Vector3(
                            this.Position.x * 32 + 2 + move.x,
                            this.Position.y * -32 - move.y,
                            0);
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
            Object.Sprite.sprite = sprites[spriteAnimationCurrentIndex + GetSpriteIndexByDirection()];
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
    public interface CharacterAction
    {
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
        public CharacterMove(Form from, Form des)
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

        public CharacterAttack(Form dir)
        {
            Direction = dir;
        }
    }
}

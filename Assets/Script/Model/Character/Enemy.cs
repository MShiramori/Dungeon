using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Model
{
    public class Enemy : Character
    {
        protected override CharacterParams Params { get { return _params; } }
        private EnemyParams _params;
        public override CharacterType Type { get { return CharacterType.Enemy; } }
        public EnemyType EnemyType { get; set; }
        private Form destination;

        public Enemy(Dungeon _dungeon, EnemyType type) : base(_dungeon)
        {
            _params = new EnemyParams();
            EnemyType = type;

            //TODO: 種類ごとのパラメータ取得
            Params.Name = "Noname";
            HP = 10;
            MaxHP = 5;
            Speed = 8;
            Params.Str = 0;
            Params.Vit = 0;
            Params.Dex = 0;
            Params.Agi = 0;

            destination = new Form(-1, -1);
        }

        public override void SetPosition(Form position)
        {
            base.SetPosition(position);
            if (destination == new Form(-1, -1))
                this.destination = this.Position;
        }

        // 行動処理
        protected override void Action()
        {
            //TODO:ＡＩパターンで切り替え
            //視認判定、目標地点設定処理
            var isAdjacent = IsAdjacentPlayer();
            if (IsVisiblePlayerOnRoom() || isAdjacent)
            {
                //同じ部屋、または隣接地点にプレイヤーがいれば、そこを目的地として更新
                destination = dungeon.Player.Position;
            }
            else if (destination == this.Position)
            {
                //プレイヤーが視界におらず、目的地に到着していたら、とりあえず移動先を新たに設定
                if (false)
                {
                    //TODO:部屋にいるなら何れかの通路を目的地とする
                }
                else
                {
                    //周囲から移動可能な場所を探す
                    var des = this.Position;
                    var dirs = Character.DirectionManager.GetDirectionsForSearch(this.Direction, UnityEngine.Random.Range(0, 256) < 128);
                    foreach (var dir in dirs)
                    {
                        if (CanMove(this.Position + dir))
                        {
                            des = this.Position + dir;
                            break;
                        }
                    }
                    destination = des;
                }
            }

            //行動処理
            {
                //攻撃
                if (isAdjacent && !CheckWallForAction(destination))
                {
                    //隣接してたら攻撃
                    this.SetDirection(dungeon.Player.Position - this.Position);
                    this.Attack();
                    return;
                }
                //移動
                if (destination != this.Position)
                {
                    //移動可能な場所を探す
                    var tempDir = (destination - this.Position).GetDirection();
                    var dirs = Character.DirectionManager.GetDirectionsForSearch(tempDir, UnityEngine.Random.Range(0, 256) < 128).Take(3);
                    foreach (var dir in dirs)
                    {
                        if (CanMove(this.Position + dir))
                        {
                            tempDir = dir;
                            break;
                        }
                    }
                    if (this.Move(this.Position + tempDir) && (IsAdjacentPlayer() || IsVisiblePlayerOnRoom()))
                    {
                        //移動後にプレイヤーが視認可能な位置に来ていたら目的地更新
                        destination = dungeon.Player.Position;
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// プレイヤーと隣接しているか判定
        /// </summary>
        private bool IsAdjacentPlayer()
        {
            var tempDes = dungeon.Player.Position - this.Position;
            return tempDes.AbsTotal == 1 || tempDes.AbsMultiple == 1;
        }

        /// <summary>
        /// 今いる部屋からプレイヤーが見える
        /// </summary>
        private bool IsVisiblePlayerOnRoom()
        {
            var currentRoom = this.GetCurrentRoom();
            return currentRoom != null && dungeon.Player.GetCurrentRoomAround() == currentRoom;
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

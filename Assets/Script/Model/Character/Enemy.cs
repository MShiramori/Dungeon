using Assets.Script.Database;
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
        public EnemyId EnemyId { get; set; }
        public EnemyMaster Master { get; set; }
        public int HasExp { get { return Master.HasExp; } }

        private Form destination;
        //速度と行動回数が違うキャラ用
        private int actionCount;
        private bool isActioned { get { return Master.MaxActionCount != 0 && actionCount >= Master.MaxActionCount; } }//未設定(0)の時は使用しない

        public Enemy(Dungeon _dungeon, EnemyId id) : base(_dungeon)
        {
            _params = new EnemyParams();
            EnemyId = id;
            Master = DataBase.EnemyMasters[id];

            //マスタからパラメータ取得
            Params.Name = Master.Name;
            HP = Master.HP;
            MaxHP = Master.HP;
            Speed = Master.Speed;
            Params.Str = Master.Str;
            Params.Vit = Master.Vit;
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
                var room = this.GetCurrentRoom();
                if (room != null)
                {
                    //部屋にいるなら何れかの通路を目的地とする
                    var points = new List<Form>();
                    for (int x = Mathf.Max(0, room.Position.x - 1); x <= Mathf.Min(dungeon.MapSize.x, room.EndPosition.x + 1); x++)
                    {
                        for (int y = Mathf.Max(0, room.Position.y - 1); y <= Mathf.Min(dungeon.MapSize.y, room.EndPosition.y + 1); y++)
                        {
                            if (dungeon.MapData[x, y].Terra == Enums.Terrain.Passage)
                            {
                                points.Add(new Form(x, y));
                            }
                        }
                    }
                    if (points.Any())
                    {
                        //入ってきた通路以外に通路があればランダムで選択、なければ戻る
                        var back = this.Position + this.Direction * -1;
                        var tmpTargets = points.Where(x => x != back).ToArray();
                        if (tmpTargets.Any())
                        {
                            destination = tmpTargets[UnityEngine.Random.Range(0, tmpTargets.Length)];
                        }
                        else
                        {
                            destination = back;
                        }
                    }
                }
                else
                {
                    //周囲から移動可能な場所を探す
                    var des = this.Position;
                    var dirs = Character.DirectionManager.GetDirectionsForSearch(this.Direction, UnityEngine.Random.Range(0, 256) < 128);
                    dirs = dirs.Take(5).Shuffle().Concat(dirs.Skip(5)).ToList();
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
                //行動済み判定
                if (isActioned)
                {
                    return;
                }
                //隣接時行動
                if (isAdjacent && !CheckWallForAction(destination))
                {
                    //隣接してたら攻撃
                    this.SetDirection(dungeon.Player.Position - this.Position);
                    this.Attack();
                    this.actionCount++;
                    return;
                }
                //移動
                if (destination != this.Position)
                {
                    //移動可能な場所を探す
                    var tempDir = (destination - this.Position).GetDirection();
                    var dirs = Character.DirectionManager.GetDirectionsForSearch(tempDir, UnityEngine.Random.Range(0, 256) < 128).Take(5);
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
        /// １回の行動ループ終了時の後処理
        /// </summary>
        public override void TurnEndEvent()
        {
            actionCount = 0;
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

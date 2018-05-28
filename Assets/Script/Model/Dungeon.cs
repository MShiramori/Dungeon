using Assets.Script.Enums;
using Assets.Script.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Model
{
    public class Dungeon
    {
        public Form MapSize { get; private set; }
        public MapCell[,] MapData { get; private set; }
        public Room[] Rooms { get; private set; }
        public List<MapObject> Objects { get; private set; }
        public List<Character> Characters { get; private set; }
        public Character Player { get { return Characters.FirstOrDefault(x => x.Type == CharacterType.Player); } }

        public int Floor { get; private set; }

        public Camera MainCamera { get; set; }

        public Dungeon()
        {
            this.Objects = new List<MapObject>();
            this.Characters = new List<Character>();
            ResetMap();
            this.Floor = 1;
        }

        public void ResetMap()
        {
            this.Objects.Clear();
            this.Characters.Clear();

            var generator = new DungeonMapGenerator();
            generator.CreateMap(new Form(10, 10), new Form(4, 4), UnityEngine.Random.Range(5, 10));

            this.MapSize = generator.MapSize;
            this.MapData = generator.Map;
            this.Rooms = generator.Rooms;

            //階段生成
            AddObject(new Step(this) { IsNext = true });

            //プレイヤー生成
            AddCharacter(new Player(this));

            //モンスター生成
            var enemyCnt = UnityEngine.Random.Range(3, 6);
            for (int i = 0; i < enemyCnt; i++)
            {
                AddCharacter(new Enemy(this, EnemyType.ゴブリン));
            }
        }

        //MapObjectをID採番して追加
        public bool AddObject(MapObject obj, Form position)
        {
            //既に何かあったら失敗
            if (Objects.Any(x => x.Position == position))
                return false;
            var id = Objects.Any() ? Objects.Max(x => x.UniqueId) + 1 : 0;
            obj.UniqueId = id;
            obj.Position = position;
            Objects.Add(obj);
            return true;
        }

        //MapObjectを位置ランダム指定＆ID採番して追加
        public bool AddObject(MapObject obj)
        {
            //全てのマスのうち、部屋かつすでにオブジェクトがない場所を取得
            var cell = GetObjectFreeCell();
            if (cell != null)
            {
                return AddObject(obj, cell.Position);
            }
            return false;
        }

        public void AddCharacter(Character chara)
        {
            var cell = GetCharacterFreeCell();
            if (cell != null)
            {
                chara.SetPosition(cell.Position);
                chara.SetDirection(new Form((int)UnityEngine.Random.Range(-1, 2), (int)UnityEngine.Random.Range(-1, 2)));
                Characters.Add(chara);
            }
        }

        /// <summary>
        /// 全てのマスのうち、部屋かつすでにオブジェクトがない場所を取得
        /// </summary>
        /// <returns></returns>
        public MapCell GetObjectFreeCell()
        {
            return MapData.Cast<MapCell>()
                    .Where(x => x.Terra == Enums.Terrain.Room && !Objects.Any(y => y.Position == x.Position))
                    .WaitedSample(x => 1);
        }

        /// <summary>
        /// 全てのマスのうち、部屋かつすでにキャラクターがいない場所を取得
        /// </summary>
        /// <returns></returns>
        public MapCell GetCharacterFreeCell()
        {
            return MapData.Cast<MapCell>()
                    .Where(x => x.Terra == Enums.Terrain.Room && !Characters.Any(y => y.Position == x.Position))
                    .WaitedSample(x => 1);
        }

        public void UpdateFloor(int floor)
        {
            this.Floor = floor;
        }
    }
}

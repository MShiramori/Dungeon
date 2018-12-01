using Assets.Script.Components;
using Assets.Script.Enums;
using Assets.Script.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;
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
        public Player Player { get { return Characters.FirstOrDefault(x => x.Type == CharacterType.Player) as Player; } }

        public int Floor { get; private set; }

        public Camera MainCamera { get; set; }
        public Transform MapRoot { get; set; }
        public Transform ObjectRoot { get; set; }
        public Transform CharacterRoot { get; set; }
        public DungeonPrefabs DungeonPrefabs { get; set; }

        public Subject<Unit> StatusUpdateEventTrigger { get; private set; }

        public Dungeon()
        {
            this.Objects = new List<MapObject>();
            this.Characters = new List<Character>();
            this.StatusUpdateEventTrigger = new Subject<Unit>();
        }

        public void Initialize()
        {
            this.Floor = 1;
            ResetMap();
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

            //フロア情報取得
            var floorInfo = Database.DataBase.FloorMasters[this.Floor];

            //アイテム生成
            var itemCnt = UnityEngine.Random.Range(4, 7);
            for (int i = 0; i < itemCnt; i++)
            {
                //フロア情報から抽選
                var rotItem = floorInfo.ItemTable.WaitedSample(x => x.Rate);
                var count = rotItem.CountFrom > 0 ? UnityEngine.Random.Range(rotItem.CountFrom, rotItem.CountTo) : 0;
                AddObject(new Item(this, rotItem.ItemId, count));
            }

            //プレイヤー生成
            AddCharacter(new Player(this));

            //モンスター生成
            var enemyCnt = UnityEngine.Random.Range(3, 6);
            for (int i = 0; i < enemyCnt; i++)
            {
                //フロア情報から抽選
                AddCharacter(new Enemy(this, floorInfo.EnemyTable.WaitedSample(x => x.Rate).Id));
            }

            //TODO:削除しないでオブジェクトのリサイクル
            //オブジェクトを全削除
            var cells = MapRoot.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < cells.Length; i++) GameObject.Destroy(cells[i].gameObject);
            var charas = CharacterRoot.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < charas.Length; i++) GameObject.Destroy(charas[i].gameObject);
            var objects = ObjectRoot.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < objects.Length; i++) GameObject.Destroy(objects[i].gameObject);

            //プレイヤー＆敵オブジェクト配置
            foreach (var chara in this.Characters)
            {
                chara.InstantiateObject(DungeonPrefabs.PlayerPrefab, CharacterRoot);
            }

            //その他オブジェクト配置
            foreach (var obj in this.Objects)
            {
                obj.InstantiateObject(DungeonPrefabs.ObjectPrefab, ObjectRoot);
            }

            //マップ描画
            CreateMap();
        }

        /// <summary>
        /// マップ描画
        /// </summary>
        private void CreateMap()
        {
            for (int x = 0; x < this.MapSize.x; x++)
            {
                for (int y = 0; y < this.MapSize.y; y++)
                {
                    GameObject obj;
                    if (this.MapData[x, y].Terra == Enums.Terrain.Wall)
                        obj = GameObject.Instantiate(DungeonPrefabs.WallPrefab);
                    else if (this.MapData[x, y].Terra == Enums.Terrain.Passage)
                        obj = GameObject.Instantiate(DungeonPrefabs.PassagePrefab);
                    else
                        obj = GameObject.Instantiate(DungeonPrefabs.FloorPrefab);
                    obj.transform.SetParent(MapRoot, false);
                    obj.transform.localPosition = new Vector3(x * DungeonConstants.MAPTIP_PIXCEL_SIZE, y * -DungeonConstants.MAPTIP_PIXCEL_SIZE, 0);
                }
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
            StatusUpdateEventTrigger.OnNext(Unit.Default);
        }
    }
}

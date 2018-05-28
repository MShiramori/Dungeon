using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Model
{
    public class DungeonMapGenerator
    {
        public Form MapSize { get; private set; }
        public MapCell[,] Map { get; private set; }
        public Room[] Rooms { get; private set; }

        private List<Section> sections = new List<Section>();
        private List<Passage> passages = new List<Passage>();
        private Random rnd = new Random();

        public Dungeon CreateMap(Form sectionSize, Form SectionCount, int roomCount)
        {
            MapSize = sectionSize * SectionCount;

            //セクション生成
            for (int y = 0; y < SectionCount.y; y++)
            {
                for (int x = 0; x < SectionCount.x; x++)
                {
                    var pos = new Form(x, y);
                    sections.Add(new Section(x + y * SectionCount.x, pos, sectionSize));
                }
            }

            //ルーム生成
            var ids = sections.Select(x => x.ID).ToArray();
            var cnt = Math.Min(SectionCount.Count, roomCount);
            for (int i = 0; i < cnt; i++)
            {
                var id = ids[rnd.Next(0, ids.Count())];
                ids = ids.Where(x => x != id).ToArray();
                sections.First(x => x.ID == id).CreateRoom();
                //string.Format("room {0}_{1}", sections.First(x => x.ID == id).Position.x, sections.First(x => x.ID == id).Position.y).Dump();
            }

            //パス生成
            CreatePasses();

            //部屋をつなげて大部屋作る
            UnionRooms(rnd.Next(0, 5));

            //通路の位置設定
            InitializePassate();

            return CreateMap();
        }

        private bool CreatePasses()
        {
            if (sections.Where(x => x.Room != null && !x.Room.IsRelayPoint).Select(x => x.GroupId).Distinct().Count() <= 1)
                return true;

            //開始Section
            var start = sections.Where(x => x.Room != null && !x.Room.IsRelayPoint)
                                .OrderBy(x => x.RandomSeed)
                                .First();
            //目的地Section
            var goal = sections.Where(x => x.Room != null && !x.Room.IsRelayPoint && x.GroupId != start.GroupId)
                                .OrderBy(x => x.RandomSeed)
                                .First();

            //通路生成
            Dig(start, goal, ref passages);

            //goal側のGroupIdのSectionのGroupIdをすべてStartのものに上書き（繋がってる判定）
            var targets = sections.Where(x => x.GroupId == goal.GroupId).ToArray();
            foreach (var target in targets)
            {
                target.GroupId = start.GroupId;
            }

            //全部つながるまで再帰
            return CreatePasses();
        }

        //通路を生成
        private void Dig(Section current, Section goal, ref List<Passage> passages)
        {
            var dis = goal.Position - current.Position;
            var dir = dis.GetDirection();
            if (dir.AbsTotal > 1)
            {
                if (rnd.Next(2) < 1)
                    dir.x = 0;
                else
                    dir.y = 0;
            }
            var target = sections.First(x => x.Position == current.Position + dir);
            //ダブってなければ一覧追加
            if (!passages.Any(x => (x.From == current && x.To == target) || (x.From == target && x.To == current)))
            {
                passages.Add(new Passage(current, target));
            }
            if (target != goal)
            {
                Dig(target, goal, ref passages);
            }
        }

        //部屋をつなげて大部屋にする
        private void UnionRooms(int unionCount)
        {
            for (int i = 0; i < unionCount; i++)
            {
                var passes = passages
                            .Where(x => !x.From.Room.IsRelayPoint)
                            .ToArray();
                if (passes.Count() == 0)
                    break;
                var pass = passes[new Random().Next(0, passes.Count())];
                var parent = pass.From.Parent ?? pass.From;
                var toParent = pass.To.Parent ?? pass.To;
                var tmps = sections.Where(x => x == parent || x == toParent || x.Parent == parent || x.Parent == toParent).ToArray();
                var targets = GetAreaRangeSections(tmps);
                var newPosX = targets.Where(x => x.Room != null).Min(x => x.Room.Position.x);
                var newPosY = targets.Where(x => x.Room != null).Min(x => x.Room.Position.y);
                var newSizeX = targets.Where(x => x.Room != null).Max(x => x.Room.EndPosition.x) - newPosX + 1;
                var newSizeY = targets.Where(x => x.Room != null).Max(x => x.Room.EndPosition.y) - newPosY + 1;
                foreach (var target in targets)
                {
                    if (target != parent)
                    {
                        target.IsUnioned = true;
                        target.Parent = parent;
                    }
                }
                parent.Room.Position = new Form(newPosX, newPosY);
                parent.Room.Size = new Form(newSizeX, newSizeY);
                passages = passages.Where(x => !targets.Contains(x.From) || !targets.Contains(x.To)).ToList();
                //string.Format("{0}:{1} → {2}:{3}", pass.From.Position.x, pass.From.Position.y, pass.To.Position.x, pass.To.Position.y).Dump();
            }
        }

        //接続対象部屋抽出再帰処理
        private Section[] GetAreaRangeSections(Section[] array)
        {
            var rangeFrom = new Form(array.Min(x => x.Position.x), array.Min(x => x.Position.y));
            var rangeTo = new Form(array.Max(x => x.Position.x), array.Max(x => x.Position.y));
            var targets = sections.Where(x => x.Position.x >= rangeFrom.x
                                            && x.Position.y >= rangeFrom.y
                                            && x.Position.x <= rangeTo.x
                                            && x.Position.y <= rangeTo.y)
                                    .ToArray();
            targets = sections.Where(x => targets.Any(y => y == x || y.Parent == x || y == x.Parent)).ToArray();
            if (array.Count() == targets.Count())
                return targets;
            return GetAreaRangeSections(targets);
        }

        //通路のパラメータを設定
        private void InitializePassate()
        {
            foreach (var pass in passages)
            {
                pass.SetPositions();
            }
        }

        private Dungeon CreateMap()
        {
            var map = new MapCell[MapSize.x, MapSize.y];
            for (int y = 0; y < MapSize.y; y++)
            {
                for (int x = 0; x < MapSize.x; x++)
                {
                    var roomSec = sections
                        .Where(n => n.Room != null && !n.IsUnioned)
                        .FirstOrDefault(n => n.Room.OnRoom(new Form(x, y)));
                    var isPass = passages.Any(n => n.OnPassage(new Form(x, y)));
                    var t = Terrain.Wall;
                    if (roomSec != null)
                    {
                        if (!roomSec.Room.IsRelayPoint) t = Terrain.Room;
                        else t = Terrain.Passage;
                    }
                    else if (isPass) t = Terrain.Passage;
                    map[x, y] = new MapCell(new Form(x, y), t);
                }
            }
            var rooms = sections.Where(x => !x.IsUnioned && x.Room != null && !x.Room.IsRelayPoint)
                                .Select(x => x.Room)
                                .ToArray();
            var dungeon = new Dungeon(MapSize, map, rooms);

            //階段生成
            dungeon.AddObject(new Step() { IsNext = true });

            //プレイヤー生成
            dungeon.AddCharacter(new Player(dungeon));

            //モンスター生成
            var enemyCnt = new Random().Next(3, 6);
            for (int i = 0; i < enemyCnt; i++)
            {
                dungeon.AddCharacter(new Enemy(dungeon, EnemyType.ゴブリン));
            }

            return dungeon;
        }
    }
}

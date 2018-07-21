using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Model
{
    public class Section
    {
        public int ID { get; private set; }
        public Form Position { get; private set; }
        public Form Size { get; private set; }
        public Room Room { get; private set; }
        public bool IsUnioned { get; set; }
        public Section Parent { get; set; }

        //ランダム抽選用
        public int RandomSeed { get { return new Random(Environment.TickCount + ID).Next(10000); } }

        //通路接続用
        public int GroupId { get; set; }

        public Section(int id, Form position, Form size)
        {
            this.ID = id;
            this.Position = position;
            this.Size = size;
            this.GroupId = id;
            this.IsUnioned = false;
            this.Parent = null;
        }

        //部屋
        public void CreateRoom()
        {
            var rnd = new Random(Environment.TickCount + ID);
            var roomSize = new Form(rnd.Next(Size.x / 3, Size.x - 3), rnd.Next(Size.y / 3, Size.y - 3));
            var localPosition = new Form(rnd.Next(2, Size.x - roomSize.x - 1), rnd.Next(2, Size.y - roomSize.y - 1));
            Room = new Room(Position * Size + localPosition, roomSize, false);
        }

        //中継点
        public void CreateRelayPoint()
        {
            var rnd = new Random(Environment.TickCount + ID);
            var localPosition = new Form(rnd.Next(2, Size.x - 1), rnd.Next(2, Size.y - 1));
            Room = new Room(Position * Size + localPosition, new Form(1, 1), true);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Model
{
    public class Passage
    {
        public Section From { get; set; }
        public Section To { get; set; }
        public Form FromPosition { get; set; }
        public Form ToPosition { get; set; }
        //public int CornerDistance { get; set;}

        public Passage(Section section1, Section section2)
        {
            //検索めんどくなるのでID小さいほうが必ずfrom
            this.From = section1.ID < section2.ID ? section1 : section2;
            this.To = section1.ID < section2.ID ? section2 : section1;
            //Roomがなければ中継点を生成
            if (From.Room == null) { From.CreateRelayPoint(); }
            if (To.Room == null) { To.CreateRelayPoint(); }
        }

        //始点と終点を生成
        public void SetPositions()
        {
            //始点・終点生成
            var rnd = new Random();
            var distance = To.Position - From.Position;
            if (distance.AbsTotal != 1)
            {
                throw new Exception("隣の部屋じゃないとつなげないよ");
            }
            FromPosition = new Form(0, 0);
            ToPosition = new Form(0, 0);
            if (distance.y == 0)
            {
                FromPosition.x = distance.x > 0 ? From.Room.OriginEndPosition.x + 1 : From.Room.OriginPosition.x - 1;
                FromPosition.y = rnd.Next(From.Room.OriginPosition.y, From.Room.OriginEndPosition.y);
                ToPosition.x = distance.x > 0 ? To.Room.OriginPosition.x - 1 : To.Room.OriginEndPosition.x + 1;
                ToPosition.y = rnd.Next(To.Room.OriginPosition.y, To.Room.OriginEndPosition.y);
                //CornerDistance = rnd.Next(Math.Min(2, Math.Abs(ToPosition.x - FromPosition.x) - 1), Math.Max(2, Math.Abs(ToPosition.x - FromPosition.x) - 1));
            }
            else if (distance.x == 0)
            {
                FromPosition.x = rnd.Next(From.Room.OriginPosition.x, From.Room.OriginEndPosition.x);
                FromPosition.y = distance.y > 0 ? From.Room.OriginEndPosition.y + 1 : From.Room.OriginPosition.y - 1;
                ToPosition.x = rnd.Next(To.Room.OriginPosition.x, To.Room.OriginEndPosition.x);
                ToPosition.y = distance.y > 0 ? To.Room.OriginPosition.y - 1 : To.Room.OriginEndPosition.y + 1;
                //CornerDistance = rnd.Next(Math.Min(2, Math.Abs(ToPosition.y - FromPosition.y) - 1), Math.Max(2, Math.Abs(ToPosition.y - FromPosition.y) - 1));
            }
        }

        public bool OnPassage(Form point)
        {
            if (
                point.x < Math.Min(FromPosition.x, ToPosition.x)
                || point.x > Math.Max(FromPosition.x, ToPosition.x)
                || point.y < Math.Min(FromPosition.y, ToPosition.y)
                || point.y > Math.Max(FromPosition.y, ToPosition.y)
                )
            {
                return false;
            }

            if (From.Position.x != To.Position.x)
            {
                //横通路
                //var c = Math.Min(FromPosition.x, ToPosition.x) + CornerDistance;
                var c = (From.Position.x > To.Position.x ? From.Position.x : To.Position.x) * From.Size.x;
                if (point.x < c)
                    return point.y == FromPosition.y;
                else if (point.x > c)
                    return point.y == ToPosition.y;
                else
                    return point.x == c;
            }
            else
            {
                //縦通路
                //var c = Math.Min(FromPosition.y, ToPosition.y) + CornerDistance;
                var c = (From.Position.y > To.Position.y ? From.Position.y : To.Position.y) * From.Size.y;
                if (point.y < c)
                    return point.x == FromPosition.x;
                else if (point.y > c)
                    return point.x == ToPosition.x;
                else
                    return point.y == c;
            }
        }
    }
}

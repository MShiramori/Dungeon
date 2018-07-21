using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Model
{
    public class Room
    {
        public Form Position { get; set; }
        public Form Size { get; set; }
        public Form EndPosition { get { return Position + Size - new Form(1, 1); } }
        public bool IsRelayPoint { get; private set; }//通路の中継地点か

        public Form OriginPosition { get; private set; }
        public Form OriginSize { get; private set; }
        public Form OriginEndPosition { get { return OriginPosition + OriginSize - new Form(1, 1); } }

        public Room(Form position, Form size, bool isRelay)
        {
            this.Position = position;
            this.Size = size;
            this.IsRelayPoint = isRelay;
            this.OriginPosition = position;
            this.OriginSize = size;
        }

        public bool OnRoom(Form point)
        {
            return point.x >= Position.x && point.x <= EndPosition.x && point.y >= Position.y && point.y <= EndPosition.y;
        }
    }
}

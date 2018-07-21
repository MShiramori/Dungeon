using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Model
{
    public class Form
    {
        public int x { get; set; }
        public int y { get; set; }

        public int Count { get { return x * y; } }
        public int AbsTotal { get { return Math.Abs(x) + Math.Abs(y); } }

        public Form(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Form operator +(Form z, Form w)
        {
            return new Form(z.x + w.x, z.y + w.y);
        }

        public static Form operator -(Form z, Form w)
        {
            return new Form(z.x - w.x, z.y - w.y);
        }

        public static Form operator *(Form z, Form w)
        {
            return new Form(z.x * w.x, z.y * w.y);
        }

        public static Form operator *(Form z, int w)
        {
            return new Form(z.x * w, z.y * w);
        }

        public static bool operator ==(Form z, Form w)
        {
            return z.x == w.x && z.y == w.y;
        }

        public static bool operator !=(Form z, Form w)
        {
            return z.x != w.x || z.y != w.y;
        }

        public override bool Equals(object obj)
        {
            return this == (Form)obj;
        }

        public override int GetHashCode()
        {
            return this.x + this.Count;
        }

        public Form GetDirection()
        {
            var dirx = x < 0 ? -1 : x > 0 ? 1 : 0;
            var diry = y < 0 ? -1 : y > 0 ? 1 : 0;
            return new Form(dirx, diry);
        }
    }
}

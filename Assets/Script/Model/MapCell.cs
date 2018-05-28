using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Model
{
    public class MapCell
    {
        public Form Position { get; set; }
        public Terrain Terra { get; set; }

        public MapCell(Form position, Terrain terra)
        {
            this.Position = position;
            this.Terra = terra;
        }
    }
}

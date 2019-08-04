using Assets.Script.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Components
{
    public class MapCellPresenter : MonoBehaviour
    {
        public SpriteRenderer[] Sprite;

        public MapCell Cell { get; private set; }

        private Dungeon dungeon;
        private Dictionary<string, Sprite> tips;
        private string typeText;

        public void Initialize(Dungeon _dungeon, MapCell cell)
        {
            this.dungeon = _dungeon;
            this.Cell = cell;

            //TODO: ダンジョンタイプで取得セットを判定
            typeText = "001";
            this.tips = Resources.LoadAll<Sprite>(string.Format("Textures/TileSet/" + typeText))
                .ToDictionary(x => x.name, x => x);
            UpdateImage();
        }

        public void UpdateImage()
        {
            if (Cell.Terra == Enums.Terrain.Wall)
            {
                var up = Cell.Position.y == 0 ? Enums.Terrain.Wall : dungeon.MapData[Cell.Position.x, Cell.Position.y - 1].Terra;
                var down = Cell.Position.y >= dungeon.MapSize.y - 1 ? Enums.Terrain.Wall : dungeon.MapData[Cell.Position.x, Cell.Position.y + 1].Terra;
                var left = Cell.Position.x == 0 ? Enums.Terrain.Wall : dungeon.MapData[Cell.Position.x - 1, Cell.Position.y].Terra;
                var right = Cell.Position.x >= dungeon.MapSize.x - 1 ? Enums.Terrain.Wall : dungeon.MapData[Cell.Position.x + 1, Cell.Position.y].Terra;
                var upleft = Cell.Position.y == 0 || Cell.Position.x == 0 ? Enums.Terrain.Wall : dungeon.MapData[Cell.Position.x - 1, Cell.Position.y - 1].Terra;
                var upright = Cell.Position.y == 0 || Cell.Position.x >= dungeon.MapSize.x - 1 ? Enums.Terrain.Wall : dungeon.MapData[Cell.Position.x + 1, Cell.Position.y - 1].Terra;
                var downleft = Cell.Position.y >= dungeon.MapSize.y - 1 || Cell.Position.x == 0 ? Enums.Terrain.Wall : dungeon.MapData[Cell.Position.x - 1, Cell.Position.y + 1].Terra;
                var downright = Cell.Position.y >= dungeon.MapSize.y - 1 || Cell.Position.x >= dungeon.MapSize.x - 1 ? Enums.Terrain.Wall : dungeon.MapData[Cell.Position.x + 1, Cell.Position.y + 1].Terra;

                string type = "a";

                if (up == Enums.Terrain.Wall && left == Enums.Terrain.Wall)
                {
                    if (upleft == Enums.Terrain.Wall) type = "a";
                    else type = "b";
                }
                else if (up != Enums.Terrain.Wall && left != Enums.Terrain.Wall) type = "c";
                else if (up != Enums.Terrain.Wall && left == Enums.Terrain.Wall) type = "d";
                else if (up == Enums.Terrain.Wall && left != Enums.Terrain.Wall) type = "e";
                this.Sprite[0].sprite = tips[typeText + "_w_" + 0 + type];

                if (up == Enums.Terrain.Wall && right == Enums.Terrain.Wall)
                {
                    if (upright == Enums.Terrain.Wall) type = "a";
                    else type = "b";
                }
                else if (up != Enums.Terrain.Wall && right != Enums.Terrain.Wall) type = "d";
                else if (up != Enums.Terrain.Wall && right == Enums.Terrain.Wall) type = "c";
                else if (up == Enums.Terrain.Wall && right != Enums.Terrain.Wall) type = "f";
                this.Sprite[1].sprite = tips[typeText + "_w_" + 1 + type];

                if (down == Enums.Terrain.Wall && left == Enums.Terrain.Wall)
                {
                    if (downleft == Enums.Terrain.Wall) type = "a";
                    else type = "b";
                }
                else if (down != Enums.Terrain.Wall && left != Enums.Terrain.Wall) type = "e";
                else if (down != Enums.Terrain.Wall && left == Enums.Terrain.Wall) type = "f";
                else if (down == Enums.Terrain.Wall && left != Enums.Terrain.Wall) type = "c";
                this.Sprite[2].sprite = tips[typeText + "_w_" + 2 + type];

                if (down == Enums.Terrain.Wall && right == Enums.Terrain.Wall)
                {
                    if (downright == Enums.Terrain.Wall) type = "a";
                    else type = "b";
                }
                else if (down != Enums.Terrain.Wall && right != Enums.Terrain.Wall) type = "f";
                else if (down != Enums.Terrain.Wall && right == Enums.Terrain.Wall) type = "e";
                else if (down == Enums.Terrain.Wall && right != Enums.Terrain.Wall) type = "d";
                this.Sprite[3].sprite = tips[typeText + "_w_" + 3 + type];
            }
            else if (Cell.Terra == Enums.Terrain.Room)
            {
                for (int i = 0; i < 4; i++)
                {
                    this.Sprite[i].sprite = tips[typeText + "_f_" + i];
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    this.Sprite[i].sprite = tips[typeText + "_p_" + i];
                }
            }
        }
    }
}

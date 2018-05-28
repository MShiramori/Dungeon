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
        public SpriteRenderer Sprite;

        public Sprite FloorSprite;
        public Sprite WallSprite;

        public MapCell Cell { get; private set; }

        public void Initialized(MapCell cell)
        {
            this.Cell = cell;
            this.Sprite.sprite = cell.Terra == Enums.Terrain.Wall ? WallSprite : FloorSprite;
        }
    }
}

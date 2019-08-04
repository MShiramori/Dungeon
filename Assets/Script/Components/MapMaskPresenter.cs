using Assets.Script.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Components
{
    public class MapMaskPresenter : MonoBehaviour
    {
        public SpriteRenderer Sprite;
        public Form Point { get; private set; }
        private Dungeon dungeon;

        public void Initialize(Dungeon _dungeon, int x, int y)
        {
            this.dungeon = _dungeon;
            this.Point = new Form(x, y);
        }

        public void UpdateImage()
        {
            var room = dungeon.Player.GetCurrentRoom();
            if (room != null)
            {
                if (room.OnRoomAround(this.Point))
                {
                    Sprite.gameObject.SetActive(false);
                }
                else
                {
                    Sprite.gameObject.SetActive(true);
                }
            }
            else
            {
                if (this.Point.x >= dungeon.Player.Position.x - 1
                    && this.Point.x <= dungeon.Player.Position.x + 1
                    && this.Point.y >= dungeon.Player.Position.y - 1
                    && this.Point.y <= dungeon.Player.Position.y + 1)
                {
                    Sprite.gameObject.SetActive(false);
                }
                else
                    Sprite.gameObject.SetActive(true);
            }
        }
    }
}

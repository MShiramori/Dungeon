﻿using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Model
{
    public class Enemy : Character
    {
        public override CharacterType Type { get { return CharacterType.Enemy; } }
        public EnemyType EnemyType { get; set; }

        public Enemy(Dungeon _dungeon, EnemyType type) : base(_dungeon)
        {
            EnemyType = type;
        }

        // 行動処理
        protected override void Action()
        {

        }

        /// <summary>
        /// 画像設定
        /// </summary>
        protected override void SetTexture()
        {
            sprites = Resources.LoadAll<Sprite>(string.Format("Textures/Character/test/player"));

            UpdateSpriteImage();
        }
    }
}

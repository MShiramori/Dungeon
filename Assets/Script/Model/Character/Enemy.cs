using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Model
{
    public class Enemy : Character
    {
        protected override CharacterParams Params { get { return _params; } }
        private EnemyParams _params;
        public override CharacterType Type { get { return CharacterType.Enemy; } }
        public EnemyType EnemyType { get; set; }

        public Enemy(Dungeon _dungeon, EnemyType type) : base(_dungeon)
        {
            _params = new EnemyParams();
            EnemyType = type;

            //TODO: 種類ごとのパラメータ取得
            Params.Name = "Noname";
            HP = 5;
            MaxHP = 5;
            Speed = 8;
            Params.Str = 0;
            Params.Vit = 0;
            Params.Dex = 0;
            Params.Agi = 0;
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

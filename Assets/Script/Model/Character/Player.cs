using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UniRx;

namespace Assets.Script.Model
{
    public class Player : Character
    {
        public override CharacterType Type { get { return CharacterType.Player; } }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Stamina { get; set; }
        public int MaxStamina { get; set; }

        public Player(Dungeon _dungeon) : base(_dungeon)
        {
            Level = 1;
            HP = 30;
            MaxHP = 30;
            Speed = 8;
            Stamina = 100;
            MaxStamina = 100;
        }

        // コマンド入力を受けて行動する
        public int CommandExec()
        {
            return MAX_WAIT / Speed;
        }

        public override void ResetViewPotition()
        {
            base.ResetViewPotition();
            dungeon.MainCamera.transform.localPosition = new Vector3(this.Object.transform.localPosition.x, this.Object.transform.localPosition.y, 0);
        }

        public override void MovingAnimationEveryFrameAction()
        {
            dungeon.MainCamera.transform.localPosition = new Vector3(this.Object.transform.localPosition.x, this.Object.transform.localPosition.y, 0);
        }

        /// <summary>
        /// オブジェクトを生成する
        /// </summary>
        public void SetCamera(Camera mainCamera)
        {
            dungeon.MainCamera = mainCamera;
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

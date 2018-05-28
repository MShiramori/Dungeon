﻿using Assets.Script.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Assets.Script.Enums;
using UniRx;

namespace Assets.Script.Scene
{
    public class DungeonMainScene : MonoBehaviour
    {
        public Transform MapRoot;
        public GameObject FloorPrefab;
        public GameObject WallPrefab;
        public Transform CharacterRoot;
        public GameObject PlayerPrefab;
        public Camera MainCamera;

        private InputMode mode = InputMode.None;
        private int inputWait;

        private Dungeon dungeon;

        // Use this for initialization
        void Start()
        {
            var generator = new DungeonMapGenerator();
            dungeon = generator.CreateMap(new Form(10, 10), new Form(4, 4), new System.Random().Next(5, 10));
            dungeon.MainCamera = this.MainCamera;

            //マップ描画
            for (int x = 0; x < dungeon.MapSize.x; x++)
            {
                for (int y = 0; y < dungeon.MapSize.y; y++)
                {
                    GameObject obj;
                    if(dungeon.MapData[x,y].Terra == Enums.Terrain.Wall)
                        obj = GameObject.Instantiate(WallPrefab);
                    else
                        obj = GameObject.Instantiate(FloorPrefab);
                    obj.transform.SetParent(MapRoot, false);
                    obj.transform.localPosition = new Vector3(x * 32, y * -32, 0);
                }
            }

            //プレイヤー＆敵配置
            foreach (var chara in dungeon.Characters)
            {
                chara.InstantiateObject(PlayerPrefab, CharacterRoot);
            }

            mode = InputMode.Waiting;
            inputWait = 3;
        }

        // Update is called once per frame
        void Update()
        {
            //入力可能
            if (mode == InputMode.Waiting)
            {
                var isAction = false;

                //移動判定
                {
                    var direction = new Form(0, 0);
                    if (Input.GetKey(KeyCode.UpArrow))
                        direction.y--;
                    if (Input.GetKey(KeyCode.DownArrow))
                        direction.y++;
                    if (Input.GetKey(KeyCode.LeftArrow))
                        direction.x--;
                    if (Input.GetKey(KeyCode.RightArrow))
                        direction.x++;

                    var destination = dungeon.Player.Position + direction;
                    if (direction.AbsTotal > 0)
                    {
                        if (inputWait <= 0)
                            isAction |= dungeon.Player.Move(destination);
                        else
                            inputWait--;
                    }
                    else
                    {
                        inputWait = 3;
                    }
                }
                //攻撃判定
                if (!isAction)
                {
                    if (Input.GetKey(KeyCode.Z))
                        isAction |= dungeon.Player.Attack();
                }

                //ターン経過処理実行
                if (isAction)
                {
                    //プレイヤーの行動で経過した時間だけ他のキャラを行動させる
                    var time = dungeon.Player.ActionWait / dungeon.Player.Speed;
                    for (int i = 0; i < time; i++)
                    {
                        foreach (var chara in dungeon.Characters.Where(x => x.Type != CharacterType.Player))
                        {
                            chara.Tick();
                        }
                    }

                    //アニメーションを開始
                    mode = InputMode.OnAnimation;
                    
                    //プレイヤーの移動以外のアクションを実行
                    dungeon.Player.ActionAnimations()
                        //移動アニメ
                        .SelectMany(_ => Observable.WhenAll(dungeon.Characters.Select(x => x.MovingAnimation())))
                        //プレイヤー以外の移動以外のアクションを順次実行
                        .SelectMany(_ => dungeon.Characters.Where(x => x.Type != CharacterType.Player).Select(x => x.ActionAnimations()).Concat())
                        //後処理
                        .Do(x =>
                        {
                            foreach (var chara in dungeon.Characters)
                            {
                                chara.ResetActions();
                                chara.ResetViewPotition();
                            }
                            mode = InputMode.Waiting;//終わったので入力待機に戻す
                        })
                        .Subscribe()
                        .AddTo(this);
                }
            }
        }
    }
}
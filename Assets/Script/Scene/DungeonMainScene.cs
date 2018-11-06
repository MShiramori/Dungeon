using Assets.Script.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Assets.Script.Enums;
using UniRx;
using Assets.Script.Components;
using UnityEngine.UI;

namespace Assets.Script.Scene
{
    public class DungeonMainScene : MonoBehaviour
    {
        public Transform MapRoot;
        public Transform CharacterRoot;
        public Transform ObjectRoot;
        public DungeonPrefabs DungeonPrefabs;
        public HeaderUIPresenter HeaderUI;
        public Text MessageText;
        public WindowRootPresenter WindowRoot;
        public Camera MainCamera;

        private InputMode mode = InputMode.None;
        private int inputWait;

        private Dungeon dungeon;

        // Use this for initialization
        void Start()
        {
            dungeon = new Dungeon();
            dungeon.MainCamera = this.MainCamera;
            dungeon.MapRoot = this.MapRoot;
            dungeon.ObjectRoot = this.ObjectRoot;
            dungeon.CharacterRoot = this.CharacterRoot;
            dungeon.DungeonPrefabs = this.DungeonPrefabs;
            dungeon.Initialize();

            //ヘッダ表示
            HeaderUI.Initialized(dungeon);

            //メッセージ表示クラス初期化
            StaticData.Message = new Message(MessageText);

            //ウィンドウクラス初期化
            WindowRoot.Initialize();

            mode = InputMode.Waiting;
            inputWait = 3;
        }

        // Update is called once per frame
        void Update()
        {
            if(StaticData.Message.Mode != MessageMode.None)
            {
                if (Input.GetKey(KeyCode.Z))
                {
                    StaticData.Message.OnInput();
                }
            }
            //入力可能
            else if (mode == InputMode.Waiting)
            {
                if (WindowRoot.GetCurrentWindowType() == WindowType.None)
                {
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
                                dungeon.Player.IsAction |= dungeon.Player.Move(destination);
                            else
                                inputWait--;
                        }
                        else
                        {
                            inputWait = 3;
                        }
                    }

                    if (!dungeon.Player.IsAction)
                    {
                        //攻撃判定
                        if (Input.GetKey(KeyCode.Z))
                        {
                            dungeon.Player.IsAction |= dungeon.Player.Attack();
                        }
                        //アイテムを拾う
                        if (Input.GetKey(KeyCode.P))
                        {
                            var item = dungeon.Objects.Where(x => x as Item != null && x.Position == dungeon.Player.Position).FirstOrDefault();
                            if (item != null)
                            {
                                dungeon.Player.IsAction |= dungeon.Player.PickUp(item as Item);
                            }
                        }
                        //階段を下りる
                        else if (Input.GetKey(KeyCode.LeftShift))//コマンドは仮
                        {
                            //プレイヤーの位置にある階段を取得
                            var step = dungeon.Objects.Where(x => x as Step != null && x.Position == dungeon.Player.Position).FirstOrDefault();
                            if (step != null)
                            {
                                //マップ再生成
                                dungeon.UpdateFloor(dungeon.Floor + 1);
                                dungeon.ResetMap();
                            }
                        }
                    }
                }

                if (!dungeon.Player.IsAction)
                {
                    //ウィンドウの入力チェック
                    WindowRoot.CheckInput();
                }

                //ターン経過処理実行
                if (dungeon.Player.IsAction)
                {
                    //プレイヤーの行動時自動処理
                    dungeon.Player.AfterAction();

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
                        .Do(_ => dungeon.StatusUpdateEventTrigger.OnNext(Unit.Default))//表示更新
                        //プレイヤー以外の移動以外のアクションを順次実行
                        .SelectMany(_ => dungeon.Characters.Where(x => x.Type != CharacterType.Player).Select(x => x.ActionAnimations()).Concat())
                        .Last()
                        //後処理
                        .Do(x =>
                        {
                            foreach (var chara in dungeon.Characters)
                            {
                                chara.ResetActions();
                                chara.ResetViewPotition();
                            }
                            dungeon.Player.IsAction = false;
                            mode = InputMode.Waiting;//終わったので入力待機に戻す
                        })
                        .Subscribe()
                        .AddTo(this);
                }
            }
        }
    }
}

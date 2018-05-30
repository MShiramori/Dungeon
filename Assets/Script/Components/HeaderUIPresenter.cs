using Assets.Script.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Assets.Script.Components
{
    public class HeaderUIPresenter : MonoBehaviour
    {
        public Text FloorText;
        public Text LevelText;
        public Text HpText;

        private Dungeon dungeon;

        public void Initialized(Dungeon _dungeon)
        {
            this.dungeon = _dungeon;
            UpdateText();
            dungeon.StatusUpdateEventTrigger.Subscribe(_ => UpdateText()).AddTo(this);
        }

        private void UpdateText()
        {
            var player = dungeon.Player;
            FloorText.text = dungeon.Floor > 0 ? string.Format("{0}F", dungeon.Floor) : "";
            LevelText.text = player != null ? string.Format("Lv {0}", player.Level) : "";
            HpText.text = player != null ? string.Format("HP {0}/{1}", player.HP, player.MaxHP) : "";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UniRx;

namespace Assets.Script.Components
{
    public class CharacterObject : MonoBehaviour
    {
        public SpriteRenderer Sprite;
        public Transform Transform { get { return _transform ?? (_transform = this.transform); } }
        private Transform _transform;
    }
}

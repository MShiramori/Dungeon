using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Components
{
    public class MapObjectPresenter : MonoBehaviour
    {
        public SpriteRenderer Sprite;
        public Transform Transform { get { return _transform ?? (_transform = this.transform); } }
        protected Transform _transform;
    }
}

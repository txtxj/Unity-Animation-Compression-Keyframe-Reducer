using UnityEngine;

namespace Citrine.Animation.Editor
{
    internal interface IKeyframeBase<T> where T : struct
    {
        public Keyframe[] keyframe { get; set; }

        public float time { get; }

        public T value { get; set; }

        public void ClearSlope();
    }
}
using UnityEngine;

namespace Citrine.Utils.AnimationCompression
{
    internal interface IKeyframeBase<T> where T : struct
    {
        public Keyframe[] keyframe { get; set; }
        
        public float time { get; }
        
        public T value { get; set; }

        public void ClearSlope();
    }
}

using UnityEngine;

namespace Citrine.Utils.Editor.AnimationCompression
{
    internal interface IKeyframeBase<T> where T : struct
    {
        public Keyframe[] keyframe { get; set; }
        
        public float time { get; }
        
        public T value { get; set; }

        public void ClearSlope();
    }
}

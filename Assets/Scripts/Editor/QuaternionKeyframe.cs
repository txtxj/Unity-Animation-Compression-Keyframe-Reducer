using UnityEngine;

namespace Citrine.Utils.Editor.AnimationCompression
{
    internal struct QuaternionKeyframe : IKeyframeBase<Quaternion>
    {
        public Keyframe[] keyframe { get; set; }

        public QuaternionKeyframe(params Keyframe[] list)
        {
            keyframe = new[] { list[0], list[1], list[2], list[3] };
        }

        public float time => keyframe[0].time;

        public Quaternion value
        {
            get => new (keyframe[0].value, keyframe[1].value, keyframe[2].value, keyframe[3].value);
            set
            {
                keyframe[0].value = value[0];
                keyframe[1].value = value[1];
                keyframe[2].value = value[2];
                keyframe[3].value = value[3];
            }
        }

        public void ClearSlope()
        {
            for (int i = 0; i < 4; i++)
            {
                keyframe[i].inTangent = 0f;
                keyframe[i].outTangent = 0f;
                keyframe[i].inWeight = 0f;
                keyframe[i].outWeight = 0f;
            }
        }
    }
}

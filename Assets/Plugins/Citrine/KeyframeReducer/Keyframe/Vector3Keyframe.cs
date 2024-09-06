using UnityEngine;

namespace Citrine.Animation.Editor
{
    internal struct Vector3Keyframe : IKeyframeBase<Vector3>
    {
        public Keyframe[] keyframe { get; set; }

        public Vector3Keyframe(params Keyframe[] list)
        {
            keyframe = new[] { list[0], list[1], list[2] };
        }

        public float time
        {
            get => keyframe[0].time;
        }

        public Vector3 value
        {
            get => new (keyframe[0].value, keyframe[1].value, keyframe[2].value);
            set
            {
                keyframe[0].value = value[0];
                keyframe[1].value = value[1];
                keyframe[2].value = value[2];
            }
        }

        public void ClearSlope()
        {
            for (int i = 0; i < 3; i++)
            {
                keyframe[i].inTangent = 0f;
                keyframe[i].outTangent = 0f;
                keyframe[i].inWeight = 0f;
                keyframe[i].outWeight = 0f;
            }
        }
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Citrine.Utils.Editor.AnimationCompression
{
    internal class QuaternionAnimationCurve : AnimationCurveBase<Quaternion>
    {
        internal QuaternionAnimationCurve()
        {
            curve = new AnimationCurve[4];
            binding = new EditorCurveBinding[4];
        }

        protected override Quaternion Evaluate(float time)
        {
            return new (curve[0].Evaluate(time), curve[1].Evaluate(time), curve[2].Evaluate(time), curve[3].Evaluate(time));
        }
        
        protected override Quaternion Interpolate(IKeyframeBase<Quaternion> begin, IKeyframeBase<Quaternion> end, float time)
        {
            Quaternion ret = new Quaternion();
            for (int i = 0; i < 4; i++)
            {
                if (((begin.keyframe[i].weightedMode & WeightedMode.Out) |
                     (end.keyframe[i].weightedMode & WeightedMode.In)) == WeightedMode.None)
                {
                    ret[i] = Hermite(begin.keyframe[i], end.keyframe[i], time);
                }
                else
                {
                    ret[i] = Bezier(begin.keyframe[i], end.keyframe[i], time);
                }
            }

            return ret;
        }

        protected override IKeyframeBase<Quaternion> GetKey(int index)
        {
            return new QuaternionKeyframe(curve[0].keys[index], curve[1].keys[index], curve[2].keys[index], curve[3].keys[index]);
        }
        
        protected override void SetKey(int index, IKeyframeBase<Quaternion> key)
        {
            (curve[0].keys[index], curve[1].keys[index], curve[2].keys[index], curve[3].keys[index])
                = (key.keyframe[0], key.keyframe[1], key.keyframe[2], key.keyframe[3]);
        }

        protected override void SetKeys(List<IKeyframeBase<Quaternion>> list)
        {
            base.SetKeys(list);
            Keyframe[] keyX = new Keyframe[list.Count];
            Keyframe[] keyY = new Keyframe[list.Count];
            Keyframe[] keyZ = new Keyframe[list.Count];
            Keyframe[] keyW = new Keyframe[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                keyX[i] = list[i].keyframe[0];
                keyY[i] = list[i].keyframe[1];
                keyZ[i] = list[i].keyframe[2];
                keyW[i] = list[i].keyframe[3];
            }
            
            curve[0].keys = keyX;
            curve[1].keys = keyY;
            curve[2].keys = keyZ;
            curve[3].keys = keyW;
        }
    }
}

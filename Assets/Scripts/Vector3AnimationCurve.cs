using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Citrine.Utils.AnimationCompression
{
    internal class Vector3AnimationCurve : AnimationCurveBase<Vector3>
    {
        internal Vector3AnimationCurve()
        {
            curve = new AnimationCurve[3];
            binding = new EditorCurveBinding[3];
        }

        protected override Vector3 Evaluate(float time)
        {
            return new (curve[0].Evaluate(time), curve[1].Evaluate(time), curve[2].Evaluate(time));
        }
        
        protected override Vector3 Interpolate(IKeyframeBase<Vector3> begin, IKeyframeBase<Vector3> end, float time)
        {
            Vector3 ret = new Vector3();
            for (int i = 0; i < 3; i++)
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

        protected override IKeyframeBase<Vector3> GetKey(int index)
        {
            return new Vector3Keyframe(curve[0].keys[index], curve[1].keys[index], curve[2].keys[index]);
        }
        
        protected override void SetKey(int index, IKeyframeBase<Vector3> key)
        {
            (curve[0].keys[index], curve[1].keys[index], curve[2].keys[index]) = (key.keyframe[0], key.keyframe[1], key.keyframe[2]);
        }

        protected override void SetKeys(List<IKeyframeBase<Vector3>> list)
        {
            Keyframe[] keyX = new Keyframe[list.Count];
            Keyframe[] keyY = new Keyframe[list.Count];
            Keyframe[] keyZ = new Keyframe[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                keyX[i] = list[i].keyframe[0];
                keyY[i] = list[i].keyframe[1];
                keyZ[i] = list[i].keyframe[2];
            }
    
            curve[0].keys = keyX;
            curve[1].keys = keyY;
            curve[2].keys = keyZ;
        }
    }
}

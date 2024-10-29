using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Citrine.Animation.Editor
{
    public static class KeyframeReducer
    {
        private static void ReduceKeyframes<T>(AnimationCurveBase<T>[] curves,
            KeyframeReducerErrorFunction.ErrorFunction<T> reductionFunction, float error, float sampleRate)
            where T : struct
        {
            foreach (var curve in curves)
            {
                curve.ReduceKeyframes(reductionFunction, error, sampleRate);
                if (curve.Reduced)
                {
                    Debug.Log($"{curve.Path}.{curve.PropertyName} is reduced!");
                }
            }
        }

        private static bool CheckData<T>(params AnimationCurveBase<T>[][] curvess) where T : struct
        {
            foreach (AnimationCurveBase<T>[] curves in curvess)
            {
                foreach (AnimationCurveBase<T> curve in curves)
                {
                    if (!curve.CheckData())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static AnimationCurveBase<Quaternion>[] GetQuaternionCurves(AnimationClip clip)
        {
            List<AnimationCurveBase<Quaternion>> ret = new List<AnimationCurveBase<Quaternion>>();
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            foreach (EditorCurveBinding binding in bindings)
            {
                if (binding.propertyName.ToLower().Contains("localrotation"))
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                    int index = -1;
                    string bindingName = binding.path + "." + binding.propertyName[..^2];
                    if (dictionary.TryGetValue(bindingName, out var value))
                    {
                        index = value;
                    }

                    if (index == -1)
                    {
                        dictionary.Add(bindingName, ret.Count);
                        index = ret.Count;
                        ret.Add(new QuaternionAnimationCurve());
                    }

                    if (binding.propertyName[^1] == 'x')
                    {
                        ret[index].curve[0] = curve;
                        ret[index].binding[0] = binding;
                    }
                    else if (binding.propertyName[^1] == 'y')
                    {
                        ret[index].curve[1] = curve;
                        ret[index].binding[1] = binding;
                    }
                    else if (binding.propertyName[^1] == 'z')
                    {
                        ret[index].curve[2] = curve;
                        ret[index].binding[2] = binding;
                    }
                    else if (binding.propertyName[^1] == 'w')
                    {
                        ret[index].curve[3] = curve;
                        ret[index].binding[3] = binding;
                    }
                }
            }

            return ret.ToArray();
        }

        private static AnimationCurveBase<Vector3>[] GetVector3Curves(AnimationClip clip, string propertyName)
        {
            List<AnimationCurveBase<Vector3>> ret = new List<AnimationCurveBase<Vector3>>();
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            foreach (EditorCurveBinding binding in bindings)
            {
                if (binding.propertyName.ToLower().Contains(propertyName))
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                    int index = -1;
                    string bindingName = binding.path + "." + binding.propertyName[..^2];
                    if (dictionary.TryGetValue(bindingName, out var value))
                    {
                        index = value;
                    }

                    if (index == -1)
                    {
                        dictionary.Add(bindingName, ret.Count);
                        index = ret.Count;
                        ret.Add(new Vector3AnimationCurve());
                    }

                    if (binding.propertyName[^1] == 'x')
                    {
                        ret[index].curve[0] = curve;
                        ret[index].binding[0] = binding;
                    }
                    else if (binding.propertyName[^1] == 'y')
                    {
                        ret[index].curve[1] = curve;
                        ret[index].binding[1] = binding;
                    }
                    else if (binding.propertyName[^1] == 'z')
                    {
                        ret[index].curve[2] = curve;
                        ret[index].binding[2] = binding;
                    }
                }
            }

            return ret.ToArray();
        }

        private static AnimationCurveBase<Vector3>[] GetEulerCurves(AnimationClip clip)
        {
            return GetVector3Curves(clip, "localeuleranglesraw");
        }

        private static AnimationCurveBase<Vector3>[] GetPositionCurves(AnimationClip clip)
        {
            return GetVector3Curves(clip, "localposition");
        }

        private static AnimationCurveBase<Vector3>[] GetScaleCurves(AnimationClip clip)
        {
            return GetVector3Curves(clip, "localscale");
        }

        private static void SetCurves(AnimationClip clip, AnimationCurveBase<Quaternion>[] list)
        {
            List<EditorCurveBinding> bindings = new List<EditorCurveBinding>(list.Length * 4);
            List<AnimationCurve> curves = new List<AnimationCurve>(list.Length * 4);

            foreach (var iCurve in list)
            {
                bindings.AddRange(iCurve.binding);
                curves.AddRange(iCurve.curve);
            }

            AnimationUtility.SetEditorCurves(clip, bindings.ToArray(), curves.ToArray());
        }

        private static void SetCurves(AnimationClip clip, AnimationCurveBase<Vector3>[] list)
        {
            List<EditorCurveBinding> bindings = new List<EditorCurveBinding>(list.Length * 3);
            List<AnimationCurve> curves = new List<AnimationCurve>(list.Length * 3);

            foreach (var iCurve in list)
            {
                bindings.AddRange(iCurve.binding);
                curves.AddRange(iCurve.curve);
            }

            AnimationUtility.SetEditorCurves(clip, bindings.ToArray(), curves.ToArray());
        }

        /// <summary>
        /// Compressing keyframes of an animation clip
        /// </summary>
        /// <param name="clip">The animation clip to compress</param>
        /// <param name="rotationError"></param>
        /// <param name="positionError"></param>
        /// <param name="scaleError"></param>
        /// <param name="checkData">Whether to test the curve before compression</param>
        public static void ReduceKeyframes(this AnimationClip clip, float rotationError, float positionError,
            float scaleError, bool checkData)
        {
            AnimationCurveBase<Quaternion>[] rot = GetQuaternionCurves(clip);
            AnimationCurveBase<Vector3>[] euler = GetEulerCurves(clip);
            AnimationCurveBase<Vector3>[] pos = GetPositionCurves(clip);
            AnimationCurveBase<Vector3>[] scale = GetScaleCurves(clip);

            if (!checkData || CheckData(euler, pos, scale))
            {
                rotationError = Mathf.Cos(Mathf.Deg2Rad * rotationError);
                positionError /= 100.0f;
                scaleError /= 100.0f;
                float sampleRate = clip.frameRate;

                ReduceKeyframes(rot, KeyframeReducerErrorFunction.QuaternionRotationErrorFunction, rotationError, sampleRate);
                ReduceKeyframes(euler, KeyframeReducerErrorFunction.RawEulerAngleErrorFunction, rotationError, sampleRate);
                ReduceKeyframes(pos, KeyframeReducerErrorFunction.PositionErrorFunction, positionError, sampleRate);
                ReduceKeyframes(scale, KeyframeReducerErrorFunction.ScaleErrorFunction, scaleError, sampleRate);

                SetCurves(clip, rot);
                SetCurves(clip, euler);
                SetCurves(clip, pos);
                SetCurves(clip, scale);
            }
        }
    }
}
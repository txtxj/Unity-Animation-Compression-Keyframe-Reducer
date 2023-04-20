using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Citrine.Utils.AnimationCompression
{
    public class KeyframeReducer
    {
        public bool checkFlag = false;

        private void ReduceKeyframes<T>(AnimationCurveBase<T>[] curves,
            Func<T, T, float, bool> reductionFunction, float error, float sampleRate) where T : struct
        {
            foreach (var curve in curves)
            {
                curve.ReduceKeyframes(reductionFunction, error, sampleRate);
                if (curve.Reduced)
                {
                    Debug.Log($"{curve.path}.{curve.propertyName} is reduced!");
                }
            }
        }

        private bool CheckData<T>(params AnimationCurveBase<T>[][] curvess) where T : struct
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

        private AnimationCurveBase<Quaternion>[] GetQuaternionCurves(AnimationClip clip)
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
                    if (dictionary.ContainsKey(bindingName))
                    {
                        index = dictionary[bindingName];
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
        
        private AnimationCurveBase<Vector3>[] GetVector3Curves(AnimationClip clip, string propertyName)
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
                    if (dictionary.ContainsKey(bindingName))
                    {
                        index = dictionary[bindingName];
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
        
        private AnimationCurveBase<Vector3>[] GetEulerCurves(AnimationClip clip)
        {
            return GetVector3Curves(clip, "localeuleranglesraw");
        }
        
        private AnimationCurveBase<Vector3>[] GetPositionCurves(AnimationClip clip)
        {
            return GetVector3Curves(clip, "localposition");
        }
        
        private AnimationCurveBase<Vector3>[] GetScaleCurves(AnimationClip clip)
        {
            return GetVector3Curves(clip, "localscale");
        }

        private void SetQuaternionCurves(AnimationClip clip, AnimationCurveBase<Quaternion>[] list)
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
        
        private void SetVector3Curves(AnimationClip clip, AnimationCurveBase<Vector3>[] list)
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

        private bool QuaternionRotationErrorFunction(Quaternion reduced, Quaternion value, float maxError)
        {
            return Quaternion.Dot(reduced, reduced) > maxError && Quaternion.Dot(reduced.normalized, value.normalized) > maxError;
        }
        
        private bool RawEulerAngleErrorFunction(Vector3 reduced, Vector3 value, float maxError)
        {
            return QuaternionRotationErrorFunction(Quaternion.Euler(reduced), Quaternion.Euler(value), maxError);
        }
        
        private bool PositionErrorFunction(Vector3 reduced, Vector3 value, float maxError)
        {
            float minValue = 0.00001f * maxError;

            float distance = (value - reduced).sqrMagnitude;
            float length = value.sqrMagnitude;
            float lengthReduced = reduced.sqrMagnitude;
            if (DeltaError(length, lengthReduced, distance, maxError * maxError, minValue * minValue))
                return false;

            var distanceX = Mathf.Abs(value.x - reduced.x);
            var distanceY = Mathf.Abs(value.y - reduced.y);
            var distanceZ = Mathf.Abs(value.z - reduced.z);

            if (DeltaError(value.x, reduced.x, distanceX, maxError, minValue))
                return false;
            if (DeltaError(value.y, reduced.y, distanceY, maxError, minValue))
                return false;
            if (DeltaError(value.z, reduced.z, distanceZ, maxError, minValue))
                return false;

            return true;
        }
        
        private bool ScaleErrorFunction(Vector3 reduced, Vector3 value, float maxError)
        {
            return PositionErrorFunction(reduced, value, maxError);
        }

        private bool DeltaError(float value, float reduced, float delta, float percentage, float minValue)
        {
            float absValue = Mathf.Abs(value);
            return (absValue > minValue || Mathf.Abs(reduced) > minValue) && delta > absValue * percentage;
        }
        
        public void ReduceKeyframes(AnimationClip clip, float rotationError, float positionError, float scaleError)
        {
            AnimationCurveBase<Quaternion>[] rot = GetQuaternionCurves(clip);
            AnimationCurveBase<Vector3>[] euler = GetEulerCurves(clip);
            AnimationCurveBase<Vector3>[] pos = GetPositionCurves(clip);
            AnimationCurveBase<Vector3>[] scale = GetScaleCurves(clip);

            if (!checkFlag || CheckData(euler, pos, scale))
            {
                rotationError = Mathf.Cos(Mathf.Deg2Rad * rotationError);
                positionError /= 100.0f;
                scaleError /= 100.0f;
                float sampleRate = clip.frameRate;

                ReduceKeyframes(rot, (q1, q2, maxError) => QuaternionRotationErrorFunction(q1, q2, maxError), rotationError, sampleRate);
                ReduceKeyframes(euler, (e1, e2, maxError) => RawEulerAngleErrorFunction(e1, e2, maxError), rotationError, sampleRate);
                ReduceKeyframes(pos, (p1, p2, maxError) => PositionErrorFunction(p1, p2, maxError), positionError, sampleRate);
                ReduceKeyframes(scale, (s1, s2, maxError) => ScaleErrorFunction(s1, s2, maxError), scaleError, sampleRate);

                SetQuaternionCurves(clip, rot);
                SetVector3Curves(clip, euler);
                SetVector3Curves(clip, pos);
                SetVector3Curves(clip, scale);
            }
        }
    }
}

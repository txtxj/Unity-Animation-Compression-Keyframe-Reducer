using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Citrine.Animation.Editor
{
    internal abstract class AnimationCurveBase<T> where T : struct
    {
        private const float Epsilon = (float)1e-6;

        internal AnimationCurve[] curve;

        internal EditorCurveBinding[] binding;

        internal bool Reduced { get; private set; }

        internal string path => binding[0].path;
        internal string propertyName => binding[0].propertyName[..^2];
        private int length => curve[0]?.length ?? 0;

        internal IKeyframeBase<T> this[int index]
        {
            get => GetKey(index);
            set => SetKey(index, value);
        }

        protected abstract T Evaluate(float time);
        protected abstract IKeyframeBase<T> GetKey(int index);
        protected abstract void SetKey(int index, IKeyframeBase<T> key);

        protected virtual void SetKeys(List<IKeyframeBase<T>> list)
        {
            Reduced = true;
        }

        protected abstract T Interpolate(IKeyframeBase<T> begin, IKeyframeBase<T> end, float time);

        private float GetTimeAt(int index) => curve[0].keys[index].time;

        private bool CalculateErrorAtTime(IKeyframeBase<T> key0, IKeyframeBase<T> key1,
            float time, Func<T, T, float, bool> errorFunction, float error)
        {
            return errorFunction(Interpolate(key0, key1, time), Evaluate(time), error);
        }

        private bool CheckConstantAndReduce(IKeyframeBase<T> begin, IKeyframeBase<T> end, Func<T, T, float, bool> errorFunction, float error)
        {
            begin.ClearSlope();
            end.ClearSlope();

            for (int i = 1; i < length - 1; i++)
            {
                float time = GetTimeAt(i);

                if (!CalculateErrorAtTime(begin, end, time, errorFunction, error))
                {
                    return false;
                }
            }

            SetKeys(new List<IKeyframeBase<T>>(2) { begin, end });

            return true;
        }

        private bool IsReducible(int beginIndex, int endIndex, Func<T, T, float, bool> errorFunction, float error, float sampleRate)
        {
            IKeyframeBase<T> begin = GetKey(beginIndex);
            IKeyframeBase<T> end = GetKey(endIndex);

            for (int i = beginIndex + 1; i < endIndex; i++)
            {
                float time = GetTimeAt(i);

                if (!CalculateErrorAtTime(begin, end, time, errorFunction, error))
                {
                    return false;
                }
            }

            int frameCount = (int) ((end.time - begin.time) * sampleRate);
            float frameStep = 1f / sampleRate;

            for (int i = 0; i <= frameCount; i++)
            {
                float time = begin.time + i * frameStep;

                if (!CalculateErrorAtTime(begin, end, time, errorFunction, error))
                {
                    return false;
                }
            }

            CalculateErrorAtTime(GetKey(0), GetKey(3), 1f, errorFunction, error);

            return true;
        }

        internal void ReduceKeyframes(Func<T, T, float, bool> errorFunction, float error, float sampleRate)
        {
            if (length <= 2)
            {
                return;
            }

            IKeyframeBase<T> begin = GetKey(0);
            IKeyframeBase<T> end = GetKey(length - 1);

            if (CheckConstantAndReduce(begin, end, errorFunction, error))
            {
                return;
            }

            List<IKeyframeBase<T>> reducedKeyframes = new List<IKeyframeBase<T>>(length);

            reducedKeyframes.Add(GetKey(0));

            int comparerFrameIndex = 0;
            for (int curIndex = 2; curIndex < length; curIndex++)
            {
                if (!IsReducible(comparerFrameIndex, curIndex, errorFunction, error, sampleRate))
                {
                    reducedKeyframes.Add(GetKey(curIndex - 1));
                    comparerFrameIndex = curIndex - 1;
                }
            }

            reducedKeyframes.Add(GetKey(length - 1));

            if (reducedKeyframes.Count < length)
            {
                SetKeys(reducedKeyframes);
            }
        }

        internal bool CheckData()
        {
            if (length < 2)
            {
                Debug.LogWarning("Curve length is less than 2!");
            }
            for (int i = 1; i < curve.Length; i++)
            {
                AnimationCurve iCurve = curve[i];
                if (iCurve == null)
                {
                    Debug.LogError("Curve is null!");
                    return false;
                }
                if (iCurve.length != length)
                {
                    Debug.LogError("Curve length illegal!");
                    return false;
                }
                for (int j = 0; j < iCurve.length; j++)
                {
                    if (Math.Abs(iCurve[j].time - curve[0][j].time) > Epsilon)
                    {
                        Debug.LogError("Keyframes have different time!");
                        return false;
                    }
                }
            }

            return true;
        }

        protected float Hermite(Keyframe begin, Keyframe end, float time)
        {
            if (end.time - begin.time < Epsilon)
                return begin.value;

            float normal = end.time - begin.time;

            float t = (time - begin.time) / normal;
            float t2 = t * t;
            float t3 = t2 * t;

            float h00 = 2f * t3 - 3f * t2 + 1f;
            float h10 = t3 - 2f * t2 + t;
            float h01 = -2f * t3 + 3f * t2;
            float h11 = t3 - t2;

            float p0 = begin.value;
            float p1 = end.value;
            float m0 = begin.outTangent * normal;
            float m1 = end.inTangent * normal;

            return h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
        }

        protected float Bezier(Keyframe begin, Keyframe end, float time)
        {
            throw new NotImplementedException("We don't know how to interpolate a bezier curve with weights.");
        }
    }
}
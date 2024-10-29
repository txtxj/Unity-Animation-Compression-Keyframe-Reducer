using UnityEngine;

namespace Citrine.Animation.Editor
{
    public static class KeyframeReducerErrorFunction
    {
        /// <summary>
        /// Returns true if Abs(reduced - value) > maxError
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public delegate bool ErrorFunction<in T>(T reduced, T value, float maxError) where T : struct;
        
        internal static readonly ErrorFunction<Quaternion> QuaternionRotationErrorFunction =
            (reduced, value, maxError) => Quaternion.Dot(reduced, reduced) > maxError &&
                                          Quaternion.Dot(reduced.normalized, value.normalized) > maxError;

        internal static readonly ErrorFunction<Vector3> RawEulerAngleErrorFunction =
            (reduced, value, maxError) =>
                QuaternionRotationErrorFunction(Quaternion.Euler(reduced), Quaternion.Euler(value), maxError);

        internal static readonly ErrorFunction<Vector3> PositionErrorFunction =
            (reduced, value, maxError) =>
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
            };

        internal static readonly ErrorFunction<Vector3> ScaleErrorFunction =
            PositionErrorFunction;
        
        private static bool DeltaError(float value, float reduced, float delta, float percentage, float minValue)
        {
            float absValue = Mathf.Abs(value);
            return (absValue > minValue || Mathf.Abs(reduced) > minValue) && delta > absValue * percentage;
        }
    }
}
using UnityEditor;
using UnityEngine;

namespace Citrine.Utils.AnimationCompression
{
    public class KeyframeReducerUtility : Editor
    {
        private static float rotationError = 0.5f;
        private static float positionError = 0.5f;
        private static float scaleError = 0.5f;

        [MenuItem("Assets/Compress")]
        private static void Execute()
        {
            if (Selection.activeObject is AnimationClip clip)
            {
                KeyframeReducer reducer = new KeyframeReducer();
                reducer.ReduceKeyframes(clip, rotationError, positionError, scaleError, true);
            }
        }
    }
}

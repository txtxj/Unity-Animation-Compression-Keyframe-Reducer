using System;
using UnityEditor;
using UnityEngine;

namespace Citrine.Animation.Editor
{
    internal class KeyframeReducerUtils : UnityEditor.Editor
    {
        public const float RotationError = 0.5f;
        public const float PositionError = 0.5f;
        public const float ScaleError = 0.5f;

        [MenuItem(StaticMetaData.MenuItem)]
        [ContextMenu(StaticMetaData.ContextItem)]
        private static void Execute()
        {
            if (Selection.activeObject is AnimationClip clip)
            {
                try
                {
                    KeyframeReducer reducer = new KeyframeReducer();
                    reducer.ReduceKeyframes(clip, RotationError, PositionError, ScaleError, true);
                    Debug.Log($"Keyframes Reduced with default errors for clip \"{clip.name}\"");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to reduce keyframes for clip \"{clip.name}\": {ex.Message}");
                }
            }
        }
    }
}
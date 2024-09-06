using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Citrine.Animation.Editor
{
    internal class KeyframeReducerWindow : EditorWindow
    {
        private float rotationError = KeyframeReducerUtils.RotationError;
        private float positionError = KeyframeReducerUtils.PositionError;
        private float scaleError = KeyframeReducerUtils.ScaleError;
        private bool checkData = true;

        private string path = null;

        private string[] fileAnimation = null;

        [MenuItem(StaticMetaData.WindowMenuItem)]
        public static void Init()
        {
            KeyframeReducerWindow window = GetWindow<KeyframeReducerWindow>(StaticMetaData.WindowTitle);
            window.Show();
        }

        private void HandleMouseDragEvent()
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }
            if (Event.current.type == EventType.DragPerform)
            {
                if (DragAndDrop.paths is { Length: > 0 })
                {
                    path = DragAndDrop.paths[0];
                }
            }
        }

        public void OnGUI()
        {
            HandleMouseDragEvent();

            EditorGUILayout.LabelField(path ?? "Drag a folder here");

            rotationError = EditorGUILayout.FloatField("Rotation Error", rotationError);
            positionError = EditorGUILayout.FloatField("Position Error", positionError);
            scaleError = EditorGUILayout.FloatField("Scale Error", scaleError);
            checkData = EditorGUILayout.Toggle("Check Data", checkData);

            if (GUILayout.Button("Reduce Keyframes") && path is not null)
            {
                Run();
            }
        }

        private void Run()
        {
            KeyframeReducer reducer = new KeyframeReducer();
            DirectoryInfo direction = new DirectoryInfo(path);
            fileAnimation = direction.GetFiles("*.anim", SearchOption.AllDirectories).Select(fileInfo => fileInfo.FullName).ToArray();
            for (int i = 0; i < fileAnimation.Length; i++)
            {
                string file = fileAnimation[i];
                int startIndex = file.IndexOf(@"Assets", StringComparison.Ordinal);
                if (startIndex == -1)
                {
                    Debug.LogError($"{file} is not under folder 'Assets'");
                    continue;
                }
                string localPath = file[startIndex..];
                float progress = (float)i / fileAnimation.Length;
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(localPath);
                EditorUtility.DisplayProgressBar($"Keyframes Reduction {i} / {fileAnimation.Length}...", localPath, progress);
                try
                {
                    Debug.Log($"Keyframes Reduction for clip \"{localPath}\"...");
                    reducer.ReduceKeyframes(clip, rotationError, positionError, scaleError, checkData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error while keyframe reducing for \"{localPath}\": {ex.Message}");
                }
            }
            EditorUtility.ClearProgressBar();
            Debug.Log($"Keyframes Reduced for all animations in folder \"{direction.FullName}\"");
        }
    }
}
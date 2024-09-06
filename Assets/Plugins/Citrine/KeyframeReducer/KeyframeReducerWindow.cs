using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Citrine.Animation.Editor
{
    internal enum ClipGetType
    {
        OneClip = 0,
        ManyClips = 1,
        Folder = 2
    }

    internal class KeyframeReducerWindow : EditorWindow
    {
        private ClipGetType getType = ClipGetType.OneClip;
        private AnimationClip[] clips = { };
        private string folderPath = null;

        private float rotationError = KeyframeReducerUtils.RotationError;
        private float positionError = KeyframeReducerUtils.PositionError;
        private float scaleError = KeyframeReducerUtils.ScaleError;
        private bool checkData = true;

        [MenuItem(StaticMetaData.WindowMenuItem)]
        public static void Init()
        {
            KeyframeReducerWindow window = GetWindow<KeyframeReducerWindow>(StaticMetaData.WindowTitle);
            window.Show();
        }

        private void UpdateFolderPathFromDrag()
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }

            if (Event.current.type == EventType.DragPerform)
            {
                if (DragAndDrop.paths is { Length: > 0 })
                {
                    folderPath = DragAndDrop.paths[0];
                }
            }
        }

        public void OnGUI()
        {
            EditorGUILayout.LabelField("");

            getType = (ClipGetType)EditorGUILayout.EnumPopup("How to get clips?", getType);
            switch (getType)
            {
                case ClipGetType.OneClip:
                    clips = clips.Length == 1 ? clips : new AnimationClip[1];
                    clips[0] = EditorGUILayout.ObjectField(clips[0], typeof(AnimationClip), false) as AnimationClip;
                    break;
                case ClipGetType.ManyClips:
                    clips = clips.Where(c => c != null).Append(null).ToArray();
                    for (int i = 0; i < clips.Length; i++)
                        clips[i] = EditorGUILayout.ObjectField(clips[i], typeof(AnimationClip), false) as AnimationClip;
                    break;
                case ClipGetType.Folder:
                    UpdateFolderPathFromDrag();
                    folderPath = EditorGUILayout.TextField("Folder path", string.IsNullOrEmpty(folderPath) ? "Drag a folder here" : folderPath);
                    break;
            }

            EditorGUILayout.LabelField("");

            rotationError = EditorGUILayout.FloatField("Rotation Error", rotationError);
            positionError = EditorGUILayout.FloatField("Position Error", positionError);
            scaleError = EditorGUILayout.FloatField("Scale Error", scaleError);
            checkData = EditorGUILayout.Toggle("Check Data", checkData);

            if (GUILayout.Button("Reduce Keyframes"))
            {
                ReduceKeyframes(GetClips(getType));
            }
        }

        private IEnumerable<AnimationClip> GetClips(ClipGetType getType)
        {
            return getType switch
            {
                ClipGetType.ManyClips => clips,
                ClipGetType.OneClip => Enumerable.Repeat(clips[0], 1),
                ClipGetType.Folder => new DirectoryInfo(folderPath)
                    .GetFiles("*.anim", SearchOption.AllDirectories)
                    .Select(fileInfo =>
                    {
                        string file = fileInfo.FullName;
                        int startIndex = file.IndexOf("Assets", StringComparison.Ordinal);
                        if (startIndex == -1)
                        {
                            Debug.LogError($"{file} is not under folder 'Assets'");
                            return null;
                        }

                        string localPath = file[startIndex..];
                        return AssetDatabase.LoadAssetAtPath<AnimationClip>(localPath);
                    }),
                _ => Enumerable.Empty<AnimationClip>()
            };
        }

        private void ReduceKeyframes(IEnumerable<AnimationClip> clips)
        {
            KeyframeReducer reducer = new KeyframeReducer();
            AnimationClip[] animationClips = clips.Where(c => c!= null).ToArray();

            for (int i = 0; i < animationClips.Length; i++)
            {
                AnimationClip clip = animationClips[i];
                float progress = (float)i / animationClips.Length;
                EditorUtility.DisplayProgressBar($"Keyframes Reduction {i} / {animationClips.Length}...", clip.name, progress);
                try
                {
                    Debug.Log($"Keyframes Reduction for clip \"{clip.name}\"...");
                    reducer.ReduceKeyframes(clip, rotationError, positionError, scaleError, checkData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error while keyframe reducing for \"{clip.name}\": {ex.Message}");
                }
            }
            EditorUtility.ClearProgressBar();
            Debug.Log($"Keyframes Reduced for all clips");
        }
    }
}
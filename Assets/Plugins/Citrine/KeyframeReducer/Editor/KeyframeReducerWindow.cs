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
        private ClipGetType _getType = ClipGetType.OneClip;
        private AnimationClip[] _clips = { };
        private string _folderPath;

        private float _rotationError = KeyframeReducerUtils.RotationError;
        private float _positionError = KeyframeReducerUtils.PositionError;
        private float _scaleError = KeyframeReducerUtils.ScaleError;
        private bool _checkData = true;

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
                    _folderPath = DragAndDrop.paths[0];
                }
            }
        }

        public void OnGUI()
        {
            EditorGUILayout.LabelField("");

            _getType = (ClipGetType)EditorGUILayout.EnumPopup("How to get clips?", _getType);
            switch (_getType)
            {
                case ClipGetType.OneClip:
                    _clips = _clips.Length == 1 ? _clips : new AnimationClip[1];
                    _clips[0] = EditorGUILayout.ObjectField(_clips[0], typeof(AnimationClip), false) as AnimationClip;
                    break;
                case ClipGetType.ManyClips:
                    _clips = _clips.Where(c => c != null).Append(null).ToArray();
                    for (int i = 0; i < _clips.Length; i++)
                        _clips[i] = EditorGUILayout.ObjectField(_clips[i], typeof(AnimationClip), false) as AnimationClip;
                    break;
                case ClipGetType.Folder:
                    UpdateFolderPathFromDrag();
                    _folderPath = EditorGUILayout.TextField("Folder path", string.IsNullOrEmpty(_folderPath) ? "Drag a folder here" : _folderPath);
                    break;
            }

            EditorGUILayout.LabelField("");

            _rotationError = EditorGUILayout.FloatField("Rotation Error", _rotationError);
            _positionError = EditorGUILayout.FloatField("Position Error", _positionError);
            _scaleError = EditorGUILayout.FloatField("Scale Error", _scaleError);
            _checkData = EditorGUILayout.Toggle("Check Data", _checkData);

            if (GUILayout.Button("Reduce Keyframes"))
            {
                ReduceKeyframes(GetClips(_getType));
            }
        }

        private IEnumerable<AnimationClip> GetClips(ClipGetType getType)
        {
            return getType switch
            {
                ClipGetType.ManyClips => _clips,
                ClipGetType.OneClip => Enumerable.Repeat(_clips[0], 1),
                ClipGetType.Folder => new DirectoryInfo(_folderPath)
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
            AnimationClip[] animationClips = clips.Where(c => c!= null).ToArray();

            for (int i = 0; i < animationClips.Length; i++)
            {
                AnimationClip clip = animationClips[i];
                float progress = (float)i / animationClips.Length;
                EditorUtility.DisplayProgressBar($"Keyframes Reduction {i} / {animationClips.Length}...", clip.name, progress);
                try
                {
                    Debug.Log($"Keyframes Reduction for clip \"{clip.name}\"...");
                    clip.ReduceKeyframes(_rotationError, _positionError, _scaleError, _checkData);
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
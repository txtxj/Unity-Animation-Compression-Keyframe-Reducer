using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Citrine.Utils.AnimationCompression;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

namespace Citrine.Utils.AnimationCompression
{
    public class CompressWindow : EditorWindow
    {
        private float rotationError = 0.5f;
        private float positionError = 0.5f;
        private float scaleError = 0.5f;
        private bool checkData = true;

        private string path = null;

        private string[] fileAnimation = null;

        [MenuItem("Animation Tool/Keyframe Reducer")]
        public static void Init()
        {
            CompressWindow window = EditorWindow.GetWindow<CompressWindow>("Keyframe Reducer");
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

            if (GUILayout.Button("Apply") && path is not null)
            {
                Run();
            }
        }

        private void Run()
        {
            KeyframeReducer reducer = new KeyframeReducer();
            DirectoryInfo direction = new DirectoryInfo(path);
            fileAnimation = direction.GetFiles("*.anim", SearchOption.AllDirectories).Select(fileInfo => fileInfo.FullName).ToArray();
            for (int i = 1; i <= fileAnimation.Length; i++)
            {
                string file = fileAnimation[i];
                int startIndex = file.IndexOf(@"Assets", StringComparison.Ordinal);
                if (startIndex == -1)
                {
                    Debug.LogError($"{file} is not under folder 'Assets'");
                    continue;
                }
                string localPath = file[startIndex..];
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(localPath);
                EditorUtility.DisplayProgressBar("Compressing Animations...", localPath, (float)i / fileAnimation.Length);
                reducer.ReduceKeyframes(clip, rotationError, positionError, scaleError, checkData);
            }
        }
    }
}

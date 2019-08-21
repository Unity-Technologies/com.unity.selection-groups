using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Recorder;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Utj.Film
{
    [InitializeOnLoad]
    public static class RecorderControllerSettingsExtensions
    {
        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var o = EditorUtility.InstanceIDToObject(instanceID);
            return ApplyPreset(o);
        }

        static bool ApplyPreset(Object o)
        {
            var w = EditorWindow.GetWindow<RecorderWindow>();
            var p = AssetDatabase.GetAssetPath(o);
            if (o.GetType().Name == "RecorderControllerSettingsPreset")
            {
                var method = w.GetType().GetMethod("ApplyPreset", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method == null)
                {
                    Debug.LogWarning("Recorder Controller API has changed!");
                    return false;
                }
                method.Invoke(w, new[] { p });
                return true;
            }
            return false;
        }


        static RecorderControllerSettingsExtensions()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (!scene.isSubScene)
            {
                var lrs = GameObject.FindObjectOfType<LoadRecorderSettings>();
                ApplyPreset(lrs.preset);
            }
        }
    }
}


// (C) UTJ
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using Compositor = Utj.Film.Compositor;
using Compositor;
using Compositor.Util;

namespace Compositor.Util
{
    public static class SceneObjectTraverser
    {
        public static void TraverseAllScenesAndGameObjects(
            System.Action<UnityEngine.SceneManagement.Scene, GameObject, int> action
        )
        {
            var nScene = UnityEngine.SceneManagement.SceneManager.sceneCount;
            for (int iScene = 0; iScene < nScene; ++iScene)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(iScene);
                action(scene, null, -1);
                TraverseScenes(scene, action);
            }
        }

        public static void TraverseScenes(
              UnityEngine.SceneManagement.Scene scene
            , System.Action<UnityEngine.SceneManagement.Scene, GameObject, int> action
        )
        {
            var rootGos = scene.GetRootGameObjects();
            foreach (var rootGo in rootGos)
            {
                TraverseGameObjects(scene, rootGo, action, 1);
            }
        }

        public static void TraverseGameObjects(
              UnityEngine.SceneManagement.Scene scene
            , GameObject go
            , System.Action<UnityEngine.SceneManagement.Scene, GameObject, int> action
            , int depth = 0
        )
        {
            if (go == null)
            {
                return;
            }
            action(scene, go, depth);
            foreach (Transform childTransform in go.transform)
            {
                if (childTransform == go.transform)
                {
                    continue;
                }
                TraverseGameObjects(scene, childTransform.gameObject, action, depth + 1);
            }
        }
    }
}

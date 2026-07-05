using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils.Misc.Props
{
    public static class ScenePropFinder
    {
        public static (Transform dynamicTransform, Transform staticTransform) GetPropTransforms(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);

            if (!scene.isLoaded)
                throw new Exception($"Scene {sceneName} is not loaded.");

            foreach (var rootObject in scene.GetRootGameObjects())
            {
                if (!rootObject.CompareTag("Props"))
                    continue;

                var props = rootObject.transform;

                Transform dynamic = null;
                Transform statics = null;

                foreach (Transform child in props)
                {
                    if (child.CompareTag("Dynamic"))
                        dynamic = child;

                    if (child.CompareTag("Static"))
                        statics = child;
                }

                if (dynamic == null || statics == null)
                    throw new Exception("Missing Dynamic or Static prop containers.");

                return (dynamic, statics);
            }

            throw new Exception($"No Props object found in scene {sceneName}");
        }
    }
}
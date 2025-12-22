#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using SaintsBuild.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
#if SAINTSBUILD_POST_PROCESS_SCENE
using UnityEditor.Callbacks;
#endif

namespace SaintsBuild.Editor
{
    public static class Callbacks
    {
        private static bool _watchListProcessed;

        // add this to your Editor.YouStaticMethod and call this function
#if SAINTSBUILD_POST_PROCESS_SCENE
        [PostProcessScene]
#endif
        public static void OnPostProcessScene()
        {


            bool isBuilding = !Application.isPlaying;

            Scene scene = SceneManager.GetActiveScene();
#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
            Debug.Log($"#PostProcessScene# checking scene {scene.name}");
#endif

            List<IPostProcess> toProcess = new List<IPostProcess>();
            if (!_watchListProcessed)
            {
                _watchListProcessed = true;
                AssetPostprocessorWatcherList assetPostprocessorWatcherList =
                    AssetDatabase.LoadAssetAtPath<AssetPostprocessorWatcherList>(
                        "Assets/Editor Default Resources/SaintsBuild/AssetPostprocessorWatcherList.asset");
                if (assetPostprocessorWatcherList != null)
                {
                    foreach (Component component in assetPostprocessorWatcherList.components)
                    {
                        if (component != null && component is IPostProcess compPostProcess)
                        {
                            toProcess.Add(compPostProcess);
                        }
                    }

                    foreach (ScriptableObject so in assetPostprocessorWatcherList.scriptableObjs)
                    {
                        if (so != null && so is IPostProcess soPostProcess)
                        {
                            toProcess.Add(soPostProcess);
                        }
                    }
                }
            }

            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
                Debug.Log($"#PostProcessScene# get GameObject {rootGameObject.name} in scene {scene.name}");
#endif
                if(rootGameObject)
                {
                    foreach (Component transformsInChild in rootGameObject.GetComponentsInChildren(typeof(Transform),
                                 true))
                    {
                        if (transformsInChild)
                        {
                            // ReSharper disable once SuspiciousTypeConversion.Global
                            foreach (IPostProcess onSceneBuildCallback in transformsInChild
                                         .GetComponents<MonoBehaviour>().OfType<IPostProcess>())
                            {
                                if (onSceneBuildCallback is Component component && component && component.gameObject)
                                {
#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
                                    Debug.Log(
                                        $"#PostProcessScene# OnPostProcessScene from scene {scene.name}: {onSceneBuildCallback} {onSceneBuildCallback.GetType().Name}");
#endif
                                    toProcess.Add(onSceneBuildCallback);
                                    // onSceneBuildCallback.EditorOnPostProcessScene(isBuilding);
                                }
                            }
                        }
                    }
                }
            }

            foreach (IPostProcess postProcess in toProcess)
            {
                postProcess.EditorOnPostProcessScene(isBuilding);
            }

#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
            Debug.Log(
                $"#PostProcessScene# OnPostProcessScene from scene {scene.name} finished");
#endif
        }
    }
}

#endif

using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
#if SAINTSBUILD_POST_PROCESS_SCENE
using UnityEditor.Callbacks;
#endif

namespace SaintsBuild.Editor
{
    public static class Callbacks
    {
        // add this to your Editor.YouStaticMethod and call this function
#if SAINTSBUILD_POST_PROCESS_SCENE
        [PostProcessScene]
#endif
        public static void OnPostProcessScene()
        {
#if UNITY_EDITOR
            bool isBuilding = !Application.isPlaying;

            Scene scene = SceneManager.GetActiveScene();
#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
            Debug.Log($"#PostProcessScene# checking scene {scene.name}");
#endif

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
                            foreach (IPostProcessScene onSceneBuildCallback in transformsInChild
                                         .GetComponents<MonoBehaviour>().OfType<IPostProcessScene>())
                            {
                                if (onSceneBuildCallback is Component component && component && component.gameObject)
                                {
#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
                                    Debug.Log(
                                        $"#PostProcessScene# OnPostProcessScene from scene {scene.name}: {onSceneBuildCallback} {onSceneBuildCallback.GetType().Name}");
#endif
                                    onSceneBuildCallback.EditorOnPostProcessScene(isBuilding);
                                }
                            }
                        }
                    }
                }
            }

#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
            Debug.Log(
                $"#PostProcessScene# OnPostProcessScene from scene {scene.name} finished");
#endif

#endif
        }
    }
}

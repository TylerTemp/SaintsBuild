#if UNITY_EDITOR
using System;
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

            List<(PostProcessInfo info, IPostProcess process)> toProcessNormal = new List<(PostProcessInfo, IPostProcess)>();
            List<PrefabInfo> toProcessPrefabs = new List<PrefabInfo>();
            if (!_watchListProcessed)
            {
                _watchListProcessed = true;
                AssetPostprocessorWatcherList assetPostprocessorWatcherList =
                    AssetDatabase.LoadAssetAtPath<AssetPostprocessorWatcherList>(
                        "Assets/Editor Default Resources/SaintsBuild/AssetPostprocessorWatcherList.asset");
                if (assetPostprocessorWatcherList != null)
                {
                    foreach (PrefabInfo prefabInfo in assetPostprocessorWatcherList.prefabInfos)
                    {
                        if (prefabInfo.root != null && prefabInfo.component != null && prefabInfo.component is IPostProcess compPostProcess)
                        {
                            toProcessPrefabs.Add(prefabInfo);
                        }
                    }

                    foreach (ScriptableObject so in assetPostprocessorWatcherList.scriptableObjs)
                    {
                        if (so != null && so is IPostProcess soPostProcess)
                        {
                            toProcessNormal.Add((new PostProcessInfo(
                                isBuilding,
                                PostProcessType.ScriptableObject,
                                "",
                                null,
                                null,
                                so
                                ), soPostProcess));
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
                                    toProcessNormal.Add((new PostProcessInfo(
                                            isBuilding,
                                            PostProcessType.SceneGameObject,
                                            "",
                                            component.gameObject,
                                            component,
                                            null
                                        ), onSceneBuildCallback));
                                    // onSceneBuildCallback.EditorOnPostProcessScene(isBuilding);
                                }
                            }
                        }
                    }
                }
            }

            foreach ((PostProcessInfo info, IPostProcess process) in toProcessNormal)
            {
                process.EditorOnPostProcess(info);
            }

            foreach (IGrouping<GameObject, PrefabInfo> grouping in toProcessPrefabs.GroupBy(each => each.root))
            {
                string assetPath = AssetDatabase.GetAssetPath(grouping.Key);
                GameObject root = PrefabUtility.LoadPrefabContents(assetPath);

                foreach (PrefabInfo info in grouping)
                {
                    string hierarchyPath = GetTransformPath(info.component.transform);
                    // Debug.Log($"prefab try processing {info.root}@{hierarchyPath}");
                    Transform targetTransform = string.IsNullOrEmpty(hierarchyPath)? root.transform: root.transform.Find(hierarchyPath);
                    Type rawType = info.component.GetType();

                    foreach (Component component in targetTransform.GetComponents<Component>())
                    {
                        if (component.GetType() == rawType && component is IPostProcess postProcess)  // strict equal
                        {
                            // Debug.Log($"process {component} in prefab");
                            postProcess.EditorOnPostProcess(new PostProcessInfo(
                                isBuilding,
                                PostProcessType.Prefab,
                                assetPath,
                                root,
                                component,
                                null
                            ));
                        }
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(root, assetPath);
                PrefabUtility.UnloadPrefabContents(root);
            }

#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
            Debug.Log(
                $"#PostProcessScene# OnPostProcessScene from scene {scene.name} finished");
#endif
        }

        private static string GetTransformPath(Transform t)
        {
            List<string> pathSegments = new List<string>
            {
                t.name,
            };
            // string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                // path = t.name + "/" + path;
                pathSegments.Insert(0, t.name);
            }

            pathSegments.RemoveAt(0);
            return string.Join("/", pathSegments);
            // return path;
        }
    }
}

#endif

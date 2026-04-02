using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaintsBuild.Editor.Utils;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
#if SAINTSBUILD_POST_PROCESS
using UnityEditor.Build;
using UnityEditor.Callbacks;
#endif

namespace SaintsBuild.Editor
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Callbacks
#if SAINTSBUILD_POST_PROCESS
        : IPreprocessBuildWithReport, IPostprocessBuildWithReport
#endif
    {
#if SAINTSBUILD_POST_PROCESS
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("#PostProcessBuild# start to process");
            OnPreprocessBuildCallback();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            Debug.Log("#PostProcessBuild# exit, checking restore");
            OnPostprocessBuildCallback();
        }
#endif

        // ReSharper disable once MemberCanBePrivate.Global
        public static void OnPreprocessBuildCallback()
        {
            bool isBuilding = !Application.isPlaying;

            int backupIndex = 0;
            string backupFolder = AssetPostprocessorWatcherList.EnsureBackupFolder();

            AssetPostprocessorWatcherList assetPostprocessorWatcherList = AssetPostprocessorWatcherList.instance;

            using SerializedObject serList = new SerializedObject(AssetPostprocessorWatcherList.instance);
            SerializedProperty serBackup = serList.FindProperty(nameof(AssetPostprocessorWatcherList.backupInfos));
            serBackup.arraySize = 0;

            foreach (ScriptableObject so in assetPostprocessorWatcherList.scriptableObjs)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (so == null || so is not IPostProcess soPostProcess)
                {
                    continue;
                }

#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
                Debug.Log(
                    $"#PostProcess# will process so {so}");
#endif
                string source = AssetDatabase.GetAssetPath(so);
                string fileBaseName = Path.GetFileNameWithoutExtension(source);
                string backupTarget = Path.Combine(backupFolder, $"{backupIndex}_{fileBaseName}.bk").Replace("\\", "/");

#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
#else
                if(BuildPipeline.isBuildingPlayer)
#endif
                {
                    Debug.Log(
                        $"#PostProcess# backup {source} to {backupTarget} ({so})");
                }

                File.Copy(source, backupTarget, true);

                bool needRestore = soPostProcess.EditorOnPostProcess(new PostProcessInfo(
                    isBuilding,
                    PostProcessType.ScriptableObject,
                    "",
                    null,
                    null,
                    so));
                if (needRestore)
                {
                    AddToSerList(serBackup, source, backupTarget);
                }
            }

            Dictionary<GameObject, List<PrefabInfo>> toProcessPrefabToInfo = new Dictionary<GameObject, List<PrefabInfo>>();
            foreach (PrefabInfo prefabInfo in assetPostprocessorWatcherList.prefabInfos)
            {
                if (prefabInfo.root != null && prefabInfo.component != null && prefabInfo.component is IPostProcess)
                {
                    if (!toProcessPrefabToInfo.TryGetValue(prefabInfo.root, out List<PrefabInfo> toProcessInfo))
                    {
                        toProcessInfo = toProcessPrefabToInfo[prefabInfo.root] = new List<PrefabInfo>();
                    }
                    toProcessInfo.Add(prefabInfo);
                }
            }

            foreach (KeyValuePair<GameObject, List<PrefabInfo>> kv in toProcessPrefabToInfo)
            {
                string assetPath = AssetDatabase.GetAssetPath(kv.Key);
                string fileBaseName = Path.GetFileNameWithoutExtension(assetPath);
                string backupTarget = Path.Combine(backupFolder, $"{backupIndex}_{fileBaseName}.bk").Replace("\\", "/");
                backupIndex += 1;
#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
#else
                if(BuildPipeline.isBuildingPlayer)
#endif
                {
                    Debug.Log(
                        $"#PostProcessScene# backup {assetPath} to {backupTarget} (prefab)");
                }

                File.Copy(assetPath, backupTarget, true);

                GameObject root = PrefabUtility.LoadPrefabContents(assetPath);

                bool needRestore = false;

                foreach (PrefabInfo info in kv.Value)
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
#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
                            Debug.Log(
                                $"#PostProcessScene# call EditorOnPostProcess on {component} at {assetPath} (prefab)");
#endif
                            // Undo.RecordObject(component, $"SaintsBuild processing {component}");
                            bool thisNeedRestore = postProcess.EditorOnPostProcess(new PostProcessInfo(
                                isBuilding,
                                PostProcessType.Prefab,
                                assetPath,
                                root,
                                component,
                                null
                            ));
                            if (thisNeedRestore)
                            {
                                needRestore = true;
                            }
                        }
                    }
                }

                if(needRestore)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, assetPath);
                    PrefabUtility.UnloadPrefabContents(root);

                    AddToSerList(serBackup, assetPath, backupTarget);
                }
            }

            serList.ApplyModifiedPropertiesWithoutUndo();
            // AssetPostprocessorWatcherList.instance.SaveToDisk();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static void OnPostprocessBuildCallback()
        {
            AssetPostprocessorWatcherList.instance.RestoreFromBackupAndClear();
        }

        private static bool _watchListProcessed;

        // add this to your Editor.YouStaticMethod and call this function
#if SAINTSBUILD_POST_PROCESS
        [PostProcessScene]
#endif
        public static void OnPostProcessScene()
        {
            bool isBuilding = !Application.isPlaying;

            Scene scene = SceneManager.GetActiveScene();
#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
            Debug.Log($"#PostProcessScene# checking scene {scene.name}");
#endif

            if (!_watchListProcessed
                    // building processor will handle asset when building the game
                    // thus we don't handle it here
                    && !BuildPipeline.isBuildingPlayer
                )
            {
                _watchListProcessed = true;
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                OnPreprocessBuildCallback();
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
                                    onSceneBuildCallback.EditorOnPostProcess(new PostProcessInfo(
                                        isBuilding,
                                        PostProcessType.SceneGameObject,
                                        "",
                                        component.gameObject,
                                        component,
                                        null
                                    ));
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
        }

        private static void AddToSerList(SerializedProperty serBackup, string assetPath, string backupTarget)
        {
            int index = serBackup.arraySize;
            serBackup.arraySize += 1;
            SerializedProperty serBackupInfo = serBackup.GetArrayElementAtIndex(index);
            serBackupInfo.FindPropertyRelative(nameof(BackupInfo.assetPath)).stringValue = assetPath;
            serBackupInfo.FindPropertyRelative(nameof(BackupInfo.backupPath)).stringValue = backupTarget;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                AssetPostprocessorWatcherList.instance.RestoreFromBackupAndClear();
            }
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

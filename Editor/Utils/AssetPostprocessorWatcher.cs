using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SaintsBuild.Editor.Utils
{
    public class AssetPostprocessorWatcher: AssetPostprocessor
    {
        // private static AssetPostprocessorWatcherList _assetPostprocessorWatcherList;

        [InitializeOnLoadMethod]
        private static void EnsureAssetPostprocessorWatcherList()
        {
            if (!Directory.Exists("Assets/Editor Default Resources"))
            {
                Debug.Log("Create folder: Assets/Editor Default Resources");
                AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
            }

            if (!Directory.Exists("Assets/Editor Default Resources/SaintsBuild"))
            {
                Debug.Log("Create folder: Assets/Editor Default Resources/SaintsBuild");
                AssetDatabase.CreateFolder("Assets/Editor Default Resources", "SaintsBuild");
            }
            const string path = "Assets/Editor Default Resources/SaintsBuild/AssetPostprocessorWatcherList.asset";
            if (!File.Exists(path))
            {
                AssetPostprocessorWatcherList assetPostprocessorWatcherList = AssetPostprocessorWatcherList.instance;
                Debug.Log("Create AssetPostprocessorWatcherList");
                AssetDatabase.CreateAsset(assetPostprocessorWatcherList, path);
                AssetDatabase.SaveAssets();
            }
        }



        // importedAssets to check scriptableObject
        // delete trigger we wants
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (EditorApplication.isPlaying)  // don't use EditorApplication.isPlayingOrWillChangePlaymode
            {
                return;
            }

            if (EditorApplication.isCompiling)
            {
                return;
            }

            if (BuildPipeline.isBuildingPlayer)
            {
                return;
            }

            // if (EditorApplication.timeSinceStartup < 0.1f)  // Too short time, might be a domain-reload
            // {
            //     return;
            // }
            //
            // if (AssetDatabase.IsAssetImportWorkerProcess())  // still importing, skip checking too
            // {
            //     return;
            // }

            // if (EditorApplication.isUpdating)  // edit busy, skip this too
            // {
            //     return;
            // }

            AssetPostprocessorWatcherList watchedList = AssetPostprocessorWatcherList.instance;
            if (watchedList.backupInfos.Count > 0)  // don't update if there are backups needs restore
            {
                return;
            }

            List<ScriptableObject> toAddSo = new List<ScriptableObject>();
            List<PrefabInfo> toAddComponents = new List<PrefabInfo>();

            // List<int> toDeleteSoIndex = new List<int>();
            // List<int> toDeleteComponentIndex = new List<int>();

            // List<UnityEngine.Object> importedObjs = new List<Object>();

            foreach (string importedAsset in importedAssets)
            {
                if (importedAsset.EndsWith(".asset"))
                {
                    ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(importedAsset);
                    if (so != null)
                    {
                        // importedObjs.Add(so);
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        if (so is IPostProcess && !toAddSo.Contains(so) && !watchedList.scriptableObjs.Contains(so))
                        {
                            toAddSo.Add(so);
                        }
                    }
                }
                else if (importedAsset.EndsWith(".prefab"))
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(importedAsset);
                    // Debug.Log($"{go}: {importedAsset}");
                    // importedObjs.Add(go);
                    if (go != null)
                    {
                        foreach (Component component in go.GetComponentsInChildren<Component>(true))
                        {
                            // Debug.Log($"comp {component}: {component is IPostProcess}, {toAddComponents.All(each => each.root != go && each.component != component)}");
                            // ReSharper disable once SuspiciousTypeConversion.Global
                            PrefabInfo checkContent = new PrefabInfo
                            {
                                root = go,
                                component = component,
                            };
                            if (component != null && component is IPostProcess && !toAddComponents.Contains(checkContent) && !watchedList.prefabInfos.Contains(checkContent))
                            {
                                // Debug.Log($"{checkContent} not in {string.Join(",", toAddComponents)}");
                                toAddComponents.Add(checkContent);
                            }
                        }
                    }
                }
            }

            // // check delete
            // for (int index = watchedList.prefabInfos.Length - 1; index >= 0; index--)
            // {
            //     PrefabInfo target = watchedList.prefabInfos[index];
            //     Component comp = target.component;
            //     GameObject root = target.root;
            //     if (importedObjs.Contains(root))
            //     {
            //         continue;
            //     }
            //
            //     // ReSharper disable once SuspiciousTypeConversion.Global
            //     if (comp == null || comp is not IPostProcess)
            //     {
            //         Debug.Log($"Component {comp} is not target, will get delete at {index}");
            //         toDeleteComponentIndex.Add(index);
            //     }
            // }
            // for (int index = watchedList.scriptableObjs.Length - 1; index >= 0; index--)
            // {
            //     ScriptableObject so = watchedList.scriptableObjs[index];
            //     if (importedObjs.Contains(so))
            //     {
            //         continue;
            //     }
            //
            //     // ReSharper disable once SuspiciousTypeConversion.Global
            //     if (so == null || so is not IPostProcess)
            //     {
            //         toDeleteSoIndex.Add(index);
            //     }
            // }

            // if (toAddSo.Count == 0 && toDeleteSoIndex.Count == 0 && toDeleteComponentIndex.Count == 0 && toAddComponents.Count == 0)
            if (toAddSo.Count != 0 || toAddComponents.Count != 0)
            {
                using SerializedObject so = new SerializedObject(watchedList);

                // int toAddSoCount = toAddSo.Count;
                SerializedProperty soPropArray =
                    so.FindProperty(nameof(AssetPostprocessorWatcherList.scriptableObjs));

                foreach (ScriptableObject target in toAddSo)
                {
                    int toAddIndex = soPropArray.arraySize;
                    soPropArray.arraySize = toAddIndex + 1;
                    SerializedProperty soPropItem = soPropArray.GetArrayElementAtIndex(toAddIndex);
                    Debug.Log($"Add {target}@[{toAddIndex}] to watched scriptableObjects");
                    soPropItem.objectReferenceValue = target;
                }

                SerializedProperty compPropArray =
                    so.FindProperty(nameof(AssetPostprocessorWatcherList.prefabInfos));

                foreach (PrefabInfo target in toAddComponents)
                {
                    int toAddIndex = compPropArray.arraySize;
                    compPropArray.arraySize = toAddIndex + 1;
                    SerializedProperty compPropItem = compPropArray.GetArrayElementAtIndex(toAddIndex);
                    Debug.Log($"Add {target}@[{toAddIndex}] to watched comp");
                    compPropItem.FindPropertyRelative(nameof(PrefabInfo.root)).objectReferenceValue = target.root;
                    compPropItem.FindPropertyRelative(nameof(PrefabInfo.component)).objectReferenceValue =
                        target.component;
                }
                so.ApplyModifiedPropertiesWithoutUndo();
                // AssetPostprocessorWatcherList.instance.SaveToDisk();
            }

            // Don't check right now, some asset might not be ready
            EditorApplication.delayCall += DelayCheckDelete;
        }

        private static void DelayCheckDelete()
        {
            EditorApplication.delayCall += CheckDelete;
        }

        private static void CheckDelete()
        {
            AssetPostprocessorWatcherList watchedList = AssetPostprocessorWatcherList.instance;
            if (watchedList.backupInfos.Count > 0)  // don't update if there are backups needs restore
            {
                return;
            }

            List<int> toDeleteSoIndex = new List<int>();
            List<int> toDeleteComponentIndex = new List<int>();

            for (int index = watchedList.prefabInfos.Length - 1; index >= 0; index--)
            {
                PrefabInfo target = watchedList.prefabInfos[index];
                Component comp = target.component;

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (comp == null || comp is not IPostProcess)
                {
                    Debug.Log($"Component {comp} is not target, will get delete at {index}");
                    toDeleteComponentIndex.Add(index);
                }
            }
            for (int index = watchedList.scriptableObjs.Length - 1; index >= 0; index--)
            {
                ScriptableObject so = watchedList.scriptableObjs[index];

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (so == null || so is not IPostProcess)
                {
                    toDeleteSoIndex.Add(index);
                }
            }

            if (toDeleteComponentIndex.Count == 0)
            {
                return;
            }

            using (SerializedObject so = new SerializedObject(watchedList))
            {
                // int toAddSoCount = toAddSo.Count;
                SerializedProperty soPropArray = so.FindProperty(nameof(AssetPostprocessorWatcherList.scriptableObjs));
                SerializedProperty compPropArray = so.FindProperty(nameof(AssetPostprocessorWatcherList.prefabInfos));

                foreach (int toDeleteComp in toDeleteComponentIndex)
                {
                    Debug.Log($"Delete [{toDeleteComp}] from watched components");
                    compPropArray.DeleteArrayElementAtIndex(toDeleteComp);
                }

                foreach (int toDeleteSo in toDeleteSoIndex)
                {
                    Debug.Log($"Delete [{toDeleteSo}] from watched scriptableObject");
                    soPropArray.DeleteArrayElementAtIndex(toDeleteSo);
                }

                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // private static readonly List<Component> ToAddComponents = new List<Component>();
        // private void OnPostprocessPrefab(GameObject go)
        // {
        //     // List<Component> toAddComponents = new List<Component>();
        //     foreach (Component component in go.GetComponentsInChildren<Component>(true))
        //     {
        //         // ReSharper disable once SuspiciousTypeConversion.Global
        //         if (component != null && component is IPostProcess && !ToAddComponents.Contains(component))
        //         {
        //             ToAddComponents.Add(component);
        //         }
        //     }
        //
        //     // if (toAddComponents.Count == 0)
        //     // {
        //     //     return;
        //     // }
        //     //
        //     // using SerializedObject so = new SerializedObject(EnsureAssetPostprocessorWatcherList());
        //     // SerializedProperty compPropArray = so.FindProperty(nameof(AssetPostprocessorWatcherList.components));
        //     //
        //     // foreach (Component addComponent in toAddComponents)
        //     // {
        //     //     int index = compPropArray.arraySize;
        //     //     compPropArray.arraySize += 1;
        //     //     SerializedProperty itemProp = compPropArray.GetArrayElementAtIndex(index);
        //     //     itemProp.objectReferenceValue = addComponent;
        //     //     Debug.Log($"Add component {addComponent}@[{index}] to watch list: {itemProp.objectReferenceValue}");
        //     // }
        //     // so.ApplyModifiedProperties();
        // }
    }
}

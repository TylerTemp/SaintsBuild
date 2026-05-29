using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SaintsBuild.Editor.Utils
{
    [FilePath("Assets/Editor Default Resources/SaintsBuild/AssetPostprocessorWatcherList.asset", FilePathAttribute.Location.ProjectFolder)]
    public class AssetPostprocessorWatcherList: ScriptableSingleton<AssetPostprocessorWatcherList>
    {
        private void OnEnable()
        {
            hideFlags &= ~HideFlags.NotEditable;
        }

        private const string BackupTargetFolder = "Library/SaintsBuildBackup";

        public PrefabInfo[] prefabInfos = {};
        public ScriptableObject[] scriptableObjs = {};

        public List<BackupInfo> backupInfos = new List<BackupInfo>();

        // This is very broken...
        public void SaveToDisk()
        {
            using(new DisableUnityLogScoop())
            {
                try
                {
                    Save(true);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public static string EnsureBackupFolder()
        {
            if (!Directory.Exists(BackupTargetFolder))
            {
                Directory.CreateDirectory(BackupTargetFolder);
            }
            return BackupTargetFolder;
        }

        public void RestoreFromBackupAndClear()
        {
            if (backupInfos.Count == 0)
            {
                return;
            }

            List<string> reImports = new List<string>();
            foreach (BackupInfo backupInfo in backupInfos)
            {
#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
#else
                if(BuildPipeline.isBuildingPlayer)
#endif
                {
                    Debug.Log($"#PostProcess# restore {backupInfo.assetPath} from {backupInfo.backupPath}");
                }
                File.Copy(backupInfo.backupPath, backupInfo.assetPath, true);
                reImports.Add(backupInfo.assetPath);
            }

            foreach (string reImport in reImports)
            {
#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
                Debug.Log($"reimport {reImport}");
#endif
                AssetDatabase.ImportAsset(reImport);
            }

            EditorApplication.delayCall += () =>
            {
#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
                Debug.Log("clean list");
#endif
                backupInfos.Clear();

                // ReSharper disable once InvertIf
                // Can happen after build processer
                if(this != null)
                {
                    using SerializedObject serializedObject = new SerializedObject(this);
                    serializedObject.FindProperty(nameof(backupInfos)).arraySize = 0;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            };
        }
    }
}

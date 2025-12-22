using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace SaintsBuild.Editor.Utils
{
    public static class SaintsMenu
    {
#if SAINTSBUILD_POST_PROCESS_SCENE
        [MenuItem("Window/Saints/Enable Scene Post Process")]
        public static void PostProcessScene() => RemoveCompileDefine("SAINTSBUILD_POST_PROCESS_SCENE");
#else
        [MenuItem("Window/Saints/Disable Scene Post Process")]
        public static void PostProcessScene() => AddCompileDefine("SAINTSBUILD_POST_PROCESS_SCENE");
#endif

        // ReSharper disable once UnusedMember.Local
        private static void AddCompileDefine(string newDefineCompileConstant, IEnumerable<BuildTargetGroup> targetGroups = null)
        {
            IEnumerable<BuildTargetGroup> targets = targetGroups ?? Enum.GetValues(typeof(BuildTargetGroup)).Cast<BuildTargetGroup>();

            foreach (BuildTargetGroup grp in targets.Where(each => each != BuildTargetGroup.Unknown))
            {
                AddCompileForGroup(newDefineCompileConstant, grp);
            }
        }

        private static void AddCompileForGroup(string newDefineCompileConstant, BuildTargetGroup grp)
        {
#if UNITY_6000_0_OR_NEWER
            try
            {
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(grp),
                    newDefineCompileConstant);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#else
            string defines;
            try
            {
                defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(grp);
            }
            catch (ArgumentException)
            {
                return;
            }

            if (defines.Contains(newDefineCompileConstant))
            {
                return;
            }

            if (defines.Length > 0)
            {
                defines += ";";
            }

            defines += newDefineCompileConstant;
            try
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(grp, defines);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#endif
        }

        // ReSharper disable once UnusedMember.Local
        private static void RemoveCompileDefine(string defineCompileConstant, IEnumerable<BuildTargetGroup> targetGroups = null)
        {
            IEnumerable<BuildTargetGroup> targets = targetGroups ?? Enum.GetValues(typeof(BuildTargetGroup)).Cast<BuildTargetGroup>();

            foreach (BuildTargetGroup grp in targets.Where(each => each != BuildTargetGroup.Unknown))
            {
                string defines;
                try
                {
                    defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(grp);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                string result = string.Join(";", defines
                    .Split(';')
                    .Select(each => each.Trim())
                    .Where(each => each != defineCompileConstant));

                // Debug.Log(result);

                try
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(grp, result);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}

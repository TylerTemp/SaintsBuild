using System;
using UnityEngine;

namespace SaintsBuild
{
    public enum PostProcessType
    {
        SceneGameObject,
        Prefab,
        ScriptableObject,
    }

    public readonly struct PostProcessInfo
    {
        public readonly bool IsBuilding;
        public readonly PostProcessType Type;
        public readonly string PrefabPath;
        public readonly GameObject Prefab;
        public readonly Component Component;
        public readonly ScriptableObject ScriptableObject;

        public PostProcessInfo(
            bool isBuilding,
            PostProcessType type,
            string prefabPath,
            GameObject prefab,
            Component component,
            ScriptableObject scriptableObject)
        {
            IsBuilding = isBuilding;
            Type = type;
            PrefabPath = prefabPath;
            Prefab = prefab;
            Component = component;
            ScriptableObject = scriptableObject;
        }

        public bool PrefabDangerousDestroy() => Type == PostProcessType.Prefab && !IsBuilding;
    }
}

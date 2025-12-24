using System;
using UnityEngine;

namespace SaintsBuild.Editor.Utils
{
    [Serializable]
    public struct PrefabInfo: IEquatable<PrefabInfo>
    {
        public GameObject root;
        public Component component;

        public bool Equals(PrefabInfo other)
        {
            return Equals(root, other.root) && Equals(component, other.component);
        }

        public override bool Equals(object obj)
        {
            return obj is PrefabInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(root, component);
        }

        public override string ToString()
        {
            return $"{root.name}->{component}";
        }
    }
}

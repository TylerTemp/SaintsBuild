using System;
using UnityEngine;

namespace SaintsBuild.Samples
{
    public class PrefabCreator : MonoBehaviour
    {
        public GameObject prefab;

        private void Start()
        {
            Instantiate(prefab, transform);
        }
    }
}

using UnityEngine;

namespace SaintsBuild.Samples.Scripts
{
    public class PrefabCreator : MonoBehaviour
    {
        public GameObject prefab;

        private void Start()
        {
            Debug.Log("Prefab Creator Start");
            Instantiate(prefab, transform);
        }
    }
}

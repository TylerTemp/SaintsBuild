using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SaintsBuild.Samples
{
    public class TextContainer : MonoBehaviour, IPostProcessScene
    {
        public GameObject prefab;
        public Transform container;

        private void Awake()
        {
#if UNITY_EDITOR
            // in build, the clean process will happen before build
            // but in play mode, we need to call it manually
            CleanUpExample();
#endif

            // other works
            foreach (int index in Enumerable.Range(0, 5))
            {
                GameObject example = Instantiate(prefab, container);
                example.GetComponent<Text>().text = $"Runtime Awake created {index}";
            }
        }

#if UNITY_EDITOR
        public void EditorOnPostProcessScene(bool isBuilding)
        {
            if (isBuilding)  // in building process, Unity will call this function and apply changes to build result
            {
                CleanUpExample();
            }
            else  // in play mode, unity will call Awake first, then call this function. Changes will be revoked after exiting play mode
            {
                // deal conflict with Awake
            }
        }

        private void CleanUpExample()
        {
            foreach (Transform eachExample in container.Cast<Transform>().ToArray())
            {
                Debug.Log(eachExample.gameObject.name);
                DestroyImmediate(eachExample.gameObject);
            }
        }
#endif
    }
}

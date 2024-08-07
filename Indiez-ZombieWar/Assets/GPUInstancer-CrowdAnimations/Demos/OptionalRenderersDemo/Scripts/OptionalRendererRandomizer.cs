#if GPU_INSTANCER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer.CrowdAnimations
{
    public class OptionalRendererRandomizer : MonoBehaviour
    {
        public GPUICrowdManager gpuiCrowdManager;
        public GPUICrowdPrototype crowdPrototype;

        private void OnEnable()
        {
            StartCoroutine(RandomizeCoroutine());
        }

        public IEnumerator RandomizeCoroutine()
        {
            if (gpuiCrowdManager != null)
            {
                while (!gpuiCrowdManager.isInitialized)
                    yield return null;
                RandomizeOptionalRenderers();
            }
        }

        public void RandomizeOptionalRenderers()
        {
            if (gpuiCrowdManager != null)
            {
                Dictionary<GPUInstancerPrototype, List<GPUInstancerPrefab>> registeredPrefabsDict = gpuiCrowdManager.GetRegisteredPrefabsRuntimeData();
                List<GPUInstancerPrefab> prefabList;
                if (registeredPrefabsDict != null && registeredPrefabsDict.TryGetValue(crowdPrototype, out prefabList))
                {
                    foreach (GPUICrowdPrefab crowdPrefab in prefabList)
                    {
                        foreach (GPUICrowdPrefab child in crowdPrefab.childCrowdPrefabs)
                        {
                            child.gameObject.SetActive(Random.value > 0.25f);
                        }
                    }
                }
            }
        }
    }
}
#endif //GPU_INSTANCER

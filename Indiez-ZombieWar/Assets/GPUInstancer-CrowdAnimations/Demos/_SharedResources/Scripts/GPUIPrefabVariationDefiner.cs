using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer
{
    public class GPUIPrefabVariationDefiner : MonoBehaviour
    {
        public GPUInstancerPrefabManager prefabManager;
        public GPUInstancerPrefabPrototype prototype;

        public readonly string textureBufferName = "textureUVBuffer";
        public readonly string texturePropertyName = "_TextureUV";

        void Awake()
        {
            if (prefabManager == null)
                return;
            GPUInstancerAPI.DefinePrototypeVariationBuffer<Vector4>(prefabManager, prototype, textureBufferName);
        }

        private void Reset()
        {
            if (prefabManager == null)
                prefabManager = FindObjectOfType<GPUInstancerPrefabManager>();
        }
    }
}
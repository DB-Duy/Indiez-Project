using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif //UNITY_EDITOR
using UnityEngine;

namespace GPUInstancer
{
    [ExecuteInEditMode]
    public class GPUIPrefabVariationHandler : MonoBehaviour
    {
        public GPUIPrefabVariationDefiner variationDefiner;
        [HideInInspector]
        public Vector4 textureUV;
#if UNITY_EDITOR
        [HideInInspector]
        public Vector4 cachedUV;
#endif //UNITY_EDITOR

        public void Start()
        {
            if (variationDefiner == null)
                return;
            GPUInstancerPrefab gpuInstancerPrefab = GetComponent<GPUInstancerPrefab>();
            if (gpuInstancerPrefab.state == PrefabInstancingState.Instanced)
            {
                GPUInstancerAPI.UpdateVariation(variationDefiner.prefabManager, gpuInstancerPrefab, variationDefiner.textureBufferName, textureUV);
            }
            else
            {
                SkinnedMeshRenderer[] meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer mr in meshRenderers)
                {
                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                    //mr.GetPropertyBlock(mpb);
                    mpb.SetVector(variationDefiner.texturePropertyName, textureUV);

                    mr.SetPropertyBlock(mpb);
                }
            }
#if UNITY_EDITOR
            cachedUV = textureUV;
#endif //UNITY_EDITOR
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GPUIPrefabVariationHandler))]
    public class GPUICrowdManagerEditor : Editor
    {
        protected SerializedProperty prop_textureUV;

        private GPUIPrefabVariationHandler _variationHandler;

        protected virtual void OnEnable()
        {
            _variationHandler = (target as GPUIPrefabVariationHandler);

            prop_textureUV = serializedObject.FindProperty("textureUV");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(prop_textureUV, new GUIContent("Texture UV"));

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck() || prop_textureUV.vector4Value != _variationHandler.cachedUV)
            {
                _variationHandler.Start();
                SceneView.RepaintAll();
            }
        }
    }
#endif //UNITY_EDITOR
}
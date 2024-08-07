#if GPU_INSTANCER
using UnityEngine;
using UnityEngine.UI;

namespace GPUInstancer.CrowdAnimations
{
    public class NavMeshDemoSceneGUIHandler : MonoBehaviour
    {
        [Header("Script References")]
        public GPUICrowdManager gpuiCrowdManager;
        public GPUInstancerLODColorDebugger gpuiLODColorDebugger;
        public NavMeshDemoSceneController demoSceneController;

        [Header("GUI References")]
        public Image imageManagerToggle;
        public GameObject panelPrototypeControl;
        public Text textInstanceInfo;

        private string _prototypeInfoText;

        private void Start()
        {
            SetPrototypeInfoText();
        }

        public void ToggleCrowdManager(bool isEnabled)
        {
            if (gpuiCrowdManager == null)
                return;

            gpuiCrowdManager.enabled = isEnabled;

            demoSceneController.isGPUIManagerActive = isEnabled;

            imageManagerToggle.color = isEnabled ? Color.green : Color.red;

            panelPrototypeControl.SetActive(isEnabled);

            if (isEnabled)
                demoSceneController.ResetAnimations();
        }

        public void SetPrototypeInfoText()
        {
            GPUICrowdPrototype crowdPrototype = (GPUICrowdPrototype)gpuiCrowdManager.prototypeList[0];
            _prototypeInfoText = crowdPrototype.prefabObject.name;
            GPUInstancerRuntimeData runtimeData = gpuiCrowdManager.GetRuntimeData(crowdPrototype);
            if (runtimeData != null && runtimeData.instanceLODs.Count > 0)
            {
                _prototypeInfoText += "\nVertex Counts: ";
                for (int i = 0; i < runtimeData.instanceLODs.Count; i++)
                {
                    _prototypeInfoText += " LOD" + i + ": " + runtimeData.instanceLODs[i].renderers[0].mesh.vertexCount;
                }
            }
            _prototypeInfoText += "\nInstance Count: ";
            textInstanceInfo.text = _prototypeInfoText + demoSceneController.instanceCount;
        }

        public void ToggleLODColors(bool isColored)
        {
            if (gpuiLODColorDebugger == null || gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;
            gpuiLODColorDebugger.enabled = isColored;
        }

        public void OnAddInstances()
        {
            demoSceneController.AddAgents(100);
            demoSceneController.instanceCount += 100;
            SetPrototypeInfoText();
        }

    }
}
#endif //GPU_INSTANCER
#if GPU_INSTANCER
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GPUInstancer.CrowdAnimations
{
    /// <summary>
    /// This Demo scene controller shows an example usage of handling the Crowd Manager with various use cases. 
    /// Check the CrowdAnimationsDemoScene under [/GPUInstancer-CrowdAnimations/Demos/CrowdAnimationsDemo] to see it in action.
    /// </summary>
    public class CrowdAnimationsDemoSceneHandler : MonoBehaviour
    {
        public GPUICrowdManager gpuiCrowdManager; // reference to the Crowd Manager. The crowd prototypes are defined and their animations 
                                                  // already baked in the scene before this script runs.
        public GPUInstancerLODColorDebugger gpuiLODColorDebugger; // reference to the The LOD Color Debuger tool.
        public GPUICrowdAnimatorRandomizer gpuiCrowdAnimatorRandomizer; // reference to the The Crowd Animator Randomizer tool.

        // GUI references
        public Image imageManagerToggle;
        public Button buttonMecanimAnimator;
        public Button buttonCrowdAnimator;
        public Text textSpeedValue;
        public Slider sliderSpeed;
        public Button buttonRandomizeClips;
        public Button buttonRandomizeFrames;
        public Button buttonRandomizeSpeeds;
        public Button buttonResetAnimations;
        public Image imageLODWithColorToggle;
        public GameObject panelAnimatorControl;
        public GameObject panelPrototypeControl;
        public Text textInstanceCount;

        // demo specific internal fields
        private int _selectedPrototypeIndex = 0;
        private int _rowCount = 30;
        private int _collumnCount = 30;
        private float _space = 1.5f;

        private List<GPUInstancerPrototype> _prototypeList; // Will be used to cache the list of crowd prototypes on the Crowd Manager
        private string _prototypeNameText;
        private List<GPUInstancerPrefab> _instanceList;

        private void Start()
        {
            if (gpuiCrowdManager == null)
                return;

            // Disabling the Crowd Manager here to change prototype settings. 
            // Enabling it after this will make it re-initialize with the new settings for the prototypes
            gpuiCrowdManager.enabled = false;

            _instanceList = new List<GPUInstancerPrefab>();
            // Setup the prototype in the manager
            GPUICrowdPrototype crowdPrototype = (GPUICrowdPrototype)gpuiCrowdManager.prototypeList[_selectedPrototypeIndex];
            crowdPrototype.animationData.useCrowdAnimator = true; // indicate the Crowd Animator Workflow will be used initially

            // Edit runtime properties for this prototype:
            crowdPrototype.enableRuntimeModifications = true;   // Enable runtime modifications to be able to...
            crowdPrototype.addRemoveInstancesAtRuntime = true;  // add and remove instances at runtime ...
            crowdPrototype.extraBufferSize = 10000;             // with this amount of extra instances that can be added after the initial ones.

            // Instantiate instance GOs:
            GameObject prefabObject = crowdPrototype.prefabObject;
            Vector3 pos = Vector3.zero;
            Quaternion rotation = Quaternion.Euler(0, 180, 0) * prefabObject.transform.rotation;
            for (int r = 0; r < _rowCount; r++)
            {
                for (int c = 0; c < _collumnCount; c++)
                {
                    pos.x = _space * r;
                    pos.z = _space * c;

                    GameObject instanceGO = Instantiate(prefabObject, pos, rotation);
                    _instanceList.Add(instanceGO.GetComponent<GPUICrowdPrefab>());
                }
            }
            
            // Register the instantiated GOs to the Crowd Manager
            GPUInstancerAPI.RegisterPrefabInstanceList(gpuiCrowdManager, _instanceList);

            // Enabling the Crowd Manager back; this will re-initialize it with the new settings for the prototypes
            gpuiCrowdManager.enabled = true;

            _prototypeNameText = crowdPrototype.prefabObject.name;

            // Get the Runtime Data for the current protoype to display information anbout it
            GPUInstancerRuntimeData runtimeData = gpuiCrowdManager.GetRuntimeData(crowdPrototype);
            if (runtimeData != null && runtimeData.instanceLODs.Count > 0)
            {
                _prototypeNameText += "\nVertex Counts: ";
                for (int i = 0; i < runtimeData.instanceLODs.Count; i++)
                {
                    _prototypeNameText += " LOD" + i + ": " + runtimeData.instanceLODs[i].renderers[0].mesh.vertexCount; // Rendered vertex count
                }
            }
            _prototypeNameText += "\nInstance Count: ";
            textInstanceCount.text = _prototypeNameText + gpuiCrowdManager.runtimeDataList[_selectedPrototypeIndex].instanceCount; // Rendered instance count

            _prototypeList = gpuiCrowdManager.prototypeList; // cache the prototype list on the Manager to access later
        }

        private void OnDestroy()
        {
            if (_prototypeList == null)
                return;

            // We reset the protoypes back to using the Crowd Animator workflow, since changes would persist in the prototype data.
            foreach (GPUICrowdPrototype prototype in _prototypeList)
            {
                prototype.animationData.useCrowdAnimator = true;
            }
        }

        // Handles the LOD Color Debuger tool. Called from GUI.
        public void ChangeLODColors(bool isColored)
        {
            if (gpuiLODColorDebugger == null || gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;
            gpuiLODColorDebugger.enabled = isColored;
        }

        // Handles the Crowd Animator Randomizer tool. Called from GUI.
        public void RandomizeClips(bool randomizeClips)
        {
            if (gpuiCrowdAnimatorRandomizer == null || gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;
            gpuiCrowdAnimatorRandomizer.enabled = false;
            gpuiCrowdAnimatorRandomizer.randomizeClips = randomizeClips;
            gpuiCrowdAnimatorRandomizer.randomizeFrame = false;
            gpuiCrowdAnimatorRandomizer.resetAnimations = false;
            gpuiCrowdAnimatorRandomizer.enabled = true;
        }

        // Handles the Crowd Animator Randomizer tool. Called from GUI.
        public void RandomizeFrames(bool randomizeFrame)
        {
            if (gpuiCrowdAnimatorRandomizer == null || gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;
            gpuiCrowdAnimatorRandomizer.enabled = false;
            gpuiCrowdAnimatorRandomizer.randomizeClips = false;
            gpuiCrowdAnimatorRandomizer.randomizeFrame = randomizeFrame;
            gpuiCrowdAnimatorRandomizer.resetAnimations = false;
            gpuiCrowdAnimatorRandomizer.enabled = true;
        }

        // Handles the Crowd Animator Randomizer tool. Called from GUI.
        public void ResetAnimations()
        {
            if (gpuiCrowdAnimatorRandomizer == null || gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;
            gpuiCrowdAnimatorRandomizer.enabled = false;
            gpuiCrowdAnimatorRandomizer.resetAnimations = true;
            gpuiCrowdAnimatorRandomizer.enabled = true;
            textSpeedValue.text = "1.00";
            sliderSpeed.value = 1;
        }

        // Turns the Crowd Manager on and off. Called from GUI.
        public void ToggleCrowdManager(bool enabled)
        {
            if (gpuiCrowdManager == null)
                return;
            gpuiCrowdManager.enabled = enabled;
            if (enabled)
                imageManagerToggle.color = Color.green;
            else
                imageManagerToggle.color = Color.red;

            panelAnimatorControl.SetActive(enabled);
            panelPrototypeControl.SetActive(enabled);
            textInstanceCount.gameObject.SetActive(enabled);
    }

        // Toggles the Animation Workflows for the current prototype. Called from GUI.
        public void ToggleCrowdAnimator(bool enabled)
        {
            if (gpuiCrowdManager == null)
                return;

            gpuiCrowdManager.enabled = false; // Disable the manager to make changes to the prototype
            ((GPUICrowdPrototype)gpuiCrowdManager.prototypeList[_selectedPrototypeIndex]).animationData.useCrowdAnimator = enabled; // change prototype's workflow
            gpuiCrowdManager.enabled = true; // Enable the manager back on to initialize the prototype with the changes (the toggled workflow)

            buttonCrowdAnimator.interactable = !enabled;
            buttonMecanimAnimator.interactable = enabled;

            buttonRandomizeClips.interactable = enabled;
            buttonRandomizeFrames.interactable = enabled;
            buttonResetAnimations.interactable = enabled;
            buttonRandomizeSpeeds.interactable = enabled;
            sliderSpeed.interactable = enabled;
        }

        // Changes the animation speed for the current prototype. Called from GUI.
        public void SetCrowdAnimatorSpeed(float speed)
        {
            // Simply call this API method to change animation speed for all instances of a prototype:
            GPUICrowdAPI.SetAnimationSpeedsForPrototype(gpuiCrowdManager, (GPUICrowdPrototype)gpuiCrowdManager.prototypeList[_selectedPrototypeIndex], speed);
            textSpeedValue.text = speed.ToString("0.00");
        }

        // Ranomizes the animation speeds for the current prototype. Called from GUI.
        public void RandomizeSpeeds()
        {
            List<GPUInstancerPrefab> instanceList = gpuiCrowdManager.GetRegisteredPrefabsRuntimeData()[gpuiCrowdManager.prototypeList[_selectedPrototypeIndex]];
            foreach (GPUICrowdPrefab crowdInstance in instanceList)
            {
                // Simply call this API method to change animation speed per instance:
                GPUICrowdAPI.SetAnimationSpeed(crowdInstance, Random.Range(0.1f, 4.0f));
            }
        }

        // Removes the currently rendered prototype instances and instantiates the next prototype's instances.  Called from GUI.
        public void SwitchProtoype()
        {
            buttonCrowdAnimator.interactable = !enabled;
            buttonMecanimAnimator.interactable = enabled;

            buttonRandomizeClips.interactable = enabled;
            buttonRandomizeFrames.interactable = enabled;
            buttonResetAnimations.interactable = enabled;
            buttonRandomizeSpeeds.interactable = enabled;
            sliderSpeed.interactable = enabled;

            _selectedPrototypeIndex++; // switch to the next prototype index.
            if (_selectedPrototypeIndex >= gpuiCrowdManager.prototypeList.Count)
                _selectedPrototypeIndex = 0;

            // Call this API method to clear the registered instnces from the manager.
            GPUInstancerAPI.ClearRegisteredPrefabInstances(gpuiCrowdManager);
            foreach (var instance in _instanceList)
                Destroy(instance);
            _rowCount = 30;
            _collumnCount = 30;
            Start(); // Just calling the start method again to re-initialize the scene with the next prototype index.
        }

        // Adds more instances of the current prototype to the scene. Called from GUI.
        public void AddInstances()
        {
            if (gpuiCrowdManager == null)
                return;
            if (_rowCount >= 100)
                return;
            GameObject prefabObject = gpuiCrowdManager.prototypeList[_selectedPrototypeIndex].prefabObject;
            Vector3 pos = Vector3.zero;
            GPUICrowdPrefab prefabInstance;
            Quaternion rotation = Quaternion.Euler(0, 180, 0) * prefabObject.transform.rotation;
            for (int r = 0; r < _rowCount + 10; r++)
            {
                for (int c = (r < _rowCount ? _collumnCount : 0); c < _collumnCount + 10; c++)
                {
                    pos.x = _space * r;
                    pos.z = _space * c;
                    GameObject instanceGO = Instantiate(prefabObject, pos, rotation);
                    prefabInstance = instanceGO.GetComponent<GPUICrowdPrefab>(); // We reference the prototype by the GPUICrowdPrefab component that GPUI adds on the prefab...
                    GPUInstancerAPI.AddPrefabInstance(gpuiCrowdManager, prefabInstance); // and add the instance to the manager using that reference using this API method.
                    _instanceList.Add(prefabInstance);
                }
            }
            _rowCount += 10;
            _collumnCount += 10;
            textInstanceCount.text = _prototypeNameText + gpuiCrowdManager.runtimeDataList[_selectedPrototypeIndex].instanceCount;
        }

        public void QuitDemo()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
#endif //GPU_INSTANCER
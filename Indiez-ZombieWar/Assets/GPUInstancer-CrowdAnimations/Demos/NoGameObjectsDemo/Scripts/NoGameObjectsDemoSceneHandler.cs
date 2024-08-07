#if GPU_INSTANCER
using UnityEngine;
using UnityEngine.UI;

namespace GPUInstancer.CrowdAnimations
{
    /// <summary>
    /// This Demo scene controller is similar to the CrowdAnimationsDemoSceneHandler.cs but shows an example usage of handling the Crowd Manager 
    /// with various use cases in a no-GameObject workflow. 
    /// Check the NoGameObjectsDemoScene under [/GPUInstancer-CrowdAnimations/Demos/NoGameObjectsDemo] to see it in action.
    /// </summary>
    public class NoGameObjectsDemoSceneHandler : MonoBehaviour
    {
        public GPUICrowdManager gpuiCrowdManager; // reference to the Crowd Manager. The crowd prototypes are defined and their animations 
                                                  // already baked in the scene before this script runs.
        public GPUInstancerLODColorDebugger gpuiLODColorDebugger; // reference to the The LOD Color Debuger tool.

        // GUI references
        public Slider sliderSpeed;
        public Text textSpeedValue;
        public Text textInstanceCount;

        // demo specific internal fields
        private int _selectedPrototypeIndex = 0;
        private int _rowCount = 30;
        private int _collumnCount = 30;
        private float _space = 1.5f;

        private Matrix4x4[] _instanceDataArray; // the matrix array for the instances. Since we are not using GameObjects, we will store and 
                                                // reference the instances by their transform matrices. 
                                                // (Each matrix hold the position, scale and rotation information for an instance.)

        private GPUICrowdAnimator[] _animators; // reference to the GPUI Animators.
        private int _instanceCount;

        private GPUICrowdPrototype _crowdPrototype; // reference to a prototype that is defined and baked on the gpuiCrowdManager.
        private GPUICrowdRuntimeData _runtimeData; // reference to the runtime data of the _crowdPrototype.

        private string _prototypeNameText;

        public void Start()
        {
            if (gpuiCrowdManager == null)
                return;

            _crowdPrototype = (GPUICrowdPrototype)gpuiCrowdManager.prototypeList[_selectedPrototypeIndex]; // Get the crowd prototype at the current index,
            _crowdPrototype.animationData.useCrowdAnimator = true; // and set its Animator Workflow to be the Crowd Animator Workflow by default.

            _instanceDataArray = new Matrix4x4[10000]; // initialize the matrix arrays for a maximum of 10k instances,
            _animators = new GPUICrowdAnimator[10000]; // and initialize an animator array for the same amount of instances. 
                                                       // The indexes will be the same for the instances in each array.

            // Create the matrices for all the potential 10k instances. 
            // Rest of the matrix array other than the initial 900 instances will be ignored by GPUI 
            // so there will be no performance overhang (See GPUInstancerAPI.SetInstanceCount below).
            // Memory will be reserved for the whole array. However, caching the instances as such will 
            // result in lightning fast add/remove operations on the instances.
            GameObject prefabObject = _crowdPrototype.prefabObject; 
            Vector3 pos = Vector3.zero;
            Quaternion rotation = Quaternion.Euler(0, 180, 0) * prefabObject.transform.rotation; // we refer to the prototype's prefab to account for the rotation in the original prefab.
            int index = 0;
            for (int cycle = 1; cycle <= 10; cycle++)
            {
                int count = cycle * 10;
                for (int r = 0; r < count; r++)
                {
                    for (int c = (r < count - 10 ? count - 10 : 0); c < count; c++)
                    {
                        pos.x = _space * r;
                        pos.z = _space * c;
                        _instanceDataArray[index] = Matrix4x4.TRS(pos, rotation, Vector3.one); // create a transform matrix
                        _animators[index] = new GPUICrowdAnimator(); // and a Crowd Animator for each instance.
                        index++;
                    }
                }
            }
            _instanceCount = _rowCount * _collumnCount;

            GPUInstancerAPI.InitializeWithMatrix4x4Array(gpuiCrowdManager, _crowdPrototype, _instanceDataArray); // Initialize the manager with the created matrix
            GPUInstancerAPI.SetInstanceCount(gpuiCrowdManager, _crowdPrototype, _instanceCount); // and set max instance counts to render (initially to 900, ignoring the rest of the array).

            // get the runtime data for the instances of the current prototype
            _runtimeData = (GPUICrowdRuntimeData)gpuiCrowdManager.GetRuntimeData(_crowdPrototype); 

            // get the default animation clip from the prorotype (set in the Manager)
            GPUIAnimationClipData clipData = _crowdPrototype.animationData.clipDataList[_crowdPrototype.animationData.crowdAnimatorDefaultClip];
            for (int i = 0; i < _instanceCount; i++)
            {
                // and start the animation from the corresponding Crowd Animator
                _animators[i].StartAnimation(_runtimeData, i, clipData, UnityEngine.Random.Range(0.0f, clipData.length));
            }

            SetPrototypeInfoText();
        }

        // Change LOD colors using the GPUI LOD Color Debugger tool.
        public void ChangeLODColors(bool isColored)
        {
            if (gpuiLODColorDebugger == null || gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;
            gpuiLODColorDebugger.enabled = isColored;
        }

        // Randomizes Clips by starting a random animation for each instance.
        public void RandomizeClips()
        {
            if ( gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;

            for (int i = 0; i < _instanceCount; i++)
            {
                // Start a random animation using the Crowd Animator for this instance
                _animators[i].StartAnimation(_runtimeData, i, _crowdPrototype.animationData.clipDataList[UnityEngine.Random.Range(0, _crowdPrototype.animationData.clipDataList.Count)]);
            }
        }

        // Randomizes current animation frames for each instance by starting the animation from a random frame index
        public void RandomizeFrames()
        {
            if (gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;
            for (int i = 0; i < _instanceCount; i++)
            {
                // Starts the current animation from a random frame index using the Crowd Animator for this instance
                GPUIAnimationClipData clip = _animators[i].currentAnimationClipData[0];
                _animators[i].SetClipTime(_runtimeData, i, clip, UnityEngine.Random.Range(0.0f, clip.length));
            }
        }

        // Resets the animations for all instances by staring the default animation clip for the current prototype
        public void ResetAnimations()
        {
            if (gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;

            // Get the default animation clip for this prototype
            GPUIAnimationClipData clipData = _crowdPrototype.animationData.clipDataList[_crowdPrototype.animationData.crowdAnimatorDefaultClip];
            for (int i = 0; i < _instanceCount; i++)
            {
                // Staring the default animation clip for the current prototype using the Crowd Animator for this instance
                _animators[i].StartAnimation(_runtimeData, i, clipData, 0);
            }
            textSpeedValue.text = "1.00";
            sliderSpeed.value = 1;
        }

        // Sets the currently playing animation's speed for all instances
        public void SetCrowdAnimatorSpeed(float speed)
        {
            if (gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;
            for (int i = 0; i < _instanceCount; i++)
            {
                // Set the animation speed for the current animation using the Crowd Animator for this instance
                _animators[i].SetAnimationSpeed(_runtimeData, i, speed);
            }
            textSpeedValue.text = speed.ToString("0.00");
        }

        // Randomizes the currently playing animation's speed for all instances
        public void RandomizeSpeeds()
        {
            if (gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;
            for (int i = 0; i < _instanceCount; i++)
            {
                // Set the animation speed to a randow value for the current animation using the Crowd Animator for this instance
                _animators[i].SetAnimationSpeed(_runtimeData, i, Random.Range(0.1f, 4.0f));
            }
        }

        // Re-initializes the manager with the next prototype. 
        // We use the same transform matrices here since their positions, rotations and scales will be the same in this scene. 
        // This will result in better initialization performance.
        public void SwitchProtoype()
        {
            if (gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;

            // set the rendered instance count to 0 for the current prototype, ignoring all matrices
            GPUInstancerAPI.SetInstanceCount(gpuiCrowdManager, _crowdPrototype, 0);

            // switch prototype index
            _selectedPrototypeIndex++;
            if (_selectedPrototypeIndex >= gpuiCrowdManager.prototypeList.Count)
                _selectedPrototypeIndex = 0;

            // get the next prototype from the manager with the switched index
            _crowdPrototype = (GPUICrowdPrototype)gpuiCrowdManager.prototypeList[_selectedPrototypeIndex];

            // initialize the manager with the new prototype and the same transform matrix array
            GPUInstancerAPI.InitializeWithMatrix4x4Array(gpuiCrowdManager, _crowdPrototype, _instanceDataArray);

            // set the instance count to the currently selected instance count, ignoring the rest of the matrices
            GPUInstancerAPI.SetInstanceCount(gpuiCrowdManager, _crowdPrototype, _instanceCount);
            
            // get the runtime data for the current prototype
            _runtimeData = (GPUICrowdRuntimeData)gpuiCrowdManager.GetRuntimeData(_crowdPrototype);

            // get the default animation clip for this prototype (set from the manager)
            GPUIAnimationClipData clip = _crowdPrototype.animationData.clipDataList[_crowdPrototype.animationData.crowdAnimatorDefaultClip];
            for (int i = 0; i < _instanceCount; i++)
            {
                // start the default animation clip from a random frame using the Crowd Animator for this instance
                _animators[i].StartAnimation(_runtimeData, i, clip, UnityEngine.Random.Range(0.0f, clip.length));
            }

            SetPrototypeInfoText();
        }

        // Adds more instances of the currently rendering crowd prototype.
        public void AddInstances()
        {
            if (gpuiCrowdManager == null)
                return;
            if (_rowCount >= 100)
                return;

            int startInstanceCount = _instanceCount;
            _rowCount += 10;
            _collumnCount += 10;
            _instanceCount = _rowCount * _collumnCount;

            // notify the manager that the instance count is increased so it will not ignore the new ones anymore
            GPUInstancerAPI.SetInstanceCount(gpuiCrowdManager, _crowdPrototype, _instanceCount);

            // get the default animation clip for this prototype (set from the manager)
            GPUIAnimationClipData clip = _crowdPrototype.animationData.clipDataList[_crowdPrototype.animationData.crowdAnimatorDefaultClip];

            // for the newly added instances
            for (int i = startInstanceCount; i < _instanceCount; i++)
            {
                // start the default animation clip from a random frame using the Crowd Animator for this instance
                _animators[i].StartAnimation(_runtimeData, i, clip, UnityEngine.Random.Range(0.0f, clip.length));
            }

            textInstanceCount.text = _prototypeNameText + _instanceCount;
        }

        // Removes instances of the currently rendering crowd prototype.
        public void RemoveInstances()
        {
            if (gpuiCrowdManager == null)
                return;
            if (_rowCount <= 10)
                return;

            _rowCount -= 10;
            _collumnCount -= 10;
            _instanceCount = _rowCount * _collumnCount;
            
            // Notift the manager to ignore the instances after the new instance count
            GPUInstancerAPI.SetInstanceCount(gpuiCrowdManager, _crowdPrototype, _instanceCount);
            textInstanceCount.text = _prototypeNameText + _instanceCount;
        }

        // Displays information on the currently rendering crwod prototype instances
        private void SetPrototypeInfoText()
        {
            _prototypeNameText = _crowdPrototype.prefabObject.name;
            if (_runtimeData != null && _runtimeData.instanceLODs.Count > 0)
            {
                _prototypeNameText += "\nVertex Counts: ";
                for (int i = 0; i < _runtimeData.instanceLODs.Count; i++)
                {
                    _prototypeNameText += " LOD" + i + ": " + _runtimeData.instanceLODs[i].renderers[0].mesh.vertexCount; // Rendered vertex count
                }
            }
            _prototypeNameText += "\nInstance Count: ";
            textInstanceCount.text = _prototypeNameText + _instanceCount;
        }
    }
}
#endif //GPU_INSTANCER
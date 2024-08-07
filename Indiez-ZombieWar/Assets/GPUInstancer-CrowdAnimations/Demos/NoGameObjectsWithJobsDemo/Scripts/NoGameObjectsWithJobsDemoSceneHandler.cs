#if GPU_INSTANCER
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.UI;

namespace GPUInstancer.CrowdAnimations
{
    /// <summary>
    /// This Demo scene controller is similar to the CrowdAnimationsDemoSceneHandler.cs but shows an example usage of handling the Crowd Manager 
    /// with Jobs in a no-GameObject workflow. 
    /// Check the NoGameObjectsWithJobsDemoScene under [/GPUInstancer-CrowdAnimations/Demos/NoGameObjectsWithJobsDemo] to see it in action.
    /// </summary>
    [DefaultExecutionOrder(1000)] // we want the Mono Behaviour methods inside this class to run after the Crowd Manager
    public class NoGameObjectsWithJobsDemoSceneHandler : MonoBehaviour
    {
        public GPUICrowdManager gpuiCrowdManager; // reference to the Crowd Manager. The crowd prototypes are defined and their animations 
                                                  // already baked in the scene before this script runs.
        public GPUInstancerLODColorDebugger gpuiLODColorDebugger; // reference to the LOD Color Debugger tool.

        // GUI references
        public Slider sliderSpeed;
        public Text textSpeedValue;
        public Text textInstanceCount;

        // demo specific internal fields
        private int _selectedPrototypeIndex = 0;
        private int _rowCount = 30;
        private int _collumnCount = 30;
        private float _space = 1.5f;

        private NativeArray<Matrix4x4> _instanceMatrixArray; // the matrix array for the instances. Since we are not using GameObjects, we will store and 
                                                             // reference the instances by their transform matrices. 
                                                             // (Each matrix hold the position, scale and rotation information for an instance.)
        private NativeArray<CrowdInstanceState> _instanceStateArray; // CrowdInstanceState array to determine next state of the animator

        private int _instanceCount;

        private GPUICrowdPrototype _crowdPrototype; // reference to a prototype that is defined and baked on the gpuiCrowdManager.
        private GPUICrowdRuntimeData _runtimeData; // reference to the runtime data of the _crowdPrototype.

        private string _prototypeNameText;
        private readonly int _bufferSize = 10000;
        private bool _isStateModified; // set to true when the state array is modified

        private struct CrowdInstanceState  // example struct that determines the state for crowd animator
        {
            public int animationIndex;
            public float animationSpeed;
            public float animationStartTimeMultiplier;
            public StateModificationType modificationType;
        }

        private enum StateModificationType // example enum to determine what to update inside the Job
        {
            None = 0,
            All = 1,
            Clip = 2,
            Speed = 3,
            StartTime = 4
        }

#if GPUI_BURST
        [Unity.Burst.BurstCompile]
#endif
        private struct ApplyAnimationStateJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<GPUIAnimationClipData> clipDatas;
            [ReadOnly] public NativeArray<CrowdInstanceState> instanceStateArray;

            /// <summary>
            /// <para>index: 0 x -> frameNo1, y -> frameNo2, z -> frameNo3, w -> frameNo4</para> 
            /// <para>index: 1 x -> weight1, y -> weight2, z -> weight3, w -> weight4</para> 
            /// </summary>
            [NativeDisableParallelForRestriction] public NativeArray<Vector4> animationData;
            /// <summary>
            /// 0 to 4: x ->  minFrame, y -> maxFrame (negative if not looping), z -> speed, w -> startTime
            /// </summary>
            [NativeDisableParallelForRestriction] public NativeArray<Vector4> crowdAnimatorControllerData;

            public void Execute(int index)
            {
                CrowdInstanceState state = instanceStateArray[index];
                if (state.modificationType == StateModificationType.None)
                    return;

                Vector4 activeClip0 = crowdAnimatorControllerData[index * 4];
                Vector4 clipFrames = animationData[index * 2];
                Vector4 clipWeights = animationData[index * 2 + 1];
                GPUIAnimationClipData clipData = clipDatas[state.animationIndex];

                switch (state.modificationType)
                {
                    case StateModificationType.All:
                        clipFrames.x = clipData.clipStartFrame;
                        activeClip0.x = clipData.clipStartFrame;
                        activeClip0.y = clipData.clipStartFrame + clipData.clipFrameCount - 1;
                        activeClip0.z = state.animationSpeed;
                        activeClip0.w = clipDatas[state.animationIndex].length * state.animationStartTimeMultiplier;
                        clipWeights = new Vector4(1f, 0f, 0f, 0f);
                        break;
                    case StateModificationType.Clip:
                        clipFrames.x = clipData.clipStartFrame;
                        activeClip0.x = clipData.clipStartFrame;
                        activeClip0.y = clipData.clipStartFrame + clipData.clipFrameCount - 1;
                        activeClip0.w = 0f;
                        clipWeights = new Vector4(1f, 0f, 0f, 0f);
                        break;
                    case StateModificationType.Speed:
                        activeClip0.z = state.animationSpeed;
                        break;
                    case StateModificationType.StartTime:
                        activeClip0.w = clipData.length * state.animationStartTimeMultiplier;
                        break;
                }

                animationData[index * 2] = clipFrames;
                animationData[index * 2 + 1] = clipWeights;
                crowdAnimatorControllerData[index * 4] = activeClip0;
            }
        }

        public void OnEnable()
        {
            if (gpuiCrowdManager == null)
                return;

            _crowdPrototype = (GPUICrowdPrototype)gpuiCrowdManager.prototypeList[_selectedPrototypeIndex]; // Get the crowd prototype at the current index,
            _crowdPrototype.animationData.useCrowdAnimator = true; // and set its Animator Workflow to be the Crowd Animator Workflow by default.

            _instanceCount = _rowCount * _collumnCount; 
            GPUInstancerAPI.InitializePrototype(gpuiCrowdManager, _crowdPrototype, _bufferSize, _instanceCount); // Initialize buffers with the buffer size
            _runtimeData = (GPUICrowdRuntimeData)gpuiCrowdManager.GetRuntimeData(_crowdPrototype); // get the runtime data for the instances of the current prototype

            // Make sure the GPUIAnimationClipData array is created to be able to access clip data inside Jobs
            if (!_runtimeData.clipDatas.IsCreated)
                _runtimeData.clipDatas = new NativeArray<GPUIAnimationClipData>(_crowdPrototype.animationData.clipDataList.ToArray(), Allocator.Persistent);

            _instanceMatrixArray = new NativeArray<Matrix4x4>(_bufferSize, Allocator.Persistent); // initialize the matrix arrays for a maximum of 10k instances,
            _instanceStateArray = new NativeArray<CrowdInstanceState>(_bufferSize, Allocator.Persistent); // and initialize the state array for the same amount of instances. 
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
                        _instanceMatrixArray[index] = Matrix4x4.TRS(pos, rotation, Vector3.one); // create a transform matrix
                        _instanceStateArray[index] = new CrowdInstanceState()
                        {
                            animationIndex = _crowdPrototype.animationData.crowdAnimatorDefaultClip,
                            animationSpeed = 1,
                            animationStartTimeMultiplier = UnityEngine.Random.Range(0.0f, 0.99f),
                            modificationType = StateModificationType.All
                        }; // and a state for each instance.
                        index++;
                    }
                }
            }

            GPUInstancerAPI.UpdateVisibilityBufferWithNativeArray(gpuiCrowdManager, _crowdPrototype, _instanceMatrixArray); // Set Matrix array to the visibility buffer

            SetPrototypeInfoText();
            _isStateModified = true;
        }

        private void OnDisable()
        {
            // Dispose NativeArrays
            if (_instanceMatrixArray.IsCreated)
                _instanceMatrixArray.Dispose();
            if (_instanceStateArray.IsCreated)
                _instanceStateArray.Dispose();
        }

        private void LateUpdate()
        {
            if (_isStateModified)
            {
                // Schedule the ApplyAnimationStateJob
                _runtimeData.dependentJob = new ApplyAnimationStateJob()
                {
                    clipDatas = _runtimeData.clipDatas,
                    animationData = _runtimeData.animationData,
                    crowdAnimatorControllerData = _runtimeData.crowdAnimatorControllerData,
                    instanceStateArray = _instanceStateArray
                }.Schedule(_instanceCount, 64, _runtimeData.dependentJob);
                _isStateModified = false;

                // Notify Crowd Manager that the NativeArrays are modified
                _runtimeData.animationDataModified = true;
                _runtimeData.crowdAnimatorDataModified = true;
            }
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
            _runtimeData.dependentJob.Complete();

            int clipCount = _crowdPrototype.animationData.clipDataList.Count;
            for (int i = 0; i < _instanceCount; i++)
            {
                // Start a random animation for this instance
                CrowdInstanceState previousState = _instanceStateArray[i];
                previousState.animationIndex = UnityEngine.Random.Range(0, clipCount);
                previousState.animationStartTimeMultiplier = 0;
                previousState.modificationType = StateModificationType.Clip;
                _instanceStateArray[i] = previousState;
            }
            _isStateModified = true;
        }

        // Randomizes current animation frames for each instance by starting the animation from a random frame index
        public void RandomizeFrames()
        {
            if (gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;
            _runtimeData.dependentJob.Complete();

            for (int i = 0; i < _instanceCount; i++)
            {
                // Starts the current animation from a random frame index for this instance
                CrowdInstanceState previousState = _instanceStateArray[i];
                previousState.animationStartTimeMultiplier = UnityEngine.Random.Range(0.0f, 0.99f);
                previousState.modificationType = StateModificationType.StartTime;
                _instanceStateArray[i] = previousState;
            }
            _isStateModified = true;
        }

        // Resets the animations for all instances by staring the default animation clip for the current prototype
        public void ResetAnimations()
        {
            if (gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;
            _runtimeData.dependentJob.Complete();

            for (int i = 0; i < _instanceCount; i++)
            {
                // Staring the default animation clip for the current prototype for this instance
                CrowdInstanceState previousState = _instanceStateArray[i];
                previousState.animationIndex = _crowdPrototype.animationData.crowdAnimatorDefaultClip;
                previousState.animationSpeed = 1f;
                previousState.animationStartTimeMultiplier = 0;
                previousState.modificationType = StateModificationType.All;
                _instanceStateArray[i] = previousState;
            }
            textSpeedValue.text = "1.00";
            sliderSpeed.value = 1;
            _isStateModified = true;
        }

        // Sets the currently playing animation's speed for all instances
        public void SetCrowdAnimatorSpeed(float speed)
        {
            if (gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;
            _runtimeData.dependentJob.Complete();

            for (int i = 0; i < _instanceCount; i++)
            {
                // Set the animation speed for the current animation for this instance
                CrowdInstanceState previousState = _instanceStateArray[i];
                previousState.animationSpeed = speed;
                previousState.modificationType = StateModificationType.Speed;
                _instanceStateArray[i] = previousState;
            }
            textSpeedValue.text = speed.ToString("0.00");
            _isStateModified = true;
        }

        // Randomizes the currently playing animation's speed for all instances
        public void RandomizeSpeeds()
        {
            if (gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;
            _runtimeData.dependentJob.Complete();

            for (int i = 0; i < _instanceCount; i++)
            {
                // Set the animation speed to a random value for the current animation for this instance
                CrowdInstanceState previousState = _instanceStateArray[i];
                previousState.animationSpeed = UnityEngine.Random.Range(0.1f, 4.0f);
                previousState.modificationType = StateModificationType.Speed;
                _instanceStateArray[i] = previousState;
            }
            _isStateModified = true;
        }

        // Re-initializes the manager with the next prototype. 
        // We use the same transform matrices here since their positions, rotations and scales will be the same in this scene. 
        // This will result in better initialization performance.
        public void SwitchProtoype()
        {
            if (gpuiCrowdManager == null || !gpuiCrowdManager.enabled)
                return;
            _runtimeData.dependentJob.Complete();

            // set the rendered instance count to 0 for the current prototype, ignoring all matrices
            GPUInstancerAPI.SetInstanceCount(gpuiCrowdManager, _crowdPrototype, 0);

            // switch prototype index
            _selectedPrototypeIndex++;
            if (_selectedPrototypeIndex >= gpuiCrowdManager.prototypeList.Count)
                _selectedPrototypeIndex = 0;

            // get the next prototype from the manager with the switched index
            _crowdPrototype = (GPUICrowdPrototype)gpuiCrowdManager.prototypeList[_selectedPrototypeIndex];

            // initialize the manager with the new prototype and the same transform matrix array
            GPUInstancerAPI.InitializePrototype(gpuiCrowdManager, _crowdPrototype, _bufferSize, _instanceCount);
            GPUInstancerAPI.UpdateVisibilityBufferWithNativeArray(gpuiCrowdManager, _crowdPrototype, _instanceMatrixArray);

            // get the runtime data for the current prototype
            _runtimeData = (GPUICrowdRuntimeData)gpuiCrowdManager.GetRuntimeData(_crowdPrototype);
            // Set GPUIAnimationClipData array to be able to access clip data inside Jobs
            if (!_runtimeData.clipDatas.IsCreated)
                _runtimeData.clipDatas = new NativeArray<GPUIAnimationClipData>(_crowdPrototype.animationData.clipDataList.ToArray(), Allocator.Persistent);

            for (int i = 0; i < _instanceCount; i++)
            {
                // start the default animation clip from a random frame for this instance
                _instanceStateArray[i] = new CrowdInstanceState()
                {
                    animationIndex = _crowdPrototype.animationData.crowdAnimatorDefaultClip,
                    animationSpeed = 1,
                    animationStartTimeMultiplier = UnityEngine.Random.Range(0.0f, 0.99f),
                    modificationType = StateModificationType.All
                };
            }

            SetPrototypeInfoText();
            _isStateModified = true;
        }

        // Adds more instances of the currently rendering crowd prototype.
        public void AddInstances()
        {
            if (gpuiCrowdManager == null)
                return;
            if (_rowCount >= 100)
                return;
            _runtimeData.dependentJob.Complete();

            int startInstanceCount = _instanceCount;
            _rowCount += 10;
            _collumnCount += 10;
            _instanceCount = _rowCount * _collumnCount;

            // notify the manager that the instance count is increased so it will not ignore the new ones anymore
            GPUInstancerAPI.SetInstanceCount(gpuiCrowdManager, _crowdPrototype, _instanceCount);

            // for the newly added instances
            for (int i = startInstanceCount; i < _instanceCount; i++)
            {
                // start the default animation clip from a random frame using the Crowd Animator for this instance
                _instanceStateArray[i] = new CrowdInstanceState()
                {
                    animationIndex = _crowdPrototype.animationData.crowdAnimatorDefaultClip,
                    animationSpeed = 1,
                    animationStartTimeMultiplier = UnityEngine.Random.Range(0.0f, 0.99f),
                    modificationType = StateModificationType.All
                };
            }

            textInstanceCount.text = _prototypeNameText + _instanceCount;
            _isStateModified = true;
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
            
            // Notify the manager to ignore the instances after the new instance count
            GPUInstancerAPI.SetInstanceCount(gpuiCrowdManager, _crowdPrototype, _instanceCount);
            textInstanceCount.text = _prototypeNameText + _instanceCount;
        }

        // Displays information on the currently rendering crowd prototype instances
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
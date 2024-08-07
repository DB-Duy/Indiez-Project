#if GPU_INSTANCER
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer.CrowdAnimations
{
    /// <summary>
    /// This Demo scene controller shows an example usage of animation events with the Crowd Animator workflow. 
    /// Check the AnimationEvents under [/GPUInstancer-CrowdAnimations/Demos/AnimationEvents] to see it in action.
    /// </summary>
    public class AnimationEventsDemoSceneHandler : MonoBehaviour
    {
        public GPUICrowdManager gpuiCrowdManager; // reference to the Crowd Manager. The crowd prototypes are defined and their animations 
                                                  // already baked in the scene before this script runs.

        public Transform spitPrefab;
        
        private readonly Vector3 spitVector = new Vector3(0, 8f,8f);
        private readonly Vector3 spitPosAdd = new Vector3(0, 0.8f, 0.25f);
        private Transform[] _spits;
        private Transform _spitsParent;

        // This method is given as a parameter to the AddAnimationEvent API.
        // It requires to have one parameter (GPUICrowdPrefab) which gives a reference to the instance that the event is running for.
        public void Spit(GPUICrowdPrefab crowdInstance, float floatParam, int intParam, string stringParam)
        {
            if (_spitsParent == null)
            {
                _spitsParent = (new GameObject("Spits")).transform;
                _spits = new Transform[gpuiCrowdManager.runtimeDataList[0].instanceCount];
            }
            else if (_spits.Length < gpuiCrowdManager.runtimeDataList[0].instanceCount)
                Array.Resize(ref _spits, gpuiCrowdManager.runtimeDataList[0].instanceCount);

            Vector3 spitPos = crowdInstance.GetInstanceTransform().position + spitPosAdd;

            Transform spitTransform = _spits[crowdInstance.gpuInstancerID - 1];
            if (spitTransform == null)
            {
                spitTransform = Instantiate(spitPrefab, spitPos, Quaternion.identity, _spitsParent);
                _spits[crowdInstance.gpuInstancerID - 1] = spitTransform;
            }

            spitTransform.position = spitPos;

            Rigidbody spitRb = spitTransform.GetComponent<Rigidbody>();

            spitRb.isKinematic = false;
            spitRb.velocity = crowdInstance.GetInstanceTransform().rotation * spitVector;
            spitRb.detectCollisions = false;
        }
    }
}
#endif //GPU_INSTANCER
#if GPU_INSTANCER
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GPUInstancer.CrowdAnimations
{
    /// <summary>
    /// This Demo scene controller shows an example usage of AI agents with the Crowd Animator Workflow. The Agent animations are handled using the GPUICrowdAPI.
    /// Check the NavMeshDemoScene under [/GPUInstancer-CrowdAnimations/Demos/NavMeshDemo] to see it in action.
    /// </summary>
    public class NavMeshDemoSceneController : MonoBehaviour
    {
        [Header("References")]
        public GameObject indicator;

        [Header("Settings")]
        public GPUICrowdPrefab prefab; // reference 
        public Transform SpawnLoacation;
        public int instanceCount = 500;
        public int avoidanceRange = 25;
        public bool isGPUIManagerActive = true;

        [Header("Animations")]
        public AnimationClip idle;
        public AnimationClip walk;
        public AnimationClip run;

        // Scene Logic
        private Ray _ray;
        private RaycastHit _hit;
        private NavMeshHit _navMeshHit;
        private Vector3 _currentTarget;
        private Vector3 _previousTarget;
        private bool _isTargetChanged;
        private GameObject _indicatorSphere;
        private GameObject _instancesParent;
        private bool _resetAnimations;

        // Crowd Animations
        private List<DemoAgent> _demoAgents;
        private enum AnimationState { Idle, Locomotion }
        private AnimationState _currentState;
        private Vector4 _blendWeights; //This will cache animation blend weights.

        // This class will be used for easy access to the navmesh agents and their corresponding Crowd Manager prototype.
        private class DemoAgent
        {
            public GPUICrowdPrefab gpuiCrowdInstance;
            public AnimationState currentState;
            public NavMeshAgent ai;
        }

        #region MonoBehaviour Methods

        private void Start()
        {
            if (prefab == null)
                return;

            // a simple indicator to show where the agents' destination.
            _indicatorSphere = Instantiate(indicator, Vector3.zero, Quaternion.identity);

            _demoAgents = new List<DemoAgent>();

            AddAgents(instanceCount); // instantiate NavMesh Agents.
            _currentTarget = GetRandomNavMeshPositionNearLocation(SpawnLoacation.position, avoidanceRange); // get a random initial destination around the spawn location.
            _indicatorSphere.transform.position = _currentTarget;
            _blendWeights = Vector4.zero;
        }


        private void Update()
        {
            // Handle mouse clicks and sample a destination point on the NavMesh on left mouse click:
            if (Input.GetMouseButtonUp(0))
            {
                _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(_ray, out _hit, Mathf.Infinity))
                {
                    _indicatorSphere.transform.position = _hit.point;
                    NavMesh.SamplePosition(_hit.point, out _navMeshHit, avoidanceRange, NavMesh.AllAreas);
                    _currentTarget = _navMeshHit.position;
                }
            }

            if (_demoAgents != null)
            {
                // Check if the target destination has changed
                _isTargetChanged = _currentTarget != _previousTarget;

                // loop all the AI agents in the scene
                for (int i = 0; i <_demoAgents.Count; i++)
                {
                    DemoAgent agent = _demoAgents[i];

                    // ans set their new destination if the target destination has changed.
                    if (_isTargetChanged)
                        agent.ai.SetDestination(GetRandomNavMeshPositionNearLocation(_currentTarget, avoidanceRange));

                    // get this agent's speed to use for its animation
                    float agentSpeed = Mathf.Clamp01(agent.ai.velocity.magnitude / agent.ai.speed);

                    // and handle its animation. This is where animations are handled in this Crowd Animator Workflow
                    HandleAnimationState(agent, agentSpeed);
                }

                if (_isTargetChanged)
                    _previousTarget = _currentTarget;

                if (_resetAnimations)
                    _resetAnimations = false;
            }
        }

        #endregion MonoBehaviour Methods

        #region AI Agent Methods

        // Adds Navmesh agents. The Crowd Manager in the demo scene uses the "Auto. Add Remove Instances" runtime setting, 
        // so not extra initialization or registration is necessary for the manager. This method simply adds Agents as game objects to the scene.
        public void AddAgents(int count)
        {
            if (_instancesParent == null)
            {
                _instancesParent = new GameObject("Instances");
            }
            for (int i = 0; i < count; i++)
            {
                DemoAgent agent = new DemoAgent();

                // instantiate the agent GameObject
                GameObject instance = Instantiate(prefab, GetRandomNavMeshPositionNearLocation(SpawnLoacation.position, 10), Quaternion.identity, _instancesParent.transform).gameObject;

                agent.gpuiCrowdInstance = instance.GetComponent<GPUICrowdPrefab>(); // Store the reference to the Crowd prototype instance for later use. 
                                                                                    // The GPUICrowdPrefab component is added to the prefabs when they are defined to a Crowd Manager.

                agent.ai = instance.GetComponent<NavMeshAgent>(); // Store a reference to the NavMesh agent for later use.

                if (agent.ai != null)
                {
                    agent.ai.updateRotation = true;
                    agent.ai.updatePosition = true;
                }

                _demoAgents.Add(agent);
            }
        }

        private Vector3 GetRandomNavMeshPositionNearLocation(Vector3 origin, float range)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector3 randomPoint = origin + Random.insideUnitSphere * range;
                if (NavMesh.SamplePosition(randomPoint, out _navMeshHit, range, NavMesh.AllAreas))
                    return _navMeshHit.position;
            }
            return origin;
        }

        #endregion AI Agent Methods

        #region Crowd Animator Methods

        public void ResetAnimations()
        {
            _resetAnimations = true;
        }

        // Agents will be considered moving or idle with respect to agent speed.
        private AnimationState DetermineAgentState(float agentSpeed)
        {
            if (agentSpeed < 0.1f)
                return AnimationState.Idle;
            else
                return AnimationState.Locomotion;
        }

        // This is the relevant part for using the Crowd Animator Workflow. 
        // All animation handling is done per instance in this method.
        private void HandleAnimationState(DemoAgent agent, float agentSpeed)
        {
            if (!isGPUIManagerActive) // set from the inspector (or an outside script).
            {
                // Set speed to Unity (Mecanim) Animator if Crowd Manager is inactive
                agent.gpuiCrowdInstance.animatorRef.SetFloat("Speed", agentSpeed);
                return;
            }

            AnimationState state = DetermineAgentState(agentSpeed);

            // Agent current state is cached. We do not want to start the animation each frame, but rather only when the agent state changes.
            if (agent.currentState != state || _resetAnimations)
            {
                agent.currentState = state; // Cache the current state.

                switch (state)
                {
                    case AnimationState.Idle:
                        // This is how you can easily start an animation using the GPUICrowdAPI when using the Crowd Manager Workflow.
                        // The prototype must already be defined in a Crowd Manager, and its animations already baked (from an animator component containing the animations)
                        // The "idle" animation is referenced from the inspector in this case, and the baked prototype animations also include this animation.
                        // We also add a transition value of 0.5 for a smooth transition between idle and locomotion states.
                        GPUICrowdAPI.StartAnimation(agent.gpuiCrowdInstance, idle, -1, 1, 0.5f); 
                        break;

                    case AnimationState.Locomotion:
                        // This is how you can easily start a blend of multiple animations using the GPUICrowdAPI when using the Crowd Manager Workflow.
                        // similar to GPUICrowdAPI.StartAnimation the prototype must already be defined in a Crowd Manager, and its animations already baked 
                        // (from an animator component containing the animations). The _blendWeight parameter is a Vector4 where x, y, z and w are the blend weights for the
                        // animations in order that follow this parameter. In this example, we only use the x and y for blend weights and two animations. 
                        // We set the weights here from agent speed.
                        // Please note that the total sum of blend weights should amount to 1.
                        // We also add a transition value of 0.5 for a smooth transition between idle and locomotion states.
                        _blendWeights.x = 1 - agentSpeed;
                        _blendWeights.y = agentSpeed;
                        GPUICrowdAPI.StartBlend(agent.gpuiCrowdInstance, _blendWeights, walk, run, null, null, null, null, 0.5f);
                        break;
                }
            }
            else
            {
                // Agent state has not changed, but we still want to update blend weights according to the agent speed if this is a locomotion state.
                if (agent.currentState == AnimationState.Locomotion && !agent.gpuiCrowdInstance.crowdAnimator.isInTransition)
                {
                    _blendWeights.x = 1 - agentSpeed;
                    _blendWeights.y = agentSpeed;

                    // You can simply use the SetAnimationWeights API method to update the _blendweights for a given instance.
                    GPUICrowdAPI.SetAnimationWeights(agent.gpuiCrowdInstance, _blendWeights);
                }
            }
        }

        #endregion Crowd Animator Methods
    }
}
#endif //GPU_INSTANCER
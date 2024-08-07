// Inspired by https://github.com/keijiro/Boids
#if GPU_INSTANCER
using UnityEngine;
using GPUInstancer;
using GPUInstancer.CrowdAnimations;

public class FishSwarmBoid : MonoBehaviour
{
    public int spawnCount = 10;

    public float spawnRadius = 4.0f;

    [Range(0.1f, 20.0f)]
    public float velocity = 6.0f;

    [Range(0.0f, 0.9f)]
    public float velocityVariation = 0.5f;

    [Range(0.1f, 20.0f)]
    public float rotationCoeff = 4.0f;

    [Range(0.1f, 10.0f)]
    public float neighborDist = 2.0f;

    private Matrix4x4[] _spawnArray;
    private Vector4[] _variationArray;

    public Transform centerTransform;
    public Texture2D noiseTexture;

    private GPUICrowdManager _crowdManager;
    private GPUICrowdPrototype _prototype;
    private GPUICrowdRuntimeData _runtimeData;
    private GPUICrowdAnimator[] _animators;

    private ComputeShader _gpuiBoidsCS;
    private string _variationBufferName = "textureUVBuffer";
    private float[] _centerTransformArray = new float[16];
    private ComputeBuffer _transformDataBuffer;

    private float _nextTargetMoveTime;
    public float targetMovePeriod;

    // CS params
    public static class BoidProperties
    {
        public static readonly int BP_boidsData = Shader.PropertyToID("boidsData");
        public static readonly int BP_bufferSize = Shader.PropertyToID("bufferSize");
        public static readonly int BP_controllerTransform = Shader.PropertyToID("controllerTransform");
        public static readonly int BP_controllerVelocity = Shader.PropertyToID("controllerVelocity");
        public static readonly int BP_controllerVelocityVariation = Shader.PropertyToID("controllerVelocityVariation");
        public static readonly int BP_controllerRotationCoeff = Shader.PropertyToID("controllerRotationCoeff");
        public static readonly int BP_controllerNeighborDist = Shader.PropertyToID("controllerNeighborDist");
        public static readonly int BP_time = Shader.PropertyToID("time");
        public static readonly int BP_deltaTime = Shader.PropertyToID("deltaTime");
        public static readonly int BP_noiseTexture = Shader.PropertyToID("noiseTexture");
    }

    void Start()
    {
        // these 3 arrays hold the matrix, variation and animator data in shared indexes.
        _spawnArray = new Matrix4x4[spawnCount];
        _variationArray = new Vector4[spawnCount];
        _animators = new GPUICrowdAnimator[spawnCount];

        for (var i = 0; i < spawnCount; i++)
            Spawn(i);

        _crowdManager = FindObjectOfType<GPUICrowdManager>();
        
        if (_crowdManager != null && _crowdManager.prototypeList.Count > 0)
        {
            _prototype = (GPUICrowdPrototype)_crowdManager.prototypeList[0];
            _runtimeData = (GPUICrowdRuntimeData)_crowdManager.GetRuntimeData(_prototype);

            // Initialize the crowd (no game object workflow)
            GPUInstancerAPI.InitializeWithMatrix4x4Array(_crowdManager, _prototype, _spawnArray);

            // Define variations
            GPUInstancerAPI.DefineAndAddVariationFromArray(_crowdManager, _prototype, _variationBufferName, _variationArray);

            // Get the default animation clip from the prorotype (set in the Manager)
            GPUIAnimationClipData clipData = _prototype.animationData.clipDataList[_prototype.animationData.crowdAnimatorDefaultClip];
            for (int i = 0; i < spawnCount; i++)
            {
                // and start the animation from the corresponding Crowd Animator
                _animators[i].StartAnimation(_runtimeData, i, clipData, UnityEngine.Random.Range(0.0f, clipData.length));
            }

            // Get a reference to the data buffer to manupulate it with the boids compute during update
            _transformDataBuffer = GPUInstancerAPI.GetTransformDataBuffer(_crowdManager, _prototype); 
        }

    }

    public void Spawn(int index)
    {
        _spawnArray[index] = Matrix4x4.TRS(transform.position + Random.insideUnitSphere * spawnRadius, Random.rotation, Vector3.one);
        _variationArray[index] = new Vector4(Random.Range(0, 2) * 0.5f, Random.Range(0, 2) * 0.5f, 0.5f, 0.5f);
        _animators[index] = new GPUICrowdAnimator();
    }

    void Update()
    {
        // Move the boid target in intervals for an organic flock behavior
        if (Time.time > _nextTargetMoveTime)
        {
            _nextTargetMoveTime += targetMovePeriod;
            centerTransform.SetPositionAndRotation(new Vector3(-centerTransform.position.x, centerTransform.position.y, centerTransform.position.z), Quaternion.identity);
        }
            
        if (_gpuiBoidsCS == null)
            _gpuiBoidsCS = Resources.Load<ComputeShader>("GPUIFishBoids");

        if (_gpuiBoidsCS == null || _transformDataBuffer == null)
            return;

        // Setup and dispatch the boids compute shader to modify the crowd buffer
        centerTransform.localToWorldMatrix.Matrix4x4ToFloatArray(_centerTransformArray);

        _gpuiBoidsCS.SetBuffer(0, BoidProperties.BP_boidsData, _transformDataBuffer);
        _gpuiBoidsCS.SetTexture(0, BoidProperties.BP_noiseTexture, noiseTexture);
        _gpuiBoidsCS.SetInt(BoidProperties.BP_bufferSize, spawnCount);
        _gpuiBoidsCS.SetFloats(BoidProperties.BP_controllerTransform, _centerTransformArray);
        _gpuiBoidsCS.SetFloat(BoidProperties.BP_controllerVelocity, velocity);
        _gpuiBoidsCS.SetFloat(BoidProperties.BP_controllerVelocityVariation, velocityVariation);
        _gpuiBoidsCS.SetFloat(BoidProperties.BP_controllerRotationCoeff, rotationCoeff);
        _gpuiBoidsCS.SetFloat(BoidProperties.BP_controllerNeighborDist, neighborDist);
        _gpuiBoidsCS.SetFloat(BoidProperties.BP_time, Time.time);
        _gpuiBoidsCS.SetFloat(BoidProperties.BP_deltaTime, Time.deltaTime);

        _gpuiBoidsCS.Dispatch(0, Mathf.CeilToInt(spawnCount / GPUInstancerConstants.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
    }
}
#endif //GPU_INSTANCER
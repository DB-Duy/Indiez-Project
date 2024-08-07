using UnityEngine;

public class AnimationSampler : MonoBehaviour
{
    public AnimationClip clip; // Assign your AnimationClip in the Inspector
    public GameObject skinnedMesh; // Assign your skinned mesh GameObject in the Inspector

    void Start()
    {
        // Sample the animation at a specific time (e.g., 1 second)
        float sampleTime = 1.0f; // Change this to your desired time
        SampleAnimationAtTime(sampleTime);
    }

    void SampleAnimationAtTime(float time)
    {
        // Ensure the time is within the clip length
        if (clip != null && time >= 0 && time <= clip.length)
        {
            // Sample the animation and apply the pose to the skinned mesh
            clip.SampleAnimation(skinnedMesh, time);
        }
        else
        {
            Debug.LogWarning("Sample time is out of bounds.");
        }
    }
}